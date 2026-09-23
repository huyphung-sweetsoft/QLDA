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
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Data;

namespace SweetSoft.QLDA.Core.Managers
{
    public class ThanhToanManager : BaseManager
    {
        private static readonly Lazy<ThanhToanManager> _instance =
            new Lazy<ThanhToanManager>(() => new ThanhToanManager());
        public static ThanhToanManager Instance => _instance.Value;
        private readonly ThanhToanRepository _repository;

        public ThanhToanManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _repository = new ThanhToanRepository(new AuditManager(GetClientInfo()));
        }

        public bool CanAccessProject(Guid projectId, ActionKeys action)
        {
            Guid userId = SweetContext.Current.UserId;
            if (userId == Guid.Empty || projectId == Guid.Empty)
                return false;
            TblDuAn project = DuAnManager.Instance.GetDuAnById(projectId);
            if (project == null)
                return false;
            if (!FunctionManager.Instance.IsActionKeyExisted(userId, ModuleKeys.Payment, action))
                return false;
            if (UserManager.Instance.IsAdministrator(userId) || project.IdNhanVienQuanLy == userId)
                return true;
            return new Select().From(TblThanhVienDuAn.Schema)
                .Where(TblThanhVienDuAn.IdDuAnColumn).IsEqualTo(projectId)
                .And(TblThanhVienDuAn.IdNhanVienColumn).IsEqualTo(userId)
                .And(TblThanhVienDuAn.DaXoaColumn).IsEqualTo(false)
                .GetRecordCount() > 0;
        }

        private void RequireAccess(Guid projectId, ActionKeys action)
        {
            if (!CanAccessProject(projectId, action))
                throw new UnauthorizedAccessException();
        }

        public DataTable SearchThanhToans(Guid projectId, string keyword, string sortColumn,
            string sortDirection, int startRow, int endRow, out int totalRecord)
        {
            RequireAccess(projectId, ActionKeys.View);
            return _repository.SearchPaging(projectId, keyword, sortColumn, sortDirection,
                startRow, endRow, out totalRecord);
        }

        public TblThanhToan GetByProject(Guid id, Guid projectId)
        {
            RequireAccess(projectId, ActionKeys.View);
            return _repository.GetByProject(id, projectId);
        }

        public string GetNextPaymentCode(Guid projectId)
        {
            TblDuAn project = DuAnManager.Instance.GetDuAnById(projectId);
            BusinessValidator.ThrowIfNull(project, BackEndResourceKeys.NOT_FOUND, nameof(projectId));
            string prefix = BuildPaymentCodePrefix(project.MaDuAn);
            int maxSeq = _repository.GetMaxSequence(projectId, prefix);
            return prefix + (maxSeq + 1).ToString();
        }

        public TblThanhToan CreatePayment(Guid projectId, string paymentCode, string paymentName,
            decimal amount, DateTime dueDate, byte status, DateTime? paidDate, string note)
        {
            RequireAccess(projectId, ActionKeys.Create);
            TblDuAn project = DuAnManager.Instance.GetDuAnById(projectId);
            BusinessValidator.ThrowIfNull(project, BackEndResourceKeys.NOT_FOUND, nameof(projectId));
            paymentCode = NormalizePaymentCode(paymentCode);
            ValidatePaymentCode(paymentCode);
            ValidatePayment(paymentName, amount, status, paidDate, note);
            BusinessValidator.ThrowIf(dueDate == DateTime.MinValue,
                BackEndResourceKeys.INVALID_DATA, nameof(dueDate));
            BusinessValidator.ThrowIf(_repository.GetByCode(paymentCode) != null,
                BackEndResourceKeys.CODE_ALREADY_EXISTS, nameof(paymentCode));

            bool paid = status == (byte)ThanhToanStatus.DaThanhToan;
            var item = new TblThanhToan
            {
                IdThanhToan = UUIDv7.NewGuid(),
                IdDuAn = projectId,
                MaDotThanhToan = paymentCode,
                TenDotThanhToan = paymentName.Trim(),
                SoTien = amount,
                HanThanhToan = dueDate.Date,
                NgayThanhToanThucTe = paid ? paidDate.Value.Date : (DateTime?)null,
                TrangThai = status,
                GhiChu = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                DaXoa = false,
                NguoiTao = SweetContext.Current.UserName,
                NgayTao = DateTime.UtcNow,
                NguoiCapNhat = null,
                NgayCapNhat = null
            };
            return _repository.Insert(item);
        }

        public static string BuildPaymentCodePrefix(string projectCode)
        {
            return (projectCode ?? string.Empty).Trim() + "-TT-";
        }

        public TblThanhToan UpdatePayment(Guid id, Guid projectId, string paymentCode, string paymentName,
            decimal amount, byte status, DateTime? paidDate, string note)
        {
            RequireAccess(projectId, ActionKeys.Update);
            TblThanhToan item = _repository.GetByProject(id, projectId);
            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.NOT_FOUND, nameof(id));
            paymentCode = NormalizePaymentCode(paymentCode);
            ValidatePaymentCode(paymentCode);
            ValidatePayment(paymentName, amount, status, paidDate, note);
            TblThanhToan duplicate = _repository.GetByCode(paymentCode);
            BusinessValidator.ThrowIf(duplicate != null && duplicate.IdThanhToan != item.IdThanhToan,
                BackEndResourceKeys.CODE_ALREADY_EXISTS, nameof(paymentCode));

            bool paid = status == (byte)ThanhToanStatus.DaThanhToan;
            item.MaDotThanhToan = paymentCode;
            item.TenDotThanhToan = paymentName.Trim();
            item.SoTien = amount;
            item.TrangThai = status;
            item.NgayThanhToanThucTe = paid ? paidDate.Value.Date : (DateTime?)null;
            item.GhiChu = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            item.NguoiCapNhat = SweetContext.Current.UserName;
            item.NgayCapNhat = DateTime.UtcNow;
            return _repository.Update(item);
        }

        public TblThanhToan ApprovePayment(Guid id, Guid projectId, DateTime paidDate)
        {
            RequireAccess(projectId, ActionKeys.Update);
            TblThanhToan item = _repository.GetByProject(id, projectId);
            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.NOT_FOUND, nameof(id));
            BusinessValidator.ThrowIf(paidDate.Date > DateTime.Today,
                BackEndResourceKeys.PAYMENT_DATE_REQUIRED, nameof(paidDate));

            if (item.TrangThai == (byte)ThanhToanStatus.DaThanhToan)
                return item;

            item.TrangThai = (byte)ThanhToanStatus.DaThanhToan;
            item.NgayThanhToanThucTe = paidDate.Date;
            item.NguoiCapNhat = SweetContext.Current.UserName;
            item.NgayCapNhat = DateTime.UtcNow;
            return _repository.Update(item);
        }

        public bool DeletePayment(Guid id, Guid projectId)
        {
            RequireAccess(projectId, ActionKeys.Delete);
            TblThanhToan item = _repository.GetByProject(id, projectId);
            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.NOT_FOUND, nameof(id));
            item.DaXoa = true;
            item.NguoiCapNhat = SweetContext.Current.UserName;
            item.NgayCapNhat = DateTime.UtcNow;
            return _repository.Delete(item);
        }

        private static void ValidatePayment(string paymentName, decimal amount, byte status,
            DateTime? paidDate, string note)
        {
            paymentName = (paymentName ?? string.Empty).Trim();
            note = (note ?? string.Empty).Trim();
            BusinessValidator.ThrowIfNullOrEmpty(paymentName,
                BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(paymentName));
            BusinessValidator.ThrowIf(paymentName.Length > 255 || amount <= 0 || note.Length > 1000,
                BackEndResourceKeys.INVALID_DATA, nameof(paymentName));
            BusinessValidator.ThrowIf(
                status != (byte)ThanhToanStatus.ChuaThanhToan
                && status != (byte)ThanhToanStatus.DaThanhToan,
                BackEndResourceKeys.INVALID_DATA, nameof(status));
            bool paid = status == (byte)ThanhToanStatus.DaThanhToan;
            BusinessValidator.ThrowIf(paid && (!paidDate.HasValue || paidDate.Value.Date > DateTime.Today),
                BackEndResourceKeys.PAYMENT_DATE_REQUIRED, nameof(paidDate));
        }

        private static string NormalizePaymentCode(string paymentCode)
        {
            return (paymentCode ?? string.Empty).Trim();
        }

        private static void ValidatePaymentCode(string paymentCode)
        {
            BusinessValidator.ThrowIfNullOrEmpty(paymentCode,
                BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(paymentCode));
            BusinessValidator.ThrowIf(paymentCode.Length > 50,
                BackEndResourceKeys.INVALID_DATA, nameof(paymentCode));
        }

    }
}
