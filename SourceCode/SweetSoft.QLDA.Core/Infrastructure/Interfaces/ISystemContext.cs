using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Infrastructure.Interfaces
{
    public interface ISystemContext
    {
        string SystemName { get; }
        Guid ApplicationId { get; }
    }
}
