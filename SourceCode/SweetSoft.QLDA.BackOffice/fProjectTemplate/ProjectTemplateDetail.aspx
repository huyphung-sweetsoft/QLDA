<%@ Page Title="Chi tiết mẫu công việc" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="ProjectTemplateDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjectTemplate.ProjectTemplateDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fProjectTemplate/Controls/CtrlProjectTemplateDetail.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTemplateDetail" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .add-phase-template-body { padding: 4px 4px 0; }
        .add-phase-template-section { padding: 18px; border: 1px solid #e2e8f0; border-radius: 12px; background: #f8fafc; }
        .add-phase-template-section + .add-phase-template-section { margin-top: 14px; }
        .add-phase-template-label { display: block; margin-bottom: 7px; color: #334155; font-size: 13px; font-weight: 700; }
        .add-phase-template-label .required { color: #ef4444; }
        .add-phase-template-input { min-height: 46px; border-color: #cbd5e1; border-radius: 9px; font-size: 14px; transition: border-color .15s ease, box-shadow .15s ease; }
        .add-phase-template-input:focus { border-color: #6366f1; box-shadow: 0 0 0 3px rgba(99, 102, 241, .12); }
        .add-phase-template-help { margin-top: 7px; color: #94a3b8; font-size: 12px; }
        .add-phase-template-dependency-wrap { position: relative; }
        .add-phase-template-dependency-icon { position: absolute; top: 50%; left: 15px; z-index: 2; color: #64748b; pointer-events: none; transform: translateY(-50%); }
        .add-phase-template-dependency { padding-left: 40px; }
        
        .context-info-box { background: #eff6ff; border: 1px solid #bfdbfe; border-radius: 9px; padding: 12px 16px; margin-bottom: 14px; }
        .context-info-item { font-size: 13px; line-height: 1.5; color: #1e3a8a; }
        .context-info-item strong { color: #1e40af; font-weight: 800; }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen template-detail-card">
                <SweetSoft:Navigation runat="server" ID="Navigation1" />
                <SweetSoft:CtrlProjectTemplateDetail ID="CtrlProjectTemplateDetail1" runat="server" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="mdlAddPhaseTemplate" Type="Primary" Title="Thông tin công việc">
        <ContentTemplate>
            <asp:UpdatePanel ID="upAddPhaseTemplate" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:HiddenField ID="hdfParentTaskId" runat="server" />
                    <!-- HiddenField lưu ID công việc đang sửa (nếu rỗng -> Thêm mới) -->
                    <asp:HiddenField ID="hdfEditTaskId" runat="server" />

                    <div class="add-phase-template-body">
                        
                        <div class="context-info-box" id="divContextInfo" runat="server" visible="false">
                            <div class="context-info-item mb-1">
                                <span class="opacity-75">Thuộc giai đoạn:</span> <asp:Literal ID="ltrGiaiDoan" runat="server"></asp:Literal>
                            </div>
                            <div class="context-info-item">
                                <span class="opacity-75">Công việc cha:</span> <asp:Literal ID="ltrCongViecCha" runat="server"></asp:Literal>
                            </div>
                        </div>

                        <div class="add-phase-template-section">
                            <label class="add-phase-template-label" id="lblTaskName" runat="server">
                                Mã & Tên công việc <span class="required">*</span>
                            </label>
                            
                            <div class="d-flex gap-2">
                                <asp:TextBox ID="txtMaCongViec" runat="server" CssClass="form-control add-phase-template-input bg-light text-primary text-center fw-bold" style="width: 90px; flex: 0 0 90px;" ReadOnly="true"></asp:TextBox>
                                <asp:TextBox ID="txtTenGiaiDoanTemplate" runat="server" CssClass="form-control add-phase-template-input" style="flex: 1;" MaxLength="500"></asp:TextBox>
                            </div>
                        </div>

                        <div class="add-phase-template-section" id="divThoiHan" runat="server">
                            <label class="add-phase-template-label">
                                Thời hạn (ngày) <span class="required">*</span>
                            </label>
                            <asp:TextBox ID="txtThoiHanNgay" runat="server" CssClass="form-control add-phase-template-input" TextMode="Number" min="1"></asp:TextBox>
                        </div>

                        <div class="add-phase-template-section">
                            <label class="add-phase-template-label"><%= GetResourceText(BackEndResourceKeys.DEPENDENT) %></label>
                            <div class="add-phase-template-dependency-wrap">
                                <i class="fas fa-link add-phase-template-dependency-icon"></i>
                                <asp:DropDownList ID="ddlPhuThuocTemplate" runat="server" CssClass="form-select add-phase-template-input add-phase-template-dependency"></asp:DropDownList>
                            </div>
                            <div class="add-phase-template-help">Chọn công việc mà mục này sẽ phụ thuộc vào.</div>
                        </div>
                        
                        <div class="add-phase-template-section">
                            <label class="add-phase-template-label">Mô tả chi tiết</label>
                            <asp:TextBox ID="txtMoTa" runat="server" CssClass="form-control add-phase-template-input" TextMode="MultiLine" Rows="3" placeholder="Nhập ghi chú hoặc mô tả về công việc..."></asp:TextBox>
                        </div>

                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </ContentTemplate>
        <FooterTemplate>
            <asp:UpdatePanel ID="upnlFooterAddPhaseTemplate" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:LinkButton ID="btnSavePhaseTemplate" runat="server" CssClass="btn btn-primary" OnClick="btnSavePhaseTemplate_Click">
                        <i class="fas fa-save me-1"></i><%= GetResourceText(BackEndResourceKeys.SAVE) %>
                    </asp:LinkButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>