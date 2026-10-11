using System;
using System.Collections.Generic;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.DataAccess;

namespace SweetSoft.QLDA.Core.Managers
{
    public class DiaChinhManager
    {
        private static readonly Lazy<DiaChinhManager> _instance = new Lazy<DiaChinhManager>(() => new DiaChinhManager());
        public static DiaChinhManager Instance => _instance.Value;

        private readonly DiaChinhRepository _repository;

        public DiaChinhManager()
        {
            _repository = new DiaChinhRepository();
        }

        public List<TblTinhThanh> GetAllTinhThanh(bool onlyActive = true)
        {
            // Trong tương lai nếu dữ liệu nặng, ông có thể áp dụng Cache (AppCache.Get) ở ngay dòng này
            return _repository.GetAllTinhThanh(onlyActive);
        }

        public List<TblXaPhuong> GetXaPhuongByMaTinh(string maTinh, bool onlyActive = true)
        {
            return _repository.GetXaPhuongByMaTinh(maTinh, onlyActive);
        }
    }
}