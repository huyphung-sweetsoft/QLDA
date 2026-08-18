using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Infrastructure.Interfaces
{
    public interface ILocalizationContext
    {
        string CurrentLanguageCode { get; }
        byte CurrentLanguageId { get; set; }
    }
}
