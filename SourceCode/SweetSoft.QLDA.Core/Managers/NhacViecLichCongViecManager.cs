using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;

namespace SweetSoft.QLDA.Core.Managers
{
    /// <summary>
    /// Manager xử lý reminder theo từng công việc khi Lịch Biểu Chung thay đổi.
    /// </summary>
    public class NhacViecLichCongViecManager : BaseManager
    {
        private static readonly Lazy<NhacViecLichCongViecManager> _instance =
            new Lazy<NhacViecLichCongViecManager>(() => new NhacViecLichCongViecManager());

        public static NhacViecLichCongViecManager Instance => _instance.Value;

        private readonly NhacViecLichCongViecRepository _repository;
        private readonly AuditManager _auditManager;

        public NhacViecLichCongViecManager(IAppContext applicationContext = null)
            : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new NhacViecLichCongViecRepository(_auditManager);
        }

        #region Create

        /// <summary>
        /// Tạo một reminder cho một task.
        /// </summary>
        public TblNhacViecLichCongViec Create(
            Guid idThongBao,
            Guid idDuAn,
            Guid idCongViec,
            Guid userId,
            string loaiThayDoi,
            string noiDung,
            Guid? idCauHinh = null,
            Guid? idNgoaiLe = null,
            DateTime? ngayHieuLucTu = null,
            DateTime? ngayHieuLucDen = null)
        {
            if (idThongBao == Guid.Empty ||
                idDuAn == Guid.Empty ||
                idCongViec == Guid.Empty ||
                userId == Guid.Empty ||
                string.IsNullOrWhiteSpace(loaiThayDoi) ||
                string.IsNullOrWhiteSpace(noiDung))
            {
                return null;
            }

            string currentUser = _applicationContext?.UserName ?? "System";

            TblNhacViecLichCongViec item = new TblNhacViecLichCongViec
            {
                IdNhacViec = Guid.NewGuid(),
                IdThongBao = idThongBao,
                IdDuAn = idDuAn,
                IdCongViec = idCongViec,
                UserId = userId,
                LoaiThayDoi = loaiThayDoi,
                IdCauHinh = idCauHinh,
                IdNgoaiLe = idNgoaiLe,
                NoiDung = noiDung,
                NgayHieuLucTu = ngayHieuLucTu,
                NgayHieuLucDen = ngayHieuLucDen,
                TrangThai = NhacViecLichCongViecStatus.Pending,
                NgayXuLy = null,
                UserIdXuLy = null,
                NguoiTao = currentUser,
                NgayTao = DateTime.Now,
                NguoiCapNhat = currentUser,
                NgayCapNhat = DateTime.Now
            };

            return _repository.Insert(item);
        }

        /// <summary>
        /// Tạo nhiều reminder trong cùng một transaction.
        /// Một event thay đổi lịch thường sẽ sinh nhiều task reminder.
        /// </summary>
        public List<TblNhacViecLichCongViec> CreateRange(
            IEnumerable<TblNhacViecLichCongViec> items)
        {
            List<TblNhacViecLichCongViec> source = items == null
                ? new List<TblNhacViecLichCongViec>()
                : items.Where(x => x != null).ToList();

            if (source.Count == 0)
                return new List<TblNhacViecLichCongViec>();

            string currentUser = _applicationContext?.UserName ?? "System";
            List<TblNhacViecLichCongViec> result = new List<TblNhacViecLichCongViec>();

            using (var scope = new TransactionScope())
            {
                foreach (TblNhacViecLichCongViec item in source)
                {
                    if (item.IdNhacViec == Guid.Empty)
                        item.IdNhacViec = Guid.NewGuid();

                    if (item.TrangThai != NhacViecLichCongViecStatus.Pending)
                        item.TrangThai = NhacViecLichCongViecStatus.Pending;

                    if (item.NgayTao == default(DateTime))
                        item.NgayTao = DateTime.Now;

                    if (string.IsNullOrWhiteSpace(item.NguoiTao))
                        item.NguoiTao = currentUser;

                    item.NguoiCapNhat = currentUser;
                    item.NgayCapNhat = DateTime.Now;

                    TblNhacViecLichCongViec inserted = _repository.Insert(item);
                    if (inserted == null)
                        throw new InvalidOperationException("Không thể tạo reminder cho công việc.");

                    result.Add(inserted);
                }

                scope.Complete();
            }

            return result;
        }

        #endregion

        #region Query

        public TblNhacViecLichCongViec GetById(Guid id)
        {
            return _repository.GetById(id);
        }

        public List<TblNhacViecLichCongViec> GetPendingByProject(Guid idDuAn, Guid userId)
        {
            return _repository.GetPendingByProject(idDuAn, userId);
        }

        public List<TblNhacViecLichCongViec> GetPendingByTask(Guid idCongViec, Guid userId)
        {
            return _repository.GetPendingByTask(idCongViec, userId);
        }

        public DataTable GetPendingTaskSummary(Guid idDuAn, Guid userId)
        {
            return _repository.GetPendingTaskSummary(idDuAn, userId);
        }

        public List<TblNhacViecLichCongViec> GetPendingByNotification(Guid idThongBao, Guid userId)
        {
            return _repository.GetPendingByNotification(idThongBao, userId);
        }

        #endregion

        #region Process reminder

        /// <summary>
        /// Xử lý đúng các reminder mà PM đã tick.
        /// Không tự xử lý các reminder khác của task.
        /// </summary>
        public int MarkAsProcessed(IEnumerable<Guid> reminderIds)
        {
            Guid currentUserId = SweetContext.Current != null
                ? SweetContext.Current.UserId
                : Guid.Empty;

            if (currentUserId == Guid.Empty || reminderIds == null)
                return 0;

            string currentUser = _applicationContext?.UserName ?? "System";

            using (var scope = new TransactionScope())
            {
                int affectedRows = _repository.MarkAsProcessed(
                    reminderIds,
                    currentUserId,
                    currentUser);

                scope.Complete();
                return affectedRows;
            }
        }

        /// <summary>
        /// Đánh dấu reminder cũ là Superseded khi có thay đổi lịch mới thay thế.
        /// </summary>
        public int MarkAsSuperseded(IEnumerable<Guid> reminderIds)
        {
            if (reminderIds == null)
                return 0;

            string currentUser = _applicationContext?.UserName ?? "System";

            using (var scope = new TransactionScope())
            {
                int affectedRows = _repository.MarkAsSuperseded(
                    reminderIds,
                    currentUser);

                scope.Complete();
                return affectedRows;
            }
        }

        #endregion
    }
}
