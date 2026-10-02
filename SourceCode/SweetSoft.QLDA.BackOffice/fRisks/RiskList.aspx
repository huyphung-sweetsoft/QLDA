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
        
        /* Đồng bộ kích thước label chuẩn */
        .risk-edit-form .form-label { margin-bottom: 6px; font-weight: 600; color: #334155; font-size: 14px; line-height: normal; }

        /* =======================================================
           FIELDSET VÀ LEGEND (GOM NHÓM GIAO DIỆN CHUẨN)
           ======================================================= */
        .risk-form-section {
            min-width: 0;
            width: 100%; 
            margin: 0; 
            padding: 8px 16px 16px;
            border: 1px solid #475569;
            border-radius: 8px;
            background: #fff;
            box-sizing: border-box;
            display: flex;
            flex-direction: column;
        }

        .risk-section-legend {
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
            text-transform: none; 
        }

        .risk-section-legend i {
            color: #64748b;
            font-size: 14px;
            margin-right: 6px;
        }

        /* Class để CĂN BẰNG (ALign) các ô không viền bên trái với thẻ Đỏ bên phải */
        .align-bare-field {
            padding-top: 8px; /* Căn chỉnh đẩy dropdown xuống để viền đáy bằng tăm tắp thẻ đỏ */
        }

        /* ĐẬM VIỀN MÀU ĐEN (XÁM ĐEN) CHO TEXTBOX */
        .risk-edit-form input[type="text"].form-control:not(.risk-level-value) {
            border: 1px solid #475569 !important; /* Đen/xám đen rõ ràng */
        }

        .risk-form-section textarea.form-control { 
            border: none !important;
            padding: 0 !important;
            background: transparent !important;
            box-shadow: none !important;
            min-height: 110px; 
            resize: vertical; 
            line-height: 1.55; 
        }

        /* =======================================================
           THẺ HIỂN THỊ MỨC ĐỘ RỦI RO (THEME: ĐỎ CẢNH BÁO)
           ======================================================= */
        .risk-level-card {
            padding: 15px 14px 7px !important; /* Tăng top để ép chữ xuống thấp, giảm bottom để rút viền lên */
            border: 1px solid #fca5a5 !important; 
            border-radius: 9px; 
            background-color: #fef2f2 !important; 
            display: flex;
            flex-direction: column;
            justify-content: flex-start; 
            box-shadow: 0 1px 3px rgba(220, 38, 38, 0.08) !important; 
        }

        .risk-level-card .form-label {
            margin-bottom: -2px !important; /* Kéo chữ "Mức độ rủi ro" cực sát vào chữ "Cao (3)" */
            font-size: 14.5px !important;
            font-weight: 600 !important; 
            color: #dc2626 !important; 
            text-transform: none !important; 
            letter-spacing: normal !important;
            line-height: normal !important;
        }

        .risk-level-card .btn-info-calc {
            color: #dc2626; 
            font-size: 14px;
            transition: color 0.2s; 
            cursor: pointer;
        }
        .risk-level-card .btn-info-calc:hover { color: #991b1b; }

        .risk-level-card .risk-level-value,
        .risk-level-card input[readonly].risk-level-value,
        .risk-level-card input[disabled].risk-level-value {
            background: transparent !important; 
            background-image: none !important; 
            border: none !important; 
            padding: 0 !important;
            height: 42px !important; /* Trả lại 42px để to bằng ô dropdown bên trái */
            line-height: 42px !important;
            color: #991b1b !important; 
            font-size: 15px !important;
            font-weight: 700 !important; 
            box-shadow: none !important;
            cursor: default !important;
            text-align: left !important;
            -webkit-appearance: none !important;
        }
        .risk-level-card .risk-level-value::placeholder {
            color: #f87171 !important; 
            font-weight: 600 !important;
        }

        /* =======================================================
           TÙY CHỈNH DROPDOWN (VIỀN ĐEN ĐẬM)
           ======================================================= */
        .select2-container--default .select2-selection--single {
            height: auto !important; 
            min-height: 42px;
            border: 1px solid #475569 !important; /* ĐEN/XÁM ĐEN RÕ RÀNG Y NHƯ YÊU CẦU */
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

        .select2-results__options { padding: 4px 0 !important; }
        
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
        .select2-search--dropdown { padding: 10px 12px 6px !important; }
        .select2-search--dropdown .select2-search__field {
            border: 1px solid #475569 !important; /* Viền đen cho ô search */
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

        /* Template CSS User */
        .select2-user-result {
            display: flex;
            align-items: center;
            gap: 10px;
            padding: 4px 0;
        }
        .select2-selection__rendered .select2-user-result { padding: 8px 0; }
        .select2-user-avatar-wrap {
            width: 36px; height: 36px; flex: 0 0 36px;
            border-radius: 50%; overflow: hidden;
            display: flex; align-items: center; justify-content: center;
            color: #fff; font-size: 12px; font-weight: 800;
            border: 1px solid rgba(15,23,42,.05);
        }
        .select2-user-avatar-wrap img { width: 100%; height: 100%; object-fit: cover; }
        .select2-user-info { display: flex; flex-direction: column; min-width: 0; }
        .select2-user-name { color: #1e3a8a; font-size: 14px; font-weight: 750; line-height: 1.2; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .select2-user-email { margin-top: 3px; color: #64748b; font-size: 12px; line-height: 1.2; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

        @media (max-width: 991.98px) {
            #<%= dlDetail.ClientID %> .modal-dialog { width: calc(100% - 16px) !important; }
            #<%= dlDetail.ClientID %> .modal-body { padding: 14px 16px 10px; }
            .risk-level-card { padding: 12px 14px 10px !important; }
            .risk-form-section { padding: 10px 12px 12px; }
            .align-bare-field { padding-top: 0; }
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
                
                <!-- HÀNG 1: THÔNG TIN RỦI RO & GIÁM SÁT -->
                <div class="row g-3 mb-3">
                    
                    <!-- Nhóm Trái: Thông tin rủi ro -->
                    <div class="col-lg-8">
                        <fieldset class="risk-form-section">
                            <legend class="risk-section-legend"><i class="fas fa-exclamation-triangle text-danger"></i>Thông tin rủi ro</legend>
                            <div class="row g-3">
                                
                                <div class="col-12">
                                    <div class="align-bare-field">
                                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.RISK_NAME) %></label>
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenRuiRo" Required="true" PlaceHolder="Nhập tên rủi ro..."></SweetSoft:ExtraTextBox>
                                    </div>
                                </div>
                                
                                <div class="col-md-4">
                                    <div class="align-bare-field">
                                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PROBABILITY) %></label>
                                        <SweetSoft:ExtraDropdown runat="server" ID="ddlXacSuat" Required="true" SimpleInit="true" PlaceHolder="Chọn giá trị" AutoPostBack="true" OnSelectedIndexChanged="ddlXacSuat_SelectedIndexChanged"></SweetSoft:ExtraDropdown>
                                    </div>
                                </div>
                                
                                <div class="col-md-4">
                                    <div class="align-bare-field">
                                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.IMPACT) %></label>
                                        <SweetSoft:ExtraDropdown runat="server" ID="ddlMucDoAnhHuong" Required="true" SimpleInit="true" PlaceHolder="Chọn giá trị" AutoPostBack="true" OnSelectedIndexChanged="ddlMucDoAnhHuong_SelectedIndexChanged"></SweetSoft:ExtraDropdown>
                                    </div>
                                </div>
                                
                                <div class="col-md-4">
                                    <div class="risk-level-card">
                                        <label class="form-label d-flex align-items-center gap-1">
                                            <%= GetResourceText(BackEndResourceKeys.RISK_LEVEL) %>
                                            <a href="javascript:;" onclick="openRiskCalcModal(); return false;" title="Tính mức độ rủi ro" class="btn-info-calc ms-1">
                                                <i class="fas fa-question-circle"></i>
                                            </a>
                                        </label>
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtMucDoRuiRo" ReadOnly="true" PlaceHolder="Tự động tính..." CssClass="risk-level-value"></SweetSoft:ExtraTextBox>
                                    </div>
                                </div>
                                
                            </div>
                        </fieldset>
                    </div>

                    <!-- Nhóm Phải: Giám sát rủi ro -->
                    <div class="col-lg-4 d-flex flex-column">
                        <fieldset class="risk-form-section flex-grow-1">
                            <legend class="risk-section-legend"><i class="fas fa-user-shield text-info"></i>Giám sát rủi ro</legend>
                            
                            <!-- Đã thêm lại Nhãn và bọc trong class align-bare-field để bằng hàng với bên trái -->
                            <div class="align-bare-field">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.MONITOR) %></label>
                                <SweetSoft:ExtraDropdown runat="server" ID="ddlNhanVien" SimpleInit="true" PlaceHolder="Chọn nhân viên theo dõi"></SweetSoft:ExtraDropdown>
                            </div>
                            
                            <!-- Đã thêm mt-auto để luôn đẩy dòng gợi ý này xuống sát đáy khung -->
                            <div class="mt-auto pt-3 text-muted" style="font-size: 11.5px; line-height: 1.4;">
                                <i class="fas fa-lightbulb text-warning me-1"></i> 
                                Chọn một nhân viên để theo dõi và cập nhật trạng thái của rủi ro này trong suốt vòng đời dự án.
                            </div>
                        </fieldset>
                    </div>
                    
                </div>

                <!-- HÀNG 2: PHÒNG NGỪA & ỨNG PHÓ -->
                <div class="row g-3">
                    <div class="col-md-6 d-flex flex-column">
                        <fieldset class="risk-form-section flex-grow-1 h-100">
                            <legend class="risk-section-legend"><i class="fas fa-shield-alt text-success"></i>Phòng ngừa rủi ro</legend>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtKeHoachPhongNgua" TextMode="MultiLine" CssClass="form-control flex-grow-1 border-0 p-0" PlaceHolder="Nhập kế hoạch phòng ngừa..."></SweetSoft:ExtraTextBox>
                        </fieldset>
                    </div>
                    <div class="col-md-6 d-flex flex-column">
                        <fieldset class="risk-form-section flex-grow-1 h-100">
                            <legend class="risk-section-legend"><i class="fas fa-fire-extinguisher text-warning"></i>Kế hoạch xử lý</legend>
                            <SweetSoft:ExtraTextBox runat="server" ID="txtKeHoachUngPho" TextMode="MultiLine" CssClass="form-control flex-grow-1 border-0 p-0" PlaceHolder="Nhập kế hoạch xử lý..."></SweetSoft:ExtraTextBox>
                        </fieldset>
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

        function formatUserTemplate(state) {
            if (!state.id || state.id === "") { return state.text; }
            
            var $state = $(state.element);
            var name = $state.data('name') || state.text;
            var email = $state.data('email') || 'Chưa cập nhật email';
            var initials = $state.data('initials') || '?';
            var color = $state.data('color') || '#7c3aed';
            var avatarUrl = $state.data('avatar');

            var avatarHtml = '';
            if (avatarUrl) {
                avatarHtml = '<img src="' + avatarUrl + '" onerror="this.style.display=\'none\'; this.nextElementSibling.style.display=\'flex\';" />' +
                             '<div style="background-color:' + color + '; width:100%; height:100%; display:none; align-items:center; justify-content:center;">' + initials + '</div>';
            } else {
                avatarHtml = '<div style="background-color:' + color + '; width:100%; height:100%; display:flex; align-items:center; justify-content:center;">' + initials + '</div>';
            }

            var $html = $(
                '<div class="select2-user-result">' +
                    '<div class="select2-user-avatar-wrap">' + avatarHtml + '</div>' +
                    '<div class="select2-user-info">' +
                        '<div class="select2-user-name">' + name + '</div>' +
                        '<div class="select2-user-email">' + email + '</div>' +
                    '</div>' +
                '</div>'
            );
            return $html;
        }

        function forceShowSearchDropdown() {
            $('.risk-edit-form select').each(function () {
                var $select = $(this);

                if ($select.hasClass("select2-hidden-accessible")) {
                    $select.select2('destroy');
                }
                
                if ($select.attr('id').indexOf('ddlNhanVien') > -1) {
                    $select.select2({
                        width: '100%',
                        minimumResultsForSearch: 0,
                        dropdownParent: $('#<%= dlDetail.ClientID %>'),
                        templateResult: formatUserTemplate,
                        templateSelection: formatUserTemplate
                    });
                } else {
                    $select.select2({
                        width: '100%',
                        minimumResultsForSearch: 0, 
                        dropdownParent: $('#<%= dlDetail.ClientID %>')
                    });
                }
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