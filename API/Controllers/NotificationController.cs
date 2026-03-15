using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Application.Contract.DTOs.NotificationDtos;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // ======================================
        // GET USER NOTIFICATIONS
        // ======================================
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationFilterDto filter)
        {
            var userId = GetUserId();

            var result = await _notificationService.GetUserNotificationsAsync(userId, filter);

            return Ok(result);
        }

        // ======================================
        // GET UNREAD COUNT
        // ======================================
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();

            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(count);
        }

        // ======================================
        // MARK AS READ
        // ======================================
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);

            return Ok("Notification marked as read");
        }

        // ======================================
        // MARK ALL AS READ
        // ======================================
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();

            await _notificationService.MarkAllAsReadAsync(userId);

            return Ok("All notifications marked as read");
        }

        // ======================================
        // HELPER: GET USER ID FROM TOKEN
        // ======================================
        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                throw new Exception("UserId not found in token");

            return Guid.Parse(userId);
        }
    }
}