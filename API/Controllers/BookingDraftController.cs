using Application.DTOs.BookingDraft;
using Application.Interfaces.Services.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/bookings/drafts")]
    public class BookingDraftController : ApiController
    {
        private readonly IBookingDraftService _bookingDraftService;

        public BookingDraftController(IBookingDraftService bookingDraftService)
        {
            _bookingDraftService = bookingDraftService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingDraftRequest request,CancellationToken cancellationToken)
        {
            var result = await _bookingDraftService.CreateAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _bookingDraftService.GetAllAsync(cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{draftId:guid}")]
        public async Task<IActionResult> GetById(Guid draftId, CancellationToken cancellationToken)
        {
            var result = await _bookingDraftService.GetByIdAsync(draftId, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{draftId:guid}")]
        public async Task<IActionResult> Update(Guid draftId,[FromBody] UpdateBookingDraftRequest request,CancellationToken cancellationToken)
        {
            var result = await _bookingDraftService.UpdateAsync(draftId, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{draftId:guid}")]
        public async Task<IActionResult> Delete(Guid draftId, CancellationToken cancellationToken)
        {
            var result = await _bookingDraftService.DeleteAsync(draftId, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{draftId:guid}/confirm")]
        public async Task<IActionResult> Confirm(Guid draftId, CancellationToken cancellationToken)
        {
            var result = await _bookingDraftService.ConfirmAsync(draftId, cancellationToken);
            return HandleResult(result);
        }
    }
}
