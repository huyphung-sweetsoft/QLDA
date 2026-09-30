<%@ Page Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="CostList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCosts.CostList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Register Src="~/fCosts/Controls/CtrlCost.ascx" TagPrefix="SweetSoft" TagName="CtrlCost" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style type="text/css">
        #<%= dlDetail.ClientID %> .modal-dialog{width:calc(100vw - 32px)!important;max-width:1180px!important;margin:1rem auto!important}
        #<%= dlDetail.ClientID %> .modal-body{padding:14px 18px 12px;background:#f8fafc}
        #<%= dlDetail.ClientID %> .modal-footer{padding:10px 18px 12px;background:#fff;border-top:1px solid #e2e8f0}
        .record-attachments .file-actions{display:none!important}
        .cost-edit-layout{align-items:stretch}
        .cost-left-column,.cost-right-column{display:flex;flex-direction:column;min-width:0;height:100%}.cost-left-column>.cost-form-section:last-child{flex:1}.cost-right-column>.cost-form-section{height:100%;display:flex;flex-direction:column}
        .cost-form{width:100%}
        .cost-form-section{min-width:0;padding:7px 14px 11px;border:1px solid #d5deea;border-radius:10px;background:#fff}
        .cost-form-section+.cost-form-section{margin-top:10px}
        .cost-section-legend{display:flex;align-items:center;gap:7px;max-width:100%;margin:0 0 9px;padding:0 8px;background:#fff;color:#334155;font-size:13px;font-weight:800;line-height:1.2;letter-spacing:.02em;text-transform:uppercase}
        .cost-section-legend i{color:#7c3aed;font-size:14px}
        .cost-section-caption{color:#94a3b8;font-size:10.5px;font-weight:400;letter-spacing:0;text-transform:none}
        .cost-field-row{display:flex;align-items:stretch;gap:10px}.cost-field-row+.cost-field-row{margin-top:10px}
        .cost-field-card{padding:8px 10px;border:1px solid #d8e1ec;border-radius:9px;background:#fff}
        .cost-field-card .form-label,.cost-inline-label{display:block;margin-bottom:6px;color:#334155;font-size:11px;font-weight:700}
        .cost-field-card .form-control,.cost-field-card .form-select{border-color:#d5deea}
        .cost-field-card input[readonly],.cost-field-card input:disabled{background:#fff!important;color:#334155!important;-webkit-text-fill-color:#334155!important;opacity:1!important}
        .cost-name-field{width:620px;max-width:100%;flex:0 0 620px}
        .cost-price-field{width:275px;max-width:100%;flex:0 0 275px}
        .cost-quantity-field{width:105px;max-width:100%;flex:0 0 105px}
        .cost-total-field{width:220px;max-width:100%;flex:0 0 220px}
        .cost-requester-field{width:350px;max-width:100%;flex:0 0 350px}
        .cost-status-field{width:260px;max-width:100%;flex:0 0 260px}
        .cost-creator-field{width:350px;max-width:100%;flex:0 0 350px}
        .cost-date-field{width:260px;max-width:100%;flex:0 0 260px}
        .cost-input-money{position:relative}
        .cost-input-money>.form-control{padding-right:32px;text-align:right}
        .cost-input-money>span{position:absolute;top:50%;right:11px;transform:translateY(-50%);color:#94a3b8;font-size:12px;font-weight:700;pointer-events:none}
        .cost-total-display{display:flex;align-items:center;gap:8px;min-height:40px;padding:6px 9px;border:1px solid #c4b5fd;border-radius:9px;background:#f5f3ff;color:#5b21b6}
        .cost-total-display .cost-display-icon{width:27px;height:27px;display:inline-flex;align-items:center;justify-content:center;flex:0 0 27px;border-radius:7px;background:#fff;border:1px solid #ddd6fe;color:#7c3aed;font-size:11px}
        .cost-total-display .cost-display-value{min-width:0;flex:1;font-size:13px;font-weight:800;line-height:1.2;text-align:right;font-variant-numeric:tabular-nums}
        .cost-total-display .cost-display-unit{color:#7c3aed;font-size:11px;font-weight:800}
        .cost-status-select,.cost-status-select .form-select,select.cost-status-select{min-height:40px!important;border:1px solid #cbd5e1!important;border-radius:9px!important;background:#fff!important;color:#334155!important;font-size:12px!important;font-weight:700!important;opacity:1!important}
        .cost-status-select:disabled,.cost-status-select .form-select:disabled,select.cost-status-select:disabled{background:#f8fafc!important;color:#334155!important;-webkit-text-fill-color:#334155!important;opacity:1!important;cursor:default}
        .cost-status-badge{display:flex;align-items:center;gap:8px;min-height:40px;width:100%;padding:6px 11px;border:1px solid #fde68a;border-radius:9px;background:#fffbeb;color:#b45309;font-size:12px;font-weight:800}
        .cost-status-badge i{width:27px;height:27px;display:inline-flex;align-items:center;justify-content:center;flex:0 0 27px;border-radius:7px;background:#fff;border:1px solid #fde68a;font-size:11px}
        .cost-person-card{display:flex;align-items:center;gap:9px;min-height:58px;width:100%;padding:8px 10px;border:1px solid #d8e1ec;border-radius:9px;background:#f8fafc}
        .cost-person-avatar{width:36px;height:36px;display:flex;align-items:center;justify-content:center;flex:0 0 36px;border-radius:50%;object-fit:cover;border:1px solid #c4b5fd;background:#fff;font-size:11px;font-weight:800;color:#fff}
        .cost-person-main{min-width:0}
        .cost-person-name{color:#334155;font-size:11.5px;font-weight:700;line-height:1.25;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
        .cost-person-email{margin-top:2px;color:#94a3b8;font-size:9.5px;line-height:1.2;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
        .cost-readonly-field{display:flex;align-items:center;gap:8px;min-height:36px;padding:4px 8px;border:1px solid #d5deea;border-radius:9px;background:#fff!important}
        .cost-readonly-field>i{width:25px;height:25px;display:inline-flex;align-items:center;justify-content:center;flex:0 0 27px;border:1px solid #e2e8f0;border-radius:7px;background:#f8fafc;color:#64748b;font-size:11px}
        .cost-readonly-field .form-control,.cost-readonly-field .form-control[readonly],.cost-readonly-field .form-control:disabled{min-width:0;width:100%;padding:0!important;margin:0!important;background:transparent!important;border:0!important;box-shadow:none!important;color:#334155!important;-webkit-text-fill-color:#334155!important;font-size:12px;font-weight:600;opacity:1!important}
        .cost-reject-box{padding:10px 12px;border:1px solid #fecaca;border-radius:9px;background:#fffafa}
        .cost-reject-title{margin-bottom:5px;color:#b91c1c;font-size:11px;font-weight:800}
        .cost-reject-title span{color:#dc2626}
        @media(max-width:991.98px){#<%= dlDetail.ClientID %> .modal-dialog{width:calc(100vw - 20px)!important}.cost-name-field,.cost-price-field,.cost-quantity-field,.cost-total-field,.cost-requester-field,.cost-status-field,.cost-creator-field,.cost-date-field{width:100%;max-width:none;flex:1 1 auto}}
       
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" />
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <SweetSoft:CtrlCost runat="server" ID="CtrlCost1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" DefaultButton="lbtSubmit" Title="Thông tin khoản chi">
        <ContentTemplate>
            <div class="cost-form js-validation validationEngineContainer">
                <div class="row g-3 cost-edit-layout">
                    <div class="col-lg-7 cost-left-column">
                        <fieldset class="cost-form-section">
                            <legend class="cost-section-legend"><i class="fas fa-receipt"></i><span>Thông tin khoản chi</span><span class="cost-section-caption">Nhập thông tin và giá trị của khoản chi phí</span></legend>
                            <div class="cost-field-row">
                                <div class="cost-field-card cost-name-field">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.COST_NAME) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtTenKhoanChi" Required="true" CssClass="cost-input"></SweetSoft:ExtraTextBox>
                                </div>
                            </div>
                            <div class="cost-field-row">
                                <div class="cost-field-card cost-price-field">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.PRICE) %></label>
                                    <div class="cost-input-money">
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtDonGia" Required="true" onkeyup="calculateTotalCost(this);" onchange="calculateTotalCost(this);" CssClass="text-end"></SweetSoft:ExtraTextBox>
                                        <span>₫</span>
                                    </div>
                                </div>
                                <div class="cost-field-card cost-quantity-field">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.QUANTITY) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtSoLuong" Required="true" TextMode="Number" min="1" oninput="calculateTotalCost();" CssClass="text-end"></SweetSoft:ExtraTextBox>
                                </div>
                                <div class="cost-field-card cost-total-field">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.TOTAL_AMOUNT) %></label>
                                    <div class="cost-total-display" id="costTotalDisplay">
                                        <span class="cost-display-icon"><i class="fas fa-calculator"></i></span>
                                        <span class="cost-display-value" id="costTotalDisplayValue">0</span>
                                        <span class="cost-display-unit">₫</span>
                                    </div>
                                    <div style="display:none;"><SweetSoft:ExtraTextBox runat="server" ID="txtTongTien" ReadOnly="true" PlaceHolder="0" CssClass="cost-total-input"></SweetSoft:ExtraTextBox></div>
                                </div>
                            </div>
                        </fieldset>
                        <fieldset class="cost-form-section">
                            <legend class="cost-section-legend"><i class="fas fa-user-check"></i><span>Xử lý khoản chi</span><span class="cost-section-caption">Người yêu cầu, trạng thái và thông tin tạo</span></legend>
                            <div class="cost-field-row">
                                <div class="cost-field-card cost-requester-field">
                                    <label class="cost-inline-label"><%= GetResourceText(BackEndResourceKeys.REQUESTER) %></label>
                                    <asp:Panel runat="server" ID="pnlRequesterCard" CssClass="cost-person-card">
                                        <asp:Literal runat="server" ID="litRequesterAvatar"></asp:Literal>
                                        <div class="cost-person-main">
                                            <div class="cost-person-name"><asp:Literal runat="server" ID="litRequesterName"></asp:Literal></div>
                                            <div class="cost-person-email"><asp:Literal runat="server" ID="litRequesterEmail"></asp:Literal></div>
                                        </div>
                                    </asp:Panel>
                                    <div style="display:none;"><SweetSoft:ExtraTextBox runat="server" ID="txtNhanVienYeuCau" ReadOnly="true"></SweetSoft:ExtraTextBox><SweetSoft:ExtraDropdown runat="server" ID="ddlNhanVienYeuCau"></SweetSoft:ExtraDropdown></div>
                                </div>
                                <div class="cost-field-card cost-status-field">
                                    <label class="cost-inline-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                                    <asp:Panel runat="server" ID="pnlStatusNew">
                                        <div class="cost-status-badge"><i class="fas fa-clock"></i><span>Chưa duyệt</span></div>
                                    </asp:Panel>
                                    <asp:Panel runat="server" ID="pnlStatusEdit">
                                        <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThai" CssClass="cost-status-select"></SweetSoft:ExtraDropdown>
                                    </asp:Panel>
                                </div>
                            </div>
                            <div class="cost-field-row">
                                <div class="cost-field-card cost-creator-field">
                                    <label class="cost-inline-label">Người tạo</label>
                                    <asp:Panel runat="server" ID="pnlCreatorCard" CssClass="cost-person-card">
                                        <asp:Literal runat="server" ID="litCreatorAvatar"></asp:Literal>
                                        <div class="cost-person-main">
                                            <div class="cost-person-name"><asp:Literal runat="server" ID="litCreatorName"></asp:Literal></div>
                                            <div class="cost-person-email"><asp:Literal runat="server" ID="litCreatorEmail"></asp:Literal></div>
                                        </div>
                                    </asp:Panel>
                                    <div style="display:none;"><SweetSoft:ExtraTextBox runat="server" ID="txtNguoiTao" ReadOnly="true"></SweetSoft:ExtraTextBox></div>
                                </div>
                                <div class="cost-field-card cost-date-field">
                                    <label class="cost-inline-label"><%= GetResourceText(BackEndResourceKeys.DATE_CREATED) %></label>
                                    <div class="cost-readonly-field">
                                        <i class="far fa-calendar-alt"></i>
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtNgayTao" ReadOnly="true" CssClass="bg-transparent border-0"></SweetSoft:ExtraTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="col-12 p-0 mt-3" id="divLyDoTuChoi" runat="server" style="display:none;">
                                <div class="cost-reject-box">
                                    <div class="cost-reject-title"><i class="fas fa-exclamation-circle me-1"></i>Lý do từ chối <span>*</span></div>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtLyDoTuChoi" TextMode="MultiLine" Rows="3" CssClass="border-danger text-danger" PlaceHolder="Bắt buộc nhập lý do từ chối..."></SweetSoft:ExtraTextBox>
                                </div>
                            </div>
                        </fieldset>
                    </div>
                    <div class="col-lg-5 cost-right-column">
                        <fieldset class="cost-form-section meeting-description-card">
                            <legend class="cost-section-legend"><i class="fas fa-align-left"></i><span>Nội dung chi phí</span><span class="cost-section-caption">Mô tả chi tiết và thông tin bổ sung</span></legend>
                            <div class="cost-description-wrap">
                                <div class="cost-description-editor meeting-description-editor flex-grow-1">
                                    <CKEditor:CKEditorControl runat="server" ID="txtMoTaChiTiet" Width="100%" CssClass="ck-editor" Toolbar="Full" Language="vi-VN" AutoParagraph="false" BasePath="~/Styles/plugins/ckeditor/" Height="320" />
                                    <div class="form-text"><i class="fas fa-paperclip me-1"></i>File đính kèm được tải bằng nút thư mục của khoản chi phí trong danh sách.</div>
                                </div>
                            </div>
                        </fieldset>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false">
                <%= GetResourceText(BackEndResourceKeys.SAVE) %>
            </SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
    <SweetSoft:ExtraModal runat="server" ID="dlCostFiles" Type="Primary" Size="Large" Title="File đính kèm chi phí">
        <ContentTemplate>
            <div class="record-attachments">
                <SweetSoft:FilesBox runat="server" ID="fbCostFiles" IsMultiple="true" MaxFileSizeBytes="10485760" />
            </div>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        function calculateTotalCost(donGiaInput){
            const txtDonGia=document.getElementById('<%= txtDonGia.ClientID %>');
            const txtSoLuong=document.getElementById('<%= txtSoLuong.ClientID %>');
            const txtTongTien=document.getElementById('<%= txtTongTien.ClientID %>');
            const totalDisplay=document.getElementById('costTotalDisplayValue');
            if(!txtDonGia||!txtSoLuong||!txtTongTien||!totalDisplay)return;
            if(donGiaInput){const value=txtDonGia.value.replace(/\D/g,'');txtDonGia.value=value!==''?parseInt(value,10).toLocaleString('en-US'):''}
            const donGia=parseInt(txtDonGia.value.replace(/,/g,''),10)||0;
            const soLuong=parseInt(txtSoLuong.value,10)||0;
            const formattedTotal=(donGia*soLuong).toLocaleString('en-US');
            txtTongTien.value=formattedTotal;
            totalDisplay.textContent=formattedTotal;
        }
        $(document).on('input','.format-currency',function(){let value=$(this).val().replace(/[^0-9]/g,'');$(this).val(value!==''?parseInt(value,10).toLocaleString('en-US'):'')});
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function(){
            const $status=$('#<%= ddlTrangThai.ClientID %>');
            const $rejectReason=$('#<%= divLyDoTuChoi.ClientID %>');
            function toggleRejectReason(){if($status.val()==='2')$rejectReason.stop(true,true).slideDown(180);else $rejectReason.stop(true,true).slideUp(180);}
            $status.off('change.cost').on('change.cost',toggleRejectReason);
            toggleRejectReason();
            const txtDonGia=document.getElementById('<%= txtDonGia.ClientID %>');
            if (txtDonGia && (txtDonGia.value || '').trim()) calculateTotalCost();
        });
    </script>
</asp:Content>
