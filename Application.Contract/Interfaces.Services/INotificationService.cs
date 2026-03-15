using Application.Contract.DTOs;
using static Application.Contract.DTOs.NotificationDtos;

namespace Application.Contract.Interfaces.Services
{
    public interface INotificationService
    {
        // ================================
        // CREATE
        // ================================
        Task CreateAsync(
            Guid userId,
            string type,
            string? referenceType = null,
            Guid? referenceId = null,
            Dictionary<string, string>? parameters = null
        );

        // ================================
        // GET USER NOTIFICATIONS
        // ================================
        Task<PagedNotificationDto> GetUserNotificationsAsync(
            Guid userId,
            NotificationFilterDto filter
        );

        // ================================
        // MARK AS READ
        // ================================
        Task MarkAsReadAsync(Guid notificationId);

        // ================================
        // MARK ALL AS READ
        // ================================
        Task MarkAllAsReadAsync(Guid userId);

        // ================================
        // UNREAD COUNT
        // ================================
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}