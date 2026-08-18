using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Managers
{
    public interface IProgressNotifier
    {
        void Notify(string message);
    }
}
