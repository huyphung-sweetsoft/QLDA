
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlApplyTemplate.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlApplyTemplate" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
/* APPLY TEMPLATE - LAYOUT */
.apt-wrapper { display:flex; width:100%; height:min(65vh,calc(100vh - 250px)); max-height:calc(100vh - 250px); min-height:min(500px,calc(100vh - 300px)); overflow:hidden; box-sizing:border-box; flex-shrink:1; }
.apt-wrapper * { box-sizing:border-box; }
.apt-list-panel { width:100%; height:100%; min-width:0; min-height:0; flex:0 0 100%; padding-right:5px; overflow-x:hidden; overflow-y:hidden; transition:width .45s cubic-bezier(.2,.8,.2,1),flex-basis .45s cubic-bezier(.2,.8,.2,1),padding .3s ease; }
.apt-list-panel.is-split { width:22%; flex:0 0 22%; padding-right:14px; border-right:1px solid #e2e8f0; }
.apt-detail-panel { width:0; height:100%; min-width:0; min-height:0; flex:0 0 0; opacity:0; transform:translateX(-18px); padding-left:0; overflow:hidden; background:#f8fafc; border-radius:0 8px 8px 0; transition:width .45s cubic-bezier(.2,.8,.2,1),flex-basis .45s cubic-bezier(.2,.8,.2,1),opacity .3s ease,transform .4s cubic-bezier(.2,.8,.2,1),padding .3s ease; }
.apt-detail-panel.is-split { width:78%; flex:0 0 78%; opacity:1; transform:translateX(0); padding-left:14px; }

/* SCROLLBAR */
.apt-list-panel, .apt-detail-panel { overflow-x:hidden !important; overflow-y:hidden !important; overscroll-behavior:contain; scrollbar-gutter:auto; }
.apt-list-panel.apt-has-overflow, .apt-detail-panel.is-split.apt-has-overflow { overflow-y:auto !important; scrollbar-width:thin; }
.apt-detail-panel:not(.is-split) { overflow:hidden !important; }
.apt-list-panel::-webkit-scrollbar, .apt-detail-panel::-webkit-scrollbar { width:6px; height:6px; }
.apt-list-panel::-webkit-scrollbar-thumb, .apt-detail-panel::-webkit-scrollbar-thumb { background-color:#cbd5e1; border-radius:4px; }

/* DETAIL HEADER */
.apt-detail-header { display:flex; justify-content:space-between; align-items:center; gap:12px; margin-bottom:12px; padding:5px 0 9px; border-bottom:1px dashed #cbd5e1; position:sticky; top:0; background:#f8fafc; z-index:10; }
.apt-detail-title { min-width:0; flex:1 1 auto; font-size:16px; font-weight:700; color:#1e40af; overflow-wrap:anywhere; }
.apt-detail-title-link { display:inline-flex; align-items:center; gap:7px; max-width:100%; padding:4px 8px 4px 4px; border:1px solid transparent; border-radius:8px; color:#1e40af !important; cursor:pointer; text-decoration:none !important; line-height:1.4; transition:color .18s ease,background .18s ease,border-color .18s ease,box-shadow .18s ease; }
.apt-detail-title-link:hover { color:#1d4ed8 !important; background:#eff6ff; border-color:#bfdbfe; text-decoration:none !important; box-shadow:0 2px 6px rgba(37,99,235,.08); }
.apt-detail-title-link:focus-visible { outline:3px solid rgba(59,130,246,.45); outline-offset:2px; }
.apt-detail-title-link i { flex:0 0 auto; }
.apt-detail-title-link span { min-width:0; overflow-wrap:anywhere; }
.apt-detail-header-actions { display:inline-flex; align-items:center; justify-content:flex-end; gap:8px; flex:0 0 auto; }

/* BUTTONS */
.btn-apt-apply { display:inline-flex; align-items:center; justify-content:center; gap:7px; min-height:34px; padding:7px 12px; border:1px solid #5b21b6; border-radius:8px; background:linear-gradient(135deg,#6d28d9 0%,#4c1d95 100%); color:#fff !important; font-size:12px; font-weight:700; line-height:1.2; white-space:nowrap; text-decoration:none !important; box-shadow:0 3px 8px rgba(91,33,182,.18); transition:transform .18s ease,box-shadow .18s ease,background .18s ease; }
.btn-apt-apply:hover { color:#fff !important; background:linear-gradient(135deg,#5b21b6 0%,#3b0764 100%); transform:translateY(-1px); box-shadow:0 5px 12px rgba(91,33,182,.24); }
.btn-apt-apply:active { transform:translateY(0); }
.btn-apt-collapse { flex:0 0 auto; display:inline-flex; align-items:center; justify-content:center; gap:7px; min-height:34px; padding:7px 11px; cursor:pointer; color:#475569; background:#fff; border:1px solid #cbd5e1; border-radius:8px; font-size:12px; font-weight:700; line-height:1.2; white-space:nowrap; text-decoration:none !important; transition:color .2s ease,background .2s ease,border-color .2s ease,box-shadow .2s ease; }
.btn-apt-collapse i { font-size:12px; }
.btn-apt-collapse:hover { color:#4c1d95 !important; background:#f5f3ff; border-color:#c4b5fd; box-shadow:0 2px 6px rgba(91,33,182,.1); }

/* TEMPLATE LIST */
.template-item-name-link { display:block; width:100%; border-radius:5px; text-decoration:none !important; }
.template-item-name { display:inline-block; font-size:14px; font-weight:700; color:#5b21b6; margin-bottom:3px; overflow-wrap:anywhere; transition:color .16s ease; }
.template-item-name-link:hover .template-item-name { color:#2563eb; }
.template-item-name-link:focus-visible { outline:2px solid #93c5fd; outline-offset:2px; }
.template-item-desc { font-size:12px; color:#64748b; line-height:1.45; display:-webkit-box; -webkit-line-clamp:2; -webkit-box-orient:vertical; overflow:hidden; overflow-wrap:anywhere; }

/* GRIDVIEW */
.apt-list-panel .table, .apt-detail-panel .table { width:100% !important; max-width:100%; margin-bottom:0; }
.apt-list-panel .table-responsive, .apt-detail-panel .table-responsive { max-height:none !important; overflow-x:auto !important; overflow-y:hidden !important; }
.apt-list-panel .table th, .apt-list-panel .table td, .apt-detail-panel .table th, .apt-detail-panel .table td { vertical-align:middle; }
.apt-list-panel .table thead tr > th:first-child, .apt-list-panel .table tbody tr > td:first-child, .apt-detail-panel .table thead tr > th:first-child, .apt-detail-panel .table tbody tr > td:first-child { display:none !important; }
.apt-list-panel.is-split .table thead tr > th:last-child, .apt-list-panel.is-split .table tbody tr > td:last-child { display:none !important; }
.apt-detail-panel .table { table-layout:fixed; }
.apt-detail-panel .table th:nth-child(2), .apt-detail-panel .table td:nth-child(2) { width:auto; min-width:0; }
.apt-detail-panel .table th:nth-child(3), .apt-detail-panel .table td:nth-child(3) { width:92px; white-space:nowrap; text-align:center; }
.apt-detail-panel .table th:nth-child(4), .apt-detail-panel .table td:nth-child(4) { width:112px; white-space:nowrap; text-align:center; }
.apt-task-name { display:block; min-width:0; overflow-wrap:anywhere; line-height:1.5; }
.apt-task-phase-icon { display:inline-block; color:#7c3aed; font-size:12px; margin-right:5px; }
.apt-task-code { color:#6d28d9; font-weight:800; white-space:nowrap; margin-right:5px; }
.apt-task-title { color:#334155; font-size:13px; font-weight:500; }
.apt-task-duration { display:inline-block; color:#475569; font-size:12px; font-weight:700; }
.apt-dependency-code { display:inline-flex; align-items:center; justify-content:center; min-width:34px; padding:3px 8px; border-radius:6px; background:#eff6ff; border:1px solid #bfdbfe; color:#1d4ed8; font-size:12px; font-weight:700; cursor:help; }
.apt-dependency-empty { color:#94a3b8; font-weight:600; }

/* TEMPLATE ACTIONS */
.apt-template-actions { display:flex; align-items:center; justify-content:center; gap:7px; white-space:nowrap; }
.apt-template-action { display:inline-flex; align-items:center; justify-content:center; gap:6px; height:35px; border-radius:8px; font-size:12px; font-weight:700; line-height:1; text-decoration:none !important; white-space:nowrap; transition:background-color .17s ease,border-color .17s ease,color .17s ease,box-shadow .17s ease,transform .17s ease; }
.apt-template-action i { font-size:13px; }
.apt-template-view { width:36px; flex:0 0 36px; border:1px solid #d9c4f3; color:#6330a1 !important; background:#f9f6ff; }
.apt-template-view:hover { background:#efe5ff; border-color:#b990e8; color:#4c1d95 !important; box-shadow:0 3px 9px rgba(91,33,182,.12); transform:translateY(-1px); }
.apt-template-use { min-width:94px; padding:0 11px; border:1px solid #54218d; color:#fff !important; background:#54218d; box-shadow:0 2px 6px rgba(76,29,149,.12); }
.apt-template-use:hover { background:#432078; border-color:#432078; color:#fff !important; box-shadow:0 4px 11px rgba(76,29,149,.22); transform:translateY(-1px); }
.apt-template-action:focus-visible { outline:3px solid rgba(124,58,237,.35); outline-offset:2px; }

/* MODAL - GIỮ CHIỀU RỘNG KHI ĐÓNG */
.modal:has(.apt-wrapper) .modal-dialog { transition:none !important; }
.modal:has(.apt-wrapper) .modal-dialog.modal-template-expanded, .modal:has(.apt-wrapper) .modal-dialog:has(.apt-detail-panel.is-split), .modal.apt-keep-wide .modal-dialog { width:78vw !important; max-width:78vw !important; min-width:0 !important; }
.modal.apt-is-closing.apt-keep-wide .apt-list-panel { width:22% !important; flex:0 0 22% !important; padding-right:14px; transition:none !important; }
.modal.apt-is-closing.apt-keep-wide .apt-detail-panel { width:78% !important; flex:0 0 78% !important; opacity:1 !important; transform:none !important; padding-left:14px; transition:none !important; }

/* RESPONSIVE */
@supports (height:100dvh) {
    .apt-wrapper { height:min(65dvh,calc(100dvh - 250px)); max-height:calc(100dvh - 250px); min-height:min(500px,calc(100dvh - 300px)); }
}
@media (max-width:991.98px) {
    .apt-list-panel.is-split { width:30%; flex-basis:30%; padding-right:9px; }
    .apt-detail-panel.is-split { width:70%; flex-basis:70%; padding-left:9px; }
    .modal:has(.apt-wrapper) .modal-dialog.modal-template-expanded, .modal:has(.apt-wrapper) .modal-dialog:has(.apt-detail-panel.is-split), .modal.apt-keep-wide .modal-dialog { width:92vw !important; max-width:92vw !important; }
    .modal.apt-is-closing.apt-keep-wide .apt-list-panel { width:30% !important; flex-basis:30% !important; }
    .modal.apt-is-closing.apt-keep-wide .apt-detail-panel { width:70% !important; flex-basis:70% !important; }
}
@media (max-width:575.98px) {
    .apt-wrapper { height:min(72vh,calc(100vh - 220px)); max-height:calc(100vh - 220px); min-height:min(360px,calc(100vh - 270px)); }
    .apt-wrapper:has(.apt-detail-panel.is-split) { flex-direction:column; }
    .apt-list-panel.is-split { width:100%; height:30%; flex:0 0 30%; border-right:0; border-bottom:1px solid #e2e8f0; padding:0 0 9px; }
    .apt-detail-panel.is-split { width:100%; height:70%; flex:0 0 70%; padding:9px 0 0; border-radius:0; }
    .apt-detail-title { font-size:14px; }
    .apt-detail-header-actions { gap:5px; }
    .btn-apt-apply { min-height:31px; padding:6px 8px; font-size:11px; }
    .btn-apt-collapse { min-height:31px; padding:6px 8px; gap:5px; font-size:11px; }
    .modal:has(.apt-wrapper) .modal-dialog.modal-template-expanded, .modal:has(.apt-wrapper) .modal-dialog:has(.apt-detail-panel.is-split), .modal.apt-keep-wide .modal-dialog { width:calc(100vw - 16px) !important; max-width:calc(100vw - 16px) !important; }
    .modal.apt-is-closing.apt-keep-wide .apt-wrapper { flex-direction:column; }
    .modal.apt-is-closing.apt-keep-wide .apt-list-panel { width:100% !important; flex:0 0 30% !important; padding:0 0 9px; }
    .modal.apt-is-closing.apt-keep-wide .apt-detail-panel { width:100% !important; flex:0 0 70% !important; padding:9px 0 0; }
}
@supports (height:100dvh) {
    @media (max-width:575.98px) {
        .apt-wrapper { height:min(72dvh,calc(100dvh - 220px)); max-height:calc(100dvh - 220px); min-height:min(360px,calc(100dvh - 270px)); }
    }
}
@media (prefers-reduced-motion:reduce) {
    .apt-list-panel, .apt-detail-panel, .btn-apt-collapse, .apt-template-action { transition:none !important; }
}

/* Khóa kích thước dialog ở lớp ngoài UpdatePanel; không phụ thuộc .is-split khi đóng. */
.modal.apt-width-frozen .modal-dialog { width:var(--apt-frozen-width)!important; min-width:var(--apt-frozen-width)!important; max-width:var(--apt-frozen-width)!important; transition:none!important; }

/* Bootstrap/ExtraModal có thể áp hiệu ứng scale/translate lúc đóng: chỉ cho modal fade, không biến dạng khung. */
.modal.apt-is-closing .modal-dialog, .modal.apt-is-closing.fade .modal-dialog, .modal.apt-is-closing .modal-content { transform:none!important; animation:none!important; transition:none!important; }
.modal.apt-is-closing.apt-keep-wide .apt-wrapper { transition:none!important; }
</style>

<SweetSoft:ExtraModal DefaultButton="btnCloseApply" ID="mdlApplyTemplate" Type="Primary" runat="server">
    <ContentTemplate>
        <asp:UpdatePanel ID="upnlApplyTemplate" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="p-3">
                    <div class="apt-wrapper">
                        <div id="pnlList" runat="server" class="apt-list-panel">
                            <div class="d-flex justify-content-between align-items-center mb-3">
                                <h6 class="text-primary fw-bold m-0"><i class="fas fa-layer-group me-2"></i>Chọn mẫu công việc</h6>
                            </div>
                            <SweetSoft:GridviewExtension ID="grvTemplates" runat="server" ShowHeader="true" AutoGenerateColumns="false" AllowSorting="false" IsEnableIndex="false" IsEnableSelectColumn="false" CssClass="table table-bordered table-hover align-middle w-100" DataKeyNames="IdMau" OnRowCommand="grvTemplates_RowCommand" OnNeedDataSource="grvTemplates_NeedDataSource">
                                <Columns>
                                    <asp:TemplateField HeaderText="Tên mẫu công việc">
                                        <ItemTemplate>
                                            <asp:LinkButton runat="server" CommandName="VIEW_DETAIL" CommandArgument='<%# Eval("IdMau") %>' CssClass="template-item-name-link" ToolTip="Nhấn để xem chi tiết mẫu">
                                                <span class="template-item-name"><%# Eval("TenMau") %></span>
                                            </asp:LinkButton>
                                            <div class="template-item-desc"><%# Eval("MoTa") %></div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Hành động" HeaderStyle-Width="164px" ItemStyle-Width="164px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <div class="apt-template-actions">
                                                <asp:LinkButton runat="server" CommandName="VIEW_DETAIL" CommandArgument='<%# Eval("IdMau") %>' CssClass="apt-template-action apt-template-view" ToolTip="Xem chi tiết mẫu" aria-label="Xem chi tiết mẫu"><i class="fas fa-eye" aria-hidden="true"></i></asp:LinkButton>
                                                <asp:LinkButton runat="server" CommandName="APPLY_TEMPLATE" CommandArgument='<%# Eval("IdMau") %>' CssClass="apt-template-action apt-template-use" ToolTip="Áp dụng mẫu này" aria-label="Áp dụng mẫu"><i class="fas fa-check" aria-hidden="true"></i><span>Áp dụng</span></asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </SweetSoft:GridviewExtension>
                        </div>

                        <div id="pnlDetail" runat="server" class="apt-detail-panel">
                            <div class="apt-detail-header">
                                <a class="apt-detail-title apt-detail-title-link" href="<%= SelectedTemplateDetailUrl %>" title="Mở trang chi tiết mẫu công việc">
                                    <i class="fas fa-sitemap"></i><span><asp:Literal runat="server" ID="ltrDetailName"></asp:Literal></span>
                                </a>
                                <div class="apt-detail-header-actions">
                                    <asp:LinkButton runat="server" ID="lbtApplySelectedTemplate" OnClick="lbtApplySelectedTemplate_Click" CssClass="btn-apt-apply" ToolTip="Sử dụng mẫu công việc này"><i class="fas fa-check"></i> Sử dụng mẫu</asp:LinkButton>
                                    <asp:LinkButton runat="server" ID="lbtCloseDetail" OnClick="lbtCloseDetail_Click" CssClass="btn-apt-collapse" ToolTip="Thu gọn khung chi tiết"><i class="fas fa-chevron-left"></i><span>Thu gọn</span></asp:LinkButton>
                                </div>
                            </div>
                            <SweetSoft:GridviewExtension ID="grvTemplateDetails" runat="server" ShowHeader="true" AutoGenerateColumns="false" AllowSorting="false" IsEnableIndex="false" IsEnableSelectColumn="false" CssClass="table table-bordered table-hover align-middle w-100 mb-0">
                                <Columns>
                                    <asp:TemplateField HeaderText="Tên công việc">
                                        <ItemTemplate>
                                            <div class="apt-task-name" style='<%# "padding-left:" + GetTaskIndent(Eval("MaCongViec")) + "px;" %>'>
                                                <%# (!string.IsNullOrWhiteSpace(Convert.ToString(Eval("MaCongViec"))) && !Convert.ToString(Eval("MaCongViec")).Contains(".")) ? "<i class='fas fa-flag apt-task-phase-icon'></i>" : "" %>
                                                <span class="apt-task-code"><%# Eval("MaCongViec") %>.</span>
                                                <span class="apt-task-title"><%# Eval("TenCongViec") %></span>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Thời hạn" HeaderStyle-Width="92px" ItemStyle-Width="92px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <span class="apt-task-duration"><%# Eval("ThoiHanNgay") != DBNull.Value && Eval("ThoiHanNgay") != null ? Eval("ThoiHanNgay") + " ngày" : "—" %></span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Phụ thuộc" HeaderStyle-Width="112px" ItemStyle-Width="112px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <span class='<%# Convert.ToString(Eval("PhuThuocMaCongViec")) == "—" ? "apt-dependency-empty" : "apt-dependency-code" %>' title='<%# Server.HtmlEncode(Convert.ToString(Eval("PhuThuocTenCongViec"))) %>'><%# Eval("PhuThuocMaCongViec") %></span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </SweetSoft:GridviewExtension>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    (function () {
        'use strict';

        function aptGetModals() {
            var wrappers = document.querySelectorAll('.apt-wrapper');
            var result = [];
            for (var i = 0; i < wrappers.length; i++) {
                var modal = wrappers[i].closest('.modal');
                if (modal && result.indexOf(modal) === -1) result.push(modal);
            }
            return result;
        }

        function aptMeasureScrollbars() {
            var wrappers = document.querySelectorAll('.apt-wrapper');

            for (var i = 0; i < wrappers.length; i++) {
                var panels = wrappers[i].querySelectorAll('.apt-list-panel, .apt-detail-panel');

                for (var j = 0; j < panels.length; j++) {
                    var panel = panels[j];
                    panel.classList.remove('apt-has-overflow');

                    if ((panel.classList.contains('apt-detail-panel') && !panel.classList.contains('is-split')) ||
                        panel.clientHeight < 10 || panel.clientWidth < 10 || !panel.getClientRects().length) {
                        continue;
                    }

                    var bounds = panel.getBoundingClientRect();
                    var rows = panel.querySelectorAll('tbody tr');
                    var lastRow = rows.length ? rows[rows.length - 1] : null;
                    var required = panel.scrollHeight;

                    if (lastRow && lastRow.getClientRects().length) {
                        required = Math.ceil(lastRow.getBoundingClientRect().bottom - bounds.top + panel.scrollTop + 8);
                        var paging = panel.querySelector('.pagination, [id*="Paging"]');

                        if (paging && paging.getClientRects().length) {
                            required = Math.max(required, Math.ceil(paging.getBoundingClientRect().bottom - bounds.top + panel.scrollTop + 8));
                        }
                    }

                    if (required > panel.clientHeight + 8) {
                        panel.classList.add('apt-has-overflow');
                    }
                }
            }
        }

        var scheduled = false;

        function aptScheduleMeasure() {
            if (scheduled) return;
            scheduled = true;

            window.requestAnimationFrame(function () {
                scheduled = false;
                aptMeasureScrollbars();
            });
        }

        window.refreshApplyTemplateScrollbars = function () {
            aptScheduleMeasure();
            window.setTimeout(aptScheduleMeasure, 120);
            window.setTimeout(aptScheduleMeasure, 500);
        };

        // Modal wrapper va .modal-dialog nam ben NGOAI UpdatePanel; bien CSS duoc luu o day.
        function aptDialog(modal) {
            return modal ? modal.querySelector('.modal-dialog') : null;
        }

        function aptHasDetail(modal) {
            return !!(modal && modal.querySelector('.apt-detail-panel.is-split'));
        }

        function aptIsHidden(modal) {
            return !modal || window.getComputedStyle(modal).display === 'none';
        }

        // Khi xem chi tiet: latch chieu rong THUC TE cua popup (78vw / 92vw tren mobile).
        // Khong giam chieu rong khi server render lai cac div .apt-*.
        function aptFreezeWidth(modal) {
            if (!modal || (!aptHasDetail(modal) && !modal.classList.contains('apt-keep-wide'))) return;

            var dialog = aptDialog(modal);
            if (!dialog) return;

            if (!modal.classList.contains('apt-keep-wide')) {
                modal.classList.add('apt-keep-wide');
            }

            if (modal.classList.contains('apt-width-frozen')) return;

            var width = dialog.getBoundingClientRect().width;
            if (!(width > 0)) return;

            dialog.style.setProperty('--apt-frozen-width', width + 'px');
            modal.classList.add('apt-width-frozen');
        }

        function aptReleaseWidth(modal) {
            if (!modal) return;

            var dialog = aptDialog(modal);

            if (modal.classList.contains('apt-width-frozen') ||
                modal.classList.contains('apt-keep-wide') ||
                modal.classList.contains('apt-is-closing')) {
                modal.classList.remove(
                    'apt-width-frozen',
                    'apt-keep-wide',
                    'apt-is-closing'
                );
            }

            if (dialog && dialog.style.getPropertyValue('--apt-frozen-width')) {
                dialog.style.removeProperty('--apt-frozen-width');
            }

            if (modal.dataset.aptCollapsePending) {
                delete modal.dataset.aptCollapsePending;
            }
        }

        function aptBeginClose(modal) {
            if (!modal || modal.classList.contains('apt-is-closing')) return;

            aptFreezeWidth(modal);

            // Ngan hieu ung scale-down trong luc fade-out.
            modal.classList.add('apt-is-closing');
        }

        function aptOnShow(modal) {
            if (!modal) return;
            if (aptIsHidden(modal)) return;

            if (modal.classList.contains('apt-is-closing')) {
                modal.classList.remove('apt-is-closing');
            }

            if (aptHasDetail(modal)) {
                aptFreezeWidth(modal);
            } else if (!modal.classList.contains('apt-is-closing')) {
                aptReleaseWidth(modal);
            }
        }

        function aptWatchModal(modal) {
            if (!modal || modal.__aptCloseObserver || !window.MutationObserver) return;

            var observer = new MutationObserver(function () {
                // Chi reset sau khi modal DA AN.
                if (aptIsHidden(modal)) {
                    aptReleaseWidth(modal);
                } else if (aptHasDetail(modal) &&
                    !modal.classList.contains('apt-is-closing')) {
                    aptFreezeWidth(modal);
                }
            });

            observer.observe(modal, {
                attributes: true,
                attributeFilter: ['class', 'style'],
                childList: true,
                subtree: true
            });

            modal.__aptCloseObserver = observer;
        }

        function aptUpdateModalsAfterPostback() {
            aptGetModals().forEach(function (modal) {
                aptWatchModal(modal);

                if (aptIsHidden(modal)) {
                    aptReleaseWidth(modal);
                    return;
                }

                if (modal.dataset.aptCollapsePending === '1') {
                    delete modal.dataset.aptCollapsePending;

                    if (!modal.classList.contains('apt-is-closing') &&
                        !aptHasDetail(modal)) {
                        // Thu gon duoc phep quay ve kich thuoc nho.
                        aptReleaseWidth(modal);
                    }
                    return;
                }

                if (!modal.classList.contains('apt-is-closing')) {
                    aptFreezeWidth(modal);
                }
            });

            window.refreshApplyTemplateScrollbars();
        }

        function aptInit() {
            aptGetModals().forEach(function (modal) {
                aptWatchModal(modal);

                if (!aptIsHidden(modal)) {
                    aptFreezeWidth(modal);
                }
            });

            window.refreshApplyTemplateScrollbars();
        }

        // Dang ky su kien mot lan, tranh trung sau UpdatePanel.
        if (!window.__aptCloseWidthFix20261024) {
            window.__aptCloseWidthFix20261024 = true;

            document.addEventListener('click', function (evt) {
                var target = evt.target;
                if (!target || !target.closest) return;

                var button = target.closest(
                    'button, a, input[type="button"], input[type="submit"]'
                );

                if (!button) return;

                var modal = button.closest('.modal');
                if (!modal || !modal.querySelector('.apt-wrapper')) return;

                // Thu gon khong phai dong popup.
                if (button.id && /lbtCloseDetail/.test(button.id)) {
                    modal.dataset.aptCollapsePending = '1';
                    return;
                }

                var footer = button.closest('.modal-footer');
                var header = button.closest('.modal-header');
                var label = (button.textContent || button.value || '').trim();

                var dismiss =
                    button.hasAttribute('data-bs-dismiss') ||
                    button.hasAttribute('data-dismiss');

                var closeIcon = button.matches(
                    '.btn-close, .close, [aria-label="Close"], [aria-label="Đóng"]'
                );

                if (dismiss ||
                    closeIcon ||
                    (footer && /đóng|close/i.test(label)) ||
                    (header && /^(?:×|✕|x)$/i.test(label))) {
                    aptBeginClose(modal);
                }
            }, true);

            // Bootstrap 5.
            document.addEventListener('hide.bs.modal', function (evt) {
                if (evt.target && evt.target.querySelector('.apt-wrapper')) {
                    aptBeginClose(evt.target);
                }
            }, true);

            document.addEventListener('show.bs.modal', function (evt) {
                if (evt.target && evt.target.querySelector('.apt-wrapper')) {
                    aptOnShow(evt.target);
                }
            }, true);

            document.addEventListener('hidden.bs.modal', function (evt) {
                if (evt.target && evt.target.querySelector('.apt-wrapper')) {
                    aptReleaseWidth(evt.target);
                }
            }, true);

            // Bootstrap 4 / jQuery.
            if (window.jQuery &&
                window.jQuery.fn &&
                window.jQuery.fn.modal) {

                window.jQuery(document)
                    .on('hide.bs.modal.aptWidthFix', '.modal', function () {
                        if (this.querySelector('.apt-wrapper')) {
                            aptBeginClose(this);
                        }
                    })
                    .on('show.bs.modal.aptWidthFix', '.modal', function () {
                        if (this.querySelector('.apt-wrapper')) {
                            aptOnShow(this);
                        }
                    })
                    .on('hidden.bs.modal.aptWidthFix', '.modal', function () {
                        if (this.querySelector('.apt-wrapper')) {
                            aptReleaseWidth(this);
                        }
                    });
            }

            window.addEventListener('resize', function () {
                aptGetModals().forEach(function (modal) {
                    if (aptIsHidden(modal) ||
                        modal.classList.contains('apt-is-closing')) {
                        return;
                    }

                    var dialog = aptDialog(modal);

                    if (modal.classList.contains('apt-width-frozen')) {
                        modal.classList.remove('apt-width-frozen');

                        if (dialog) {
                            dialog.style.removeProperty('--apt-frozen-width');
                        }
                    }

                    aptFreezeWidth(modal);
                });

                aptScheduleMeasure();
            });

            // ASP.NET WebForms UpdatePanel.
            if (window.Sys &&
                Sys.WebForms &&
                Sys.WebForms.PageRequestManager) {

                var prm = Sys.WebForms.PageRequestManager.getInstance();

                prm.add_beginRequest(function () {
                    aptGetModals().forEach(function (modal) {
                        if (!aptIsHidden(modal)) {
                            aptFreezeWidth(modal);
                        }
                    });
                });

                prm.add_endRequest(aptUpdateModalsAfterPostback);
            }
        }

        if (document.readyState === 'loading') {
            document.addEventListener(
                'DOMContentLoaded',
                aptInit,
                { once: true }
            );
        } else {
            aptInit();
        }
    })();
</script>
