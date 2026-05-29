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
    [Authorize]
    [ApiController]
    [Route("api/support/tickets")]
    public class SupportTicketController : ApiController
    {
        private readonly ISupportTicketService _supportTicketService;

        public SupportTicketController(ISupportTicketService supportTicketService)
        {
            _supportTicketService = supportTicketService;
        }

        [Authorize(Roles = "CUSTOMER, WORKER")]
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateSupportTicketRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var userRole = GetUserRoles();
            var result = await _supportTicketService.CreateTicketAsync(userId, userRole, request, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "CUSTOMER, WORKER")]
        [HttpGet]
        public async Task<IActionResult> GetMyTickets([FromQuery] PagedQuery query, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _supportTicketService.GetUserTicketsAsync(userId, query, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "CUSTOMER, WORKER")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTicketDetails(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var userRole = GetUserRoles();
            var result = await _supportTicketService.GetTicketDetailsAsync(id, userId, userRole, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "ADMIN, CUSTOMER, WORKER")]
        [HttpGet("{id:guid}/messages")]
        public async Task<IActionResult> GetTicketMessages(Guid id, [FromQuery] PagedQuery query, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var userRole = GetUserRoles();
            var result = await _supportTicketService.GetTicketMessagesAsync(id, userId, userRole, query, cancellationToken);
            return HandleResult(result);
        }

        [Authorize(Roles = "CUSTOMER, WORKER")]
        [HttpPost("{id:guid}/messages")]
        public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var userRole = GetUserRoles();
            var result = await _supportTicketService.SendMessageAsync(id, userId, userRole, request.Content, cancellationToken);
            return HandleResult(result);
        }
    }
}
