using System;

namespace SweetSoft.QLDA.Controls.Helpers
{
    public class ExtraControlEventArg
    {
        ExtraIcon cr_icon = ExtraIcon.A_Delete;
        string cr_text = "Agree";
        public bool IsControl { get; set; }
        public string CommandName { get; set; }
        public string ControlClientID { get; set; }
        public Type SourceType { get; set; }
        public object Value { get; set; }
        public object Tag { get; set; }
        public ExtraIcon SubmitIcon { get { return cr_icon; } set { cr_icon = value; } }
        public string SubmitText { get { return cr_text; } set { cr_text = value; } }
        public ExtraControlEventArg() { }
    }
}
