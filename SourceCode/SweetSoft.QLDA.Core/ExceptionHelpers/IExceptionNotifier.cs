using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.ExceptionHelpers
{
    public interface IExceptionNotifier
    {
        void Notify(BusinessException exception);
    }
}
