using Application.Common;
using Application.DTOs.Support;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    [Route("api/admin/support/tickets")]
    public class AdminSupportTicketController : ApiController
    {
        private readonly ISupportTicketService _supportTicketService;

        public AdminSupportTicketController(ISupportTicketService supportTicketService)
        {
            _supportTicketService = supportTicketService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets([FromQuery] AdminTicketsQuery query, CancellationToken cancellationToken)
        {
            var result = await _supportTicketService.GetAdminTicketsAsync(query, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTicketDetails(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var userRole = GetUserRoles();
            var result = await _supportTicketService.GetTicketDetailsAsync(id, userId, userRole, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id:guid}/assign")]
        public async Task<IActionResult> AssignTicket(Guid id, CancellationToken cancellationToken)
        {
            var adminId = GetUserId();
            var result = await _supportTicketService.AssignTicketAsync(id, adminId, cancellationToken);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateTicketStatus(Guid id, [FromBody] UpdateTicketStatusRequest request, CancellationToken cancellationToken)
        {
            var result = await _supportTicketService.UpdateTicketStatusAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{id:guid}/messages")]
        public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
        {
            var adminId = GetUserId();
            var userRole = GetUserRoles();
            var result = await _supportTicketService.SendMessageAsync(id, adminId, userRole, request.Content, cancellationToken);
            return HandleResult(result);
        }
    }
}
