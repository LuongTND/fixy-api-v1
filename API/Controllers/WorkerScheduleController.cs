using Application.DTOs.WorkerProfile;
using Application.DTOs.WorkerProfile.WorkerSchedule;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/worker-schedules")]
    [ApiController]
    public class WorkerSchedulesController : ControllerBase
    {
        private readonly IWorkerWeeklyScheduleService _workerScheduleService;
        private readonly IWorkerScheduleExceptionService _workerScheduleExceptionService;

        public WorkerSchedulesController(
            IWorkerWeeklyScheduleService workerScheduleService,
            IWorkerScheduleExceptionService workerScheduleExceptionService
        )
        {
            _workerScheduleService = workerScheduleService;
            _workerScheduleExceptionService = workerScheduleExceptionService;
        }

        [HttpPut("{workerProfileId}/weekly")]
        public async Task<IActionResult> UpdateWeeklySchedule(
            Guid workerProfileId,
            [FromBody] UpdateWeeklyScheduleRequestDto request,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerScheduleService.UpdateWeeklyScheduleAsync(
                workerProfileId,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                request.IsActive,
                cancellationToken
            );

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{workerProfileId}/day-off")]
        public async Task<IActionResult> AddDayOff(
            Guid workerProfileId,
            [FromBody] AddDayOffRequestDto request,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerScheduleExceptionService.AddDayOffAsync(
                workerProfileId,
                request.Date,
                request.Reason,
                cancellationToken
            );

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{workerProfileId}/day-off")]
        public async Task<IActionResult> RemoveDayOff(
            Guid workerProfileId,
            [FromQuery] DateOnly date,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerScheduleExceptionService.RemoveDayOffAsync(
                workerProfileId,
                date,
                cancellationToken
            );

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{workerProfileId}/check-availability")]
        public async Task<IActionResult> CheckAvailability(
            Guid workerProfileId,
            [FromBody] CheckWorkerAvailabilityRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerScheduleExceptionService.IsWorkerAvailableAsync(
                workerProfileId,
                request.BookingTime,
                cancellationToken
            );

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{workerProfileId}/weekly")]
        public async Task<IActionResult> GetWeeklySchedules(
            Guid workerProfileId,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerScheduleService.GetWeeklySchedulesAsync(
                workerProfileId,
                cancellationToken
            );

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{workerProfileId}/exceptions")]
        public async Task<IActionResult> GetScheduleExceptions(
            Guid workerProfileId,
            CancellationToken cancellationToken
        )
        {
            var result = await _workerScheduleExceptionService.GetScheduleExceptionsAsync(
                workerProfileId,
                cancellationToken
            );

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
