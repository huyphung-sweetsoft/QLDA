<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlGiaiDoanDuAn.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlGiaiDoanDuAn" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.EnumHelper.Defines" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Web" %>

<style>
    .stage-timeline-wrapper {
        position: relative;
        display: flex;
        align-items: center;
        gap: 0;
    }

    .stage-timeline-viewport {
        flex: 1;
        overflow: hidden;
    }

    .stage-timeline-track {
        display: flex;
        align-items: flex-start;
        position: relative;
        transition: transform 0.35s ease;
    }

    /* Đường nối nằm ngang ở giữa các vòng tròn */
    .stage-connector-bg {
        position: absolute;
        top: 11px;
        left: 0;
        right: 0;
        height: 2px;
        background-color: #dee2e6;
        z-index: 0;
    }

    /* Mỗi ô giai đoạn chiếm đúng 25% (4 ô = 100%) */
    .stage-item {
        flex: 0 0 25%;
        display: flex;
        flex-direction: column;
        align-items: center;
        position: relative;
        z-index: 1;
    }

    .stage-dot-wrapper {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 24px;
        height: 24px;
        margin-bottom: 8px;
    }

    /* Vòng tròn mặc định – chưa bắt đầu */
    .stage-dot {
        width: 20px;
        height: 20px;
        border-radius: 50%;
        border: 2px solid #6c757d;
        background-color: #fff;
        transition: transform 0.2s;
    }

    /* Đã hoàn thành */
    .stage-dot.stage-done {
        background-color: var(--bs-primary, #0d6efd);
        border-color: var(--bs-primary, #0d6efd);
    }

    /* Đang thực hiện */
    .stage-dot.stage-active {
        background-color: #fff;
        border-color: var(--bs-primary, #0d6efd);
        border-width: 2px;
        box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.15);
    }

    .stage-label {
        text-align: center;
        padding: 0 4px;
        width: 100%;
    }

    .stage-name {
        color: #495057;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .stage-percent {
        margin-top: 2px;
    }

    /* Nút prev / next */
    .stage-nav-btn {
        flex-shrink: 0;
        width: 28px;
        height: 28px;
        border-radius: 50%;
        border: 1px solid #dee2e6;
        background: #fff;
        color: #6c757d;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        font-size: 11px;
        transition: background 0.2s, color 0.2s;
        padding: 0;
    }

    .stage-nav-btn:hover {
        background: var(--bs-primary, #0d6efd);
        color: #fff;
        border-color: var(--bs-primary, #0d6efd);
    }

    .stage-nav-prev {
        margin-right: 8px;
    }

    .stage-nav-next {
        margin-left: 8px;
    }

    /* ---- Bảng quản lý giai đoạn trong drawer ---- */
    .stage-mgmt-table {
        width: 100%;
    }

    .stage-mgmt-header,
    .stage-mgmt-row {
        display: grid;
        grid-template-columns: 1.8fr 0.9fr 0.9fr 1.1fr 1fr;
        align-items: center;
        gap: 8px;
        padding: 10px 0;
    }

    .stage-mgmt-header {
        border-bottom: 1px solid #dee2e6;
        padding-bottom: 8px;
        margin-bottom: 2px;
    }

    .stage-mgmt-row {
        border-bottom: 1px solid #f1f3f5;
    }

    .stage-mgmt-row:last-child {
        border-bottom: none;
    }

    /* Badge trạng thái */
    .stage-status-badge {
        display: inline-block;
        padding: 3px 10px;
        border-radius: 20px;
        font-size: 12px;
        white-space: nowrap;
    }

    .stage-status-badge.badge-done {
        border: 1px solid #adb5bd;
        color: #495057;
        background: transparent;
    }

    .stage-status-badge.badge-active {
        border: 1px solid #0d6efd;
        color: #0d6efd;
        background: transparent;
    }

    .stage-status-badge.badge-pending {
        border: 1px solid #dee2e6;
        color: #6c757d;
        background: transparent;
    }

    /* Mini progress bar */
    .stage-progress-bar-wrap {
        width: 60px;
        height: 6px;
        background: #e9ecef;
        border-radius: 4px;
        overflow: hidden;
        flex-shrink: 0;
    }

    .stage-progress-bar-fill {
        height: 100%;
        border-radius: 4px;
        background: #adb5bd;
        transition: width 0.3s;
    }

    .stage-progress-bar-fill.fill-done {
        background: #6c757d;
    }

    .stage-progress-bar-fill.fill-active {
        background: var(--bs-primary, #0d6efd);
    }
</style>

<%-- Phần hiển thị trên chi tiết dự án --%>
<section>
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h5 class="text-uppercase fw-bold mb-0">
            Giai đoạn
        </h5>

        <a
            href="javascript:;"
            class="small text-primary text-decoration-none"
            onclick="ProjectStageJs.ShowOffcanvas();">

            Quản lý giai đoạn
            <i class="fas fa-arrow-right ms-1"></i>
        </a>
    </div>

    <div class="card border shadow-none rounded-3">
        <div class="card-body py-3 px-3">

            <%-- Trạng thái rỗng --%>
            <asp:Panel
                runat="server"
                ID="pnlEmpty"
                Visible="false"
                CssClass="text-center text-muted py-4">

                <i class="fas fa-project-diagram d-block mb-2"></i>
                Dự án chưa có giai đoạn.
            </asp:Panel>

            <%-- Timeline giai đoạn (tối đa 4 hiển thị cùng lúc) --%>
            <asp:Repeater
                runat="server"
                ID="rptStages">

                <HeaderTemplate>
                    <div class="stage-timeline-wrapper">
                        <%-- Nút prev --%>
                        <button
                            type="button"
                            id="btnStagePrev"
                            class="stage-nav-btn stage-nav-prev d-none"
                            onclick="ProjectStageJs.Slide(-1);">
                            <i class="fas fa-chevron-left"></i>
                        </button>

                        <%-- Viewport cố định 4 ô --%>
                        <div class="stage-timeline-viewport">
                            <div class="stage-timeline-track" id="stageTrack">
                                <%-- Đường nền --%>
                                <div class="stage-connector-bg"></div>
                </HeaderTemplate>

                <ItemTemplate>
                                <%-- Một ô giai đoạn --%>
                                <div class="stage-item" data-index="<%# Container.ItemIndex %>">
                                    <div class="stage-dot-wrapper">
                                        <div class="stage-dot <%# Convert.ToString(Eval("DotCssClass")) %>">
                                        </div>
                                    </div>

                                    <div class="stage-label">
                                        <div class="stage-name small">
                                            <%# HttpUtility.HtmlEncode(
                                                Convert.ToString(
                                                    Eval("TenGiaiDoan"))) %>
                                        </div>

                                        <div class="stage-percent fw-semibold small text-primary">
                                            <%# Eval("PhanTramHienThi") %>
                                        </div>
                                    </div>
                                </div>
                </ItemTemplate>

                <FooterTemplate>
                            </div><%-- /.stage-timeline-track --%>
                        </div><%-- /.stage-timeline-viewport --%>

                        <%-- Nút next --%>
                        <button
                            type="button"
                            id="btnStageNext"
                            class="stage-nav-btn stage-nav-next d-none"
                            onclick="ProjectStageJs.Slide(1);">
                            <i class="fas fa-chevron-right"></i>
                        </button>
                    </div><%-- /.stage-timeline-wrapper --%>
                </FooterTemplate>
            </asp:Repeater>

        </div>
    </div>
</section>

<%-- Drawer quản lý giai đoạn --%>
<div
    class="offcanvas offcanvas-end offcanvas-form-search"
    id="project-stage-offcanvas"
    aria-hidden="true"
    tabindex="-1">

    <div class="offcanvas-header">
        <div class="flex flex-column flex-md-row align-items-center gap-3">
            <h5 class="offcanvas-title">
                Quản lý giai đoạn
            </h5>
        </div>

        <button
            class="btn-close"
            type="button"
            data-bs-dismiss="offcanvas"
            aria-label="Close">
        </button>
    </div>

    <div class="div offcanvas-body">
        <div class="card shadow-none card-body text-muted mb-0">

        <asp:UpdatePanel
            runat="server"
            ID="upnlStageManagement"
            UpdateMode="Conditional">

            <ContentTemplate>

                <%-- Thông báo lỗi --%>
                <asp:Label
                    runat="server"
                    ID="lblStageError"
                    Visible="false"
                    CssClass="alert alert-danger d-block mx-4 mt-3 mb-0">
                </asp:Label>

                <%-- Form thêm/sửa — hiện ở đầu khi nhấn Thêm --%>
                <asp:Panel
                    runat="server"
                    ID="pnlStageForm"
                    Visible="false"
                    CssClass="border-bottom mb-3 pb-3">

                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <div>
                            <asp:Label
                                runat="server"
                                ID="lblStageFormTitle"
                                CssClass="fw-bold mb-0 d-block"
                                Text="Thêm giai đoạn">
                            </asp:Label>

                            <div class="small mt-1">
                                Điền thông tin giai đoạn dưới đây
                            </div>
                        </div>

                        <asp:LinkButton
                            runat="server"
                            ID="lbtCancelStage"
                            CssClass="btn btn-outline-secondary btn-sm"
                            OnClick="lbtCancelStage_Click">

                            <i class="fas fa-times me-1"></i>
                            Hủy
                        </asp:LinkButton>
                    </div>

                    <%-- Loại giai đoạn --%>
                    <div class="mb-3">
                        <label class="form-label">
                            Loại giai đoạn
                        </label>

                        <asp:RadioButtonList
                            runat="server"
                            ID="rblStageType"
                            AutoPostBack="true"
                            RepeatDirection="Horizontal"
                            CssClass="d-flex gap-4"
                            OnSelectedIndexChanged="rblStageType_SelectedIndexChanged">

                            <asp:ListItem
                                Text="Giai đoạn chung"
                                Value="COMMON"
                                Selected="True">
                            </asp:ListItem>

                            <asp:ListItem
                                Text="Giai đoạn tùy chỉnh"
                                Value="CUSTOM">
                            </asp:ListItem>
                        </asp:RadioButtonList>
                    </div>

                    <%-- Giai đoạn chung --%>
                    <asp:Panel
                        runat="server"
                        ID="pnlCommonStage"
                        CssClass="mb-3">

                        <label class="form-label">
                            Giai đoạn
                            <span class="text-danger">*</span>
                        </label>

                        <asp:DropDownList
                            runat="server"
                            ID="ddlCommonStage"
                            CssClass="form-select">
                        </asp:DropDownList>
                    </asp:Panel>

                    <%-- Giai đoạn tùy chỉnh --%>
                    <asp:Panel
                        runat="server"
                        ID="pnlCustomStage"
                        Visible="false"
                        CssClass="mb-3">

                        <label class="form-label">
                            Tên giai đoạn
                            <span class="text-danger">*</span>
                        </label>

                        <asp:TextBox
                            runat="server"
                            ID="txtCustomStageName"
                            CssClass="form-control"
                            MaxLength="250">
                        </asp:TextBox>
                    </asp:Panel>

                    <div class="row g-3 mb-3">
                        <div class="col-md-6">
                            <label class="form-label">Ngày bắt đầu</label>

                            <asp:TextBox
                                runat="server"
                                ID="txtStartDate"
                                TextMode="Date"
                                CssClass="form-control">
                            </asp:TextBox>
                        </div>

                        <div class="col-md-6">
                            <label class="form-label">Dự kiến hoàn thành</label>

                            <asp:TextBox
                                runat="server"
                                ID="txtExpectedEndDate"
                                TextMode="Date"
                                CssClass="form-control">
                            </asp:TextBox>
                        </div>

                        <div class="col-md-6">
                            <label class="form-label">Hoàn thành thực tế</label>

                            <asp:TextBox
                                runat="server"
                                ID="txtActualEndDate"
                                TextMode="Date"
                                CssClass="form-control">
                            </asp:TextBox>
                        </div>

                        <div class="col-md-6">
                            <label class="form-label">Thứ tự</label>

                            <asp:TextBox
                                runat="server"
                                ID="txtStageOrder"
                                TextMode="Number"
                                CssClass="form-control">
                            </asp:TextBox>
                        </div>

                        <div class="col-12">
                            <label class="form-label">Mô tả</label>

                            <asp:TextBox
                                runat="server"
                                ID="txtStageDescription"
                                TextMode="MultiLine"
                                Rows="3"
                                MaxLength="1000"
                                CssClass="form-control">
                            </asp:TextBox>
                        </div>
                    </div>

                    <div class="d-flex justify-content-end">
                        <asp:LinkButton
                            runat="server"
                            ID="lbtSaveStage"
                            CssClass="btn btn-primary"
                            OnClick="lbtSaveStage_Click">

                            <i class="fas fa-save me-1"></i>
                            Lưu giai đoạn
                        </asp:LinkButton>
                    </div>
                </asp:Panel>

                <%-- Header danh sách --%>
                <div class="d-flex justify-content-between align-items-start mb-3">
                    <div>
                        <div class="fw-bold">Danh sách giai đoạn</div>

                        <div class="small mt-1">
                            Theo dõi thời gian, trạng thái và mức độ hoàn thành
                        </div>
                    </div>

                    <asp:LinkButton
                        runat="server"
                        ID="lbtAddStage"
                        CssClass="btn btn-info btn-sm flex-shrink-0"
                        OnClick="lbtAddStage_Click">

                        <i class="fas fa-plus me-1"></i>
                        Thêm giai đoạn
                    </asp:LinkButton>
                </div>

                <%-- Không có dữ liệu --%>
                <asp:Panel
                    runat="server"
                    ID="pnlEmptyManagement"
                    Visible="false"
                    CssClass="text-center text-muted py-5 px-4">

                    <i class="fas fa-layer-group d-block mb-2 fs-4"></i>
                    Dự án chưa có giai đoạn nào.
                </asp:Panel>

                <%-- Bảng giai đoạn --%>
                <asp:Repeater
                    runat="server"
                    ID="rptStageManagement">

                    <HeaderTemplate>
                        <div class="stage-mgmt-table px-4">
                            <div class="stage-mgmt-header">
                                <div class="col-stage-name text-uppercase small text-muted fw-semibold">Giai đoạn</div>
                                <div class="col-stage-date text-uppercase small text-muted fw-semibold">Bắt đầu</div>
                                <div class="col-stage-date text-uppercase small text-muted fw-semibold">Kết thúc</div>
                                <div class="col-stage-status text-uppercase small text-muted fw-semibold">Trạng thái</div>
                                <div class="col-stage-pct text-uppercase small text-muted fw-semibold text-end">Hoàn thành</div>
                            </div>
                    </HeaderTemplate>

                    <ItemTemplate>
                            <div class="stage-mgmt-row">
                                <%-- Tên giai đoạn --%>
                                <div class="col-stage-name">
                                    <div class="fw-semibold">
                                        <%# HttpUtility.HtmlEncode(Convert.ToString(Eval("TenGiaiDoan"))) %>
                                    </div>

                                    <div class="text-muted small">
                                        Phase <%# Convert.ToString(Eval("ThuTuGiaiDoan")).PadLeft(2, '0') %>
                                    </div>
                                </div>

                                <%-- Ngày bắt đầu --%>
                                <div class="col-stage-date small text-muted">
                                    <%# GetFormattedDate(Eval("NgayBatDau")) %>
                                </div>

                                <%-- Ngày kết thúc --%>
                                <div class="col-stage-date small text-muted">
                                    <%# GetFormattedDate(Eval("NgayDuKienHoanThanh")) %>
                                </div>

                                <%-- Trạng thái --%>
                                <div class="col-stage-status">
                                    <span class="stage-status-badge <%# GetStatusBadgeClass(Convert.ToString(Eval("TrangThaiHienThi"))) %>">
                                        <%# Eval("TrangThaiHienThi") %>
                                    </span>
                                </div>

                                <%-- Hoàn thành --%>
                                <div class="col-stage-pct">
                                    <div class="d-flex align-items-center gap-2 justify-content-end">
                                        <div class="stage-progress-bar-wrap">
                                            <div
                                                class="stage-progress-bar-fill <%# GetProgressFillClass(Convert.ToString(Eval("TrangThaiHienThi"))) %>"
                                                style="width: <%# GetHardCodedPercent(Convert.ToString(Eval("TrangThaiHienThi"))) %>%;">
                                            </div>
                                        </div>

                                        <span class="small fw-semibold" style="min-width: 32px; text-align: right;">
                                            <%# GetHardCodedPercent(Convert.ToString(Eval("TrangThaiHienThi"))) %>%
                                        </span>
                                    </div>
                                </div>
                            </div>
                    </ItemTemplate>

                    <FooterTemplate>
                        </div><%-- /.stage-mgmt-table --%>
                    </FooterTemplate>
                </asp:Repeater>

            </ContentTemplate>
        </asp:UpdatePanel>
        </div>
    </div>

</div>

<script type="text/javascript">
    window.ProjectStageJs = window.ProjectStageJs || {};

    // ---- Sliding window (tối đa 4 giai đoạn hiển thị) ----
    ProjectStageJs._offset = 0;
    ProjectStageJs.MAX_VISIBLE = 4;

    ProjectStageJs.InitSlider = function () {
        var track = document.getElementById("stageTrack");
        if (!track) return;

        var items = track.querySelectorAll(".stage-item");
        var total = items.length;

        var btnPrev = document.getElementById("btnStagePrev");
        var btnNext = document.getElementById("btnStageNext");

        if (total <= ProjectStageJs.MAX_VISIBLE) {
            if (btnPrev) btnPrev.classList.add("d-none");
            if (btnNext) btnNext.classList.add("d-none");
            return;
        }

        ProjectStageJs._total = total;
        ProjectStageJs._btnPrev = btnPrev;
        ProjectStageJs._btnNext = btnNext;
        ProjectStageJs._track = track;

        ProjectStageJs._updateNav();
    };

    ProjectStageJs.Slide = function (direction) {
        var total = ProjectStageJs._total;
        if (!total) return;

        var newOffset = ProjectStageJs._offset + direction;
        var maxOffset = total - ProjectStageJs.MAX_VISIBLE;

        if (newOffset < 0) newOffset = 0;
        if (newOffset > maxOffset) newOffset = maxOffset;

        ProjectStageJs._offset = newOffset;

        // Mỗi item chiếm 25% viewport → dịch chuyển offset * 25%
        var pct = newOffset * (100 / ProjectStageJs.MAX_VISIBLE);
        ProjectStageJs._track.style.transform =
            "translateX(-" + pct + "%)";

        ProjectStageJs._updateNav();
    };

    ProjectStageJs._updateNav = function () {
        var btnPrev = ProjectStageJs._btnPrev;
        var btnNext = ProjectStageJs._btnNext;
        var offset  = ProjectStageJs._offset;
        var maxOffset = ProjectStageJs._total - ProjectStageJs.MAX_VISIBLE;

        if (btnPrev) {
            if (offset <= 0) {
                btnPrev.classList.add("d-none");
            } else {
                btnPrev.classList.remove("d-none");
            }
        }

        if (btnNext) {
            if (offset >= maxOffset) {
                btnNext.classList.add("d-none");
            } else {
                btnNext.classList.remove("d-none");
            }
        }
    };

    // ---- Offcanvas helpers ----
    ProjectStageJs.ShowOffcanvas = function () {
        var element = document.getElementById("project-stage-offcanvas");

        if (!element) {
            console.error("Không tìm thấy project-stage-offcanvas");
            return;
        }

        if (typeof bootstrap === "undefined" || !bootstrap.Offcanvas) {
            console.error("Bootstrap Offcanvas chưa được tải");
            return;
        }

        var instance = bootstrap.Offcanvas.getOrCreateInstance(element);
        instance.show();
    };

    ProjectStageJs.HideOffcanvas = function () {
        var element = document.getElementById("project-stage-offcanvas");

        if (!element ||
            typeof bootstrap === "undefined" ||
            !bootstrap.Offcanvas) {
            return;
        }

        var instance = bootstrap.Offcanvas.getInstance(element);
        if (instance) {
            instance.hide();
        }
    };

    // Khởi tạo slider sau khi DOM sẵn sàng
    document.addEventListener("DOMContentLoaded", function () {
        ProjectStageJs.InitSlider();
    });

    // Khởi tạo lại sau UpdatePanel refresh
    if (typeof Sys !== "undefined" && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance()
            .add_endRequest(function () {
                ProjectStageJs._offset = 0;
                ProjectStageJs.InitSlider();
            });
    }
</script>