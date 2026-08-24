using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class DocumentGroupRepository : BaseRepository<TblNhomTaiLieu>
    {
        public DocumentGroupRepository(AuditManager auditManager)
            : base(auditManager)
        {
        }

        public List<TblNhomTaiLieu> GetAll(string keyword = null)
        {
            List<TblNhomTaiLieu> items = new Select()
                .From(TblNhomTaiLieu.Schema)
                .Where(TblNhomTaiLieu.DaXoaColumn).IsEqualTo(false)
                .ExecuteTypedList<TblNhomTaiLieu>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchValue = keyword.Trim();

                items = items.Where(item =>
                        ContainsIgnoreCase(item.TenNhom, searchValue)
                        || ContainsIgnoreCase(item.MoTa, searchValue))
                    .ToList();
            }

            return items
                .OrderBy(item => item.ThuTuHienThi)
                .ThenBy(item => item.TenNhom)
                .ToList();
        }

        public override TblNhomTaiLieu GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblNhomTaiLieu.Schema)
                .Where(TblNhomTaiLieu.IdNhomTaiLieuColumn).IsEqualTo(id)
                .And(TblNhomTaiLieu.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblNhomTaiLieu>();
        }

        public bool IsNameExisted(string name, Guid excludeId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            Select select = new Select();
            select.From(TblNhomTaiLieu.Schema);
            select.Where(TblNhomTaiLieu.TenNhomColumn)
                .IsEqualTo(name.Trim());
            select.And(TblNhomTaiLieu.DaXoaColumn)
                .IsEqualTo(false);

            if (excludeId != Guid.Empty)
            {
                select.And(TblNhomTaiLieu.IdNhomTaiLieuColumn)
                    .IsNotEqualTo(excludeId);
            }

            return select.GetRecordCount() > 0;
        }

        public bool IsInUse(Guid id)
        {
            if (id == Guid.Empty)
                return false;

            return new Select()
                .From(TblLoaiTaiLieu.Schema)
                .Where(TblLoaiTaiLieu.IdNhomTaiLieuColumn).IsEqualTo(id)
                .And(TblLoaiTaiLieu.DaXoaColumn).IsEqualTo(false)
                .GetRecordCount() > 0;
        }

        public override TblNhomTaiLieu Insert(TblNhomTaiLieu item)
        {
            if (item == null)
                return null;

            item.Save();
            LogCreate(item);
            return item;
        }

        public override TblNhomTaiLieu Update(TblNhomTaiLieu itemNew)
        {
            if (itemNew == null)
                return null;

            TblNhomTaiLieu itemOld = GetById(itemNew.IdNhomTaiLieu);
            itemNew.Save();
            LogUpdate(itemOld, itemNew);
            return itemNew;
        }

        public override bool Delete(TblNhomTaiLieu item)
        {
            if (item == null)
                return false;

            item.DaXoa = true;
            item.Save();
            LogDelete(item);
            return true;
        }

        private void LogCreate(TblNhomTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.CREATE,
                            item,
                            _tableName,
                            item.IdNhomTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log CREATE action for TblNhomTaiLieu");
                }
            });
        }

        private void LogUpdate(
            TblNhomTaiLieu itemOld,
            TblNhomTaiLieu itemNew)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(
                            itemOld,
                            itemNew,
                            _tableName,
                            itemNew.IdNhomTaiLieu,
                            itemNew.NguoiCapNhat ?? string.Empty)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log changes for TblNhomTaiLieu");
                }
            });
        }

        private void LogDelete(TblNhomTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.DELETE,
                            item,
                            _tableName,
                            item.IdNhomTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log DELETE action for TblNhomTaiLieu");
                }
            });
        }

        private static bool ContainsIgnoreCase(
            string source,
            string searchValue)
        {
            if (string.IsNullOrEmpty(source))
                return false;

            return source.IndexOf(
                searchValue,
                StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }
}
