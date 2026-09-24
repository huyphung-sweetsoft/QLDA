using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlSwapPhase : BaseAdminUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public Action SwapSuccessCallback { get; set; }

        public void OpenModal(Guid projectId)
        {
            ViewState["CurrentProjectId"] = projectId;

            BindDropdown(ddlPhase1, string.Empty);
            BindDropdown(ddlPhase2, string.Empty);

            upSwap.Update();
            mdlSwapPhase.OpenModal(true);
        }

        private void BindDropdown(DropDownList ddl, string excludeValue)
        {
            if (ViewState["CurrentProjectId"] == null) return;
            Guid projId = (Guid)ViewState["CurrentProjectId"];

            string currentValue = ddl.SelectedValue;

            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("-- Chọn giai đoạn --", ""));

            DataTable dt = TaskManager.Instance.FetchPhasesByProjectId(projId);
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
            {
                ddl.SelectedValue = currentValue;
            }
        }

        protected void ddlPhase_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindDropdown(ddlPhase2, ddlPhase1.SelectedValue);
            BindDropdown(ddlPhase1, ddlPhase2.SelectedValue);
            upSwap.Update();
        }

        // ==========================================
        // 1. KHI BẤM NÚT -> GỌI MESSAGE BOX
        // ==========================================
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

            // Khởi tạo ConfirmResult
            ConfirmResult result = new ConfirmResult { CommandName = "CONFIRM_SWAP_PHASES" };
            this.CURRENT_PAGE.CurrentConfirmResult = result;

            // Gọi MessageBox của hệ thống
            MessageBox msg = new MessageBox(
                GetResourceText(BackEndResourceKeys.NOTIFICATION),
                string.Format("Bạn có chắc chắn muốn hoán đổi vị trí và tiến độ của <b>[{0}]</b> và <b>[{1}]</b> không?", phase1.MaCongViec, phase2.MaCongViec),
                MSGButton.AcceptCancel,
                MSGIcon.Warning
            );

            OpenMessageBox(msg, result, false, false);
        }

        // ==========================================
        // 2. KHI BẤM "ACCEPT" TRÊN MESSAGE BOX
        // ==========================================
        public override void ConfirmRequest(ConfirmResult e)
        {
            if (e != null && e.Submit && e.CommandName == "CONFIRM_SWAP_PHASES")
            {
                ExecuteSwapLogic();
            }
        }

        // ==========================================
        // 3. THỰC THI HOÁN ĐỔI LOGIC (GANTT)
        // ==========================================
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

                if (tCode == codeEarly || tCode.StartsWith(codeEarly + "."))
                    earlyTasks.Add(TaskManager.Instance.FetchById(tid));
                else if (tCode == codeLate || tCode.StartsWith(codeLate + "."))
                    lateTasks.Add(TaskManager.Instance.FetchById(tid));
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
                if (t != null)
                {
                    bool changed = false;
                    if (t.IdCongViecPhuThuoc == earlyPhase.IdCongViec)
                    {
                        t.IdCongViecPhuThuoc = latePhase.IdCongViec;
                        changed = true;
                    }
                    else if (t.IdCongViecPhuThuoc == latePhase.IdCongViec)
                    {
                        t.IdCongViecPhuThuoc = earlyPhase.IdCongViec;
                        changed = true;
                    }

                    if (changed) t.Save();
                }
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

            if (SwapSuccessCallback != null)
            {
                SwapSuccessCallback.Invoke();
            }
            ShowNotify("Hoán đổi vị trí giai đoạn thành công!", MSGType.Success);
        }
    }
}