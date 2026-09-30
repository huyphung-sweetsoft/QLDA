<%@ Page Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="RiskList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fRisks.RiskList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<%@ Register Src="~/fRisks/Controls/CtrlRisk.ascx" TagPrefix="SweetSoft" TagName="CtrlRisk" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style type="text/css">
        #<%= mdlRiskCalcInfo.ClientID %> .modal-dialog { width: calc(100% - 24px) !important; max-width: 680px !important; margin: 1.5rem auto; }
        #<%= dlDetail.ClientID %> .modal-dialog { width: calc(100% - 24px) !important; max-width: 1180px !important; margin: 1rem auto; }
        #<%= dlDetail.ClientID %> .modal-body { padding: 18px 22px 14px; background: #f8fafc; } 
        
        .risk-edit-form { padding: 0; }
        .risk-edit-form .form-label { margin-bottom: 6px; font-weight: 600; color: #334155; }
        
        .risk-edit-section-title { 
            display: flex; align-items: center; gap: 7px; margin: 0 0 10px; 
            font-size: 12.5px; font-weight: 800; color: #475569; 
            text-transform: uppercase; letter-spacing: .03em; 
        }
        .risk-edit-section-title i { font-size: 13.5px; color: #64748b; }
        
        .risk-edit-form .risk-field-card { 
            padding: 14px 16px 12px; 
            border: 1px solid #cbd5e1; 
            border-radius: 9px; 
            background: #fff; 
            box-shadow: 0 1px 3px rgba(15, 23, 42, .04); 
        }
        
        /* Box ReadOnly (Mức độ rủi ro) */
        .risk-edit-form input[readonly] { background-color: #f1f5f9; font-weight: 700; color: #1e293b; border-color: #cbd5e1; }
        
        .risk-edit-form textarea.form-control { min-height: 120px; resize: vertical; line-height: 1.55; border-color: #cbd5e1; }
        
        /* Nút hỏi chấm tooltip */
        .btn-info-calc { font-size: 14px; color: #3b82f6; transition: color 0.2s; cursor: pointer; }
        .btn-info-calc:hover { color: #1d4ed8; }

        @media (max-width: 991.98px) {
            #<%= dlDetail.ClientID %> .modal-dialog { width: calc(100% - 16px) !important; }
            #<%= dlDetail.ClientID %> .modal-body { padding: 14px 16px 10px; }
            .risk-edit-form .risk-field-card { padding: 12px 14px 10px; }
        }
        @media (max-width: 767.98px) {
            #<%= dlDetail.ClientID %> .modal-dialog { width: calc(100% - 10px) !important; margin: .5rem auto; }
            .risk-edit-form textarea.form-control { min-height: 96px; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <SweetSoft:CtrlRisk runat="server" id="CtrlRisk1" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Risk Information" DefaultButton="lbtSubmit">
        <ContentTemplate>
            <div class="risk-edit-form js-validation validationEngineContainer">
                
                <div class="row g-3 mb-3">
                    
                    <div class="col-lg-8">
                        <div class="row g-3">
                            <div class="col-12">
                                <div class="risk-field-card">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.RISK_NAME) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtTenRuiRo" Required="true" PlaceHolder="Nhập tên rủi ro..."></SweetSoft:ExtraTextBox>
                                </div>
                            </div>
                            
                            <div class="col-md-4">
                                <div class="risk-field-card h-100">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROBABILITY) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlXacSuat" Required="true" SimpleInit="true" PlaceHolder="Chọn giá trị" AutoPostBack="true" OnSelectedIndexChanged="ddlXacSuat_SelectedIndexChanged"></SweetSoft:ExtraDropdown>
                                </div>
                            </div>
                            <div class="col-md-4">
                                <div class="risk-field-card h-100">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.IMPACT) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlMucDoAnhHuong" Required="true" SimpleInit="true" PlaceHolder="Chọn giá trị" AutoPostBack="true" OnSelectedIndexChanged="ddlMucDoAnhHuong_SelectedIndexChanged"></SweetSoft:ExtraDropdown>
                                </div>
                            </div>
                            <div class="col-md-4">
                                <div class="risk-field-card h-100">
                                    <label class="form-label d-flex align-items-center gap-1">
                                        <%= GetResourceText(BackEndResourceKeys.RISK_LEVEL) %>
                                        <a href="javascript:;" onclick="openRiskCalcModal(); return false;" title="Tính mức độ rủi ro" class="btn-info-calc ms-1">
                                            <i class="fas fa-question-circle"></i>
                                        </a>
                                    </label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtMucDoRuiRo" ReadOnly="true" PlaceHolder="Tự động tính..."></SweetSoft:ExtraTextBox>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-lg-4 d-flex flex-column">
                        <div class="risk-field-card flex-grow-1">
                            <div class="risk-edit-section-title"><i class="fas fa-user-shield"></i> Giám sát rủi ro</div>
                            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.MONITOR) %></label>
                            <SweetSoft:ExtraDropdown runat="server" ID="ddlNhanVien" SimpleInit="true" PlaceHolder="Chọn nhân viên theo dõi"></SweetSoft:ExtraDropdown>
                            
                            <div class="mt-3 text-muted" style="font-size: 11.5px; line-height: 1.4;">
                                <i class="fas fa-lightbulb text-warning me-1"></i> 
                                Chọn một nhân viên để theo dõi và cập nhật trạng thái của rủi ro này trong suốt vòng đời dự án.
                            </div>
                        </div>
                    </div>
                    
                </div>

                <div class="row g-3">
                    <div class="col-md-6 d-flex flex-column">
                        <div class="risk-field-card flex-grow-1 d-flex flex-column">
                            <div class="risk-edit-section-title"><i class="fas fa-shield-alt"></i> <%= GetResourceText(BackEndResourceKeys.MITIGATION) %></div>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtKeHoachPhongNgua" TextMode="MultiLine" CssClass="form-control flex-grow-1" PlaceHolder="Nhập kế hoạch phòng ngừa..."></SweetSoft:ExtraTextBox>
                        </div>
                    </div>
                    <div class="col-md-6 d-flex flex-column">
                        <div class="risk-field-card flex-grow-1 d-flex flex-column">
                            <div class="risk-edit-section-title"><i class="fas fa-fire-extinguisher"></i> <%= GetResourceText(BackEndResourceKeys.HANDLING_PLAN) %></div>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtKeHoachUngPho" TextMode="MultiLine" CssClass="form-control flex-grow-1" PlaceHolder="Nhập kế hoạch ứng phó..."></SweetSoft:ExtraTextBox>
                        </div>
                    </div>
                </div>

            </div>
        </ContentTemplate>
        <FooterTemplate>
            <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true"
                        OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false">Lưu</SweetSoft:ExtraButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>
    </SweetSoft:ExtraModal>

    <SweetSoft:ExtraModal runat="server" ID="mdlRiskCalcInfo" Type="Primary" Title="Cách tính Điểm rủi ro">
        <ContentTemplate>
            <div class="p-3" style="font-size: 14.5px; line-height: 1.65; color: #475569;">
                <p class="mb-2"><strong>Công thức gốc:</strong></p>
                <div class="p-3 mb-4 bg-white border border-primary rounded text-center" style="border-style: dashed !important;">
                    <strong class="text-primary" style="font-size: 17px;">Điểm rủi ro = (Xác suất / 100) × Mức độ ảnh hưởng</strong>
                </div>
                
                <p class="mb-3 fw-bold" style="color: #334155;">Hệ thống phân loại Mức độ rủi ro dựa trên số điểm này theo 2 trường hợp:</p>
                
                <p class="mb-1 text-danger fw-bold"><i class="fas fa-exclamation-triangle me-1"></i> 1. Trường hợp ưu tiên (Nguy cơ cao):</p>
                <p class="mb-2 text-muted fst-italic ms-3" style="font-size: 13.5px;">Áp dụng khi Xác suất ≥ 75% HOẶC Mức độ ảnh hưởng ≥ 4:</p>
                <ul class="ms-4 mb-4 text-muted">
                    <li><strong>Rất cao:</strong> Điểm rủi ro ≥ 4.5</li>
                    <li><strong>Cao:</strong> Điểm rủi ro < 4.5</li>
                </ul>

                <p class="mb-1 text-primary fw-bold"><i class="fas fa-list-ul me-1"></i> 2. Trường hợp thông thường:</p>
                <p class="mb-2 text-muted fst-italic ms-3" style="font-size: 13.5px;">Áp dụng cho các chỉ số còn lại:</p>
                <ul class="ms-4 mb-4 text-muted">
                    <li><strong>Rất cao:</strong> Điểm rủi ro ≥ 4.5</li>
                    <li><strong>Cao:</strong> 3.5 ≤ Điểm rủi ro < 4.5</li>
                    <li><strong>Trung bình:</strong> 2.0 ≤ Điểm rủi ro < 3.5</li>
                    <li><strong>Thấp:</strong> 1.0 ≤ Điểm rủi ro < 2.0</li>
                    <li><strong>Rất thấp:</strong> Điểm rủi ro < 1.0</li>
                </ul>
                
                <div class="alert alert-info py-2 px-3 mb-0" style="font-size: 13px; border-radius: 8px;">
                    <i class="fas fa-lightbulb me-1 text-warning"></i> <strong>Lưu ý:</strong> Nếu chưa nhập đủ thông tin, kết quả hiển thị là "--". Nếu có dữ liệu, kết quả gồm Tên mức độ kèm theo Điểm số (VD: Trung bình (2.5)).
                </div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        function openRiskCalcModal() {
            var $calcModal = $('#<%= mdlRiskCalcInfo.ClientID %>');
            $calcModal.modal('show');
            setTimeout(function () {
                $calcModal.css('z-index', '99999');
                $('.modal-backdrop').last().css('z-index', '99998');
            }, 150);
        }
    </script>
</asp:Content>