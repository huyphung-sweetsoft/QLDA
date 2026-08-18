using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SweetSoft.QLDA.BackOffice.Common
{
    public class GridviewCustomDelegateClass
    {
        public delegate void PageChangedEventHandler(object sender, GridviewCustomPageChangeArgs e);
    }
}