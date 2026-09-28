<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlAddPhase.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlAddPhase" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<style>
    .phase-context {
        background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        padding: 15px;
        margin-bottom: 20px;
        border-left: 4px solid #3b82f6;
    }
    .end-date-box {
        background-color: #f0fdf4; border: 1px solid #bbf7d0;
        border-radius: 6px; padding: 8px 12px; height: 100%;
        display: flex; flex-direction: column; justify-content: center;
    }
    .end-date-label { font-size: 11px; color: #166534; font-weight: 700; text-transform: uppercase; margin-bottom: 2px;}
    .end-date-value { font-size: 16px; color: #15803d; font-weight: 800; }
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlAddPhase" Type="Primary" Title="Thêm Giai đoạn mới">
    <ContentTemplate>
        <asp:UpdatePanel ID="upAddPhase" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="p-3">
                    <div class="phase-context">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <span class="text-muted" style="font-size: 11px; font-weight: 700;">MÃ GIAI ĐOẠN</span><br />
                                <span class="text-primary" style="font-size: 18px; font-weight: 800;"><asp:Literal ID="ltrPhaseCode" runat="server"></asp:Literal></span>
                            </div>
                            <div class="text-end">
                                <span class="badge bg-secondary">Trạng thái: Chưa bắt đầu</span>
                            </div>
                        </div>
                    </div>

                    <div class="row g-3 mb-3">
                        <div class="col-12">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.TASK_NAME) %> <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtTenGiaiDoan" runat="server" CssClass="form-control form-control-lg" placeholder="Nhập tên giai đoạn chính..."></asp:TextBox>
                        </div>
                    </div>

                    <!-- THÊM DÒNG PHỤ THUỘC TẠI ĐÂY -->
                    <div class="row g-3 mb-3">
                        <div class="col-12">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.DEPENDENT) %></label>
                            <asp:DropDownList ID="ddlPhuThuoc" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlPhuThuoc_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                    </div>

                    <div class="row g-3 mb-3">
                        <div class="col-md-4">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.START_DATE) %><span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtNgayBatDau" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="txtNgayBatDau_TextChanged"></asp:TextBox>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.DURATION) %></label>
                            <div class="input-group">
                                <asp:TextBox ID="txtThoiHan" runat="server" CssClass="form-control text-center fw-bold bg-light" ReadOnly="true" Text="1"></asp:TextBox>
                                <span class="input-group-text bg-light">Ngày</span>
                            </div>
                        </div>
                        <div class="col-md-5">
                            <div class="end-date-box">
                                <div class="end-date-label"><i class="far fa-calendar-check me-1"></i><%= GetResourceText(BackEndResourceKeys.EXPECTED_COMPLETION_DATE) %></div>
                                <div class="end-date-value">
                                    <asp:Label ID="lblNgayKetThuc" runat="server">--/--/----</asp:Label>
                                    <asp:TextBox ID="txtNgayKetThuc" runat="server" style="display:none;"></asp:TextBox>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row g-3">
                        <div class="col-12">
                            <label class="form-label fw-bold"><%= GetResourceText(BackEndResourceKeys.SUMMARY) %></label>
                            <asp:TextBox ID="txtMoTa" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" placeholder="Mô tả tóm tắt..."></asp:TextBox>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:UpdatePanel ID="upnlFooterAddSubTask" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:LinkButton ID="btnSaveSubTask" runat="server" CssClass="btn btn-primary" OnClick="btnSavePhase_Click">
                    <i class="fas fa-save me-1"></i> <%= GetResourceText(BackEndResourceKeys.SAVE) %>
                </asp:LinkButton>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>