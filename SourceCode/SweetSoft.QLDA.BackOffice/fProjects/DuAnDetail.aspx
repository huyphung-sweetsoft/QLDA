<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="DuAnDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.DuAnDetail" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjects/Controls/CtrlGiaiDoanDuAn.ascx" TagPrefix="SweetSoft" TagName="CtrlGiaiDoanDuAn" %>
<%@ Register Src="~/fProjects/Controls/CtrlLichSuDuAn.ascx" TagPrefix="SweetSoft" TagName="CtrlLichSuDuAn" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        /* CSS CHO AVATAR STACK CỦA OWNER VÀ THÀNH VIÊN */
        .avatar-group { 
            display: inline-flex !important; 
            align-items: center; 
            justify-content: center; 
            flex-wrap: nowrap !important; 
            white-space: nowrap !important; /* KHOA CHẶT: Cấm tuyệt đối việc rớt dòng */
        }  
        .avatar-stack-container { 
            display: flex; 
            align-items: center; 
        }    
        .avatar-circle { 
            width: 30px; height: 30px; border-radius: 50%; display: flex; align-items: center; justify-content: center; 
            font-size: 11px; font-weight: 700; color: #ffffff; border: 2px solid #ffffff; 
            margin-left: -8px; position: relative; z-index: 1; box-shadow: 0 1px 2px rgba(0,0,0,0.1);
        }    
        .avatar-circle:first-child { margin-left: 0; }    
        .avatar-more { 
            background-color: #f1f5f9; color: #475569; border-color: #cbd5e1; z-index: 0; font-weight: 800; font-size: 10px; 
        }    
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
    <div class="col-xl-12">
        <div class="card p-3 min-vh-100">
            <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Project detail" />
            <%-- Tiêu đề và thao tác --%>
            <div class="d-flex flex-column flex-lg-row justify-content-between align-items-lg-start gap-3 mb-4">

                <div class="d-flex flex-wrap align-items-center gap-2">
                    <h4 class="mb-0 fw-semibold text-primary">
                        <asp:Label runat="server" ID="lblTenDuAn"></asp:Label>
                    </h4>

                    <span class="badge rounded-pill bg-light text-secondary border px-3 py-2">
                        Đang thực hiện
                    </span>
                </div>

                <div class="d-flex align-items-center gap-2">

                    <%-- Dropdown trạng thái --%>
                    <div class="dropdown">
                        <button type="button" class="btn btn-outline-secondary dropdown-toggle" data-bs-toggle="dropdown" aria-expanded="false">
                            <i runat="server" id="iCurrentStatusIcon" class="fas fa-circle text-info me-2 small"></i>
                            <asp:Literal runat="server" ID="ltrCurrentStatusName"></asp:Literal>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end">
                            <asp:Repeater runat="server" ID="rptStatusDropdown" OnItemCommand="rptStatusDropdown_ItemCommand">
                                <ItemTemplate>
                                    <li>
                                        <asp:LinkButton 
                                            runat="server" 
                                            CommandName="ChangeStatus" 
                                            CommandArgument='<%# Eval("Value") %>'  
                                            CssClass='<%# "dropdown-item " + (Convert.ToByte(Eval("Value")) == CurrentStatusValue ? "active" : "") %>'>
                                            <i class='<%# "fas fa-circle me-2 small " + GetStatusCssClass((SweetSoft.QLDA.Core.EnumHelper.Defines.DuAnStatus)Convert.ToByte(Eval("Value"))) %>'></i>
                                            <%# Eval("Name") %>
                                        </asp:LinkButton>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </div>

                    <button type="button" class="btn btn-outline-secondary">
                        <i class="fas fa-pencil-alt me-1"></i>
                        Sửa
                    </button>

                    <div class="dropdown">
                        <button type="button" class="btn btn-outline-secondary" data-bs-toggle="dropdown" aria-expanded="false">
                            <i class="fas fa-ellipsis-h"></i>
                        </button>

                        <ul class="dropdown-menu dropdown-menu-end">
                            <li>
                                <a class="dropdown-item" href="javascript:;">
                                    <i class="fas fa-history me-2 text-muted"></i>
                                    Lịch sử hoạt động
                                </a>
                            </li>

                            <li>
                                <a class="dropdown-item" href="javascript:;">
                                    <i class="fas fa-project-diagram me-2 text-muted"></i>
                                    Quản lý giai đoạn
                                </a>
                            </li>

                            <li><hr class="dropdown-divider" /></li>

                            <li>
                                <a class="dropdown-item text-danger" href="javascript:;">
                                    <i class="fas fa-trash me-2"></i>
                                    Xóa dự án
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>

            <SweetSoft:CtrlProjectTabs
                runat="server"
                ID="CtrlProjectTabs1" />

            <div class="row g-4 align-items-start">

                <%-- Cột trái --%>
                <div class="col-xl-8">

                    <%-- Thông tin dự án --%>
                    <section class="mb-3">
                        <h5 class="text-uppercase fw-bold mb-3">
                            <%= GetResourceText(BackEndResourceKeys.PROJECT_INFORMATION) %>
                        </h5>
                         
                        <div class="row g-4">
                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.CUSTOMER) %>
                                </div>
                                <asp:Label runat="server" ID="lblKhachHang"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.CONTRACT_VALUE) %>
                                </div>
                                <asp:Label runat="server" ID="lblGiaTriHopDong"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.SIGN_DATE) %>
                                </div>
                                <asp:Label runat="server" ID="lblNgayKy"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.PROJECT_TYPE) %>
                                </div>
                                <asp:Label runat="server" ID="lblLoaiDuAn"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.START_DATE) %>
                                </div>
                                <asp:Label runat="server" ID="lblNgayBatDau"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    Ngày hoàn thành dự kiến
                                </div>
                                <asp:Label runat="server" ID="lblNgayHoanThanhDuKien"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    Hoàn thành thực tế
                                </div>
                                <asp:Label runat="server" ID="lblNgayHoanThanhThucTe"></asp:Label>
                            </div>

                            <div class="col-md-6">
                                <div class="font-size-8 fw-bold mb-1">
                                    <%= GetResourceText(BackEndResourceKeys.STATUS) %>
                                </div>
                                <asp:Label runat="server" ID="lblTrangThai"></asp:Label>
                            </div>
                        </div>
                    </section>

                    <%-- Tiến độ --%>
                    <section class="mb-3">
                        <h5 class="text-uppercase fw-bold mb-3">
                            Tiến độ
                        </h5>

                        <div class="card border shadow-none rounded-3">
                            <div class="card-body py-2 px-3">

                                <div class="mb-4">
                                    <div class="d-flex justify-content-between align-items-center mb-2">
                                        <span class="fs-6">
                                            Theo tỷ trọng ngày thực hiện
                                        </span>

                                        <strong runat="server" id="lblTienDoThoiGian" class="small text-primary">
                                        </strong>
                                    </div>

                                    <div runat="server" id="divTienDoThoiGian" class="progress" role="progressbar" aria-valuemin="0" aria-valuemax="100">
                                        <div runat="server" id="divTienDoThoiGianBar" class="progress-bar bg-primary">
                                        </div>
                                    </div>
                                </div>

                                <div>
                                    <div class="d-flex justify-content-between align-items-center mb-2">
                                        <span class="fs-6">
                                            Theo bình quân % hoàn thành công việc
                                        </span>

                                        <strong runat="server" id="lblTienDoCongViec" class="small text-primary">
                                        </strong>
                                    </div>

                                    <div runat="server" id="divTienDoCongViec" class="progress" role="progressbar" aria-valuemin="0" aria-valuemax="100">
                                        <div runat="server" id="divTienDoCongViecBar" class="progress-bar bg-primary">
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </section>

                    <%-- Mô tả --%>
                    <section class="mb-3">
                        <h5 class="text-uppercase fw-bold mb-3 ">
                            Mô tả
                        </h5>

                        <div class="card border shadow-none rounded-3">
                            <div class="card-body py-2 px-3">
                                <asp:Literal runat="server" ID="ltrMoTa"></asp:Literal>
                            </div>
                        </div>
                    </section>

                    <%-- Giai đoạn --%>
                    <section>
                        <SweetSoft:CtrlGiaiDoanDuAn runat="server" ID="CtrlGiaiDoanDuAn1" />
                    </section>
                </div>

                <%-- Cột phải --%>
                <div class="col-xl-4">

                    <%-- Phụ trách --%>
                    <section class="card border shadow-none mb-3 rounded-3">
                        <div class="card-body py-2 px-3">
                            <h5 class="text-uppercase fw-bold mb-3">
                                Phụ trách
                            </h5>

                            <div class="d-flex align-items-center gap-3">
                                <img runat="server" src="" ID="imgAvatarPM" class="rounded-circle" style="width:30px; height:30px; object-fit:cover;" alt="avatar"/>

                                <div>
                                    <div class="font-size-8 fw-bold mb-1">
                                        <asp:Label runat="server" ID="lblNhanVienQuanLy"></asp:Label>
                                    </div>

                                    <div class="small text-muted">
                                        Project Manager
                                        <i class="fas fa-external-link-alt ms-1"></i>
                                    </div>
                                </div>
                            </div>

                            <div class="mt-2">
                                <asp:Literal ID="ltrThanhVienGroup" runat="server"></asp:Literal>
                            </div>
                        </div>
                    </section>

                    <%-- Hợp đồng --%>
                    <asp:Panel
                        runat="server"
                        ID="pnlContract"
                        VisibleConditionKey='<%# this.IsContractView %>'>

                        <section class="card border shadow-none mb-3 rounded-3">
                            <div class="card-body py-2 px-3">
                                <h6 class="text-uppercase fw-bold mb-3">
                                    Hợp đồng thực hiện
                                </h6>

                                <asp:UpdatePanel
                                    runat="server"
                                    ID="upnlOpenContract"
                                    UpdateMode="Conditional"
                                    RenderMode="Inline">

                                    <ContentTemplate>
                                        <asp:LinkButton
                                            runat="server"
                                            ID="lbtViewContract"
                                            CausesValidation="false"
                                            CssClass="text-primary text-decoration-none"
                                            OnClick="lbtViewContract_Click">

                                            <i class="fas fa-file-contract me-2"></i>

                                            <asp:Label
                                                runat="server"
                                                ID="lblSoHopDong">
                                            </asp:Label>

                                            <i class="fas fa-external-link-alt ms-1 small"></i>
                                        </asp:LinkButton>

                                        <asp:LinkButton
                                            runat="server"
                                            ID="lbtOpenContractDocument"
                                            CausesValidation="false"
                                            CssClass="btn btn-outline-primary btn-sm mt-2"
                                            OnClick="lbtOpenContractDocument_Click"
                                            Visible="false">
                                            <i class="fas fa-folder-open me-1"></i>
                                            <%= GetResourceText(BackEndResourceKeys.CONTRACT_DOCUMENT) %>
                                        </asp:LinkButton>

                                        <asp:Label
                                            runat="server"
                                            ID="lblNoContract"
                                            CssClass="text-muted"
                                            Visible="false">
                                            Chưa có hợp đồng thực hiện
                                        </asp:Label>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </section>

                    </asp:Panel>

                    <%-- Hoạt động gần đây --%>
                    <section class="card border shadow-none rounded-3">
                        <div class="card-body py-2 px-3">

                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <h5 class="text-uppercase fw-bold mb-3 mb-0">
                                    Hoạt động gần đây
                                </h5>

                                <asp:UpdatePanel runat="server" ID="upnlOpenProjectHistory" UpdateMode="Conditional" RenderMode="Inline">

                                    <ContentTemplate>
                                        <asp:LinkButton runat="server" ID="lbtViewAllHistory" CausesValidation="false" CssClass="small text-primary text-decoration-none" OnClick="lbtViewAllHistory_Click">

                                            Xem tất cả
                                        </asp:LinkButton>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>

                            <div class="list-group list-group-flush">

                                <asp:Repeater runat="server" ID="rptRecentProjectHistory" OnItemDataBound="rptRecentProjectHistory_ItemDataBound">

                                    <ItemTemplate>
                                        <div class="list-group-item px-0 py-3">
                                            <div class="d-flex gap-3">
                                                <span class="text-primary pt-1">
                                                    <i class="far fa-circle"></i>
                                                </span>

                                                <div class="flex-grow-1">
                                                    <asp:Label runat="server" ID="lblHistoryContent" CssClass="small">
                                                    </asp:Label>

                                                    <div class="small text-muted mt-1">
                                                        <asp:Label runat="server" ID="lblHistoryTime">
                                                        </asp:Label>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>

                                <asp:Panel runat="server" ID="pnlEmptyRecentHistory" Visible="false" CssClass="text-muted small py-3">

                                    Chưa có hoạt động nào.
                                </asp:Panel>

                            </div>
                        </div>
                    </section>
                </div>
            </div>
        </div>
    </div>
</div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:CtrlLichSuDuAn runat="server" ID="CtrlLichSuDuAn1" />
    <SweetSoft:ExtraModal
    runat="server"
    ID="dlContractDetail"
    Type="Primary"
    Title="Thông tin hợp đồng thực hiện">

    <ContentTemplate>
        <div class="row">

            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label">
                        Số hợp đồng
                    </label>

                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtContractNumber"
                        Enabled="false">
                    </SweetSoft:ExtraTextBox>
                </div>
            </div>

            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label">
                        Tên hợp đồng
                    </label>

                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtContractName"
                        Enabled="false">
                    </SweetSoft:ExtraTextBox>
                </div>
            </div>

            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label">
                        Khách hàng
                    </label>

                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtContractCustomer"
                        Enabled="false">
                    </SweetSoft:ExtraTextBox>
                </div>
            </div>

            <div class="col-lg-6">
                <div class="mb-3">
                    <label class="form-label">
                        Giá trị hợp đồng
                    </label>

                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtContractValue"
                        Enabled="false">
                    </SweetSoft:ExtraTextBox>
                </div>
            </div>

            <div class="col-lg-4">
                <div class="mb-3">
                    <label class="form-label">
                        Ngày ký
                    </label>

                    <asp:TextBox
                        runat="server"
                        ID="txtContractSignDate"
                        type="date"
                        Enabled="false"
                        CssClass="form-control">
                    </asp:TextBox>
                </div>
            </div>

            <div class="col-lg-4">
                <div class="mb-3">
                    <label class="form-label">
                        Ngày hiệu lực
                    </label>

                    <asp:TextBox
                        runat="server"
                        ID="txtContractEffectiveDate"
                        type="date"
                        Enabled="false"
                        CssClass="form-control">
                    </asp:TextBox>
                </div>
            </div>

            <div class="col-lg-4">
                <div class="mb-3">
                    <label class="form-label">
                        Ngày hết hạn
                    </label>

                    <asp:TextBox
                        runat="server"
                        ID="txtContractExpiryDate"
                        type="date"
                        Enabled="false"
                        CssClass="form-control">
                    </asp:TextBox>
                </div>
            </div>

            <div class="col-lg-12">
                <div class="mb-3">
                    <label class="form-label">
                        Mô tả
                    </label>

                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtContractDescription"
                        TextMode="MultiLine"
                        Rows="4"
                        Enabled="false">
                    </SweetSoft:ExtraTextBox>
                </div>
            </div>
        </div>
    </ContentTemplate>
</SweetSoft:ExtraModal>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
</asp:Content>
