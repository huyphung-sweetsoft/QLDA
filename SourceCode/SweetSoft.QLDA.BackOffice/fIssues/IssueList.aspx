<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="IssueList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fIssues.IssueList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<%@ Register Src="~/fIssues/Controls/CtrlIssue.ascx" TagPrefix="SweetSoft" TagName="CtrlIssue" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style type="text/css">
        #<%= dlDetail.ClientID %> .modal-dialog { 
            width: calc(100% - 24px) !important; 
            max-width: 1180px !important; 
            margin: 1.8rem auto 1.5rem !important; 
        }

        #<%= dlDetail.ClientID %> .modal-content {
            width: 100%;
            margin: auto; 
        }
        .issue-edit-form { padding: 0; }
        .issue-edit-form .form-label { margin-bottom: 6px; font-weight: 600; color: #334155; font-size: 14px; line-height: normal; }

        .issue-edit-form input[type="text"].form-control {
            border: 1px solid #475569 !important;
        }

        .issue-form-section {
            min-width: 0;
            width: 100%; 
            margin: 0; 
            padding: 8px 16px 16px;
            border: 1px solid #475569; /* Viền xám đen rõ nét */
            border-radius: 8px;
            background: #fff;
            box-sizing: border-box;
            display: flex;
            flex-direction: column;
        }

        .issue-section-legend {
            display: block; 
            width: max-content;
            max-width: 100%;
            margin: 0 0 10px 0;
            padding: 0 8px;
            background: #fff;
            color: #334155;
            font-size: 14px; 
            font-weight: 600; 
            line-height: 1.2;
            letter-spacing: normal;
            text-transform: none; /* Tắt in hoa */
        }

        .issue-section-legend i {
            color: #64748b;
            font-size: 14px;
            margin-right: 6px;
        }

        .issue-form-section textarea.form-control { 
            border: none !important;
            padding: 0 !important;
            background: transparent !important;
            box-shadow: none !important;
            min-height: 110px; 
            resize: vertical; 
            line-height: 1.55; 
        }
        .select2-container--default .select2-selection--single {
            height: auto !important; 
            min-height: 42px; /* Tăng chiều cao */
            border: 1px solid #475569 !important; /* ĐEN/XÁM ĐEN */
            border-radius: 6px !important;
            background-color: #fff !important;
            outline: none !important;
            display: flex !important;
            align-items: center !important; 
            position: relative;
        }

        .select2-container--default .select2-selection--single .select2-selection__rendered {
            line-height: normal !important; 
            padding-left: 14px !important;
            padding-right: 30px !important; 
            color: #1f2937 !important; 
            font-size: 15px !important; 
            width: 100%;
        }

        .select2-container--default .select2-selection--single .select2-selection__arrow {
            height: 100% !important; 
            right: 8px !important;
            display: flex !important;
            align-items: center !important; 
            position: absolute !important; 
        }

        .select2-container--default.select2-container--focus .select2-selection--single {
            border-color: #8b5cf6 !important;
            box-shadow: 0 0 0 3px rgba(139, 92, 246, 0.15) !important;
        }

        .select2-container--default.select2-container--open.select2-container--below .select2-selection--single {
            border-bottom-left-radius: 0 !important;
            border-bottom-right-radius: 0 !important;
            border-color: #8b5cf6 !important;
        }
        .select2-dropdown--below {
            border: 1px solid #8b5cf6 !important; 
            border-top: none !important; 
            border-bottom-left-radius: 6px !important;
            border-bottom-right-radius: 6px !important;
            box-shadow: 0 10px 15px -3px rgba(139, 92, 246, 0.15) !important; 
            margin-top: 0 !important; 
        }

        .select2-container--default.select2-container--open.select2-container--above .select2-selection--single {
            border-top-left-radius: 0 !important;
            border-top-right-radius: 0 !important;
            border-color: #8b5cf6 !important;
        }
        .select2-dropdown--above {
            border: 1px solid #8b5cf6 !important;
            border-bottom: none !important;
            border-top-left-radius: 6px !important;
            border-top-right-radius: 6px !important;
            box-shadow: 0 -10px 15px -3px rgba(139, 92, 246, 0.15) !important;
            margin-bottom: 0 !important;
        }

        .select2-results__options { 
            padding: 4px 0 !important; 
            max-height: 240px !important; 
        }

        .select2-results__option {
            padding: 12px 16px !important; 
            font-size: 15px !important; 
            color: #334155 !important;
            transition: background-color 0.15s ease !important;
            border-radius: 0 !important; 
        }

        .select2-container--default .select2-results__option--highlighted[aria-selected],
        .select2-container--default .select2-results__option--highlighted[aria-selected]:hover {
            background-color: #f1f5f9 !important; 
            color: #0f172a !important;
        }

        .select2-container--default .select2-results__option[aria-selected=true] {
            background-color: #f5f3ff !important; 
            color: #542e88 !important; 
            font-weight: 600 !important;
        }

        .select2-search--dropdown {
            padding: 10px 12px 6px !important; 
        }
        .select2-search--dropdown .select2-search__field {
            border: 1px solid #475569 !important; /* Viền đen cho ô Search */
            border-radius: 6px !important;
            padding: 8px 12px !important;
            font-size: 14px !important;
            color: #334155 !important;
            outline: none !important;
            transition: all 0.2s ease !important;
        }
        .select2-search--dropdown .select2-search__field:focus {
            border-color: #8b5cf6 !important;
            box-shadow: 0 0 0 3px rgba(139, 92, 246, 0.15) !important;
        }
        .issue-edit-assignee-list { flex: 1; display: flex; flex-direction: column; margin-top: 5px; }
        .issue-edit-assignee-row { display: flex; align-items: center; gap: 10px; min-width: 0; padding: 8px 0; }
        .issue-edit-assignee-row + .issue-edit-assignee-row { border-top: 1px solid #eef2f7; }
        .issue-edit-assignee-avatar { width: 38px; height: 38px; flex: 0 0 38px; }
        .issue-edit-assignee-avatar .issue-edit-person-avatar { width: 38px; height: 38px; border-radius: 50%; display: flex; align-items: center; justify-content: center; object-fit: cover; color: #fff; font-size: 12px; font-weight: 800; border: 1px solid rgba(15,23,42,.05); }
        .issue-edit-assignee-main { min-width: 0; }
        .issue-edit-assignee-name { color: #1e3a8a; font-size: 14px; font-weight: 750; line-height: 1.25; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .issue-edit-assignee-email { margin-top: 3px; color: #64748b; font-size: 12px; line-height: 1.25; word-break: break-all; }
        .issue-edit-empty-assignee { padding: 12px; background: #f8fafc; border: 1px dashed #cbd5e1; border-radius: 7px; color: #64748b; font-size: 12px; font-weight: 500; text-align: center; }

        @media (max-width: 991.98px) {
            #<%= dlDetail.ClientID %> .modal-dialog { width: calc(100% - 16px) !important; }
            #<%= dlDetail.ClientID %> .modal-body { padding: 14px 16px 10px; }
            .issue-form-section { padding: 10px 12px 12px; }
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
                        <fieldset class="issue-form-section">
                            <legend class="issue-section-legend"><i class="fas fa-exclamation-triangle text-danger"></i>Thông tin vấn đề</legend>
                            <div class="row g-3">
                                
                                <div class="col-12">
                                    <div class="row g-3">
                                        <div class="col-md">
                                            <div>
                                                <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.ISSUE_NAME) %></label>
                                                <SweetSoft:ExtraTextBox runat="server" ID="txtTenVanDe" Required="true"></SweetSoft:ExtraTextBox>
                                            </div>
                                        </div>
                                        <div class="col-md-4 col-xl-3" runat="server" id="divTrangThaiVanDe">
                                            <div>
                                                <label class="form-label">Trạng thái vấn đề</label>
                                                <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThaiVanDe" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="col-md-6">
                                    <div>
                                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.IMPACT) %></label>
                                        <SweetSoft:ExtraDropdown runat="server" ID="ddlMucDoAnhHuong" Required="true" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div>
                                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.ORIGIN) %></label>
                                        <SweetSoft:ExtraDropdown runat="server" ID="ddlNguonGocVanDe" Required="true" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div>
                                        <label class="form-label">Vấn đề từ công việc</label>
                                        <SweetSoft:ExtraDropdown runat="server" ID="ddlCongViecPhatSinh" SimpleInit="true"></SweetSoft:ExtraDropdown>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div>
                                        <label class="form-label">Công việc bị ảnh hưởng</label>
                                        <SweetSoft:ExtraDropdown runat="server" ID="ddlCongViecBiAnhHuong" SimpleInit="true" AutoPostBack="true" OnSelectedIndexChanged="ddlCongViecBiAnhHuong_SelectedIndexChanged"></SweetSoft:ExtraDropdown>
                                    </div>
                                </div>
                                
                            </div>
                        </fieldset>
                    </div>

                    <div class="col-lg-4 d-flex flex-column">
                        <fieldset class="issue-form-section flex-grow-1">
                            <legend class="issue-section-legend"><i class="fas fa-users text-info"></i>Nhân viên xử lý</legend>
                            
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
                        </fieldset>
                    </div>
                </div>

                <div class="row g-3">
                    <div class="col-md-6 d-flex flex-column">
                        <fieldset class="issue-form-section flex-grow-1 h-100">
                            <legend class="issue-section-legend"><i class="fas fa-align-left text-success"></i>Nội dung vấn đề</legend>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtMoTaChiTiet" TextMode="MultiLine" CssClass="form-control flex-grow-1 border-0 p-0"></SweetSoft:ExtraTextBox>
                        </fieldset>
                    </div>
                    <div class="col-md-6 d-flex flex-column">
                        <fieldset class="issue-form-section flex-grow-1 h-100">
                            <legend class="issue-section-legend"><i class="fas fa-list text-warning"></i>Kế hoạch xử lý</legend>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtKeHoachXuLy" TextMode="MultiLine" CssClass="form-control flex-grow-1 border-0 p-0"></SweetSoft:ExtraTextBox>
                        </fieldset>
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
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        function forceShowSearchDropdown() {
            $('.issue-edit-form select').each(function () {
                var $select = $(this);

                if ($select.hasClass("select2-hidden-accessible")) {
                    $select.select2('destroy');
                }

                $select.select2({
                    width: '100%',
                    minimumResultsForSearch: 0, 
                    dropdownParent: $('#<%= dlDetail.ClientID %>')
                });
            });
        }

        $(document).ready(function () {
            setTimeout(forceShowSearchDropdown, 100);
        });

        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
                setTimeout(forceShowSearchDropdown, 100);
            });
        }
    </script>
</asp:Content>