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
            _repository   = new ThongBaoRepository(_auditManager);
        }

        #endregion

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
            Guid   userId,
            string tieuDe,
            string noiDung          = null,
            string loaiThongBao     = ThongBaoTypes.HeThong,
            Guid?  idCongViec       = null,
            Guid?  idDuAn           = null)
        {
            if (userId == Guid.Empty)
                return null;

            if (string.IsNullOrWhiteSpace(tieuDe))
                return null;

            string currentUser = _applicationContext?.UserName ?? "System";

            TblThongBao item = new TblThongBao
            {
                IdThongBao      = Guid.NewGuid(),
                UserId          = userId,
                IdCongViec      = idCongViec,
                IdDuAn          = idDuAn,
                TieuDe          = tieuDe.Length > 255 ? tieuDe.Substring(0, 255) : tieuDe,
                NoiDung         = noiDung,
                LoaiThongBao    = loaiThongBao,
                DuongDanLienKet = null, // Không lưu cứng URL nữa
                DaDoc           = false,
                NgayDoc         = null,
                DaXoa           = false,
                NguoiTao        = currentUser,
                NgayTao         = DateTime.Now,
                NguoiCapNhat    = currentUser,
                NgayCapNhat     = DateTime.Now
            };

            var result = _repository.Insert(item);

            if (result != null)
            {
                // Gửi Email Notification trong background
                SendEmailNotification(result);
            }

            return result;
        }

        private void SendEmailNotification(TblThongBao notification)
        {
            Task.Run(async () =>
            {
                try
                {
                    var recipient = UserManager.Instance.GetUserById(notification.UserId);
                    var memUser = System.Web.Security.Membership.GetUser(recipient.UserName);
                    string email = memUser != null ? memUser.Email : null;
                    if (recipient == null ||
                        string.IsNullOrWhiteSpace(email))
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
            _repository.MarkAsRead(idThongBao);
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo chưa đọc của user là đã đọc.
        /// </summary>
        public void MarkAllAsRead(Guid userId)
        {
            _repository.MarkAllAsRead(userId);
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

            return _repository.Delete(item);
        }

        #endregion
    }
}
