using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.DataAccess;
using SweetSoft.QLDA.Core.Respositories;
using System;
using System.Collections.Generic;

namespace SweetSoft.QLDA.Core.Managers
{
    public class PhongBanManager : BaseManager
    {
        // 1. Khai báo đúng kiểu PhongBanManager cho Singleton
        private static readonly Lazy<PhongBanManager> _instance = new Lazy<PhongBanManager>(() => new PhongBanManager());
        public static PhongBanManager Instance => _instance.Value;

        // 2. Constructor kế thừa từ BaseManager
        public PhongBanManager(IAppContext applicationContext = null) : base(applicationContext)
        {
        }

        public List<TblPhongBan> GetListForDropdown()
        {
            // Trả về trực tiếp từ hàm static của Repository
            return PhongBanRepository.GetListForDropdown();
        }
    }
}