using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

//---------------------- PROGRAMMER LOG ----------------------//
//Created by: Truong, 09 Apr 2025
namespace SweetSoft.QLDA.Core.ExcelManager
{
    public class ExcelExportOptions
    {
        public string SheetName { get; set; } = "Report";
        public List<HeaderLine> HeaderRows { get; set; } = new List<HeaderLine>();
        public bool IsFixedHeader { get; set; } = true;
        public Dictionary<string, Action<ExcelRange>> ColumnStyles { get; set; } = new Dictionary<string, Action<ExcelRange>>();
        public List<(int FromRow, int FromCol, int ToRow, int ToCol)> MergeCells { get; set; } = new List<(int FromRow, int FromCol, int ToRow, int ToCol)>();
        public HashSet<string> ShowColumns { get; set; } = null;
        public List<string> ColumnNames { get; set; } = null;
        public bool EnableZebraStripe { get; set; } = false;
        public bool ShowGridLines { get; set; } = true;
        public Color EvenRowColor { get; set; } = Color.White;
        public Color OddRowColor { get; set; } = Color.LightGray;
        public string GroupByColumn { get; set; }
        public List<string> SumColumns { get; set; } = new List<string>();
        public ExcelStyleOptions GroupSummaryStyle { get; set; }
        public ExcelStyleOptions GrandTotalStyle { get; set; }
        public bool ShowGrandTotal { get; set; } = false;
        public List<ConditionalFormat> ConditionalFormats { get; set; } = new List<ConditionalFormat>();
        public List<ConditionalMappingText> ConditionalMappingTexts { get; set; } = new List<ConditionalMappingText>();
        public List<int> GroupRowRanges { get; set; } = new List<int>();
        public List<int> GroupColumnRanges { get; set; } = new List<int>();
        public ePictureType ImageType { get; set; }
        public byte[] LogoImageBytes { get; set; } = null;
        public int LogoHeight { get; set; } = 80;
        public int LogoWidth { get; set; } = 120;
        public int LogoCols { get; set; } = 1;
        public bool IsLogoCenter { get; set; } = false;
        public bool UseHiddenDataSheet { get; set; } = true;
        public List<ChartDefinition> Charts { get; set; } = new List<ChartDefinition>();
        public GroupedChartDefinition GroupedChart { get; set; } = null;
        public HashSet<string> DecryptColumns { get; set; } = null;
        public Dictionary<string, Type> EnumMappings { get; set; }
        public ExcelDocumentProperties ExcelProperties { get; set; } = null;
        public ExcelProtectionOptions ExcelProtections { get; set; }
        public Dictionary<string, double> ColumnWidths { get; set; } = null;
        public List<string> WrapTextColumns { get; set; } = null;
        public MasterDetailOptions MasterDetailOptions { get; set; }

    }
    public class MasterDetailOptions
    {
        public string MasterKeyColumn { get; set; } // Khóa chính
        public DataTable DetailDataTable { get; set; } 
        public string DetailForeignKeyColumn { get; set; } // Khóa ngoại (sub table)
        public List<string> DetailColumns { get; set; } 
        public Dictionary<string, string> DetailColumnNames { get; set; }
        public Dictionary<string, int> DetailHeaderMergeRightSpans { get; set; } = new Dictionary<string, int>();
        public bool ShowDetailHeaders { get; set; } = true;
        public ExcelStyleOptions DetailHeaderStyle { get; set; } // Style cho header detail
        public ExcelStyleOptions DetailRowStyle { get; set; } // Style cho row detail (áp dụng chung)
        public Dictionary<string, ExcelStyleOptions> DetailColumnStyles { get; set; } // Style riêng cho từng cột detail
        public Dictionary<string, string> DetailNumberFormats { get; set; } // Number format cho từng cột detail
        public Dictionary<string, Color> DetailColumnBackgroundColors { get; set; } // Background color cho từng cột
        public Dictionary<string, Color> DetailColumnTextColors { get; set; } // Text color cho từng cột
        public Dictionary<string, bool> DetailColumnBold { get; set; } // Bold cho từng cột
        public Dictionary<string, ExcelHorizontalAlignment> DetailColumnAlignments { get; set; } // Alignment cho từng cột 
        public Dictionary<string, int> DetailColumnMergeRightSpans { get; set; } = new Dictionary<string, int>(); // Ví dụ: { "Programme" : 3 } => merge ô hiện tại + 2 ô kế bên (tổng 3 cột: C..E)
        public ExcelStyle MasterRowStyle { get; set; } // Style cho master row (dòng sản phẩm)
        public Color MasterRowBackgroundColor { get; set; } = Color.LightBlue; // Background color cho master row
        public Color MasterRowTextColor { get; set; } = Color.DarkBlue; // Text color cho master row
        public bool MasterRowBold { get; set; } = true; // Master row có bold không
        public int DetailIndentLevel { get; set; } = 1; // Số level indent cho detail
        public List<ConditionalMappingText> ConditionalMappingTexts { get; set; }
    }
    public class HeaderLine
    {
        public string Text { get; set; }
        public float? FontSize { get; set; }
        public bool? Bold { get; set; }
        public Color? FontColor { get; set; }
        public ExcelHorizontalAlignment? HorizontalAlignment { get; set; }
    }
    public class ExcelStyleOptions
    {
        public Color BackgroundColor { get; set; } = Color.LightGray;
        public Color FontColor { get; set; } = Color.Black;
        public int FontSize { get; set; } = 12;
        public bool Bold { get; set; } = true;
        public string NumberFormat { get; set; } = "";
        public bool WrapText { get; set; } = false;
        public ExcelHorizontalAlignment HorizontalAlignment { get; set; } = ExcelHorizontalAlignment.Left;
    }

    public class ExcelDocumentProperties
    {
        public string Title { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Author { get; set; } = "";
        public string Keywords { get; set; } = "";
        public string Comments { get; set; } = "";
        public string Company { get; set; } = "";
        public string LastSaveBy { get; set; } = "";
    }
    public class ExcelProtectionOptions
    {
        public string Password { get; set; } = "";
        public bool ProtectWorkbookStructure { get; set; } = false;
        public bool ProtectWorksheet { get; set; } = false;

        public bool AllowSelectLockedCells { get; set; } = true;
        public bool AllowSelectUnlockedCells { get; set; } = true;
        public bool AllowFormatCells { get; set; } = false;
        public bool AllowInsertRows { get; set; } = false;
        public bool AllowDeleteColumns { get; set; } = false;
        public bool AllowSort { get; set; } = false;
    }
    public class ChartDefinition
    {
        public string Title { get; set; } = "";
        public string CategoryColumn { get; set; }
        public string ValueColumn { get; set; }
        public eChartType ChartType { get; set; } = eChartType.ColumnClustered;
    }

    public class GroupedChartDefinition
    {
        public string GroupByColumn { get; set; }
        public string CategoryColumn { get; set; }
        public string ValueColumn { get; set; }
        public eChartType ChartType { get; set; } = eChartType.ColumnClustered;
        public bool DisplayRowData { get; set; } = false;
    }
    public class ConditionalFormat
    {
        public string ColumnName { get; set; }
        public Func<object, bool> Condition { get; set; }
        public Color HighlightColor { get; set; }
    }

    public class ConditionalMappingText
    {
        public string ColumnName { get; set; }
        public Dictionary<string, string> ValueMappings { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string DefaultText { get; set; }
        public Func<object, string> ResolveFromDb { get; set; }
    }
}
