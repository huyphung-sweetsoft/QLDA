<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlViewTaskDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlViewTaskDetail" %>
<%@ Register Src="~/fProjects/Controls/CtrlChiTietCong.ascx" TagPrefix="SweetSoft" TagName="CtrlChiTietCong" %>
<asp:UpdatePanel ID="upTaskView" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <SweetSoft:ExtraModal ID="mdlTaskView" runat="server" Type="Primary" Title="Chi tiết công việc">
            <ContentTemplate>
                <div class="task-view-wrapper">
                    <div class="tvd-two-column">
                        <!-- TRÁI: THÔNG TIN / NHÂN VIÊN / MÔ TẢ / LÝ DO TRỄ -->
                        <div class="tvd-left-column">
                            <fieldset class="task-view-card tvd-identity-card">
                                <legend class="fieldlegend"><i class="fas fa-info-circle"></i> Thông tin công việc</legend>
                                <asp:Label ID="lblTaskName" runat="server" CssClass="task-view-title"></asp:Label>
                                <div class="tvd-phase"><i class="fas fa-layer-group"></i> Thuộc giai đoạn: <asp:Label ID="lblPhaseName" runat="server"></asp:Label></div>
                                <div class="tvd-meta-grid">
                                    <div class="task-meta-item meta-status"><span class="meta-label">Trạng thái</span><asp:Label ID="lblStatus" runat="server" CssClass="meta-value"></asp:Label></div>
                                    <div class="task-meta-item meta-priority"><span class="meta-label">Độ ưu tiên</span><asp:Label ID="lblPriority" runat="server" CssClass="meta-value"></asp:Label></div>
                                </div>
                            </fieldset>
                            <fieldset class="task-view-card tvd-timing-card">
                                <legend class="fieldlegend"><i class="far fa-calendar-check"></i> Thời gian &amp; tiến độ</legend>
                                <div class="tvd-timing-grid">
                                    <button type="button" class="tvd-timing-field tvd-timing-jump" data-tvd-jump="start" title="Đến ngày bắt đầu trên lịch"><span class="tvd-timing-label">Ngày bắt đầu dự kiến</span><asp:Label ID="lblStartDate" runat="server" CssClass="tvd-timing-value"></asp:Label></button>
                                    <div class="tvd-timing-field"><span class="tvd-timing-label">Thời lượng (ngày)</span><asp:Label ID="lblDuration" runat="server" CssClass="tvd-timing-value"></asp:Label></div>
                                    <button type="button" class="tvd-timing-field tvd-timing-jump" data-tvd-jump="due" title="Đến ngày kết thúc dự kiến trên lịch"><span class="tvd-timing-label">Ngày kết thúc dự kiến</span><asp:Label ID="lblEndDate" runat="server" CssClass="tvd-timing-value"></asp:Label></button>
                                    <button type="button" class="tvd-timing-field tvd-timing-jump" data-tvd-jump="actual" title="Đến ngày hoàn thành thực tế trên lịch"><span class="tvd-timing-label">Hoàn thành thực tế</span><asp:Label ID="lblActualEndDate" runat="server" CssClass="tvd-timing-value"></asp:Label></button>
                                </div>
                            </fieldset>
                            <fieldset class="task-view-card task-view-assignee-card">
                                <legend class="fieldlegend"><i class="fas fa-users"></i> Nhân viên thực hiện</legend>
                                <div class="tvd-assignee-summary">
                                    <div class="tvd-avatar-stack" data-tvd-avatar-stack="1" aria-label="Ảnh đại diện nhân viên thực hiện"><span class="tvd-avatar-more" hidden></span></div>
                                    <button type="button" class="tvd-assignee-trigger" data-tvd-people="1" aria-label="Xem danh sách nhân viên thực hiện; bấm để ghim" aria-haspopup="dialog" aria-pressed="false" aria-expanded="false"><i class="fas fa-users" aria-hidden="true"></i> Xem danh sách</button>
                                </div>
                                <!-- Chỉ render nhân viên một lần từ server; JS dùng nguồn này cho avatar và bảng hover. -->
                                <div class="tvd-assignee-source" hidden>
                                    <asp:Repeater ID="rptAssignees" runat="server">
                                        <HeaderTemplate><div class="task-assignee-list"></HeaderTemplate>
                                        <ItemTemplate>
                                            <div class="task-assignee-row">
                                                <div class="tvd-staff-person">
                                                    <div class="task-assignee-avatar"><%# Eval("AvatarHtml") %></div>
                                                    <div class="task-assignee-main">
                                                        <div class="task-assignee-name"><%# Eval("DisplayName") %></div>
                                                        <div class="task-assignee-email"><%# Eval("Email") %></div>
                                                    </div>
                                                </div>
                                                <div class="tvd-staff-labor"><%# Eval("LaborHtml") %></div>
                                                <asp:Literal runat="server" Text='<%# Eval("DetailLinkHtml") %>' />
                                            </div>
                                        </ItemTemplate>
                                        <FooterTemplate></div></FooterTemplate>
                                    </asp:Repeater>
                                </div>
                                <asp:Panel ID="pnlNoAssignees" runat="server" CssClass="task-empty-assignee" Visible="false">Chưa phân công nhân viên.</asp:Panel>
                            </fieldset>
                            <fieldset class="task-view-card tvd-description-card">
                                <legend class="fieldlegend"><i class="fas fa-align-left"></i> Mô tả công việc</legend>
                                <div class="task-view-text-block"><asp:Literal ID="ltrDescription" runat="server"></asp:Literal></div>
                            </fieldset>
                        </div>
                        <!-- PHẢI: LỊCH CHỈ XEM, KHÔNG CÓ ĐIỀU KHIỂN CHỈNH NGÀY -->
                        <div class="tvd-right-column">
                            <fieldset class="task-view-card tvd-calendar-card">
                                <legend class="fieldlegend"><i class="fas fa-calendar-alt"></i> Lịch tiến độ công việc</legend>
                                <div class="tvd-calendar-intro"><span><i class="fas fa-eye"></i> Chỉ xem lịch và các mốc thời gian</span><span class="tvd-readonly-chip"><i class="fas fa-lock"></i> Không thể chỉnh sửa</span></div>
                                <div class="tvd-calendar-root" data-tvd-calendar="1">
                                    <div class="tvd-cal-toolbar">
                                        <button type="button" class="tvd-cal-nav" data-tvd-month="-1" aria-label="Xem tháng trước" title="Tháng trước"><i class="fas fa-chevron-left"></i></button>
                                        <strong class="tvd-cal-month" data-tvd-month-name="1">Lịch công việc</strong>
                                        <button type="button" class="tvd-cal-nav" data-tvd-month="1" aria-label="Xem tháng sau" title="Tháng sau"><i class="fas fa-chevron-right"></i></button>
                                    </div>
                                    <div class="tvd-cal-grid" data-tvd-grid="1"></div>
                                    <div class="tvd-cal-legend">
                                        <span><b class="tvd-legend-range"></b> Thời gian dự kiến</span>
                                        <span><b class="tvd-legend-start"></b> Ngày bắt đầu</span>
                                        <span><b class="tvd-legend-end"></b> Kết thúc dự kiến</span>
                                        <span><b class="tvd-legend-actual"></b> Hoàn thành thực tế</span>
                                        <span><b class="tvd-legend-weekend"></b> Cuối tuần</span>
                                        <span><b class="tvd-legend-holiday"></b> Nghỉ lễ</span>
                                        <span><b class="tvd-legend-locked"></b> Ngoài giới hạn tiến độ</span>
                                    </div>
                                    <div class="tvd-cal-footnote"><i class="fas fa-info-circle"></i> Di chuột lên ngày để xem thông tin chi tiết. Các mốc đều chỉ đọc.</div>
                                </div>
                            </fieldset>
                            <fieldset id="divDelayReason" runat="server" class="task-view-card tvd-delay-card" visible="false">
                                <legend class="fieldlegend"><i class="fas fa-exclamation-triangle"></i> Lý do hoàn thành trễ hạn</legend>
                                <div class="tvd-delay-text-block"><asp:Label ID="lblDelayReason" runat="server" CssClass="tvd-delay-text"></asp:Label></div>
                            </fieldset>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
            <FooterTemplate>
                <asp:LinkButton ID="btnCloseTaskView" runat="server" CssClass="btn btn-light" CausesValidation="false" OnClientClick="$(this).closest('.modal').modal('hide'); return false;"><i class="fas fa-times me-1"></i> Đóng</asp:LinkButton>
            </FooterTemplate>
        </SweetSoft:ExtraModal>
    </ContentTemplate>
</asp:UpdatePanel>
<%-- Một bảng công riêng dành cho popup ViewTask: cùng nghiệp vụ CtrlChiTietCong nhưng không chiếm API của DuAnDetail. --%>
<SweetSoft:CtrlChiTietCong runat="server" ID="tvdLaborPopup" EmbeddedInTaskView="true" />

<style>
    /* Chỉ áp dụng cho CtrlViewTaskDetail; giữ nguyên khung/nút X của SweetSoft:ExtraModal. */
    #<%= mdlTaskView.ClientID %> .modal-dialog { width:min(1080px,calc(100vw - 32px))!important;max-width:calc(100vw - 32px)!important;margin:38px auto 15px!important; }
    #<%= mdlTaskView.ClientID %> .modal-content { display:flex;flex-direction:column;max-height:calc(100vh - 60px);overflow:visible!important; }
    #<%= mdlTaskView.ClientID %> .modal-body { min-height:0;overflow-y:auto;padding:17px 20px 19px;background:#f8fafc; }
    #<%= mdlTaskView.ClientID %> .modal-header { overflow:visible!important; }
    #<%= mdlTaskView.ClientID %> .task-view-wrapper { padding:0; }
    #<%= mdlTaskView.ClientID %> .tvd-two-column { display:grid;grid-template-columns:minmax(0,46fr) minmax(0,54fr);gap:16px;align-items:stretch;min-width:0; }
    #<%= mdlTaskView.ClientID %> .tvd-left-column { display:flex;flex-direction:column;gap:10px;min-width:0; }
    #<%= mdlTaskView.ClientID %> .tvd-right-column { display:flex;flex-direction:column;min-width:0; }
    #<%= mdlTaskView.ClientID %> fieldset.task-view-card { position:relative;min-width:0;margin:0;padding:9px 15px 15px;border:1px solid #dce3ee;border-radius:10px;background:#fff;box-shadow:0 1px 4px rgba(15,23,42,.035); }
    #<%= mdlTaskView.ClientID %> legend.fieldlegend { display:inline-flex;align-items:center;gap:8px;width:auto!important;margin:0 0 9px 5px;padding:0 7px;border:0;background:#fff!important;color:#40516b;font-size:12px;font-weight:850;letter-spacing:.025em;text-transform:uppercase;line-height:1.3; }
    #<%= mdlTaskView.ClientID %> legend.fieldlegend i { display:inline-flex;align-items:center;justify-content:center;width:25px;height:25px;border:1px solid #e2d6f6;border-radius:7px;background:#f6f0ff;color:#6730a5;font-size:12px; }
    #<%= mdlTaskView.ClientID %> .task-view-title { display:block;margin-bottom:8px;color:#18273e;font-size:18px;font-weight:800;line-height:1.45;overflow-wrap:anywhere; }
    #<%= mdlTaskView.ClientID %> .tvd-phase { color:#728098;font-size:12.5px;font-weight:600;line-height:1.45; }
    #<%= mdlTaskView.ClientID %> .tvd-phase i { color:#825ea6;margin-right:5px; }
    #<%= mdlTaskView.ClientID %> .tvd-meta-grid { display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:9px;margin-top:14px; }
    #<%= mdlTaskView.ClientID %> .task-meta-item { min-width:0;padding:10px 11px;border:1px solid transparent;border-radius:8px; }
    #<%= mdlTaskView.ClientID %> .task-meta-item .meta-label { display:block;margin-bottom:3px;color:#667085;font-size:10px;font-weight:800;text-transform:uppercase; }
    #<%= mdlTaskView.ClientID %> .task-meta-item .meta-value { display:block;font-size:12.5px;font-weight:750;line-height:1.4;overflow-wrap:anywhere; }
    #<%= mdlTaskView.ClientID %> .meta-priority { background:#fffbeb;border-color:#fde68a;color:#b45309; }
    #<%= mdlTaskView.ClientID %> .meta-status { background:#ebf8ff;border-color:#bee3f8;color:#3182ce; }
    #<%= mdlTaskView.ClientID %> .task-view-assignee-card { flex:0 0 auto;min-height:112px;overflow:visible; }
    #<%= mdlTaskView.ClientID %> .task-assignee-list { display:flex;flex-direction:column;min-height:0; }
    #<%= mdlTaskView.ClientID %> .task-assignee-row { display:flex;align-items:center;gap:10px;min-width:0;padding:8px 0; }
    #<%= mdlTaskView.ClientID %> .task-assignee-row+.task-assignee-row { border-top:1px solid #f0edf5; }
    #<%= mdlTaskView.ClientID %> .task-assignee-avatar { flex:0 0 36px;width:36px;height:36px; }
    #<%= mdlTaskView.ClientID %> .task-assignee-avatar .task-person-avatar { display:flex;align-items:center;justify-content:center;width:36px;height:36px;border:1px solid rgba(15,23,42,.06);border-radius:50%;object-fit:cover;color:#fff;font-size:11px;font-weight:800; }
    #<%= mdlTaskView.ClientID %> .task-assignee-main { min-width:0; }
    #<%= mdlTaskView.ClientID %> .task-assignee-name { color:#334155;font-size:13px;font-weight:750;line-height:1.3;overflow:hidden;text-overflow:ellipsis;white-space:nowrap; }
    #<%= mdlTaskView.ClientID %> .task-assignee-email { margin-top:2px;color:#7c899d;font-size:11px;line-height:1.3;overflow-wrap:anywhere; }
    #<%= mdlTaskView.ClientID %> .task-empty-assignee { padding:15px 10px;border:1px dashed #d8e2f0;border-radius:8px;background:#f8fafc;color:#8290a6;text-align:center;font-size:12px; }
    #<%= mdlTaskView.ClientID %> .tvd-description-card { flex:0 0 auto; }
    #<%= mdlTaskView.ClientID %> .task-view-text-block { min-height:60px;max-height:130px;overflow:auto;padding:11px 13px;border:1px solid #e4e9f1;border-radius:8px;background:#f9fbfe;color:#43536b;font-size:13px;line-height:1.6;white-space:normal;overflow-wrap:anywhere; }
    #<%= mdlTaskView.ClientID %> .tvd-delay-card { padding:12px 14px;border:1px solid #fecaca;border-left:4px solid #ef4444;border-radius:9px;background:#fff5f4; }
    #<%= mdlTaskView.ClientID %> .tvd-delay-head { display:flex;align-items:center;gap:7px;margin-bottom:6px;color:#ad3030;font-size:12px; }
    #<%= mdlTaskView.ClientID %> .tvd-delay-text { display:block;color:#923737;font-size:12.5px;font-weight:600;line-height:1.55;overflow-wrap:anywhere;white-space:pre-wrap; }
    #<%= mdlTaskView.ClientID %> .tvd-calendar-card { display:flex;flex-direction:column;flex:1;min-height:0; }
    #<%= mdlTaskView.ClientID %> .tvd-calendar-intro { display:flex;align-items:center;justify-content:space-between;gap:7px;flex-wrap:wrap;margin-bottom:13px;color:#7d6a8f;font-size:11.5px; }
    #<%= mdlTaskView.ClientID %> .tvd-calendar-intro i { margin-right:4px; }
    #<%= mdlTaskView.ClientID %> .tvd-readonly-chip { padding:5px 8px;border:1px solid #e6d9f5;border-radius:7px;background:#f8f4fe;color:#7444a4;font-size:10.5px;font-weight:750;white-space:nowrap; }
    #<%= mdlTaskView.ClientID %> .tvd-calendar-root { min-width:0; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-toolbar { display:flex;align-items:center;justify-content:space-between;gap:10px;margin-bottom:12px; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-nav { appearance:none;display:grid;place-items:center;flex:0 0 34px;width:34px;height:34px;padding:0;border:1px solid #ddcfed;border-radius:9px;background:#fff;color:#65339a;cursor:pointer;transition:background .16s,border-color .16s; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-nav:hover { background:#f3eafa;border-color:#bd9ddf; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-month { color:#54218d;font-size:15px;font-weight:850; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-grid { display:grid;grid-template-columns:repeat(7,minmax(0,1fr));gap:5px;min-width:0; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-dow { padding:5px 0;text-align:center;color:#76588e;font-size:11px;font-weight:800; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day { position:relative;display:flex;flex-direction:column;justify-content:space-between;gap:3px;min-height:55px;min-width:0;padding:6px 5px;border:1px solid #e6e4ed;border-radius:8px;background:#fff;color:#3c3450;box-sizing:border-box;cursor:help;outline:none; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-other { opacity:.55; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-weekend { background:#f1f6fd;border-color:#d9e5f5;color:#527293; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-holiday { background:#fff7e7;border-color:#f6d9a9;color:#9c5112; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-range { background:#eee7fa;border-color:#ceb5e9;color:#54218d; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-late-range { background:#eee7fa;border-color:#ceb5e9;color:#54218d; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-range.is-holiday,#<%= mdlTaskView.ClientID %> .tvd-cal-day.is-late-range.is-holiday { background:#fff5e6; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-range.is-weekend,#<%= mdlTaskView.ClientID %> .tvd-cal-day.is-late-range.is-weekend { background:#f1f6fd; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-start { box-shadow:inset 0 0 0 2px #7c3aed; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-expected { box-shadow:inset 0 0 0 2px #2563eb; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-actual:not(.is-holiday):not(.is-weekend) { box-shadow:inset 0 0 0 2px #9466c7; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-late-work .tvd-cal-number { color:#dc2626; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-expected.is-actual { box-shadow:inset 0 0 0 2px #2563eb,0 0 0 1px #16a34a; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-expected.is-actual { box-shadow:inset 0 0 0 2px #2563eb; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day:focus-visible,#<%= mdlTaskView.ClientID %> .tvd-cal-day:hover { border-color:#996cc9;outline:2px solid rgba(144,93,201,.35);outline-offset:0; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-number { font-size:12px;font-weight:850;line-height:1.2; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-caption { display:block;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;color:inherit;font-size:9.5px;font-weight:750;line-height:1.2; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-caption.is-holiday { color:#a65e16; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-legend { display:flex;flex-wrap:wrap;gap:7px 12px;margin-top:12px;color:#756680;font-size:10.5px; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-legend span { display:inline-flex;align-items:center;gap:5px; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-legend b { display:inline-block;flex:0 0 10px;width:10px;height:10px;border:1px solid transparent;border-radius:3px; }
    #<%= mdlTaskView.ClientID %> .tvd-legend-range { background:#a17bd2; }
    #<%= mdlTaskView.ClientID %> .tvd-legend-start { background:#7c3aed; }
    #<%= mdlTaskView.ClientID %> .tvd-legend-end { background:#2563eb; }
    #<%= mdlTaskView.ClientID %> .tvd-legend-actual { background:#9865c8; }
    #<%= mdlTaskView.ClientID %> .tvd-legend-weekend { background:#aac5e7; }
    #<%= mdlTaskView.ClientID %> .tvd-legend-holiday { background:#f0bd68; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-footnote { display:flex;align-items:center;gap:7px;margin-top:9px;color:#8b7a9b;font-size:11px;line-height:1.4; }
    #<%= mdlTaskView.ClientID %> .tvd-read-values[hidden] { display:none!important; }
    .tvd-floating-tip { position:fixed;z-index:2147483000;box-sizing:border-box;width:max-content;max-width:min(320px,calc(100vw - 20px));padding:10px 12px;border:1px solid #dcc9ed;border-radius:10px;background:#fff;color:#3b324a;box-shadow:0 12px 32px rgba(48,26,71,.19);pointer-events:none;font-size:12px;line-height:1.5; }
    .tvd-floating-tip strong { display:block;margin-bottom:4px;color:#54218d;font-size:12.5px; }
    .tvd-floating-tip .tvd-tip-item { display:block;margin-top:3px; }
    /* Chi tiet nhan su: avatar gon tren mot hang va danh sach noi tren body, khong cat boi modal. */
    #<%= mdlTaskView.ClientID %> .tvd-assignee-summary { display:flex;align-items:center;gap:12px;min-width:0;min-height:51px; }
    #<%= mdlTaskView.ClientID %> .tvd-avatar-stack { display:flex;align-items:center;flex:1 1 auto;min-width:0;overflow:hidden;padding:2px 0 3px 2px; }
    #<%= mdlTaskView.ClientID %> .tvd-avatar-item { flex:0 0 38px;display:flex;align-items:center;justify-content:center;width:38px;height:38px;margin-left:-8px;border:2px solid #fff;border-radius:50%;background:#fff;box-shadow:0 1px 3px rgba(15,23,42,.13);overflow:hidden;box-sizing:border-box; }
    #<%= mdlTaskView.ClientID %> .tvd-avatar-item:first-child { margin-left:0; }
    #<%= mdlTaskView.ClientID %> .tvd-avatar-item[hidden] { display:none!important; }
    #<%= mdlTaskView.ClientID %> .tvd-avatar-item .task-person-avatar { display:grid;place-items:center;width:100%;height:100%;border:0;border-radius:50%;object-fit:cover;color:#fff;font-size:11px;font-weight:800; }
    #<%= mdlTaskView.ClientID %> .tvd-avatar-more { display:grid;place-items:center;flex:0 0 38px;width:38px;height:38px;margin-left:-7px;border:2px solid #cad5e3;border-radius:50%;background:#f1f5f9;color:#475569;font-size:11px;font-weight:800;box-sizing:border-box; }
    #<%= mdlTaskView.ClientID %> .tvd-avatar-more[hidden] { display:none!important; }
    #<%= mdlTaskView.ClientID %> .tvd-assignee-trigger { display:inline-flex;align-items:center;justify-content:center;flex:0 0 auto;gap:6px;min-height:35px;padding:7px 10px;border:1px solid #d9c2f1;border-radius:8px;background:#faf7ff;color:#623096;cursor:pointer;font-size:11.5px;font-weight:750;white-space:nowrap; }
    #<%= mdlTaskView.ClientID %> .tvd-assignee-trigger:hover,#<%= mdlTaskView.ClientID %> .tvd-assignee-trigger:focus-visible { background:#f0e6fc;border-color:#b693db;outline:none; }
    #<%= mdlTaskView.ClientID %> .tvd-assignee-source[hidden] { display:none!important; }
    .tvd-staff-popover { position:fixed;z-index:2147483000;width:min(365px,calc(100vw - 24px));max-height:min(380px,65vh);overflow-y:auto;border:1px solid #d7c6ee;border-radius:12px;background:#fff;box-shadow:0 16px 38px rgba(52,27,87,.22);box-sizing:border-box; }
    .tvd-staff-popover .tvd-staff-popover-heading { position:sticky;top:0;display:flex;align-items:center;justify-content:space-between;gap:9px;padding:12px 15px;border-bottom:1px solid #e8e3ef;background:#f7f2fe;color:#532285;font-size:13px;font-weight:800; }
    .tvd-staff-popover .tvd-staff-popover-count { padding:3px 7px;border:1px solid #decdf1;border-radius:7px;background:#fff;font-size:11px; }
    .tvd-staff-popover .task-assignee-list { display:block;padding:0 12px; }
    .tvd-staff-popover .task-assignee-row { display:flex;align-items:center;gap:11px;min-width:0;padding:12px 2px;border-bottom:1px solid #edf0f5; }
    .tvd-staff-popover .task-assignee-row:last-child { border-bottom:0; }
    .tvd-staff-popover .task-assignee-avatar { flex:0 0 40px;width:40px;height:40px; }
    .tvd-staff-popover .task-person-avatar { display:grid;place-items:center;width:40px;height:40px;border-radius:50%;object-fit:cover;color:#fff;font-size:11px;font-weight:800; }
    .tvd-staff-popover .task-assignee-main { min-width:0; }
    .tvd-staff-popover .task-assignee-name { color:#2d374b;font-size:12.5px;font-weight:750;line-height:1.35;overflow-wrap:anywhere; }
    .tvd-staff-popover .task-assignee-email { margin-top:3px;color:#7b8ba3;font-size:11px;line-height:1.4;overflow-wrap:anywhere; }
    /* Ngoai khoang: giong o lich khoa tren TaskList, khong cho chon gia tri. */
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-locked { background:repeating-linear-gradient(-45deg,#f2f2f7 0 8px,#e7e8ef 8px 16px)!important;border-color:#dadde7!important;color:#8790a0!important;box-shadow:none!important; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-locked .tvd-cal-number,#<%= mdlTaskView.ClientID %> .tvd-cal-day.is-locked .tvd-cal-caption { color:#8790a0!important; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-locked.is-other { opacity:.55; }
    /* Ngay nghi uu tien mau goc (ngay ca khi thuc te cham), khong do hoa ngay nghi. */
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-weekend:not(.is-locked) { background:#f1f6fd!important;border-color:#d9e5f5!important;color:#527293!important; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-holiday:not(.is-locked) { background:#fff7e7!important;border-color:#f6d9a9!important;color:#9c5112!important; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-holiday .tvd-cal-off-badge { display:inline-flex;align-items:center;gap:3px;width:max-content;max-width:100%;padding:2px 4px;border:1px solid #f5d6a3;border-radius:5px;background:#ffedcf;color:#a6500b;font-size:9px;font-weight:800;white-space:nowrap;overflow:hidden;text-overflow:ellipsis; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day .tvd-cal-code { display:block;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:9.5px;font-weight:750;line-height:1.2; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day .tvd-cal-caption { font-size:9px; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day .tvd-cal-caption.is-milestone { font-weight:850; }
    /* Mốc ngày ưu tiên hơn màu nghỉ lễ/cuối tuần; khi trùng mốc: Bắt đầu > Dự kiến > Thực tế. */
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-actual:not(.is-start):not(.is-expected):not(.is-locked) { box-shadow:inset 0 0 0 2px #9865c8!important;border-color:#9865c8!important; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-expected:not(.is-start):not(.is-locked) { box-shadow:inset 0 0 0 2px #2563eb!important;border-color:#2563eb!important; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.is-start:not(.is-locked) { box-shadow:inset 0 0 0 2px #7c3aed!important;border-color:#8b5cf6!important; }
    /* Nhan vao 3 moc ngay o cot trai: chi chuyen lich, tuyet doi khong thay doi du lieu. */
    #<%= mdlTaskView.ClientID %> button.tvd-timing-jump { display:block;width:100%;box-sizing:border-box;appearance:none;font:inherit;text-align:left;color:inherit;cursor:pointer;transition:background-color .16s,border-color .16s,box-shadow .16s; }
    #<%= mdlTaskView.ClientID %> button.tvd-timing-jump:hover:not(:disabled) { background:#f5efff;border-color:#bda3e7;box-shadow:0 0 0 1px rgba(124,58,237,.07); }
    #<%= mdlTaskView.ClientID %> button.tvd-timing-jump:focus-visible { outline:2px solid #7c3aed;outline-offset:2px; }
    #<%= mdlTaskView.ClientID %> button.tvd-timing-jump:disabled { cursor:default;opacity:1; }
    #<%= mdlTaskView.ClientID %> .tvd-cal-day.tvd-jump-flash { z-index:3;transform-origin:center center;animation:tvd-jump-pulse .85s ease-out both; }
    @keyframes tvd-jump-pulse { 0% { transform:scale(1);filter:none; } 36% { transform:scale(1.075);filter:drop-shadow(0 2px 9px rgba(103,48,165,.32)); } 68% { transform:scale(.99);filter:drop-shadow(0 1px 4px rgba(103,48,165,.14)); } 100% { transform:scale(1);filter:none; } }
    @media(prefers-reduced-motion:reduce) { #<%= mdlTaskView.ClientID %> .tvd-cal-day.tvd-jump-flash { animation:none; } }
    #<%= mdlTaskView.ClientID %> .tvd-legend-locked { background:repeating-linear-gradient(-45deg,#ececf2 0 3px,#cbd0d9 3px 6px);border-color:#cbd0d9!important; }
    /* Thời gian / tiến độ chỉ xem; dùng lại bốn Label cũ (không tạo ID hoặc code-behind mới). */
    #<%= mdlTaskView.ClientID %> .tvd-timing-card { flex:0 0 auto; }
    #<%= mdlTaskView.ClientID %> .tvd-timing-grid { display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:7px; }
    #<%= mdlTaskView.ClientID %> .tvd-timing-field { min-width:0;padding:8px 10px;border:1px solid #e2e8f0;border-radius:8px;background:#f8fafc; }
    #<%= mdlTaskView.ClientID %> .tvd-timing-label { display:block;margin-bottom:3px;color:#728098;font-size:9.5px;font-weight:800;line-height:1.3;text-transform:uppercase; }
    #<%= mdlTaskView.ClientID %> .tvd-timing-value { display:block;min-width:0;color:#25344e;font-size:12px;font-weight:750;line-height:1.35;overflow-wrap:anywhere;font-variant-numeric:tabular-nums; }
    #<%= mdlTaskView.ClientID %> .tvd-timing-card legend.fieldlegend { margin-bottom:7px; }
    /* Giữ đủ không gian cho Xem danh sách; click sẽ chuyển hover thành bảng ghim. */
    #<%= mdlTaskView.ClientID %> .tvd-assignee-trigger[aria-pressed="true"] { background:#ede1fa;border-color:#aa82d6;color:#512185; }
    .tvd-staff-popover.is-pinned { border-color:#aa82d6;box-shadow:0 16px 38px rgba(52,27,87,.24),0 0 0 1px rgba(131,72,184,.08); }
    .tvd-staff-popover .tvd-staff-pin { display:inline-flex;align-items:center;gap:5px;min-height:27px;padding:4px 7px;border:1px solid #d9c6ed;border-radius:7px;background:#fff;color:#60309a;font-size:10.5px;font-weight:750;cursor:pointer;white-space:nowrap; }
    .tvd-staff-popover .tvd-staff-pin:hover,.tvd-staff-popover .tvd-staff-pin:focus-visible { background:#efe6fc;outline:none; }
    .tvd-staff-popover .tvd-staff-pin[aria-pressed="true"] { background:#6731a3;border-color:#6731a3;color:#fff; }
    .tvd-staff-popover .tvd-staff-popover-head-actions { display:inline-flex;align-items:center;gap:7px;flex:0 0 auto; }
    #<%= mdlTaskView.ClientID %> .tvd-calendar-card { justify-content:flex-start; }
    /* Viewport thấp: thu khoảng trống, chỉ dùng cuộn dọc khi nội dung thực sự vượt màn hình. */
    @media (min-width:951px) and (max-height:830px) { #<%= mdlTaskView.ClientID %> .modal-dialog { margin-top:24px!important;margin-bottom:12px!important; } #<%= mdlTaskView.ClientID %> .modal-content { max-height:calc(100dvh - 38px)!important; } #<%= mdlTaskView.ClientID %> .modal-body { padding:10px 15px 11px; } #<%= mdlTaskView.ClientID %> .tvd-cal-day { min-height:47px;padding:4px 5px; } #<%= mdlTaskView.ClientID %> .tvd-cal-toolbar { margin-bottom:7px; } #<%= mdlTaskView.ClientID %> .tvd-calendar-intro { margin-bottom:7px; } #<%= mdlTaskView.ClientID %> .tvd-left-column { gap:7px; } #<%= mdlTaskView.ClientID %> fieldset.task-view-card { padding:6px 11px 11px; } #<%= mdlTaskView.ClientID %> .tvd-meta-grid { margin-top:9px; } #<%= mdlTaskView.ClientID %> .tvd-timing-field { padding:6px 9px; } #<%= mdlTaskView.ClientID %> .tvd-description-card .task-view-text-block { min-height:45px;max-height:90px; } }
    @media(max-width:950px) { #<%= mdlTaskView.ClientID %> .tvd-two-column { grid-template-columns:minmax(0,1fr); } #<%= mdlTaskView.ClientID %> .tvd-calendar-card { min-height:400px; } }
    @media(max-width:560px) { #<%= mdlTaskView.ClientID %> .modal-dialog { width:calc(100vw - 16px)!important;max-width:calc(100vw - 16px)!important; } #<%= mdlTaskView.ClientID %> .modal-body { padding:13px 10px; } #<%= mdlTaskView.ClientID %> .tvd-cal-day { min-height:51px;padding:4px 3px; } #<%= mdlTaskView.ClientID %> .tvd-cal-caption { font-size:8.5px; } }
    /* Bảng nhân viên gọn: tên bên trái, giờ công và nút Xem chi tiết ở bên phải. */
    .tvd-staff-popover { width:min(425px,calc(100vw - 20px));max-height:min(495px,calc(100vh - 22px)); }
    .tvd-staff-popover .task-assignee-row { display:grid;grid-template-columns:minmax(0,1fr) auto;grid-template-rows:auto auto;align-items:center;gap:4px 12px;min-height:72px;padding:12px 2px; }
    .tvd-staff-popover .tvd-staff-person { grid-column:1;grid-row:1 / 3;display:flex;align-items:center;gap:9px;min-width:0; }
    .tvd-staff-popover .task-assignee-main { flex:1 1 auto;min-width:0; }
    .tvd-staff-popover .task-assignee-name { overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:12px; }
    .tvd-staff-popover .task-assignee-email { overflow-wrap:anywhere;font-size:10.5px;line-height:1.35; }
    .tvd-staff-popover .tvd-staff-labor { grid-column:2;grid-row:1;justify-self:end;align-self:end;text-align:right;white-space:nowrap; }
    .tvd-staff-popover .tvd-staff-hour-text { display:inline-flex;align-items:center;justify-content:flex-end;gap:5px;color:#5d2597;font-size:11.5px;font-weight:800; }
    .tvd-staff-popover .tvd-staff-hour-text.is-empty { color:#8390a2;font-size:10.5px;font-weight:650; }
    .tvd-staff-popover .tvd-staff-detail-link { grid-column:2;grid-row:2;justify-self:end;align-self:start;display:inline-flex;align-items:center;gap:5px;margin:0;padding:5px 8px;border:1px solid #ccb6e8;border-radius:7px;background:#faf7ff;color:#5b2499!important;font-size:10.5px;font-weight:750;line-height:1.25;text-decoration:none!important;white-space:nowrap; }
    .tvd-staff-popover .tvd-staff-detail-link:hover,.tvd-staff-popover .tvd-staff-detail-link:focus-visible { border-color:#9b73ce;background:#f0e6fc;outline:none; }
    @media(max-width:470px) { .tvd-staff-popover .task-assignee-row { gap:5px;grid-template-columns:minmax(0,1fr) auto; } .tvd-staff-popover .task-assignee-name { max-width:160px; } }
    /* Lý do trễ nằm dưới lịch, cùng cấu trúc fieldset/ô Mô tả; chỉ đổi bảng màu sang đỏ. */
    #<%= mdlTaskView.ClientID %> .tvd-right-column { gap:10px; }
    #<%= mdlTaskView.ClientID %> fieldset.task-view-card.tvd-delay-card { flex:0 0 auto;min-height:0;margin:0;padding:9px 14px 13px;border:1px solid #fca5a5;border-radius:10px;background:#fff;box-shadow:0 1px 4px rgba(127,29,29,.06); }
    #<%= mdlTaskView.ClientID %> .tvd-delay-card legend.fieldlegend { color:#b4232a;background:#fff!important; }
    #<%= mdlTaskView.ClientID %> .tvd-delay-card legend.fieldlegend i { border-color:#fecaca;background:#fff1f2;color:#dc2626; }
    #<%= mdlTaskView.ClientID %> .tvd-delay-text-block { min-height:50px;max-height:130px;overflow:auto;padding:11px 13px;border:1px solid #fecaca;border-radius:8px;background:#fff4f4; }
    #<%= mdlTaskView.ClientID %> .tvd-delay-text { display:block;color:#a32b32;font-size:13px;font-weight:600;line-height:1.6;overflow-wrap:anywhere;white-space:pre-wrap; }
    /* Bảng công dùng CHUNG backdrop của ViewTask. Không mở modal Bootstrap thứ hai.
       Gắn thẳng DOM ExtraModal công vào gốc modal ViewTask, tránh stacking context ở master/UpdatePanel. */
    #<%= mdlTaskView.ClientID %>.tvd-labor-underlay::after { content:"";position:fixed;inset:0;z-index:5;background:rgba(15,23,42,.32);pointer-events:none; }
    #<%= mdlTaskView.ClientID %>.tvd-labor-underlay > .modal-dialog { position:relative;z-index:1; }
    #<%= mdlTaskView.ClientID %> > #<%= TaskLaborControl.TaskModalClientId %>.tvd-labor-top-layer { display:block!important;position:fixed!important;inset:0;z-index:6!important;width:100%!important;height:100%!important;padding:0!important;overflow-x:hidden;overflow-y:auto;background:transparent;pointer-events:auto; }
    #<%= mdlTaskView.ClientID %> > #<%= TaskLaborControl.TaskModalClientId %>.tvd-labor-top-layer .modal-dialog { position:relative;z-index:1;pointer-events:auto; }
    #<%= mdlTaskView.ClientID %> > #<%= TaskLaborControl.TaskModalClientId %>.tvd-labor-top-layer .modal-content { pointer-events:auto; }
    #<%= mdlTaskView.ClientID %>.tvd-labor-underlay .tvd-staff-popover { display:none!important; }
    #<%= TaskLaborControl.TaskModalClientId %>.tvd-labor-top-layer .modal-dialog { width:min(1280px,calc(100vw - 32px))!important;max-width:calc(100vw - 32px)!important;margin:32px auto!important; }
    #<%= TaskLaborControl.TaskModalClientId %>.tvd-labor-top-layer .modal-content { display:flex;flex-direction:column;max-height:calc(100dvh - 64px);overflow:visible!important; }
    #<%= TaskLaborControl.TaskModalClientId %>.tvd-labor-top-layer .modal-body { overflow-y:auto!important;min-height:0;scrollbar-width:thin; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table-scroller { max-width:100%;overflow-x:auto;margin:12px 0 15px;border:1px solid #d7dfe9;border-radius:9px; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table { width:100%;min-width:990px;border-collapse:separate;border-spacing:0;color:#334155;font-size:12px; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table th { position:sticky;top:0;z-index:2;background:#f7f9fc;color:#54218d;white-space:nowrap;text-align:left;font-weight:800; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table th,#<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table td { padding:11px 12px;border-right:1px solid #e7ecf2;border-bottom:1px solid #e7ecf2;vertical-align:middle; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table th:last-child,#<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table td:last-child { border-right:0; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table td:not(.pd-ds-name) { white-space:nowrap; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table .pd-ds-name { min-width:220px;font-weight:700; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table td strong { color:#54218d; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-status { padding:4px 7px;border:1px solid #d7dfe9;border-radius:7px;background:#f2f4f8;color:#64748b;font-size:11px; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-subtitle { color:#54218d;font-weight:800; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-view-object-link { appearance:none;border:0;background:none;padding:0;color:#334155;font:inherit;font-weight:750;cursor:pointer;text-align:left;text-decoration:none!important; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-view-object-link:hover { color:#2563eb;text-decoration:none!important; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-team-record-code { background:none;border:0;padding:0;margin:0 4px 0 0;color:inherit;font:inherit; }
    #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table tr.tvd-labor-focus td { animation:tvdLaborPulse 1.75s ease-in-out 1 both; }
    @keyframes tvdLaborPulse { 0%,100% { background:transparent;box-shadow:inset 0 0 0 0 rgba(245,158,11,0); } 16%,52%,84% { background:#fff5bc;box-shadow:inset 0 0 0 2px #f59e0b; } 34%,70% { background:#fffae5;box-shadow:inset 0 0 0 1px #fbbf24; } }
    @media(prefers-reduced-motion:reduce) { #<%= TaskLaborControl.TaskModalClientId %> .pd-ds-table tr.tvd-labor-focus td { animation:none;background:#fff8d6;box-shadow:inset 0 0 0 2px #f59e0b; } }
</style>

<script type="text/javascript">
    (function () {
        'use strict';
        if (window.QLDATaskDetailCalendar) return;
        var modalId = '<%= mdlTaskView.ClientID %>';
        var startId = '<%= lblStartDate.ClientID %>';
        var dueId = '<%= lblEndDate.ClientID %>';
        var taskNameId = '<%= lblTaskName.ClientID %>';
        var actualId = '<%= lblActualEndDate.ClientID %>';
        var apiUrl = '<%= ResolveUrl("~/fTasks/TaskList.aspx/LayLichDuKienCongViec") %>';
        var state = { root: null, start: '', due: '', actual: '', y: 0, m: 0, days: {}, code: '', request: 0, cache: {}, flashKey: '', flashUntil: 0 };
        var tip = null, peopleTip = null, peopleTrigger = null, peopleTimer = null, peoplePinned = false;
        function el(id) { return document.getElementById(id); }
        function z(n) { return (n < 10 ? '0' : '') + n; }
        function key(d) { return d.getFullYear() + '-' + z(d.getMonth() + 1) + '-' + z(d.getDate()); }
        function fmt(k) { if (!k) return 'Chưa có'; var a = k.split('-'); return a.length === 3 ? a[2] + '/' + a[1] + '/' + a[0] : k; }
        function val(id) { var n = el(id); return n ? (n.textContent || '').trim() : ''; }
        function toKey(s) { var m = /^(\d{2})\/(\d{2})\/(\d{4})/.exec(s || ''); return m ? m[3] + '-' + m[2] + '-' + m[1] : ''; }
        function parse(k) { if (!k) return null; var a = k.split('-'), d = new Date(+a[0], +a[1] - 1, +a[2]); return d.getFullYear() === +a[0] && d.getMonth() === +a[1] - 1 && d.getDate() === +a[2] ? d : null; }
        function daysBetween(a, b) { var x = parse(a), y = parse(b); if (!x || !y) return 0; return Math.round((Date.UTC(y.getFullYear(), y.getMonth(), y.getDate()) - Date.UTC(x.getFullYear(), x.getMonth(), x.getDate())) / 86400000); }
        function esc(s) { var v = document.createElement('span'); v.textContent = String(s == null ? '' : s); return v.innerHTML.replace(/"/g, '&quot;'); }
        function init() {
            var modal = el(modalId);
            if (!modal) return;
            var root = modal.querySelector('[data-tvd-calendar="1"]');
            if (!root) return;
            // shown.bs.modal va startup postback co the cung goi init: khong fetch hai lan.
            if (state.root === root && root.getAttribute('data-tvd-initialized') === '1') return;
            root.setAttribute('data-tvd-initialized', '1');
            state.root = root;
            state.start = toKey(val(startId));
            state.due = toKey(val(dueId));
            state.actual = toKey(val(actualId));
            modal.querySelectorAll('[data-tvd-jump]').forEach(function (button) {
                var marker = button.getAttribute('data-tvd-jump');
                var dateValue = marker === 'start' ? state.start : marker === 'due' ? state.due : marker === 'actual' ? state.actual : '';
                button.disabled = !parse(dateValue);
                button.title = dateValue ? 'Đến ngày ' + fmt(dateValue) + ' trên lịch' : 'Chưa có ngày để xem';
            });
            state.flashKey = ''; state.flashUntil = 0;
            state.code = (el(taskNameId) && el(taskNameId).getAttribute('data-tvd-code')) || '';
            closePeople();
            fitAvatars(modal);
            var actualLegend = root.querySelector('.tvd-legend-actual');
            if (actualLegend) actualLegend.style.backgroundColor = '#9865c8';
            state.days = {};
            state.cache = {};
            state.request++;
            var focus = parse((state.actual && (!state.due || state.actual > state.due)) ? state.actual : (state.due || state.start)) || new Date();
            state.y = focus.getFullYear(); state.m = focus.getMonth();
            draw(); fetchMonth();
        }
        function draw() {
            var root = state.root;
            if (!root || !root.isConnected) return;
            var title = root.querySelector('[data-tvd-month-name="1"]');
            if (title) title.textContent = 'Tháng ' + (state.m + 1) + '/' + state.y;
            var grid = root.querySelector('[data-tvd-grid="1"]');
            if (!grid) return;
            var first = new Date(state.y, state.m, 1), offset = (first.getDay() + 6) % 7;
            var total = Math.max(35, Math.ceil((offset + new Date(state.y, state.m + 1, 0).getDate()) / 7) * 7);
            var late = !!(state.actual && state.due && state.actual > state.due);
            // Kết thúc giới hạn là mốc XA NHẤT; hoàn thành sớm không được khóa mất ngày dự kiến.
            var limitEnd = state.actual && (!state.due || state.actual > state.due) ? state.actual : state.due;
            var html = '';
            ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'].forEach(function (v) { html += '<div class="tvd-cal-dow">' + v + '</div>'; });
            for (var i = 0; i < total; i++) {
                var d = new Date(state.y, state.m, i - offset + 1), k = key(d);
                var day = state.days[k], isWeekend = (d.getDay() === 0 || d.getDay() === 6);
                var holiday = !!(day && day.status === 'holiday');
                var weekend = !!(day && day.status === 'weekend') || (!day && isWeekend);
                var offDay = holiday || weekend || (day && day.working === false);
                var locked = (!!state.start && k < state.start) || (!!limitEnd && k > limitEnd);
                var range = !!state.start && !!state.due && k >= state.start && k <= state.due;
                var lateRange = late && k > state.due && k <= state.actual;
                var activeWorkDay = !locked && !offDay && ((range || lateRange) || !!state.start && !state.due && k === state.start);
                var isStart = k === state.start, isDue = k === state.due, isActual = k === state.actual;
                var lateWork = !locked && lateRange && !offDay;
                var css = 'tvd-cal-day' + (d.getMonth() !== state.m ? ' is-other' : '') + (locked ? ' is-locked' : '') + (holiday ? ' is-holiday' : weekend ? ' is-weekend' : '')
                    + (activeWorkDay && range ? ' is-range' : '') + (activeWorkDay && lateRange ? ' is-late-range' : '') + (lateWork ? ' is-late-work' : '')
                    + (!locked && isStart ? ' is-start' : '') + (!locked && isDue ? ' is-expected' : '') + (!locked && isActual ? ' is-actual' : '')
                    + (k === state.flashKey && Date.now() < state.flashUntil ? ' tvd-jump-flash' : '');
                var caption = '';
                if (!locked) {
                    if (isActual) caption = 'Thực tế';
                    else if (isStart) caption = 'Bắt đầu';
                    else if (isDue) caption = 'Dự kiến';
                    else if (holiday) caption = 'Nghỉ lễ';
                    else if (weekend) caption = 'Cuối tuần';
                }
                var holidayBadge = !locked && holiday && !isActual && !isStart && !isDue;
                html += '<div class="' + css + '" data-tvd-date="' + k + '" tabindex="0" role="img" aria-label="Ngày ' + fmt(k) + '">'
                    + '<span class="tvd-cal-number">' + d.getDate() + '</span>';
                if (holidayBadge) html += '<span class="tvd-cal-off-badge"><i class="fas fa-mug-hot" aria-hidden="true"></i> Nghỉ lễ</span>';
                else if (caption) html += '<span class="tvd-cal-caption' + (isActual || isStart || isDue ? ' is-milestone' : '') + '">' + esc(caption) + '</span>';
                if (activeWorkDay && state.code && (!state.actual || k <= state.actual)) html += '<span class="tvd-cal-code">[' + esc(state.code) + ']</span>';
                html += '</div>';
            }
            grid.innerHTML = html;
        }
        function fetchMonth() {
            // Dùng WebMethod lịch biểu chung SẴN CÓ của TaskList, chỉ lấy màu lễ/cuối tuần; không lưu hay đổi ngày.
            if (!state.root || !state.root.isConnected || !window.jQuery) return;
            var monthKey = state.y + '-' + z(state.m + 1);
            if (state.cache[monthKey]) { state.days = state.cache[monthKey]; draw(); return; }
            var token = ++state.request, currentRoot = state.root, y = state.y, m = state.m;
            // Chọn mốc giữa tháng để API tính thời lượng hợp lệ; ngày dự kiến thực tế vẫn lấy từ task.
            var refDate = new Date(y, m, 15);
            var req = { ngayBatDau: key(refDate), thoiHan: 1, ngayKetThucChon: null, nam: y, thang: m + 1 };
            window.jQuery.ajax({ url: apiUrl, type: 'POST', contentType: 'application/json; charset=utf-8', dataType: 'json', data: JSON.stringify(req), timeout: 12000 })
                .done(function (r) { if (token !== state.request || currentRoot !== state.root) return; var data = r && (r.d || r); if (data && data.success && data.days) { state.cache[monthKey] = data.days; state.days = data.days; draw(); } })
                .fail(function () { /* Không bịa ngày lễ nếu API không khả dụng; lịch vẫn hiển thị đầy đủ các mốc. */ });
        }
        function jumpToMarker(marker) {
            if (!state.root || !state.root.isConnected) init();
            if (!state.root || !state.root.isConnected) return;
            var dateValue = marker === 'start' ? state.start : marker === 'due' ? state.due : marker === 'actual' ? state.actual : '';
            var target = parse(dateValue);
            if (!target) return;
            hideTip();
            state.flashKey = dateValue;
            state.flashUntil = Date.now() + 1100;
            var monthChanged = state.y !== target.getFullYear() || state.m !== target.getMonth();
            state.y = target.getFullYear(); state.m = target.getMonth();
            if (monthChanged) state.days = state.cache[state.y + '-' + z(state.m + 1)] || {};
            draw();
            if (monthChanged) fetchMonth();
            // Neu popup co scroll noi bo, chi cuon vua du de o ngay duoc nhin thay.
            var cell = state.root.querySelector('[data-tvd-date="' + dateValue + '"]');
            var body = state.root.closest('.modal-body');
            if (cell && body) {
                var c = cell.getBoundingClientRect(), b = body.getBoundingClientRect();
                if ((c.top < b.top + 8 || c.bottom > b.bottom - 8) && cell.scrollIntoView)
                    cell.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'nearest' });
            }
        }
        function hideTip() { if (tip) { tip.remove(); tip = null; } }
        function showTip(cell) {
            if (!cell || !state.root || !state.root.contains(cell)) return;
            var k = cell.getAttribute('data-tvd-date');
            if (tip && tip.getAttribute('data-tvd-date') === k) return;
            hideTip();
            var d = state.days[k];
            var lines = [], late = !!state.due && !!state.actual && state.actual > state.due;
            if (k === state.start) lines.push('Bắt đầu dự kiến: ' + fmt(state.start));
            if (k === state.due) lines.push('Kết thúc dự kiến: ' + fmt(state.due));
            if (k === state.actual) {
                lines.push('Hoàn thành thực tế: ' + val(actualId));
                if (state.due) {
                    lines.push('Dự kiến hoàn thành: ' + fmt(state.due));
                    var diff = daysBetween(state.due, state.actual);
                    lines.push(diff > 0 ? 'Trễ ' + diff + ' ngày' : diff < 0 ? 'Sớm ' + (-diff) + ' ngày' : 'Đúng hạn');
                }
            }
            // Ngày trong hạn không cần tooltip. Chỉ ngày làm việc sau hạn mới hiện số ngày trễ cộng dồn.
            var calendarDay = parse(k);
            var isWeekend = calendarDay && (calendarDay.getDay() === 0 || calendarDay.getDay() === 6);
            var nonWorkingDay = !!isWeekend || !!(d && (d.status === 'holiday' || d.status === 'weekend' || d.working === false));
            if (late && k > state.due && k <= state.actual && k !== state.actual && !nonWorkingDay)
                lines.push('Trễ hạn ' + daysBetween(state.due, k) + ' ngày');
            if (state.start && k < state.start) lines.push('Ngoài giới hạn: trước ngày bắt đầu ' + fmt(state.start));
            var limitEnd = state.actual && (!state.due || state.actual > state.due) ? state.actual : state.due;
            if (limitEnd && k > limitEnd) lines.push('Ngoài giới hạn: sau mốc kết thúc ' + fmt(limitEnd));
            if (d && d.status === 'holiday') lines.push('Ngày nghỉ lễ: ' + (d.text || 'Ngày nghỉ'));
            else if (d && d.status === 'weekend') lines.push('Cuối tuần');
            else if (!d) { var pd = parse(k); if (pd && (pd.getDay() === 0 || pd.getDay() === 6)) lines.push('Cuối tuần'); }
            // Các ngày làm việc trong khoảng dự kiến chỉ giữ màu và mã task trên lịch, không bật khung hover.
            if (!lines.length && state.start && state.due && k >= state.start && k <= state.due) return;
            if (!lines.length) lines.push('Không có mốc công việc vào ngày này');
            tip = document.createElement('div'); tip.className = 'tvd-floating-tip'; tip.setAttribute('data-tvd-date', k);
            var header = document.createElement('strong'); header.textContent = fmt(k); tip.appendChild(header);
            lines.forEach(function (line) { var s = document.createElement('span'); s.className = 'tvd-tip-item'; s.textContent = line; tip.appendChild(s); });
            document.body.appendChild(tip);
            var rect = cell.getBoundingClientRect(), w = tip.offsetWidth, h = tip.offsetHeight;
            var left = rect.right + 10; if (left + w > window.innerWidth - 8) left = rect.left - w - 10;
            if (left < 8) left = Math.max(8, Math.min(window.innerWidth - w - 8, rect.left));
            var top = rect.top + (rect.height - h) / 2;
            top = Math.max(8, Math.min(window.innerHeight - h - 8, top));
            tip.style.left = left + 'px'; tip.style.top = top + 'px';
        }
        function fitAvatars(modal) {
            if (!modal) return;
            var stack = modal.querySelector('[data-tvd-avatar-stack="1"]');
            var source = modal.querySelector('.tvd-assignee-source .task-assignee-list');
            var trigger = modal.querySelector('[data-tvd-people="1"]');
            if (!stack || !source || !trigger) return;
            var rows = source.querySelectorAll('.task-assignee-row');
            stack.querySelectorAll('.tvd-avatar-item').forEach(function (n) { n.remove(); });
            var more = stack.querySelector('.tvd-avatar-more');
            var width = stack.clientWidth || 240;
            // Avatar 38 px, chồng 8 px. Giữ 38px cho thẻ +N khi thiếu chỗ.
            var capacity = Math.max(0, Math.floor((width - 38) / 30));
            if (rows.length && width >= 40) capacity = Math.max(1, capacity);
            if (rows.length <= Math.floor((width - 8) / 30)) capacity = rows.length;
            var visible = Math.min(rows.length, capacity);
            for (var i = 0; i < visible; i++) {
                var ava = rows[i].querySelector('.task-assignee-avatar');
                var name = rows[i].querySelector('.task-assignee-name');
                var wrap = document.createElement('span'); wrap.className = 'tvd-avatar-item';
                if (name) wrap.title = name.textContent;
                if (ava) wrap.innerHTML = ava.innerHTML;
                stack.insertBefore(wrap, more);
            }
            var remaining = rows.length - visible;
            more.hidden = remaining < 1;
            if (remaining) more.textContent = '+' + remaining;
            trigger.hidden = rows.length < 1;
            trigger.setAttribute('aria-label', 'Xem danh sách ' + rows.length + ' nhân viên thực hiện');
        }
        function stopPeopleCloseTimer() { if (peopleTimer) clearTimeout(peopleTimer); peopleTimer = null; }
        function setPeoplePinned(pinned) {
            peoplePinned = !!pinned;
            stopPeopleCloseTimer();
            if (peopleTip) {
                peopleTip.classList.toggle('is-pinned', peoplePinned);
                var pin = peopleTip.querySelector('[data-tvd-pin-people]');
                if (pin) { pin.setAttribute('aria-pressed', String(peoplePinned)); pin.innerHTML = peoplePinned ? '<i class="fas fa-thumbtack"></i> Đã ghim' : '<i class="fas fa-thumbtack"></i> Ghim'; pin.title = peoplePinned ? 'Bấm để bỏ ghim' : 'Bấm để ghim bảng nhân viên'; }
            }
            if (peopleTrigger) {
                peopleTrigger.setAttribute('aria-pressed', String(peoplePinned));
                peopleTrigger.setAttribute('aria-expanded', peopleTip ? 'true' : 'false');
                peopleTrigger.title = peoplePinned ? 'Đã ghim - bấm lần nữa để đóng' : 'Bấm để ghim bảng nhân viên';
            }
        }
        function closePeople() {
            stopPeopleCloseTimer();
            if (peopleTrigger) { peopleTrigger.setAttribute('aria-expanded', 'false'); peopleTrigger.setAttribute('aria-pressed', 'false'); peopleTrigger.title = 'Bấm để ghim bảng nhân viên'; }
            if (peopleTip && peopleTip.parentNode) peopleTip.parentNode.removeChild(peopleTip);
            peopleTip = null; peopleTrigger = null; peoplePinned = false;
        }
        function positionPeople(trigger) {
            if (!peopleTip || !trigger || !trigger.isConnected) return;
            var r = trigger.getBoundingClientRect();
            var w = peopleTip.offsetWidth, h = peopleTip.offsetHeight;
            var vw = window.innerWidth || document.documentElement.clientWidth;
            var vh = window.innerHeight || document.documentElement.clientHeight;
            var edge = 10, gap = 12;
            // Ưu tiên mở về bên phải của nút, thay vì đè xuống thẻ Nhân viên/Mô tả.
            var left = r.right + gap;
            if (left + w > vw - edge) {
                // Màn hình hẹp: chỉ đổi sang trái khi bên phải thật sự không đủ chỗ.
                var leftSide = r.left - w - gap;
                left = leftSide >= edge ? leftSide : Math.max(edge, vw - w - edge);
            }
            // ĐÁY popup ngang đáy nút Xem danh sách; danh sách dài sẽ mọc ngược lên trên.
            // Khi chiều cao popup vượt phần trên viewport, giới hạn top để không bị cắt.
            var top = r.bottom - h;
            top = Math.max(edge, Math.min(vh - h - edge, top));
            peopleTip.style.left = Math.round(left) + 'px';
            peopleTip.style.top = Math.round(top) + 'px';
        }
        function showPeople(trigger) {
            if (!trigger) return;
            if (trigger === peopleTrigger && peopleTip) { stopPeopleCloseTimer(); return; }
            closePeople();
            var modal = el(modalId), source = modal && modal.querySelector('.tvd-assignee-source .task-assignee-list');
            if (!source) return;
            var count = source.querySelectorAll('.task-assignee-row').length;
            if (!count) return;
            peopleTip = document.createElement('div'); peopleTip.className = 'tvd-staff-popover'; peopleTip.setAttribute('role', 'dialog'); peopleTip.setAttribute('aria-label', 'Danh sách nhân viên thực hiện');
            var heading = document.createElement('div'); heading.className = 'tvd-staff-popover-heading';
            var label = document.createElement('span'); label.textContent = 'Nhân viên thực hiện'; heading.appendChild(label);
            var actions = document.createElement('span'); actions.className = 'tvd-staff-popover-head-actions';
            var badge = document.createElement('span'); badge.className = 'tvd-staff-popover-count'; badge.textContent = count + ' người'; actions.appendChild(badge);
            var pin = document.createElement('button'); pin.type = 'button'; pin.className = 'tvd-staff-pin'; pin.setAttribute('data-tvd-pin-people', '1'); pin.setAttribute('aria-pressed', 'false'); pin.title = 'Bấm để ghim bảng nhân viên'; pin.innerHTML = '<i class="fas fa-thumbtack"></i> Ghim'; actions.appendChild(pin);
            heading.appendChild(actions); peopleTip.appendChild(heading); peopleTip.appendChild(source.cloneNode(true));
            document.body.appendChild(peopleTip); peopleTrigger = trigger; positionPeople(trigger);
            trigger.setAttribute('aria-expanded', 'true');
            peopleTip.addEventListener('mouseenter', stopPeopleCloseTimer);
            peopleTip.addEventListener('mouseleave', function () { scheduleClosePeople(); });
        }
        function scheduleClosePeople() { stopPeopleCloseTimer(); if (peoplePinned) return; peopleTimer = setTimeout(function () { if (!peoplePinned) closePeople(); }, 200); }
        document.addEventListener('mouseover', function (e) { var trigger = e.target.closest && e.target.closest('#' + modalId + ' [data-tvd-people="1"]'); if (trigger) showPeople(trigger); });
        document.addEventListener('mouseout', function (e) { var trigger = e.target.closest && e.target.closest('#' + modalId + ' [data-tvd-people="1"]'); if (trigger && !trigger.contains(e.relatedTarget)) scheduleClosePeople(); });
        document.addEventListener('focusin', function (e) { var trigger = e.target.closest && e.target.closest('#' + modalId + ' [data-tvd-people="1"]'); if (trigger) showPeople(trigger); });
        document.addEventListener('focusout', function (e) { if (e.target === peopleTrigger && !peoplePinned && (!peopleTip || !peopleTip.contains(e.relatedTarget))) scheduleClosePeople(); });
        document.addEventListener('click', function (e) {
            var trigger = e.target.closest && e.target.closest('#' + modalId + ' [data-tvd-people="1"]');
            if (trigger) { e.preventDefault(); if (peopleTrigger !== trigger || !peopleTip) showPeople(trigger); if (peoplePinned) closePeople(); else setPeoplePinned(true); return; }
            var pin = e.target.closest && e.target.closest('.tvd-staff-popover [data-tvd-pin-people]');
            if (pin && peopleTip && peopleTip.contains(pin)) { e.preventDefault(); e.stopPropagation(); if (peoplePinned) closePeople(); else setPeoplePinned(true); return; }
            // Nhấn ngay trên danh sách đang hover cũng có thể ghim bảng.
            if (peopleTip && peopleTip.contains(e.target)) { if (!peoplePinned) setPeoplePinned(true); return; }
            if (peopleTip) closePeople();
        });
        document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && peopleTip) { closePeople(); } });
        document.addEventListener('click', function (e) { var jump = e.target.closest && e.target.closest('#' + modalId + ' [data-tvd-jump]'); if (!jump) return; e.preventDefault(); if (!jump.disabled) jumpToMarker(jump.getAttribute('data-tvd-jump')); });
        document.addEventListener('click', function (e) { var nav = e.target.closest && e.target.closest('#' + modalId + ' [data-tvd-month]'); if (!nav) return; e.preventDefault(); hideTip(); if (!state.root) return; var step = Number(nav.getAttribute('data-tvd-month')); if (step !== 1 && step !== -1) return; var dt = new Date(state.y, state.m + step, 1); state.y = dt.getFullYear(); state.m = dt.getMonth(); state.days = {}; state.flashKey = ''; state.flashUntil = 0; draw(); fetchMonth(); });
        document.addEventListener('mouseover', function (e) { var cell = e.target.closest && e.target.closest('#' + modalId + ' [data-tvd-date]'); if (cell) showTip(cell); });
        document.addEventListener('mouseout', function (e) { var cell = e.target.closest && e.target.closest('#' + modalId + ' [data-tvd-date]'); if (cell && !cell.contains(e.relatedTarget)) hideTip(); });
        document.addEventListener('focusin', function (e) { var cell = e.target.closest && e.target.closest('#' + modalId + ' [data-tvd-date]'); if (cell) showTip(cell); });
        document.addEventListener('focusout', function (e) { var cell = e.target.closest && e.target.closest('#' + modalId + ' [data-tvd-date]'); if (cell) hideTip(); });
        window.addEventListener('scroll', function (e) { hideTip(); if (peoplePinned && peopleTrigger && peopleTip) { positionPeople(peopleTrigger); } else if (peopleTip && !peopleTip.contains(e.target)) { closePeople(); } }, true);
        window.addEventListener('resize', function () { hideTip(); fitAvatars(el(modalId)); if (peoplePinned && peopleTrigger && peopleTip) positionPeople(peopleTrigger); else closePeople(); });
        document.addEventListener('hidden.bs.modal', function (e) { if (e.target && e.target.id === modalId) { state.request++; hideTip(); closePeople(); state.root = null; } });
        if (window.jQuery) { window.jQuery(document).on('shown.bs.modal.tvdCal', '#' + modalId, function () { init(); }).on('hidden.bs.modal.tvdCal', '#' + modalId, function () { state.request++; hideTip(); closePeople(); state.root = null; }); }
        /* Bảng công của chính tài khoản đang xem: không chuyển trang, không mượn singleton DuAnDetail. */
        var laborModalId = '<%= TaskLaborControl.TaskModalClientId %>';
        var laborTypeId = '<%= TaskLaborControl.TaskTypeClientId %>';
        var laborStaffId = '<%= TaskLaborControl.TaskStaffClientId %>';
        var laborLoadId = '<%= TaskLaborControl.TaskLoadClientId %>';
        var laborBodyId = '<%= TaskLaborControl.TaskBodyHtmlId %>';
        var laborPending = false, laborFocus = '';
        function laborModal() { var v = el(laborModalId); return v && v.classList.contains('modal') ? v : (v ? v.querySelector('.modal') : null); }
        function isShown(v) { return !!(v && (v.classList.contains('show') || v.classList.contains('in'))); }
        // Không kích hoạt Bootstrap.Modal cho popup công khi ViewTask còn đang mở:
        // Bootstrap thêm một backdrop vào body, làm ViewTask và bảng công kẹt ở hai tầng khác nhau.
        // Nội dung vẫn là SweetSoft:ExtraModal, chỉ hiện nó như lớp nội bộ của ViewTask.
        var laborMountParent = null, laborMountNext = null, laborOpen = false, laborReturnFocus = null;
        function mountLaborInsideTask() {
            var popup = laborModal(), parent = el(modalId);
            if (!popup || !parent) return false;
            var taskForm = parent.closest('form'), laborForm = popup.closest('form');
            if (!taskForm || taskForm !== laborForm) return false; // LinkButton/UpdatePanel phải ở trong cùng form WebForms.
            if (popup.parentNode !== parent) {
                laborMountParent = popup.parentNode;
                laborMountNext = popup.nextSibling;
                parent.appendChild(popup); // cùng stacking context với ViewTask, trên chính .modal-dialog.
            }
            return true;
        }
        function restoreLaborParent() {
            var popup = laborModal();
            if (!popup || !laborMountParent) return;
            if (laborMountNext && laborMountNext.parentNode === laborMountParent) laborMountParent.insertBefore(popup, laborMountNext);
            else laborMountParent.appendChild(popup);
            laborMountParent = null; laborMountNext = null;
        }
        function showLaborLayer() {
            var popup = laborModal(), under = el(modalId);
            if (!popup || !under || !mountLaborInsideTask()) {
                laborPending = false;
                if (window.QLDAModalLoading && typeof window.QLDAModalLoading.end === 'function') window.QLDAModalLoading.end();
                return;
            }
            // Bật lớp công bằng CSS; tuyệt đối KHÔNG gọi bootstrap.Modal.show() / jQuery.modal('show').
            // Vậy sẽ không xuất hiện backdrop thứ hai, và focus trap của ViewTask vẫn bao lớp công.
            laborOpen = true;
            under.classList.add('tvd-labor-underlay');
            popup.classList.add('show', 'tvd-labor-top-layer');
            popup.style.display = 'block';
            popup.setAttribute('role', 'dialog');
            popup.setAttribute('aria-modal', 'true');
            popup.removeAttribute('aria-hidden');
            document.body.classList.add('modal-open');
            var heading = popup.querySelector('.modal-header .modal-title, .pd-detail-modal-title');
            if (heading && heading.setAttribute) heading.setAttribute('tabindex', '-1');
            if (heading && heading.focus) heading.focus({ preventScroll: true });
        }
        function hideLaborLayer() {
            var popup = laborModal(), under = el(modalId);
            if (!popup) return;
            laborOpen = false; laborPending = false; laborFocus = '';
            popup.classList.remove('show', 'tvd-labor-top-layer');
            popup.style.removeProperty('display');
            popup.removeAttribute('aria-modal');
            popup.setAttribute('aria-hidden', 'true');
            if (under) under.classList.remove('tvd-labor-underlay');
            restoreLaborParent();
            // Chưa đóng ViewTask, vì vậy tuyệt đối không gỡ modal-open của trang.
            if (under && isShown(under)) document.body.classList.add('modal-open');
            var returnFocus = laborReturnFocus; laborReturnFocus = null;
            if (returnFocus && returnFocus.isConnected && returnFocus.focus) returnFocus.focus({ preventScroll: true });
        }
        function focusLaborRow() {
            var popup = laborModal(), body = el(laborBodyId); if (!popup || !body || !laborFocus) return;
            var row = body.querySelector('tr[data-pd-task-id="' + laborFocus + '"]'); if (!row) return;
            var scrollBox = row.closest('.pd-ds-table-scroller'); if (scrollBox) scrollBox.scrollLeft = 0;
            var modalBody = row.closest('.modal-body');
            if (modalBody) modalBody.scrollTop += row.getBoundingClientRect().top - modalBody.getBoundingClientRect().top - 115;
            row.classList.remove('tvd-labor-focus'); void row.offsetWidth; row.classList.add('tvd-labor-focus');
            window.setTimeout(function () { row.classList.remove('tvd-labor-focus'); }, 1850);
        }
        function laborReceived() {
            laborPending = false;
            showLaborLayer();
            if (laborOpen) window.setTimeout(focusLaborRow, 80);
            if (window.QLDAModalLoading && typeof window.QLDAModalLoading.end === 'function') window.QLDAModalLoading.end();
        }
        function openLabor(taskId) {
            if (laborPending || laborOpen || !/^[0-9a-f]{8}(?:-[0-9a-f]{4}){3}-[0-9a-f]{12}$/i.test(taskId || '')) return;
            var type = el(laborTypeId), staff = el(laborStaffId), load = el(laborLoadId);
            if (!type || !staff || !load) return;
            laborFocus = taskId.toLowerCase(); laborPending = true;
            laborReturnFocus = el(modalId) && el(modalId).querySelector('[data-tvd-people="1"]');
            type.value = 'pdPersonalTaskDetail'; staff.value = '';
            closePeople();
            if (window.QLDAModalLoading && typeof window.QLDAModalLoading.expect === 'function') window.QLDAModalLoading.expect(load);
            try { load.click(); } catch (ex) { laborPending = false; if (window.QLDAModalLoading && typeof window.QLDAModalLoading.end === 'function') window.QLDAModalLoading.end(); throw ex; }
        }
        // Bắt đóng modal từ nút X/Đóng/ESC trên lớp trên, KHÔNG cho Bootstrap đóng cả ViewTask.
        document.addEventListener('click', function (e) {
            var target = e.target && e.target.closest && e.target.closest('.tvd-staff-popover [data-tvd-labor-task-id]');
            if (target) { e.preventDefault(); e.stopPropagation(); openLabor(target.getAttribute('data-tvd-labor-task-id')); return; }
            if (!laborOpen) return;
            var popup = laborModal(); if (!popup || !popup.contains(e.target)) return;
            var closeButton = e.target.closest && e.target.closest('[data-bs-dismiss="modal"],[data-dismiss="modal"],.modal-header .close,.modal-header .btn-close,.modal-footer .close,.modal-footer .btn-close,.modal-footer [onclick*="modal"]');
            if (e.target === popup || closeButton) { e.preventDefault(); e.stopPropagation(); e.stopImmediatePropagation(); hideLaborLayer(); }
        }, true);
        document.addEventListener('keydown', function (e) {
            if (laborOpen && (e.key === 'Escape' || e.key === 'Esc')) {
                e.preventDefault(); e.stopPropagation(); e.stopImmediatePropagation(); hideLaborLayer();
            }
        }, true);
        // Parent có thể đóng bằng X khi lớp công chưa được đóng; luôn trả các control về DOM gốc.
        function onParentClosed(e) { if (e.target === el(modalId) && laborOpen) hideLaborLayer(); }
        document.addEventListener('hidden.bs.modal', onParentClosed);
        if (window.jQuery) window.jQuery(document).on('hidden.bs.modal.tvdLabor', '#' + modalId, onParentClosed);
        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
                if (laborPending && args.get_error()) {
                    laborPending = false;
                    if (window.QLDAModalLoading && typeof window.QLDAModalLoading.end === 'function') window.QLDAModalLoading.end();
                }
            });
        }
        window.QLDATaskLaborLayer = { received: laborReceived };
        window.QLDATaskDetailCalendar = { init: init };
    })();
</script>
