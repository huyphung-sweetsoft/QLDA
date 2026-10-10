using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Transactions;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlApplyTemplate : BaseAdminUserControl
    {
        public EventHandler ApplySuccessCallback;

        public Guid ProjectId
        {
            get => ViewState["ProjectId"] != null ? (Guid)ViewState["ProjectId"] : Guid.Empty;
            set => ViewState["ProjectId"] = value;
        }

        private Guid SelectedTemplateId
        {
            get => ViewState["SelectedTemplateId"] != null ? (Guid)ViewState["SelectedTemplateId"] : Guid.Empty;
            set => ViewState["SelectedTemplateId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected string SelectedTemplateDetailUrl
        {
            get
            {
                return SelectedTemplateId == Guid.Empty
                    ? "#"
                    : RewriteURLHelper.ProjectTemplateDetail(SelectedTemplateId);
            }
        }

        public void OpenModal(Guid projectId)
        {
            this.ProjectId = projectId;
            SelectedTemplateId = Guid.Empty;

            pnlList.Attributes["class"] = "apt-list-panel";
            pnlDetail.Attributes["class"] = "apt-detail-panel";

            mdlApplyTemplate.Title = "Áp dụng Mẫu Công Việc";
            grvTemplates.Rebind();
            mdlApplyTemplate.OpenModal(true);
            upnlApplyTemplate.Update();
        }

        protected void grvTemplates_NeedDataSource(object sender, ExtraGridEventArg e)
        {
            DataTable dt = new Select().From(TblMauDuAn.Schema)
                                       .Where(TblMauDuAn.Columns.TrangThai).IsEqualTo(1)
                                       .OrderDesc(TblMauDuAn.Columns.NgayTao)
                                       .ExecuteDataSet().Tables[0];
            grvTemplates.DataSource = dt;
            grvTemplates.DataBind();
        }

        protected void grvTemplates_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "VIEW_DETAIL")
            {
                Guid templateId = Guid.Parse(e.CommandArgument.ToString());
                LoadTemplateDetail(templateId);
            }
            else if (e.CommandName == "APPLY_TEMPLATE")
            {
                Guid templateId = Guid.Parse(e.CommandArgument.ToString());
                ApplyTemplateToProject(templateId);
            }
        }

        private void LoadTemplateDetail(Guid templateId)
        {
            SelectedTemplateId = templateId;
            TblMauDuAn mau = TblMauDuAn.FetchByID(templateId);
            if (mau != null)
                ltrDetailName.Text = mau.TenMau;

            DataTable dtDetail = new Select().From(TblChiTietMau.Schema)
                                             .Where(TblChiTietMau.Columns.IdMau).IsEqualTo(templateId)
                                             .OrderAsc(TblChiTietMau.Columns.MaCongViec)
                                             .ExecuteDataSet().Tables[0];
            AddDependencyDisplayColumns(dtDetail);
            grvTemplateDetails.DataSource = dtDetail;
            grvTemplateDetails.DataBind();

            pnlList.Attributes["class"] = "apt-list-panel is-split";
            pnlDetail.Attributes["class"] = "apt-detail-panel is-split";
            upnlApplyTemplate.Update();
        }

        private void AddDependencyDisplayColumns(DataTable dtDetail)
        {
            if (dtDetail == null)
                return;

            if (!dtDetail.Columns.Contains("PhuThuocMaCongViec"))
                dtDetail.Columns.Add("PhuThuocMaCongViec", typeof(string));
            if (!dtDetail.Columns.Contains("PhuThuocTenCongViec"))
                dtDetail.Columns.Add("PhuThuocTenCongViec", typeof(string));

            Dictionary<Guid, DataRow> taskRowsById = new Dictionary<Guid, DataRow>();
            foreach (DataRow row in dtDetail.Rows)
            {
                Guid taskId;
                if (row["IdChiTietMau"] != DBNull.Value && Guid.TryParse(Convert.ToString(row["IdChiTietMau"]), out taskId))
                    taskRowsById[taskId] = row;
            }

            foreach (DataRow row in dtDetail.Rows)
            {
                row["PhuThuocMaCongViec"] = "—";
                row["PhuThuocTenCongViec"] = string.Empty;

                Guid dependencyId;
                object rawDependencyId = row["IdCongViecPhuThuoc"];
                if (rawDependencyId == DBNull.Value || !Guid.TryParse(Convert.ToString(rawDependencyId), out dependencyId))
                    continue;

                DataRow dependencyRow;
                if (!taskRowsById.TryGetValue(dependencyId, out dependencyRow))
                    continue;

                string dependencyCode = Convert.ToString(dependencyRow["MaCongViec"]);
                string dependencyName = Convert.ToString(dependencyRow["TenCongViec"]);
                row["PhuThuocMaCongViec"] = string.IsNullOrWhiteSpace(dependencyCode) ? "—" : dependencyCode;
                row["PhuThuocTenCongViec"] = dependencyName ?? string.Empty;
            }
        }

        protected void lbtApplySelectedTemplate_Click(object sender, EventArgs e)
        {
            if (SelectedTemplateId == Guid.Empty)
            {
                ShowNotify("Vui lòng chọn mẫu công việc cần áp dụng.", MSGType.Error);
                return;
            }

            ApplyTemplateToProject(SelectedTemplateId);
        }

        protected void lbtCloseDetail_Click(object sender, EventArgs e)
        {
            pnlList.Attributes["class"] = "apt-list-panel";
            pnlDetail.Attributes["class"] = "apt-detail-panel";
            upnlApplyTemplate.Update();
        }

        private void ApplyTemplateToProject(Guid templateId)
        {
            if (this.ProjectId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }
            if (templateId == Guid.Empty)
            {
                ShowNotify("Vui lòng chọn mẫu công việc cần áp dụng.", MSGType.Warning);
                return;
            }
            try
            {
                TblDuAn duAn = TblDuAn.FetchByID(this.ProjectId);
                if (duAn == null)
                {
                    ShowInvalidNotFoundData();
                    return;
                }
                TblMauDuAn mau = TblMauDuAn.FetchByID(templateId);
                if (mau == null || mau.TrangThai != 1)
                {
                    ShowNotify("Mẫu công việc không tồn tại hoặc chưa được phát hành.", MSGType.Warning);
                    return;
                }
                List<TblChiTietMau> lstChiTietMau = new Select()
                    .From(TblChiTietMau.Schema)
                    .Where(TblChiTietMau.Columns.IdMau).IsEqualTo(templateId)
                    .OrderAsc(TblChiTietMau.Columns.MaCongViec)
                    .ExecuteTypedList<TblChiTietMau>();
                if (lstChiTietMau == null || lstChiTietMau.Count == 0)
                {
                    ShowNotify("Mẫu này chưa có công việc nào để áp dụng. Công việc hiện tại của dự án được giữ nguyên.", MSGType.Warning);
                    return;
                }
                List<TblChiTietMau> rootPhases = lstChiTietMau
                    .Where(x => !x.IdCongViecCha.HasValue)
                    .ToList();
                if (rootPhases.Count == 0)
                {
                    ShowNotify("Cấu trúc mẫu không hợp lệ vì không có giai đoạn gốc. Công việc hiện tại của dự án được giữ nguyên.", MSGType.Error);
                    return;
                }
                Dictionary<Guid, TblChiTietMau> templateById = lstChiTietMau
                    .ToDictionary(x => x.IdChiTietMau, x => x);
                // Kiểm tra công việc cha tồn tại và phát hiện vòng lặp trước khi ghi dữ liệu.
                foreach (TblChiTietMau item in lstChiTietMau)
                {
                    TblChiTietMau current = item;
                    HashSet<Guid> visited = new HashSet<Guid>();
                    while (current.IdCongViecCha.HasValue)
                    {
                        if (!visited.Add(current.IdChiTietMau))
                            throw new InvalidOperationException("Mẫu có vòng lặp trong quan hệ công việc cha-con.");
                        TblChiTietMau parent;
                        if (!templateById.TryGetValue(current.IdCongViecCha.Value, out parent))
                            throw new InvalidOperationException("Mẫu chứa công việc có công việc cha không tồn tại.");
                        current = parent;
                    }
                }
                DateTime projectStartDate = duAn.NgayBatDau.Date;
                Dictionary<Guid, Guid> mapIds = new Dictionary<Guid, Guid>();
                foreach (TblChiTietMau item in lstChiTietMau)
                    mapIds.Add(item.IdChiTietMau, Guid.NewGuid());
                Dictionary<Guid, Guid> mapStageIds = new Dictionary<Guid, Guid>();
                foreach (TblChiTietMau rootPhase in rootPhases)
                    mapStageIds.Add(rootPhase.IdChiTietMau, Guid.NewGuid());
                TblChiTietMau firstPhaseTemplate = rootPhases.FirstOrDefault(x =>
                    string.Equals(x.MaCongViec, "1", StringComparison.Ordinal)) ?? rootPhases[0];
                Guid firstPhaseId = mapIds[firstPhaseTemplate.IdChiTietMau];
                int createdCount = 0;
                using (var scope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(5)))
                {
                    DataTable oldTasks = new Select()
                        .From(TblCongViec.Schema)
                        .Where(TblCongViec.Columns.IdDuAn).IsEqualTo(this.ProjectId)
                        .ExecuteDataSet().Tables[0];
                    foreach (DataRow oldTaskRow in oldTasks.Rows)
                    {
                        Guid oldTaskId;
                        if (oldTaskRow["IdCongViec"] == DBNull.Value ||
                            !Guid.TryParse(Convert.ToString(oldTaskRow["IdCongViec"]), out oldTaskId))
                            continue;
                        new Delete().From(TblCongViecNhanVien.Schema)
                            .Where(TblCongViecNhanVien.Columns.IdCongViec).IsEqualTo(oldTaskId)
                            .Execute();
                    }
                    new Update(TblCongViec.Schema)
                        .Set(TblCongViec.Columns.IdCongViecCha).EqualTo((Guid?)null)
                        .Where(TblCongViec.Columns.IdDuAn).IsEqualTo(this.ProjectId)
                        .Execute();
                    new Update(TblCongViec.Schema)
                        .Set(TblCongViec.Columns.IdCongViecPhuThuoc).EqualTo((Guid?)null)
                        .Where(TblCongViec.Columns.IdDuAn).IsEqualTo(this.ProjectId)
                        .Execute();
                    new Delete().From(TblCongViec.Schema)
                        .Where(TblCongViec.Columns.IdDuAn).IsEqualTo(this.ProjectId)
                        .Execute();
                    new Delete().From(TblGiaiDoanDuAn.Schema)
                        .Where(TblGiaiDoanDuAn.Columns.IdDuAn).IsEqualTo(this.ProjectId)
                        .Execute();
                    string currentUser = SweetContext.Current != null
                        ? SweetContext.Current.UserName
                        : "System";
                    if (string.IsNullOrWhiteSpace(currentUser))
                        currentUser = "System";
                    if (currentUser.Length > 150)
                        currentUser = currentUser.Substring(0, 150);
                    int phaseOrder = 1;
                    foreach (TblChiTietMau rootTemplate in rootPhases)
                    {
                        int phaseDuration = Math.Max(1, rootTemplate.ThoiHanNgay ?? 1);
                        string phaseName = string.IsNullOrWhiteSpace(rootTemplate.TenCongViec)
                            ? "Giai đoạn " + phaseOrder
                            : rootTemplate.TenCongViec.Trim();
                        if (phaseName.Length > 250)
                            phaseName = phaseName.Substring(0, 250);
                        string phaseDescription = rootTemplate.MoTa;
                        if (phaseDescription != null && phaseDescription.Length > 1000)
                            phaseDescription = phaseDescription.Substring(0, 1000);
                        TblGiaiDoanDuAn phaseRecord = new TblGiaiDoanDuAn
                        {
                            IdGiaiDoanDuAn = mapStageIds[rootTemplate.IdChiTietMau],
                            IdDuAn = this.ProjectId,
                            IdGiaiDoan = null,
                            NgayBatDau = projectStartDate,
                            NgayDuKienHoanThanh = LichBieuChungManager.Instance.CalculateTaskEndDate(projectStartDate, phaseDuration),
                            NgayHoanThanhThucTe = null,
                            ThuTuGiaiDoan = phaseOrder++,
                            MoTa = phaseDescription,
                            DaXoa = false,
                            NguoiTao = currentUser,
                            NgayTao = DateTime.Now,
                            NguoiCapNhat = currentUser,
                            NgayCapNhat = DateTime.Now,
                            TenGiaiDoanTuyChinh = phaseName
                        };
                        phaseRecord.IsNew = true;
                        phaseRecord.Save();
                    }
                    foreach (TblChiTietMau itemMau in lstChiTietMau)
                    {
                        TblChiTietMau rootTemplate = itemMau;
                        while (rootTemplate.IdCongViecCha.HasValue)
                            rootTemplate = templateById[rootTemplate.IdCongViecCha.Value];
                        Guid projectStageId = mapStageIds[rootTemplate.IdChiTietMau];
                        DateTime taskStartDate = projectStartDate;
                        int taskDuration = Math.Max(1, itemMau.ThoiHanNgay ?? 1);
                        DateTime taskEndDate = LichBieuChungManager.Instance.CalculateTaskEndDate(
                            taskStartDate,
                            taskDuration);
                        TblCongViec taskMoi = new TblCongViec
                        {
                            IdCongViec = mapIds[itemMau.IdChiTietMau],
                            IdDuAn = this.ProjectId,
                            IdCongViecCha = null,
                            IdCongViecPhuThuoc = null,
                            IdGiaiDoanDuAn = projectStageId,
                            MaCongViec = itemMau.MaCongViec,
                            TenCongViec = itemMau.TenCongViec,
                            MoTa = itemMau.MoTa,
                            ThoiHanNgay = itemMau.ThoiHanNgay,
                            NgayBatDau = taskStartDate,
                            NgayKetThuc = taskEndDate,
                            TrangThai = 0,
                            DaXoa = false,
                            NguoiTao = currentUser,
                            NgayTao = DateTime.Now,
                            NguoiCapNhat = currentUser,
                            NgayCapNhat = DateTime.Now
                        };
                        if (itemMau.IdCongViecCha.HasValue)
                            taskMoi.IdCongViecCha = mapIds[itemMau.IdCongViecCha.Value];
                        if (itemMau.IdCongViecPhuThuoc.HasValue &&
                            mapIds.ContainsKey(itemMau.IdCongViecPhuThuoc.Value))
                            taskMoi.IdCongViecPhuThuoc = mapIds[itemMau.IdCongViecPhuThuoc.Value];
                        taskMoi.IsNew = true;
                        taskMoi.Save();
                        createdCount++;
                    }
                    TblCongViec phase1 = TaskManager.Instance.FetchById(firstPhaseId);
                    if (phase1 != null)
                    {
                        phase1.NgayBatDau = projectStartDate;
                        bool hasChildren = TaskManager.Instance.CheckHasChildTasks(this.ProjectId, phase1);
                        if (!hasChildren)
                        {
                            int thoiHan = Math.Max(1, phase1.ThoiHanNgay ?? 1);
                            phase1.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(
                                projectStartDate,
                                thoiHan);
                            phase1.NgayCapNhat = DateTime.Now;
                            phase1.Save();
                        }
                        else
                        {
                            phase1.NgayCapNhat = DateTime.Now;
                            phase1.Save();
                            TaskManager.Instance.AutoSetFirstChildStartTime(
                                this.ProjectId,
                                phase1.IdCongViec,
                                projectStartDate,
                                true);
                        }
                        TaskManager.Instance.AutoSetDependentTime(
                            this.ProjectId,
                            phase1.IdCongViec,
                            true);
                    }
                    scope.Complete();
                }
                ShowNotify($"Đã áp dụng mẫu thành công: tạo {createdCount} công việc và tính toán thời gian.", MSGType.Success);
                mdlApplyTemplate.CloseModal();
                ApplySuccessCallback?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ShowNotify("Lỗi hệ thống khi áp dụng mẫu: " + ex.Message, MSGType.Error);
                upnlApplyTemplate.Update();
            }
        }

        protected string GetTaskIndent(object value)
        {
            string code = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(code)) return "0";
            int level = code.Split('.').Length;
            return Math.Max(0, (level - 1) * 20).ToString(); // Thụt lề theo cấp bậc công việc
        }
    }
}