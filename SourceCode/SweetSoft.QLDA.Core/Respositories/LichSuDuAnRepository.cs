using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Respositories
{
    public class LichSuDuAnRepository : BaseRepository<TblLichSuDuAn>
    {
        public LichSuDuAnRepository(AuditManager auditManager) : base(auditManager)
        {
        }
    }
}
