<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlChonNhanVien.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fProjects.Controls.CtrlChonNhanVien" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<style>
#<%= mdlMemberPicker.ClientID %> .modal-dialog{max-width:1360px;width:calc(100vw - 32px)}
.ppm-root{--ppm-purple:#54218e;--ppm-line:#e3e9f1;--ppm-row-h:74px;font-size:12px;min-width:0;width:100%}
.ppm-root *{box-sizing:border-box}
.ppm-info{padding:9px 12px;border:1px solid #bfdbfe;background:#eff6ff;color:#1e40af;border-radius:6px;display:flex;align-items:center;gap:6px;flex-wrap:wrap}
.ppm-header{display:flex;align-items:center;gap:9px;margin-bottom:10px}
.ppm-header .ppm-info{flex:1;min-width:0}
.ppm-switch{display:inline-flex;align-items:center;border:1px solid #b9cde9;border-radius:6px;padding:1px;white-space:nowrap;flex-shrink:0}
.ppm-switch button{width:32px;height:30px;border:0;border-radius:4px;background:#fff;color:#718096;font-size:13px;cursor:pointer}
.ppm-switch button.active{color:#2563eb;background:#dbeafe;box-shadow:inset 0 0 0 1px #93c5fd}
.ppm-toolbar{display:flex;align-items:center;gap:8px;min-height:41px;min-width:0;border:1px solid #e6eaf2;border-radius:8px;padding:6px 10px;margin-bottom:9px;white-space:nowrap}
.ppm-filter-label{color:#7b8799;font-size:11px;font-weight:700}
.ppm-role{flex:0 0 auto;min-width:100px;max-width:235px}
.ppm-role .bootstrap-dropdown{margin-top:0!important}
.ppm-role .bootstrap-dropdown,.ppm-role .dropdown-toggle{width:auto!important;max-width:235px;min-width:100px;height:32px!important;min-height:32px!important;white-space:nowrap}
.ppm-role .dropdown-toggle{font-size:12px!important;overflow:hidden;text-overflow:ellipsis}
.ppm-role .dropdown-menu{min-width:250px;max-height:260px;overflow-y:auto}
.ppm-search{flex:0 1 340px!important;min-width:170px;width:340px;max-width:340px;display:flex;flex-wrap:nowrap}
.ppm-search .input-search-filter{font-size:12px!important;height:33px!important;min-width:0;flex:1 1 0}
.ppm-search .btn-search-filter{height:33px!important;flex:0 0 39px;font-size:12px!important}
.ppm-tags{display:flex;align-items:center;min-width:0;max-width:360px;overflow-x:auto;overflow-y:hidden;scrollbar-width:thin}
.ppm-tags .listSearchTagBox{white-space:nowrap;display:flex;align-items:center;min-height:26px;max-width:100%;overflow:visible}
.ppm-tags .listSearchTagBox>div{display:inline-flex;align-items:center;min-width:0}
.ppm-period{display:flex;align-items:center;gap:5px;margin-left:auto;min-width:0;white-space:nowrap;color:#536783;font-size:10.5px}
.ppm-period a{display:inline-flex;align-items:center;justify-content:center;height:26px;width:26px;border:1px solid #dbe3ef;background:#fff;border-radius:5px;color:#5b238b;cursor:pointer}
.ppm-period a[disabled]{opacity:.4;pointer-events:none}
.ppm-period strong{font-size:11px;color:#4b5570;font-weight:650}
.ppm-views{height:min(56vh,490px);min-height:310px;overflow:hidden}
.ppm-list{height:100%;display:grid;grid-template-columns:minmax(0,1fr) minmax(0,1fr);gap:12px}
.ppm-pane{min-width:0;min-height:0;height:100%;display:flex;flex-direction:column;background:#f8fafc;border:1px solid #e3e9f1;border-radius:9px;overflow:hidden}
.ppm-pane-head{display:flex;align-items:center;justify-content:space-between;gap:6px;flex:0 0 40px;padding:0 12px;background:#eef2f8;color:#22517e;font-size:12.5px;font-weight:750}
.ppm-pane-head.selected{background:#f2ebfa;color:#642798}
.ppm-count{display:inline-flex;align-items:center;justify-content:center;min-width:22px;height:22px;background:#54218e;border-radius:12px;color:#fff;font-size:12px;padding:0 6px}
.ppm-candidates-hint{flex:0 0 auto;display:flex;gap:14px;align-items:center;min-height:30px;padding:5px 12px;background:#fff;border-bottom:1px solid #e6edf4;font-size:10px;color:#7b8799}
.ppm-pane-content{padding:10px;overflow-y:auto;overflow-x:hidden;min-height:0;flex:1}
.ppm-row{display:flex;align-items:center;gap:7px;min-height:56px;margin-bottom:7px;padding:7px 9px;border:1px solid #e2e8f0;border-radius:8px;background:#fff;min-width:0;position:relative}
.ppm-row.is-picked{background:#f7f1ff;border-color:#d9c8f6;box-shadow:inset 3px 0 #7c3aed}
.ppm-checkbox{flex:0 0 16px;display:flex;align-items:center;justify-content:center}
.ppm-checkbox input{width:16px;height:16px;accent-color:#2563eb;cursor:pointer;margin:0}
.ppm-avatar{flex:0 0 29px;height:29px;display:flex;align-items:center;justify-content:center}
.ppm-row .single-avatar-circle{flex:0 0 29px;width:29px;height:29px;border-radius:50%;color:#fff;font-size:10.5px;font-weight:700;display:inline-flex;align-items:center;justify-content:center;object-fit:cover}
.ppm-person{flex:1 1 auto;min-width:0;display:flex;flex-direction:column;gap:2px;line-height:1.25}
.ppm-person-title{min-width:0;display:flex;align-items:center;gap:5px;white-space:nowrap;overflow:hidden;font-size:12px}
.ppm-person-title b{color:#29384b;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;flex:0 1 auto}
.ppm-person-title span.ppm-role-label{font-size:10.5px;color:#71819a;text-overflow:ellipsis;overflow:hidden;white-space:nowrap;flex:0 1 auto}
.ppm-person-email{color:#7889a0;font-size:10.5px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.ppm-badge{flex:0 0 auto;font-size:10px;font-weight:650;border-radius:14px;padding:3px 7px;white-space:nowrap;background:#e0f2fe;border:1px solid #bce9ff;color:#0284c7}
.ppm-badge.new{background:#f2eafb;border-color:#e3d3fc;color:#6b21a8}
.ppm-badge.free{background:#22c55e;border-color:#22c55e;color:#fff}
.ppm-calendar-btn{flex:0 0 32px;border:1px solid #e1e7f1;border-radius:8px;background:#fff;width:32px;height:32px;font-size:14px;cursor:pointer}
.ppm-calendar-btn:hover{background:#eff6ff;border-color:#93c5fd}
.ppm-empty{color:#94a3b8;text-align:center;padding:30px 10px;font-size:12px}

/* POPUP DỰ ÁN: hai cột 50/50, không có Timeline. */
.ppm-views{height:min(56vh,490px);min-height:310px;overflow:hidden}
.ppm-toolbar{justify-content:flex-start}
.ppm-role{flex:0 0 auto}
.ppm-search{flex:0 1 340px!important}
.ppm-badge[hidden]{display:none!important}
.ppm-badge{background:#e0f2fe;color:#0284c7;border:1px solid #bae6fd}
.ppm-badge.new{background:#f2eafb;color:#6b21a8;border-color:#e3d3fc}
.ppm-pane-content{overflow-y:auto;overscroll-behavior:contain}
.ppm-person-title{overflow:hidden}

/* LICH MINI: CSS nguồn CtrlChonNhanVienTask bản cuối, giới hạn bên trong .ppm-root. */
.ppm-root{position:relative;isolation:isolate}
.ppm-root .tmp-calendar-pop[hidden]{display:none!important}
.ppm-root .tmp-calendar-pop{z-index:150}
    .ppm-root .mc-viewport{ position: relative; overflow: hidden; height: 310px; width: 100% !important; }
    .ppm-root .mc-weekdays, .ppm-root .mc-grid{ display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); width: 100% !important; }

    .ppm-root .mini-cal-wrap{ flex: 1; min-width: 0; display: flex; flex-direction: column; }
    .ppm-root .mini-cal{ flex: 1; min-width: 0; user-select: none; }
    .ppm-root .mc-header{ display: flex; align-items: center; justify-content: space-between; padding: 0 2px 4px 4px; }
    .ppm-root .mc-title{ font-size: 14px; font-weight: 700; color: #1e293b; }
    .ppm-root .mc-nav{ display: flex; gap: 2px; }
    .ppm-root .mc-nav-btn{ width: 30px; height: 26px; border: 0; background: transparent; border-radius: 6px; color: #64748b; font-size: 10px; line-height: 1; cursor: pointer; transition: background 0.15s, color 0.15s; }
    .ppm-root .mc-nav-btn:hover{ background: #eff6ff; color: #2563eb; }
    .ppm-root .mc-nav-btn:focus-visible{ outline: 2px solid #2563eb; outline-offset: 1px; }
    .ppm-root .mc-weekdays span{ text-align: center; font-size: 11px; font-weight: 700; color: #64748b; padding: 3px 0 5px; }
    .ppm-root .mc-weekdays span.mc-we{ color: #94a3b8; }
    
    /* VỀ LẠI 50PX CHO MỖI DÒNG LỊCH GỐC */
    .ppm-root .mc-grid{ grid-auto-rows: 50px; gap: 2px; }
    .ppm-root .mc-viewport .mc-grid{ position: absolute; top: 0; left: 0; right: 0; will-change: transform, opacity; }
    
    .ppm-root .mc-day{ display: flex; flex-direction: column; align-items: center; justify-content: center; border-radius: 6px; border: 1px solid transparent; box-sizing: border-box; font-size: 12px; color: #1e293b; cursor: default; min-width: 0; }
    .ppm-root .mc-day .mc-num{ font-weight: 600; line-height: 1; }
    .ppm-root .mc-day.out-month .mc-num{ color: #94a3b8; }
    .ppm-root .mc-day.out-range{ opacity: 0.32; }
    .ppm-root .mc-day.st-busy{ background: #fee2e2; color: #b91c1c; }
    .ppm-root .mc-day.st-holiday{ background: #fef3c7; color: #b45309; }
    .ppm-root .mc-day.st-weekend{ background: #f1f5f9; color: #64748b; }
    .ppm-root .mc-day.st-free{ background: #e6f4ea; color: #137333; }
    .ppm-root .mc-day.today{ border-color: #2563eb; box-shadow: inset 0 0 0 1px #2563eb; }
    
    .ppm-root .mc-label{ margin-top: 3px; max-width: 100%; padding: 0 3px; box-sizing: border-box; font-size: 9.5px; font-weight: 600; line-height: 1.1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.ppm-root .tmp-calendar-pop{position:absolute;z-index:100;min-width:240px;display:flex;flex-direction:column;overflow:visible;padding:10px;border:1px solid #d9c5ee;border-radius:10px;background:#fff;box-shadow:0 12px 35px rgba(28,18,58,.22);animation:tmp-pop-in .14s ease-out}
@keyframes tmp-pop-in{from{opacity:.5;transform:scale(.985)}to{opacity:1;transform:scale(1)}}
.ppm-root .tmp-calendar-pop:before, .ppm-root .tmp-calendar-pop:after{position:absolute;content:"";width:0;height:0;border-top:10px solid transparent;border-bottom:10px solid transparent;top:var(--tmp-arrow-y,50%);transform:translateY(-50%)}
.ppm-root .tmp-calendar-pop.tmp-from-left:before{left:-12px;border-right:12px solid #d9c5ee}
.ppm-root .tmp-calendar-pop.tmp-from-left:after{left:-10px;border-right:11px solid #fff}
.ppm-root .tmp-calendar-pop.tmp-from-right:before{right:-12px;border-left:12px solid #d9c5ee}
.ppm-root .tmp-calendar-pop.tmp-from-right:after{right:-10px;border-left:11px solid #fff}
.ppm-root .tmp-pop-head{display:flex;align-items:center;justify-content:space-between;gap:8px;min-height:28px;padding:0 1px 6px;border-bottom:1px solid #f1ebf7}
.ppm-root .tmp-pop-name{font-size:12.5px;font-weight:750;color:#25324a;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.ppm-root .tmp-pop-close{display:inline-flex;align-items:center;justify-content:center;width:24px;height:24px;border-radius:5px;border:0;background:#f7f2fd;color:#61309e;cursor:pointer;font-size:19px}
.ppm-root .tmp-pop-inner{min-height:0;overflow-y:auto;overflow-x:hidden}
.ppm-root .tmp-calendar-pop .mc-header{padding-top:5px}
.ppm-root .tmp-calendar-pop .mc-grid{grid-auto-rows:var(--tmp-day-size,36px);gap:2px}
.ppm-root .tmp-calendar-pop .mc-viewport{height:calc(var(--tmp-day-size,36px)*6 + 10px)}
.ppm-root .tmp-calendar-pop .mc-weekdays span{font-size:10px;padding:2px 0}
.ppm-root .tmp-calendar-pop .mc-day{font-size:11px}
.ppm-root .tmp-calendar-pop .mc-label{font-size:9px}
.ppm-root .tmp-calendar-pop .mc-day.has-current:after{content:"";width:5px;height:5px;border-radius:50%;background:#7c3aed;position:absolute;right:5px;bottom:4px}
.ppm-root .tmp-pop-help{display:flex;gap:8px;flex-wrap:wrap;align-items:center;padding:5px 1px 0;color:#64748b;font-size:10px}
.ppm-root .tmp-pop-help span::before{content:"";display:inline-block;width:8px;height:8px;border-radius:2px;vertical-align:middle;margin-right:4px;background:var(--tmp-legend)}
.ppm-root .tmp-pop-detail{display:none;min-height:0;overflow-y:auto;font-size:12px;line-height:1.5;color:#334155;padding-top:9px}
.ppm-root .tmp-calendar-pop.tmp-detail-mode .tmp-pop-calendar{display:none}
.ppm-root .tmp-calendar-pop.tmp-detail-mode .tmp-pop-detail{display:block}
.ppm-root .tmp-pop-detail-back{border:1px solid #e1d6ee;border-radius:6px;background:#faf7ff;color:#5b2189;padding:5px 9px;font-size:11.5px;cursor:pointer;margin-bottom:8px}
.ppm-root .tmp-pop-task{border-bottom:1px dashed #e2e8f0;padding:8px 0}
.ppm-root .tmp-pop-task:last-child{border:0}
.ppm-root .tmp-calendar-pop .mc-grid{grid-auto-rows:var(--tmp-day-size,48px)}
.ppm-root .tmp-calendar-pop .mc-viewport{height:calc(var(--tmp-day-size,48px)*6 + 10px)}
.ppm-root .tmp-calendar-pop .mc-day{font-size:12px}
.ppm-root .tmp-calendar-pop .mc-label{font-size:10px}
/* Dùng chính kích thước ô ngày được calendarPosition() của Task tính từ cột đối diện. */
.ppm-root .tmp-calendar-pop .tmp-pop-inner{flex:1 1 auto;min-height:0}
.ppm-root .tmp-calendar-pop .tmp-pop-calendar{min-height:0}
.ppm-root .tmp-calendar-pop .mc-day{position:relative!important;overflow:visible!important}
.ppm-root .tmp-calendar-pop .mc-day.has-tooltip{cursor:pointer}
.ppm-root .tmp-calendar-pop .mc-day:hover,.ppm-root .tmp-calendar-pop .mc-day.show-tooltip{z-index:50}
.ppm-root .tmp-calendar-pop .mc-viewport.is-animating{overflow:hidden}
.ppm-tooltip.global-task-tooltip{position:fixed!important;z-index:108020!important;display:block;max-width:min(520px,calc(100vw - 24px));min-width:min(340px,calc(100vw - 24px));max-height:min(410px,calc(100vh - 24px));width:max-content;overflow-y:auto;background:#fff;color:#334155;padding:11px 16px;border:1px solid #cbd5e1;border-radius:10px;box-shadow:0 10px 40px rgba(15,23,42,.2);font-size:12.5px;line-height:1.5;white-space:normal;word-break:break-word;overflow-wrap:anywhere;opacity:0;visibility:hidden;pointer-events:none;transition:opacity .15s ease,visibility .15s ease}
.ppm-tooltip.global-task-tooltip.show{opacity:1;visibility:visible;pointer-events:auto}
.ppm-tooltip.global-task-tooltip:after{content:"";position:absolute;top:100%;left:var(--arrow-left,50%);transform:translateX(-50%);border:8px solid transparent;border-top-color:#fff}
.ppm-tooltip.global-task-tooltip.tmp-tooltip-below:after{top:auto;bottom:100%;border-color:transparent transparent #fff transparent}
.ppm-tooltip .tmp-tooltip-heading{color:#334155;font-size:11px;font-weight:750;border-bottom:1px solid #e5e7eb;padding-bottom:5px;margin-bottom:4px}
.ppm-tooltip .tmp-other-heading{font-weight:750;color:#b45309;font-size:11px;margin:5px 0}
.ppm-tooltip .tooltip-task-list{list-style:none;margin:0;padding:0;text-align:left}
.ppm-tooltip .tooltip-task-list li{margin:0;padding:8px 0;border-bottom:1px dashed #e2e8f0;color:#334155}
.ppm-tooltip .tooltip-task-list li:last-child{border-bottom:0;padding-bottom:0}
.ppm-tooltip .tooltip-task-list li:first-child{padding-top:0}
.ppm-tooltip .t-project-name{font-size:10.5px;text-transform:uppercase;letter-spacing:.5px;margin-bottom:3px;font-weight:600;color:#64748b}
.ppm-tooltip .t-code{display:inline-block;color:#2563eb;font-weight:700;margin-right:6px}
@media(prefers-reduced-motion:reduce){.ppm-root .tmp-calendar-pop{animation:none}}
</style>
<SweetSoft:ExtraModal runat="server" ID="mdlMemberPicker" Type="Primary" Size="ExtraLarge">
<ContentTemplate>
<asp:UpdatePanel ID="upnlMemberPicker" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
<ContentTemplate>
<div class="ppm-root p-2" id="ppmRoot_<%= ClientID %>">
  <div class="ppm-header">
    <div class="ppm-info"><asp:Literal ID="ltrInfoNote" runat="server"/></div>
  </div>
  <div class="ppm-toolbar">
    <span class="ppm-filter-label">Lọc ứng viên</span>
    <asp:UpdatePanel runat="server" ID="upnlSearchDefault" UpdateMode="Conditional"><ContentTemplate>
      <asp:Panel runat="server" ID="pnlSearchDefault" CssClass="ppm-role">
        <SweetSoft:BootstrapDropdown ID="ddlSearchChucDanh" runat="server" Text="Chức Danh" AutoPostBack="true" AllowClear="true" SearchColumn="IdChucDanh" EnableSearch="true" ValueIsOfTypeGUID="True" SearchPlaceholder="Tìm kiếm theo tên chức danh..." NoResultsText="Không tìm thấy tên chức danh" OnSelectedValueChanged="ddlSearchChucDanh_SelectedValueChanged"></SweetSoft:BootstrapDropdown>
      </asp:Panel>
    </ContentTemplate></asp:UpdatePanel>
    <asp:Panel runat="server" DefaultButton="lbtSearchSingle" CssClass="input-group ppm-search">
      <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
      <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
    </asp:Panel>
    <div class="ppm-tags"><div class="listSearchTagBox"><asp:UpdatePanel ID="upSearchTagBox" runat="server" UpdateMode="Conditional"><ContentTemplate>
      <SweetSoft:ExtraSearchBox ID="searchTagBox" runat="server" OnTagClosed="searchTagBox_TagClosed"></SweetSoft:ExtraSearchBox>
    </ContentTemplate></asp:UpdatePanel></div></div>
  </div>
  <asp:UpdatePanel ID="upMemberList" runat="server" UpdateMode="Conditional"><ContentTemplate>
    <asp:HiddenField ID="hfPickedClient" runat="server"/>
    <asp:HiddenField ID="hfOriginalClient" runat="server"/>
    <asp:HiddenField ID="hfMiniUser" runat="server"/>
    <asp:HiddenField ID="hfMiniMonth" runat="server"/>
    <asp:HiddenField ID="hfMiniPayload" runat="server"/>
    <asp:LinkButton ID="btnLoadMiniMonth" runat="server" OnClick="btnLoadMiniMonth_Click" Style="display:none" CausesValidation="false"/>
    <div class="ppm-views">
      <div class="ppm-list">
        <div class="ppm-pane">
          <div class="ppm-pane-head selected"><span><i class="fas fa-user-check me-1"></i> Thành viên được chọn</span><span class="ppm-count" data-ppm-count="selected">0</span></div>
          <div class="ppm-pane-content" data-ppm-list="selected"><div class="ppm-empty">Chưa chọn nhân viên cho dự án.</div></div>
        </div>
        <div class="ppm-pane">
          <div class="ppm-pane-head"><span><i class="fas fa-users me-1"></i> Có thể thêm vào dự án</span><span class="ppm-count" data-ppm-count="candidate">0</span></div>
          <div class="ppm-pane-content" data-ppm-list="candidate"></div>
        </div>
      </div>
    </div>
    <div data-ppm-source style="display:none" aria-hidden="true">
      <asp:Repeater ID="rptCompanyMembers" runat="server" OnItemDataBound="rptCompanyMembers_ItemDataBound"><ItemTemplate>
        <div class="ppm-row" data-ppm-id='<%# Eval("UserId") %>' data-ppm-original='<%# Convert.ToBoolean(Eval("IsOriginal")) ? "1" : "0" %>' data-ppm-taskcount='<%# Eval("TaskCount") %>'>
          <div class="ppm-checkbox"><asp:CheckBox ID="chkSelect" runat="server"/></div>
          <asp:HiddenField ID="hdfUserId" runat="server" Value='<%# Eval("UserId") %>'/>
          <span class="ppm-avatar"><%# Eval("AvatarHtml") %></span>
          <span class="ppm-person">
            <span class="ppm-person-title"><b><%#: Eval("DisplayName") %></b><span class="ppm-role-label"><%#: Eval("ChucDanh") %></span></span>
            <span class="ppm-person-email" title='<%#: Eval("Email") %>'><%#: Eval("Email") %></span>
          </span>
          <span class="ppm-badge" data-ppm-badge></span>
          <button type="button" class="ppm-calendar-btn" data-ppm-calendar title="Xem lịch tháng">📅</button>
        </div>
      </ItemTemplate></asp:Repeater>
      <asp:Literal ID="ltrCountCompany" runat="server" Visible="false"/>
      <asp:Panel ID="pnlNoData" runat="server" Visible="false"><asp:Literal ID="ltrNoData" runat="server"/></asp:Panel>
    </div>
  </ContentTemplate></asp:UpdatePanel>
  <div class="ppm-footer-summary" data-ppm-summary></div>
<div class="tmp-calendar-pop" id="ppmMini_<%= ClientID %>" role="dialog" aria-label="Lịch làm việc của nhân viên" hidden>
  <div class="tmp-pop-head"><span class="tmp-pop-name" data-ppm-title></span><button type="button" class="tmp-pop-close" data-ppm-pop-close aria-label="Đóng lịch">&times;</button></div>
  <div class="tmp-pop-inner"><div class="tmp-pop-calendar">
    <div class="mini-cal" data-ppm-calendar-body></div>
    <div class="tmp-pop-help"><span style="--tmp-legend:#fee2e2">Bận</span><span style="--tmp-legend:#e6f4ea">Trống</span><span style="--tmp-legend:#fef3c7">Nghỉ lễ</span><span style="--tmp-legend:#f1f5f9">Nghỉ tuần</span></div>
  </div></div>
</div>
</div>
</ContentTemplate></asp:UpdatePanel>
</ContentTemplate>
<FooterTemplate><asp:UpdatePanel runat="server" UpdateMode="Conditional"><ContentTemplate>
  <SweetSoft:ExtraButton runat="server" ID="btnConfirm" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClick="btnConfirm_Click"><%= GetResourceText(BackEndResourceKeys.CONFIRM) %></SweetSoft:ExtraButton>
</ContentTemplate></asp:UpdatePanel></FooterTemplate>
</SweetSoft:ExtraModal>
<div class="ppm-tooltip global-task-tooltip" id="ppmTooltip_<%= ClientID %>"></div>
<script type="text/javascript">
(function(){
    var rootId='ppmRoot_<%= ClientID %>';
    var popupId='ppmMini_<%= ClientID %>';
    var tooltipId='ppmTooltip_<%= ClientID %>';
    var stateMap=window.__projectPickerState=window.__projectPickerState||{};
    var state=stateMap[rootId]||(stateMap[rootId]={mini:null,pinned:null});
    var fields={picked:'hfPickedClient',original:'hfOriginalClient',miniUser:'hfMiniUser',miniMonth:'hfMiniMonth',miniPayload:'hfMiniPayload',miniButton:'btnLoadMiniMonth'};
    function root(){return document.getElementById(rootId)}
    function field(name){var r=root();return r?r.querySelector('[id$="'+fields[name]+'"]'):null}
    function value(name){var f=field(name);return f?f.value:''}
    function encode(s){return String(s==null?'':s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;').replace(/'/g,'&#39;')}
    function parse(s){try{return JSON.parse(s||'{}')||{}}catch(e){return {}}}
    function picked(){return new Set(value('picked').split(',').filter(Boolean))}
    function savePicked(set){var f=field('picked');if(f)f.value=Array.from(set).join(',')}
    function userRows(){var r=root();return r?Array.from(r.querySelectorAll('[data-ppm-list] > .ppm-row[data-ppm-id], [data-ppm-source] > .ppm-row[data-ppm-id]')):[]}
    function personName(row){var e=row.querySelector('.ppm-person-title b');return e?e.textContent.trim():'Nhân viên'}
    function badge(row,chosen){
        var b=row.querySelector('[data-ppm-badge]');if(!b)return;
        var old=row.dataset.ppmOriginal==='1',num=parseInt(row.dataset.ppmTaskcount||'0',10)||0;
        b.className='ppm-badge'+(chosen&&!old?' new':'');
        if(!chosen){b.textContent='';b.hidden=true;b.removeAttribute('title');return}
        b.hidden=false;
        b.textContent=old?(num>0?'Đảm nhận '+num+' công việc':'Đã tham gia'):'Sẽ tham gia';
        b.title=old?(num>0?'Thành viên đã tham gia dự án, hiện có '+num+' công việc':'Thành viên đã tham gia dự án'):'Thành viên vừa được chọn, chưa lưu';
    }
    function updateSummary(){
        var r=root();if(!r)return;
        var original=new Set(value('original').split(',').filter(Boolean)),selected=picked(),keep=0,add=0,remove=0;
        selected.forEach(function(id){if(original.has(id))keep++;else add++});
        original.forEach(function(id){if(!selected.has(id))remove++});
        var target=r.querySelector('[data-ppm-summary]');if(target)target.textContent=selected.size+' thành viên sau lưu · Giữ '+keep+' · Thêm '+add+' · Gỡ '+remove;
    }
    function renderList(){
        var r=root();if(!r)return;
        var left=r.querySelector('[data-ppm-list="selected"]'),right=r.querySelector('[data-ppm-list="candidate"]');
        if(!left||!right)return;
        var scrollLeft=left.scrollTop,scrollRight=right.scrollTop,selected=picked();
        r.querySelectorAll('.ppm-empty').forEach(function(e){e.remove()});
        userRows().forEach(function(row){
            var isSelected=selected.has(row.dataset.ppmId),cb=row.querySelector('input[type="checkbox"]');
            if(cb)cb.checked=isSelected;
            row.classList.toggle('is-picked',isSelected);
            badge(row,isSelected);
            (isSelected?left:right).appendChild(row);
        });
        if(!left.querySelector('.ppm-row'))left.innerHTML='<div class="ppm-empty">Chưa chọn nhân viên cho dự án.</div>';
        if(!right.querySelector('.ppm-row'))right.innerHTML='<div class="ppm-empty">Không có ứng viên phù hợp.</div>';
        left.scrollTop=scrollLeft;right.scrollTop=scrollRight;
        r.querySelectorAll('[data-ppm-count]').forEach(function(e){
            e.textContent=r.querySelectorAll('[data-ppm-list="'+e.dataset.ppmCount+'"] > .ppm-row').length;
        });
        updateSummary();
    }
    function closeTip(){var t=document.getElementById(tooltipId);if(t)t.classList.remove('show','tmp-tooltip-below');state.pinned=null;var p=document.getElementById(popupId);if(p)p.querySelectorAll('.mc-day.show-tooltip').forEach(function(e){e.classList.remove('show-tooltip')})}
    function closeMini(resetFields){
        var p=document.getElementById(popupId);if(p){p.hidden=true;p.classList.remove('show','tmp-from-left','tmp-from-right');}
        state.mini=null;closeTip();
        if(resetFields){['miniUser','miniMonth','miniPayload'].forEach(function(k){var f=field(k);if(f)f.value=(k==='miniPayload'?'{}':'')})}
    }
    function projectNotifyVisible(){var n=document.getElementById('modal-notify');return !!(n&&(n.classList.contains('show')||n.getAttribute('aria-hidden')==='false'||(n.style.display==='block')))}
    function dateFromKey(k){var s=k.split('-');return new Date(+s[0],+s[1]-1,+s[2])}
    function dayKey(d){return d.getFullYear()+'-'+('0'+(d.getMonth()+1)).slice(-2)+'-'+('0'+d.getDate()).slice(-2)}
    function dateShort(k){var p=k.split('-');return p[2]+'/'+p[1]+'/'+p[0]}
    function monthKey(d){return d.getFullYear()+'-'+('0'+(d.getMonth()+1)).slice(-2)}
    // Cùng tooltip-task-list, tmp-tooltip-heading và cú pháp TaskMemberCalendarEngine.
    // Chỉ khác nghiệp vụ: nhóm công việc thuộc dự án hiện tại hay dự án khác.
    function taskDayTooltip(info,key,employee,outside){
        if(!info)return '';
        var tasks=info.tasks||[],own=tasks.filter(function(t){return t.currentProject}),other=tasks.filter(function(t){return !t.currentProject});
        var p=key.split('-');
        var html='<div class="tmp-tooltip-heading">'+(employee?encode(employee)+' · ':'Ngày ')+p[2]+'/'+p[1]+'/'+p[0]+'</div>';
        function group(items,heading){
            if(!items.length)return '';
            var s='<div class="tmp-other-heading">'+heading+' ('+items.length+')</div><ul class="tooltip-task-list">';
            items.forEach(function(t){s+='<li><div class="t-project-name"><i class="fas fa-cube me-1"></i>'+encode(t.projectName||'')+'</div><span class="t-code">['+encode(t.code||'')+']</span>'+encode(t.name||'')+'</li>'});
            return s+'</ul>';
        }
        html+=group(own,'Công việc thuộc dự án này')+group(other,'Công việc thuộc dự án khác');
        if(!tasks.length)html+='<div style="font-size:11.5px;color:#64748b">'+encode(info.text||'Chưa có lịch công việc')+'</div>';
        return html;
    }
    // Các hàm dựng ô và header lấy đúng từ Task; dữ liệu dự án chỉ đổi ở taskDayTooltip().
    function toKey(y,m,d){return y+'-'+('0'+(m+1)).slice(-2)+'-'+('0'+d).slice(-2)}
    function escAttr(s){return encode(s)}
    function titleText(st){return 'Tháng '+(st.month+1)+', '+st.year}
    var WEEKDAYS=['T2','T3','T4','T5','T6','T7','CN'];
        function buildGridHtml(st) {
            var now = new Date();
            var todayKey = toKey(now.getFullYear(), now.getMonth(), now.getDate());
            var offset = (new Date(st.year, st.month, 1).getDay() + 6) % 7;
            var html = '<div class="mc-grid">';

            for (var i = 0; i < 42; i++) {
                var d = new Date(st.year, st.month, 1 - offset + i);
                var key = toKey(d.getFullYear(), d.getMonth(), d.getDate());
                var inRange = !!st.minKey && key >= st.minKey && key <= st.maxKey;
                var info = inRange ? st.data[key] : null;

                var cls = 'mc-day';
                if (d.getMonth() !== st.month) cls += ' out-month';
                if (!inRange) cls += ' out-range';
                else if (info && info.status) cls += ' st-' + info.status;
                // Có task hiện tại mà không vướng task khác: màu tím, không ghi là 'Trống'.
                if (info && info.current && info.status === 'free') cls += ' st-current-only';
                if (key === todayKey) cls += ' today';
                if (info && info.current) cls += ' has-current';

                var extra = '';
                var tooltipHtml = '';
                var clickAttr = '';

                if (info) {
                    var label = info.current && info.status === 'free' ? 'Task này' : String(info.text || '').replace(/^[^A-Za-z0-9\u00C0-\u1EF9]+/, '');
                    if (label) extra = '<span class="mc-label">' + escAttr(label) + '</span>';
                    var hasOthers = !!(info.tasks && info.tasks.length);
                    var hasCurrent = !!info.current;
                    if (hasOthers || hasCurrent) {
                        cls += ' has-tooltip';
                        var listHtml = taskDayTooltip(info, key, '', false);
                        clickAttr = ' data-tooltip-content="' + encodeURIComponent(listHtml) + '"';
                    } else if (info.text) {
                        clickAttr = ' title="' + escAttr(d.getDate() + '/' + (d.getMonth() + 1) + ' - ' + info.text) + '"';
                    }
                }
                html += '<div class="' + cls + '" data-day-key="' + key + '"' + clickAttr + '><span class="mc-num">' + d.getDate() + '</span>' + extra + '</div>';
            }
            return html + '</div>';
        }
        function renderMiniCalendar(cal) {
            var st = cal.data('mc');
            var html = '<div class="mc-header">' +
                '<span class="mc-title">' + titleText(st) + '</span>' +
                '<div class="mc-nav">' +
                '<button type="button" class="mc-nav-btn" title="Tháng trước" data-ppm-month-change="-1">&#9650;</button>' +
                '<button type="button" class="mc-nav-btn" title="Tháng sau" data-ppm-month-change="1">&#9660;</button>' +
                '</div></div>';

            html += '<div class="mc-weekdays">';
            for (var w = 0; w < 7; w++) {
                html += '<span' + (w >= 5 ? ' class="mc-we"' : '') + '>' + WEEKDAYS[w] + '</span>';
            }
            html += '</div>';

            html += '<div class="mc-viewport">' + buildGridHtml(st) + '</div>';
            cal.html(html);
        }

    function transitionMonth(cal,oldMonth){
        var st=cal.data('mc'),viewport=cal.find('.mc-viewport');
        if(!viewport.length||oldMonth==null||oldMonth===st.year+'-'+st.month){renderMiniCalendar(cal);return;}
        closeTip();viewport.addClass('is-animating');cal.find('.mc-title').text('Tháng '+(st.month+1)+', '+st.year);
        var oldGrid=viewport.children('.mc-grid').last(),newGrid=$(buildGridHtml(st));viewport.append(newGrid);
        var prev=oldMonth.split('-'),oldIndex=(+prev[0])*12+(+prev[1]),newIndex=st.year*12+st.month,dir=newIndex>oldIndex?1:-1;
        var reduceMotion=window.matchMedia&&window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        if(reduceMotion||!newGrid[0].animate){oldGrid.remove();viewport.removeClass('is-animating');return;}
        var opts={duration:260,easing:'cubic-bezier(0.22, 1, 0.36, 1)',fill:'both'};
        oldGrid[0].animate([{transform:'translateY(0)',opacity:1},{transform:'translateY('+(-dir*40)+'%)',opacity:0}],opts).onfinish=function(){oldGrid.remove()};
        newGrid[0].animate([{transform:'translateY('+(dir*40)+'%)',opacity:0},{transform:'translateY(0)',opacity:1}],opts).onfinish=function(){viewport.removeClass('is-animating')};
    }
    function showTip(cell){
        if(projectNotifyVisible())return;
        var raw=cell.getAttribute('data-tooltip-content');if(!raw)return;
        var el=document.getElementById(tooltipId);if(!el)return;
        try{el.innerHTML=decodeURIComponent(raw)}catch(e){return}
        el.classList.add('show');
        var rect=cell.getBoundingClientRect(),w=el.offsetWidth,h=el.offsetHeight;
        var left=Math.max(10,Math.min(innerWidth-w-10,rect.left+rect.width/2-w/2));
        var top=rect.top-h-10,below=false;
        if(top<10){top=Math.max(10,Math.min(innerHeight-h-10,rect.bottom+10));below=true}
        el.classList.toggle('tmp-tooltip-below',below);
        el.style.left=left+'px';el.style.top=top+'px';
        el.style.setProperty('--arrow-left',Math.max(5,Math.min(95,(rect.left+rect.width/2-left)/w*100))+'%');
    }
    function requestMini(id,month){
        if(projectNotifyVisible())return;
        var user=field('miniUser'),mo=field('miniMonth'),payload=field('miniPayload'),button=field('miniButton');
        if(!user||!mo||!payload||!button)return;
        if(!state.mini||state.mini.id!==id)closeMini(false);else closeTip();user.value=id;mo.value=month;payload.value='{}';state.mini={id:id,month:month};
        button.click();
    }
    // Thuật toán calendarPosition() từ CtrlChonNhanVienTask: đo CHÍNH CỘT ĐỐI DIỆN.
    function calendarPosition(button){
        var r=root(),pop=document.getElementById(popupId);
        if(!r||!pop||!button||!document.contains(button))return;
        var ownPanel=button.closest('.ppm-pane');
        var panes=r.querySelectorAll('.ppm-list > .ppm-pane');
        if(!ownPanel||panes.length!==2)return;
        var other=ownPanel===panes[0]?panes[1]:panes[0];
        var ownList=button.closest('.ppm-pane-content');
        var br=button.getBoundingClientRect(),pr=other.getBoundingClientRect(),rr=r.getBoundingClientRect();
        if(ownList){var lr=ownList.getBoundingClientRect();if(br.bottom<lr.top||br.top>lr.bottom){closeMini(false);return}}
        var left=pr.left-rr.left+5,top=pr.top-rr.top+5,width=Math.max(215,pr.width-10),height=Math.max(235,pr.height-10);
        pop.style.left=Math.round(left)+'px';pop.style.top=Math.round(top)+'px';
        pop.style.width=Math.round(width)+'px';pop.style.height=Math.round(height)+'px';
        pop.classList.toggle('tmp-from-left',ownPanel===panes[0]);
        pop.classList.toggle('tmp-from-right',ownPanel!==panes[0]);
        pop.style.setProperty('--tmp-arrow-y',Math.max(16,Math.min(height-16,br.top+br.height/2-pr.top))+'px');
        var available=height-125;
        var size=Math.max(33,Math.min(59,Math.floor((available-10)/6)));
        pop.style.setProperty('--tmp-day-size',size+'px');
    }
    function renderMini(id,month,json){
        if(projectNotifyVisible()){closeMini(true);return}
        var row=userRows().find(function(r){return r.dataset.ppmId===id});
        var pop=document.getElementById(popupId);if(!row||!pop)return;
        var button=row.querySelector('[data-ppm-calendar]');if(!button)return;
        var first=dateFromKey(month+'-01'),y=first.getFullYear(),m=first.getMonth(),days=parse(json);
        pop.querySelector('[data-ppm-title]').textContent='Lịch · '+personName(row);
        var cal=$(pop.querySelector('[data-ppm-calendar-body]'));
        var offset=(first.getDay()+6)%7,lo=dayKey(new Date(y,m,1-offset)),hi=dayKey(new Date(y,m,42-offset));
        var previous=cal.data('mc'),oldMonth=previous?(previous.year+'-'+previous.month):null;
        cal.data('mc',{data:days,minKey:lo,maxKey:hi,year:y,month:m});
        if(pop.classList.contains('show')&&oldMonth)transitionMonth(cal,oldMonth);
        else renderMiniCalendar(cal);
        pop.hidden=false;pop.classList.add('show');
        calendarPosition(button);
        state.mini={id:id,month:month};
    }
    function init(){
        var r=root();if(!r)return;
        // Close popover when a new picker session starts; the server clears mini fields on OpenPicker/BindData.
        renderList();
        if(!projectNotifyVisible()&&value('miniUser')&&value('miniMonth')&&value('miniPayload')!=='{}')renderMini(value('miniUser'),value('miniMonth'),value('miniPayload'));
        else if(projectNotifyVisible())closeMini(true);
        else if(!value('miniUser')||!value('miniMonth'))closeMini(false);
    }
    if(!window.__ppmProjectListeners)window.__ppmProjectListeners={};
    if(!window.__ppmProjectListeners[rootId]){
        window.__ppmProjectListeners[rootId]=true;
        document.addEventListener('change',function(e){
            var r=root(),cb=e.target.closest('.ppm-row input[type="checkbox"]');
            if(!r||!cb||!r.contains(cb))return;
            var row=cb.closest('.ppm-row'),set=picked();if(!row)return;
            if(cb.checked)set.add(row.dataset.ppmId);else set.delete(row.dataset.ppmId);
            savePicked(set);renderList();closeTip();
        });
        document.addEventListener('click',function(e){
            var r=root();if(!r)return;
            var cal=e.target.closest('[data-ppm-calendar]');
            if(cal&&r.contains(cal)){e.preventDefault();e.stopPropagation();var row=cal.closest('.ppm-row');if(row)requestMini(row.dataset.ppmId,monthKey(new Date()));return}
            if(!e.target.closest('#'+popupId)&&!e.target.closest('#'+tooltipId)){closeMini(false)}
        });
        document.addEventListener('click',function(e){
            var pop=document.getElementById(popupId);
            if(!pop||!pop.contains(e.target))return;
            e.stopPropagation();
            if(e.target.closest('[data-ppm-pop-close]')){closeMini(true);return}
            var nav=e.target.closest('[data-ppm-month-change]');
            var d=nav?parseInt(nav.getAttribute('data-ppm-month-change'),10):0;
            if(d&&state.mini){var now=dateFromKey(state.mini.month+'-01');now.setMonth(now.getMonth()+d);requestMini(state.mini.id,monthKey(now));return}
            var cell=e.target.closest('.mc-day.has-tooltip');
            if(cell){if(state.pinned===cell){closeTip();return}closeTip();state.pinned=cell;cell.classList.add('show-tooltip');showTip(cell)}
        });
        document.addEventListener('mouseover',function(e){
            var pop=document.getElementById(popupId),cell=e.target.closest('.mc-day.has-tooltip');
            if(pop&&cell&&pop.contains(cell)&&!state.pinned)showTip(cell);
        });
        document.addEventListener('mouseout',function(e){
            var pop=document.getElementById(popupId),cell=e.target.closest('.mc-day.has-tooltip');
            if(pop&&cell&&pop.contains(cell)&&!state.pinned&&!(e.relatedTarget&&cell.contains(e.relatedTarget)))closeTip();
        });
        document.addEventListener('click',function(e){
            var t=document.getElementById(tooltipId);
            if(t&&!t.contains(e.target)&&!e.target.closest('.mc-day.has-tooltip'))closeTip();
        });
        document.addEventListener('scroll',function(e){
            var r=root();if(r&&r.contains(e.target))closeMini(false);
        },true);
        // This runs in capture phase BEFORE the WebForms save postback, so an open calendar cannot overlay MessageBox.
        document.addEventListener('click',function(e){
            var save=e.target.closest('[id$="btnConfirm"]');
            if(!save)return;
            var parentModal=save.closest('.modal');
            if(save.id==='<%= btnConfirm.ClientID %>'||(parentModal&&parentModal.contains(root())))closeMini(true);
        },true);
        // SweetSoft MessageBox may be opened by async postback; also watch its visibility.
        function bindNotify(){
            var notify=document.getElementById('modal-notify');if(!notify||notify.__ppmObserved)return;
            notify.__ppmObserved=true;
            new MutationObserver(function(){if(projectNotifyVisible())closeMini(true)}).observe(notify,{attributes:true,attributeFilter:['class','style','aria-hidden']});
        }
        document.addEventListener('show.bs.modal',function(e){if(e.target&&e.target.id==='modal-notify')closeMini(true)});
        if(typeof Sys!=='undefined'&&Sys.Application&&Sys.Application.add_load){Sys.Application.add_load(function(){bindNotify();init()})}
        window.addEventListener('resize',function(){closeMini(false)});
        if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',function(){bindNotify();init()});
        else{bindNotify();init()}
    }
})();

</script>
<script type="text/javascript">
    (function () {
        var BASE_Z = 106000;

        // Giữ hành vi cũ: 2 popup nghiệp vụ chồng nhau (không tính MessageBox)
        function stackBusinessModals() {
            var $modals = $('.modal:visible').not('#modal-notify');
            if ($modals.length > 1) {
                $('.modal-backdrop').each(function (i) {
                    $(this).css('z-index', BASE_Z + (i * 10));
                });
                $modals.each(function (i) {
                    $(this).css('z-index', BASE_Z + (i * 10) + 5);
                });
            }
        }

        // Đẩy MessageBox hệ thống lên trên cùng, chỉ khi đang có popup khác
        function raiseNotify() {
            var notify = document.getElementById('modal-notify');
            if (!notify || !$(notify).is(':visible')) return;

            var $others = $('.modal:visible').not(notify);
            if ($others.length === 0) return; // MessageBox dùng đơn lẻ -> không đụng

            var maxZ = BASE_Z;
            $others.each(function () {
                maxZ = Math.max(maxZ, parseInt(window.getComputedStyle(this).zIndex, 10) || 0);
            });

            notify.style.setProperty('z-index', String(maxZ + 20), 'important');
            var bds = document.querySelectorAll('.modal-backdrop');
            if (bds.length) bds[bds.length - 1].style.setProperty('z-index', String(maxZ + 15), 'important');
        }

        function raiseNotifyRepeatedly() {
            [0, 100, 250, 500, 900].forEach(function (ms) {
                setTimeout(function () { stackBusinessModals(); raiseNotify(); }, ms);
            });
        }

        // 1) Bám sự kiện Bootstrap của chính MessageBox (không phụ thuộc mốc thời gian)
        ['show.bs.modal', 'shown.bs.modal'].forEach(function (evt) {
            document.addEventListener(evt, function (e) {
                if (e.target && e.target.id === 'modal-notify') raiseNotifyRepeatedly();
            });
        });

        // 2) Trả z-index về mặc định khi MessageBox đóng
        document.addEventListener('hidden.bs.modal', function (e) {
            if (e.target && e.target.id === 'modal-notify') {
                e.target.style.removeProperty('z-index');
            }
        });

        // 3) Dự phòng: sau mỗi postback ajax
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(raiseNotifyRepeatedly);
        }
    })();
</script>
