<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="TaskList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.TaskList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fTasks/Controls/CtrlTask.ascx" TagPrefix="SweetSoft" TagName="CtrlTask" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
<style>
    input.form-control:disabled, 
    input.form-control[disabled],
    input.form-control[readonly],
    select.form-select:disabled, 
    select.form-select[disabled],
    select.form-select[readonly],
    textarea.form-control:disabled,
    textarea.form-control[readonly] {
        background-color: #ffffff !important; 
        color: #334155 !important; 
        border-color: black !important;
        cursor: not-allowed !important;
        background-image: none !important; 
        opacity: 1 !important;
    }
    select.form-select:not(:disabled):not([readonly]),
    input.form-control:not(:disabled):not([readonly]),
    textarea.form-control:not(:disabled):not([readonly]) {
        background-color: #ffffff !important; /* Nền trắng tinh */
        color: #0f172a !important; /* Chữ màu đen/xám cực đậm cho dễ đọc */
        border: 1px solid #475569 !important; /* Viền xám đen sắc nét, hết mờ mờ */
        opacity: 1 !important; /* Chống trình duyệt tự làm mờ */
        font-weight: 500 !important; /* Tăng độ dày nét chữ lên một xíu */
    }

    select.form-select:not(:disabled):not([readonly]):focus,
    input.form-control:not(:disabled):not([readonly]):focus,
    textarea.form-control:not(:disabled):not([readonly]):focus {
        border-color: #2563eb !important; /* Viền đổi sang xanh dương đậm */
        box-shadow: 0 0 0 0.2rem rgba(37, 99, 235, 0.15) !important; /* Hiệu ứng sáng nhẹ quanh viền */
        color: #000000 !important;
    }
    .btn-filter-overdue {
        border: 1px solid #f87171 !important;
        color: #ef4444 !important;
        background-color: #ffffff !important;
        padding: 5px 14px !important;
        border-radius: 20px !important;
        font-size: 12px !important;
        font-weight: 600 !important;
        cursor: pointer !important;
        display: inline-flex !important;
        align-items: center !important;
        gap: 6px !important;
        transition: all 0.2s ease !important;
    }
    .btn-filter-overdue:hover,
    .btn-filter-overdue.active {
        background-color: #ef4444 !important;
        color: #ffffff !important;
        border-color: #ef4444 !important;
    }

    .btn-tool-folder {
        background-color: #ffffff !important;
        color: #334155 !important;
        border: 1px solid #cbd5e1 !important;
        padding: 5px 12px !important;
        border-radius: 6px !important;
        font-size: 12px !important;
        font-weight: 600 !important;
        cursor: pointer !important;
        display: inline-flex !important;
        align-items: center !important;
        gap: 6px !important;
        transition: all 0.15s ease !important;
    }
    .btn-tool-folder:hover {
        background-color: #f1f5f9 !important;
        border-color: #94a3b8 !important;
    }

    .row-overdue-bg {
        background-color: #fee2e2 !important;
        transition: background-color 0.2s ease !important;
    }
    .row-overdue-bg:hover, .row-overdue-bg.active {
        background-color: #fecaca !important; 
    }

    .row-warning-bg {
        background-color: #fffbeb !important; 
        transition: background-color 0.2s ease !important; 
    }
    .row-warning-bg:hover, .row-warning-bg.active {
        background-color: #fef3c7 !important; 
    }

    .btn-toggle-tree {
        background-color: #ffffff !important;
        color: #334155 !important;
        border: 1px solid #cbd5e1 !important;
        padding: 6px 14px !important;
        border-radius: 6px !important;
        font-size: 13px !important;
        font-weight: 600 !important;
        cursor: pointer !important;
        display: inline-flex !important;
        align-items: center !important;
        gap: 6px !important;
        transition: all 0.2s ease !important;
    }
    .btn-toggle-tree:hover {
        background-color: #f1f5f9 !important;
        border-color: #94a3b8 !important;
    }

    .task-phase-name {
        font-size: 13.5px !important;
        font-weight: 700 !important;
        color: #0f172a !important;
    }
    .task-sub-name {
        font-size: 12.5px !important;
        font-weight: 600 !important;
        color: #1e293b !important;
    }
    .task-sub-name strong {
        font-weight: 700 !important;
        color: #0f172a !important;
    }
    .task-tree-branch {
        color: #475569 !important;
        font-weight: 700 !important;
    }

    .badge-pill-custom {
        padding: 3px 8px !important;
        border-radius: 4px !important;
        font-size: 11px !important;
        font-weight: 600 !important;
        display: inline-block !important;
        white-space: nowrap !important;
        line-height: 1.2 !important;
    }

    .badge-status-doing { background-color: #e0f2fe !important; color: #0369a1 !important; border: 1px solid #bae6fd !important; }
    .badge-status-todo  { background-color: #f1f5f9 !important; color: #475569 !important; border: 1px solid #cbd5e1 !important; }
    .badge-status-done  { background-color: #dcfce7 !important; color: #15803d !important; border: 1px solid #bbf7d0 !important; }

    .badge-pri-low  { background-color: #e0f2fe !important; color: #0369a1 !important; border: 1px solid #bae6fd !important; font-weight: 600 !important; }
    .badge-pri-med  { background-color: #fef3c7 !important; color: #b45309 !important; border: 1px solid #fde68a !important; font-weight: 600 !important; }
    .badge-pri-high { background-color: #fee2e2 !important; color: #dc2626 !important; border: 1px solid #fca5a5 !important; font-weight: 700 !important; }

    .opt-status-todo  { color: #475569 !important; font-weight: 600; background-color: #f1f5f9; }
    .opt-status-doing { color: #0369a1 !important; font-weight: 600; background-color: #e0f2fe; }
    .opt-status-done  { color: #15803d !important; font-weight: 700; background-color: #dcfce7; }
    .opt-pri-low      { color: #0284c7 !important; font-weight: 600; background-color: #f0f9ff; }
    .opt-pri-med      { color: #d97706 !important; font-weight: 600; background-color: #fffbeb; }
    .opt-pri-high     { color: #dc2626 !important; font-weight: 700; background-color: #fef2f2; }
    .task-phase-box {
    background-color: #f3e8ff !important;
    border-left: 4px solid #6f42c1 !important;
    padding: 8px 12px !important;
    border-radius: 6px !important;
    box-shadow: 0 1px 2px rgba(0,0,0,0.05) !important;
    }
    .task-phase-text {
        font-size: 15.5px !important;
        font-weight: 800 !important;
        color: #4c1d95 !important;
    }
    .task-sub-box {
        font-size: 14.5px !important;
        font-weight: 600 !important;
        color: black !important;
    }
    .task-sub-code {
        font-weight: 700 !important;
        color: #0f172a !important;
    }
    .table-task-grid th:first-child,
    .table-task-grid td:first-child {
        display: none !important;
    }
</style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2">
                <SweetSoft:Navigation runat="server" ID="Navigation1" />
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <SweetSoft:CtrlTask runat="server" ID="CtrlTask1" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="mdlEditTask" Type="Primary">
        <ContentTemplate>
            <asp:UpdatePanel ID="upModal" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="p-3">
                        <asp:HiddenField ID="hfEditTaskId" runat="server" />
                        
                        <div class="row g-2 mb-3">
                            <div class="col-md-3">
                                <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.TASK_CODE) %></label>
                                <asp:TextBox ID="txtEditMaCv" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                            </div>
                            <div class="col-md-9">
                                <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %> <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtEditTenCv" runat="server" CssClass="form-control" placeholder="Nhập tên đầu việc..."></asp:TextBox>
                            </div>
                        </div>

                        <div class="row g-2 mb-3">
                            <div class="col-md-6">
                                <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.PHASE) %></label>
                                <asp:TextBox ID="txtEditGiaiDoan" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.PARENT_TASK) %></label>
                                <asp:DropDownList ID="ddlEditCongViecCha" runat="server" CssClass="form-select" 
                                                  AutoPostBack="true" OnSelectedIndexChanged="ddlEditCongViecChaSelected">
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="row g-2 mb-3">
                            <div class="col-md-4">
                                <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.DEPENDENT) %></label>
                                <asp:DropDownList ID="ddlEditPhuThuoc" runat="server" CssClass="form-select"
                                                  AutoPostBack="true" OnSelectedIndexChanged="ddlEditPhuThuocSelected">
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-4">
                                <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.PRIORITY) %></label>
                                <asp:DropDownList ID="ddlEditDoUuTien" runat="server" CssClass="form-select"></asp:DropDownList>
                            </div>
                            <div class="col-md-4">
                                <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                                <asp:DropDownList ID="ddlEditTrangThai" runat="server" CssClass="form-select"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="row g-2 mb-3">
                            <div class="col-md-4">
                                <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.START_DATE) %><span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtEditNgayBatDau" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                            </div>
                            <div class="col-md-4">
                                <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.DURATION) %> <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtEditThoiHan" runat="server" CssClass="form-control" TextMode="Number" min="1"></asp:TextBox>
                            </div>
                            <div class="col-md-4">
                                <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.END_DATE) %></label>
                                <asp:TextBox ID="txtEditNgayKetThuc" runat="server" CssClass="form-control" TextMode="Date" ReadOnly="true"></asp:TextBox>
                            </div>
                        </div>

                        <div>
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.SUMMARY) %></label>
                            <asp:TextBox ID="txtEditMoTa" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>
        <FooterTemplate>
            <asp:UpdatePanel ID="upnlFooterEdit" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:LinkButton ID="btnSaveTask" runat="server" CssClass="btn btn-primary waves-effect waves-light" 
                                    CausesValidation="false" OnClick="btnSaveTask_Click">
                        <i class="fas fa-save me-1"></i> <%= GetResourceText(BackEndResourceKeys.SAVE) %>
                    </asp:LinkButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        var isOverdueFiltered = false;
        var isTreeCollapsed = false;

        function applyTaskFilters() {
            var gridRows = document.querySelectorAll('.table-task-grid tbody tr');

            gridRows.forEach(function (row) {
                if (!row.hasAttribute('data-level')) return;

                var isOverdue = row.getAttribute('data-overdue') === '1';
                var level = parseInt(row.getAttribute('data-level'), 10);

                var showRow = true;

                if (isOverdueFiltered && !isOverdue) {
                    showRow = false;
                }

                if (isTreeCollapsed && level > 1) {
                    showRow = false;
                }

                row.style.display = showRow ? '' : 'none';
            });
        }

        window.toggleOverdueFilter = function () {
            isOverdueFiltered = !isOverdueFiltered;

            if (isOverdueFiltered && isTreeCollapsed) {
                isTreeCollapsed = false;
                var btnTree = document.getElementById('btnToggleTree');
                var lbl = document.getElementById('lblToggleText');
                var icon = btnTree ? btnTree.querySelector('i') : null;

                if (btnTree) {
                    if (lbl) lbl.innerText = btnTree.getAttribute('data-collapse-text') || 'Thu gọn';
                    if (icon) {
                        icon.classList.remove('fa-folder');
                        icon.classList.add('fa-folder-open');
                    }
                }
            }

            var btnOverdue = document.getElementById('btnFilterOverdue');
            if (btnOverdue) {
                if (isOverdueFiltered) {
                    btnOverdue.classList.add('active'); 
                } else {
                    btnOverdue.classList.remove('active');
                }
            }

            applyTaskFilters();
        };

        window.toggleTaskTree = function () {
            isTreeCollapsed = !isTreeCollapsed;

            if (isTreeCollapsed && isOverdueFiltered) {
                isOverdueFiltered = false;
                var btnOverdue = document.getElementById('btnFilterOverdue');
                if (btnOverdue) btnOverdue.classList.remove('active');
            }

            var btn = document.getElementById('btnToggleTree');
            var lbl = document.getElementById('lblToggleText');
            var icon = btn ? btn.querySelector('i') : null;

            if (btn) {
                var expandText = btn.getAttribute('data-expand-text') || 'Mở rộng';
                var collapseText = btn.getAttribute('data-collapse-text') || 'Thu gọn';

                if (isTreeCollapsed) {
                    if (lbl) lbl.innerText = expandText;
                    if (icon) {
                        icon.classList.remove('fa-folder-open');
                        icon.classList.add('fa-folder');
                    }
                } else {
                    if (lbl) lbl.innerText = collapseText;
                    if (icon) {
                        icon.classList.remove('fa-folder');
                        icon.classList.add('fa-folder-open');
                    }
                }
            }

            applyTaskFilters();
        };

        window.addEventListener("storage", function (e) {
            if (e.key === "ScheduleChanged") {
                window.location.reload();
            }
        });
    </script>
</asp:Content>
