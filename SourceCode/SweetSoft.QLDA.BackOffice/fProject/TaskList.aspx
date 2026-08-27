<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="TaskList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.TaskList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProject/Controls/CtrlTask.ascx" TagPrefix="SweetSoft" TagName="CtrlTask" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
<style>
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
        background-color: #fef2f2 !important;
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

    .modal-overlay {
        display: none;
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(15, 23, 42, 0.65);
        backdrop-filter: blur(2px);
        z-index: 1050;
        justify-content: center;
        align-items: center;
        padding: 20px;
    }
    .modal-overlay.active {
        display: flex;
    }
    .modal-card {
        background: #ffffff;
        border-radius: 10px;
        box-shadow: 0 15px 30px rgba(0, 0, 0, 0.2);
        display: flex;
        flex-direction: column;
        overflow: hidden;
        border: 1px solid #e2e8f0;
    }
    .modal-header-sweet {
        background: linear-gradient(135deg, #4c1d95, #6f42c1);
        color: #ffffff;
        padding: 14px 20px;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }
    .modal-header-sweet h3 {
        font-size: 15px;
        font-weight: 700;
        margin: 0;
    }
    .modal-header-sweet button {
        background: none;
        border: none;
        color: #ffffff;
        font-size: 18px;
        cursor: pointer;
    }
    .modal-body-sweet {
        padding: 20px;
        max-height: 75vh;
        overflow-y: auto;
    }
    .modal-body-sweet .form-control:disabled,
    .modal-body-sweet .form-control[readonly],
    .modal-body-sweet .form-select:disabled,
    .modal-body-sweet input:disabled,
    .modal-body-sweet select:disabled,
    .modal-body-sweet textarea:disabled {
        background: #f1f5f9 !important;
        color: #64748b !important;
        border-color: #cbd5e1 !important;
        cursor: not-allowed !important;
        opacity: 1 !important;
    }
</style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" />
                <SweetSoft:CtrlTask runat="server" ID="CtrlTask1" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <div id="editTaskModal" class="modal-overlay">
        <asp:UpdatePanel ID="upModal" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="modal-card" style="width: 760px; max-width: 95vw;">
                    <div class="modal-header-sweet">
                        <h3>
                            <i class="fas fa-tasks me-2"></i>
                            <asp:Literal ID="litModalTitle" runat="server" Text="Cập nhật thông tin công việc"></asp:Literal>
                        </h3>
                        <button type="button" onclick="closeEditModal()">✕</button>
                    </div>
                    <div class="modal-body-sweet">
                        <asp:HiddenField ID="hfEditTaskId" runat="server" />
                        
                        <div class="row g-2 mb-3">
                            <div class="col-md-3">
                                <label class="form-label fw-bold">Mã công việc</label>
                                <asp:TextBox ID="txtEditMaCv" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                            </div>
                            <div class="col-md-9">
                                <label class="form-label fw-bold">Tên công việc <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtEditTenCv" runat="server" CssClass="form-control" placeholder="Nhập tên đầu việc..."></asp:TextBox>
                            </div>
                        </div>

                        <div class="row g-2 mb-3">
                            <div class="col-md-6">
                                <label class="form-label fw-bold">Thuộc giai đoạn</label>
                                <asp:TextBox ID="txtEditGiaiDoan" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label fw-bold">Công việc cha</label>
                                <asp:DropDownList ID="ddlEditCongViecCha" runat="server" CssClass="form-select" 
                                                  AutoPostBack="true" OnSelectedIndexChanged="ddlEditCongViecChaSelected">
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="row g-2 mb-3">
                            <div class="col-md-4">
                                <label class="form-label fw-bold">Phụ thuộc công việc</label>
                                <asp:DropDownList ID="ddlEditPhuThuoc" runat="server" CssClass="form-select"
                                                  AutoPostBack="true" OnSelectedIndexChanged="ddlEditPhuThuocSelected">
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-4">
                                <label class="form-label fw-bold">Độ ưu tiên</label>
                                <asp:DropDownList ID="ddlEditDoUuTien" runat="server" CssClass="form-select"></asp:DropDownList>
                            </div>
                            <div class="col-md-4">
                                <label class="form-label fw-bold">Nhân viên phụ trách</label>
                                <asp:DropDownList ID="ddlEditNhanVien" runat="server" CssClass="form-select"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="row g-2 mb-3">
                            <div class="col-md-3">
                                <label class="form-label fw-bold">Thời hạn (ngày) <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtEditThoiHan" runat="server" CssClass="form-control" TextMode="Number" min="1"></asp:TextBox>
                            </div>
                            <div class="col-md-3">
                                <label class="form-label fw-bold">Ngày bắt đầu <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtEditNgayBatDau" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                            </div>
                            <div class="col-md-3">
                                <label class="form-label fw-bold">Ngày kết thúc</label>
                                <asp:TextBox ID="txtEditNgayKetThuc" runat="server" CssClass="form-control" TextMode="Date" ReadOnly="true"></asp:TextBox>
                            </div>
                            <div class="col-md-3">
                                <label class="form-label fw-bold">Trạng thái</label>
                                <asp:DropDownList ID="ddlEditTrangThai" runat="server" CssClass="form-select"></asp:DropDownList>
                            </div>
                        </div>

                        <div>
                            <label class="form-label fw-bold">Mô tả công việc</label>
                            <asp:TextBox ID="txtEditMoTa" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
                        </div>
                    </div>

                    <div class="p-3 border-top bg-light d-flex justify-content-end gap-2">
                        <button type="button" class="btn btn-secondary" onclick="closeEditModal()">Quay về</button>
                        <asp:LinkButton ID="btnSaveTask" runat="server" CssClass="btn btn-primary" 
                                        CausesValidation="false" OnClick="btnSaveTask_Click">
                            <i class="fas fa-save me-1"></i> Lưu thay đổi
                        </asp:LinkButton>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        function openEditModal() {
            $('#editTaskModal').addClass('active');
        }

        function closeEditModal() {
            $('#editTaskModal').removeClass('active');
        }

        $(document).on('keydown', function (e) {
            if (e.key === "Escape" && $('#editTaskModal').hasClass('active')) {
                closeEditModal();
            }
        });
    </script>
</asp:Content>