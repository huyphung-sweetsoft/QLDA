<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlSwapPhase.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlSwapPhase" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    .btn-swap-icon {
        width: 48px; height: 48px; border-radius: 50%; display: flex; align-items: center; justify-content: center;
        background-color: #f1f5f9; color: #64748b; transition: all 0.3s ease; text-decoration: none; border: 1px solid #cbd5e1;
        box-shadow: 0 2px 5px rgba(0,0,0,0.05);
    }
    .btn-swap-icon:hover {
        background-color: #6366f1; color: #ffffff; border-color: #6366f1; transform: scale(1.05); box-shadow: 0 4px 12px rgba(99, 102, 241, 0.3);
    }
</style>

<SweetSoft:ExtraModal DefaultButton="btnConfirmSwap" ID="mdlSwapPhase" Type="Primary" runat="server" Title="Hoán đổi vị trí Giai đoạn">
    <ContentTemplate>
        <asp:UpdatePanel ID="upSwap" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="p-3 text-center">
                    <p class="text-muted mb-4">Chọn hai giai đoạn bên dưới và bấm mũi tên ở giữa để hoán đổi vị trí.</p>
                    
                    <div class="row align-items-end justify-content-center mb-3 px-2">
                        <div class="col-5 text-start">
                            <label class="form-label fw-bold" style="color: #4f46e5;"><i class="fas fa-flag me-1"></i> Giai đoạn A</label>
                            <asp:DropDownList ID="ddlPhase1" runat="server" CssClass="form-select border-primary shadow-sm" style="font-weight: 500;" AutoPostBack="true" OnSelectedIndexChanged="ddlPhase_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>

                        <div class="col-2 d-flex justify-content-center pb-1">
                            <asp:LinkButton ID="btnConfirmSwap" runat="server" CssClass="btn-swap-icon" OnClick="btnConfirmSwap_Click" ToolTip="Bấm để hoán đổi" CausesValidation="false">
                                <i class="fas fa-exchange-alt fs-5"></i>
                            </asp:LinkButton>
                        </div>

                        <div class="col-5 text-start">
                            <label class="form-label fw-bold text-danger"><i class="fas fa-flag me-1"></i> Giai đoạn B</label>
                            <asp:DropDownList ID="ddlPhase2" runat="server" CssClass="form-select border-danger shadow-sm" style="font-weight: 500;" AutoPostBack="true" OnSelectedIndexChanged="ddlPhase_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
</SweetSoft:ExtraModal>