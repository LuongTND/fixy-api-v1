using Application.DTOs.Worker;
using Application.Interfaces.Services.Worker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/workers")]
    public class WorkerLocationController : ApiController
    {
        private readonly IWorkerLocationService _workerLocationService;

        public WorkerLocationController(IWorkerLocationService workerLocationService)
        {
            _workerLocationService = workerLocationService;
        }

        /// <summary>
        /// Worker App calls this periodically (every 10-30s) while traveling to a booking.
        /// Saves GPS to Redis and pushes coordinates to the customer's tracking screen via SignalR.
        /// </summary>
        [HttpPost("location")]
        public async Task<IActionResult> UpdateLocation(
            [FromBody] UpdateWorkerLocationRequest request,
            CancellationToken cancellationToken
        )
        {
            var workerId = GetUserId();
            var result = await _workerLocationService.UpdateLocationAsync(workerId, request, cancellationToken);
            return HandleResult(result);
        }
    }
}
