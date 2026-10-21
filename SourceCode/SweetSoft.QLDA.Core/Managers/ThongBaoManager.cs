using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.MailManager;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.Managers
{
    /// <summary>
    /// Các loại thông báo dùng để phân biệt nguồn gốc và hiển thị icon phù hợp.
    /// </summary>
    public static class ThongBaoTypes
    {
        /// <summary>Được giao / cập nhật công việc</summary>
        public const string CongViec = "CONG_VIEC";

        /// <summary>Được thêm vào / cập nhật dự án</summary>
        public const string DuAn = "DU_AN";

        /// <summary>Được thêm vào lịch họp</summary>
        public const string LichHop = "LICH_HOP";

        /// <summary>Vấn đề (Issue) được giao</summary>
        public const string VanDe = "VAN_DE";

        /// <summary>Tài liệu cần ký duyệt</summary>
        public const string TaiLieu = "TAI_LIEU";

        /// <summary>Thông báo hệ thống chung</summary>
        public const string HeThong = "HE_THONG";

        /// <summary>Rủi ro được yêu cầu giám sát</summary>
        public const string RuiRo = "RUI_RO";

        /// <summary>Khoản chi cần duyệt/đã duyệt</summary>
        public const string ChiPhi = "CHI_PHI";
    }

    /// <summary>
    /// Manager xử lý nghiệp vụ thông báo trong ứng dụng.
    /// Sử dụng Singleton pattern giống các manager khác trong dự án.
    /// </summary>
    public class ThongBaoManager : BaseManager
    {
        #region Singleton

        private static readonly Lazy<ThongBaoManager> _instance =
            new Lazy<ThongBaoManager>(() => new ThongBaoManager());

        public static ThongBaoManager Instance => _instance.Value;

        private readonly ThongBaoRepository _repository;
        private readonly AuditManager _auditManager;

        public ThongBaoManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new ThongBaoRepository(_auditManager);
        }

        #endregion

        // DOT 3 - Core KHONG tham chieu BackOffice/SignalR. Startup gan callback
        // luc website khoi dong; publication la best-effort, khong anh huong DB.
        public static Action<Guid> SauKhiThongBaoThayDoi { get; set; }

        private static void PhatThayDoiSauLuu(Guid userId)
        {
            if (userId == Guid.Empty) return;
            Action<Guid> callback = SauKhiThongBaoThayDoi;
            if (callback == null) return;

            var transaction = System.Transactions.Transaction.Current;
            if (transaction == null)
            {
                PhatAnToan(callback, userId);
                return;
            }

            // Quan trong: KBG/Task co the tao thong bao TRONG TransactionScope.
            // Tuyet doi khong publish truoc COMMIT, khong publish khi rollback.
            try
            {
                transaction.TransactionCompleted += (sender, e) =>
                {
                    try
                    {
                        if (e.Transaction != null &&
                            e.Transaction.TransactionInformation.Status ==
                                System.Transactions.TransactionStatus.Committed)
                            PhatAnToan(callback, userId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError("QLDA notifications realtime completion: {0}", ex);
                    }
                };
            }
            catch (Exception ex)
            {
                // Dang trong transaction thi KHONG duoc fallback publish ngay.
                System.Diagnostics.Trace.TraceError("QLDA notifications realtime subscribe: {0}", ex);
            }
        }

        private static void PhatAnToan(Action<Guid> callback, Guid userId)
        {
            try
            {
                // Dam bao publisher chay ngoai ambient transaction.
                using (var suppressed = new System.Transactions.TransactionScope(
                    System.Transactions.TransactionScopeOption.Suppress))
                {
                    callback(userId);
                    suppressed.Complete();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("QLDA notifications realtime callback: {0}", ex);
            }
        }

        #region Create

        /// <summary>
        /// Delegate nhận logic sinh URL từ tầng UI/BackOffice (tránh vòng lặp phụ thuộc).
        /// </summary>
        public static Func<string, Guid?, Guid?, string> ResolveNotificationUrlFunc { get; set; }

        /// <summary>
        /// Tạo một thông báo mới cho nhân viên và gửi Email ngầm.
        /// </summary>
        /// <param name="userId">User nhận thông báo (aspnet_Users.UserId)</param>
        /// <param name="tieuDe">Tiêu đề ngắn gọn hiển thị trong dropdown</param>
        /// <param name="noiDung">Nội dung chi tiết (có thể null)</param>
        /// <param name="loaiThongBao">Một trong các hằng số <see cref="ThongBaoTypes"/></param>
        /// <param name="idCongViec">FK đến TblCongViec (nullable)</param>
        /// <param name="idDuAn">FK đến TblDuAn (nullable)</param>
        /// <returns>Bản ghi vừa được tạo, hoặc null nếu có lỗi</returns>
        public TblThongBao Create(
            Guid userId,
            string tieuDe,
            string noiDung          = null,
            string loaiThongBao     = ThongBaoTypes.HeThong,
            Guid?  idCongViec       = null,
            Guid?  idDuAn           = null,
            string duongDanLienKet = null,
            bool guiEmail = true)
        {
            if (userId == Guid.Empty)
                return null;

            if (string.IsNullOrWhiteSpace(tieuDe))
                return null;

            string currentUser = _applicationContext?.UserName ?? "System";

            TblThongBao item = new TblThongBao
            {
                IdThongBao = Guid.NewGuid(),
                UserId = userId,
                IdCongViec = idCongViec,
                IdDuAn = idDuAn,
                TieuDe = tieuDe.Length > 255 ? tieuDe.Substring(0, 255) : tieuDe,
                NoiDung = noiDung,
                LoaiThongBao = loaiThongBao,
                DuongDanLienKet = string.IsNullOrWhiteSpace(duongDanLienKet) ? null : duongDanLienKet.Trim(),
                DaDoc = false,
                NgayDoc = null,
                DaXoa = false,
                NguoiTao = currentUser,
                NgayTao = DateTime.Now,
                NguoiCapNhat = currentUser,
                NgayCapNhat = DateTime.Now
            };

            var result = _repository.Insert(item);
            if (result != null) PhatThayDoiSauLuu(result.UserId);

            if (result != null && guiEmail)
            {
                // Mac dinh true de giu nguyen hanh vi cac luong cu.
                // Rieng 2A guiEmail=false theo chinh sach chi thong bao web.
                SendEmailNotification(result);
            }

            return result;
        }
        public TblThongBao GetById(Guid idThongBao)
        {
            if (idThongBao == Guid.Empty)
                return null;

            return _repository.GetById(idThongBao);
        }
        private void SendEmailNotification(TblThongBao notification)
        {
            Task.Run(async () =>
            {
                try
                {
                    var recipient = UserManager.Instance.GetUserById(notification.UserId);
                    if (recipient == null) return;
                    var memUser = System.Web.Security.Membership.GetUser(recipient.UserName);
                    string email = memUser != null ? memUser.Email : null;
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        return;
                    }

                    string url = ResolveNotificationUrlFunc != null
                        ? ResolveNotificationUrlFunc(
                            notification.LoaiThongBao,
                            notification.IdDuAn,
                            notification.IdCongViec)
                        : string.Empty;

                    string notificationBody =
                        string.IsNullOrWhiteSpace(notification.NoiDung)
                            ? notification.TieuDe
                            : notification.NoiDung;

                    string notificationTypeString = notification.LoaiThongBao;
                    switch (notification.LoaiThongBao)
                    {
                        case ThongBaoTypes.CongViec:
                            notificationTypeString = "Công việc";
                            break;

                        case ThongBaoTypes.DuAn:
                            notificationTypeString = "Dự án";
                            break;

                        case ThongBaoTypes.LichHop:
                            notificationTypeString = "Lịch họp";
                            break;

                        case ThongBaoTypes.VanDe:
                            notificationTypeString = "Vấn đề";
                            break;

                        case ThongBaoTypes.TaiLieu:
                            notificationTypeString = "Tài liệu";
                            break;

                        case ThongBaoTypes.HeThong:
                            notificationTypeString = "Hệ thống";
                            break;
                    }
                    var placeholdersBody = new Dictionary<string, string>
                    {
                        { "[[COMPANY_NAME]]", "SweetSoft QLDA" },
                        { "[[FULL_NAME]]", recipient.DisplayName },
                        { "[[NOTIFICATION_TYPE]]", notificationTypeString },
                        { "[[NOTIFICATION_TITLE]]", notification.TieuDe },
                        { "[[NOTIFICATION_BODY]]", notificationBody },
                        {
                            "[[NOTIFICATION_TIME]]",
                            notification.NgayTao.ToString("dd/MM/yyyy HH:mm")
                        },
                        {
                            "[[NOTIFICATION_URL]]",
                            string.IsNullOrEmpty(url)
                                ? "javascript:void(0)"
                                : url
                        }
                    };

                    var emailManager = new EmailManager(SweetContext.CreateBackgroundContext());

                    // Lưu các giá trị cần dùng vào biến local
                    Guid notificationId = notification.IdThongBao;

                    await emailManager.SendEmailWithTemplateAsync(
                        refId: notification.UserId,
                        refType: EmailType.Notification,
                        customerId: notification.UserId,
                        toEmail: email,
                        templateKey: "TemplateNotification",
                        formatType: EmailFormatTypes.Admin,
                        placeholdersBody: placeholdersBody,
                        attachments: null,
                        useBackgroundThread: false
                        );
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to send email notification for IdThongBao: " + notification.IdThongBao);
                }
            });
        }
        /// <summary>
        /// Tạo thông báo và gửi Email chuyên biệt cho sự kiện thay đổi Lịch Biểu Chung
        /// </summary>
        public TblThongBao CreateScheduleChangeNotification(
            Guid userId,
            string tieuDe,
            string noiDung,
            Guid idDuAn,
            string projectName,
            string reason,
            string affectedTasksHtml)
        {
            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(tieuDe))
                return null;

            string currentUser = _applicationContext?.UserName ?? "System";

            TblThongBao item = new TblThongBao
            {
                IdThongBao = Guid.NewGuid(),
                UserId = userId,
                IdCongViec = null,
                IdDuAn = idDuAn,
                TieuDe = tieuDe.Length > 255 ? tieuDe.Substring(0, 255) : tieuDe,
                NoiDung = noiDung,
                LoaiThongBao = ThongBaoTypes.CongViec, // Dùng cờ Dự án để hiện icon cái cặp
                DuongDanLienKet = null,
                DaDoc = false,
                NgayDoc = null,
                DaXoa = false,
                NguoiTao = currentUser,
                NgayTao = DateTime.Now,
                NguoiCapNhat = currentUser,
                NgayCapNhat = DateTime.Now
            };

            var result = _repository.Insert(item);
            if (result != null) PhatThayDoiSauLuu(result.UserId);

            if (result != null)
            {
                SendScheduleChangeEmail(result, projectName, reason, affectedTasksHtml);
            }

            return result;
        }

        private void SendScheduleChangeEmail(TblThongBao notification, string projectName, string reason, string affectedTasksHtml)
        {
            Task.Run(async () =>
            {
                try
                {
                    var recipient = UserManager.Instance.GetUserById(notification.UserId);
                    var memUser = System.Web.Security.Membership.GetUser(recipient.UserName);
                    string email = memUser != null ? memUser.Email : null;

                    if (recipient == null || string.IsNullOrWhiteSpace(email)) return;

                    // Chỉ gửi nội dung thuần, không cần xử lý Domain hay URL chuyển hướng
                    var placeholdersBody = new Dictionary<string, string>
                    {
                        { "[[COMPANY_NAME]]", "SweetSoft QLDA" },
                        { "[[FULL_NAME]]", recipient.DisplayName },
                        { "[[PROJECT_NAME]]", projectName },
                        { "[[REASON]]", reason },
                        { "[[AFFECTED_TASKS_HTML]]", affectedTasksHtml }
                    };

                    var emailManager = new EmailManager(SweetContext.CreateBackgroundContext());

                    await emailManager.SendEmailWithTemplateAsync(
                        refId: notification.UserId,
                        refType: EmailType.Notification,
                        customerId: notification.UserId,
                        toEmail: email,
                        templateKey: "TemplateScheduleChange",
                        formatType: EmailFormatTypes.Admin,
                        placeholdersBody: placeholdersBody,
                        attachments: null,
                        useBackgroundThread: false
                    );
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Failed to send schedule change email for IdThongBao: " + notification.IdThongBao);
                }
            });
        }

        #endregion

        #region Query

        /// <summary>
        /// Lấy danh sách thông báo của user, sắp xếp mới nhất trước.
        /// Dùng cho lazy load — mỗi lần lấy <paramref name="pageSize"/> bản ghi.
        /// </summary>
        /// <param name="userId">User cần lấy thông báo</param>
        /// <param name="pageNumber">Hàng bắt đầu (tính theo ROW_NUMBER, bắt đầu từ 1)</param>
        /// <param name="pageSize">Hàng kết thúc (= pageNumber + số item mỗi trang - 1)</param>
        /// <param name="totalRecord">Tổng số thông báo (dùng để biết còn item hay không)</param>
        public DataTable GetByUser(Guid userId, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.GetByUser(userId, pageNumber, pageSize, out totalRecord);
        }

        /// <summary>
        /// Đếm số thông báo chưa đọc — dùng cho badge icon chuông.
        /// </summary>
        public int CountUnread(Guid userId)
        {
            return _repository.CountUnread(userId);
        }

        #endregion

        #region Mark as read / Delete

        /// <summary>
        /// Đánh dấu một thông báo đã đọc theo Id.
        /// </summary>
        public void MarkAsRead(Guid idThongBao)
        {
            TblThongBao item = _repository.GetById(idThongBao);
            if (item == null || item.DaXoa == true) return;
            _repository.MarkAsRead(idThongBao);
            PhatThayDoiSauLuu(item.UserId);
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo chưa đọc của user là đã đọc.
        /// </summary>
        public void MarkAllAsRead(Guid userId)
        {
            _repository.MarkAllAsRead(userId);
            PhatThayDoiSauLuu(userId);
        }

        /// <summary>
        /// Xóa mềm một thông báo.
        /// </summary>
        public bool Delete(Guid idThongBao)
        {
            TblThongBao item = _repository.GetById(idThongBao);

            if (item == null)
                return false;

            string currentUser = _applicationContext?.UserName ?? "System";
            item.NguoiCapNhat = currentUser;

            bool deleted = _repository.Delete(item);
            if (deleted) PhatThayDoiSauLuu(item.UserId);
            return deleted;
        }

        #endregion
        // =========================================================================
        // LUỒNG 1: GỬI THÔNG BÁO & EMAIL NHẮC LỊCH HỌP (DÙNG CHO HANGFIRE)
        // =========================================================================
        public TblThongBao CreateMeetingReminderNotification(
            Guid userId, string tieuDe, string noiDung, Guid? idDuAn,
            string meetingName, DateTime startTime, string room, bool isOnline = false)
        {
            if (userId == Guid.Empty) return null;
            string currentUser = _applicationContext?.UserName ?? "System";

            TblThongBao item = new TblThongBao
            {
                IdThongBao = Guid.NewGuid(),
                UserId = userId,
                IdCongViec = null,
                IdDuAn = idDuAn,
                TieuDe = tieuDe.Length > 255 ? tieuDe.Substring(0, 255) : tieuDe,
                NoiDung = noiDung,
                LoaiThongBao = ThongBaoTypes.LichHop,
                DuongDanLienKet = null,
                DaDoc = false,
                NgayDoc = null,
                DaXoa = false,
                NguoiTao = currentUser,
                NgayTao = DateTime.Now,
                NguoiCapNhat = currentUser,
                NgayCapNhat = DateTime.Now
            };

            var result = _repository.Insert(item);
            if (result != null) PhatThayDoiSauLuu(result.UserId);
            if (result != null)
                SendMeetingReminderEmail(result, meetingName, startTime, room, isOnline);
            return result;
        }

        private static string GetSafeMeetingLink(string room, bool isOnline)
        {
            if (!isOnline || string.IsNullOrWhiteSpace(room)) return null;
            Uri uri;
            if (!Uri.TryCreate(room.Trim(), UriKind.Absolute, out uri)) return null;
            if (uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrWhiteSpace(uri.Host) ||
                !string.IsNullOrEmpty(uri.UserInfo) || uri.AbsoluteUri.Length > 2048) return null;
            return uri.AbsoluteUri;
        }

        private void SendMeetingReminderEmail(TblThongBao notification, string meetingName, DateTime startTime, string room, bool isOnline)
        {
            Task.Run(async () =>
            {
                try
                {
                    var recipient = UserManager.Instance.GetUserById(notification.UserId);
                    if (recipient == null) return;
                    var memUser = System.Web.Security.Membership.GetUser(recipient.UserName);
                    string email = memUser != null ? memUser.Email : null;
                    if (string.IsNullOrWhiteSpace(email)) return;

                    string joinUrl = GetSafeMeetingLink(room, isOnline);
                    string encodedRoom = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(room) ? "Chưa cung cấp" : room);
                    string joinSection = string.Empty;
                    if (!string.IsNullOrEmpty(joinUrl))
                    {
                        string link = HttpUtility.HtmlAttributeEncode(joinUrl);
                        joinSection = "<div style=\"margin:24px 0;text-align:center;\">" +
                            "<a href=\"" + link + "\" target=\"_blank\" rel=\"noopener noreferrer\" " +
                            "style=\"display:inline-block;padding:13px 22px;border-radius:8px;background:#54218d;" +
                            "color:#ffffff;text-decoration:none;font-size:15px;font-weight:bold;\">" +
                            "Tham gia cuộc họp</a>" +
                            "<div style=\"margin-top:9px;font-size:12px;color:#64748b;overflow-wrap:anywhere;\">" +
                            "Nếu nút không hoạt động, mở liên kết: <a href=\"" + link + "\" " +
                            "style=\"color:#54218d;\">" + HttpUtility.HtmlEncode(joinUrl) + "</a></div></div>";
                    }
                    else if (isOnline)
                    {
                        joinSection = "<p style=\"margin:14px 0;color:#a16207;font-size:13px;\">" +
                            "Liên kết họp trực tuyến chưa hợp lệ. Vui lòng liên hệ người chủ trì.</p>";
                    }

                    var placeholdersBody = new Dictionary<string, string>
                    {
                        { "[[COMPANY_NAME]]", "SweetSoft QLDA" },
                        { "[[FULL_NAME]]", HttpUtility.HtmlEncode(recipient.DisplayName ?? recipient.UserName) },
                        { "[[MEETING_NAME]]", HttpUtility.HtmlEncode(meetingName ?? string.Empty) },
                        { "[[START_TIME]]", startTime.ToString("HH:mm dd/MM/yyyy") },
                        { "[[MEETING_TYPE]]", isOnline ? "Trực tuyến (Online)" : "Trực tiếp (Offline)" },
                        { "[[ROOM]]", encodedRoom },
                        { "[[MEETING_JOIN_SECTION]]", joinSection }
                    };

                    var emailManager = new EmailManager(SweetContext.CreateBackgroundContext());
                    await emailManager.SendEmailWithTemplateAsync(
                        refId: notification.UserId,
                        refType: EmailType.Notification,
                        customerId: notification.UserId,
                        toEmail: email,
                        templateKey: "TemplateMeetingReminder",
                        formatType: EmailFormatTypes.Admin,
                        placeholdersBody: placeholdersBody,
                        attachments: null,
                        useBackgroundThread: false
                    );
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Lỗi gửi Mail nhắc Lịch Họp IdThongBao: " + notification.IdThongBao);
                }
            });
        }

        // =========================================================================
        // LUỒNG 2: GỬI THÔNG BÁO & EMAIL NHẮC HẠN CHÓT CÔNG VIỆC (DÙNG CHO HANGFIRE)
        // =========================================================================
        public TblThongBao CreateTaskReminderNotification(
            Guid userId, string tieuDe, string noiDung, Guid? idCongViec, Guid? idDuAn,
            string taskCode, string taskName, DateTime deadline, ThongBaoAction? action = null)
        {
            if (userId == Guid.Empty) return null;
            string currentUser = _applicationContext?.UserName ?? "System";

            TblThongBao item = new TblThongBao
            {
                IdThongBao = Guid.NewGuid(),
                UserId = userId,
                IdCongViec = idCongViec,
                IdDuAn = idDuAn,
                TieuDe = tieuDe.Length > 255 ? tieuDe.Substring(0, 255) : tieuDe,
                NoiDung = noiDung,
                LoaiThongBao = ThongBaoTypes.CongViec,
                DuongDanLienKet = action.HasValue ? action.Value.ToString() : null,
                DaDoc = false,
                NgayDoc = null,
                DaXoa = false,
                NguoiTao = currentUser,
                NgayTao = DateTime.Now,
                NguoiCapNhat = currentUser,
                NgayCapNhat = DateTime.Now
            };

            var result = _repository.Insert(item);
            if (result != null) PhatThayDoiSauLuu(result.UserId);
            if (result != null)
            {
                SendTaskReminderEmail(result, taskCode, taskName, deadline);
            }
            return result;
        }

        private void SendTaskReminderEmail(TblThongBao notification, string taskCode, string taskName, DateTime deadline)
        {
            Task.Run(async () =>
            {
                try
                {
                    var recipient = UserManager.Instance.GetUserById(notification.UserId);
                    var memUser = System.Web.Security.Membership.GetUser(recipient.UserName);
                    string email = memUser != null ? memUser.Email : null;

                    if (recipient == null || string.IsNullOrWhiteSpace(email)) return;

                    var placeholdersBody = new Dictionary<string, string>
                    {
                        { "[[COMPANY_NAME]]", "SweetSoft QLDA" },
                        { "[[FULL_NAME]]", recipient.DisplayName },
                        { "[[TASK_CODE]]", taskCode },
                        { "[[TASK_NAME]]", taskName },
                        { "[[DEADLINE]]", deadline.ToString("dd/MM/yyyy") }
                    };

                    var emailManager = new EmailManager(SweetContext.CreateBackgroundContext());
                    await emailManager.SendEmailWithTemplateAsync(
                        refId: notification.UserId,
                        refType: EmailType.Notification,
                        customerId: notification.UserId,
                        toEmail: email,
                        templateKey: "TemplateTaskReminder", // Cần cấu hình template này
                        formatType: EmailFormatTypes.Admin,
                        placeholdersBody: placeholdersBody,
                        attachments: null,
                        useBackgroundThread: false
                    );
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(ex, "Lỗi gửi Mail nhắc Công Việc IdThongBao: " + notification.IdThongBao);
                }
            });
        }
    }
}
