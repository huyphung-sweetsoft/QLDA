using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class DocumentTypeRepository : BaseRepository<TblLoaiTaiLieu>
    {
        public DocumentTypeRepository(AuditManager auditManager)
            : base(auditManager)
        {
        }

        public List<TblLoaiTaiLieu> GetAll(
            string keyword = null,
            Guid? idNhomTaiLieu = null)
        {
            List<TblLoaiTaiLieu> items = new Select()
                .From(TblLoaiTaiLieu.Schema)
                .Where(TblLoaiTaiLieu.DaXoaColumn).IsEqualTo(false)
                .ExecuteTypedList<TblLoaiTaiLieu>();

            if (idNhomTaiLieu.HasValue
                && idNhomTaiLieu.Value != Guid.Empty)
            {
                items = items.Where(item =>
                        item.IdNhomTaiLieu == idNhomTaiLieu.Value)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string searchValue = keyword.Trim();

                items = items.Where(item =>
                        ContainsIgnoreCase(item.TenLoai, searchValue)
                        || ContainsIgnoreCase(item.MoTa, searchValue))
                    .ToList();
            }

            return items
                .OrderBy(item => item.ThuTuHienThi)
                .ThenBy(item => item.TenLoai)
                .ToList();
        }

        public override TblLoaiTaiLieu GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return new Select()
                .From(TblLoaiTaiLieu.Schema)
                .Where(TblLoaiTaiLieu.IdLoaiTaiLieuColumn).IsEqualTo(id)
                .And(TblLoaiTaiLieu.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblLoaiTaiLieu>();
        }

        public bool IsNameExisted(
            string name,
            Guid idNhomTaiLieu,
            Guid excludeId)
        {
            if (string.IsNullOrWhiteSpace(name)
                || idNhomTaiLieu == Guid.Empty)
            {
                return false;
            }

            Select select = new Select();
            select.From(TblLoaiTaiLieu.Schema);
            select.Where(TblLoaiTaiLieu.IdNhomTaiLieuColumn)
                .IsEqualTo(idNhomTaiLieu);
            select.And(TblLoaiTaiLieu.TenLoaiColumn)
                .IsEqualTo(name.Trim());
            select.And(TblLoaiTaiLieu.DaXoaColumn)
                .IsEqualTo(false);

            if (excludeId != Guid.Empty)
            {
                select.And(TblLoaiTaiLieu.IdLoaiTaiLieuColumn)
                    .IsNotEqualTo(excludeId);
            }

            return select.GetRecordCount() > 0;
        }

        public bool IsInUse(Guid id)
        {
            if (id == Guid.Empty)
                return false;

            int documentCount = new Select()
                .From(TblTaiLieu.Schema)
                .Where(TblTaiLieu.IdLoaiTaiLieuColumn).IsEqualTo(id)
                .And(TblTaiLieu.DaXoaColumn).IsEqualTo(false)
                .GetRecordCount();

            if (documentCount > 0)
                return true;

            return new Select()
                .From(TblMauTaiLieu.Schema)
                .Where(TblMauTaiLieu.IdLoaiTaiLieuColumn).IsEqualTo(id)
                .And(TblMauTaiLieu.DaXoaColumn).IsEqualTo(false)
                .GetRecordCount() > 0;
        }

        public override TblLoaiTaiLieu Insert(TblLoaiTaiLieu item)
        {
            if (item == null)
                return null;

            item.Save();
            LogCreate(item);
            return item;
        }

        public override TblLoaiTaiLieu Update(TblLoaiTaiLieu itemNew)
        {
            if (itemNew == null)
                return null;

            TblLoaiTaiLieu itemOld = GetById(itemNew.IdLoaiTaiLieu);
            itemNew.Save();
            LogUpdate(itemOld, itemNew);
            return itemNew;
        }

        public override bool Delete(TblLoaiTaiLieu item)
        {
            if (item == null)
                return false;

            item.DaXoa = true;
            item.Save();
            LogDelete(item);
            return true;
        }

        private void LogCreate(TblLoaiTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.CREATE,
                            item,
                            _tableName,
                            item.IdLoaiTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log CREATE action for TblLoaiTaiLieu");
                }
            });
        }

        private void LogUpdate(
            TblLoaiTaiLieu itemOld,
            TblLoaiTaiLieu itemNew)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogChangesAsync(
                            itemOld,
                            itemNew,
                            _tableName,
                            itemNew.IdLoaiTaiLieu,
                            itemNew.NguoiCapNhat ?? string.Empty)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log changes for TblLoaiTaiLieu");
                }
            });
        }

        private void LogDelete(TblLoaiTaiLieu item)
        {
            Task.Run(async () =>
            {
                try
                {
                    await _auditManager.LogActionAsync(
                            LogActions.Actions.DELETE,
                            item,
                            _tableName,
                            item.IdLoaiTaiLieu)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SysLogger.LogError(
                        ex,
                        "Failed to log DELETE action for TblLoaiTaiLieu");
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
