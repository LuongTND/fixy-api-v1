using Application.DTOs.Booking;
using Application.Interfaces.Services.Booking;
using Application.Interfaces.Services.Worker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : ApiController
    {
        private readonly IBookingService _bookingService;
        private readonly IWorkerLocationService _workerLocationService;

        public BookingController(IBookingService bookingService, IWorkerLocationService workerLocationService)
        {
            _bookingService = bookingService;
            _workerLocationService = workerLocationService;
        }

        [Authorize(Roles = "WORKER, CUSTOMER")]
        // Get booking detail by ID.
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _bookingService.GetByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "CUSTOMER, WORKER")]
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingRequest request, CancellationToken cancellationToken)
        {
            var result = await _bookingService.CancelAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        // Worker accepts the booking. Pending -> Confirmed.
        [HttpPost("{id:guid}/accept")]
        public async Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken)
        {
            var result = await _bookingService.AcceptAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        // Worker starts traveling. Confirmed -> Traveling.
        [HttpPost("{id:guid}/start-travel")]
        public async Task<IActionResult> StartTravel(Guid id, CancellationToken cancellationToken)
        {
            var result = await _bookingService.StartTravelAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        // Worker arrives at location. Traveling -> Arrived.
        [HttpPost("{id:guid}/arrive")]
        public async Task<IActionResult> Arrive(Guid id, CancellationToken cancellationToken)
        {
            var result = await _bookingService.ArriveAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        // Worker starts working. Arrived -> InProgress.
        [HttpPost("{id:guid}/start-work")]
        public async Task<IActionResult> StartWork(Guid id, CancellationToken cancellationToken)
        {
            var result = await _bookingService.StartWorkAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        // Worker completes the job. InProgress -> Completed.
        [HttpPost("{id:guid}/complete")]
        public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteBookingRequest request, CancellationToken cancellationToken)
        {
            var result = await _bookingService.CompleteAsync(id, request, cancellationToken);
            return HandleResult(result);
        }
        /// <summary>
        /// Get real-time tracking info: current status + last known worker GPS.
        /// Called once when the customer opens the tracking screen (before SignalR kicks in).
        /// </summary>
        [Authorize(Roles = "CUSTOMER")]
        [HttpGet("{id:guid}/tracking")]
        public async Task<IActionResult> GetTracking(Guid id, CancellationToken cancellationToken)
        {
            var result = await _workerLocationService.GetBookingTrackingAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "CUSTOMER, ADMIN")]
        [HttpGet("{id:guid}/matching-queue")]
        public async Task<IActionResult> GetMatchingQueue(Guid id, CancellationToken cancellationToken)
        {
            var result = await _bookingService.GetMatchingQueueAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        // Worker declines the booking with a reason. Triggers re-routing to next worker.
        [HttpPost("{id:guid}/decline")]
        public async Task<IActionResult> Decline(Guid id, [FromBody] DeclineBookingRequest request, CancellationToken cancellationToken)
        {
            var result = await _bookingService.DeclineAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        // Worker proposes alternative price/time before accepting.
        [HttpPost("{id:guid}/propose")]
        public async Task<IActionResult> Propose(Guid id, [FromBody] ProposeBookingRequest request, CancellationToken cancellationToken)
        {
            var result = await _bookingService.ProposeAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "CUSTOMER")]
        // Customer accepts or rejects the worker's counter-proposal.
        [HttpPost("{id:guid}/respond-proposal")]
        public async Task<IActionResult> RespondProposal(Guid id, [FromBody] RespondProposalRequest request, CancellationToken cancellationToken)
        {
            var result = await _bookingService.RespondProposalAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        [HttpPost("{id:guid}/report-issue")]
        public async Task<IActionResult> ReportIssue(Guid id, [FromBody] ReportBookingIssueRequest request, CancellationToken cancellationToken)
        {
            var result = await _bookingService.ReportIssueAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetAllBookings([FromQuery] AllBookingsQuery query, CancellationToken cancellationToken)
        {
            var result = await _bookingService.GetAllBookingsAsync(query, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "CUSTOMER")]
        [HttpGet("customer")]
        public async Task<IActionResult> GetCustomerBookings([FromQuery] CustomerBookingsQuery query, CancellationToken cancellationToken)
        {
            var result = await _bookingService.GetCustomerBookingsAsync(query, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "WORKER")]
        [HttpGet("worker")]
        public async Task<IActionResult> GetWorkerBookings([FromQuery] WorkerBookingsQuery query, CancellationToken cancellationToken)
        {
            var result = await _bookingService.GetWorkerBookingsAsync(query, cancellationToken);
            return HandleResult(result);
        }
    }
}
