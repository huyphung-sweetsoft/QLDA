using SubSonic;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class LoaiRepository : BaseRepository<TblLoai>
    {
        public LoaiRepository(AuditManager auditManager) : base(auditManager)
        {
        }

        public override TblLoai GetById(Guid id)
        {
            return new Select()
                .From(TblLoai.Schema)
                .Where(TblLoai.IdLoaiColumn).IsEqualTo(id)
                .And(TblLoai.DaXoaColumn).IsEqualTo(false)
                .ExecuteSingle<TblLoai>();
        }

        public List<TblLoai> GetByDoiTuong(string doiTuong)
        {
            Select select = new Select();
            select.From(TblLoai.Schema)
                .Where(TblLoai.DoiTuongColumn).IsEqualTo(doiTuong)
                .And(TblLoai.DaXoaColumn).IsEqualTo(false)
                .OrderAsc(TblLoai.Columns.ThuTuHienThi);
            return select.ExecuteTypedList<TblLoai>();
        }

        public bool IsDuplicate(string doiTuong, string tenLoai, Guid? excludeId = null)
        {
            Select select = new Select();
            select.From(TblLoai.Schema)
                .Where(TblLoai.DoiTuongColumn).IsEqualTo(doiTuong)
                .And(TblLoai.TenLoaiColumn).IsEqualTo(tenLoai)
                .And(TblLoai.DaXoaColumn).IsEqualTo(false);

            if (excludeId.HasValue)
            {
                select.And(TblLoai.IdLoaiColumn).IsNotEqualTo(excludeId.Value);
            }

            return select.GetRecordCount() > 0;
        }

        public override TblLoai Insert(TblLoai item)
        {
            item.Save();
            Task.Run(async () =>
            {
                await _auditManager.LogActionAsync(LogActions.Actions.CREATE, item, _tableName, item.IdLoai);
            });
            return item;
        }

        public override TblLoai Update(TblLoai itemNew)
        {
            var id = itemNew.IdLoai;
            TblLoai itemOld = GetById(id);
            itemNew.Save();
            
            string updatedBy = itemNew.NguoiCapNhat ?? "";
            
            Task.Run(async () =>
            {
                await _auditManager.LogChangesAsync(itemOld, itemNew, _tableName, id, updatedBy);
            });
            return itemNew;
        }

        public bool DeleteLoai(Guid idLoai)
        {
            TblLoai item = GetById(idLoai);
            if (item != null)
            {
                item.DaXoa = true;
                item.Save();
                
                Task.Run(async () =>
                {
                    await _auditManager.LogActionAsync(LogActions.Actions.DELETE, item, _tableName, idLoai);
                });
                return true;
            }
            return false;
        }
    }
}
