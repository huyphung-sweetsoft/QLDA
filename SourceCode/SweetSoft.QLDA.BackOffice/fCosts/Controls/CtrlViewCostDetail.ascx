<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlViewCostDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCosts.Controls.CtrlViewCostDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<asp:UpdatePanel ID="upCostView" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <SweetSoft:ExtraModal ID="mdlCostView" runat="server" Type="Primary" Title="Chi tiết khoản chi">
            <ContentTemplate>
                <div class="cost-view-wrapper">
                    <div class="row g-3 cost-view-layout">
                        <div class="col-lg-7 d-flex flex-column gap-3">
                            <fieldset class="cost-view-card">
                                <legend class="fieldlegend"><i class="fas fa-receipt"></i> Thông tin khoản chi</legend>
                                <div class="cost-view-name"><span class="cost-view-name-value"><asp:Literal ID="lblCostName" runat="server"></asp:Literal></span></div>
                                <div class="row g-2 mt-2">
                                    <div class="col-md-5">
                                        <div class="cost-view-field">
                                            <span class="cost-view-label">Đơn giá</span>
                                            <div class="cost-view-money"><span class="cost-view-money-value"><asp:Literal ID="lblUnitPrice" runat="server"></asp:Literal></span><span>₫</span></div>
                                        </div>
                                    </div>
                                    <div class="col-md-3">
                                        <div class="cost-view-field">
                                            <span class="cost-view-label">Số lượng</span>
                                            <span class="cost-view-value"><asp:Literal ID="lblQuantity" runat="server"></asp:Literal></span>
                                        </div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="cost-view-field cost-view-total">
                                            <span class="cost-view-label">Tổng chi phí</span>
                                            <div class="cost-view-money"><span class="cost-view-money-value"><asp:Literal ID="lblTotal" runat="server"></asp:Literal></span><span>₫</span></div>
                                        </div>
                                    </div>
                                </div>
                            </fieldset>
                            <fieldset class="cost-view-card flex-grow-1">
                                <legend class="fieldlegend"><i class="fas fa-user-check"></i> Xử lý khoản chi</legend>
                                <div class="row g-2">
                                    <div class="col-md-7">
                                        <div class="cost-view-person-field">
                                            <span class="cost-view-label">Nhân viên yêu cầu</span>
                                            <div class="cost-view-person-card">
                                                <asp:Literal ID="ltrRequesterAvatar" runat="server"></asp:Literal>
                                                <div class="cost-view-person-main">
                                                    <div class="cost-view-person-name"><asp:Literal ID="lblRequesterName" runat="server"></asp:Literal></div>
                                                    <div class="cost-view-person-email"><asp:Literal ID="lblRequesterEmail" runat="server"></asp:Literal></div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-5">
                                        <div class="cost-view-person-field">
                                            <span class="cost-view-label">Trạng thái</span>
                                            <asp:Literal ID="lblStatus" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                </div>
                                <div class="row g-2 mt-2">
                                    <div class="col-md-7">
                                        <div class="cost-view-person-field">
                                            <span class="cost-view-label">Người tạo</span>
                                            <div class="cost-view-person-card">
                                                <asp:Literal ID="ltrCreatorAvatar" runat="server"></asp:Literal>
                                                <div class="cost-view-person-main">
                                                    <div class="cost-view-person-name"><asp:Literal ID="lblCreatorName" runat="server"></asp:Literal></div>
                                                    <div class="cost-view-person-email"><asp:Literal ID="lblCreatorEmail" runat="server"></asp:Literal></div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-5">
                                        <div class="cost-view-field">
                                            <span class="cost-view-label">Ngày tạo</span>
                                            <div class="cost-view-date"><i class="far fa-calendar-alt"></i><asp:Literal ID="lblCreatedDate" runat="server"></asp:Literal></div>
                                        </div>
                                    </div>
                                </div>
                                <asp:Panel ID="pnlReject" runat="server" CssClass="cost-view-reject mt-2" Visible="false">
                                    <span class="cost-view-reject-title"><i class="fas fa-exclamation-circle me-1"></i>Lý do từ chối</span>
                                    <div class="cost-view-reject-content"><asp:Literal ID="lblRejectReason" runat="server"></asp:Literal></div>
                                </asp:Panel>
                            </fieldset>
                        </div>
                        <div class="col-lg-5 d-flex flex-column">
                            <fieldset class="cost-view-card flex-grow-1 d-flex flex-column">
                                <legend class="fieldlegend"><i class="fas fa-align-left"></i> Nội dung chi phí</legend>
                                <div class="cost-view-text-block flex-grow-1"><asp:Literal ID="ltrDescription" runat="server"></asp:Literal></div>
                                <div class="cost-view-hint"><i class="fas fa-paperclip me-1"></i>File đính kèm được tải bằng nút thư mục của khoản chi phí trong danh sách.</div>
                            </fieldset>
                        </div>
                    </div>
                    <div style="display:none;"><SweetSoft:ExtraDropdown ID="ddlStatusSource" runat="server"></SweetSoft:ExtraDropdown></div>
                </div>
            </ContentTemplate>
            <FooterTemplate>
                <asp:LinkButton ID="btnCloseCostView" runat="server" CssClass="btn btn-light" CausesValidation="false" OnClientClick="$(this).closest('.modal').modal('hide'); return false;"><i class="fas fa-times me-1"></i> Đóng</asp:LinkButton>
            </FooterTemplate>
        </SweetSoft:ExtraModal>
    </ContentTemplate>
</asp:UpdatePanel>
<style type="text/css">
    #<%= mdlCostView.ClientID %> .modal-dialog{width:calc(100% - 24px)!important;max-width:1180px!important;margin:1rem auto}
    #<%= mdlCostView.ClientID %> .modal-body{padding:12px 18px 16px;background:#f8fafc}
    .cost-view-wrapper{color:#334155}
    .cost-view-layout{align-items:stretch}
    .cost-view-layout>div{min-width:0}
    .cost-view-card{position:relative;padding:7px 16px 14px;border:1px solid #cbd5e1;border-radius:9px;background:#fff;box-shadow:0 1px 3px rgba(15,23,42,.04);margin:0;min-width:0}
    .fieldlegend{width:auto!important;display:inline-flex;align-items:center;gap:8px;margin-left:8px;margin-bottom:0;padding:0 8px;background:#fff!important;border:0;color:#475569;font-size:13px;font-weight:800;line-height:1.2;letter-spacing:.04em;text-transform:uppercase}
    .fieldlegend i{width:26px;height:26px;display:inline-flex;align-items:center;justify-content:center;border:1px solid #dce7f4;border-radius:6px;background:#f7faff;color:#6d28d9;font-size:12px}
    .cost-view-name{color:#542e88;font-size:20px;font-weight:800;line-height:1.35;word-break:break-word;padding:3px 12px 6px;background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px}
    .cost-view-field,.cost-view-person-field{padding:9px 11px;background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px;height:100%}
    .cost-view-label{display:block;margin-bottom:4px;color:#64748b;font-size:10.5px;font-weight:800;text-transform:uppercase}
    .cost-view-value{display:block;color:#1e293b;font-size:13.5px;font-weight:700;line-height:1.35;word-break:break-word}
    .cost-view-money{display:flex;align-items:baseline;justify-content:flex-end;gap:5px;color:#1e293b;font-size:15px;font-weight:800;font-variant-numeric:tabular-nums}
    .cost-view-money span{color:#7c3aed;font-size:11px;font-weight:800}
    .cost-view-total{background:#f5f3ff;border-color:#c4b5fd}
    .cost-view-total .cost-view-money{color:#5b21b6}
    .cost-view-person-card{display:flex;align-items:center;gap:10px;min-height:58px;padding:7px 9px;background:#fff;border:1px solid #e2e8f0;border-radius:8px}
    .cost-view-person-main{min-width:0}
    .cost-view-person-name{color:#1f2937;font-size:13px;font-weight:750;line-height:1.25;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
    .cost-view-person-email{margin-top:3px;color:#6b7280;font-size:11px;line-height:1.25;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
    .cost-view-person-card .cost-view-avatar{width:40px;height:40px;flex:0 0 40px;display:flex;align-items:center;justify-content:center;border-radius:50%;object-fit:cover;border:1px solid #c4b5fd;background:#fff;color:#fff;font-size:11px;font-weight:800}
    .cost-view-status{display:inline-flex;align-items:center;gap:7px;padding:7px 10px;border:1px solid transparent;border-radius:999px;font-size:12px;font-weight:700;line-height:1.2;white-space:nowrap}
    .cost-view-status:before{content:"";width:7px;height:7px;flex:0 0 7px;border-radius:50%;background:currentColor}
    .cost-view-status.status-pending{color:#b45309;background:#fffbeb;border-color:#fde68a}
    .cost-view-status.status-approved{color:#15803d;background:#f0fdf4;border-color:#bbf7d0}
    .cost-view-status.status-rejected{color:#dc2626;background:#fef2f2;border-color:#fecaca}
    .cost-view-status.status-neutral{color:#64748b;background:#f8fafc;border-color:#cbd5e1}
    .cost-view-date{display:flex;align-items:center;gap:8px;color:#1e293b;font-size:13px;font-weight:700;line-height:1.35;min-height:36px}
    .cost-view-date i{width:27px;height:27px;display:inline-flex;align-items:center;justify-content:center;flex:0 0 27px;border:1px solid #dce7f4;border-radius:7px;background:#fff;color:#64748b;font-size:11px}
    .cost-view-reject{padding:10px 12px;background:#fff7f7;border:1px solid #fecaca;border-left:3px solid #ef4444;border-radius:8px}
    .cost-view-reject-title{display:block;margin-bottom:5px;color:#dc2626;font-size:11px;font-weight:800}
    .cost-view-reject-content{color:#7f1d1d;font-size:12px;line-height:1.5;word-break:break-word}
    .cost-view-text-block{min-height:260px;padding:12px 14px;background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px;color:#334155;font-size:14px;font-weight:500;line-height:1.6;word-break:break-word;overflow:auto}
    .cost-view-text-block p:last-child{margin-bottom:0}
    .cost-view-hint{margin-top:7px;color:#94a3b8;font-size:10.5px;line-height:1.4}
    .cost-view-hint i{color:#7c3aed}
    @media(max-width:991.98px){#<%= mdlCostView.ClientID %> .modal-dialog{width:calc(100% - 16px)!important}.cost-view-layout>div+div{margin-top:12px}}
    @media(max-width:767.98px){#<%= mdlCostView.ClientID %> .modal-dialog{width:calc(100% - 10px)!important;margin:.5rem auto}.cost-view-text-block{min-height:180px}}
</style>
