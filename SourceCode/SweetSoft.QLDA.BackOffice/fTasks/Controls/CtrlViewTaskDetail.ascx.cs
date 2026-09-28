using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;

namespace SweetSoft.QLDA.BackOffice.fTasks.Controls
{
    public partial class CtrlViewTaskDetail : BaseAdminUserControl
    {
        private readonly ControlHelpers _controlHelpers = new ControlHelpers();

        public void OpenModal(Guid projectId, Guid taskId)
        {
            TblCongViec task = TaskManager.Instance.FetchById(taskId);
            if (task == null || task.DaXoa == true) return;

            bool isPhase = !task.IdCongViecCha.HasValue;
            mdlViewTask.Title = isPhase ? "Chi tiết Giai đoạn" : "Chi tiết Công việc con";

            // 1. Header
            ltrMaCV.Text = $"MÃ CV: {task.MaCongViec}";
            ltrTenCV.Text = task.TenCongViec;

            string phaseName = TaskManager.Instance.GetRootPhaseName(projectId, task.IdCongViecCha);
            ltrGiaiDoan.Text = string.IsNullOrEmpty(phaseName) ? "Giai đoạn gốc" : $"Thuộc Giai đoạn: <strong>{phaseName}</strong>";

            // Ưu tiên (Đưa lên header)
            if (task.IdDoUuTien.HasValue)
            {
                var priority = TaskManager.Instance.GetPriorityById(task.IdDoUuTien.Value);
                ltrDoUuTien.Text = priority != null ? _controlHelpers.GetTaskPriorityBadge(priority.TenDoUuTien, priority.DiemUuTien) : "<span class='empty-val'>—</span>";
            }
            else ltrDoUuTien.Text = "<span class='empty-val'>—</span>";

            // 2. Thời gian & Công việc cha (Hàng 1)
            ltrNgayBatDau.Text = task.NgayBatDau.HasValue ? task.NgayBatDau.Value.ToString("dd/MM/yyyy") : "<span class='empty-val'>—</span>";

            if (task.IdCongViecCha.HasValue)
            {
                var parent = TaskManager.Instance.FetchById(task.IdCongViecCha.Value);
                ltrCongViecCha.Text = parent != null ? $"[{parent.MaCongViec}] {parent.TenCongViec}" : "<span class='empty-val'>—</span>";
            }
            else ltrCongViecCha.Text = "<span class='empty-val text-muted'>Lớp gốc (Không có cha)</span>";

            // 3. Thời hạn & Phụ thuộc (Hàng 2)
            ltrThoiHan.Text = task.ThoiHanNgay.HasValue ? $"<strong style='font-size:18px;'>{task.ThoiHanNgay.Value}</strong> ngày" : "<span class='empty-val'>—</span>";

            if (task.IdCongViecPhuThuoc.HasValue)
            {
                var dep = TaskManager.Instance.FetchById(task.IdCongViecPhuThuoc.Value);
                ltrPhuThuoc.Text = dep != null ? $"[{dep.MaCongViec}] {dep.TenCongViec}" : "<span class='empty-val'>—</span>";
            }
            else ltrPhuThuoc.Text = "<span class='empty-val'>— Không phụ thuộc —</span>";

            // 4. Ngày kết thúc & Trạng thái (Hàng 3)
            ltrNgayKetThuc.Text = task.NgayKetThuc.HasValue ? task.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "--/--/----";

            if (task.NgayHoanThanhThucTe.HasValue)
                ltrNgayHoanThanhThucTe.Text = task.NgayHoanThanhThucTe.Value.ToString("dd/MM/yyyy");
            else
                ltrNgayHoanThanhThucTe.Text = "<span style='font-size: 13.5px; font-weight: 500; color: #94a3b8;'>Đang chờ...</span>";

            ltrTrangThai.Text = GetTaskStatusBadge(task.TrangThai);

            // 5. Mô tả
            ltrMoTa.Text = !string.IsNullOrWhiteSpace(task.MoTa) ? task.MoTa : "<span class='empty-val'>Chưa có mô tả chi tiết cho công việc này.</span>";

            upViewTask.Update();
            mdlViewTask.OpenModal(true);
        }

        private string GetTaskStatusBadge(byte status)
        {
            TrangThaiCongViec enumStatus = (TrangThaiCongViec)status;
            string resourceKey = TaskManager.Instance.GetValueForTrangThaiCongViec(enumStatus);
            string statusText = GetResourceText(resourceKey);

            // ĐÃ SỬA: Đưa html về cấu trúc thẳng tắp để ăn css làm to chữ (như trong ảnh bác yêu cầu)
            switch (status)
            {
                case 1: return $"<div class='w-100 text-center'><span class=\"badge-pill-custom badge-status-doing\">{statusText}</span></div>";
                case 2: return $"<div class='w-100 text-center'><span class=\"badge-pill-custom badge-status-done\">{statusText}</span></div>";
                case 3: return $"<div class='w-100 text-center'><span class=\"badge-pill-custom badge-status-done\">{statusText}</span><span class='late-label' style='display:block; color:#dc2626; font-weight:800;'>({GetResourceText(BackEndResourceKeys.OVERDUE)})</span></div>";
                case 0: default: return $"<div class='w-100 text-center'><span class=\"badge-pill-custom badge-status-todo\">{statusText}</span></div>";
            }
        }
    }
}