using Application.Constants;
using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static Application.Contract.DTOs.NotificationDtos;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;

        public NotificationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ================================
        // CREATE
        // ================================
        public async Task CreateAsync(
            Guid userId,
            string type,
            string? referenceType = null,
            Guid? referenceId = null,
            Dictionary<string, string>? parameters = null)
        {
            var repo = _uow.GetRepository<Notification>();

            if (!NotificationConstants.TypeDefinitions.TryGetValue(type, out var def))
                throw new Exception($"Notification type '{type}' not found");

            var title = def.TitleTemplate;
            var message = def.MessageTemplate;

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    title = title?.Replace($"{{{p.Key}}}", p.Value);
                    message = message?.Replace($"{{{p.Key}}}", p.Value);
                }
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                IsRead = false,
                CreatedTime = DateTimeOffset.UtcNow
            };

            await repo.InsertAsync(notification);
            await _uow.SaveAsync();
        }

        // ================================
        // GET USER NOTIFICATIONS
        // ================================
        public async Task<PagedNotificationDto> GetUserNotificationsAsync(
            Guid userId,
            NotificationFilterDto filter)
        {
            var repo = _uow.GetRepository<Notification>();

            var query = repo.NoTrackingEntities
                .Where(x => x.UserId == userId);

            if (filter.IsRead.HasValue)
            {
                query = query.Where(x => x.IsRead == filter.IsRead);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new NotificationResponseDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Message = x.Message,
                    Type = x.Type,
                    ReferenceType = x.ReferenceType,
                    ReferenceId = x.ReferenceId,
                    IsRead = x.IsRead,
                    CreatedTime = x.CreatedTime
                })
                .ToListAsync();

            return new PagedNotificationDto
            {
                TotalCount = total,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = items
            };
        }

        // ================================
        // MARK AS READ
        // ================================
        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var repo = _uow.GetRepository<Notification>();

            var notification = await repo.FirstOrDefaultAsync(x => x.Id == notificationId);

            if (notification == null)
                throw new Exception("Notification not found");

            notification.IsRead = true;

            await repo.UpdateAsync(notification);
            await _uow.SaveAsync();
        }

        // ================================
        // MARK ALL AS READ
        // ================================
        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var repo = _uow.GetRepository<Notification>();

            var notifications = await repo.Entities
                .Where(x => x.UserId == userId && !x.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
            }

            await _uow.SaveAsync();
        }

        // ================================
        // UNREAD COUNT
        // ================================
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            var repo = _uow.GetRepository<Notification>();

            return await repo.NoTrackingEntities
                .CountAsync(x => x.UserId == userId && !x.IsRead);
        }
    }
}