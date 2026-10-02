<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlSwapPhase.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlSwapPhase" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    .btn-swap-icon { width: 48px; height: 48px; border-radius: 50%; display: flex; align-items: center; justify-content: center; background-color: #f1f5f9; color: #64748b; transition: all .3s ease; text-decoration: none; border: 1px solid #cbd5e1; box-shadow: 0 2px 5px rgba(0,0,0,.05); }
    .btn-swap-icon:hover { background-color: #6366f1; color: #fff; border-color: #6366f1; transform: scale(1.05); box-shadow: 0 4px 12px rgba(99,102,241,.3); }
    .reorder-warning { background: linear-gradient(135deg,#fff7ed 0%,#fffaf5 100%); border: 1px solid #fed7aa; color: #9a3412; border-radius: 10px; padding: 13px 15px; margin-bottom: 14px; }
    .reorder-warning-title { display: flex; align-items: center; gap: 7px; font-weight: 700; margin-bottom: 4px; }
    .reorder-summary { background: linear-gradient(180deg,#f8fafc 0%,#f3f6fa 100%); border: 1px solid #dde5ee; border-radius: 12px; padding: 13px; margin-bottom: 14px; }
    .reorder-summary-title { color: #334155; font-weight: 750; font-size: 14px; margin-bottom: 9px; }
    .reorder-table-wrap { max-height: 345px; overflow: auto; border: 1px solid #dfe7f0; border-radius: 10px; background: #fff; box-shadow: inset 0 1px 0 rgba(255,255,255,.75); }
    .reorder-table { width: 100%; min-width: 0; border-collapse: separate; border-spacing: 0; font-size: 11.5px; color: #334155; }
    .reorder-table th { position: sticky; top: 0; z-index: 2; background: linear-gradient(180deg,#f8fafc 0%,#eef3f8 100%); color: #475569; font-weight: 800; padding: 9px 10px; border-bottom: 1px solid #d9e2ec; text-align: center; white-space: nowrap; }
    .reorder-table th:first-child { text-align: left; }
    .reorder-table td { padding: 8px 10px; border-bottom: 1px solid #edf2f7; vertical-align: middle; }
    .reorder-table tbody tr:last-child td { border-bottom: 0; }
    .reorder-table tbody tr { transition: background-color .18s ease, transform .18s ease; }
    .reorder-table tbody tr:hover { background: #fafbff; }
    .reorder-table .task-cell { min-width: 0; }
    .reorder-task-main { display: flex; align-items: center; gap: 7px; min-width: 0; }
    .reorder-task-indent { flex: 0 0 auto; width: 0; }
    .reorder-task-code { display: inline-flex; align-items: center; justify-content: center; min-width: 29px; padding: 3px 6px; border-radius: 6px; background: #ede9fe; color: #5b21b6; font-weight: 800; font-size: 10.5px; line-height: 1.2; white-space: nowrap; }
    .reorder-task-name { min-width: 0; color: #334155; font-weight: 600; overflow-wrap: anywhere; line-height: 1.35; }
    .reorder-table tr.phase-row td:first-child { border-left: 3px solid #8b5cf6; }
    .reorder-table tr.phase-row { background: linear-gradient(90deg,#fbf9ff 0%,#fff 72%); }
    .reorder-table tr.phase-row .reorder-task-name { font-weight: 750; color: #2f1b50; }
    .reorder-table tr.child-row td:first-child { border-left: 3px solid #cbd5e1; }
    .reorder-task-meta { margin-top: 2px; color: #94a3b8; font-size: 10px; }
    .reorder-center { text-align: center; white-space: nowrap; }
    .reorder-date { text-align: center; white-space: nowrap; color: #475569; font-variant-numeric: tabular-nums; }
    .reorder-date strong { color: #334155; font-weight: 700; }
    .reorder-status { text-align: center; white-space: nowrap; }
    .reorder-status-badge { display: inline-flex; align-items: center; justify-content: center; min-width: 84px; padding: 5px 8px; border-radius: 999px; font-size: 10px; font-weight: 750; line-height: 1.2; white-space: nowrap; }
    .reorder-status-badge.status-todo { background: #f1f5f9; color: #475569; border: 1px solid #cbd5e1; }
    .reorder-status-badge.status-doing { background: #e0f2fe; color: #0369a1; border: 1px solid #bae6fd; }
    .reorder-status-badge.status-done { background: #dcfce7; color: #15803d; border: 1px solid #bbf7d0; }
    .reorder-dependency { text-align: center; white-space: nowrap; color: #475569; font-weight: 700; }
    .reorder-dependency-empty { color: #94a3b8; font-weight: 500; }

    .reorder-option-grid { display: grid; grid-template-columns: repeat(2,minmax(0,1fr)); gap: 10px; margin-bottom: 12px; }
    .reorder-option { display: flex; align-items: center; gap: 11px; min-width: 0; background: linear-gradient(180deg,#fff 0%,#fbfdff 100%); border: 1px solid #e2e8f0; border-radius: 12px; padding: 10px 11px; transition: border-color .2s ease, box-shadow .2s ease, transform .2s ease; }
    .reorder-option:hover { border-color: #c4b5fd; box-shadow: 0 7px 16px rgba(15,23,42,.055); transform: translateY(-1px); }
    .reorder-option-copy { flex: 1; min-width: 0; }
    .reorder-option-title { display: flex; align-items: center; gap: 6px; font-weight: 750; color: #334155; line-height: 1.3; font-size: 12.5px; }
    .reorder-option-title i { width: 22px; height: 22px; border-radius: 7px; display: inline-flex; align-items: center; justify-content: center; flex: 0 0 22px; background: #f3efff; color: #6d28d9; font-size: 10px; }
    .reorder-option-desc { font-size: 10.5px; color: #64748b; line-height: 1.4; margin-top: 3px; }

    .reorder-switch-button { display: block; width: 84px; flex: 0 0 84px; padding: 0; border: 0; background: transparent; text-decoration: none !important; cursor: pointer; }
    .reorder-switch-button:focus { outline: none; }
    .reorder-switch { position: relative; display: flex; align-items: center; width: 84px; height: 30px; padding: 3px; border-radius: 999px; overflow: hidden; box-shadow: inset 0 1px 2px rgba(15,23,42,.14),0 2px 5px rgba(15,23,42,.07); transition: background .25s ease, box-shadow .25s ease, transform .2s ease; }
    .reorder-switch.off { background: linear-gradient(135deg,#ef4444 0%,#dc2626 100%); }
    .reorder-switch.on { background: linear-gradient(135deg,#34d399 0%,#16a34a 100%); }
    .reorder-switch-button:hover .reorder-switch { transform: scale(1.02); box-shadow: inset 0 1px 2px rgba(15,23,42,.12),0 5px 11px rgba(15,23,42,.12); }
    .reorder-switch-button:focus .reorder-switch { outline: 2px solid rgba(99,102,241,.3); outline-offset: 2px; }
    .reorder-switch-thumb { position: absolute; top: 4px; width: 22px; height: 22px; border-radius: 50%; background: linear-gradient(180deg,#fff 0%,#e5e7eb 100%); border: 1px solid rgba(15,23,42,.13); box-shadow: 0 1px 4px rgba(15,23,42,.22); transition: left .28s cubic-bezier(.4,0,.2,1), transform .28s cubic-bezier(.4,0,.2,1); }
    .reorder-switch.off .reorder-switch-thumb { left: 4px; }
    .reorder-switch.on .reorder-switch-thumb { left: 58px; }
    .reorder-switch-state { position: absolute; top: 0; height: 30px; display: flex; align-items: center; color: #fff; font-size: 9.5px; font-weight: 900; letter-spacing: .55px; transition: opacity .2s ease, transform .25s ease; }
    .reorder-switch.off .reorder-switch-state { right: 10px; }
    .reorder-switch.on .reorder-switch-state { left: 10px; }

    .reorder-review-button { display: inline-flex; align-items: center; justify-content: center; gap: 7px; width: 100%; min-height: 36px; border: 1px dashed #c4b5fd; background: linear-gradient(180deg,#fcfbff 0%,#f7f5ff 100%); color: #5b21b6; border-radius: 10px; padding: 7px 11px; font-weight: 750; font-size: 11.5px; text-decoration: none !important; transition: all .2s ease; }
    .reorder-review-button:hover { background: #f3efff; color: #4c1d95; border-color: #8b5cf6; transform: translateY(-1px); box-shadow: 0 6px 14px rgba(91,33,182,.07); }

    .reorder-review-shell { animation: reorderReviewIn .25s ease both; }
    .reorder-review-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 15px; margin-bottom: 11px; }
    .reorder-review-caption { color: #64748b; font-size: 11px; line-height: 1.45; margin-top: 3px; }
    .reorder-review-options { display: grid; grid-template-columns: repeat(2,minmax(0,1fr)); gap: 9px; margin-bottom: 11px; }
    .reorder-review-board { display: grid; grid-template-columns: minmax(0,1fr) minmax(0,1fr); gap: 12px; }
    .reorder-review-column { min-width: 0; border: 1px solid #dfe7f0; border-radius: 11px; overflow: hidden; background: #fff; box-shadow: 0 3px 12px rgba(15,23,42,.035); }
    .reorder-review-column-title { display: flex; align-items: center; justify-content: space-between; gap: 8px; padding: 9px 11px; border-bottom: 1px solid #e2e8f0; background: linear-gradient(180deg,#f8fafc 0%,#f1f5f9 100%); }
    .reorder-review-column-title strong { color: #334155; font-size: 12.5px; }
    .reorder-review-column-title span { color: #64748b; font-size: 10px; }
    .reorder-review-list { max-height: 56vh; overflow: auto; padding: 6px; }
    .reorder-review-list .reorder-table { min-width: 0; width: 100%; }

    .reorder-empty { padding: 24px 12px; text-align: center; color: #94a3b8; font-size: 11.5px; }
    .reorder-scrollbar::-webkit-scrollbar, .reorder-table-wrap::-webkit-scrollbar, .reorder-review-list::-webkit-scrollbar { width: 6px; height: 6px; }
    .reorder-scrollbar::-webkit-scrollbar-thumb, .reorder-table-wrap::-webkit-scrollbar-thumb, .reorder-review-list::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 999px; }
    .reorder-scrollbar::-webkit-scrollbar-track, .reorder-table-wrap::-webkit-scrollbar-track, .reorder-review-list::-webkit-scrollbar-track { background: transparent; }

    [id$="_mdlReorderOptions"] .modal-dialog, [id$="mdlReorderOptions"] .modal-dialog { max-width: 1020px !important; width: calc(100vw - 34px) !important; }
    [id$="_mdlReorderReview"] .modal-dialog, [id$="mdlReorderReview"] .modal-dialog { max-width: 1440px !important; width: calc(100vw - 28px) !important; }
    [id$="_mdlReorderOptions"] .modal-content, [id$="mdlReorderOptions"] .modal-content, [id$="_mdlReorderReview"] .modal-content, [id$="mdlReorderReview"] .modal-content { border-radius: 13px !important; overflow: visible !important; }
    [id$="_mdlReorderOptions"] .modal-header, [id$="mdlReorderOptions"] .modal-header, [id$="_mdlReorderReview"] .modal-header, [id$="mdlReorderReview"] .modal-header { border-top-left-radius: 13px !important; border-top-right-radius: 13px !important; }

    .reorder-loading-btn { display: inline-flex; align-items: center; justify-content: center; gap: 8px; }
    .reorder-modal-loading { position: relative; }
    .reorder-loading-overlay { position: absolute; inset: 0; z-index: 9999; display: flex; align-items: center; justify-content: center; background: rgba(255,255,255,.8); backdrop-filter: blur(2px); border-radius: 13px; animation: reorderLoadingFadeIn .18s ease both; pointer-events: none; }
    .reorder-loading-box { min-width: 290px; max-width: 90%; padding: 22px 24px; text-align: center; background: rgba(255,255,255,.97); border: 1px solid #e2e8f0; border-radius: 14px; box-shadow: 0 14px 40px rgba(15,23,42,.14); animation: reorderLoadingPopup .22s ease both; }
    .reorder-loading-spinner { width: 50px; height: 50px; margin: 0 auto 12px; display: flex; align-items: center; justify-content: center; border-radius: 50%; background: #f3efff; color: #6366f1; font-size: 21px; }
    .reorder-loading-title { color: #334155; font-size: 14px; font-weight: 800; line-height: 1.4; }
    .reorder-loading-desc { margin-top: 5px; color: #64748b; font-size: 11px; line-height: 1.45; }
    .reorder-loading-progress { width: 170px; height: 4px; margin: 14px auto 0; overflow: hidden; border-radius: 999px; background: #e2e8f0; }
    .reorder-loading-progress::after { content: ""; display: block; width: 45%; height: 100%; border-radius: 999px; background: #6366f1; animation: reorderLoadingProgress 1.05s ease-in-out infinite; }

    .reorder-review-back-button:hover { color: #4f46e5 !important; background: #f5f3ff !important; border-radius: 7px !important; }

    @keyframes reorderReviewIn {
        from { opacity: 0; transform: translateY(6px) scale(.995); }
        to { opacity: 1; transform: translateY(0) scale(1); }
    }

    @keyframes reorderLoadingFadeIn {
        from { opacity: 0; }
        to { opacity: 1; }
    }

    @keyframes reorderLoadingPopup {
        from { opacity: 0; transform: translateY(8px) scale(.97); }
        to { opacity: 1; transform: translateY(0) scale(1); }
    }

    @keyframes reorderLoadingProgress {
        0% { transform: translateX(-190%); }
        100% { transform: translateX(390%); }
    }

    @media (max-width: 980px) {
        .reorder-option-grid, .reorder-review-options { grid-template-columns: 1fr; }
        .reorder-review-board { grid-template-columns: 1fr; }
        .reorder-review-list { max-height: 42vh; }
    }

    @media (max-width: 640px) {
        .reorder-option { align-items: center; }
        .reorder-switch-button, .reorder-switch { width: 82px; flex-basis: 82px; }
        .reorder-switch.on .reorder-switch-thumb { left: 56px; }
        .reorder-review-header { flex-direction: column; }
        .reorder-loading-box { min-width: 250px; padding: 20px 18px; }
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
                            <asp:DropDownList ID="ddlPhase1" runat="server" CssClass="form-select border-primary shadow-sm" style="font-weight: 500;" AutoPostBack="true" OnSelectedIndexChanged="ddlPhase_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                        <div class="col-2 d-flex justify-content-center pb-1">
                            <asp:LinkButton ID="btnConfirmSwap" runat="server" CssClass="btn-swap-icon" OnClick="btnConfirmSwap_Click" ToolTip="Bấm để hoán đổi" CausesValidation="false">
                                <i class="fas fa-exchange-alt fs-5"></i>
                            </asp:LinkButton>
                        </div>
                        <div class="col-5 text-start">
                            <label class="form-label fw-bold text-danger"><i class="fas fa-flag me-1"></i> Giai đoạn B</label>
                            <asp:DropDownList ID="ddlPhase2" runat="server" CssClass="form-select border-danger shadow-sm" style="font-weight: 500;" AutoPostBack="true" OnSelectedIndexChanged="ddlPhase_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
</SweetSoft:ExtraModal>

<SweetSoft:ExtraModal DefaultButton="btnConfirmReorder" ID="mdlReorderOptions" Type="Primary" runat="server" Title="Xác nhận thay đổi thứ tự giai đoạn">
    <ContentTemplate>
        <asp:UpdatePanel ID="upReorderOptions" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="p-3">
                    <div class="reorder-warning">
                        <div class="reorder-warning-title">
                            <i class="fas fa-exclamation-triangle"></i>
                            <span>Cảnh báo</span>
                        </div>
                        <asp:Literal ID="ltrReorderWarning" runat="server"></asp:Literal>
                    </div>

                    <div class="reorder-summary">
                        <div class="reorder-summary-title">Thứ tự sau khi đổi</div>
                        <div class="reorder-table-wrap reorder-scrollbar">
                            <asp:Literal ID="ltrReorderOrder" runat="server"></asp:Literal>
                        </div>
                    </div>

                    <div class="reorder-option-grid">
                        <div class="reorder-option">
                            <div class="reorder-option-copy">
                                <div class="reorder-option-title">
                                    <i class="fas fa-calendar-check"></i>
                                    <span>Cập nhật lịch tự động</span>
                                </div>
                                <div class="reorder-option-desc">Tự tính lại ngày bắt đầu và kết thúc theo phụ thuộc mới.</div>
                            </div>
                            <asp:LinkButton ID="btnToggleAutoUpdateDates" runat="server" CssClass="reorder-switch-button" OnClick="btnToggleAutoUpdateDates_Click" CausesValidation="false">
                                <asp:Literal ID="ltrAutoUpdateSwitch" runat="server"></asp:Literal>
                            </asp:LinkButton>
                        </div>
                    </div>

                    <asp:LinkButton ID="btnOpenReorderReview" runat="server" CssClass="reorder-review-button" OnClick="btnOpenReorderReview_Click" CausesValidation="false">
                        <i class="fas fa-table"></i>
                        <span>Xem chi tiết công việc trước và sau</span>
                    </asp:LinkButton>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>

    <FooterTemplate>
        <asp:LinkButton ID="btnConfirmReorder" runat="server" CssClass="btn btn-primary waves-effect waves-light" CausesValidation="false" OnClientClick="return setReorderLoading(this);" OnClick="btnConfirmReorder_Click">
            <i class="fas fa-check me-1"></i> Áp dụng thay đổi
        </asp:LinkButton>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<SweetSoft:ExtraModal DefaultButton="btnConfirmReorderFromReview" ID="mdlReorderReview" Type="Primary" runat="server" Title="Chi tiết công việc bị ảnh hưởng">
    <ContentTemplate>
        <asp:UpdatePanel ID="upReorderReview" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="p-3 reorder-review-shell">
                    <div class="reorder-review-header">
                        <div>
                            <div class="fw-bold" style="color:#334155;font-size:15px;">So sánh trước và sau khi đổi thứ tự</div>
                            <div class="reorder-review-caption">Chỉ hiển thị các công việc thuộc vùng giai đoạn bị ảnh hưởng. Quan hệ nội bộ của task con được giữ nguyên; các task bị ảnh hưởng sẽ được đưa về “Chưa bắt đầu”.</div>
                        </div>
                        <asp:Literal ID="ltrReorderReviewScope" runat="server"></asp:Literal>
                    </div>

                    <div class="reorder-review-options">
                        <div class="reorder-option">
                            <div class="reorder-option-copy">
                                <div class="reorder-option-title">
                                    <i class="fas fa-calendar-check"></i>
                                    <span>Cập nhật lịch tự động</span>
                                </div>
                                <div class="reorder-option-desc">Tính lại ngày bắt đầu và ngày kết thúc theo phụ thuộc mới.</div>
                            </div>
                            <asp:LinkButton ID="btnToggleAutoUpdateDatesReview" runat="server" CssClass="reorder-switch-button" OnClick="btnToggleAutoUpdateDatesReview_Click" CausesValidation="false">
                                <asp:Literal ID="ltrAutoUpdateSwitchReview" runat="server"></asp:Literal>
                            </asp:LinkButton>
                        </div>
                    </div>

                    <div class="reorder-review-board">
                        <div class="reorder-review-column">
                            <div class="reorder-review-column-title">
                                <strong>Trước khi đổi</strong>
                                <span><asp:Literal ID="ltrBeforeCount" runat="server"></asp:Literal></span>
                            </div>
                            <div class="reorder-review-list reorder-scrollbar">
                                <asp:Literal ID="ltrReorderBefore" runat="server"></asp:Literal>
                            </div>
                        </div>

                        <div class="reorder-review-column">
                            <div class="reorder-review-column-title">
                                <strong>Sau khi đổi</strong>
                                <span><asp:Literal ID="ltrAfterCount" runat="server"></asp:Literal></span>
                            </div>
                            <div class="reorder-review-list reorder-scrollbar">
                                <asp:Literal ID="ltrReorderAfter" runat="server"></asp:Literal>
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>

    <FooterTemplate>
        <asp:LinkButton ID="btnBackFromReview" runat="server" CausesValidation="false" OnClick="btnBackFromReview_Click" Style="display:none !important;"></asp:LinkButton>

        <asp:LinkButton ID="btnConfirmReorderFromReview" runat="server" CssClass="btn btn-primary waves-effect waves-light" CausesValidation="false" OnClientClick="return setReorderLoading(this);" OnClick="btnConfirmReorderFromReview_Click">
            <i class="fas fa-check me-1"></i> Áp dụng thay đổi
        </asp:LinkButton>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    (function () {
        function setupReorderReviewBackButton() {
            var modal = document.getElementById('<%= mdlReorderReview.ClientID %>');
            if (!modal) return;

            var closeButton = modal.querySelector(
                '.modal-header button[data-bs-dismiss="modal"],' +
                '.modal-header a[data-bs-dismiss="modal"],' +
                '.modal-header button[data-dismiss="modal"],' +
                '.modal-header a[data-dismiss="modal"],' +
                '.modal-header button.close,' +
                '.modal-header a.close,' +
                '.modal-header .btn-close'
            );

            if (!closeButton) {
                var header = modal.querySelector('.modal-header');
                if (header) {
                    closeButton = header.querySelector('button, a');
                }
            }

            if (!closeButton || closeButton.getAttribute('data-reorder-back-bound') === '1') {
                return;
            }

            closeButton.setAttribute('data-reorder-back-bound', '1');
            closeButton.setAttribute('aria-label', 'Quay lại');
            closeButton.setAttribute('title', 'Quay lại');
            closeButton.classList.remove('btn-close');
            closeButton.classList.add('reorder-review-back-button');
            closeButton.innerHTML = '<i class="fas fa-arrow-left me-1"></i><span>Quay lại</span>';
            closeButton.style.width = 'auto';
            closeButton.style.height = 'auto';
            closeButton.style.opacity = '1';
            closeButton.style.padding = '6px 10px';
            closeButton.style.border = '0';
            closeButton.style.background = 'transparent';
            closeButton.style.color = '#64748b';
            closeButton.style.fontSize = '13px';
            closeButton.style.fontWeight = '600';
            closeButton.style.boxShadow = 'none';

            closeButton.onclick = function (event) {
                event.preventDefault();
                event.stopImmediatePropagation();
                __doPostBack('<%= btnBackFromReview.UniqueID %>', '');
                return false;
            };
        }

        function removeReorderLoadingOverlay() {
            var overlays = document.querySelectorAll('.reorder-loading-overlay');

            for (var i = 0; i < overlays.length; i++) {
                if (overlays[i] && overlays[i].parentNode) {
                    overlays[i].parentNode.removeChild(overlays[i]);
                }
            }

            var loadingContents = document.querySelectorAll('.reorder-modal-loading');

            for (var j = 0; j < loadingContents.length; j++) {
                loadingContents[j].classList.remove('reorder-modal-loading');
            }

            var loadingButtons = document.querySelectorAll('[data-loading="1"]');

            for (var k = 0; k < loadingButtons.length; k++) {
                loadingButtons[k].removeAttribute('data-loading');
                loadingButtons[k].style.pointerEvents = '';
                loadingButtons[k].style.opacity = '';
            }
        }

        if (typeof Sys !== 'undefined' && Sys.Application) {
            Sys.Application.add_load(setupReorderReviewBackButton);
        } else if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', setupReorderReviewBackButton);
        } else {
            setupReorderReviewBackButton();
        }

        window.removeReorderLoadingOverlay = removeReorderLoadingOverlay;
    })();

    function setReorderLoading(btn) {
        if (!btn) {
            return true;
        }

        if (btn.getAttribute('data-loading') === '1') {
            return false;
        }

        btn.setAttribute('data-loading', '1');
        btn.classList.add('disabled');
        btn.style.pointerEvents = 'none';
        btn.style.opacity = '0.75';

        btn.innerHTML =
            '<span class="reorder-loading-btn">' +
                '<i class="fas fa-spinner fa-spin"></i>' +
                '<span>Đang xử lý...</span>' +
            '</span>';

        var modal = btn.closest('.modal');

        if (modal) {
            var modalContent = modal.querySelector('.modal-content');

            if (modalContent) {
                modalContent.classList.add('reorder-modal-loading');

                var oldOverlay = modalContent.querySelector('.reorder-loading-overlay');

                if (oldOverlay) {
                    oldOverlay.parentNode.removeChild(oldOverlay);
                }

                var overlay = document.createElement('div');
                overlay.className = 'reorder-loading-overlay';

                overlay.innerHTML =
                    '<div class="reorder-loading-box">' +
                        '<div class="reorder-loading-spinner">' +
                            '<i class="fas fa-spinner fa-spin"></i>' +
                        '</div>' +
                        '<div class="reorder-loading-title">Đang áp dụng thay đổi...</div>' +
                        '<div class="reorder-loading-desc">Hệ thống đang cập nhật thứ tự và thời gian công việc.</div>' +
                        '<div class="reorder-loading-progress"></div>' +
                    '</div>';

                modalContent.appendChild(overlay);
            }
        }

        return true;
    }

    if (typeof Sys !== 'undefined' &&
        Sys.WebForms &&
        Sys.WebForms.PageRequestManager) {

        var reorderRequestManager =
            Sys.WebForms.PageRequestManager.getInstance();

        reorderRequestManager.add_endRequest(function () {
            if (typeof window.removeReorderLoadingOverlay === 'function') {
                window.removeReorderLoadingOverlay();
            }

            var modal = document.getElementById('<%= mdlReorderReview.ClientID %>');

            if (modal && typeof Sys !== 'undefined' && Sys.Application) {
                var eventName = 'reorder-review-bind-check';

                if (modal.getAttribute(eventName) !== '1') {
                    modal.setAttribute(eventName, '1');

                    var header = modal.querySelector('.modal-header');

                    if (header) {
                        var closeButton = header.querySelector(
                            'button[data-bs-dismiss="modal"],' +
                            'a[data-bs-dismiss="modal"],' +
                            'button[data-dismiss="modal"],' +
                            'a[data-dismiss="modal"],' +
                            'button.close,' +
                            'a.close,' +
                            'button.btn-close,' +
                            'a.btn-close'
                        );

                        if (closeButton) {
                            closeButton.removeAttribute(eventName);
                        }
                    }
                }
            }
        });
    }

    window.addEventListener('pageshow', function () {
        if (typeof window.removeReorderLoadingOverlay === 'function') {
            window.removeReorderLoadingOverlay();
        }
    });
</script>

<style type="text/css">
    .reorder-review-back-button:hover {
        color: #4f46e5 !important;
        background: #f5f3ff !important;
        border-radius: 7px !important;
    }
</style>