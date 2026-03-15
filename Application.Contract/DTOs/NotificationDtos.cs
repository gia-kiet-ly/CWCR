using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    public class NotificationDtos
    {
        // =============================
        // RESPONSE
        // =============================
        public class NotificationResponseDto
        {
            public Guid Id { get; set; }

            public string Title { get; set; } = default!;

            public string Message { get; set; } = default!;

            public string Type { get; set; } = default!;

            public bool IsRead { get; set; }

            public string? ReferenceType { get; set; }

            public Guid? ReferenceId { get; set; }

            public DateTimeOffset CreatedTime { get; set; }
        }

        // =============================
        // FILTER + PAGING
        // =============================
        public class NotificationFilterDto
        {
            public bool? IsRead { get; set; }

            public string? Type { get; set; }

            public DateTimeOffset? FromDate { get; set; }

            public DateTimeOffset? ToDate { get; set; }

            public int PageNumber { get; set; } = 1;

            public int PageSize { get; set; } = 10;
        }

        // =============================
        // PAGED RESULT
        // =============================
        public class PagedNotificationDto
        {
            public int TotalCount { get; set; }

            public int PageNumber { get; set; }

            public int PageSize { get; set; }

            public List<NotificationResponseDto> Items { get; set; } = new();
        }

        // =============================
        // MARK AS READ
        // =============================
        public class MarkAsReadDto
        {
            [Required]
            public Guid NotificationId { get; set; }
        }

        // =============================
        // UNREAD COUNT
        // =============================
        public class NotificationUnreadDto
        {
            public int UnreadCount { get; set; }
        }
    }
}