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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBlobService _blobService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IChatMessageRepository chatMessageRepository,
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            IHubContext<ChatHub> hubContext,
            ICurrentUserService currentUserService,
            IBlobService blobService,
            ILogger<ChatService> logger)
        {
            _chatMessageRepository = chatMessageRepository ?? throw new ArgumentNullException(nameof(chatMessageRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult<PagedResponse<ChatMessageDto>>> GetChatHistoryAsync(Guid bookingId, PagedQuery query, CancellationToken cancellationToken = default)
        {
            var currentUserIdStr = _currentUserService.UserId;
            if (string.IsNullOrEmpty(currentUserIdStr))
            {
                return OperationResult<PagedResponse<ChatMessageDto>>.Failure("User is not authenticated.");
            }

            var currentUserId = Guid.Parse(currentUserIdStr);
            var booking = await _bookingRepository.GetBookingWithWorkerAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                return OperationResult<PagedResponse<ChatMessageDto>>.Failure("Booking not found.");
            }

            var isCustomer = booking.CustomerId == currentUserId;
            var isWorker = booking.Worker != null && booking.Worker.UserId == currentUserId;

            if (!isCustomer && !isWorker)
            {
                return OperationResult<PagedResponse<ChatMessageDto>>.Failure("You are not authorized to view this chat history.");
            }

            var (messages, totalCount) = await _chatMessageRepository.GetChatHistoryAsync(bookingId, query, cancellationToken);

            var dtos = messages.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                BookingId = m.BookingId,
                SenderId = m.SenderId,
                SenderName = m.Sender?.FullName ?? (booking.CustomerId == m.SenderId ? (booking.Customer?.FullName ?? "Customer") : (booking.Worker?.User?.FullName ?? "Worker")),
                SenderRole = booking.CustomerId == m.SenderId 
                    ? "CUSTOMER" 
                    : (booking.Worker != null && booking.Worker.UserId == m.SenderId ? "WORKER" : "UNKNOWN"),
                Type = m.Type.ToString(),
                Content = m.Content,
                FileUrl = m.FileUrl,
                IsRead = m.IsRead,
                CreatedDate = m.CreatedDate
            }).ToList();

            // Reverse to ascending order so clients receive history from oldest to newest within the page
            dtos.Reverse();

            var response = new PagedResponse<ChatMessageDto>
            {
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                Items = dtos
            };

            _logger.LogInformation("Successfully retrieved {Count} chat messages for booking: {BookingId}, Page: {PageNumber}", dtos.Count, bookingId, query.PageNumber);

            return OperationResult<PagedResponse<ChatMessageDto>>.Success(response, "Get chat history paged successfully");
        }

        public async Task<OperationResult<ChatMessageDto>> SendMessageAsync(Guid bookingId, SendChatMessageRequest request, CancellationToken cancellationToken = default)
        {
            var currentUserIdStr = _currentUserService.UserId;
            if (string.IsNullOrEmpty(currentUserIdStr))
            {
                return OperationResult<ChatMessageDto>.Failure("User is not authenticated.");
            }

            var currentUserId = Guid.Parse(currentUserIdStr);
            var booking = await _bookingRepository.GetBookingWithWorkerAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                return OperationResult<ChatMessageDto>.Failure("Booking not found.");
            }

            var isCustomer = booking.CustomerId == currentUserId;
            var isWorker = booking.Worker != null && booking.Worker.UserId == currentUserId;

            if (!isCustomer && !isWorker)
            {
                return OperationResult<ChatMessageDto>.Failure("You are not authorized to send messages in this chat room.");
            }

            string senderName = "Unknown";
            string senderRole = "UNKNOWN";
            if (isCustomer)
            {
                senderName = booking.Customer?.FullName ?? "Customer";
                senderRole = "CUSTOMER";
            }
            else if (isWorker)
            {
                senderName = booking.Worker?.User?.FullName ?? "Worker";
                senderRole = "WORKER";
            }

            string? fileUrl = null;
            if (request.Type == ChatMessageType.Image || request.Type == ChatMessageType.Voice)
            {
                if (request.File == null)
                {
                    return OperationResult<ChatMessageDto>.Failure($"File attachment is required for message type: {request.Type}");
                }

                try
                {
                    fileUrl = await _blobService.UploadImageAsync(request.File);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload file attachment for booking {BookingId} by user {UserId}", bookingId, currentUserId);
                    return OperationResult<ChatMessageDto>.Failure($"Failed to upload file: {ex.Message}");
                }
            }

            var chatMessage = new ChatMessage
            {
                BookingId = bookingId,
                SenderId = currentUserId,
                Type = request.Type,
                Content = request.Content,
                FileUrl = fileUrl,
                IsRead = false
            };

            await _chatMessageRepository.AddAsync(chatMessage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully sent and persisted a {MessageType} chat message (ID: {MessageId}) for booking {BookingId} by user {UserId}", request.Type, chatMessage.Id, bookingId, currentUserId);

            var dto = new ChatMessageDto
            {
                Id = chatMessage.Id,
                BookingId = chatMessage.BookingId,
                SenderId = chatMessage.SenderId,
                SenderName = senderName,
                SenderRole = senderRole,
                Type = chatMessage.Type.ToString(),
                Content = chatMessage.Content,
                FileUrl = chatMessage.FileUrl,
                IsRead = chatMessage.IsRead,
                CreatedDate = chatMessage.CreatedDate
            };

            // Broadcast message via SignalR
            var groupName = ChatHub.GetGroupName(bookingId.ToString());
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveChatMessage", dto, cancellationToken);

            return OperationResult<ChatMessageDto>.Success(dto);
        }

        public async Task<OperationResult> MarkAsReadAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var currentUserIdStr = _currentUserService.UserId;
            if (string.IsNullOrEmpty(currentUserIdStr))
            {
                return OperationResult.Failure("User is not authenticated.");
            }

            var currentUserId = Guid.Parse(currentUserIdStr);
            var booking = await _bookingRepository.GetBookingWithWorkerAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                return OperationResult.Failure("Booking not found.");
            }

            var isCustomer = booking.CustomerId == currentUserId;
            var isWorker = booking.Worker != null && booking.Worker.UserId == currentUserId;

            if (!isCustomer && !isWorker)
            {
                return OperationResult.Failure("You are not authorized to mark messages as read for this booking.");
            }

            await _chatMessageRepository.MarkMessagesAsReadAsync(bookingId, currentUserId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully marked messages as read for booking {BookingId} by user {UserId}", bookingId, currentUserId);

            return OperationResult.Success("Messages marked as read successfully.");
        }
    }
}
