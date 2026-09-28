<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlStartTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlStartTask" %>

<!-- POPUP XÁC NHẬN BẮT ĐẦU CÔNG VIỆC -->
<SweetSoft:ExtraModal runat="server" ID="mdlStartTask" Type="Primary" Title="Xác nhận bắt đầu công việc">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlStartTask" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:HiddenField ID="hdfStartTaskId" runat="server" />
                
                <div class="p-4 text-center">
                    <div class="mb-4">
                        <i class="fas fa-play-circle text-primary" style="font-size: 48px;"></i>
                    </div>
                    <h5 class="text-dark mb-3">Xác nhận bắt đầu thực hiện</h5>
                    <p class="text-muted mb-2">Bạn có chắc chắn muốn chuyển trạng thái công việc này sang <strong>"Đang thực hiện"</strong>?</p>
                    <p class="fw-bold text-primary fs-6 bg-light p-3 border rounded mb-0">
                        <asp:Literal ID="ltrTaskName" runat="server"></asp:Literal>
                    </p>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <asp:UpdatePanel ID="upnlFooterStartTask" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="d-flex justify-content-end gap-2">
                    <button type="button" class="btn btn-outline-secondary px-4" data-bs-dismiss="modal">Hủy</button>
                    <asp:LinkButton ID="btnConfirmStart" runat="server" CssClass="btn btn-primary px-4 fw-bold" OnClick="btnConfirmStart_Click">
                        <i class="fas fa-play me-1"></i> Bắt đầu ngay
                    </asp:LinkButton>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>