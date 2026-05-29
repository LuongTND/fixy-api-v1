using Application.Common;
using Application.DTOs.Booking;
using Application.DTOs.Support;
using Application.Interfaces;
using Application.Interfaces.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entity;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services.Support
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly ISupportTicketRepository _supportTicketRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IWorkerProfileRepository _workerProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISupportHubService _supportHubService;
        private readonly IBookingHubService _bookingHubService;

        public SupportTicketService(
            ISupportTicketRepository supportTicketRepository,
            IBookingRepository bookingRepository,
            ICustomerProfileRepository customerProfileRepository,
            IWorkerProfileRepository workerProfileRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ISupportHubService supportHubService,
            IBookingHubService bookingHubService)
        {
            _supportTicketRepository = supportTicketRepository ?? throw new ArgumentNullException(nameof(supportTicketRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _customerProfileRepository = customerProfileRepository ?? throw new ArgumentNullException(nameof(customerProfileRepository));
            _workerProfileRepository = workerProfileRepository ?? throw new ArgumentNullException(nameof(workerProfileRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _supportHubService = supportHubService ?? throw new ArgumentNullException(nameof(supportHubService));
            _bookingHubService = bookingHubService ?? throw new ArgumentNullException(nameof(bookingHubService));
        }

        public async Task<OperationResult<SupportTicketDto>> CreateTicketAsync(Guid reporterId, string reporterRole, CreateSupportTicketRequest request, CancellationToken cancellationToken = default)
        {
            // Determine Reporter Type
            SupportReporterType reporterType;
            if (reporterRole.Equals("WORKER", StringComparison.OrdinalIgnoreCase))
            {
                reporterType = SupportReporterType.Worker;

                // Double check if worker exists
                var workerProfile = await _workerProfileRepository.FirstOrDefaultAsync(x => x.UserId == reporterId, cancellationToken);
                if (workerProfile == null)
                {
                    return OperationResult<SupportTicketDto>.Failure("Worker profile not found");
                }
            }
            else if (reporterRole.Equals("CUSTOMER", StringComparison.OrdinalIgnoreCase))
            {
                reporterType = SupportReporterType.Customer;
                var customerProfile = await _customerProfileRepository.FirstOrDefaultAsync(x => x.UserId == reporterId, cancellationToken);
                if (customerProfile == null)
                {
                    return OperationResult<SupportTicketDto>.Failure("Customer profile not found");
                }
            }
            else
            {
                return OperationResult<SupportTicketDto>.Failure("Invalid role for reporting tickets");
            }

            // Verify Booking if provided
            if (request.BookingId.HasValue)
            {
                var booking = await _bookingRepository.GetByIdAsync(request.BookingId.Value, cancellationToken);
                if (booking == null)
                {
                    return OperationResult<SupportTicketDto>.Failure("Booking not found");
                }

                // Check ownership/relation
                if (reporterType == SupportReporterType.Customer)
                {
                    var customerProfile = await _customerProfileRepository.FirstOrDefaultAsync(x => x.UserId == reporterId, cancellationToken);
                    if (customerProfile == null || booking.CustomerProfileId != customerProfile.Id)
                    {
                        return OperationResult<SupportTicketDto>.Failure("Forbidden: Booking does not belong to this customer");
                    }
                }
                else // Worker
                {
                    var workerProfile = await _workerProfileRepository.FirstOrDefaultAsync(x => x.UserId == reporterId, cancellationToken);
                    if (workerProfile == null || booking.WorkerProfileId != workerProfile.Id)
                    {
                        return OperationResult<SupportTicketDto>.Failure("Forbidden: Booking is not assigned to this worker");
                    }
                }
            }

            var ticket = new SupportTicket
            {
                ReporterId = reporterId,
                BookingId = request.BookingId,
                ReporterType = reporterType,
                Category = request.Category,
                Subject = request.Subject,
                Priority = request.Priority,
                Status = SupportStatus.Open,
                CreatedDate = DateTime.UtcNow,
                Messages = new List<SupportMessage>
                {
                    new SupportMessage
                    {
                        SenderId = reporterId,
                        Content = request.Description,
                        CreatedDate = DateTime.UtcNow
                    }
                }
            };

            await _supportTicketRepository.AddAsync(ticket, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<SupportTicketDto>(ticket);
            return OperationResult<SupportTicketDto>.Success(dto, "Ticket created successfully");
        }

        public async Task<OperationResult<PagedResponse<SupportTicketDto>>> GetUserTicketsAsync(Guid userId, PagedQuery query, CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _supportTicketRepository.GetUserTicketsPagedAsync(
                userId, 
                query.PageNumber, 
                query.PageSize, 
                cancellationToken
            );

            var dtos = _mapper.Map<List<SupportTicketDto>>(items);

            var pagedResponse = new PagedResponse<SupportTicketDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return OperationResult<PagedResponse<SupportTicketDto>>.Success(pagedResponse, "Tickets retrieved successfully");
        }

        public async Task<OperationResult<SupportTicketDetailsDto>> GetTicketDetailsAsync(Guid ticketId, Guid userId, string userRole, CancellationToken cancellationToken = default)
        {
            var ticket = await _supportTicketRepository.GetTicketDetailsWithMessagesAsync(ticketId, cancellationToken);
            if (ticket == null)
            {
                return OperationResult<SupportTicketDetailsDto>.Failure("Ticket not found");
            }

            // Authorization check: Must be the reporter or an Admin
            bool isAdmin = userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && ticket.ReporterId != userId)
            {
                return OperationResult<SupportTicketDetailsDto>.Failure("Forbidden: You do not have permission to view this ticket");
            }

            var dto = _mapper.Map<SupportTicketDetailsDto>(ticket);
            return OperationResult<SupportTicketDetailsDto>.Success(dto, "Ticket details retrieved successfully");
        }

        public async Task<OperationResult<PagedResponse<SupportMessageDto>>> GetTicketMessagesAsync(
            Guid ticketId, 
            Guid userId, 
            string userRole, 
            PagedQuery query, 
            CancellationToken cancellationToken = default)
        {
            var ticket = await _supportTicketRepository.GetByIdAsync(ticketId, cancellationToken);
            if (ticket == null)
            {
                return OperationResult<PagedResponse<SupportMessageDto>>.Failure("Ticket not found");
            }

            // Authorization check: Must be the reporter or an Admin
            bool isAdmin = userRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && ticket.ReporterId != userId)
            {
                return OperationResult<PagedResponse<SupportMessageDto>>.Failure("Forbidden: You do not have permission to view messages for this ticket");
            }

            var (items, totalCount) = await _supportTicketRepository.GetTicketMessagesPagedAsync(
                ticketId, 
                query.PageNumber, 
                query.PageSize, 
                cancellationToken
            );

            var dtos = _mapper.Map<List<SupportMessageDto>>(items);

            var pagedResponse = new PagedResponse<SupportMessageDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return OperationResult<PagedResponse<SupportMessageDto>>.Success(pagedResponse, "Ticket messages retrieved successfully");
        }

        public async Task<OperationResult<SupportMessageDto>> SendMessageAsync(Guid ticketId, Guid senderId, string senderRole, string content, CancellationToken cancellationToken = default)
        {
            var ticket = await _supportTicketRepository.GetByIdAsync(ticketId, cancellationToken);
            if (ticket == null)
            {
                return OperationResult<SupportMessageDto>.Failure("Ticket not found");
            }

            // Authorization check
            bool isAdmin = senderRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && ticket.ReporterId != senderId)
            {
                return OperationResult<SupportMessageDto>.Failure("Forbidden: You do not have permission to send messages to this ticket");
            }

            // Ticket must not be Closed (unless Admin wants to send message, but generally Closed ticket should not receive messages)
            if (ticket.Status == SupportStatus.Closed)
            {
                return OperationResult<SupportMessageDto>.Failure("Cannot send message to a closed ticket");
            }

            var message = new SupportMessage
            {
                TicketId = ticketId,
                SenderId = senderId,
                Content = content,
                CreatedDate = DateTime.UtcNow
            };

            // Add directly to DbContext or load navigation.
            // Since SupportTicket doesn't have an explicit repository for SupportMessage,
            // we can just add it to ticket's message collection, or if ticket is attached to context, EF core tracks it.
            // Actually, we can add it to the ticket collection or use the AppDbContext if we had it, but SupportTicket's messages
            // collection is monitored. We can just add it to ticket.Messages.
            // But since ticket was retrieved without Messages, adding to ticket.Messages might trigger a save if we update ticket.
            // Let's retrieve ticket WITH messages to safely append, or use the DbSet directly.
            // A safer approach: since _supportTicketRepository inherits from Repository<SupportTicket>, it has _context.
            // But we don't expose _context. However, we can fetch the ticket with messages first:
            var ticketWithMessages = await _supportTicketRepository.GetTicketDetailsWithMessagesAsync(ticketId, cancellationToken);
            if (ticketWithMessages == null)
            {
                return OperationResult<SupportMessageDto>.Failure("Ticket not found");
            }

            ticketWithMessages.Messages.Add(message);
            _supportTicketRepository.Update(ticketWithMessages);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Re-load the message with sender info to populate mapped properties correctly
            // To do this, let's load sender details:
            var sender = await _userRepository.GetByIdWithRolesAsync(senderId, cancellationToken);
            if (sender != null)
            {
                // Load roles for sender
                // In our repo pattern, we can fetch roles if they are not loaded, or map manually.
                // Let's fetch the sender with roles from DB to be clean:
                // Actually, our repository exposes FindAsync / FirstOrDefaultAsync, but we want Roles eager loaded.
                // Let's do it manually in mapping or fetch sender:
                message.Sender = sender;
            }

            var dto = _mapper.Map<SupportMessageDto>(message);

            // Broadcast message via SignalR
            await _supportHubService.SendSupportMessageAsync(ticketId, dto, cancellationToken);

            return OperationResult<SupportMessageDto>.Success(dto, "Message sent successfully");
        }

        public async Task<OperationResult<PagedResponse<SupportTicketDto>>> GetAdminTicketsAsync(AdminTicketsQuery query, CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _supportTicketRepository.GetAdminTicketsPagedAsync(query, cancellationToken);
            var dtos = _mapper.Map<List<SupportTicketDto>>(items);

            var pagedResponse = new PagedResponse<SupportTicketDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return OperationResult<PagedResponse<SupportTicketDto>>.Success(pagedResponse, "Admin tickets retrieved successfully");
        }

        public async Task<OperationResult> AssignTicketAsync(Guid ticketId, Guid adminId, CancellationToken cancellationToken = default)
        {
            var ticket = await _supportTicketRepository.GetByIdAsync(ticketId, cancellationToken);
            if (ticket == null)
            {
                return OperationResult.Failure("Ticket not found");
            }

            ticket.AssignedToId = adminId;
            ticket.Status = SupportStatus.InProgress;
            ticket.UpdatedDate = DateTime.UtcNow;

            _supportTicketRepository.Update(ticket);

            // If the ticket is related to a Booking and is a Dispute, transition booking status to Disputed
            if (ticket.BookingId.HasValue && ticket.Category == SupportCategory.Dispute)
            {
                var booking = await _bookingRepository.GetByIdAsync(ticket.BookingId.Value, cancellationToken);
                if (booking != null && booking.Status != BookingStatus.Disputed)
                {
                    var oldStatus = booking.Status;
                    booking.Status = BookingStatus.Disputed;
                    booking.UpdatedDate = DateTime.UtcNow;
                    _bookingRepository.Update(booking);

                    // Send Realtime status update to booking hub clients
                    await _bookingHubService.SendStatusUpdateAsync(
                        booking.Id,
                        new BookingStatusUpdateDto
                        {
                            BookingId = booking.Id,
                            Status = BookingStatus.Disputed.ToString(),
                            UpdatedAt = DateTime.UtcNow,
                            Message = $"Admin accepted dispute ticket. Booking status changed from {oldStatus} to Disputed."
                        },
                        cancellationToken
                    );
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Ticket assigned successfully");
        }

        public async Task<OperationResult> UpdateTicketStatusAsync(Guid ticketId, UpdateTicketStatusRequest request, CancellationToken cancellationToken = default)
        {
            var ticket = await _supportTicketRepository.GetByIdAsync(ticketId, cancellationToken);
            if (ticket == null)
            {
                return OperationResult.Failure("Ticket not found");
            }

            ticket.Status = request.Status;
            if (request.Status == SupportStatus.Resolved || request.Status == SupportStatus.Closed)
            {
                ticket.ResolvedAt = DateTime.UtcNow;
            }

            if (request.Priority.HasValue)
            {
                ticket.Priority = request.Priority.Value;
            }

            if (request.Category.HasValue)
            {
                ticket.Category = request.Category.Value;
            }

            ticket.UpdatedDate = DateTime.UtcNow;

            _supportTicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Ticket status updated successfully");
        }
    }
}
