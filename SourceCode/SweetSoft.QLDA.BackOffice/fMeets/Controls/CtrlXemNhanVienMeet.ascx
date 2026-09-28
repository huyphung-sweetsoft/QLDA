<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlXemNhanVienMeet.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.Controls.CtrlXemNhanVienMeet" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    .meet-info-banner {
        background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
        border: 1px solid #e2e8f0;
        border-left: 4px solid #3b82f6;
        border-radius: 8px;
        padding: 14px;
        font-size: 13px;
        color: #334155;
    }
    .meet-info-grid {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 8px;
        margin-top: 8px;
    }
    @media (max-width: 768px) { .meet-info-grid { grid-template-columns: 1fr; } }

    .meet-member-scroll {
        max-height: 50vh;
        overflow-y: auto;
        overflow-x: hidden;
        padding-right: 4px;
    }
    .meet-member-scroll::-webkit-scrollbar { width: 6px; }
    .meet-member-scroll::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }

    .meet-member-row {
        position: relative; background: white; border: 1px solid #e2e8f0; border-radius: 8px;
        margin-bottom: 8px; padding: 10px 12px; display: flex; align-items: center; justify-content: space-between;
        transition: all 0.2s ease;
    }
    .meet-member-row:hover { border-color: #93c5fd; background: #faf5ff; }
    .meet-member-row.is-host { background: #fffbeb; border-color: #fde68a; }

    .member-info-group { display: flex; align-items: center; gap: 12px; min-width: 0; flex: 1; }
    .single-avatar-circle {
        width: 36px; height: 36px; border-radius: 50%; display: flex; align-items: center; justify-content: center; 
        font-size: 12px; font-weight: 700; color: #ffffff; flex-shrink: 0; box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    }
    .member-name-block { display: flex; flex-direction: column; min-width: 0; line-height: 1.3; flex: 1; }
    .member-email { font-size: 11.5px; color: #64748b; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; margin-top: 2px; }
    
    .section-label {
        font-size: 11.5px; font-weight: 700; text-transform: uppercase; color: #64748b;
        letter-spacing: 0.5px; margin: 12px 0 6px 4px; display: flex; align-items: center; gap: 6px;
    }
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlViewMeetMember" Type="Primary" Title="Hồ sơ điều phối cuộc họp" Size="Large">
    <ContentTemplate>
        <div class="row p-1">
            <div class="col-12 mb-3">
                <div class="meet-info-banner">
                    <div class="fw-bold text-dark fs-6 mb-1 d-flex align-items-center justify-content-between">
                        <span><i class="fas fa-handshake text-primary me-2"></i> <asp:Literal runat="server" ID="ltrTenCuocHop"></asp:Literal></span>
                        <span class="badge bg-secondary" style="font-size: 11px;"><asp:Literal runat="server" ID="ltrMaCuocHop"></asp:Literal></span>
                    </div>
                    <div class="meet-info-grid">
                        <div><i class="far fa-clock text-muted me-1"></i> <strong>Thời gian:</strong> <asp:Literal runat="server" ID="ltrThoiGian"></asp:Literal></div>
                        <div><i class="fas fa-map-marker-alt text-muted me-1"></i> <strong>Địa điểm:</strong> <asp:Literal runat="server" ID="ltrDiaDiem"></asp:Literal></div>
                    </div>
                </div>
            </div>

            <div class="col-12">
                <div class="meet-member-scroll">
                    <div class="section-label"><i class="fas fa-users"></i> Danh sách thành viên tham gia (<asp:Literal runat="server" ID="ltrTotalMember">0</asp:Literal>)</div>
                    
                    <asp:Repeater ID="rptAssignedMembers" runat="server">
                        <ItemTemplate>
                            <div class="meet-member-row <%# Convert.ToBoolean(Eval("IsHost")) ? "is-host" : "" %>">
                                <div class="member-info-group">
                                    <%# Eval("AvatarHtml") %>
                                    <div class="member-name-block">
                                        <div class="d-flex align-items-center gap-2">
                                            <span class="fw-bold text-dark" style="font-size: 13.5px;"><%# Eval("DisplayName") %></span>
                                            <%# Convert.ToBoolean(Eval("IsHost")) ? "<span class='badge bg-warning text-dark' style='font-size: 9.5px; padding: 2px 6px;'><i class='fas fa-crown me-1'></i> Chủ trì</span>" : "" %>
                                        </div>
                                        <span class="member-email"><%# Eval("Email") %></span>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <div runat="server" id="divEmpty" visible="false" class="text-center p-5 text-muted border rounded bg-white">
                        <i class="fas fa-user-slash fs-2 mb-2 text-secondary opacity-50"></i><br />
                        <span>Chưa có thành viên nào được phân công tham gia cuộc họp này.</span>
                    </div>
                </div>
            </div> 
        </div>
    </ContentTemplate>
    <FooterTemplate>
        <button type="button" class="btn btn-secondary px-4 waves-effect" data-bs-dismiss="modal">
            <i class="fas fa-times me-1"></i> Đóng
        </button>
    </FooterTemplate>
</SweetSoft:ExtraModal>