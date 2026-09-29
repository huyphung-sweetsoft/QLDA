<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="IssueList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fIssues.IssueList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<%@ Register Src="~/fIssues/Controls/CtrlIssue.ascx" TagPrefix="SweetSoft" TagName="CtrlIssue" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
<style type="text/css">
        #<%= dlDetail.ClientID %> .modal-dialog { width: calc(100% - 24px) !important; max-width: 1180px !important; margin: 1rem auto; }
        #<%= dlDetail.ClientID %> .modal-body { padding: 18px 22px 14px; background: #f8fafc; } 
        .issue-edit-form { padding: 0; }
        .issue-edit-form .form-label { margin-bottom: 6px; font-weight: 600; color: #334155; }
        .issue-edit-section-title { 
            display: flex; align-items: center; gap: 7px; margin: 0 0 10px; 
            font-size: 12.5px; font-weight: 800; color: #475569; 
            text-transform: uppercase; letter-spacing: .03em; 
        }
        .issue-edit-section-title i { font-size: 13.5px; color: #64748b; }
        .issue-edit-form .issue-field-card { 
            padding: 14px 16px 12px; 
            border: 1px solid #cbd5e1; 
            border-radius: 9px; 
            background: #fff; 
            box-shadow: 0 1px 3px rgba(15, 23, 42, .04); 
        }
        .issue-edit-form textarea.form-control { min-height: 120px; resize: vertical; line-height: 1.55; border-color: #cbd5e1; }
        .issue-edit-assignee-card { display: flex; flex-direction: column; }
        .issue-edit-assignee-list { flex: 1; display: flex; flex-direction: column; }
        .issue-edit-assignee-row { display: flex; align-items: center; gap: 9px; min-width: 0; padding: 8px 0; }
        .issue-edit-assignee-row + .issue-edit-assignee-row { border-top: 1px solid #eef2f7; }
        .issue-edit-assignee-avatar { width: 38px; height: 38px; flex: 0 0 38px; }
        .issue-edit-assignee-avatar .issue-edit-person-avatar { width: 38px; height: 38px; border-radius: 50%; display: flex; align-items: center; justify-content: center; object-fit: cover; color: #fff; font-size: 11px; font-weight: 800; border: 1px solid rgba(15,23,42,.05); }
        .issue-edit-assignee-main { min-width: 0; }
        .issue-edit-assignee-name { color: #1f2937; font-size: 13px; font-weight: 750; line-height: 1.25; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .issue-edit-assignee-email { margin-top: 2px; color: #6b7280; font-size: 11px; line-height: 1.25; word-break: break-all; }
        .issue-edit-empty-assignee { padding: 12px; background: #f8fafc; border: 1px dashed #cbd5e1; border-radius: 7px; color: #64748b; font-size: 12px; font-weight: 500; text-align: center; }
        @media (max-width: 991.98px) {
            #<%= dlDetail.ClientID %> .modal-dialog { width: calc(100% - 16px) !important; }
            #<%= dlDetail.ClientID %> .modal-body { padding: 14px 16px 10px; }
            .issue-edit-form .issue-field-card { padding: 12px 14px 10px; }
        }
        @media (max-width: 767.98px) {
            #<%= dlDetail.ClientID %> .modal-dialog { width: calc(100% - 10px) !important; margin: .5rem auto; }
            .issue-edit-form textarea.form-control { min-height: 96px; }
        }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <SweetSoft:CtrlIssue runat="server" ID="CtrlIssue1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" DefaultButton="lbtSubmit">
        <ContentTemplate>
            <div class="issue-edit-form js-validation validationEngineContainer">
                <div class="row g-3 mb-3">
                    <div class="col-lg-8">
                        <div class="row g-3">
                            <div class="col-12">
                                <div class="row g-3">
                                    <div class="col-md">
                                        <div class="issue-field-card h-100">
                                            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.ISSUE_NAME) %></label>
                                            <SweetSoft:ExtraTextBox runat="server" ID="txtTenVanDe" Required="true"></SweetSoft:ExtraTextBox>
                                        </div>
                                    </div>
                                    <div class="col-md-4 col-xl-3" runat="server" id="divTrangThaiVanDe">
                                        <div class="issue-field-card h-100">
                                            <label class="form-label">Trạng thái vấn đề</label>
                                            <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThaiVanDe" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="issue-field-card h-100">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.IMPACT) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlMucDoAnhHuong" Required="true" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="issue-field-card h-100">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.ORIGIN) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlNguonGocVanDe" Required="true" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="issue-field-card h-100">
                                    <label class="form-label">Vấn đề từ công việc</label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlCongViecPhatSinh" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="issue-field-card h-100">
                                    <label class="form-label">Công việc bị ảnh hưởng</label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlCongViecBiAnhHuong" SimpleInit="true"  AutoPostBack="true" OnSelectedIndexChanged="ddlCongViecBiAnhHuong_SelectedIndexChanged"></SweetSoft:ExtraDropdown>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-lg-4 d-flex flex-column">
                        <div class="issue-field-card issue-edit-assignee-card flex-grow-1">
                            <div class="issue-edit-section-title"><i class="fas fa-users"></i> Nhân viên xử lý</div>
                            
                            <asp:Repeater ID="rptNhanVienXuLy" runat="server">
                                <HeaderTemplate><div class="issue-edit-assignee-list"></HeaderTemplate>
                                <ItemTemplate>
                                    <div class="issue-edit-assignee-row">
                                        <div class="issue-edit-assignee-avatar"><%# Eval("AvatarHtml") %></div>
                                        <div class="issue-edit-assignee-main">
                                            <div class="issue-edit-assignee-name"><%# Eval("DisplayName") %></div>
                                            <div class="issue-edit-assignee-email"><%# Eval("Email") %></div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                                <FooterTemplate></div></FooterTemplate>
                            </asp:Repeater>
                            
                            <asp:Panel ID="pnlNoNhanVienXuLy" runat="server" CssClass="issue-edit-empty-assignee" Visible="false">
                                Công việc này không có nhân viên đảm nhận
                            </asp:Panel>
                        </div>
                    </div>
                </div>
                <div class="row g-3">
                    <div class="col-md-6 d-flex flex-column">
                        <div class="issue-field-card flex-grow-1 d-flex flex-column">
                            <div class="issue-edit-section-title"><i class="fas fa-align-left"></i> Nội dung vấn đề</div>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtMoTaChiTiet" TextMode="MultiLine" CssClass="form-control flex-grow-1"></SweetSoft:ExtraTextBox>
                        </div>
                    </div>
                    <div class="col-md-6 d-flex flex-column">
                        <div class="issue-field-card flex-grow-1 d-flex flex-column">
                            <div class="issue-edit-section-title"><i class="fas fa-list"></i> Kế hoạch xử lý</div>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtKeHoachXuLy" TextMode="MultiLine" CssClass="form-control flex-grow-1"></SweetSoft:ExtraTextBox>
                        </div>
                    </div>
                </div>

            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false"></SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server"></asp:Content>
