using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class IssueManager:BaseManager
    {
        private static readonly Lazy<IssueManager> _instance = new Lazy<IssueManager>(() => new IssueManager());
        public static IssueManager Instance => _instance.Value;
        private readonly IssueRepository _repository;
        private readonly AuditManager _auditManager;

        public IssueManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new IssueRepository(_auditManager);
        }

        public DataTable SearchIssue(Guid projectId, string searchTerm, Dictionary<string, object> parameters, string orderBy, int startRow, int endRow, out int totalRecord)
        {
            return _repository.SearchIssue(projectId, searchTerm, parameters, orderBy, startRow, endRow, out totalRecord);
        }
        public void DeleteIssue(TblVanDe issue)
        {
            if (issue == null || issue.IdVanDe == Guid.Empty || issue.IdDuAn == Guid.Empty)
                throw new InvalidOperationException("Vấn đề không hợp lệ.");
            // Giu audit va soft-delete cua partner. Chi them guard tranh xoa
            // Issue co du lieu gio; dung khoa du an chung voi Start/Stop.
            var gioRepo = new GioXuLyVanDeRepository();
            using (SqlConnection con = gioRepo.MoKetNoi())
            using (SqlTransaction tx = con.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                gioRepo.KhoaDuAn(con, tx, issue.IdDuAn);
                using (var cmd = new SqlCommand(@"SELECT CASE WHEN EXISTS(
                    SELECT 1 FROM dbo.TblKhaiBaoGioPhien
                    WHERE IdDuAn=@D AND IdVanDe=@V AND LoaiGio=3
                    ) OR EXISTS(SELECT 1 FROM dbo.TblVanDe
                       WHERE IdDuAn=@D AND IdVanDe=@V AND TrangThaiDuyetGio=1)
                    THEN 1 ELSE 0 END", con, tx))
                {
                    cmd.Parameters.Add("@D", SqlDbType.UniqueIdentifier).Value = issue.IdDuAn;
                    cmd.Parameters.Add("@V", SqlDbType.UniqueIdentifier).Value = issue.IdVanDe;
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                        throw new InvalidOperationException("Vấn đề đã có phiên giờ hoặc đang chờ duyệt, không thể xóa. Vui lòng xử lý lịch sử giờ trước.");
                }
                _repository.DeleteIssue(issue);
                tx.Commit();
            }
        }
        public string GetValueForMucDoAnhHuong(MucDoAnhHuonEnum impact)
        {
            switch (impact)
            {
                case MucDoAnhHuonEnum.VeryLow: return "Rất thấp";
                case MucDoAnhHuonEnum.Low: return "Thấp";
                case MucDoAnhHuonEnum.Medium: return "Trung bình";
                case MucDoAnhHuonEnum.High: return "Cao";
                case MucDoAnhHuonEnum.VeryHigh: return "Rất cao";
                default: return "—";
            }
        }
        public string GetValueForTrangThaiVanDe(TrangThaiVanDeEnum score)
        {
            switch (score)
            {
                case TrangThaiVanDeEnum.Processing:
                    return "PROCESSING";
                case TrangThaiVanDeEnum.Processed:
                    return "PROCESSED";
                default:
                    return score.ToString();
            }
        }
        public string GetValueForNguonGocVanDe(NguonGocVanDeEnum source)
        {
            switch (source)
            {
                case NguonGocVanDeEnum.Other:
                    return "OTHER";
                case NguonGocVanDeEnum.TaskIssue:
                    return "TASK_ISSUE";
                case NguonGocVanDeEnum.CustomerFeedback:
                    return "CUSTOMER_FEEDBACK";
                default:
                    return source.ToString();
            }
        }
        public void SyncNhanVienXuLyVanDe(Guid idVanDe, Guid idCongViec)
        {
            _repository.SyncNhanVienXuLyVanDe(idVanDe, idCongViec);
        }

        public string GenerateMaVanDe(Guid projectId)
        {
            return _repository.GenerateMaVanDe(projectId);
        }
        public TblVanDe CreateOrUpdate(TblVanDe dto)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenVanDe, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenVanDe));
            BusinessValidator.ThrowIf(dto.IdDuAn == Guid.Empty || dto.IdDuAn == null, BackEndResourceKeys.INVALID_DATA, nameof(dto.IdDuAn));

            string currentUser = SweetContext.Current != null ? SweetContext.Current.UserName : "System";
            bool isInsert = (dto.IdVanDe == Guid.Empty);
            TblVanDe result = null;

            if (isInsert)
            {
                dto.IdVanDe = Guid.NewGuid();
                dto.DaXoa = false;
                dto.TrangThai = 0;

                dto.MaVanDe = GenerateMaVanDe(dto.IdDuAn);

                dto.NgayTao = DateTime.Now;
                dto.NguoiTao = currentUser;
                dto.NgayCapNhat = DateTime.Now;
                dto.NguoiCapNhat = currentUser;

                dto.Save();
                result = dto;
            }
            else
            {
                TblVanDe existingIssue = TblVanDe.FetchByID(dto.IdVanDe);
                BusinessValidator.ThrowIfNull(existingIssue, BackEndResourceKeys.NOT_FOUND, nameof(dto.IdVanDe), ErrorCodes.NotFound);

                existingIssue.TenVanDe = dto.TenVanDe;
                existingIssue.MoTaChiTiet = dto.MoTaChiTiet;
                existingIssue.KeHoachXuLy = dto.KeHoachXuLy;
                existingIssue.IdCongViecBiAnhHuong = dto.IdCongViecBiAnhHuong;
                existingIssue.IdCongViecPhatSinh = dto.IdCongViecPhatSinh;
                existingIssue.MucDoAnhHuong = dto.MucDoAnhHuong;
                existingIssue.NguonGocVanDe = dto.NguonGocVanDe;
                // TrangThai chi thay doi thong qua GioXuLyVanDeManager.XuLyYeuCau sau PM duyet.
                // Sua thong tin Issue khong duoc bo qua luong gui duyet / dong gio nhom.

                existingIssue.NgayCapNhat = DateTime.Now;
                existingIssue.NguoiCapNhat = currentUser;

                existingIssue.Save();
                result = existingIssue;
            }

            if (result != null)
            {
                if (result.IdCongViecPhatSinh.HasValue)
                {
                    SyncNhanVienXuLyVanDe(result.IdVanDe, result.IdCongViecBiAnhHuong.Value);
                }
            }

            return result;
        }
    }
}
