using System.Collections.Generic;
using SubSonic;
using SweetSoft.QLDA.DataAccess;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class DiaChinhRepository
    {
        public List<TblTinhThanh> GetAllTinhThanh(bool onlyActive = true)
        {
            var query = new Select().From(TblTinhThanh.Schema);
            if (onlyActive)
            {
                query.Where(TblTinhThanh.Columns.KichHoat).IsEqualTo(true);
            }
            // Sắp xếp Alphabet theo tên Tỉnh
            query.OrderAsc(TblTinhThanh.Columns.TenTinhThanh);
            return query.ExecuteTypedList<TblTinhThanh>();
        }

        public List<TblXaPhuong> GetXaPhuongByMaTinh(string maTinh, bool onlyActive = true)
        {
            if (string.IsNullOrEmpty(maTinh))
                return new List<TblXaPhuong>();

            var query = new Select().From(TblXaPhuong.Schema)
                                    .Where(TblXaPhuong.Columns.MaTinh).IsEqualTo(maTinh);
            if (onlyActive)
            {
                query.And(TblXaPhuong.Columns.KichHoat).IsEqualTo(true);
            }
            // Sắp xếp Alphabet theo tên Xã/Phường
            query.OrderAsc(TblXaPhuong.Columns.TenXaPhuong);
            return query.ExecuteTypedList<TblXaPhuong>();
        }
    }
}