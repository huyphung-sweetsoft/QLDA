using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class DuAnManager: BaseManager
    {
        private static readonly Lazy<DuAnManager> _instance = new Lazy<DuAnManager>(() => new DuAnManager());
        public static DuAnManager Instance => _instance.Value;
        private readonly DuAnRepository _repository;
        private readonly AuditManager _auditManager;

        public DuAnManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new DuAnRepository(_auditManager);
        }

        public DataTable SearchDuAn(string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchTerm, orderBy, pageNumber, pageSize, out totalRecord);
        }

        public TblDuAn CreateOrUpdate(TblDuAn dto)
        {
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.TenDuAn, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.TenDuAn));
            BusinessValidator.ThrowIfNullOrEmpty(dto.IdNhanVienQuanLy, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.IdNhanVienQuanLy));
        }
    }
}
