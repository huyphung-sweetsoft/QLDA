<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlProjectTabs.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlProjectTabs" %>

<style>
    /* Dieu huong du an: tab chu mot hang, khong dung nen tim cho trang thai active. */
    .project-feature-tabs li[data-server-pinned="0"] { display:none!important; }
    .project-feature-tabs.project-tabs-restoring,.project-feature-tabs.project-tabs-restoring.project-tabs-open { transition:none!important; }
    .project-feature-tabs { --project-tab-accent:#54218d;position:relative;display:block;width:100%!important;max-width:100%;max-height:0!important;margin:0!important;padding:0!important;border:0!important;border-radius:0!important;background:transparent!important;box-shadow:none!important;opacity:0;visibility:hidden;pointer-events:none;transform:translateY(-7px);overflow-x:auto!important;overflow-y:hidden!important;scrollbar-width:none;-ms-overflow-style:none;-webkit-overflow-scrolling:touch;transition:max-height .28s ease,opacity .2s ease,transform .28s ease,margin .28s ease,visibility 0s linear .28s; }
    .project-feature-tabs::-webkit-scrollbar { display:none;width:0;height:0; }
    .project-feature-tabs.project-tabs-open { max-height:52px!important;margin:0 0 9px!important;padding:0!important;opacity:1;visibility:visible;pointer-events:auto;transform:translateY(0);transition:max-height .28s ease,opacity .2s ease,transform .28s ease,margin .28s ease,visibility 0s; }
    .project-feature-tabs .nav-pills { display:flex!important;align-items:stretch!important;flex-wrap:nowrap!important;gap:2px;width:max-content;min-width:100%;min-height:43px;margin:0!important;padding:0!important;list-style:none!important;border:0!important;border-bottom:1px solid #e4e8ef!important;border-radius:0!important;background:transparent!important;box-shadow:none!important; }
    .project-feature-tabs .nav-item { position:relative;display:flex;align-items:stretch;flex:0 0 auto;margin:0!important;padding:0!important;border:0!important;background:transparent!important;box-shadow:none!important; }
    .project-feature-tabs .nav-pills .nav-link { position:relative;display:inline-flex!important;align-items:center;justify-content:center;min-height:42px;padding:0 12px!important;border:0!important;border-radius:0!important;outline:none!important;background:transparent!important;box-shadow:none!important;color:#64748b!important;font-size:13.5px;font-weight:580;line-height:1.25;text-decoration:none!important;white-space:nowrap;cursor:pointer;-webkit-tap-highlight-color:transparent;transition:color .15s ease,background-color .15s ease; }
    .project-feature-tabs .nav-item[data-module] > .nav-link > i { display:none!important; }
    .project-feature-tabs .nav-link .project-tab-label { display:inline-block!important;width:auto!important;max-width:none!important;margin:0!important;overflow:visible!important;opacity:1!important;transform:none!important;white-space:nowrap;transition:none!important; }
    .project-feature-tabs .nav-pills .nav-item[data-module] > .nav-link:not(.active) { padding-right:25px!important; }
    .project-feature-tabs .nav-pills .nav-item[data-module] > .nav-link:not(.active):hover { color:#54218d!important;background:rgba(84,33,141,.065)!important;border-radius:9px!important;box-shadow:none!important; }
    .project-feature-tabs .nav-pills .nav-item[data-module] > .nav-link:not(.active):focus-visible { border-radius:9px!important; }
    .project-feature-tabs .nav-pills .nav-link.active,.project-feature-tabs .nav-pills .show > .nav-link { color:#54218d!important;background:transparent!important;box-shadow:none!important;font-weight:750; }
    .project-feature-tabs .nav-pills .nav-link.active::after { content:"";position:absolute;left:10px;right:10px;bottom:5px;height:3px;border-radius:3px;background:var(--project-tab-accent);pointer-events:none; }
    .project-feature-tabs .nav-pills .nav-link:focus-visible,.project-feature-tabs .unpin-btn:focus-visible { outline:2px solid rgba(84,33,141,.55)!important;outline-offset:-3px!important;box-shadow:none!important; }
    /* Nut bo ghim la button rieng, chi hien khi hover/focus; khong lam tab dich chuyen. */
    .project-feature-tabs .nav-item > .unpin-btn { position:absolute;right:5px;top:50%;z-index:2;display:inline-flex;align-items:center;justify-content:center;width:17px;height:17px;margin:0;padding:0;border:0;border-radius:5px;background:transparent;color:#9ca3af;font-size:10px;line-height:1;cursor:pointer;opacity:0;visibility:hidden;pointer-events:none;transform:translateY(-50%);transition:color .15s ease,background-color .15s ease,opacity .15s ease; }
    .project-feature-tabs .nav-item:hover > .unpin-btn,.project-feature-tabs .nav-item:focus-within > .unpin-btn { opacity:1;visibility:visible;pointer-events:auto; }
    .project-feature-tabs .nav-item > .unpin-btn:hover { color:#54218d;background:#ede7f6; }
    /* Tuy chinh la hanh dong phu, khong canh tranh voi tab dang chon. */
    .project-feature-tabs .project-tab-customize-item { margin-left:10px!important;padding-left:11px!important; }
    .project-feature-tabs .project-tab-customize-item::before { content:"";position:absolute;left:2px;top:12px;width:1px;height:19px;background:#e1e5eb; }
    .project-feature-tabs .nav-pills .nav-link.project-tab-customize-link { display:inline-flex!important;align-items:center;gap:6px!important;min-height:42px;padding:0 10px!important;border:0!important;border-radius:6px!important;background:transparent!important;color:#6b7280!important;font-size:12.5px;font-weight:600;box-shadow:none!important; }
    .project-feature-tabs .nav-pills .nav-link.project-tab-customize-link:hover { background:#f2edf8!important;color:#54218d!important; }
    .project-feature-tabs .project-tab-customize-icon { display:inline-flex;align-items:center;justify-content:center;flex:0 0 16px;width:16px;height:16px;border:0!important;border-radius:0;background:transparent!important;color:inherit!important;font-size:12px;line-height:1;transition:none!important;transform:none!important; }
    .project-feature-tabs .project-tab-customize-link:hover .project-tab-customize-icon { background:transparent!important;color:inherit!important;transform:none!important; }
    .project-feature-tabs .project-tab-customize-label { display:inline-block!important;width:auto!important;max-width:none!important;margin:0!important;overflow:visible!important;opacity:1!important;font-size:12.5px;font-weight:600;line-height:1;white-space:nowrap;transition:none!important; }
    .project-feature-tabs .project-tab-customize-count { display:inline-flex;align-items:center;justify-content:center;flex:0 0 auto;min-width:18px;max-width:none!important;height:18px;margin:0!important;padding:0 5px!important;border:0!important;border-radius:5px;background:#eef0f4;color:#667085;font-size:11px;font-weight:700;font-variant-numeric:tabular-nums;line-height:1;opacity:1!important;transition:none!important; }
    @media(max-width:767.98px) { .project-feature-tabs .nav-pills .nav-link { min-height:40px;font-size:12.5px;padding:0 10px!important; } .project-feature-tabs .nav-pills .nav-item[data-module] > .nav-link:not(.active) { padding-right:23px!important; } .project-feature-tabs .nav-pills { min-height:41px; } .project-feature-tabs .nav-pills .nav-link.project-tab-customize-link { min-height:40px;font-size:12px; } .project-feature-tabs .project-tab-customize-label { font-size:12px; } }
    @media(prefers-reduced-motion:reduce) { .project-feature-tabs,.project-feature-tabs.project-tabs-open,.project-feature-tabs .nav-pills .nav-link,.project-feature-tabs .nav-item > .unpin-btn { transition:none!important; } }
    /* Dùng khung ExtraModal mặc định của SweetSoft. Không can thiệp modal-content, border, overflow hay nút X. */
    #<%= mdlProjectQuickLauncher.ClientID %> .modal-dialog { max-width:1040px; }
    .ql-customize { padding:16px 18px 9px; color:#354155; }
    .ql-customize .ql-intro { display:flex;align-items:flex-start;gap:12px;padding:13px 15px;margin-bottom:13px;border:1px solid #ded8ed;border-left:4px solid #7c3db4;border-radius:10px;background:#f6f8fc;color:#33465e; }
    .ql-customize .ql-intro-icon { display:grid;place-items:center;flex:0 0 36px;width:36px;height:36px;margin-top:1px;border:1px solid #e2d3f5;border-radius:10px;background:#ede5fa;color:#63239a;font-size:16px; }
    .ql-customize .ql-intro-content { flex:1;min-width:0; }
    .ql-customize .ql-intro-title { margin:0 0 4px;color:#293e5a;font-size:14.5px;font-weight:800;line-height:1.35; }
    .ql-customize .ql-intro-desc { margin:0;color:#4c6079;font-size:13px;font-weight:550;line-height:1.6; }
    .ql-customize .ql-shortcuts { display:flex;align-items:center;flex-wrap:wrap;gap:7px 14px;margin-top:10px; }
    .ql-customize .ql-shortcut { display:inline-flex;align-items:center;flex-wrap:wrap;gap:6px;color:#405675;font-size:12.5px;font-weight:650;line-height:1.5; }
    .ql-customize .ql-kbd { display:inline-flex;align-items:center;justify-content:center;min-height:25px;padding:3px 8px;border:1px solid #cbb7e5;border-radius:6px;background:#fff;color:#54218d;font-family:inherit;font-size:11.5px;font-weight:800;line-height:1.2;white-space:nowrap;box-shadow:0 1px 0 rgba(84,33,141,.08); }
    .ql-customize .ql-toolbar { display:flex;align-items:center;gap:10px;margin-bottom:11px; }
    .ql-customize .ql-search-wrap { position:relative;display:flex;align-items:center;flex:1;min-width:0; }
    .ql-customize .ql-search-wrap>i { position:absolute;left:13px;color:#805b9e;pointer-events:none; }
    .ql-customize .ql-search { width:100%;height:39px;padding:0 12px 0 37px;border:1px solid #ded5ea;border-radius:8px;background:#fff;color:#342843;font-size:13px;outline:none;transition:border-color .15s ease,box-shadow .15s ease; }
    .ql-customize .ql-search:focus { border-color:#a27ac9;box-shadow:0 0 0 3px rgba(84,33,141,.10); }
    .ql-customize .ql-count { flex:0 0 auto;padding:7px 10px;border:1px solid #e8ddf3;border-radius:8px;background:#f9f6fe;color:#663399;font-size:11.5px;font-weight:700; }
    .ql-customize .ql-filters { display:flex;align-items:center;flex-wrap:wrap;gap:6px;margin-bottom:14px; }
    .ql-customize .ql-bulk-actions { display:flex;align-items:center;justify-content:flex-end;flex-wrap:wrap;gap:7px;margin-left:auto; }
    .ql-customize .ql-bulk-btn { display:inline-flex;align-items:center;justify-content:center;gap:6px;min-height:34px;padding:7px 11px;border:1px solid;border-radius:8px;font-size:12px;font-weight:750;cursor:pointer;transition:background-color .16s,border-color .16s,color .16s; }
    .ql-customize .ql-bulk-select { border-color:#adddcc;background:#eef9f4;color:#147455; }
    .ql-customize .ql-bulk-select:hover:not(:disabled) { border-color:#6fc4a2;background:#dbf4e8;color:#096849; }
    .ql-customize .ql-bulk-clear { border-color:#ebcad3;background:#fff4f6;color:#ac4560; }
    .ql-customize .ql-bulk-clear:hover:not(:disabled) { border-color:#df9cae;background:#ffe5eb;color:#97334f; }
    .ql-customize .ql-bulk-btn:disabled { opacity:.47;cursor:not-allowed; }
    .ql-customize .ql-bulk-btn:focus-visible { outline:2px solid #9562be;outline-offset:2px; }
    .ql-customize .ql-filter { padding:7px 12px;border:1px solid #e4ddeb;border-radius:8px;background:#fff;color:#70657d;font-size:12px;font-weight:700;cursor:pointer;transition:background .15s,color .15s,border-color .15s; }
    .ql-customize .ql-filter:hover { background:#f5effc;border-color:#cbb7e4;color:#54218d; }
    .ql-customize .ql-filter.is-active { background:#54218d;color:#fff;border-color:#54218d; }
    .ql-customize .ql-apps { display:grid;grid-template-columns:repeat(5,minmax(0,1fr));align-content:start;gap:11px;max-height:min(51vh,465px);min-height:130px;overflow-y:auto;padding:3px 2px 9px;scrollbar-width:thin;scrollbar-color:#d7cbe6 transparent;overscroll-behavior:contain; }
    .ql-customize .ql-app { position:relative;display:flex;align-items:center;justify-content:center;flex-direction:column;gap:9px;min-width:0;min-height:150px;padding:15px 8px 12px;border:1px solid #e5deef;border-radius:12px;background:#fff;color:#334155;text-align:center;cursor:pointer;transition:background .15s,border-color .15s,transform .15s; }
    .ql-customize .ql-app:hover { background:#faf7ff;border-color:#c6b2e0;transform:translateY(-2px); }
    .ql-customize .ql-app:focus-visible,.ql-customize .ql-filter:focus-visible { outline:2px solid #9562be;outline-offset:2px; }
    .ql-customize .ql-app.is-pinned { background:#f2faf6;border-color:#9fdcc5; }
    .ql-customize .ql-app.is-pinned:hover { background:#eaf8f1;border-color:#6fc6a5; }
    .ql-customize .ql-app.is-current { cursor:default; }
    .ql-customize .ql-app.is-current:hover { transform:none; }
    .ql-customize .ql-app:disabled { opacity:1;-webkit-text-fill-color:currentColor; }
    .ql-customize .ql-app-order { position:absolute;top:9px;left:10px;min-width:18px;height:18px;display:grid;place-items:center;padding:0 3px;border-radius:5px;color:#867399;background:#f5f0fa;font-size:10px;font-weight:800; }
    .ql-customize .ql-app-badge { position:absolute;top:8px;right:8px;display:grid;place-items:center;width:21px;height:21px;border:1px solid #e4e6ee;border-radius:50%;background:#f3f5f8;color:#8a96a8;font-size:10px; }
    .ql-customize .ql-app.is-pinned .ql-app-badge { background:#daf5e8;border-color:#b5e7d1;color:#16815c; }
    .ql-customize .ql-app.is-current .ql-app-badge { background:#eaddf9;border-color:#d9c4ef;color:#54218d; }
    .ql-customize .ql-app-icon { display:grid;place-items:center;width:60px;height:60px;flex:0 0 60px;border:1px solid var(--ql-border);border-radius:16px;background:var(--ql-bg);color:var(--ql-color);font-size:26px;box-shadow:none; }
    .ql-customize .ql-app-name { display:-webkit-box;-webkit-box-orient:vertical;-webkit-line-clamp:2;max-width:100%;min-height:33px;overflow:hidden;color:#354056;font-size:12.5px;line-height:1.35;font-weight:750;overflow-wrap:anywhere; }
    .ql-customize .ql-app-state { margin-top:-5px;color:#8492a5;font-size:10.5px; }
    .ql-customize .ql-app.is-pinned .ql-app-state { color:#1b8564; }
    .ql-customize .ql-theme-0 { --ql-bg:#efe6f9;--ql-color:#6a299f;--ql-border:#e1d2f0; }
    .ql-customize .ql-theme-1 { --ql-bg:#e0f3ec;--ql-color:#168266;--ql-border:#c9eadc; }
    .ql-customize .ql-theme-2 { --ql-bg:#fff1dd;--ql-color:#ac721b;--ql-border:#f5e2c4; }
    .ql-customize .ql-theme-3 { --ql-bg:#e9f0fe;--ql-color:#3261bc;--ql-border:#d9e5f8; }
    .ql-customize .ql-theme-4 { --ql-bg:#fde9f1;--ql-color:#b04175;--ql-border:#f8d8e4; }
    .ql-customize .ql-theme-5 { --ql-bg:#eceafa;--ql-color:#6457af;--ql-border:#dfdafa; }
    .ql-customize .ql-theme-6 { --ql-bg:#e3f5f6;--ql-color:#138397;--ql-border:#ceebee; }
    .ql-customize .ql-theme-7 { --ql-bg:#ffebe7;--ql-color:#c26650;--ql-border:#f7d8d0; }
    .ql-customize .ql-empty { grid-column:1/-1;display:flex;flex-direction:column;align-items:center;justify-content:center;min-height:190px;gap:10px;color:#7c7288;text-align:center;font-size:13px; }
    .ql-customize .ql-empty>i { color:#9a70c0;font-size:30px; }
    .ql-customize .ql-empty strong { color:#43285c;font-size:15px; }
    .ql-customize-footer { display:flex;align-items:center;justify-content:space-between;gap:12px;width:100%; }
    .ql-customize-footer .ql-footnote { display:flex;align-items:center;gap:7px;color:#82758c;font-size:11.5px;line-height:1.4; }
    .ql-customize-footer .ql-footnote i { color:#784da5; }
    .ql-customize-footer .ql-footer-actions { display:flex;align-items:center;justify-content:flex-end;gap:8px;flex:0 0 auto; }
    .ql-customize-footer .ql-btn { display:inline-flex;align-items:center;justify-content:center;gap:6px;min-height:35px;padding:7px 11px;border:1px solid #ddd4e8;border-radius:7px;background:#fff;color:#56456b;font-size:12px;font-weight:750;cursor:pointer; }
    .ql-customize-footer .ql-btn:hover { background:#f5effc;border-color:#c9b2e5; }
    .ql-customize-footer .ql-btn-primary { background:#54218d;border-color:#54218d;color:#fff; }
    .ql-customize-footer .ql-btn-primary:hover { background:#451777;border-color:#451777;color:#fff; }
    .ql-customize-footer .ql-btn:disabled { opacity:.48;cursor:default; }
    @media(max-width:991.98px) { .ql-customize .ql-apps { grid-template-columns:repeat(4,minmax(0,1fr)); } }
    @media(max-width:767.98px) { .ql-customize { padding:12px 11px 6px; } .ql-customize .ql-intro { padding:11px 12px;gap:9px; } .ql-customize .ql-intro-title { font-size:13.5px; } .ql-customize .ql-intro-desc { font-size:12.5px; } .ql-customize .ql-shortcut { font-size:12px; } .ql-customize .ql-bulk-actions { flex:1 0 100%;justify-content:flex-end;margin-left:0; } .ql-customize .ql-apps { grid-template-columns:repeat(3,minmax(0,1fr));gap:8px;max-height:min(41vh,360px); } .ql-customize .ql-app { min-height:129px;padding:12px 5px;gap:7px; } .ql-customize .ql-app-icon { width:49px;height:49px;flex-basis:49px;font-size:22px; } .ql-customize-footer { flex-direction:column;align-items:stretch; } .ql-customize-footer .ql-footer-actions { justify-content:flex-end; } }
    @media(max-width:430px) { .ql-customize .ql-apps { grid-template-columns:repeat(2,minmax(0,1fr));max-height:min(36vh,300px); } .ql-customize .ql-count { display:none; } .ql-customize .ql-app { min-height:132px; } .ql-customize .ql-intro-icon { display:none; } .ql-customize .ql-intro { align-items:flex-start; } .ql-customize .ql-bulk-btn { flex:1;min-width:0;font-size:11.5px;padding:7px 8px; } }
    @media(prefers-reduced-motion:reduce) { .ql-customize .ql-app { transition:none; } }
</style>

<nav id="projectFeatureTabs" class="project-feature-tabs project-tabs-restoring mb-3" aria-label="Chức năng trong dự án">
    <ul runat="server" id="ulProjectTabs" class="nav nav-pills align-items-center"></ul>
</nav>

<!-- Popup chung của dự án: sử dụng ExtraModal để giữ nguyên viền và nút X chuẩn SweetSoft. -->
<SweetSoft:ExtraModal ID="mdlProjectQuickLauncher" runat="server" Type="Primary" Title="Tùy chỉnh chức năng hiển thị">
    <ContentTemplate>
        <div class="ql-customize">
            <div class="ql-intro">
                <span class="ql-intro-icon"><i class="fas fa-keyboard" aria-hidden="true"></i></span>
                <div class="ql-intro-content">
                    <div class="ql-intro-title">Ghim chức năng &amp; điều hướng nhanh</div>
                    <p class="ql-intro-desc">Chọn những chức năng muốn hiển thị trên thanh công cụ. Có thể ghim hoặc bỏ ghim nhiều chức năng rồi lưu một lần.</p>
                    <div class="ql-shortcuts" aria-label="Các phím tắt chuyển trang">
                        <span class="ql-shortcut"><kbd class="ql-kbd">Shift + 1…9</kbd><span>Tab ghim thứ 1–9</span></span>
                        <span class="ql-shortcut"><kbd class="ql-kbd">Shift + 0</kbd><span>Tab ghim thứ 10</span></span>
                        <span class="ql-shortcut"><kbd class="ql-kbd">Shift + Tab</kbd><span>Trang kế tiếp</span></span>
                        <span class="ql-shortcut"><kbd class="ql-kbd">Shift + →</kbd><span>Trang kế tiếp</span></span>
                        <span class="ql-shortcut"><kbd class="ql-kbd">Shift + ←</kbd><span>Trang trước</span></span>
                    </div>
                </div>
            </div>
            <div class="ql-toolbar">
                <label class="ql-search-wrap">
                    <i class="fas fa-search" aria-hidden="true"></i>
                    <input id="qlSearch" type="search" class="ql-search" placeholder="Tìm kiếm chức năng..." autocomplete="off" aria-label="Tìm chức năng dự án" />
                </label>
                <span class="ql-count" id="qlCount" aria-live="polite">0 chức năng đã ghim</span>
            </div>
            <div class="ql-filters" role="group" aria-label="Bộ lọc chức năng">
                <button type="button" class="ql-filter is-active" data-ql-filter="all" aria-pressed="true">Tất cả</button>
                <button type="button" class="ql-filter" data-ql-filter="pinned" aria-pressed="false">Đã ghim</button>
                <button type="button" class="ql-filter" data-ql-filter="unpinned" aria-pressed="false">Chưa ghim</button>
                <div class="ql-bulk-actions" role="group" aria-label="Chọn nhanh chức năng">
                    <button type="button" id="qlPinAll" class="ql-bulk-btn ql-bulk-select"><i class="fas fa-check-double" aria-hidden="true"></i>Chọn tất cả</button>
                    <button type="button" id="qlUnpinAll" class="ql-bulk-btn ql-bulk-clear"><i class="fas fa-times-circle" aria-hidden="true"></i>Bỏ chọn tất cả</button>
                </div>
            </div>
            <div class="ql-apps" id="qlApps" role="group" aria-label="Danh sách chức năng trong dự án"></div>
        </div>
    </ContentTemplate>
    <FooterTemplate>
        <div class="ql-customize-footer">
            <span class="ql-footnote"><i class="fas fa-info-circle" aria-hidden="true"></i>Chức năng đang mở luôn được ghim.</span>
            <div class="ql-footer-actions">
                <button type="button" class="ql-btn" id="qlUndo"><i class="fas fa-undo-alt" aria-hidden="true"></i>Hoàn tác</button>
                <button type="button" class="ql-btn ql-btn-primary" id="qlSave"><i class="fas fa-save" aria-hidden="true"></i>Lưu thay đổi</button>
            </div>
        </div>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    var allTabs = [];
    var defaultPinnedKeys = [];
    var activeKey = null;
    var pinnedKeys = [];
    var tempPinnedKeys = [];

    $(function () {
        var $ul = $('#<%= ulProjectTabs.ClientID %>');
        if ($ul.length === 0) return;

        // Thu thập các chức năng do server sinh ra.
        allTabs = [];
        defaultPinnedKeys = [];
        activeKey = null;

        $ul.find('li[data-module]').each(function (index) {
            var $li = $(this);
            var key = $li.attr('data-module');
            var $a = $li.find('a').first();
            var isActive = $a.hasClass('active');
            var name = $a.clone().children().remove().end().text().trim();
            var icon = $li.find('i').first().attr('class') || 'fas fa-folder';
            var href = $a.attr('href');

            // Gom text node vao span, de tab hien day du ten ngay ca khi khong hover.
            // Chi lay text node, khong sua icon hay href cua tab.
            var textNodes = $a.contents().filter(function () {
                return this.nodeType === 3 && $.trim(this.nodeValue).length > 0;
            });
            if (textNodes.length) {
                var labelText = textNodes.map(function () { return this.nodeValue; }).get().join(' ').trim();
                textNodes.remove();
                $('<span class="project-tab-label"></span>').text(labelText).appendTo($a);
            }

            allTabs.push({ key: key, name: name, icon: icon, href: href, isActive: isActive, $el: $li });

            if (isActive) activeKey = key;
            if (index < 3) defaultPinnedKeys.push(key);
        });

        // Lần đầu truy cập: ghim 3 chức năng đầu. Những lần sau dùng cấu hình đã lưu.
        var saved = null;
        try { saved = localStorage.getItem('SweetSoft_PinnedProjectTabs_v4'); } catch (e) { }
        if (saved) {
            try {
                var parsed = JSON.parse(saved);
                pinnedKeys = Array.isArray(parsed) ? parsed : defaultPinnedKeys.slice();
            } catch (error) {
                pinnedKeys = defaultPinnedKeys.slice();
            }
        } else {
            pinnedKeys = defaultPinnedKeys.slice();
        }

        // Loại bỏ các khóa không còn tồn tại, tránh cấu hình cũ làm lệch số lượng.
        var availableKeys = allTabs.map(function (tab) { return tab.key; });
        pinnedKeys = pinnedKeys.filter(function (key, index, list) {
            return availableKeys.indexOf(key) !== -1 && list.indexOf(key) === index;
        });

        // Chỉ đồng bộ cookie với cấu hình đã ghim. Không tự lưu tab đang xem khi chuyển trang.
        // Tab hiện tại vẫn được hiển thị tạm thời nhưng không tự ghim vĩnh viễn.
        if (pinnedKeys.length === 0 && allTabs.length > 0) pinnedKeys.push(allTabs[0].key);
        syncProjectPinnedTabsCookie(pinnedKeys);
        if (activeKey && pinnedKeys.indexOf(activeKey) === -1) pinnedKeys.push(activeKey);

        // Sắp xếp thanh tab theo thứ tự server khai báo, chỉ hiển thị các tab được ghim.
        $ul.empty();
        var unpinnedCount = 0;

        allTabs.forEach(function (tab) {
            if (pinnedKeys.indexOf(tab.key) !== -1) {
                var $li = tab.$el;
                $li.removeAttr('data-server-pinned').show();

                if (tab.key !== activeKey) {
                    // Button bo ghim rieng: khong long button/span vao trong link dieu huong.
                    var $unpinBtn = $('<button type="button" class="unpin-btn" title="Bỏ ghim" aria-label="Bỏ ghim"><i class="fas fa-times" aria-hidden="true"></i></button>');
                    $unpinBtn.attr('aria-label', 'Bỏ ghim ' + tab.name);
                    $li.find('> .unpin-btn').remove();
                    $li.append($unpinBtn);

                    $unpinBtn.on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();

                        // Khong cho bo ghim tab dang mo hoac tab cuoi cung.
                        if (pinnedKeys.length <= 1 || tab.key === activeKey) return;

                        // Cap nhat trang thai ngay tren client, khong tai lai ca trang.
                        pinnedKeys = pinnedKeys.filter(function (key) { return key !== tab.key; });
                        persistProjectPinnedTabs();

                        // An tab vua bo ghim; van giu doi tuong trong allTabs de popup co the them lai.
                        if (tab.$el && tab.$el.length) {
                            tab.$el.stop(true, true).remove();
                        }

                        updateCustomizeButtonCount();
                    });
                }
                $ul.append($li);
            } else {
                unpinnedCount++;
            }
        });

        // Dieu huong va tuy chinh duoc phan cap ro: nut tuy chinh khong co nen tim.
        var customizeTitle = 'Tùy chỉnh chức năng hiển thị (' + unpinnedCount + ' chức năng chưa ghim)';
        var $btnCustomize = $('<li class="nav-item project-tab-customize-item">' +
            '<a class="nav-link project-tab-customize-link" href="#" onclick="openProjectQuickLauncher();return false;" title="' + customizeTitle + '" aria-label="' + customizeTitle + '">' +
            '<span class="project-tab-customize-icon" aria-hidden="true"><i class="fas fa-sliders-h"></i></span>' +
            '<span class="project-tab-customize-label">Tùy chỉnh</span>' +
            '<span class="project-tab-customize-count">' + unpinnedCount + '</span>' +
            '</a></li>');
        $ul.append($btnCustomize);
        updateCustomizeButtonCount();
    });

    // Cookie chỉ chứa các khóa ghim cố định, giúp server lọc tab ngay trong HTML (tránh chớp mỗi lần chuyển trang).
    function syncProjectPinnedTabsCookie(keys) {
        try { document.cookie = 'SweetSoft_PinnedProjectTabs_v4=' + encodeURIComponent(keys.join('|')) + '; path=/; max-age=31536000; SameSite=Lax'; } catch (e) { }
    }
    function persistProjectPinnedTabs() {
        try { localStorage.setItem('SweetSoft_PinnedProjectTabs_v4', JSON.stringify(pinnedKeys)); } catch (e) { }
        syncProjectPinnedTabsCookie(pinnedKeys);
    }
    // Giữ trạng thái mở/đóng trước khi trang được vẽ, không chạy animation mỗi lần điều hướng.
    (function restoreProjectTabAppearance() {
        var panel = document.getElementById('projectFeatureTabs');
        if (!panel) return;
        var isOpen = false;
        try { isOpen = localStorage.getItem('SweetSoft_ProjectFunctionsOpen_v1') === 'true'; } catch (e) { }
        panel.classList.toggle('project-tabs-open', isOpen);
        function reveal() {
            window.requestAnimationFrame(function () {
                window.requestAnimationFrame(function () { panel.classList.remove('project-tabs-restoring'); });
            });
        }
        if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', reveal);
        else reveal();
    })();

    // Cap nhat so chuc nang chua ghim ngay tren nut +, khong can reload trang.
    function updateCustomizeButtonCount() {
        var unpinnedCount = Math.max(0, allTabs.length - pinnedKeys.length);
        var title = 'Tùy chỉnh chức năng hiển thị (' + unpinnedCount + ' chức năng chưa ghim)';
        var $button = $('.project-feature-tabs .project-tab-customize-link');

        if (!$button.length) return;

        $button.find('.project-tab-customize-count').text(unpinnedCount).toggle(unpinnedCount > 0);
        $button.attr('title', title).attr('aria-label', title);
    }

    function escapeCustomizeHtml(value) {
        return $('<div/>').text(value == null ? '' : String(value)).html();
    }

    // Không cho tắt tab đang mở hoặc tắt chức năng cuối cùng còn ghim.
    function togglePin(key) {
        var idx = tempPinnedKeys.indexOf(key);

        if (idx !== -1) {
            if (key === activeKey || tempPinnedKeys.length <= 1) return;
            tempPinnedKeys.splice(idx, 1);
        } else {
            tempPinnedKeys.push(key);
        }

    }

    function saveTabPreferences() {
        // Chốt bảo vệ tối thiểu một chức năng, kể cả khi cấu hình bị thay đổi ngoài UI.
        if (tempPinnedKeys.length === 0 && allTabs.length > 0) {
            tempPinnedKeys.push(activeKey || allTabs[0].key);
        }

        // Tab hiện tại luôn được giữ lại.
        if (activeKey && tempPinnedKeys.indexOf(activeKey) === -1) {
            tempPinnedKeys.push(activeKey);
        }

        pinnedKeys = tempPinnedKeys.slice();
        persistProjectPinnedTabs();
        location.reload();
    }


    // Hai cách mở một ExtraModal duy nhất; dữ liệu lấy từ tất cả tab server sinh ra (kể cả chưa ghim).
    (function () {
        var launcherFilter = 'all';
        var modalSelector = '#<%= mdlProjectQuickLauncher.ClientID %>';
        var modalOpen = false;
        function normalizeLauncherText(value) {
            var str = String(value == null ? '' : value).toLowerCase().replace(/đ/g, 'd');
            if (typeof str.normalize === 'function') str = str.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
            return str;
        }
        function launcherHasChanges() {
            if (pinnedKeys.length !== tempPinnedKeys.length) return true;
            return pinnedKeys.some(function (key) { return tempPinnedKeys.indexOf(key) === -1; });
        }
        function renderProjectQuickLauncher() {
            var $apps = $('#qlApps');
            if (!$apps.length) return;
            var query = normalizeLauncherText($('#qlSearch').val()).trim();
            var visibleCount = 0;
            var html = '';
            allTabs.forEach(function (tab, index) {
                var isPinned = tempPinnedKeys.indexOf(tab.key) !== -1;
                if (launcherFilter === 'pinned' && !isPinned) return;
                if (launcherFilter === 'unpinned' && isPinned) return;
                if (query && normalizeLauncherText(tab.name).indexOf(query) === -1) return;
                visibleCount++;
                var isCurrent = tab.key === activeKey;
                var badgeIcon = isCurrent ? 'fa-lock' : (isPinned ? 'fa-check' : 'fa-plus');
                var stateLabel = isCurrent ? 'Đang mở' : (isPinned ? 'Đã ghim' : 'Chưa ghim');
                var hint = isCurrent ? 'Chức năng đang mở luôn được ghim' : (isPinned ? 'Bỏ ghim ' + tab.name : 'Ghim ' + tab.name);
                var pinnedOrder = getPinnedNavigationTabs().findIndex(function (item) { return item.key === tab.key; });
                if (pinnedOrder >= 0 && pinnedOrder < 10) hint += '. Phím chuyển trang: Shift + ' + ((pinnedOrder + 1) % 10);
                html += '<button type="button" class="ql-app ql-theme-' + (index % 8) + (isPinned ? ' is-pinned' : '') + (isCurrent ? ' is-current' : '') + '"' +
                    ' data-ql-key="' + escapeCustomizeHtml(tab.key) + '" title="' + escapeCustomizeHtml(hint) + '"' +
                    ' aria-label="' + escapeCustomizeHtml(hint) + '" aria-pressed="' + (isPinned ? 'true' : 'false') + '"' + (isCurrent ? ' disabled="disabled"' : '') + '>' +
                    '<span class="ql-app-order" aria-hidden="true">' + (pinnedOrder >= 0 ? (pinnedOrder + 1) : '+') + '</span>' +
                    '<span class="ql-app-badge" aria-hidden="true"><i class="fas ' + badgeIcon + '"></i></span>' +
                    '<span class="ql-app-icon" aria-hidden="true"><i class="' + escapeCustomizeHtml(tab.icon || 'fas fa-folder') + '"></i></span>' +
                    '<span class="ql-app-name">' + escapeCustomizeHtml(tab.name) + '</span>' +
                    '<span class="ql-app-state">' + stateLabel + '</span></button>';
            });
            if (!visibleCount) html = '<div class="ql-empty"><i class="fas fa-search" aria-hidden="true"></i><strong>Không tìm thấy chức năng</strong><span>Thử từ khóa hoặc bộ lọc khác.</span></div>';
            $apps.html(html);
            $('#qlCount').text(tempPinnedKeys.length + '/' + allTabs.length + ' chức năng đã ghim');
            $('#qlSave').prop('disabled', !launcherHasChanges());
            $('#qlUndo').prop('disabled', !launcherHasChanges());
            $('#qlPinAll').prop('disabled', !allTabs.length || allTabs.every(function (tab) { return tempPinnedKeys.indexOf(tab.key) !== -1; }));
            var keepKey = activeKey || (allTabs.length ? allTabs[0].key : null);
            $('#qlUnpinAll').prop('disabled', !allTabs.length || !tempPinnedKeys.some(function (key) { return key !== keepKey; }));
        }
        function openProjectQuickLauncher() {
            var $modal = $(modalSelector);
            if (!$modal.length || !allTabs.length) return;
            if ($('.modal:visible, .offcanvas.show:visible').not($modal).length) return;
            if (modalOpen || $modal.hasClass('show')) return;
            tempPinnedKeys = pinnedKeys.slice();
            launcherFilter = 'all';
            $('#qlSearch').val('');
            $modal.find('.ql-filter').removeClass('is-active').attr('aria-pressed', 'false');
            $modal.find('[data-ql-filter="all"]').addClass('is-active').attr('aria-pressed', 'true');
            renderProjectQuickLauncher();
            modalOpen = true;
            $modal.modal('show');
        }
        function closeProjectQuickLauncher() {
            $(modalSelector).modal('hide');
        }
        // URL từ tab do server sinh ra; không điều hướng javascript: hay tới domain khác.
        function validProjectTabUrl(tab) {
            if (!tab || !tab.href || /^\s*(?:javascript:|data:|vbscript:|#)/i.test(tab.href)) return null;
            try {
                var parsed = new URL(tab.href, window.location.href);
                if (!/^https?:$/.test(parsed.protocol) || parsed.origin !== window.location.origin) return null;
                return parsed.href;
            } catch (error) { return null; }
        }
        // Chỉ dùng các tab được ghim VÀ đang hiện trên thanh (không tính nút Thêm).
        // Thứ tự là thứ tự thanh công cụ, không phải thứ tự 10 chức năng gốc.
        function getPinnedNavigationTabs() {
            return allTabs.filter(function (tab) {
                return pinnedKeys.indexOf(tab.key) !== -1 && tab.$el && tab.$el.length &&
                    tab.$el.parent().length > 0 && tab.$el.css('display') !== 'none';
            });
        }
        function navigateProjectTab(index) {
            var tabs = getPinnedNavigationTabs();
            if (index < 0 || index >= tabs.length) return false;
            var tab = tabs[index];
            var url = validProjectTabUrl(tab);
            if (!url) return false;
            if (tab.key === activeKey) return true;
            window.location.assign(url);
            return true;
        }
        function navigateProjectRelative(direction) {
            var tabs = getPinnedNavigationTabs();
            if (tabs.length < 2) return false;
            var index = tabs.findIndex(function (tab) { return tab.key === activeKey; });
            if (index < 0) index = direction > 0 ? -1 : 0;
            for (var attempt = 1; attempt <= tabs.length; attempt++) {
                var nextIndex = ((index + direction * attempt) % tabs.length + tabs.length) % tabs.length;
                if (nextIndex !== index && validProjectTabUrl(tabs[nextIndex])) return navigateProjectTab(nextIndex);
            }
            return false;
        }
        function isTypingTarget(target) {
            if (!target || !target.closest) return false;
            return !!target.closest('input,textarea,select,[contenteditable],[role="textbox"],[role="combobox"]') || !!target.isContentEditable;
        }
        function isAnyPopupOpen() {
            return $('.modal:visible, .offcanvas.show:visible, [role="dialog"]:visible').length > 0;
        }
        window.openProjectQuickLauncher = openProjectQuickLauncher;
        window.openCustomizeModal = openProjectQuickLauncher;
        window.closeProjectQuickLauncher = closeProjectQuickLauncher;

        // Không bắt phím khi người dùng đang nhập, có popup khác, hoặc đang dùng Ctrl/Alt/Meta.
        $(document).off('keydown.projectQuickLauncher').on('keydown.projectQuickLauncher', function (e) {
            if (!e.shiftKey || e.ctrlKey || e.altKey || e.metaKey || e.repeat || e.isComposing) return;
            if (isTypingTarget(e.target) || !allTabs.length) return;
            var code = e.originalEvent && e.originalEvent.code ? e.originalEvent.code : e.code;
            var shiftQ = code === 'KeyQ' || (e.key || '').toLowerCase() === 'q';
            if (shiftQ) {
                if (isAnyPopupOpen() && !modalOpen) return;
                e.preventDefault();
                if (modalOpen) closeProjectQuickLauncher(); else openProjectQuickLauncher();
                return;
            }
            if (modalOpen || isAnyPopupOpen()) return;
            var numberMatch = /^(?:Digit|Numpad)([0-9])$/.exec(code || '');
            var number = numberMatch ? parseInt(numberMatch[1], 10) : null;
            // Shift+1 ... Shift+9, Shift+0 -> chức năng thứ 10.
            if (number !== null) {
                var index = number === 0 ? 9 : number - 1;
                var pinnedTabs = getPinnedNavigationTabs();
                if (index < pinnedTabs.length && validProjectTabUrl(pinnedTabs[index])) {
                    e.preventDefault();
                    navigateProjectTab(index);
                }
                return;
            }
            // Shift+Tab là thao tác lấy trang kế; Shift+mũi tên trái/phải cũng chuyển lùi/tiến.
            if (e.key === 'Tab' || code === 'ArrowRight' || code === 'ArrowLeft') {
                var direction = code === 'ArrowLeft' ? -1 : 1;
                if (getPinnedNavigationTabs().length > 1) { e.preventDefault(); navigateProjectRelative(direction); }
            }
        });
        function resetLauncherBulkFilter() {
            launcherFilter = 'all';
            $(modalSelector).find('.ql-filter').removeClass('is-active').attr('aria-pressed', 'false');
            $(modalSelector).find('[data-ql-filter="all"]').addClass('is-active').attr('aria-pressed', 'true');
        }
        $(document).off('click.projectQuickLauncher').on('click.projectQuickLauncher', '.ql-customize .ql-filter', function () {
            launcherFilter = $(this).attr('data-ql-filter') || 'all';
            $(modalSelector).find('.ql-filter').removeClass('is-active').attr('aria-pressed', 'false');
            $(this).addClass('is-active').attr('aria-pressed', 'true');
            renderProjectQuickLauncher();
        }).on('click.projectQuickLauncher', '#qlApps .ql-app', function () {
            var key = $(this).attr('data-ql-key');
            if (!key || key === activeKey) return;
            togglePin(key);
            renderProjectQuickLauncher();
        }).on('click.projectQuickLauncher', '#qlPinAll', function () {
            if (!allTabs.length) return;
            tempPinnedKeys = allTabs.map(function (tab) { return tab.key; });
            resetLauncherBulkFilter();
            renderProjectQuickLauncher();
        }).on('click.projectQuickLauncher', '#qlUnpinAll', function () {
            if (!allTabs.length) return;
            tempPinnedKeys = [activeKey || allTabs[0].key];
            resetLauncherBulkFilter();
            renderProjectQuickLauncher();
        }).on('click.projectQuickLauncher', '#qlUndo', function () {
            tempPinnedKeys = pinnedKeys.slice();
            renderProjectQuickLauncher();
        }).on('click.projectQuickLauncher', '#qlSave', function () {
            if (!launcherHasChanges()) return;
            saveTabPreferences();
        });
        $(document).off('input.projectQuickLauncher').on('input.projectQuickLauncher', '#qlSearch', renderProjectQuickLauncher);
        $(document).off('shown.bs.modal.projectQuickLauncher hidden.bs.modal.projectQuickLauncher', modalSelector)
            .on('shown.bs.modal.projectQuickLauncher', modalSelector, function () { $('#qlSearch').trigger('focus'); })
            .on('hidden.bs.modal.projectQuickLauncher', modalSelector, function () { modalOpen = false; tempPinnedKeys = pinnedKeys.slice(); });
    })();

</script>
