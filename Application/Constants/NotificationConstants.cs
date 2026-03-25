namespace Application.Constants
{
    public static class NotificationConstants
    {
        /// <summary>
        /// Notification type codes
        /// </summary>
        public static class Types
        {
            // Waste Report
            public const string WASTE_REPORT_CREATED = "WASTE_REPORT_CREATED";
            public const string WASTE_REPORT_REJECTED = "WASTE_REPORT_REJECTED";
            public const string WASTE_REPORT_NO_ENTERPRISE = "WASTE_REPORT_NO_ENTERPRISE";

            // Collection Request
            public const string COLLECTION_ACCEPTED = "COLLECTION_ACCEPTED";
            public const string COLLECTION_REJECTED = "COLLECTION_REJECTED";

            // Collector Assignment
            public const string COLLECTOR_ASSIGNED = "COLLECTOR_ASSIGNED";

            // Collection Proof
            public const string PROOF_UPLOADED = "PROOF_UPLOADED";
            public const string PROOF_VERIFIED = "PROOF_VERIFIED";

            // Dispute
            public const string PROOF_DISPUTED = "PROOF_DISPUTED";
            public const string DISPUTE_RESOLVED = "DISPUTE_RESOLVED";

            // System
            public const string SYSTEM_ANNOUNCEMENT = "SYSTEM_ANNOUNCEMENT";

            public const string PROOF_REJECTED = "PROOF_REJECTED";

        }

        /// <summary>
        /// Reference type for deep linking
        /// </summary>
        public static class ReferenceTypes
        {
            public const string WASTE_REPORT = "WasteReport";
            public const string COLLECTION_REQUEST = "CollectionRequest";
            public const string COLLECTION_PROOF = "CollectionProof";
            public const string DISPUTE = "DisputeResolution";
        }

        /// <summary>
        /// Optional frontend actions
        /// </summary>
        public static class Actions
        {
            public const string OPEN_REPORT = "OPEN_REPORT";
            public const string EDIT_REPORT = "EDIT_REPORT";
            public const string VIEW_PROOF = "VIEW_PROOF";
            public const string OPEN_DISPUTE = "OPEN_DISPUTE";
        }

        /// <summary>
        /// Notification type definitions with templates
        /// Format: (Name, TitleTemplate, MessageTemplate, DefaultIcon)
        /// </summary>
        public static readonly Dictionary<string, NotifTypeDefinition> TypeDefinitions = new()
        {
            [Types.WASTE_REPORT_CREATED] = new(
                "Waste Report Created",
                "Báo cáo rác đã được tạo",
                "Hệ thống đang tìm doanh nghiệp tái chế phù hợp cho báo cáo của bạn.",
                "file_plus"
            ),

            [Types.WASTE_REPORT_REJECTED] = new(
                "Waste Report Rejected",
                "Báo cáo rác bị từ chối",
                "Vui lòng chỉnh sửa thông tin và gửi lại báo cáo.",
                "alert_triangle"
            ),

            [Types.WASTE_REPORT_NO_ENTERPRISE] = new(
                "No Enterprise Accepted",
                "Không có doanh nghiệp nhận xử lý",
                "Không có doanh nghiệp nào chấp nhận báo cáo của bạn. Vui lòng chỉnh sửa hoặc tạo báo cáo mới.",
                "alert_circle"
            ),

            [Types.COLLECTION_ACCEPTED] = new(
                "Collection Accepted",
                "Yêu cầu thu gom đã được chấp nhận",
                "Doanh nghiệp tái chế đã nhận xử lý báo cáo của bạn.",
                "check_circle"
            ),

            [Types.COLLECTION_REJECTED] = new(
                "Collection Rejected",
                "Yêu cầu thu gom bị từ chối",
                "Doanh nghiệp đã từ chối yêu cầu. Hệ thống sẽ tìm doanh nghiệp khác.",
                "x_circle"
            ),

            [Types.COLLECTOR_ASSIGNED] = new(
                "Collector Assigned",
                "Bạn có nhiệm vụ thu gom mới",
                "Bạn vừa được giao nhiệm vụ thu gom rác.",
                "truck"
            ),

            [Types.PROOF_UPLOADED] = new(
                "Collection Proof Uploaded",
                "Ảnh chứng minh đã được tải lên",
                "Collector đã tải lên ảnh chứng minh thu gom. Vui lòng kiểm tra.",
                "camera"
            ),

            [Types.PROOF_VERIFIED] = new(
                "Proof Verified",
                "Ảnh chứng minh đã được xác nhận",
                "Cảm ơn bạn đã tham gia vào hoạt động tái chế.",
                "check_circle"
            ),

            [Types.PROOF_DISPUTED] = new(
                "Proof Disputed",
                "Có tranh chấp về ảnh chứng minh",
                "Một tranh chấp đã được tạo cho quá trình thu gom.",
                "alert_triangle"
            ),

            [Types.DISPUTE_RESOLVED] = new(
                "Dispute Resolved",
                "Tranh chấp đã được giải quyết",
                "Admin đã xử lý tranh chấp của bạn.",
                "gavel"
            ),

            [Types.SYSTEM_ANNOUNCEMENT] = new(
                "System Announcement",
                "Thông báo hệ thống",
                null,
                "megaphone"
            ),

            [Types.PROOF_REJECTED] = new(
                "Proof Rejected",
                "Ảnh chứng minh bị từ chối",
                "Ảnh chứng minh của bạn không được chấp nhận. Vui lòng gửi lại.",
                "x_circle"
            ),
        };

        public class NotifTypeDefinition
        {
            public string Name { get; set; }
            public string? TitleTemplate { get; set; }
            public string? MessageTemplate { get; set; }
            public string? DefaultIconName { get; set; }

            public NotifTypeDefinition(
                string name,
                string? titleTemplate,
                string? messageTemplate,
                string? defaultIconName)
            {
                Name = name;
                TitleTemplate = titleTemplate;
                MessageTemplate = messageTemplate;
                DefaultIconName = defaultIconName;
            }
        }
    }
}