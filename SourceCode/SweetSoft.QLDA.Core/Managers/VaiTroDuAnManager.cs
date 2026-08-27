using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public class VaiTroDuAnManager : BaseManager
    {
        private static readonly Lazy<VaiTroDuAnManager> _instance = new Lazy<VaiTroDuAnManager>(() => new VaiTroDuAnManager());
        public static VaiTroDuAnManager Instance => _instance.Value;
        private readonly VaiTroDuAnRepository _repository;
        private readonly AuditManager _auditManager;
        public VaiTroDuAnManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new VaiTroDuAnRepository(_auditManager);
        }

        public TblVaiTroDuAn GetActiveByIdVaiTro(string idVaiTro)
        {
            BusinessValidator.ThrowIfNullOrEmpty(idVaiTro, BackEndResourceKeys.INVALID_VALUE, nameof(idVaiTro), ErrorCodes.NotFound);
            TblVaiTroDuAn vaiTro = _repository.GetActiveByIdVaiTro(idVaiTro);

            if (vaiTro == null)
            {
                BusinessValidator.ThrowIfNullOrEmpty(idVaiTro, BackEndResourceKeys.NOT_FOUND, nameof(idVaiTro), ErrorCodes.NotFound);
            }

            return vaiTro;
        }
    }
}
