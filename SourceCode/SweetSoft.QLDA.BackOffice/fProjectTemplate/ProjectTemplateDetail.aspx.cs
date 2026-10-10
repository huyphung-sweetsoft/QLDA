using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fProjectTemplate
{
    public partial class ProjectTemplateDetail : BaseAdminPage
    {
        protected Guid IdMau
        {
            get
            {
                try
                {
                    string temp = CommonHelpers.QueryString("IdMau") ?? CommonHelpers.QueryString("id");
                    if (string.IsNullOrEmpty(temp)) return Guid.Empty;
                    if (Guid.TryParse(temp, out Guid id)) return id;
                    return Guid.Parse(SecurityUtilities.UnprotectUrlParameter(temp));
                }
                catch { return Guid.Empty; }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlProjectTemplateDetail1.AddPhaseTemplateHandlerCallback += ShowAddPhaseModal;
            CtrlProjectTemplateDetail1.AddSubTaskTemplateHandlerCallback += ShowAddSubTaskModal;
            CtrlProjectTemplateDetail1.EditTaskTemplateHandlerCallback += ShowEditTaskModal; // Lắng nghe sự kiện Edit

            if (!IsPostBack)
            {
                InitControls();
            }
        }

        private void InitControls()
        {
            if (IdMau == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            TblMauDuAn template = TblMauDuAn.FetchByID(IdMau);
            if (template == null || template.DaXoa == true)
            {
                ShowInvalidNotFoundData();
                return;
            }

            string title = string.IsNullOrWhiteSpace(template.TenMau) ? "Chi tiết mẫu công việc" : template.TenMau.Trim();
            SetMetaTagsOgTags(title);
            Navigation1.MainTitle = title;
            Navigation1.keyValuePairUrls = new System.Collections.Generic.Dictionary<string, string> { { "javascript:;", title } };

            CtrlProjectTemplateDetail1.TemplateId = this.IdMau;
            CtrlProjectTemplateDetail1.InitControls();
        }

        // ===============================================
        // MỞ POPUP THEO CHẾ ĐỘ: THÊM GIAI ĐOẠN (ROOT)
        // ===============================================
        private void ShowAddPhaseModal(object sender, EventArgs e)
        {
            hdfParentTaskId.Value = "";
            hdfEditTaskId.Value = ""; // Khóa edit rỗng (Đang thêm mới)
            txtTenGiaiDoanTemplate.Text = string.Empty;
            txtMoTa.Text = string.Empty; // Xóa mô tả cũ

            string newCode = GenerateNewPhaseCode();
            txtMaCongViec.Text = newCode;

            divContextInfo.Visible = false;
            divThoiHan.Visible = false;
            txtThoiHanNgay.Text = "1";
            lblTaskName.InnerHtml = "Mã & Tên giai đoạn <span class=\"required\">*</span>";
            txtTenGiaiDoanTemplate.Attributes["placeholder"] = "Nhập tên giai đoạn chính...";

            BindDependentTasks(null, newCode);

            mdlAddPhaseTemplate.Title = "Thêm Giai đoạn mới";

            upAddPhaseTemplate.Update();
            upnlFooterAddPhaseTemplate.Update();
            mdlAddPhaseTemplate.OpenModal(true);
        }

        // ===============================================
        // MỞ POPUP THEO CHẾ ĐỘ: THÊM TASK CON
        // ===============================================
        private void ShowAddSubTaskModal(object sender, EventArgs e)
        {
            if (sender == null || !(sender is Guid)) return;
            Guid parentId = (Guid)sender;

            hdfParentTaskId.Value = parentId.ToString();
            hdfEditTaskId.Value = ""; // Khóa edit rỗng (Đang thêm mới)
            txtTenGiaiDoanTemplate.Text = string.Empty;
            txtMoTa.Text = string.Empty;

            TblChiTietMau parentTask = TblChiTietMau.FetchByID(parentId);
            string newCode = "";
            if (parentTask != null)
            {
                newCode = GenerateNewSubTaskCode(parentId, parentTask.MaCongViec);
                txtMaCongViec.Text = newCode;

                divContextInfo.Visible = true;
                ltrCongViecCha.Text = $"<strong>[{parentTask.MaCongViec}]</strong> {parentTask.TenCongViec}";

                TblChiTietMau rootPhase = new Select().From(TblChiTietMau.Schema)
                    .Where(TblChiTietMau.Columns.IdMau).IsEqualTo(this.IdMau)
                    .And(TblChiTietMau.Columns.IdGiaiDoanDuAn).IsEqualTo(parentTask.IdGiaiDoanDuAn)
                    .AndExpression(TblChiTietMau.Columns.IdCongViecCha).IsNull()
                    .ExecuteSingle<TblChiTietMau>();

                if (rootPhase != null)
                {
                    ltrGiaiDoan.Text = $"<strong>[{rootPhase.MaCongViec}]</strong> {rootPhase.TenCongViec}";
                }
                else
                {
                    ltrGiaiDoan.Text = parentTask.IdGiaiDoanDuAn.ToString();
                }

                mdlAddPhaseTemplate.Title = $"Thêm công việc con";
            }

            divThoiHan.Visible = true;
            txtThoiHanNgay.Text = "1";
            lblTaskName.InnerHtml = "Mã & Tên công việc <span class=\"required\">*</span>";
            txtTenGiaiDoanTemplate.Attributes["placeholder"] = "Nhập tên công việc con...";

            BindDependentTasks(parentId, newCode);

            upAddPhaseTemplate.Update();
            upnlFooterAddPhaseTemplate.Update();
            mdlAddPhaseTemplate.OpenModal(true);
        }

        // ===============================================
        // MỞ POPUP THEO CHẾ ĐỘ: CHỈNH SỬA (EDIT)
        // ===============================================
        private void ShowEditTaskModal(object sender, EventArgs e)
        {
            if (sender == null || !(sender is Guid)) return;
            Guid taskId = (Guid)sender;

            TblChiTietMau task = TblChiTietMau.FetchByID(taskId);
            if (task == null) { ShowInvalidNotFoundData(); return; }

            hdfEditTaskId.Value = task.IdChiTietMau.ToString(); // Đánh dấu đang Sửa
            hdfParentTaskId.Value = task.IdCongViecCha?.ToString() ?? "";

            // Đổ dữ liệu cũ
            txtMaCongViec.Text = task.MaCongViec;
            txtTenGiaiDoanTemplate.Text = task.TenCongViec;
            txtMoTa.Text = task.MoTa;

            bool isPhase = !task.MaCongViec.Contains("."); // Kiểm tra xem nó là Giai đoạn hay Task con

            if (isPhase)
            {
                divContextInfo.Visible = false;
                divThoiHan.Visible = false;
                lblTaskName.InnerHtml = "Mã & Tên giai đoạn <span class=\"required\">*</span>";
                txtTenGiaiDoanTemplate.Attributes["placeholder"] = "Nhập tên giai đoạn chính...";
            }
            else
            {
                divContextInfo.Visible = true;
                divThoiHan.Visible = true;
                txtThoiHanNgay.Text = (task.ThoiHanNgay ?? 1).ToString();
                lblTaskName.InnerHtml = "Mã & Tên công việc <span class=\"required\">*</span>";
                txtTenGiaiDoanTemplate.Attributes["placeholder"] = "Nhập tên công việc con...";

                if (task.IdCongViecCha.HasValue)
                {
                    TblChiTietMau parentTask = TblChiTietMau.FetchByID(task.IdCongViecCha.Value);
                    if (parentTask != null)
                    {
                        ltrCongViecCha.Text = $"<strong>[{parentTask.MaCongViec}]</strong> {parentTask.TenCongViec}";
                        TblChiTietMau rootPhase = new Select().From(TblChiTietMau.Schema)
                            .Where(TblChiTietMau.Columns.IdMau).IsEqualTo(this.IdMau)
                            .And(TblChiTietMau.Columns.IdGiaiDoanDuAn).IsEqualTo(parentTask.IdGiaiDoanDuAn)
                            .AndExpression(TblChiTietMau.Columns.IdCongViecCha).IsNull()
                            .ExecuteSingle<TblChiTietMau>();

                        if (rootPhase != null)
                        {
                            ltrGiaiDoan.Text = $"<strong>[{rootPhase.MaCongViec}]</strong> {rootPhase.TenCongViec}";
                        }
                        else
                        {
                            ltrGiaiDoan.Text = parentTask.IdGiaiDoanDuAn.ToString();
                        }
                    }
                }
            }

            // Sinh dữ liệu Dropdown phụ thuộc (Dùng chính mã hiện tại để lọc chuẩn)
            BindDependentTasks(task.IdCongViecCha, task.MaCongViec);

            if (task.IdCongViecPhuThuoc.HasValue)
            {
                var item = ddlPhuThuocTemplate.Items.FindByValue(task.IdCongViecPhuThuoc.Value.ToString());
                if (item != null) item.Selected = true;
            }

            mdlAddPhaseTemplate.Title = $"Chỉnh sửa {(isPhase ? "giai đoạn" : "công việc")}";

            upAddPhaseTemplate.Update();
            upnlFooterAddPhaseTemplate.Update();
            mdlAddPhaseTemplate.OpenModal(true);
        }

        private int CompareWBS(string code1, string code2)
        {
            string[] p1 = (code1 ?? "").Split('.');
            string[] p2 = (code2 ?? "").Split('.');
            for (int i = 0; i < Math.Min(p1.Length, p2.Length); i++)
            {
                int.TryParse(p1[i], out int n1);
                int.TryParse(p2[i], out int n2);
                if (n1 != n2) return n1.CompareTo(n2);
            }
            return p1.Length.CompareTo(p2.Length);
        }

        private void BindDependentTasks(Guid? parentId, string currentTaskCode)
        {
            ddlPhuThuocTemplate.Items.Clear();
            ddlPhuThuocTemplate.Items.Add(new ListItem("-- Không phụ thuộc --", string.Empty));

            SqlQuery query = new Select()
                .From(TblChiTietMau.Schema)
                .Where(TblChiTietMau.Columns.IdMau).IsEqualTo(IdMau);

            DataTable dt = query.ExecuteDataSet().Tables[0];

            if (dt != null && dt.Rows.Count > 0)
            {
                var rows = dt.AsEnumerable().ToList();
                rows.Sort((row1, row2) => CompareWBS(row1["MaCongViec"]?.ToString().Trim(), row2["MaCongViec"]?.ToString().Trim()));

                string parentCode = "";
                if (parentId.HasValue)
                {
                    var pRow = rows.Find(r => r["IdChiTietMau"].ToString().Equals(parentId.Value.ToString(), StringComparison.OrdinalIgnoreCase));
                    if (pRow != null) parentCode = pRow["MaCongViec"]?.ToString().Trim() ?? "";
                }

                foreach (DataRow row in rows)
                {
                    string maCongViec = row["MaCongViec"]?.ToString().Trim() ?? string.Empty;
                    string tenCongViec = row["TenCongViec"]?.ToString().Trim() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(maCongViec)) continue;

                    if (Guid.TryParse(Convert.ToString(row["IdChiTietMau"]), out Guid idChiTietMau))
                    {
                        if (CompareWBS(maCongViec, currentTaskCode) >= 0)
                        {
                            continue;
                        }

                        if (!parentId.HasValue)
                        {
                            if (maCongViec.Contains(".")) continue;
                        }
                        else
                        {
                            if (maCongViec == parentCode || parentCode.StartsWith(maCongViec + "."))
                            {
                                continue;
                            }
                        }

                        int level = maCongViec.Split('.').Length;
                        string prefix = level > 1 ? new String('-', (level - 1) * 2) + " " : "";

                        ddlPhuThuocTemplate.Items.Add(new ListItem($"{prefix}[{maCongViec}] {tenCongViec}", idChiTietMau.ToString()));
                    }
                }
            }
        }

        private string GenerateNewPhaseCode()
        {
            SqlQuery query = new Select("MaCongViec")
                .From(TblChiTietMau.Schema)
                .Where(TblChiTietMau.Columns.IdMau).IsEqualTo(IdMau);

            DataTable dt = query.ExecuteDataSet().Tables[0];
            int maxCode = 0;

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string maCongViec = row["MaCongViec"]?.ToString().Trim() ?? string.Empty;
                    if (maCongViec.Contains(".")) continue;

                    if (int.TryParse(maCongViec, out int code) && code > maxCode)
                        maxCode = code;
                }
            }
            return (maxCode + 1).ToString();
        }

        private string GenerateNewSubTaskCode(Guid parentId, string parentCode)
        {
            SqlQuery query = new Select("MaCongViec")
                .From(TblChiTietMau.Schema)
                .Where(TblChiTietMau.Columns.IdMau).IsEqualTo(IdMau)
                .And(TblChiTietMau.Columns.IdCongViecCha).IsEqualTo(parentId);

            DataTable dt = query.ExecuteDataSet().Tables[0];
            int maxChild = 0;

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string maCongViec = row["MaCongViec"]?.ToString().Trim() ?? string.Empty;
                    string[] parts = maCongViec.Split('.');
                    if (parts.Length > 0)
                    {
                        if (int.TryParse(parts[parts.Length - 1], out int lastNum))
                        {
                            if (lastNum > maxChild) maxChild = lastNum;
                        }
                    }
                }
            }
            return $"{parentCode}.{maxChild + 1}";
        }

        private void RecalculateAllParentDurations(Guid templateId)
        {
            var tasks = new Select().From(TblChiTietMau.Schema)
                                    .Where(TblChiTietMau.Columns.IdMau).IsEqualTo(templateId)
                                    .ExecuteTypedList<TblChiTietMau>();

            if (tasks == null || tasks.Count == 0) return;

            var parentIds = tasks.Where(t => t.IdCongViecCha.HasValue)
                                 .Select(t => t.IdCongViecCha.Value)
                                 .Distinct()
                                 .ToHashSet();

            var ES = tasks.ToDictionary(t => t.IdChiTietMau, t => 0);
            var EF = tasks.ToDictionary(t => t.IdChiTietMau, t => t.ThoiHanNgay ?? 1);

            bool changed = true;
            int maxLoop = tasks.Count + 10;
            int loopCount = 0;

            while (changed && loopCount < maxLoop)
            {
                changed = false;
                loopCount++;

                foreach (var t in tasks)
                {
                    int currentES = ES[t.IdChiTietMau];
                    int newES = 0;

                    if (t.IdCongViecPhuThuoc.HasValue && EF.ContainsKey(t.IdCongViecPhuThuoc.Value))
                    {
                        newES = Math.Max(newES, EF[t.IdCongViecPhuThuoc.Value]);
                    }

                    if (t.IdCongViecCha.HasValue && ES.ContainsKey(t.IdCongViecCha.Value))
                    {
                        newES = Math.Max(newES, ES[t.IdCongViecCha.Value]);
                    }

                    if (newES != currentES)
                    {
                        ES[t.IdChiTietMau] = newES;
                        changed = true;
                    }

                    int currentEF = EF[t.IdChiTietMau];
                    int newEF = currentEF;

                    if (parentIds.Contains(t.IdChiTietMau))
                    {
                        var childrenEFs = tasks.Where(c => c.IdCongViecCha == t.IdChiTietMau).Select(c => EF[c.IdChiTietMau]).ToList();
                        if (childrenEFs.Any())
                        {
                            newEF = childrenEFs.Max();
                        }
                    }
                    else
                    {
                        newEF = ES[t.IdChiTietMau] + (t.ThoiHanNgay ?? 1);
                    }

                    if (newEF != currentEF)
                    {
                        EF[t.IdChiTietMau] = newEF;
                        changed = true;
                    }
                }
            }

            foreach (var t in tasks)
            {
                if (parentIds.Contains(t.IdChiTietMau))
                {
                    int calculatedDuration = EF[t.IdChiTietMau] - ES[t.IdChiTietMau];
                    if (calculatedDuration < 1) calculatedDuration = 1;

                    if (t.ThoiHanNgay != calculatedDuration)
                    {
                        new Update(TblChiTietMau.Schema)
                            .Set(TblChiTietMau.Columns.ThoiHanNgay).EqualTo(calculatedDuration)
                            .Where(TblChiTietMau.Columns.IdChiTietMau).IsEqualTo(t.IdChiTietMau)
                            .Execute();
                    }
                }
            }
        }

        protected void btnSavePhaseTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                string tenCongViec = txtTenGiaiDoanTemplate.Text.Trim();
                if (string.IsNullOrWhiteSpace(tenCongViec))
                {
                    ShowNotify("Vui lòng nhập tên công việc.", MSGType.Warning);
                    mdlAddPhaseTemplate.OpenModal(true);
                    return;
                }

                string moTa = txtMoTa.Text.Trim();

                Guid? idPhuThuoc = null;
                if (!string.IsNullOrWhiteSpace(ddlPhuThuocTemplate.SelectedValue) && Guid.TryParse(ddlPhuThuocTemplate.SelectedValue, out Guid selectedId))
                {
                    idPhuThuoc = selectedId;
                }

                // CỜ PHÂN BIỆT THÊM MỚI HAY EDIT (ĐÃ FIX LỖI CS0165)
                Guid editId = Guid.Empty;
                bool isEdit = !string.IsNullOrEmpty(hdfEditTaskId.Value) && Guid.TryParse(hdfEditTaskId.Value, out editId);

                if (isEdit)
                {
                    // ===== XỬ LÝ UPDATE (SỬA) =====
                    if (!this.IsEdit) { ShowAccessDeniedNotify(); return; }

                    TblChiTietMau taskUpdate = TblChiTietMau.FetchByID(editId);
                    if (taskUpdate == null) { ShowInvalidNotFoundData(); return; }

                    taskUpdate.TenCongViec = tenCongViec;
                    taskUpdate.MoTa = moTa;
                    taskUpdate.IdCongViecPhuThuoc = idPhuThuoc;

                    // Chỉ sửa Thời hạn nếu nó là Task con (không phải Phase gốc)
                    if (taskUpdate.IdCongViecCha.HasValue)
                    {
                        if (int.TryParse(txtThoiHanNgay.Text.Trim(), out int thn) && thn > 0)
                        {
                            taskUpdate.ThoiHanNgay = thn;
                        }
                        else
                        {
                            taskUpdate.ThoiHanNgay = 1; // Giá trị tối thiểu
                        }
                    }

                    taskUpdate.Save();
                    ShowNotify("Đã cập nhật công việc thành công!", MSGType.Success);
                }
                else
                {
                    // ===== XỬ LÝ INSERT (THÊM MỚI) =====
                    if (!this.IsAdd) { ShowAccessDeniedNotify(); return; }

                    string maCongViec = txtMaCongViec.Text.Trim();

                    Guid? parentId = null;
                    if (!string.IsNullOrEmpty(hdfParentTaskId.Value) && Guid.TryParse(hdfParentTaskId.Value, out Guid parsedId))
                    {
                        parentId = parsedId;
                    }

                    Guid? idGiaiDoan = Guid.NewGuid();
                    int thoiHanNgay = 1;

                    if (parentId.HasValue)
                    {
                        TblChiTietMau parent = TblChiTietMau.FetchByID(parentId.Value);
                        if (parent != null)
                        {
                            idGiaiDoan = parent.IdGiaiDoanDuAn;
                        }

                        if (!int.TryParse(txtThoiHanNgay.Text.Trim(), out thoiHanNgay) || thoiHanNgay < 1)
                        {
                            thoiHanNgay = 1;
                        }
                    }

                    TblChiTietMau taskInsert = new TblChiTietMau
                    {
                        IdChiTietMau = Guid.NewGuid(),
                        IdMau = IdMau,
                        IdGiaiDoanDuAn = idGiaiDoan,
                        IdCongViecCha = parentId,
                        IdCongViecPhuThuoc = idPhuThuoc,
                        MaCongViec = maCongViec,
                        TenCongViec = tenCongViec,
                        MoTa = moTa, // Thêm mô tả lúc Insert
                        ThoiHanNgay = thoiHanNgay
                    };

                    taskInsert.Save();
                    ShowNotify("Đã thêm công việc thành công!", MSGType.Success);
                }

                RecalculateAllParentDurations(this.IdMau);

                mdlAddPhaseTemplate.CloseModal();
                CtrlProjectTemplateDetail1.Rebind(); // Tải lại lưới
            }
            catch (Exception ex)
            {
                ShowNotify("Lỗi: " + ex.Message, MSGType.Error);
                mdlAddPhaseTemplate.OpenModal(true);
            }
        }
    }
}