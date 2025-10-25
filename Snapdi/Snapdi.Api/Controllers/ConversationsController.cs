using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.Interfaces;
using Snapdi.Services.Interfaces.Snapdi.Services.Interfaces;
using System.Security.Claims;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public ConversationsController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id)) throw new UnauthorizedAccessException();
            return int.Parse(id);
        }

        [HttpGet]
        public async Task<ActionResult> GetMyConversations()
        {
            try
            {
                var userId = GetUserId();
                var conversations = await _messageService.GetUserConversationsAsync(userId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/messages")]
        public async Task<ActionResult> GetMessages(int id, [FromQuery] int? beforeMessageId, [FromQuery] int take = 50)
        {
            try
            {
                var userId = GetUserId();
                var messages = await _messageService.GetConversationMessagesAsync(id, userId, beforeMessageId, take);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public class SendMessageRequest
        {
            public string Content { get; set; } = string.Empty;
        }

        [HttpPost("{id}/messages")]
        public async Task<ActionResult> SendMessage(int id, [FromBody] SendMessageRequest request, 
            [FromServices] Microsoft.AspNetCore.SignalR.IHubContext<Snapdi.Api.Hubs.ChatHub> hub)
        {
            try
            {
                var userId = GetUserId();
                var messageDto = await _messageService.SendMessageAsync(id, userId, request.Content);

                // G?i th�ng b�o qua SignalR
                await hub.Clients.Group($"conversation:{id}").SendCoreAsync("messageReceived", new object[] { messageDto });

                return Ok(messageDto);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("admin")]
        public async Task<ActionResult> GetOrCreateAdminConversation()
        {
            try
            {
                var userId = GetUserId();
                var conversationId = await _messageService.GetOrCreateAdminConversationAsync(userId);
                return Ok(new { conversationId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("with-user/{otherUserId}")]
        public async Task<ActionResult> GetOrCreateUserConversation(int otherUserId)
        {
            try
            {
                var userId = GetUserId();
                if (otherUserId <= 0)
                {
                    return BadRequest("Invalid user ID");
                }

                var conversationId = await _messageService.GetOrCreateUserConversationAsync(userId, otherUserId);
                return Ok(new { conversationId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}


