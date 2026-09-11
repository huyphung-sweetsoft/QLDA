<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlLichSuDuAn.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlLichSuDuAn" %>

<div
    class="offcanvas offcanvas-end"
    tabindex="-1"
    id="project-history-offcanvas"
    aria-labelledby="project-history-title"
    style="width: 620px; max-width: 100%;">

    <div class="offcanvas-header border-bottom">
        <div>
            <h5
                class="offcanvas-title"
                id="project-history-title">
                Lịch sử hoạt động
            </h5>

            <div class="small text-muted mt-1">
                Các thay đổi được thực hiện trong dự án
            </div>
        </div>

        <button
            type="button"
            class="btn-close"
            data-bs-dismiss="offcanvas"
            aria-label="Close">
        </button>
    </div>

    <div class="offcanvas-body">
        <asp:UpdatePanel
            runat="server"
            ID="upnlProjectHistory"
            UpdateMode="Conditional">

            <ContentTemplate>

                <%-- Bộ lọc --%>
                <div class="card shadow-none border mb-3">
                    <div class="card-body">
                        <div class="row g-3">

                            <div class="col-12">
                                <label class="form-label">
                                    Người thực hiện
                                </label>

                                <asp:DropDownList
                                    runat="server"
                                    ID="ddlHistoryUser"
                                    CssClass="form-select">
                                </asp:DropDownList>
                            </div>

                            <div class="col-md-6">
                                <label class="form-label">
                                    Từ ngày
                                </label>

                                <asp:TextBox
                                    runat="server"
                                    ID="txtHistoryFromDate"
                                    TextMode="Date"
                                    CssClass="form-control">
                                </asp:TextBox>
                            </div>

                            <div class="col-md-6">
                                <label class="form-label">
                                    Đến ngày
                                </label>

                                <asp:TextBox
                                    runat="server"
                                    ID="txtHistoryToDate"
                                    TextMode="Date"
                                    CssClass="form-control">
                                </asp:TextBox>
                            </div>

                            <div class="col-12 d-flex justify-content-end gap-2">
                                <asp:LinkButton
                                    runat="server"
                                    ID="lbtClearHistoryFilter"
                                    CausesValidation="false"
                                    CssClass="btn btn-outline-secondary"
                                    OnClick="lbtClearHistoryFilter_Click">

                                    <i class="fas fa-redo me-1"></i>
                                    Đặt lại
                                </asp:LinkButton>

                                <asp:LinkButton
                                    runat="server"
                                    ID="lbtFilterHistory"
                                    CausesValidation="false"
                                    CssClass="btn btn-primary"
                                    OnClick="lbtFilterHistory_Click">

                                    <i class="fas fa-filter me-1"></i>
                                    Lọc
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>
                </div>

                <%-- Danh sách lịch sử --%>
                <div class="list-group list-group-flush">
                    <asp:Repeater
                        runat="server"
                        ID="rptProjectHistory"
                        OnItemDataBound="rptProjectHistory_ItemDataBound">

                        <ItemTemplate>
                            <div class="list-group-item px-0 py-3">
                                <div class="d-flex gap-3">
                                    <span class="text-primary pt-1">
                                        <i class="far fa-circle"></i>
                                    </span>

                                    <div class="flex-grow-1">
                                        <asp:Label
                                            runat="server"
                                            ID="lblHistoryContent"
                                            CssClass="small">
                                        </asp:Label>

                                        <div class="small text-muted mt-1">
                                            <asp:Label
                                                runat="server"
                                                ID="lblHistoryTime">
                                            </asp:Label>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Panel
                        runat="server"
                        ID="pnlEmptyHistory"
                        Visible="false"
                        CssClass="text-center text-muted py-5">

                        <i class="fas fa-history fa-2x mb-3"></i>

                        <div>
                            Chưa có hoạt động phù hợp.
                        </div>
                    </asp:Panel>
                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</div>