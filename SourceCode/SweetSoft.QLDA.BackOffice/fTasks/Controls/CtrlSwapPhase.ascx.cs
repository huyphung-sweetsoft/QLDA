using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlSwapPhase : BaseAdminUserControl
    {
        public Action SwapSuccessCallback { get; set; }
        private Guid CurrentProjectId => ViewState["CurrentProjectId"] is Guid id ? id : Guid.Empty;
        private Guid ReorderPhaseId => ViewState["ReorderPhaseId"] is Guid id ? id : Guid.Empty;
        private Guid ReorderTargetPhaseId => ViewState["ReorderTargetPhaseId"] is Guid id ? id : Guid.Empty;
        private string ReorderDropPosition => Convert.ToString(ViewState["ReorderDropPosition"] ?? "before");
        private bool ReorderAutoUpdateDates
        {
            get => ViewState["ReorderAutoUpdateDates"] != null && (bool)ViewState["ReorderAutoUpdateDates"];
            set => ViewState["ReorderAutoUpdateDates"] = value;
        }
        private DateTime? ReorderPreviewAnchorStart
        {
            get => ViewState["ReorderPreviewAnchorStart"] is DateTime value ? value : (DateTime?)null;
            set => ViewState["ReorderPreviewAnchorStart"] = value;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        public void OpenModal(Guid projectId)
        {
            ViewState["CurrentProjectId"] = projectId;
            BindDropdown(ddlPhase1, string.Empty);
            BindDropdown(ddlPhase2, string.Empty);
            upSwap.Update();
            mdlSwapPhase.OpenModal(true);
        }
        public void OpenReorderOptions(Guid projectId, Guid draggedPhaseId, Guid targetPhaseId, string dropPosition)
        {
            if (projectId == Guid.Empty || draggedPhaseId == Guid.Empty || targetPhaseId == Guid.Empty)
                throw new InvalidOperationException("Dữ liệu đổi vị trí giai đoạn không hợp lệ.");
            ViewState["CurrentProjectId"] = projectId;
            ViewState["ReorderPhaseId"] = draggedPhaseId;
            ViewState["ReorderTargetPhaseId"] = targetPhaseId;
            ViewState["ReorderDropPosition"] = string.Equals(dropPosition, "after", StringComparison.OrdinalIgnoreCase) ? "after" : "before";
            ReorderAutoUpdateDates = false;
            ReorderPreviewAnchorStart = null;
            BindReorderPreview();
            mdlReorderReview.CloseModal();
            mdlReorderOptions.OpenModal(true);
            upReorderOptions.Update();
        }
        private void BindDropdown(DropDownList ddl, string excludeValue)
        {
            if (CurrentProjectId == Guid.Empty) return;
            string currentValue = ddl.SelectedValue;
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("-- Chọn giai đoạn --", ""));
            DataTable dt = TaskManager.Instance.FetchPhasesByProjectId(CurrentProjectId);
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string id = row["IdCongViec"].ToString().ToLower();
                    if (id != excludeValue)
                    {
                        string maCv = row["MaCongViec"]?.ToString() ?? "";
                        string tenCv = row["TenCongViec"]?.ToString() ?? "";
                        ddl.Items.Add(new ListItem($"[{maCv}] {tenCv}", id));
                    }
                }
            }
            if (ddl.Items.FindByValue(currentValue) != null)
                ddl.SelectedValue = currentValue;
        }
        protected void ddlPhase_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindDropdown(ddlPhase2, ddlPhase1.SelectedValue);
            BindDropdown(ddlPhase1, ddlPhase2.SelectedValue);
            upSwap.Update();
        }
        protected void btnConfirmSwap_Click(object sender, EventArgs e)
        {
            string idPhase1 = ddlPhase1.SelectedValue;
            string idPhase2 = ddlPhase2.SelectedValue;
            if (string.IsNullOrEmpty(idPhase1) || string.IsNullOrEmpty(idPhase2))
            {
                ShowNotify("Vui lòng chọn đủ 2 Giai đoạn để hoán đổi!", MSGType.Warning);
                return;
            }
            TblCongViec phase1 = TaskManager.Instance.FetchById(Guid.Parse(idPhase1));
            TblCongViec phase2 = TaskManager.Instance.FetchById(Guid.Parse(idPhase2));
            if (phase1 == null || phase2 == null) return;
            ConfirmResult result = new ConfirmResult { CommandName = "CONFIRM_SWAP_PHASES" };
            this.CURRENT_PAGE.CurrentConfirmResult = result;
            MessageBox msg = new MessageBox(
                GetResourceText(BackEndResourceKeys.NOTIFICATION),
                string.Format("Bạn có chắc chắn muốn hoán đổi vị trí và tiến độ của <b>[{0}]</b> và <b>[{1}]</b> không?", phase1.MaCongViec, phase2.MaCongViec),
                MSGButton.AcceptCancel,
                MSGIcon.Warning);
            OpenMessageBox(msg, result, false, false);
        }
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName == "CONFIRM_SWAP_PHASES")
                ExecuteSwapLogic();
        }
        private void ExecuteSwapLogic()
        {
            string idPhase1 = ddlPhase1.SelectedValue;
            string idPhase2 = ddlPhase2.SelectedValue;
            Guid p1 = Guid.Parse(idPhase1);
            Guid p2 = Guid.Parse(idPhase2);
            TblCongViec phase1 = TaskManager.Instance.FetchById(p1);
            TblCongViec phase2 = TaskManager.Instance.FetchById(p2);
            if (phase1 == null || phase2 == null) return;
            Guid projectId = phase1.IdDuAn;
            bool isPhase1Earlier = !TaskManager.Instance.IsAfterOrEqual(phase1.MaCongViec, phase2.MaCongViec);
            TblCongViec earlyPhase = isPhase1Earlier ? phase1 : phase2;
            TblCongViec latePhase = isPhase1Earlier ? phase2 : phase1;
            string codeEarly = earlyPhase.MaCongViec;
            string codeLate = latePhase.MaCongViec;
            DateTime? startDateEarly = earlyPhase.NgayBatDau;
            DataTable dtAllTasks = TaskManager.Instance.FetchByIdAndOrderASCMaCV(projectId);
            List<TblCongViec> earlyTasks = new List<TblCongViec>();
            List<TblCongViec> lateTasks = new List<TblCongViec>();
            foreach (DataRow row in dtAllTasks.Rows)
            {
                Guid tid = Guid.Parse(row["IdCongViec"].ToString());
                string tCode = row["MaCongViec"].ToString();
                if (tCode == codeEarly || tCode.StartsWith(codeEarly + ".")) earlyTasks.Add(TaskManager.Instance.FetchById(tid));
                else if (tCode == codeLate || tCode.StartsWith(codeLate + ".")) lateTasks.Add(TaskManager.Instance.FetchById(tid));
            }
            string tempPrefix = "SWAP_TEMP";
            foreach (var t in earlyTasks) { t.MaCongViec = t.MaCongViec == codeEarly ? tempPrefix : tempPrefix + t.MaCongViec.Substring(codeEarly.Length); t.Save(); }
            foreach (var t in lateTasks) { t.MaCongViec = t.MaCongViec == codeLate ? codeEarly : codeEarly + t.MaCongViec.Substring(codeLate.Length); t.Save(); }
            foreach (var t in earlyTasks) { t.MaCongViec = t.MaCongViec == tempPrefix ? codeLate : codeLate + t.MaCongViec.Substring(tempPrefix.Length); t.Save(); }
            Guid? depEarlyIn = earlyPhase.IdCongViecPhuThuoc;
            Guid? depLateIn = latePhase.IdCongViecPhuThuoc;
            if (depLateIn == earlyPhase.IdCongViec)
            {
                latePhase.IdCongViecPhuThuoc = depEarlyIn;
                earlyPhase.IdCongViecPhuThuoc = latePhase.IdCongViec;
            }
            else
            {
                latePhase.IdCongViecPhuThuoc = depEarlyIn;
                earlyPhase.IdCongViecPhuThuoc = depLateIn;
            }
            earlyPhase.Save();
            latePhase.Save();
            foreach (DataRow row in dtAllTasks.Rows)
            {
                Guid tid = Guid.Parse(row["IdCongViec"].ToString());
                if (tid == earlyPhase.IdCongViec || tid == latePhase.IdCongViec) continue;
                TblCongViec t = TaskManager.Instance.FetchById(tid);
                if (t == null) continue;
                bool changed = false;
                if (t.IdCongViecPhuThuoc == earlyPhase.IdCongViec) { t.IdCongViecPhuThuoc = latePhase.IdCongViec; changed = true; }
                else if (t.IdCongViecPhuThuoc == latePhase.IdCongViec) { t.IdCongViecPhuThuoc = earlyPhase.IdCongViec; changed = true; }
                if (changed) t.Save();
            }
            if (startDateEarly.HasValue)
            {
                latePhase.NgayBatDau = startDateEarly.Value;
                latePhase.NgayCapNhat = DateTime.Now;
                latePhase.Save();
                TaskManager.Instance.AutoSetFirstChildStartTime(projectId, latePhase.IdCongViec, startDateEarly.Value);
                TaskManager.Instance.AutoSetDependentTime(projectId, latePhase.IdCongViec);
            }
            var (minStartAllowed, _) = TaskManager.Instance.GetMinStartDate(earlyPhase.IdCongViecCha, earlyPhase.IdCongViecPhuThuoc);
            DateTime newEarlyStart = minStartAllowed ?? (startDateEarly ?? DateTime.Today);
            earlyPhase.NgayBatDau = newEarlyStart;
            earlyPhase.NgayCapNhat = DateTime.Now;
            earlyPhase.Save();
            TaskManager.Instance.AutoSetFirstChildStartTime(projectId, earlyPhase.IdCongViec, newEarlyStart);
            TaskManager.Instance.AutoSetDependentTime(projectId, earlyPhase.IdCongViec);
            mdlSwapPhase.CloseModal();
            SwapSuccessCallback?.Invoke();
            ShowNotify("Hoán đổi vị trí giai đoạn thành công!", MSGType.Success);
        }
        protected void btnToggleAutoUpdateDates_Click(object sender, EventArgs e)
        {
            ReorderAutoUpdateDates = !ReorderAutoUpdateDates;
            BindReorderPreview();
            mdlReorderOptions.OpenModal(true);
            upReorderOptions.Update();
        }
        protected void btnToggleAutoUpdateDatesReview_Click(object sender, EventArgs e)
        {
            ReorderAutoUpdateDates = !ReorderAutoUpdateDates;
            BindReorderPreview();
            mdlReorderReview.OpenModal(true);
            upReorderReview.Update();
        }
        protected void btnOpenReorderReview_Click(object sender, EventArgs e)
        {
            BindReorderPreview();
            mdlReorderOptions.CloseModal();
            mdlReorderReview.OpenModal(true);
            upReorderReview.Update();
        }
        protected void btnBackFromReview_Click(object sender, EventArgs e)
        {
            BindReorderPreview();
            mdlReorderReview.CloseModal();
            mdlReorderOptions.OpenModal(true);
            upReorderOptions.Update();
        }
        protected void btnCancelReorder_Click(object sender, EventArgs e)
        {
            mdlReorderOptions.CloseModal();
            mdlReorderReview.CloseModal();
        }
        protected void btnConfirmReorder_Click(object sender, EventArgs e)
        {
            ExecuteReorder();
        }
        protected void btnConfirmReorderFromReview_Click(object sender, EventArgs e)
        {
            ExecuteReorder();
        }
        private void ExecuteReorder()
        {
            if (ReorderPhaseId == Guid.Empty || ReorderTargetPhaseId == Guid.Empty || CurrentProjectId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }
            try
            {
                TaskManager.Instance.ReorderPhase(CurrentProjectId, ReorderPhaseId, ReorderTargetPhaseId, ReorderDropPosition, ReorderAutoUpdateDates, false);
                mdlReorderOptions.CloseModal();
                mdlReorderReview.CloseModal();
                SwapSuccessCallback?.Invoke();
                ShowNotify("Đã thay đổi vị trí giai đoạn thành công!", MSGType.Success);
            }
            catch (Exception ex)
            {
                ShowNotify(ex.Message, MSGType.Error);
            }
        }
        private void BindReorderPreview()
        {
            BindReorderSwitches();
            List<TblCongViec> oldPhases = GetOrderedPhases();
            if (oldPhases.Count == 0) return;
            int oldDraggedIndex = oldPhases.FindIndex(x => x.IdCongViec == ReorderPhaseId);
            int oldTargetIndex = oldPhases.FindIndex(x => x.IdCongViec == ReorderTargetPhaseId);
            if (oldDraggedIndex < 0 || oldTargetIndex < 0) return;
            List<TblCongViec> newPhases = BuildReorderedPhaseList(oldPhases, oldDraggedIndex, oldTargetIndex, ReorderDropPosition);
            int affectedStartIndex = Math.Min(oldDraggedIndex, oldTargetIndex);
            int affectedEndIndex = oldPhases.Count - 1;
            List<TblCongViec> affectedOldPhases = oldPhases.Skip(affectedStartIndex).Take(affectedEndIndex - affectedStartIndex + 1).ToList();
            ReorderPreviewAnchorStart = affectedOldPhases.Count > 0 && affectedOldPhases[0].NgayBatDau.HasValue
                ? affectedOldPhases[0].NgayBatDau.Value.Date
                : (DateTime?)null;
            string affectedCodes = string.Join("-", affectedOldPhases.Select(x => HttpUtility.HtmlEncode(x.MaCongViec)));
            ltrReorderWarning.Text = $"Thông tin của các công việc thuộc giai đoạn <strong>{affectedCodes}</strong> sẽ bị thay đổi.";
            DataTable dtAllTasks = TaskManager.Instance.FetchByIdAndOrderASCMaCV(CurrentProjectId, null);
            if (dtAllTasks == null || dtAllTasks.Rows.Count == 0)
            {
                ltrReorderBefore.Text = "<div class='reorder-empty'>Không có công việc bị ảnh hưởng.</div>";
                ltrReorderAfter.Text = "<div class='reorder-empty'>Không có công việc bị ảnh hưởng.</div>";
                ltrBeforeCount.Text = "0 công việc";
                ltrAfterCount.Text = "0 công việc";
                ltrReorderReviewScope.Text = "";
                return;
            }
            var allTasks = new Dictionary<Guid, TblCongViec>();
            foreach (DataRow row in dtAllTasks.Rows)
            {
                if (!Guid.TryParse(Convert.ToString(row["IdCongViec"]), out Guid id)) continue;
                TblCongViec task = TaskManager.Instance.FetchById(id);
                if (task != null && task.IdDuAn == CurrentProjectId)
                    allTasks[id] = task;
            }
            var affectedTaskIds = new HashSet<Guid>();
            var affectedTaskGroups = new Dictionary<Guid, List<TblCongViec>>();
            foreach (TblCongViec phase in affectedOldPhases)
            {
                List<TblCongViec> group = allTasks.Values
                    .Where(t => !string.IsNullOrEmpty(t.MaCongViec) && (t.MaCongViec == phase.MaCongViec || t.MaCongViec.StartsWith(phase.MaCongViec + ".", StringComparison.Ordinal)))
                    .OrderBy(t => t.MaCongViec, Comparer<string>.Create(CompareWbsCodes))
                    .ToList();
                affectedTaskGroups[phase.IdCongViec] = group;
                foreach (TblCongViec task in group)
                    affectedTaskIds.Add(task.IdCongViec);
            }
            var newCodeByTaskId = new Dictionary<Guid, string>();
            foreach (TblCongViec phase in affectedOldPhases)
            {
                int newPhaseIndex = newPhases.FindIndex(x => x.IdCongViec == phase.IdCongViec);
                string newRootCode = (newPhaseIndex + 1).ToString();
                foreach (TblCongViec task in affectedTaskGroups[phase.IdCongViec])
                {
                    string suffix = task.MaCongViec.Length > phase.MaCongViec.Length ? task.MaCongViec.Substring(phase.MaCongViec.Length) : "";
                    newCodeByTaskId[task.IdCongViec] = newRootCode + suffix;
                }
            }
            List<TblCongViec> beforeTasks = new List<TblCongViec>();
            foreach (TblCongViec phase in affectedOldPhases)
                beforeTasks.AddRange(affectedTaskGroups[phase.IdCongViec]);
            List<TblCongViec> afterTasks = new List<TblCongViec>();
            foreach (TblCongViec phase in newPhases)
            {
                if (affectedTaskGroups.ContainsKey(phase.IdCongViec))
                    afterTasks.AddRange(affectedTaskGroups[phase.IdCongViec]);
            }

            Dictionary<Guid, TblCongViec> afterPreviewMap = BuildAfterPreviewMap(
                affectedOldPhases,
                newPhases,
                affectedStartIndex,
                beforeTasks,
                newCodeByTaskId,
                allTasks,
                ReorderPreviewAnchorStart);

            ltrReorderOrder.Text = BuildPhaseOrderHtml(
                newPhases,
                afterPreviewMap);
            ltrReorderBefore.Text = BuildTaskPreviewHtml(
                beforeTasks,
                false,
                newCodeByTaskId,
                allTasks,
                oldPhases,
                newPhases,
                null);
            ltrReorderAfter.Text = BuildTaskPreviewHtml(
                afterTasks,
                true,
                newCodeByTaskId,
                allTasks,
                oldPhases,
                newPhases,
                afterPreviewMap);
            ltrBeforeCount.Text = beforeTasks.Count + " công việc";
            ltrAfterCount.Text = afterTasks.Count + " công việc";
            string oldCodes = string.Join(" - ", affectedOldPhases.Select(x => $"[{HttpUtility.HtmlEncode(x.MaCongViec)}]"));
            ltrReorderReviewScope.Text = $"<span style='display:inline-flex;align-items:center;gap:6px;background:#f5f3ff;color:#5b21b6;border:1px solid #ddd6fe;border-radius:999px;padding:5px 9px;font-size:11px;font-weight:700;white-space:nowrap;'><i class='fas fa-layer-group'></i>{oldCodes}</span>";
        }
        private void BindReorderSwitches()
        {
            ltrAutoUpdateSwitch.Text = BuildSwitchHtml(ReorderAutoUpdateDates);
            ltrAutoUpdateSwitchReview.Text = BuildSwitchHtml(ReorderAutoUpdateDates);
        }
        private string BuildSwitchHtml(bool isOn)
        {
            string state = isOn ? "ON" : "OFF";
            string css = isOn ? "on" : "off";
            return $"<span class='reorder-switch {css}' aria-label='{state}'><span class='reorder-switch-thumb'></span><span class='reorder-switch-state'>{state}</span></span>";
        }
        private List<TblCongViec> BuildReorderedPhaseList(List<TblCongViec> oldPhases, int oldDraggedIndex, int oldTargetIndex, string dropPosition)
        {
            List<TblCongViec> newPhases = oldPhases.ToList();
            TblCongViec moved = newPhases[oldDraggedIndex];
            newPhases.RemoveAt(oldDraggedIndex);
            int newTargetIndex = oldTargetIndex;
            if (oldDraggedIndex < oldTargetIndex) newTargetIndex--;
            if (string.Equals(dropPosition, "after", StringComparison.OrdinalIgnoreCase)) newTargetIndex++;
            newTargetIndex = Math.Max(0, Math.Min(newTargetIndex, newPhases.Count));
            newPhases.Insert(newTargetIndex, moved);
            return newPhases;
        }
        private string BuildPhaseOrderHtml(
            List<TblCongViec> newPhases,
            Dictionary<Guid, TblCongViec> afterPreviewMap)
        {
            if (newPhases == null || newPhases.Count == 0)
                return "<div class='reorder-empty'>Không có giai đoạn để hiển thị.</div>";
            StringBuilder html = new StringBuilder();
            html.Append("<table class='reorder-table'><colgroup><col style='width:38%'><col style='width:11%'><col style='width:22%'><col style='width:16%'><col style='width:13%'></colgroup><thead><tr><th>Công việc</th><th>Thời hạn</th><th>Ngày bắt đầu - kết thúc</th><th>Trạng thái</th><th>Phụ thuộc</th></tr></thead><tbody>");
            for (int i = 0; i < newPhases.Count; i++)
            {
                TblCongViec phase = newPhases[i];
                TblCongViec displayPhase =
                    afterPreviewMap != null && afterPreviewMap.ContainsKey(phase.IdCongViec)
                        ? afterPreviewMap[phase.IdCongViec]
                        : phase;
                string newCode = (i + 1).ToString();
                string dependency = i > 0 ? i.ToString() : "—";
                string duration = displayPhase.ThoiHanNgay.HasValue ? displayPhase.ThoiHanNgay.Value + " ngày" : "—";
                string dateRange = FormatPreviewDateRange(displayPhase.NgayBatDau, displayPhase.NgayKetThuc);
                string statusText = GetPreviewStatusText(displayPhase);
                string statusCssClass = GetPreviewStatusCssClass(displayPhase, false);
                html.Append("<tr class='phase-row'>");
                html.AppendFormat("<td class='task-cell'><div class='reorder-task-main'><span class='reorder-task-code'>{0}</span><span class='reorder-task-name'>{1}</span></div></td>", HttpUtility.HtmlEncode(newCode), HttpUtility.HtmlEncode(phase.TenCongViec));
                html.AppendFormat("<td class='reorder-center'>{0}</td>", HttpUtility.HtmlEncode(duration));
                html.AppendFormat("<td class='reorder-date'>{0}</td>", dateRange);
                html.AppendFormat("<td class='reorder-status'><span class='reorder-status-badge {0}'>{1}</span></td>", statusCssClass, HttpUtility.HtmlEncode(statusText));
                html.AppendFormat("<td class='reorder-dependency'>{0}</td>", dependency == "—" ? "<span class='reorder-dependency-empty'>—</span>" : HttpUtility.HtmlEncode(dependency));
                html.Append("</tr>");
            }
            html.Append("</tbody></table>");
            return html.ToString();
        }
        private Dictionary<Guid, TblCongViec> BuildAfterPreviewMap(
            List<TblCongViec> affectedOldPhases,
            List<TblCongViec> newPhases,
            int affectedStartIndex,
            List<TblCongViec> beforeTasks,
            Dictionary<Guid, string> newCodeByTaskId,
            Dictionary<Guid, TblCongViec> allTasks,
            DateTime? anchorStart)
        {
            var result = new Dictionary<Guid, TblCongViec>();
            if (beforeTasks == null || beforeTasks.Count == 0)
                return result;

            if (ReorderAutoUpdateDates && anchorStart.HasValue)
            {
                result = BuildAutoUpdatePreviewMap(
                    affectedOldPhases,
                    newPhases,
                    affectedStartIndex,
                    beforeTasks,
                    allTasks,
                    newCodeByTaskId,
                    anchorStart.Value.Date);
                return result;
            }

            Guid firstAffectedPhaseId = Guid.Empty;
            if (newPhases != null && affectedStartIndex >= 0 && affectedStartIndex < newPhases.Count)
                firstAffectedPhaseId = newPhases[affectedStartIndex].IdCongViec;

            foreach (TblCongViec source in beforeTasks)
            {
                TblCongViec snapshot = ClonePreviewTask(source);
                if (newCodeByTaskId != null && newCodeByTaskId.ContainsKey(source.IdCongViec))
                    snapshot.MaCongViec = newCodeByTaskId[source.IdCongViec];

                snapshot.NgayBatDau = null;
                snapshot.NgayKetThuc = null;
                if (source.IdCongViec == firstAffectedPhaseId && anchorStart.HasValue)
                    snapshot.NgayBatDau = anchorStart.Value.Date;

                snapshot.TrangThai = 0;
                snapshot.NgayHoanThanhThucTe = null;

                result[snapshot.IdCongViec] = snapshot;
            }

            return result;
        }

        private Dictionary<Guid, TblCongViec> BuildAutoUpdatePreviewMap(
            List<TblCongViec> affectedOldPhases,
            List<TblCongViec> newPhases,
            int affectedStartIndex,
            List<TblCongViec> beforeTasks,
            Dictionary<Guid, TblCongViec> allTasks,
            Dictionary<Guid, string> newCodeByTaskId,
            DateTime anchorStart)
        {
            var previewTasks = new Dictionary<Guid, TblCongViec>();
            if (allTasks == null || allTasks.Count == 0)
                return previewTasks;

            foreach (KeyValuePair<Guid, TblCongViec> item in allTasks)
            {
                TblCongViec snapshot = ClonePreviewTask(item.Value);
                if (newCodeByTaskId != null && newCodeByTaskId.ContainsKey(snapshot.IdCongViec))
                    snapshot.MaCongViec = newCodeByTaskId[snapshot.IdCongViec];
                previewTasks[snapshot.IdCongViec] = snapshot;
            }

            var affectedPhaseIds = new HashSet<Guid>(
                affectedOldPhases.Select(x => x.IdCongViec));

            foreach (TblCongViec task in previewTasks.Values)
            {
                TblCongViec root = GetPreviewRootTask(task, previewTasks);
                if (root != null && affectedPhaseIds.Contains(root.IdCongViec))
                {
                    task.TrangThai = 0;
                    task.NgayHoanThanhThucTe = null;
                }
            }

            // 1. Trong từ điển mô phỏng, thay dependency của các Phase bị ảnh hưởng
            //    theo thứ tự mới. Dependency nội bộ của task con không thay đổi.
            int previewStartIndex = Math.Max(0, affectedStartIndex);
            int previewEndIndex = newPhases != null ? newPhases.Count - 1 : -1;
            if (newPhases != null)
            {
                for (int i = previewStartIndex; i < newPhases.Count; i++)
                {
                    if (!affectedPhaseIds.Contains(newPhases[i].IdCongViec))
                    {
                        previewEndIndex = i - 1;
                        break;
                    }
                }
            }

            if (previewEndIndex < previewStartIndex)
                return FilterPreviewTasks(previewTasks, beforeTasks);

            for (int i = previewStartIndex; i <= previewEndIndex; i++)
            {
                TblCongViec phase;
                if (!previewTasks.TryGetValue(newPhases[i].IdCongViec, out phase))
                    continue;
                phase.IdCongViecPhuThuoc = i > 0
                    ? newPhases[i - 1].IdCongViec
                    : (Guid?)null;
            }

            // 2. Mô phỏng lịch hoàn toàn trên Dictionary.
            //    Phase đầu vùng ảnh hưởng dùng đúng anchor cũ.
            //    Các Phase sau nối tiếp theo ngày kết thúc của Phase trước.
            DateTime currentStart = anchorStart.Date;
            for (int i = previewStartIndex; i <= previewEndIndex; i++)
            {
                TblCongViec phase;
                if (!previewTasks.TryGetValue(newPhases[i].IdCongViec, out phase))
                    continue;

                SetPreviewTaskStartAndChildren(
                    phase.IdCongViec,
                    currentStart,
                    previewTasks);

                ResolvePreviewInternalDependencies(
                    phase.IdCongViec,
                    previewTasks);

                UpdateParentPreview(
                    phase.IdCongViec,
                    previewTasks,
                    new HashSet<Guid>());

                phase = previewTasks[phase.IdCongViec];
                if (!phase.NgayKetThuc.HasValue)
                    break;

                currentStart = phase.NgayKetThuc.Value.Date.AddDays(1);
            }

            return FilterPreviewTasks(previewTasks, beforeTasks);
        }

        private Dictionary<Guid, TblCongViec> FilterPreviewTasks(
            Dictionary<Guid, TblCongViec> previewTasks,
            List<TblCongViec> beforeTasks)
        {
            if (previewTasks == null)
                return new Dictionary<Guid, TblCongViec>();
            if (beforeTasks == null || beforeTasks.Count == 0)
                return previewTasks;

            var displayedIds = new HashSet<Guid>(beforeTasks.Select(x => x.IdCongViec));
            return previewTasks
                .Where(x => displayedIds.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private TblCongViec GetPreviewRootTask(
            TblCongViec task,
            Dictionary<Guid, TblCongViec> tasks)
        {
            if (task == null)
                return null;

            TblCongViec current = task;
            var visited = new HashSet<Guid>();
            while (current.IdCongViecCha.HasValue && current.IdCongViecCha.Value != Guid.Empty)
            {
                if (!visited.Add(current.IdCongViec))
                    return null;
                TblCongViec parent;
                if (!tasks.TryGetValue(current.IdCongViecCha.Value, out parent))
                    return null;
                current = parent;
            }
            return current;
        }

        private void SetPreviewTaskStartAndChildren(
            Guid taskId,
            DateTime startDate,
            Dictionary<Guid, TblCongViec> tasks)
        {
            TblCongViec task;
            if (!tasks.TryGetValue(taskId, out task))
                return;

            task.NgayBatDau = startDate.Date;

            List<TblCongViec> children = GetPreviewChildren(taskId, tasks);
            if (children.Count == 0)
            {
                int duration = GetPreviewTaskDuration(task);
                task.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(
                    task.NgayBatDau.Value,
                    duration);
                task.ThoiHanNgay = duration;
                return;
            }

            SetFirstChildChainPreview(taskId, startDate.Date, tasks);
            UpdateParentPreview(taskId, tasks, new HashSet<Guid>());
        }

        private void SetFirstChildChainPreview(
            Guid parentId,
            DateTime startDate,
            Dictionary<Guid, TblCongViec> tasks)
        {
            TblCongViec firstChild = GetPreviewChildren(parentId, tasks)
                .OrderBy(x => x.MaCongViec, Comparer<string>.Create(CompareWbsCodes))
                .FirstOrDefault();

            if (firstChild == null)
            {
                TblCongViec parent;
                if (!tasks.TryGetValue(parentId, out parent))
                    return;
                int duration = GetPreviewTaskDuration(parent);
                parent.NgayBatDau = startDate.Date;
                parent.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(
                    parent.NgayBatDau.Value,
                    duration);
                parent.ThoiHanNgay = duration;
                return;
            }

            firstChild.NgayBatDau = startDate.Date;
            List<TblCongViec> grandChildren = GetPreviewChildren(firstChild.IdCongViec, tasks);
            if (grandChildren.Count > 0)
            {
                SetFirstChildChainPreview(firstChild.IdCongViec, startDate.Date, tasks);
                UpdateParentPreview(firstChild.IdCongViec, tasks, new HashSet<Guid>());
            }
            else
            {
                int duration = GetPreviewTaskDuration(firstChild);
                firstChild.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(
                    firstChild.NgayBatDau.Value,
                    duration);
                firstChild.ThoiHanNgay = duration;
            }
        }

        private void ResolvePreviewInternalDependencies(
            Guid rootPhaseId,
            Dictionary<Guid, TblCongViec> tasks)
        {
            const int maxPasses = 100;
            for (int pass = 0; pass < maxPasses; pass++)
            {
                bool changed = false;
                List<TblCongViec> phaseTasks = GetPreviewTasksForRoot(rootPhaseId, tasks)
                    .Where(x => x.IdCongViec != rootPhaseId &&
                                x.IdCongViecPhuThuoc.HasValue &&
                                x.IdCongViecPhuThuoc.Value != Guid.Empty)
                    .OrderBy(x => x.MaCongViec, Comparer<string>.Create(CompareWbsCodes))
                    .ToList();

                foreach (TblCongViec task in phaseTasks)
                {
                    TblCongViec dependency;
                    if (!tasks.TryGetValue(task.IdCongViecPhuThuoc.Value, out dependency))
                        continue;
                    TblCongViec dependencyRoot = GetPreviewRootTask(dependency, tasks);
                    if (dependencyRoot == null || dependencyRoot.IdCongViec != rootPhaseId)
                        continue;
                    if (!dependency.NgayKetThuc.HasValue)
                        continue;

                    DateTime expectedStart = dependency.NgayKetThuc.Value.Date.AddDays(1);
                    if (!task.NgayBatDau.HasValue || task.NgayBatDau.Value.Date != expectedStart.Date)
                    {
                        SetPreviewTaskStartAndChildren(task.IdCongViec, expectedStart, tasks);
                        changed = true;
                    }
                }

                UpdateParentPreview(rootPhaseId, tasks, new HashSet<Guid>());
                if (!changed)
                    break;
            }
        }

        private List<TblCongViec> GetPreviewTasksForRoot(
            Guid rootPhaseId,
            Dictionary<Guid, TblCongViec> tasks)
        {
            return tasks.Values
                .Where(x =>
                {
                    TblCongViec root = GetPreviewRootTask(x, tasks);
                    return root != null && root.IdCongViec == rootPhaseId;
                })
                .ToList();
        }

        private List<TblCongViec> GetPreviewChildren(
            Guid parentId,
            Dictionary<Guid, TblCongViec> tasks)
        {
            return tasks.Values
                .Where(x => x.IdCongViecCha.HasValue && x.IdCongViecCha.Value == parentId)
                .ToList();
        }

        private int GetPreviewTaskDuration(TblCongViec task)
        {
            if (task == null)
                return 1;
            if (task.ThoiHanNgay.HasValue && task.ThoiHanNgay.Value > 0)
                return task.ThoiHanNgay.Value;
            if (task.NgayBatDau.HasValue && task.NgayKetThuc.HasValue)
            {
                int duration = (task.NgayKetThuc.Value.Date - task.NgayBatDau.Value.Date).Days + 1;
                return Math.Max(1, duration);
            }
            return 1;
        }

        private void UpdateParentPreview(
            Guid parentId,
            Dictionary<Guid, TblCongViec> tasks,
            HashSet<Guid> visited)
        {
            if (!visited.Add(parentId))
                return;

            TblCongViec parent;
            if (!tasks.TryGetValue(parentId, out parent))
                return;

            List<TblCongViec> children = GetPreviewChildren(parentId, tasks);
            if (children.Count == 0)
            {
                if (parent.NgayBatDau.HasValue)
                {
                    int duration = GetPreviewTaskDuration(parent);
                    parent.NgayKetThuc = LichBieuChungManager.Instance.CalculateTaskEndDate(
                        parent.NgayBatDau.Value,
                        duration);
                    parent.ThoiHanNgay = duration;
                }
                return;
            }

            foreach (TblCongViec child in children)
                UpdateParentPreview(child.IdCongViec, tasks, visited);

            DateTime? maxEnd = children
                .Where(x => x.NgayKetThuc.HasValue)
                .Select(x => (DateTime?)x.NgayKetThuc.Value)
                .OrderByDescending(x => x.Value)
                .FirstOrDefault();

            if (!maxEnd.HasValue || !parent.NgayBatDau.HasValue)
                return;

            parent.NgayKetThuc = maxEnd.Value.Date;
            parent.ThoiHanNgay = LichBieuChungManager.Instance.CountWorkingDaysInRange(
                parent.NgayBatDau.Value.Date,
                parent.NgayKetThuc.Value.Date);
        }

        private TblCongViec ClonePreviewTask(TblCongViec source)
        {
            var snapshot = new TblCongViec();
            snapshot.IdCongViec = source.IdCongViec;
            snapshot.IdDuAn = source.IdDuAn;
            snapshot.IdCongViecCha = source.IdCongViecCha;
            snapshot.IdCongViecPhuThuoc = source.IdCongViecPhuThuoc;
            snapshot.MaCongViec = source.MaCongViec;
            snapshot.TenCongViec = source.TenCongViec;
            snapshot.ThoiHanNgay = source.ThoiHanNgay;
            snapshot.NgayBatDau = source.NgayBatDau;
            snapshot.NgayKetThuc = source.NgayKetThuc;
            snapshot.TrangThai = source.TrangThai;
            snapshot.NgayHoanThanhThucTe = source.NgayHoanThanhThucTe;
            return snapshot;
        }

        private string BuildTaskPreviewHtml(
            List<TblCongViec> tasks,
            bool after,
            Dictionary<Guid, string> newCodeByTaskId,
            Dictionary<Guid, TblCongViec> allTasks,
            List<TblCongViec> oldPhases,
            List<TblCongViec> newPhases,
            Dictionary<Guid, TblCongViec> afterPreviewMap)
        {
            if (tasks == null || tasks.Count == 0)
                return "<div class='reorder-empty'>Không có công việc bị ảnh hưởng.</div>";
            StringBuilder html = new StringBuilder();
            html.Append("<table class='reorder-table'><colgroup><col style='width:34%'><col style='width:11%'><col style='width:22%'><col style='width:17%'><col style='width:16%'></colgroup><thead><tr><th>Công việc</th><th>Thời hạn</th><th>Ngày bắt đầu - kết thúc</th><th>Trạng thái</th><th>Phụ thuộc</th></tr></thead><tbody>");
            int delay = 0;
            foreach (TblCongViec task in tasks)
            {
                TblCongViec displayTask =
                    after && afterPreviewMap != null && afterPreviewMap.ContainsKey(task.IdCongViec)
                        ? afterPreviewMap[task.IdCongViec]
                        : task;
                string code = after && newCodeByTaskId.ContainsKey(task.IdCongViec) ? newCodeByTaskId[task.IdCongViec] : task.MaCongViec;
                int level = string.IsNullOrWhiteSpace(code) ? 1 : code.Split('.').Length;
                bool isPhase = level == 1;
                string dependency = GetPreviewDependency(task, after, newCodeByTaskId, allTasks, oldPhases, newPhases);
                string statusText = GetPreviewStatusText(displayTask, !after);
                string statusCssClass = GetPreviewStatusCssClass(displayTask, !after);
                string duration = displayTask.ThoiHanNgay.HasValue ? displayTask.ThoiHanNgay.Value + " ngày" : "—";
                html.AppendFormat("<tr class='{0}' style='animation-delay:{1}ms;'>", isPhase ? "phase-row" : "child-row", delay);
                html.AppendFormat("<td class='task-cell' style='padding-left:{0}px;'><div class='reorder-task-main'><span class='reorder-task-code'>{1}</span><span class='reorder-task-name'>{2}</span></div></td>", isPhase ? 10 : Math.Min(10 + (level - 1) * 16, 58), HttpUtility.HtmlEncode(code), HttpUtility.HtmlEncode(displayTask.TenCongViec));
                html.AppendFormat("<td class='reorder-center'>{0}</td>", HttpUtility.HtmlEncode(duration));
                html.AppendFormat("<td class='reorder-date'>{0}</td>", FormatPreviewDateRange(displayTask.NgayBatDau, displayTask.NgayKetThuc));
                html.AppendFormat("<td class='reorder-status'><span class='reorder-status-badge {0}'>{1}</span></td>", statusCssClass, HttpUtility.HtmlEncode(statusText));
                html.AppendFormat("<td class='reorder-dependency'>{0}</td>", dependency == "—" ? "<span class='reorder-dependency-empty'>—</span>" : HttpUtility.HtmlEncode(dependency));
                html.Append("</tr>");
                delay = Math.Min(delay + 6, 150);
            }
            html.Append("</tbody></table>");
            return html.ToString();
        }
        private string FormatPreviewDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue && !endDate.HasValue)
                return "<span class='reorder-dependency-empty'>—</span>";
            if (startDate.HasValue && endDate.HasValue)
                return $"<strong>{startDate.Value:dd/MM/yyyy}</strong> - <strong>{endDate.Value:dd/MM/yyyy}</strong>";
            if (startDate.HasValue)
                return $"<strong>{startDate.Value:dd/MM/yyyy}</strong> - <span class='reorder-dependency-empty'>—</span>";
            return $"<span class='reorder-dependency-empty'>—</span> - <strong>{endDate.Value:dd/MM/yyyy}</strong>";
        }
        private string GetPreviewStatusCssClass(TblCongViec task, bool before = false)
        {
            if (task == null)
                return "status-todo";
            int status = before ? task.TrangThai : 0;
            switch (status)
            {
                case 1:
                    return "status-doing";
                case 2:
                case 3:
                    return "status-done";
                case 0:
                default:
                    return "status-todo";
            }
        }
        private string GetPreviewStatusText(TblCongViec task, bool before = false)
        {
            if (task == null)
                return "—";
            if (!before)
                return "Chưa bắt đầu";
            try
            {
                int status = task.TrangThai;
                return GetResourceText(TaskManager.Instance.GetValueForTrangThaiCongViec((TrangThaiCongViec)status)) ?? "—";
            }
            catch
            {
                switch (task.TrangThai)
                {
                    case 1: return "Đang thực hiện";
                    case 2: return "Hoàn thành";
                    case 3: return "Hoàn thành trễ";
                    default: return "Chưa bắt đầu";
                }
            }
        }
        private string GetPreviewDependency(TblCongViec task, bool after, Dictionary<Guid, string> newCodeByTaskId, Dictionary<Guid, TblCongViec> allTasks, List<TblCongViec> oldPhases, List<TblCongViec> newPhases)
        {
            if (!after)
            {
                if (!task.IdCongViecPhuThuoc.HasValue || task.IdCongViecPhuThuoc.Value == Guid.Empty)
                    return "—";
                return GetTaskCodeById(task.IdCongViecPhuThuoc.Value, allTasks);
            }
            bool isRootPhase = string.IsNullOrWhiteSpace(task.MaCongViec) || !task.IdCongViecCha.HasValue;
            if (isRootPhase && oldPhases.Any(x => x.IdCongViec == task.IdCongViec))
            {
                int newIndex = newPhases.FindIndex(x => x.IdCongViec == task.IdCongViec);
                return newIndex > 0 ? newIndex.ToString() : "—";
            }
            if (!task.IdCongViecPhuThuoc.HasValue || task.IdCongViecPhuThuoc.Value == Guid.Empty)
                return "—";
            if (newCodeByTaskId.ContainsKey(task.IdCongViecPhuThuoc.Value))
                return newCodeByTaskId[task.IdCongViecPhuThuoc.Value];
            return GetTaskCodeById(task.IdCongViecPhuThuoc.Value, allTasks);
        }
        private string GetTaskCodeById(Guid taskId, Dictionary<Guid, TblCongViec> allTasks)
        {
            if (taskId == Guid.Empty)
                return "—";
            if (allTasks.ContainsKey(taskId))
                return string.IsNullOrWhiteSpace(allTasks[taskId].MaCongViec) ? "—" : allTasks[taskId].MaCongViec;
            return "—";
        }
        private List<TblCongViec> GetOrderedPhases()
        {
            DataTable dt = TaskManager.Instance.FetchPhasesByProjectId(CurrentProjectId);
            var result = new List<TblCongViec>();
            if (dt == null) return result;
            foreach (DataRow row in dt.Rows)
            {
                if (!Guid.TryParse(Convert.ToString(row["IdCongViec"]), out Guid id)) continue;
                TblCongViec phase = TaskManager.Instance.FetchById(id);
                if (phase != null && phase.IdDuAn == CurrentProjectId && !phase.IdCongViecCha.HasValue)
                    result.Add(phase);
            }
            return result.OrderBy(x => x.MaCongViec, Comparer<string>.Create(CompareWbsCodes)).ToList();
        }
        private int CompareWbsCodes(string codeA, string codeB)
        {
            if (string.Equals(codeA, codeB, StringComparison.OrdinalIgnoreCase)) return 0;
            string[] a = (codeA ?? "").Split('.');
            string[] b = (codeB ?? "").Split('.');
            int len = Math.Min(a.Length, b.Length);
            for (int i = 0; i < len; i++)
            {
                int va = int.TryParse(a[i], out int parsedA) ? parsedA : int.MaxValue;
                int vb = int.TryParse(b[i], out int parsedB) ? parsedB : int.MaxValue;
                int cmp = va.CompareTo(vb);
                if (cmp != 0) return cmp;
            }
            int lengthCompare = a.Length.CompareTo(b.Length);
            return lengthCompare != 0 ? lengthCompare : StringComparer.OrdinalIgnoreCase.Compare(codeA, codeB);
        }
    }
}
