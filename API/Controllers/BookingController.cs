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
        public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _bookingService.CompleteAsync(id, cancellationToken);
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
    }
}
