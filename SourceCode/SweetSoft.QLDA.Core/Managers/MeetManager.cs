using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using SubSonic;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SweetSoft.QLDA.Core.Managers
{
    /// <summary>
    /// The canonical project document attached to a meeting.
    /// </summary>
    public sealed class MeetingDocumentLinkResult
    {
        public Guid ProjectId { get; set; }
        public Guid DocumentId { get; set; }
        public bool IsCreated { get; set; }
    }

    public class MeetManager : BaseManager
    {
        private static readonly Lazy<MeetManager> _instance = new Lazy<MeetManager>(() => new MeetManager());
        public static MeetManager Instance => _instance.Value;
        private readonly MeetRepository _repository;
        private readonly AuditManager _auditManager;

        public MeetManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new MeetRepository(_auditManager);
        }
        public DataTable SearchMeeting(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            return _repository.SearchMeeting(projectId, searchTerm, parameters, orderBy, startRow, endRow, out totalRecord);
        }
        public string GenerateMaCuocHop(Guid projectId)
        {
            return _repository.GenerateMaCuocHop(projectId);
        }

        public byte GetMeetStatus(DateTime? dtStart, DateTime? dtEnd)
        {
            if (dtStart.HasValue && dtEnd.HasValue)
            {
                DateTime now = DateTime.Now;

                if (now > dtEnd.Value)
                {
                    return (byte)TrangThaiCuocHopEnum.Ended;
                }
                else if (now >= dtStart.Value && now <= dtEnd.Value)
                {
                    return (byte)TrangThaiCuocHopEnum.Ongoing;
                }
                else if ((dtStart.Value - now).TotalMinutes > 0 && (dtStart.Value - now).TotalMinutes <= 15)
                {
                    return (byte)TrangThaiCuocHopEnum.Upcoming;
                }
                else
                {
                    return (byte)TrangThaiCuocHopEnum.Scheduled;
                }
            }

            return (byte)TrangThaiCuocHopEnum.Scheduled;
        }
        public string GetValueForTrangThaiCuoHop(object status)
        {
            switch (status)
            {
                case TrangThaiCuocHopEnum.Scheduled:
                    return "SCHEDULED";
                case TrangThaiCuocHopEnum.Upcoming:
                    return "UPCOMING";
                case TrangThaiCuocHopEnum.Ongoing:
                    return "ONGOING";
                case TrangThaiCuocHopEnum.Ended:
                    return "ENDED";
                default:
                    return status.ToString();
            }
        }
        public void UpdateTblLichHopNhanVien(Guid idLichHop, string lstNhanVienIds)
        {
            _repository.UpdateTblLichHopNhanVien(idLichHop, lstNhanVienIds);
        }
        public DataTable GetNhanVienThamGia(Guid idLichHop)
        {
            return _repository.GetNhanVienThamGia(idLichHop);
        }
        public TblLichHop CreateOrUpdate(TblLichHop dto, string nhanVienIds)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenCuocHop, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenCuocHop));
            BusinessValidator.ThrowIf(dto.IdDuAn == Guid.Empty || dto.IdDuAn == null, BackEndResourceKeys.INVALID_DATA, nameof(dto.IdDuAn));

            Guid currentUserId = SweetContext.Current != null ? SweetContext.Current.UserId : Guid.Empty;
            bool isInsert = (dto.IdLichHop == Guid.Empty);
            TblLichHop result = null;

            int trangThai = GetMeetStatus(dto.ThoiGianBatDau, dto.ThoiGianKetThuc);

            if (isInsert)
            {
                dto.IdLichHop = Guid.NewGuid();
                dto.DaXoa = false;
                dto.TrangThai = (byte)trangThai;

                dto.MaCuocHop = GenerateMaCuocHop(dto.IdDuAn);

                dto.NgayTao = DateTime.Now;
                dto.IdNguoiTao = currentUserId;

                dto.Save();
                result = dto;
            }
            else
            {
                TblLichHop existingMeet = TblLichHop.FetchByID(dto.IdLichHop);
                BusinessValidator.ThrowIfNull(existingMeet, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdLichHop), ErrorCodes.NotFound);

                existingMeet.TenCuocHop = dto.TenCuocHop;
                existingMeet.NoiDungCuocHop = dto.NoiDungCuocHop;
                existingMeet.DiaDiemHop = dto.DiaDiemHop;
                existingMeet.ThoiGianBatDau = dto.ThoiGianBatDau;
                existingMeet.ThoiGianKetThuc = dto.ThoiGianKetThuc;
                existingMeet.TrangThai = (byte)trangThai;

                existingMeet.NgayCapNhat = DateTime.Now;
                existingMeet.IdNguoiCapNhat = currentUserId;

                existingMeet.Save();
                result = existingMeet;
            }

            if (result != null)
            {
                UpdateTblLichHopNhanVien(result.IdLichHop, nhanVienIds);
            }

            return result;
        }

        #region Meeting document

        /// <summary>
        /// Creates one canonical project document for a meeting and returns
        /// its ids so the caller can open the document version screen. The
        /// meeting title is a snapshot at creation time; later scheduling
        /// edits do not silently overwrite document metadata.
        /// </summary>
        public MeetingDocumentLinkResult GetOrCreateProjectDocument(
            Guid idLichHop)
        {
            if (idLichHop == Guid.Empty)
            {
                throw new ArgumentException(
                    "Lịch họp không hợp lệ.",
                    nameof(idLichHop));
            }

            if (!_repository.HasDocumentLinkColumn())
            {
                throw new InvalidOperationException(
                    "Chưa cài cấu trúc liên kết hồ sơ cho lịch họp. Hãy chạy Database/AddMeetingDocumentLink.sql trước.");
            }

            TransactionOptions options = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.Serializable
            };

            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                options))
            {
                TblLichHop meeting = _repository.GetById(idLichHop);
                if (meeting == null)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy lịch họp hoặc lịch họp đã bị xóa.");
                }

                TblDuAn project = DuAnManager.Instance.GetDuAnById(
                    meeting.IdDuAn);
                if (project == null)
                {
                    throw new InvalidOperationException(
                        "Dự án của lịch họp không tồn tại hoặc đã bị xóa.");
                }

                if (!DocumentManager.Instance.CanAccessProjectDocument(
                        project.IdDuAn,
                        ActionKeys.View))
                {
                    throw new UnauthorizedAccessException(
                        "Bạn không có quyền xem hồ sơ của dự án này.");
                }

                Guid? linkedDocumentId = _repository
                    .GetLinkedDocumentIdForUpdate(meeting.IdLichHop);
                if (linkedDocumentId.HasValue)
                {
                    TblTaiLieu existingDocument = DocumentManager.Instance
                        .GetProjectDocumentById(
                            linkedDocumentId.Value,
                            project.IdDuAn);
                    if (existingDocument == null)
                    {
                        throw new InvalidOperationException(
                            "Hồ sơ liên kết với lịch họp không còn thuộc dự án hiện tại. Vui lòng kiểm tra lại dữ liệu liên kết.");
                    }

                    scope.Complete();
                    return new MeetingDocumentLinkResult
                    {
                        ProjectId = project.IdDuAn,
                        DocumentId = existingDocument.IdTaiLieu,
                        IsCreated = false
                    };
                }

                if (!DocumentManager.Instance.CanAccessProjectDocument(
                        project.IdDuAn,
                        ActionKeys.Create))
                {
                    throw new UnauthorizedAccessException(
                        "Bạn không có quyền tạo hồ sơ cho dự án này.");
                }

                TblLoaiTaiLieu documentType = GetConfiguredMeetingDocumentType();
                TblTaiLieu document = DocumentManager.Instance.SaveProjectDocument(
                    project.IdDuAn,
                    Guid.Empty,
                    documentType.IdLoaiTaiLieu,
                    project.IdNhanVienQuanLy,
                    BuildMeetingDocumentCode(meeting),
                    BuildMeetingDocumentTitle(meeting),
                    BuildMeetingDocumentDescription(meeting),
                    documentType.CanTrinhKy,
                    documentType.CanTrinhKy
                        ? documentType.HinhThucKyMacDinh
                        : null,
                    documentType.CanGuiKhachHang,
                    documentType.CanLuuVatLy);

                bool linked = _repository.TryLinkDocument(
                    meeting.IdLichHop,
                    document.IdTaiLieu,
                    SweetContext.Current.UserId,
                    DateTime.UtcNow);
                if (!linked)
                {
                    throw new InvalidOperationException(
                        "Không thể liên kết hồ sơ vừa tạo với lịch họp. Dữ liệu đã được hoàn tác.");
                }

                WriteMeetingDocumentLinkAudit(meeting, project, document);
                scope.Complete();

                return new MeetingDocumentLinkResult
                {
                    ProjectId = project.IdDuAn,
                    DocumentId = document.IdTaiLieu,
                    IsCreated = true
                };
            }
        }

        private TblLoaiTaiLieu GetConfiguredMeetingDocumentType()
        {
            Guid idLoaiTaiLieu = SettingManager.Instance.GetSettingValueGuid(
                SettingKeys.MeetingDocumentTypeId);
            if (idLoaiTaiLieu == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình loại tài liệu Lịch họp. Vui lòng cấu hình Settings.MeetingDocumentTypeId trước.");
            }

            TblLoaiTaiLieu documentType = DocumentManager.Instance
                .GetDocumentTypeDefaults(idLoaiTaiLieu);
            if (documentType == null || documentType.DaXoa || !documentType.KichHoat)
            {
                throw new InvalidOperationException(
                    "Loại tài liệu Lịch họp được cấu hình không tồn tại hoặc đang bị khóa.");
            }

            return documentType;
        }

        private static string BuildMeetingDocumentCode(TblLichHop meeting)
        {
            const int maxLength = 100;
            string meetingCode = (meeting.MaCuocHop ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
            if (string.IsNullOrEmpty(meetingCode))
            {
                meetingCode = "MEETING";
            }

            string suffix = "-" + meeting.IdLichHop
                .ToString("N")
                .Substring(0, 8)
                .ToUpperInvariant();
            int meetingCodeLength = maxLength - "LH-".Length - suffix.Length;
            if (meetingCode.Length > meetingCodeLength)
            {
                meetingCode = meetingCode
                    .Substring(0, meetingCodeLength)
                    .TrimEnd();
            }

            return "LH-" + meetingCode + suffix;
        }

        private static string BuildMeetingDocumentTitle(TblLichHop meeting)
        {
            const int maxLength = 255;
            const string prefix = "Cuộc họp ";
            const string separator = " – ";
            string meetingCode = (meeting.MaCuocHop ?? string.Empty).Trim();
            string meetingName = (meeting.TenCuocHop ?? string.Empty).Trim();
            int meetingNameLength = maxLength
                - prefix.Length
                - meetingCode.Length
                - separator.Length;

            if (meetingNameLength <= 0)
            {
                return (prefix + meetingCode).Substring(0, maxLength);
            }

            if (meetingName.Length > meetingNameLength)
            {
                meetingName = meetingName
                    .Substring(0, meetingNameLength)
                    .TrimEnd();
            }

            return prefix + meetingCode + separator + meetingName;
        }

        private static string BuildMeetingDocumentDescription(TblLichHop meeting)
        {
            return "Hồ sơ cuộc họp được tạo tự động từ cuộc họp "
                + (meeting.MaCuocHop ?? string.Empty).Trim()
                + ".";
        }

        private void WriteMeetingDocumentLinkAudit(
            TblLichHop meeting,
            TblDuAn project,
            TblTaiLieu document)
        {
            try
            {
                var auditEntry = new
                {
                    IdTaiLieu = document.IdTaiLieu,
                    IdDuAn = project.IdDuAn,
                    MaTaiLieu = document.MaTaiLieu,
                    TenTaiLieu = document.TenTaiLieu
                };

                _auditManager.LogActionAsync(
                        LogActions.Actions.UPDATE,
                        auditEntry,
                        nameof(TblLichHop),
                        meeting.IdLichHop,
                        SweetContext.Current.UserName,
                        document.IdTaiLieu,
                        meeting.MaCuocHop,
                        "Đã tạo và liên kết hồ sơ cuộc họp "
                            + document.MaTaiLieu
                            + " với dự án "
                            + project.MaDuAn
                            + ".")
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exception)
            {
                SysLogger.LogError(
                    exception,
                    "Failed to log meeting-document link for "
                        + meeting.IdLichHop);
            }
        }

        #endregion

        public void DeleteMeet(TblLichHop meet)
        {
            _repository.DeleteMeet(meet);
        }
        public List<Guid> GetNhanVienCuocHop(Guid idCuocHop)
        {
            return _repository.GetNhanVienCuocHop(idCuocHop);
        }
    }
}
