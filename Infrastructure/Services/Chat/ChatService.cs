using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs.Chat;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Chat;
using Application.Interfaces.Services.Media;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Chat
{
    public class ChatService : IChatService
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IWorkerProfileRepository _workerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBlobService _blobService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IChatMessageRepository chatMessageRepository,
            IBookingRepository bookingRepository,
            ICustomerProfileRepository customerProfileRepository,
            IWorkerProfileRepository workerProfileRepository,
            IUnitOfWork unitOfWork,
            IHubContext<ChatHub> hubContext,
            ICurrentUserService currentUserService,
            IBlobService blobService,
            ILogger<ChatService> logger
        )
        {
            _chatMessageRepository = chatMessageRepository;
            _bookingRepository = bookingRepository;
            _customerProfileRepository = customerProfileRepository;
            _workerProfileRepository = workerProfileRepository;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _currentUserService = currentUserService;
            _blobService = blobService;
            _logger = logger;
        }

        public async Task<OperationResult<PagedResponse<ChatMessageDto>>> GetChatHistoryAsync(
            Guid bookingId,
            PagedQuery query,
            CancellationToken cancellationToken = default
        )
        {
            if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId))
            {
                return OperationResult<PagedResponse<ChatMessageDto>>.Failure(
                    "User is not authenticated."
                );
            }

            var booking = await _bookingRepository.GetBookingWithWorkerAsync(
                bookingId,
                cancellationToken
            );

            if (booking == null)
            {
                return OperationResult<PagedResponse<ChatMessageDto>>.Failure("Booking not found.");
            }

            var customerProfile = await _customerProfileRepository.FirstOrDefaultAsync(
                x => x.UserId == currentUserId,
                cancellationToken
            );

            var workerProfile = await _workerProfileRepository.FirstOrDefaultAsync(
                x => x.UserId == currentUserId,
                cancellationToken
            );

            var isCustomer =
                customerProfile != null && booking.CustomerProfileId == customerProfile.Id;

            var isWorker = workerProfile != null && booking.WorkerProfileId == workerProfile.Id;

            if (!isCustomer && !isWorker)
            {
                return OperationResult<PagedResponse<ChatMessageDto>>.Failure("Forbidden");
            }

            var (messages, totalCount) = await _chatMessageRepository.GetChatHistoryAsync(
                bookingId,
                query,
                cancellationToken
            );

            var dtos = messages
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    BookingId = m.BookingId,
                    SenderId = m.SenderId,

                    SenderName = m.Sender?.FullName ?? "Unknown",

                    SenderRole =
                        customerProfile != null && m.SenderId == customerProfile.UserId ? "CUSTOMER"
                        : workerProfile != null && m.SenderId == workerProfile.UserId ? "WORKER"
                        : "UNKNOWN",

                    Type = m.Type.ToString(),
                    Content = m.Content,
                    FileUrl = m.FileUrl,
                    IsRead = m.IsRead,
                    CreatedDate = m.CreatedDate,
                })
                .ToList();

            dtos.Reverse();

            var response = new PagedResponse<ChatMessageDto>
            {
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                Items = dtos,
            };

            return OperationResult<PagedResponse<ChatMessageDto>>.Success(response);
        }

        public async Task<OperationResult<ChatMessageDto>> SendMessageAsync(
            Guid bookingId,
            SendChatMessageRequest request,
            CancellationToken cancellationToken = default
        )
        {
            if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId))
            {
                return OperationResult<ChatMessageDto>.Failure("User is not authenticated.");
            }

            var booking = await _bookingRepository.GetBookingWithWorkerAsync(
                bookingId,
                cancellationToken
            );

            if (booking == null)
            {
                return OperationResult<ChatMessageDto>.Failure("Booking not found.");
            }

            var customerProfile = await _customerProfileRepository.FirstOrDefaultAsync(
                x => x.UserId == currentUserId,
                cancellationToken
            );

            var workerProfile = await _workerProfileRepository.FirstOrDefaultAsync(
                x => x.UserId == currentUserId,
                cancellationToken
            );

            var isCustomer =
                customerProfile != null && booking.CustomerProfileId == customerProfile.Id;

            var isWorker = workerProfile != null && booking.WorkerProfileId == workerProfile.Id;

            if (!isCustomer && !isWorker)
            {
                return OperationResult<ChatMessageDto>.Failure("Forbidden");
            }

            var senderName = isCustomer
                ? booking.CustomerProfile?.User?.FullName ?? "Customer"
                : booking.WorkerProfile?.User?.FullName ?? "Worker";

            var senderRole = isCustomer ? "CUSTOMER" : "WORKER";

            string? fileUrl = null;

            if (request.Type == ChatMessageType.Image || request.Type == ChatMessageType.Voice)
            {
                if (request.File == null)
                {
                    return OperationResult<ChatMessageDto>.Failure("File is required.");
                }

                fileUrl = await _blobService.UploadImageAsync(request.File);
            }

            var chatMessage = new ChatMessage
            {
                BookingId = bookingId,
                SenderId = currentUserId,
                Type = request.Type,
                Content = request.Content,
                FileUrl = fileUrl,
                IsRead = false,
                CreatedDate = DateTime.UtcNow,
            };

            await _chatMessageRepository.AddAsync(chatMessage, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new ChatMessageDto
            {
                Id = chatMessage.Id,
                BookingId = bookingId,
                SenderId = currentUserId,
                SenderName = senderName,
                SenderRole = senderRole,
                Type = chatMessage.Type.ToString(),
                Content = chatMessage.Content,
                FileUrl = chatMessage.FileUrl,
                IsRead = chatMessage.IsRead,
                CreatedDate = chatMessage.CreatedDate,
            };

            var groupName = ChatHub.GetGroupName(bookingId.ToString());

            await _hubContext
                .Clients.Group(groupName)
                .SendAsync("ReceiveChatMessage", dto, cancellationToken);

            return OperationResult<ChatMessageDto>.Success(dto);
        }

        public async Task<OperationResult> MarkAsReadAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default
        )
        {
            if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId))
            {
                return OperationResult.Failure("User is not authenticated.");
            }

            var booking = await _bookingRepository.GetBookingWithWorkerAsync(
                bookingId,
                cancellationToken
            );

            if (booking == null)
            {
                return OperationResult.Failure("Booking not found.");
            }

            var customerProfile = await _customerProfileRepository.FirstOrDefaultAsync(
                x => x.UserId == currentUserId,
                cancellationToken
            );

            var workerProfile = await _workerProfileRepository.FirstOrDefaultAsync(
                x => x.UserId == currentUserId,
                cancellationToken
            );

            var isCustomer =
                customerProfile != null && booking.CustomerProfileId == customerProfile.Id;

            var isWorker = workerProfile != null && booking.WorkerProfileId == workerProfile.Id;

            if (!isCustomer && !isWorker)
            {
                return OperationResult.Failure("Forbidden");
            }

            await _chatMessageRepository.MarkMessagesAsReadAsync(
                bookingId,
                currentUserId,
                cancellationToken
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Messages marked as read successfully.");
        }
    }
}
