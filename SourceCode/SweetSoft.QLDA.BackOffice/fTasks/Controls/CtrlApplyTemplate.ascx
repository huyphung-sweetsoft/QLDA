<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlApplyTemplate.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlApplyTemplate" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
.apt-wrapper { display: flex; width: 100%; height: min(65vh, calc(100vh - 250px)); max-height: calc(100vh - 250px); min-height: min(500px, calc(100vh - 300px)); overflow: hidden; box-sizing: border-box; flex-shrink: 1; }
.apt-wrapper * { box-sizing: border-box; }
.apt-list-panel { width: 100%; height: 100%; min-width: 0; flex: 0 0 100%; transition: width .45s cubic-bezier(.2,.8,.2,1), flex-basis .45s cubic-bezier(.2,.8,.2,1), padding .3s ease; overflow-x: hidden; overflow-y: hidden; padding-right: 5px; }
.apt-list-panel.is-split { width: 22%; flex: 0 0 22%; border-right: 1px solid #e2e8f0; padding-right: 14px; }
.apt-detail-panel { width: 0; height: 100%; min-width: 0; flex: 0 0 0; opacity: 0; transform: translateX(-18px); transition: width .45s cubic-bezier(.2,.8,.2,1), flex-basis .45s cubic-bezier(.2,.8,.2,1), opacity .3s ease, transform .4s cubic-bezier(.2,.8,.2,1), padding .3s ease; overflow-x: hidden; overflow-y: hidden; background: #f8fafc; border-radius: 0 8px 8px 0; }
.apt-list-panel.apt-has-overflow, .apt-detail-panel.apt-has-overflow { overflow-y: auto !important; }
.apt-detail-panel.is-split { width: 78%; flex: 0 0 78%; opacity: 1; transform: translateX(0); padding-left: 14px; }
.apt-list-panel::-webkit-scrollbar, .apt-detail-panel::-webkit-scrollbar { width: 6px; height: 6px; }
.apt-list-panel::-webkit-scrollbar-thumb, .apt-detail-panel::-webkit-scrollbar-thumb { background-color: #cbd5e1; border-radius: 4px; }
.apt-detail-header { display: flex; justify-content: space-between; align-items: center; gap: 12px; margin-bottom: 12px; padding: 5px 0 9px; border-bottom: 1px dashed #cbd5e1; position: sticky; top: 0; background: #f8fafc; z-index: 10; }
.apt-detail-title { min-width: 0; flex: 1 1 auto; font-size: 16px; font-weight: 700; color: #1e40af; overflow-wrap: anywhere; }
.apt-detail-title-link { display: inline-flex; align-items: center; gap: 7px; max-width: 100%; padding: 4px 8px 4px 4px; border: 1px solid transparent; border-radius: 8px; color: #1e40af !important; cursor: pointer; text-decoration: none !important; line-height: 1.4; transition: color .18s ease, background .18s ease, border-color .18s ease, box-shadow .18s ease; }
.apt-detail-title-link:hover { color: #1d4ed8 !important; background: #eff6ff; border-color: #bfdbfe; text-decoration: none !important; box-shadow: 0 2px 6px rgba(37, 99, 235, .08); }
.apt-detail-title-link:focus-visible { outline: 3px solid rgba(59, 130, 246, .45); outline-offset: 2px; }
.apt-detail-title-link i { flex: 0 0 auto; }
.apt-detail-title-link span { min-width: 0; overflow-wrap: anywhere; }
.apt-detail-header-actions { display: inline-flex; align-items: center; justify-content: flex-end; gap: 8px; flex: 0 0 auto; }
.btn-apt-apply { display: inline-flex; align-items: center; justify-content: center; gap: 7px; min-height: 34px; padding: 7px 12px; border: 1px solid #5b21b6; border-radius: 8px; background: linear-gradient(135deg, #6d28d9 0%, #4c1d95 100%); color: #fff !important; font-size: 12px; font-weight: 700; line-height: 1.2; white-space: nowrap; text-decoration: none !important; box-shadow: 0 3px 8px rgba(91, 33, 182, .18); transition: transform .18s ease, box-shadow .18s ease, background .18s ease; }
.btn-apt-apply:hover { color: #fff !important; background: linear-gradient(135deg, #5b21b6 0%, #3b0764 100%); transform: translateY(-1px); box-shadow: 0 5px 12px rgba(91, 33, 182, .24); }
.btn-apt-apply:active { transform: translateY(0); }
.btn-apt-collapse { flex: 0 0 auto; display: inline-flex; align-items: center; justify-content: center; gap: 7px; min-height: 34px; padding: 7px 11px; cursor: pointer; color: #475569; background: #fff; border: 1px solid #cbd5e1; border-radius: 8px; font-size: 12px; font-weight: 700; line-height: 1.2; white-space: nowrap; text-decoration: none !important; transition: color .2s ease, background .2s ease, border-color .2s ease, box-shadow .2s ease; }
.btn-apt-collapse i { font-size: 12px; }
.btn-apt-collapse:hover { color: #4c1d95 !important; background: #f5f3ff; border-color: #c4b5fd; box-shadow: 0 2px 6px rgba(91, 33, 182, .1); }
.template-item-name-link { display: block; width: 100%; border-radius: 5px; text-decoration: none !important; }
.template-item-name { display: inline-block; font-size: 14px; font-weight: 700; color: #5b21b6; margin-bottom: 3px; overflow-wrap: anywhere; transition: color .16s ease; }
.template-item-name-link:hover .template-item-name { color: #2563eb; }
.template-item-name-link:focus-visible { outline: 2px solid #93c5fd; outline-offset: 2px; }
.template-item-desc { font-size: 12px; color: #64748b; line-height: 1.45; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; overflow-wrap: anywhere; }
.apt-list-panel .table, .apt-detail-panel .table { width: 100% !important; max-width: 100%; margin-bottom: 0; }
.apt-list-panel .table-responsive, .apt-detail-panel .table-responsive { max-height: none !important; overflow-x: auto !important; overflow-y: hidden !important; }
.apt-list-panel .table th, .apt-list-panel .table td, .apt-detail-panel .table th, .apt-detail-panel .table td { vertical-align: middle; }
.apt-list-panel .table thead tr > th:first-child, .apt-list-panel .table tbody tr > td:first-child, .apt-detail-panel .table thead tr > th:first-child, .apt-detail-panel .table tbody tr > td:first-child { display: none !important; }
.apt-list-panel.is-split .table thead tr > th:last-child, .apt-list-panel.is-split .table tbody tr > td:last-child { display: none !important; }
.apt-detail-panel .table { table-layout: fixed; }
.apt-detail-panel .table th:nth-child(2), .apt-detail-panel .table td:nth-child(2) { width: auto; min-width: 0; }
.apt-detail-panel .table th:nth-child(3), .apt-detail-panel .table td:nth-child(3) { width: 92px; white-space: nowrap; text-align: center; }
.apt-detail-panel .table th:nth-child(4), .apt-detail-panel .table td:nth-child(4) { width: 112px; white-space: nowrap; text-align: center; }
.apt-task-name { display: block; min-width: 0; overflow-wrap: anywhere; line-height: 1.5; }
.apt-task-phase-icon { display: inline-block; color: #7c3aed; font-size: 12px; margin-right: 5px; }
.apt-task-code { color: #6d28d9; font-weight: 800; white-space: nowrap; margin-right: 5px; }
.apt-task-title { color: #334155; font-size: 13px; font-weight: 500; }
.apt-task-duration { display: inline-block; color: #475569; font-size: 12px; font-weight: 700; }
.apt-dependency-code { display: inline-flex; align-items: center; justify-content: center; min-width: 34px; padding: 3px 8px; border-radius: 6px; background: #eff6ff; border: 1px solid #bfdbfe; color: #1d4ed8; font-size: 12px; font-weight: 700; cursor: help; }
.apt-dependency-empty { color: #94a3b8; font-weight: 600; }
@supports (height: 100dvh) { .apt-wrapper { height: min(65dvh, calc(100dvh - 250px)); max-height: calc(100dvh - 250px); min-height: min(500px, calc(100dvh - 300px)); } }
@media (max-width: 991.98px) { .apt-list-panel.is-split { width: 30%; flex-basis: 30%; padding-right: 9px; } .apt-detail-panel.is-split { width: 70%; flex-basis: 70%; padding-left: 9px; } }
@media (max-width: 575.98px) { .apt-wrapper { height: min(72vh, calc(100vh - 220px)); max-height: calc(100vh - 220px); min-height: min(360px, calc(100vh - 270px)); } .apt-wrapper:has(.apt-detail-panel.is-split) { flex-direction: column; } .apt-list-panel.is-split { width: 100%; height: 30%; flex: 0 0 30%; border-right: 0; border-bottom: 1px solid #e2e8f0; padding: 0 0 9px; } .apt-detail-panel.is-split { width: 100%; height: 70%; flex: 0 0 70%; padding: 9px 0 0; border-radius: 0; } .apt-detail-title { font-size: 14px; } .apt-detail-header-actions { gap: 5px; } .btn-apt-apply { min-height: 31px; padding: 6px 8px; font-size: 11px; } .btn-apt-collapse { min-height: 31px; padding: 6px 8px; gap: 5px; font-size: 11px; } }
@supports (height: 100dvh) { @media (max-width: 575.98px) { .apt-wrapper { height: min(72dvh, calc(100dvh - 220px)); max-height: calc(100dvh - 220px); min-height: min(360px, calc(100dvh - 270px)); } } }
@media (prefers-reduced-motion: reduce) { .apt-list-panel, .apt-detail-panel, .btn-apt-collapse { transition: none !important; } }
.modal:has(.apt-wrapper) .modal-dialog { transition: width .45s cubic-bezier(.2,.8,.2,1), max-width .45s cubic-bezier(.2,.8,.2,1); }
.modal:has(.apt-wrapper) .modal-dialog.modal-template-expanded, .modal:has(.apt-wrapper) .modal-dialog:has(.apt-detail-panel.is-split) { width: 78vw !important; max-width: 78vw !important; min-width: 0 !important; }
@media (max-width: 991.98px) { .modal:has(.apt-wrapper) .modal-dialog.modal-template-expanded, .modal:has(.apt-wrapper) .modal-dialog:has(.apt-detail-panel.is-split) { width: 92vw !important; max-width: 92vw !important; } }
@media (max-width: 575.98px) { .modal:has(.apt-wrapper) .modal-dialog.modal-template-expanded, .modal:has(.apt-wrapper) .modal-dialog:has(.apt-detail-panel.is-split) { width: calc(100vw - 16px) !important; max-width: calc(100vw - 16px) !important; } }

.apt-list-panel, .apt-detail-panel { min-height: 0; overflow-x: hidden; overflow-y: auto; scrollbar-width: thin; overscroll-behavior: contain; }
.apt-list-panel:not(.apt-has-overflow), .apt-detail-panel:not(.apt-has-overflow) { scrollbar-width: none; }
.apt-list-panel:not(.apt-has-overflow)::-webkit-scrollbar, .apt-detail-panel:not(.apt-has-overflow)::-webkit-scrollbar { display: none; }
.apt-list-panel.apt-has-overflow, .apt-detail-panel.apt-has-overflow { overflow-y: auto !important; scrollbar-width: thin; }
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
                                    <asp:TemplateField HeaderText="Hành động" HeaderStyle-Width="120px" ItemStyle-Width="120px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <asp:LinkButton runat="server" CommandName="VIEW_DETAIL" CommandArgument='<%# Eval("IdMau") %>' CssClass="btn btn-sm btn-outline-info me-1" ToolTip="Xem chi tiết các task"><i class="fas fa-eye"></i></asp:LinkButton>
                                            <asp:LinkButton runat="server" CommandName="APPLY_TEMPLATE" CommandArgument='<%# Eval("IdMau") %>' CssClass="btn btn-sm btn-primary" ToolTip="Áp dụng mẫu này" OnClientClick="return beginApplyTemplate(this);"><i class="fas fa-check"></i> Chọn</asp:LinkButton>
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
                                    <asp:LinkButton runat="server" ID="lbtApplySelectedTemplate" OnClick="lbtApplySelectedTemplate_Click" CssClass="btn-apt-apply" ToolTip="Sử dụng mẫu công việc này" OnClientClick="return beginApplyTemplate(this);"><i class="fas fa-check"></i> Sử dụng mẫu</asp:LinkButton>
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
    function beginApplyTemplate(button) {
        var message = 'CẢNH BÁO: Hệ thống sẽ XÓA VĨNH VIỄN toàn bộ công việc và giai đoạn hiện tại của dự án, sau đó tạo lại theo mẫu. Hành động này không thể hoàn tác. Bạn chắc chứ?';
        if (!window.confirm(message)) return false;
        if (!button.hasAttribute('data-apt-original-html'))
            button.setAttribute('data-apt-original-html', button.innerHTML);
        button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang áp dụng...';
        button.style.pointerEvents = 'none';
        button.classList.add('opacity-75', 'is-applying');
        return true;
    }
    function restoreApplyTemplateButtons() {
        document.querySelectorAll('.is-applying[data-apt-original-html]').forEach(function (button) {
            button.innerHTML = button.getAttribute('data-apt-original-html');
            button.removeAttribute('data-apt-original-html');
            button.style.pointerEvents = '';
            button.classList.remove('opacity-75', 'is-applying');
        });
    }
    (function () {
        function measureApplyTemplatePanels() {
            var panels = document.querySelectorAll('.apt-list-panel, .apt-detail-panel');
            panels.forEach(function (panel) {
                if (!panel || !panel.isConnected) return;
                if (panel.classList.contains('apt-detail-panel') && !panel.classList.contains('is-split')) {
                    panel.classList.remove('apt-has-overflow');
                    panel.style.overflowY = 'hidden';
                    return;
                }
                if (panel.offsetWidth < 2 || panel.offsetHeight < 2) {
                    panel.classList.remove('apt-has-overflow');
                    panel.style.overflowY = 'hidden';
                    return;
                }
                panel.style.overflowY = 'hidden';
                var hasVerticalOverflow = panel.scrollHeight > panel.clientHeight + 2;
                panel.classList.toggle('apt-has-overflow', hasVerticalOverflow);
                panel.style.overflowY = hasVerticalOverflow ? 'auto' : 'hidden';
            });
        }

        window.refreshApplyTemplateScrollbars = function () {
            window.setTimeout(measureApplyTemplatePanels, 0);
            window.setTimeout(measureApplyTemplatePanels, 180);
            window.setTimeout(measureApplyTemplatePanels, 520);
        };

        if (!window.__applyTemplateScrollResizeBound) {
            window.__applyTemplateScrollResizeBound = true;
            window.addEventListener('resize', function () {
                window.refreshApplyTemplateScrollbars();
            });
        }
        if (!window.__applyTemplateEndRequestBound && window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            window.__applyTemplateEndRequestBound = true;
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                restoreApplyTemplateButtons();
                window.refreshApplyTemplateScrollbars();
            });
        }

        window.refreshApplyTemplateScrollbars();
    })();
</script>
