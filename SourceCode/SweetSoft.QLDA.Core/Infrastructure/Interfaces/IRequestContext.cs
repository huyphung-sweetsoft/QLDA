using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SweetSoft.QLDA.Core.Infrastructure.Interfaces
{
    public interface IRequestContext
    {
        IDictionary Items { get; }
        bool IsWebRequest { get; }
        HttpContext Context { get; }
        string SiteUrl { get; }
        Uri CurrentUri { get; set; }
        string HostPath { get; }
        string MapPath(string path);
        string PhysicalPath(string path);
    }
}
