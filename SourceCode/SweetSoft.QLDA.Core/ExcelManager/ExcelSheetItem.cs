using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.ExcelManager
{
    // Danh sách sheet, mỗi sheet gồm: tên sheet, dữ liệu, option riêng
    public class ExcelSheetItem
    {
        public string SheetName { get; set; }
        public DataTable Table { get; set; }
        public ExcelExportOptions Options { get; set; }
    }

}
