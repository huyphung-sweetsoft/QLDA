<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlViewMeetDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.Controls.CtrlViewMeetDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style type="text/css">
    #<%= mdlMeetView.ClientID %> .modal-dialog {
        width: calc(100% - 30px) !important;
        max-width: 1150px !important;
        margin: 1.5rem auto;
    }

    #<%= mdlMeetView.ClientID %> .modal-body {
        padding: 20px 22px 16px;
    }

    .meet-view {
        color: #334155;
    }

    .meet-view-layout {
        display: grid;
        grid-template-columns: minmax(360px, 1fr) minmax(0, 1.8fr);
        gap: 18px;
        margin-top: 4px;
        align-items: stretch;
    }

    .meet-view-side {
        min-width: 0;
        display: grid;
        grid-template-rows: auto auto auto auto;
        gap: 18px;
    }

    .meet-view-meeting-card {
        position: relative;
        min-width: 0;
        padding: 18px;
        border: 1px solid #e2e9f2;
        border-radius: 12px;
        background: #fff;
        box-shadow: 0 1px 2px rgba(15, 23, 42, .02);
        box-sizing: border-box;
    }

    .fieldlegend {
        position: absolute;
        top: -15px;
        left: 18px;
        display: inline-flex;
        align-items: center;
        gap: 8px;
        margin: 0;
        padding: 0 10px;
        background: #ffffff;
        color: #475569;
        font-size: 12.5px;
        font-weight: 800;
        line-height: 1.2;
        letter-spacing: .04em;
        text-transform: uppercase;
        z-index: 2;
    }

    .fieldlegend i {
        width: 26px;
        height: 26px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        flex: 0 0 26px;
        border: 1px solid #dce7f4;
        border-radius: 6px;
        background: #f7faff;
        color: #3b82f6;
        font-size: 12px;
    }

    .meet-view-meeting-main {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 12px;
    }

    .meet-view-meeting-info {
        min-width: 0;
        flex: 1 1 auto;
    }

    .meet-view-title {
        color: #4c1d95;
        font-size: 16px;
        font-weight: 700;
        line-height: 1.4;
        word-break: break-word;
    }

    .meet-view-location {
        display: inline-flex;
        align-items: center;
        gap: 7px;
        margin-top: 7px;
        color: #64748b;
        font-size: 12px;
        line-height: 1.3;
    }

    .meet-view-location i {
        color: #94a3b8;
        font-size: 12px;
    }

    .meet-view-status {
        display: inline-flex;
        align-items: center;
        gap: 7px;
        flex: 0 0 auto;
        margin-top: 1px;
        padding: 6px 10px;
        border: 1px solid transparent;
        border-radius: 999px;
        font-size: 11px;
        font-weight: 700;
        line-height: 1.2;
        white-space: nowrap;
    }

    .meet-view-status::before {
        content: "";
        width: 7px;
        height: 7px;
        flex: 0 0 7px;
        border-radius: 50%;
        background: currentColor;
    }

    .meet-view-status-0 { color: #2563eb; background: #eff6ff; border-color: #bfdbfe; }
    .meet-view-status-1 { color: #d97706; background: #fffbeb; border-color: #fde68a; }
    .meet-view-status-2 { color: #16a34a; background: #f0fdf4; border-color: #bbf7d0; }
    .meet-view-status-3 { color: #64748b; background: #f8fafc; border-color: #cbd5e1; }

    .meet-view-card {
        position: relative;
        min-width: 0;
        padding: 26px 18px 18px;
        border: 1px solid #e2e9f2;
        border-radius: 12px;
        background: #fff;
        box-shadow: 0 1px 2px rgba(15, 23, 42, .02);
        box-sizing: border-box;
    }

    .meet-view-side > .meet-view-card {
        height: 100%;
    }

    .meet-view-content-card {
        height: 100%;
        display: flex;
        flex-direction: column;
    }

    .meet-view-content-body {
        flex: 1 1 auto;
        min-height: 0;
        color: #475569;
        font-size: 14px;
        line-height: 1.65;
        overflow-wrap: anywhere;
    }

    .meet-view-empty {
        color: #94a3b8;
        font-size: 13px;
        font-style: italic;
    }

    .meet-view-time-list {
        display: grid;
        gap: 9px;
    }

    .meet-view-time-item {
        display: flex;
        align-items: center;
        gap: 10px;
        min-width: 0;
        padding: 9px 10px;
        border: 1px solid #edf2f7;
        border-radius: 9px;
        background: #f8fafc;
    }

    .meet-view-time-icon {
        width: 34px;
        height: 34px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        flex: 0 0 34px;
        border-radius: 8px;
        background: #fff;
        color: #64748b;
        font-size: 12.5px;
        box-shadow: 0 1px 2px rgba(15, 23, 42, .035);
    }

    .meet-view-time-main {
        min-width: 0;
    }

    .meet-view-time-label {
        color: #94a3b8;
        font-size: 10.5px;
        font-weight: 700;
        line-height: 1.15;
        text-transform: uppercase;
        letter-spacing: .02em;
    }

    .meet-view-time-value {
        margin-top: 3px;
        color: #334155;
        font-size: 13.5px;
        font-weight: 750;
        line-height: 1.2;
        white-space: nowrap;
    }

    .meet-view-creator {
        display: flex;
        align-items: center;
        gap: 11px;
        min-width: 0;
    }

    .meet-view-person-avatar {
        width: 48px;
        height: 48px;
        flex: 0 0 48px;
        display: flex;
        align-items: center;
        justify-content: center;
        border: 1px solid rgba(15, 23, 42, .05);
        border-radius: 50%;
        overflow: hidden;
        color: #fff;
        background: #2563eb;
        font-size: 14px;
        font-weight: 800;
        object-fit: cover;
    }

    .meet-view-creator-main {
        min-width: 0;
        flex: 1 1 auto;
    }

    .meet-view-creator-name {
        color: #1f2937;
        font-size: 14px;
        font-weight: 750;
        line-height: 1.25;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .meet-view-creator-email {
        margin-top: 3px;
        color: #94a3b8;
        font-size: 12px;
        line-height: 1.25;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .meet-view-created {
        display: flex;
        align-items: center;
        gap: 6px;
        margin-top: 12px;
        padding-top: 10px;
        border-top: 1px dashed #eef2f7;
        color: #64748b;
        font-size: 11.5px;
        line-height: 1.25;
    }

    .meet-view-created i {
        color: #94a3b8;
    }

    .meet-view-participant-list {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 8px 10px;
    }

    .meet-view-participant-row {
        display: flex;
        align-items: center;
        gap: 9px;
        min-width: 0;
        padding: 7px 8px;
        border: 1px solid #eef2f7;
        border-radius: 8px;
        background: #fafcff;
    }

    .meet-view-participant-avatar {
        width: 32px;
        height: 32px;
        flex: 0 0 32px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 50%;
        overflow: hidden;
        color: #fff;
        background: #64748b;
        font-size: 11px;
        font-weight: 800;
        object-fit: cover;
    }

    .meet-view-participant-main {
        min-width: 0;
        flex: 1 1 auto;
    }

    .meet-view-participant-name {
        color: #334155;
        font-size: 12.5px;
        font-weight: 700;
        line-height: 1.2;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .meet-view-participant-email {
        margin-top: 2px;
        color: #94a3b8;
        font-size: 10.5px;
        line-height: 1.15;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .meet-view-no-participant {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 8px;
        min-height: 52px;
        padding: 8px 10px;
        border: 1px dashed #d7e0eb;
        border-radius: 8px;
        background: #fafcff;
        color: #94a3b8;
        font-size: 12.5px;
        line-height: 1.35;
        text-align: center;
    }

    .meet-view-no-participant i {
        color: #a4b3c6;
        font-size: 14px;
    }

    @media (max-width: 900px) {
        .meet-view-layout {
            grid-template-columns: minmax(300px, .9fr) minmax(0, 1.1fr);
        }

        .meet-view-participant-list {
            grid-template-columns: 1fr;
        }
    }

    @media (max-width: 767.98px) {
        #<%= mdlMeetView.ClientID %> .modal-dialog {
            width: calc(100% - 16px) !important;
            margin: .5rem auto;
        }

        #<%= mdlMeetView.ClientID %> .modal-body {
            padding: 12px 14px 10px;
        }

        .meet-view-layout {
            grid-template-columns: 1fr;
            margin-top: 4px;
        }

        .meet-view-side {
            grid-template-rows: auto auto auto auto;
            gap: 18px;
        }

        .meet-view-content-card {
            min-height: 200px;
        }

        .meet-view-meeting-main {
            flex-direction: column;
            gap: 9px;
        }

        .meet-view-title {
            font-size: 16px;
        }
    }
</style>
<SweetSoft:ExtraModal runat="server" ID="mdlMeetView" Type="Primary" Title="Thông tin cuộc họp">
    <ContentTemplate>
        <asp:UpdatePanel runat="server" ID="upMeetView" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="meet-view">
                    <div class="meet-view-layout">
                        <div class="meet-view-side">
                            <div class="meet-view-meeting-card">
                                <div class="fieldlegend">
                                    <i class="fas fa-calendar-alt"></i>
                                    <span>Cuộc họp</span>
                                </div>
                                <div class="meet-view-meeting-main">
                                    <div class="meet-view-meeting-info">
                                        <div class="meet-view-title">
                                            <asp:Literal runat="server" ID="lblMeetName"></asp:Literal>
                                        </div>
                                        <div class="meet-view-location">
                                            <i class="fas fa-map-marker-alt"></i>
                                            <asp:Literal runat="server" ID="lblMeetRoom"></asp:Literal>
                                        </div>
                                    </div>
                                    <asp:Literal runat="server" ID="lblMeetStatus"></asp:Literal>
                                </div>
                            </div>

                            <div class="meet-view-card">
                                <div class="fieldlegend">
                                    <i class="far fa-clock"></i>
                                    <span>Thời gian</span>
                                </div>
                                <div class="meet-view-time-list">
                                    <div class="meet-view-time-item">
                                        <div class="meet-view-time-icon"><i class="fas fa-play"></i></div>
                                        <div class="meet-view-time-main">
                                            <div class="meet-view-time-label">Bắt đầu</div>
                                            <div class="meet-view-time-value"><asp:Literal runat="server" ID="lblMeetStart"></asp:Literal></div>
                                        </div>
                                    </div>
                                    <div class="meet-view-time-item">
                                        <div class="meet-view-time-icon"><i class="fas fa-flag-checkered"></i></div>
                                        <div class="meet-view-time-main">
                                            <div class="meet-view-time-label">Kết thúc</div>
                                            <div class="meet-view-time-value"><asp:Literal runat="server" ID="lblMeetEnd"></asp:Literal></div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="meet-view-card">
                                <div class="fieldlegend">
                                    <i class="fas fa-user"></i>
                                    <span>Người tạo</span>
                                </div>
                                <div class="meet-view-creator">
                                    <asp:Literal runat="server" ID="ltrMeetCreatorAvatar"></asp:Literal>
                                    <div class="meet-view-creator-main">
                                        <div class="meet-view-creator-name"><asp:Literal runat="server" ID="lblMeetCreator"></asp:Literal></div>
                                        <div class="meet-view-creator-email"><asp:Literal runat="server" ID="lblMeetCreatorEmail"></asp:Literal></div>
                                    </div>
                                </div>
                                <div class="meet-view-created">
                                    <i class="far fa-calendar-alt"></i>
                                    <span>Tạo lúc <asp:Literal runat="server" ID="lblMeetCreatedDate"></asp:Literal></span>
                                </div>
                            </div>

                            <div class="meet-view-card">
                                <div class="fieldlegend">
                                    <i class="fas fa-users"></i>
                                    <span>Người tham gia</span>
                                </div>
                                <asp:Repeater runat="server" ID="rptMeetParticipants">
                                    <HeaderTemplate><div class="meet-view-participant-list"></HeaderTemplate>
                                    <ItemTemplate>
                                        <div class="meet-view-participant-row">
                                            <asp:Literal runat="server" Text='<%# Eval("AvatarHtml") %>'></asp:Literal>
                                            <div class="meet-view-participant-main">
                                                <div class="meet-view-participant-name"><%# Eval("DisplayName") %></div>
                                                <div class="meet-view-participant-email"><%# Eval("Email") %></div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate></div></FooterTemplate>
                                </asp:Repeater>
                                <asp:Panel runat="server" ID="pnlNoMeetParticipants" CssClass="meet-view-no-participant" Visible="false">
                                    <i class="fas fa-user-friends"></i>
                                    <span>Chưa có nhân viên tham gia cuộc họp.</span>
                                </asp:Panel>
                            </div>
                        </div>

                        <div class="meet-view-card meet-view-content-card">
                            <div class="fieldlegend">
                                <i class="fas fa-align-left"></i>
                                <span>Nội dung cuộc họp</span>
                            </div>
                            <div class="meet-view-content-body">
                                <asp:Literal runat="server" ID="ltrMeetContent"></asp:Literal>
                            </div>
                        </div>
                    </div>

                    <div style="display:none;">
                        <asp:HiddenField runat="server" ID="hdfMeetParticipantIds" />
                        <SweetSoft:ExtraTextBox runat="server" ID="txtMeetParticipantNames" />
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
</SweetSoft:ExtraModal>
