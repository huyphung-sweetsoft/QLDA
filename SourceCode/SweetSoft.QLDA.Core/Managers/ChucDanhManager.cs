using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.DataAccess;
using SweetSoft.QLDA.Core.Respositories;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Managers
{
    public class ChucDanhManager : BaseManager
    {
        // 1. Sửa lại đúng kiểu dữ liệu ChucDanhManager cho Singleton
        private static readonly Lazy<ChucDanhManager> _instance = new Lazy<ChucDanhManager>(() => new ChucDanhManager());
        public static ChucDanhManager Instance => _instance.Value;

        // 2. Constructor gọn gàng, bỏ cái _repository bị lỗi chữ A đi
        public ChucDanhManager(IAppContext applicationContext = null) : base(applicationContext)
        {
        }

        public List<TblChucDanh> GetListForDropdown()
        {
            // Trả về trực tiếp từ hàm static
            return ChucDanhRepository.GetListForDropdown();
        }
    }
}