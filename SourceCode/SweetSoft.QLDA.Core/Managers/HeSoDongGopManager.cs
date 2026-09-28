using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Transactions;

namespace SweetSoft.QLDA.Core.Managers
{
    public class HeSoDongGopManager : BaseManager
    {
        private static readonly Lazy<HeSoDongGopManager> _instance =
            new Lazy<HeSoDongGopManager>(
                () => new HeSoDongGopManager()
            );

        public static HeSoDongGopManager Instance =>
            _instance.Value;

        private readonly HeSoDongGopRepository _repository;

        private readonly AuditManager _auditManager;


        public HeSoDongGopManager(
            IAppContext applicationContext = null)
            : base(applicationContext)
        {
            _auditManager =
                new AuditManager(
                    GetClientInfo()
                );

            _repository =
                new HeSoDongGopRepository(
                    _auditManager
                );
        }


        #region NHÓM 1: LẤY CẤU HÌNH HỆ SỐ MẶC ĐỊNH


        public DataTable GetHeSoMacDinh()
        {
            return _repository.GetSystemDefaults();
        }


        public TblHeSoDongGop GetHeSoMacDinh(
            Guid idDoUuTien)
        {
            return _repository.GetSystemCoefficient(
                idDoUuTien
            );
        }


        #endregion


        #region NHÓM 2: LẤY CẤU HÌNH HỆ SỐ CỦA PROJECT


        public DataTable GetHeSoCuaDuAn(
            Guid idDuAn)
        {
            BusinessValidator.ThrowIf(
                idDuAn == Guid.Empty,
                BackEndResourceKeys.INVALID_DATA
            );

            return _repository.GetProjectCoefficients(
                idDuAn
            );
        }


        public TblHeSoDongGop GetHeSoCuaDuAn(
            Guid idDuAn,
            Guid idDoUuTien)
        {
            BusinessValidator.ThrowIf(
                idDuAn == Guid.Empty,
                BackEndResourceKeys.INVALID_DATA
            );

            BusinessValidator.ThrowIf(
                idDoUuTien == Guid.Empty,
                BackEndResourceKeys.INVALID_DATA
            );

            return _repository.GetProjectCoefficient(
                idDuAn,
                idDoUuTien
            );
        }


        #endregion


        #region NHÓM 3: LƯU HỆ SỐ MẶC ĐỊNH CỦA HỆ THỐNG


        public bool SaveHeSoMacDinh(
            List<TblHeSoDongGop> items)
        {
            BusinessValidator.ThrowIfNull(
                items,
                BackEndResourceKeys.INVALID_DATA
            );

            if (items.Count == 0)
                return false;


            string currentUser =
                SweetContext.Current != null
                    ? SweetContext.Current.UserName
                    : "System";


            using (var scope =
                new TransactionScope())
            {
                foreach (TblHeSoDongGop item
                    in items)
                {
                    BusinessValidator.ThrowIfNull(
                        item,
                        BackEndResourceKeys.INVALID_DATA
                    );

                    BusinessValidator.ThrowIf(
                        item.IdDoUuTien == Guid.Empty,
                        BackEndResourceKeys.INVALID_DATA
                    );

                    BusinessValidator.ThrowIf(
                        item.HeSoDongGop < 0,
                        BackEndResourceKeys.INVALID_DATA
                    );


                    TblHeSoDongGop existingItem =
                        _repository.GetSystemCoefficient(
                            item.IdDoUuTien
                        );


                    if (existingItem == null)
                    {
                        item.IdHeSoDongGop =
                            item.IdHeSoDongGop == Guid.Empty
                                ? Guid.NewGuid()
                                : item.IdHeSoDongGop;

                        item.IdDuAn = null;
                        item.DaXoa = false;

                        item.NguoiTao =
                            currentUser;

                        item.NgayTao =
                            DateTime.Now;

                        item.NguoiCapNhat =
                            null;

                        item.NgayCapNhat =
                            null;


                        _repository.InsertHeSoDongGop(
                            item
                        );
                    }
                    else
                    {
                        existingItem.HeSoDongGop =
                            item.HeSoDongGop;

                        existingItem.DaXoa =
                            false;

                        existingItem.NguoiCapNhat =
                            currentUser;

                        existingItem.NgayCapNhat =
                            DateTime.Now;


                        _repository.UpdateHeSoDongGop(
                            existingItem
                        );
                    }
                }

                scope.Complete();
            }


            return true;
        }


        #endregion


        #region NHÓM 4: KHỞI TẠO HỆ SỐ RIÊNG CHO PROJECT


        public bool InitializeCustomCoefficients(
            Guid idDuAn)
        {
            BusinessValidator.ThrowIf(
                idDuAn == Guid.Empty,
                BackEndResourceKeys.INVALID_DATA
            );


            DataTable systemDefaults =
                _repository.GetSystemDefaults();


            if (systemDefaults == null ||
                systemDefaults.Rows.Count == 0)
            {
                return false;
            }


            string currentUser =
                SweetContext.Current != null
                    ? SweetContext.Current.UserName
                    : "System";


            using (var scope =
                new TransactionScope())
            {
                foreach (DataRow row
                    in systemDefaults.Rows)
                {
                    if (row["IdDoUuTien"] == DBNull.Value)
                        continue;


                    if (row["HeSoDongGop"] == DBNull.Value)
                        continue;


                    Guid idDoUuTien =
                        row.Field<Guid>(
                            "IdDoUuTien"
                        );


                    TblHeSoDongGop existingItem =
                        _repository.GetProjectCoefficient(
                            idDuAn,
                            idDoUuTien
                        );


                    if (existingItem != null)
                        continue;


                    TblHeSoDongGop newItem =
                        new TblHeSoDongGop
                        {
                            IdHeSoDongGop =
                                Guid.NewGuid(),

                            IdDuAn =
                                idDuAn,

                            IdDoUuTien =
                                idDoUuTien,

                            HeSoDongGop =
                                Convert.ToDecimal(
                                    row["HeSoDongGop"]
                                ),

                            DaXoa =
                                false,

                            NguoiTao =
                                currentUser,

                            NgayTao =
                                DateTime.Now,

                            NguoiCapNhat =
                                null,

                            NgayCapNhat =
                                null
                        };


                    _repository.InsertHeSoDongGop(
                        newItem
                    );
                }


                scope.Complete();
            }


            return true;
        }


        #endregion


        #region NHÓM 5: LƯU HỆ SỐ RIÊNG CỦA PROJECT


        public bool SaveHeSoCuaDuAn(
            Guid idDuAn,
            List<TblHeSoDongGop> items)
        {
            BusinessValidator.ThrowIf(
                idDuAn == Guid.Empty,
                BackEndResourceKeys.INVALID_DATA
            );

            BusinessValidator.ThrowIfNull(
                items,
                BackEndResourceKeys.INVALID_DATA
            );

            if (items.Count == 0)
                return false;


            string currentUser =
                SweetContext.Current != null
                    ? SweetContext.Current.UserName
                    : "System";


            using (var scope =
                new TransactionScope())
            {
                foreach (TblHeSoDongGop item
                    in items)
                {
                    BusinessValidator.ThrowIfNull(
                        item,
                        BackEndResourceKeys.INVALID_DATA
                    );

                    BusinessValidator.ThrowIf(
                        item.IdDoUuTien == Guid.Empty,
                        BackEndResourceKeys.INVALID_DATA
                    );

                    BusinessValidator.ThrowIf(
                        item.HeSoDongGop < 0,
                        BackEndResourceKeys.INVALID_DATA
                    );


                    TblHeSoDongGop existingItem =
                        _repository.GetProjectCoefficient(
                            idDuAn,
                            item.IdDoUuTien
                        );


                    if (existingItem == null)
                    {
                        item.IdHeSoDongGop =
                            item.IdHeSoDongGop == Guid.Empty
                                ? Guid.NewGuid()
                                : item.IdHeSoDongGop;

                        item.IdDuAn =
                            idDuAn;

                        item.DaXoa =
                            false;

                        item.NguoiTao =
                            currentUser;

                        item.NgayTao =
                            DateTime.Now;

                        item.NguoiCapNhat =
                            null;

                        item.NgayCapNhat =
                            null;


                        _repository.InsertHeSoDongGop(
                            item
                        );
                    }
                    else
                    {
                        existingItem.HeSoDongGop =
                            item.HeSoDongGop;

                        existingItem.DaXoa =
                            false;

                        existingItem.NguoiCapNhat =
                            currentUser;

                        existingItem.NgayCapNhat =
                            DateTime.Now;


                        _repository.UpdateHeSoDongGop(
                            existingItem
                        );
                    }
                }


                bool updated =
                    _repository.SetProjectUseDefault(
                        idDuAn,
                        false
                    );


                BusinessValidator.ThrowIf(
                    !updated,
                    BackEndResourceKeys.SERVICE_UNAVAILABLE
                );


                scope.Complete();
            }


            return true;
        }


        #endregion


        #region NHÓM 6: CHUYỂN PROJECT VỀ DÙNG DEFAULT


        public bool SetDuAnSuDungHeSoMacDinh(
            Guid idDuAn)
        {
            BusinessValidator.ThrowIf(
                idDuAn == Guid.Empty,
                BackEndResourceKeys.INVALID_DATA
            );


            return _repository.SetProjectUseDefault(
                idDuAn,
                true
            );
        }


        #endregion
    }
}