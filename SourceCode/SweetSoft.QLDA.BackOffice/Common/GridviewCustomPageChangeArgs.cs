using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SweetSoft.QLDA.BackOffice.Common
{
    public partial class GridviewCustomPageChangeArgs : EventArgs
    {
        private int _currentPageNumber;
        public int CurrentPageNumber
        {
            get { return _currentPageNumber; }
            set { _currentPageNumber = value; }
        }

        private int _totalPages;
        public int TotalPages
        {
            get { return _totalPages; }
            set { _totalPages = value; }
        }

        private int _currentPageSize;
        public int CurrentPageSize
        {
            get { return _currentPageSize; }
            set { _currentPageSize = value; }
        }
    }
}