<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="MeetList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.MeetList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fMeets/Controls/CtrlMeet.ascx" TagPrefix="SweetSoft" TagName="CtrlMeet" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
    .employee-scroll-container::-webkit-scrollbar { width: 6px; }
    .employee-scroll-container::-webkit-scrollbar-track { background: #f1f5f9; border-radius: 6px; }
    .employee-scroll-container::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 6px; }
    .employee-scroll-container::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
    .member-item-row { position: relative; background: white; border: 1px solid #e2e8f0; border-radius: 8px; margin-bottom: 8px; overflow: hidden; display: flex; flex-direction: column; align-items: stretch; transition: border-color .2s, box-shadow .2s; cursor: pointer; }
    .member-item-row:hover { border-color: #93c5fd; background-color: #f8fafc; }
    .member-item-row:last-child { margin-bottom: 0; }
    .row-default-view { display: flex; align-items: center; padding: 8px 12px; width: 100%; min-height: 52px; box-sizing: border-box; }
    .member-info-group { display: flex; align-items: center; gap: 12px; width: 100%; min-width: 0; }
    .member-info-group input[type="checkbox"] { width: 18px; height: 18px; cursor: pointer; accent-color: #2563eb; margin: 0; border-radius: 4px; border: 1px solid #cbd5e1; }
    .single-avatar-circle { width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 12px; font-weight: 700; color: #fff; flex-shrink: 0; box-shadow: 0 1px 3px rgba(0,0,0,.1); }
    .member-name-block { display: flex; flex-direction: column; min-width: 0; line-height: 1.25; }
    .member-name-block .fw-bold { font-size: 14px; font-weight: 600 !important; color: #1e293b; }
    .member-email { font-size: 12px; color: #64748b; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; margin-top: 3px; }
    .member-email:empty { display: none; }
    .record-attachments .file-actions { display: none !important; }
    #<%= dlDetail.ClientID %> .modal-dialog { max-width: 1180px; width: calc(100vw - 32px); margin: 1rem auto; }
    #<%= dlDetail.ClientID %> .modal-body { padding: 18px 20px 14px; background: #f8fafc; }
    #<%= dlDetail.ClientID %> .modal-footer { padding: 12px 20px 16px; border-top: 1px solid #e2e8f0; background: #fff; }
    .meeting-detail-layout { align-items: stretch; }
    .meeting-form-column { height: 100%; }
    .meeting-form-card { height: 100%; display: flex; flex-direction: column; background: #fff; border: 1px solid #e2e8f0; border-radius: 14px; padding: 15px; box-shadow: 0 4px 14px rgba(15,23,42,.04); }
    .meeting-form-card-title { display: flex; align-items: center; gap: 8px; margin-bottom: 13px; padding-bottom: 10px; border-bottom: 1px solid #eef2f7; font-size: 12px; font-weight: 800; color: #334155; text-transform: uppercase; letter-spacing: .35px; }
    .meeting-form-card-title i { color: #6366f1; font-size: 13px; }
    .meeting-form-field { margin-bottom: 12px; }
    .meeting-form-field:last-child { margin-bottom: 0; }
    .meeting-form-field .form-label { margin-bottom: 6px; font-size: 11px; font-weight: 700; color: #475569; }
    .meeting-form-field .form-text { margin-top: 6px; font-size: 10px; color: #94a3b8; }
    .meeting-summary-box { display: flex; align-items: center; gap: 10px; min-height: 42px; padding: 7px 10px; border: 1px solid #e2e8f0; border-radius: 10px; background: #f8fafc; }
    .meeting-summary-box i { color: #6366f1; font-size: 13px; }
    .meeting-summary-box strong { color: #334155; font-size: 12px; }
    .meeting-summary-box span { color: #64748b; font-size: 10px; }
    .meeting-description-card { display: flex; flex-direction: column; }
    .meeting-description-editor { min-height: 340px; }
    .meeting-description-editor .cke { border-radius: 10px; overflow: hidden; border: 1px solid #dbe3ec; }
    .meeting-description-editor .cke_contents { min-height: 275px !important; }
    .meeting-description-editor .cke_top { border-radius: 10px 10px 0 0; }
    .meeting-participant-box .input-group > .form-control { min-height: 54px; }
    .meeting-participant-row { flex: 1 1 auto; min-height: 0; }
    .meeting-participant-row > .col-12 { display: flex; flex-direction: column; min-height: 0; }
    .meeting-participant-row .meeting-form-field { margin-bottom: 0; flex: 1 1 auto; min-height: 0; display: flex; flex-direction: column; }
    .selected-meeting-members-box { border: 1px solid #e2e8f0; border-radius: 10px; background: #f8fafc; padding: 8px; }
    .selected-meeting-members-list { display: flex; flex-direction: column; gap: 6px; max-height: 168px; overflow-y: auto; }
    .meeting-participant-row .selected-meeting-members-box { flex: 1 1 auto; min-height: 180px; overflow: hidden; }
    .meeting-participant-row .selected-meeting-members-list { display: grid; grid-template-columns: repeat(2, minmax(0,1fr)); gap: 6px; max-height: none; height: 100%; overflow-y: auto; align-content: start; }
    .selected-meeting-member-row { display: flex; align-items: center; gap: 10px; padding: 7px 9px; background: #fff; border: 1px solid #e2e8f0; border-radius: 9px; min-width: 0; }
    .selected-meeting-member-avatar { width: 34px; height: 34px; flex: 0 0 34px; display: flex; align-items: center; justify-content: center; }
    .selected-meeting-member-avatar .single-avatar-circle { width: 34px; height: 34px; }
    .selected-meeting-member-info { min-width: 0; line-height: 1.25; }
    .selected-meeting-member-name { font-size: 12px; font-weight: 700; color: #1e293b; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .selected-meeting-member-email { font-size: 10.5px; color: #64748b; margin-top: 2px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .selected-meeting-members-empty { min-height: 58px; display: flex; align-items: center; justify-content: center; gap: 7px; border: 1px dashed #cbd5e1; border-radius: 9px; color: #94a3b8; font-size: 11px; background: #fff; }
    .selected-meeting-members-empty i { color: #94a3b8; }
    .meeting-readonly-input { background: #f8fafc !important; color: #475569 !important; }
    .meeting-time-picker, .meeting-duration-picker { position: relative; }
    .meeting-input-shell { position: relative; }
    .meeting-picker-trigger { position: absolute; top: 50%; right: 10px; transform: translateY(-50%); border: 0; background: transparent; color: #64748b; width: 34px; height: 34px; border-radius: 8px; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; transition: all .2s ease; z-index: 2; }
    .meeting-picker-trigger:hover { background: #eef2ff; color: #4f46e5; }
    .meeting-picker-input { padding-right: 46px !important; cursor: pointer; }
    .meeting-picker-popover { position: absolute; top: calc(100% + 8px); left: 0; width: min(390px, calc(100vw - 40px)); background: #fff; border: 1px solid #dbe3ec; border-radius: 14px; box-shadow: 0 16px 40px rgba(15,23,42,.14); padding: 13px; z-index: 9999; display: none; }
    .meeting-picker-popover.open { display: block; animation: meetingPickerIn .16s ease both; }
    .meeting-picker-title { display: flex; align-items: center; justify-content: space-between; gap: 8px; margin-bottom: 10px; }
    .meeting-picker-title strong { font-size: 12px; color: #334155; }
    .meeting-picker-title span { font-size: 10px; color: #94a3b8; }
    .meeting-time-selected { display: flex; align-items: center; justify-content: center; gap: 7px; min-height: 52px; background: linear-gradient(180deg,#f8fafc,#f1f5f9); border: 1px solid #e2e8f0; border-radius: 12px; margin-bottom: 10px; }
    .meeting-time-selected .selected-time { font-size: 28px; line-height: 1; font-weight: 800; letter-spacing: .8px; color: #312e81; font-variant-numeric: tabular-nums; }
    .meeting-time-section { margin-top: 10px; }
    .meeting-time-section-label { font-size: 10px; color: #64748b; font-weight: 700; margin: 0 0 7px 2px; text-transform: uppercase; letter-spacing: .4px; }
    .meeting-hour-grid { display: grid; grid-template-columns: repeat(8, minmax(0,1fr)); gap: 6px; }
    .meeting-minute-grid { display: grid; grid-template-columns: repeat(4, minmax(0,1fr)); gap: 7px; }
    .meeting-time-choice { border: 1px solid #e2e8f0; background: #fff; color: #475569; border-radius: 9px; min-height: 34px; padding: 5px 4px; font-size: 11px; font-weight: 700; cursor: pointer; transition: all .16s ease; }
    .meeting-time-choice:hover { border-color: #a5b4fc; background: #eef2ff; color: #4338ca; }
    .meeting-time-choice.active { border-color: #6366f1; background: #6366f1; color: #fff; box-shadow: 0 4px 10px rgba(99,102,241,.2); }
    .meeting-picker-footer { display: flex; align-items: center; justify-content: space-between; gap: 8px; margin-top: 11px; padding-top: 10px; border-top: 1px solid #edf2f7; }
    .meeting-picker-hint { font-size: 10px; color: #64748b; line-height: 1.35; }
    .meeting-picker-close { border: 0; background: #f8fafc; color: #475569; border: 1px solid #e2e8f0; border-radius: 8px; padding: 6px 10px; font-size: 10.5px; font-weight: 700; cursor: pointer; }
    .meeting-duration-card { display: flex; align-items: center; justify-content: center; gap: 14px; background: linear-gradient(180deg,#f8fafc,#f1f5f9); border: 1px solid #e2e8f0; border-radius: 13px; padding: 10px; }
    .duration-step-button { width: 38px; height: 38px; border-radius: 11px; border: 1px solid #cbd5e1; background: #fff; color: #475569; font-size: 20px; line-height: 1; font-weight: 700; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; transition: all .16s ease; }
    .duration-step-button:hover { background: #eef2ff; border-color: #a5b4fc; color: #4338ca; }
    .duration-value-box { min-width: 130px; text-align: center; }
    .duration-value-main { font-size: 24px; line-height: 1.05; color: #312e81; font-weight: 800; font-variant-numeric: tabular-nums; }
    .duration-value-sub { font-size: 10px; color: #64748b; margin-top: 3px; font-weight: 600; }
    .duration-quick { display: grid; grid-template-columns: repeat(6, minmax(0,1fr)); gap: 6px; margin-top: 9px; }
    .duration-quick-button { border: 1px solid #e2e8f0; background: #fff; color: #475569; border-radius: 8px; min-height: 30px; padding: 4px 3px; font-size: 10px; font-weight: 700; cursor: pointer; transition: all .16s ease; }
    .duration-quick-button:hover, .duration-quick-button.active { border-color: #a5b4fc; background: #eef2ff; color: #4338ca; }
    .meeting-duration-note { display: flex; align-items: center; gap: 6px; margin-top: 8px; color: #64748b; font-size: 10px; }
    .meeting-duration-note i { color: #6366f1; }
    .meeting-time-summary { display: flex; align-items: center; justify-content: center; gap: 7px; margin-top: 9px; font-size: 11px; color: #64748b; }
    .meeting-time-summary strong { color: #334155; font-weight: 800; }
    @keyframes meetingPickerIn {
        from { opacity: 0; transform: translateY(-3px) scale(.995); }
        to { opacity: 1; transform: translateY(0) scale(1); }
    }
    @media (max-width: 991.98px) {
        #<%= dlDetail.ClientID %> .modal-dialog { width: calc(100vw - 20px); margin: .6rem auto; }
        #<%= dlDetail.ClientID %> .modal-body { padding: 14px; }
        .meeting-description-editor { min-height: 0; }
        .meeting-description-editor .cke_contents { min-height: 220px !important; }
    }
    @media (max-width: 767.98px) {
        .meeting-participant-row .selected-meeting-members-list { grid-template-columns: 1fr; }
    }
    @media (max-width: 640px) {
        .meeting-hour-grid { grid-template-columns: repeat(6, minmax(0,1fr)); }
        .duration-quick { grid-template-columns: repeat(3, minmax(0,1fr)); }
        .meeting-picker-popover { width: min(360px, calc(100vw - 28px)); }
    }
    .meeting-method-box { display:flex;align-items:center;gap:10px;min-height:38px; }
    .meeting-method-box .form-select { height:38px;min-width:0;border:1px solid #d9c9ec;background-color:#fcfaff;color:#48247d;font-size:12px;font-weight:700;border-radius:8px; }
    .meeting-method-note { display:block;margin-top:4px;color:#84768f;font-size:10.5px;line-height:1.35; }
    .meet-join-reminder { padding:15px 16px 20px; }
    .meet-join-symbol { display:inline-flex;align-items:center;justify-content:center;width:50px;height:50px;border-radius:15px;background:#efe6fa;color:#6b32a2;font-size:24px;margin-bottom:10px; }
    .meet-join-lead { margin:0 0 9px;font-size:16px;font-weight:800;line-height:1.5;color:#40206c; }
    .meet-join-location { display:flex;align-items:start;gap:10px;margin-top:10px;padding:12px 14px;border:1px solid #e3d6f0;border-radius:10px;background:#f9f6fe;color:#3b2a57;font-size:13px;line-height:1.55;overflow-wrap:anywhere; }
    .meet-join-location i { color:#6e38a5;flex:0 0 auto;margin-top:3px; }
    .meet-join-sub { font-size:12px;color:#657187;margin-top:10px;line-height:1.55; }
    .meet-join-type { display:inline-flex;gap:5px;align-items:center;padding:5px 9px;border-radius:7px;font-size:12px;font-weight:750;color:#16636f;background:#e9f8f9; }
    .meet-join-type.is-offline { color:#78552a;background:#fff2df; }
    .meet-join-warning { display:block;margin-top:9px;font-size:12px;color:#ad5736; }
    /* Loader nhắc họp: chỉ hiển thị khi server đánh dấu cuộc họp cần nhắc. Không thay viền/nút X của ExtraModal. */
    .meet-join-preparing[hidden] { display:none!important; }
    .meet-join-preparing { position:fixed;inset:0;z-index:1200;display:flex;align-items:center;justify-content:center;padding:18px;background:rgba(33,19,54,.43);backdrop-filter:blur(5px);-webkit-backdrop-filter:blur(5px);transition:opacity .18s ease; }
    .meet-join-preparing.is-done { opacity:0;pointer-events:none; }
    .meet-join-preparing-card { width:min(390px,100%);padding:27px 27px 24px;border:1px solid #e4d5f5;border-radius:18px;background:#fff;box-shadow:0 24px 70px rgba(37,19,71,.22);text-align:center; }
    .meet-join-preparing-spinner { position:relative;display:grid;place-items:center;width:63px;height:63px;margin:0 auto 16px;border:4px solid #ebdff8;border-top-color:#6026a3;border-right-color:#9356ce;border-radius:50%;animation:meetJoinSpin .85s linear infinite; }
    .meet-join-preparing-spinner i { color:#642a9d;font-size:22px;animation:meetJoinSpinReverse .85s linear infinite; }
    .meet-join-preparing-title { display:block;margin-bottom:5px;color:#451977;font-size:17px;line-height:1.4;font-weight:800; }
    .meet-join-preparing-desc { margin:0;color:#766a86;font-size:12.5px;line-height:1.6; }
    .meet-join-preparing-progress { height:5px;margin-top:20px;overflow:hidden;border-radius:999px;background:#eee4fa; }
    .meet-join-preparing-progress span { display:block;width:0;height:100%;border-radius:inherit;background:linear-gradient(90deg,#54218d,#a56ddd); }
    .meet-join-preparing.is-running .meet-join-preparing-progress span { animation:meetJoinProgress 3s linear forwards; }
    @keyframes meetJoinSpin { to { transform:rotate(360deg); } }
    @keyframes meetJoinSpinReverse { to { transform:rotate(-360deg); } }
    @keyframes meetJoinProgress { from { width:0; } to { width:100%; } }
    @media(prefers-reduced-motion:reduce) { .meet-join-preparing-spinner,.meet-join-preparing-spinner i { animation:none; }.meet-join-preparing.is-running .meet-join-preparing-progress span { animation:none;width:100%; } }
    @media(max-width:767.98px) {
        .meeting-method-box .form-select { min-width:0; }
        .meet-join-reminder { padding:12px 9px; }
    }
</style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1"/>
                <SweetSoft:CtrlProjectTabs runat="server" ID="CtrlProjectTabs1" />
                <SweetSoft:CtrlMeet runat="server" id="CtrlMeet1" />
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Thông tin cuộc họp" DefaultButton="lbtSubmit">
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer meeting-detail-layout g-3">
                <div class="col-lg-7 meeting-form-column">
                    <div class="meeting-form-card">
                        <div class="meeting-form-card-title"><i class="fas fa-calendar-alt"></i><span>Thông tin cuộc họp</span></div>
                        <div class="row g-2">
                            <div class="col-md-8">
                                <div class="meeting-form-field">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.MEETING_NAME) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtTenCuocHop" Required="true"></SweetSoft:ExtraTextBox>
                                </div>
                            </div>
                            <div class="col-md-4">
                                <div class="meeting-form-field">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThai" SimpleInit="true" Enabled="false" CssClass="disabled meeting-readonly-input"></SweetSoft:ExtraDropdown>
                                </div>
                            </div>
                        </div>
                        <div class="row g-2">
                            <div class="col-md-4">
                                <div class="meeting-form-field">
                                    <label class="form-label label-valid">Ngày bắt đầu</label>
                                    <SweetSoft:ExtraDateTime runat="server" ID="txtNgayBatDau" Required="true" SingleDatePicker="true" Format="dd/MM/yyyy" PlaceHolder="Ngày..." />
                                </div>
                            </div>
                            <div class="col-md-4">
                                <div class="meeting-form-field meeting-time-picker" id="meetingStartTimePickerWrap">
                                    <label class="form-label label-valid">Giờ bắt đầu</label>
                                    <div class="meeting-input-shell">
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtGioBatDau" CssClass="meeting-picker-input" Required="true" PlaceHolder="Chọn giờ..." MaxLength="5"></SweetSoft:ExtraTextBox>
                                        <button type="button" class="meeting-picker-trigger" id="btnOpenStartTimePicker" aria-label="Chọn giờ bắt đầu"><i class="fas fa-clock"></i></button>
                                    </div>
                                    <div class="meeting-picker-popover" id="startTimePicker">
                                        <div class="meeting-picker-title"><strong>Chọn giờ bắt đầu</strong><span>Lần đầu mở mới sẽ tự làm tròn lên đầu giờ</span></div>
                                        <div class="meeting-time-selected"><span class="selected-time" id="selectedStartTimeText">--:--</span></div>
                                        <div class="meeting-time-section">
                                            <div class="meeting-time-section-label">Giờ</div>
                                            <div class="meeting-hour-grid" id="meetingHourGrid"></div>
                                        </div>
                                        <div class="meeting-time-section">
                                            <div class="meeting-time-section-label">Phút</div>
                                            <div class="meeting-minute-grid" id="meetingMinuteGrid"></div>
                                        </div>
                                        <div class="meeting-picker-footer">
                                            <div class="meeting-picker-hint"><i class="fas fa-lightbulb me-1"></i>Chọn giờ rồi chọn 00 / 15 / 30 / 45.</div>
                                            <button type="button" class="meeting-picker-close" id="btnCloseStartTimePicker">Xong</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-4">
                                <div class="meeting-form-field meeting-duration-picker" id="meetingDurationPickerWrap">
                                    <label class="form-label label-valid">Thời lượng cuộc họp</label>
                                    <div class="meeting-input-shell">
                                        <SweetSoft:ExtraTextBox runat="server" ID="txtThoiLuong" Required="true" AutoCompleteType="Disabled" CssClass="meeting-picker-input" PlaceHolder="Chọn thời lượng..."></SweetSoft:ExtraTextBox>
                                        <button type="button" class="meeting-picker-trigger" id="btnOpenDurationPicker" aria-label="Chọn thời lượng"><i class="fas fa-hourglass-half"></i></button>
                                    </div>
                                    <div class="meeting-picker-popover" id="durationPicker">
                                        <div class="meeting-picker-title"><strong>Chọn thời lượng</strong><span>Mỗi lần bấm tăng / giảm 15 phút</span></div>
                                        <div class="meeting-duration-card">
                                            <button type="button" class="duration-step-button" data-duration-step="-15" aria-label="Giảm 15 phút">−</button>
                                            <div class="duration-value-box">
                                                <div class="duration-value-main" id="durationValueMain">60 phút</div>
                                                <div class="duration-value-sub" id="durationValueSub">1 giờ</div>
                                            </div>
                                            <button type="button" class="duration-step-button" data-duration-step="15" aria-label="Tăng 15 phút">+</button>
                                        </div>
                                        <div class="duration-quick" id="durationQuickGrid">
                                            <button type="button" class="duration-quick-button" data-duration="15">15p</button>
                                            <button type="button" class="duration-quick-button" data-duration="30">30p</button>
                                            <button type="button" class="duration-quick-button" data-duration="45">45p</button>
                                            <button type="button" class="duration-quick-button" data-duration="60">1 giờ</button>
                                            <button type="button" class="duration-quick-button" data-duration="90">1g30</button>
                                            <button type="button" class="duration-quick-button" data-duration="120">2 giờ</button>
                                        </div>
                                        <div class="meeting-duration-note"><i class="fas fa-mouse-pointer"></i><span>Có thể bấm + / − liên tục, mỗi lần 15 phút.</span></div>
                                        <div class="meeting-picker-footer">
                                            <div class="meeting-picker-hint"><i class="fas fa-calendar-check me-1"></i>Thời gian kết thúc được cập nhật ngay.</div>
                                            <button type="button" class="meeting-picker-close" id="btnCloseDurationPicker">Xong</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="row g-2 mb-3">
                            <div class="col-md-6">
                                <div class="meeting-form-field">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.END_TIME) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtThoiGianKetThuc" Enabled="false" CssClass="disabled meeting-readonly-input" PlaceHolder="Hệ thống tự tính..."></SweetSoft:ExtraTextBox>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="meeting-form-field">
                                    <label class="form-label">Hình thức họp</label>
                                    <div class="meeting-method-box"><asp:DropDownList runat="server" ID="ddlHinhThucHop" CssClass="form-select" aria-label="Hình thức họp">
                                        <asp:ListItem Value="0" Text="Offline · Trực tiếp" Selected="True" />
                                        <asp:ListItem Value="1" Text="Online · Trực tuyến" />
                                    </asp:DropDownList></div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="meeting-form-field">
                                    <label class="form-label label-valid" id="meetingLocationLabel"><%= GetResourceText(BackEndResourceKeys.MEETING_ROOM) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtDiaDiemHop" Required="true"></SweetSoft:ExtraTextBox>
                                    <span class="meeting-method-note" id="meetingLocationHint">Nhập phòng hoặc địa điểm họp.</span>
                                </div>
                            </div>
                        </div>
                        <div class="row g-2 meeting-participant-row">
                            <div class="col-12">
                                <div class="meeting-form-field meeting-participant-box">
                                    <div class="d-flex align-items-center justify-content-between gap-2 mb-2">
                                        <label class="form-label mb-0"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME) %></label>
                                        <asp:LinkButton runat="server" ID="btnMoPopupNhanVien" CssClass="btn btn-secondary btn-sm" OnClick="btnMoPopupNhanVien_Click">
                                            <i class="fa fa-users me-1"></i><%= GetResourceText(BackEndResourceKeys.SELECT_EMPLOYEE) %>
                                        </asp:LinkButton>
                                    </div>
                                    <asp:TextBox runat="server" ID="txtNhanVienThamGia" CssClass="d-none" ReadOnly="true"></asp:TextBox>
                                    <asp:HiddenField runat="server" ID="hdfNhanVienIds" />
                                    <div id="selectedMeetingMembers" class="selected-meeting-members-box">
                                        <div class="selected-meeting-members-empty">
                                            <i class="fas fa-user-friends"></i><span>Chưa có nhân viên tham gia cuộc họp.</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-lg-5 meeting-form-column">
                    <div class="meeting-form-card meeting-description-card">
                        <div class="meeting-form-card-title"><i class="fas fa-align-left"></i><span>Mô tả / nội dung cuộc họp</span></div>
                        <div class="meeting-form-field meeting-description-editor flex-grow-1">
                            <CKEditor:CKEditorControl runat="server" ID="txtNoiDungCuocHop" Width="100%" CssClass="ck-editor" Toolbar="Full" Language="vi-VN" AutoParagraph="false" BasePath="~/Styles/plugins/ckeditor/" Height="320" />
                            <div class="form-text"><i class="fas fa-paperclip me-1"></i>File đính kèm được tải bằng nút thư mục của cuộc họp trong danh sách.</div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false">Lưu</SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
    <SweetSoft:ExtraModal runat="server" ID="dlMeetingFiles" Type="Primary" Size="Large" Title="File đính kèm lịch họp">
        <ContentTemplate>
            <div class="record-attachments"><SweetSoft:FilesBox runat="server" ID="fbMeetingFiles" IsMultiple="true" MaxFileSizeBytes="104857600" /></div>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
    <SweetSoft:ExtraModal runat="server" ID="dlChonNhanVien" Type="Primary" DefaultButton="btnXacNhanNhanVien" Title="Chọn nhân viên">
        <ContentTemplate>
            <div class="p-1">
                <div class="row align-items-center mb-3 g-2">
                    <div class="col-md-7">
                        <div class="input-group">
                            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter" PlaceHolder="Tìm kiếm theo tên nhân viên..."></SweetSoft:ExtraTextBox>
                            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClientClick="return false;"></SweetSoft:ExtraButton>
                        </div>
                    </div>
                    <div class="col-md-5 d-flex justify-content-md-end align-items-center pe-2">
                        <div class="form-check form-switch custom-switch-primary ps-0 d-flex align-items-center gap-2">
                            <input class="form-check-input cursor-pointer m-0" type="checkbox" id="chkSelectAllEmployees" style="width: 2.5em; height: 1.25em;">
                            <label class="form-check-label fw-bold cursor-pointer user-select-none text-secondary" for="chkSelectAllEmployees">Chọn tất cả</label>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-12">
                        <div class="employee-scroll-container" style="max-height: 380px; overflow-y: auto; border: 1px solid #e2e8f0; border-radius: 10px; background-color: #f8fafc; padding: 10px;">
                            <asp:Repeater ID="rptNhanVien" runat="server" OnItemDataBound="rptNhanVien_ItemDataBound">
                                <ItemTemplate>
                                    <label class="member-item-row m-0 w-100" id='mem-row-<%# Eval("UserId") %>'>
                                        <div class="row-default-view">
                                            <div class="member-info-group">
                                                <asp:CheckBox runat="server" ID="chkSelect" />
                                                <asp:HiddenField runat="server" ID="hdfUserId" Value='<%# Eval("UserId") %>' />
                                                <asp:HiddenField runat="server" ID="hdfDisplayName" Value='<%# Eval("DisplayName") %>' />
                                                <%# Eval("AvatarHtml") %>
                                                <div class="member-name-block">
                                                    <span class="fw-bold text-dark"><%# Eval("DisplayName") %></span>
                                                    <span class="member-email"><%# Eval("Email") %></span>
                                                </div>
                                            </div>
                                        </div>
                                    </label>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="btnXacNhanNhanVien" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" OnClick="btnXacNhanNhanVien_Click">Xác nhận</SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
    <SweetSoft:ExtraModal runat="server" ID="mdlMeetJoinReminder" Type="Primary" Title="Nhắc tham gia cuộc họp">
        <ContentTemplate>
            <div class="meet-join-reminder">
                <asp:HiddenField runat="server" ID="hfMeetJoinId" />
                <asp:HiddenField runat="server" ID="hfMeetJoinPending" Value="0" />
                <span class="meet-join-symbol"><i class="fas fa-video" aria-hidden="true"></i></span>
                <p class="meet-join-lead" id="meetJoinTitleText"><asp:Literal runat="server" ID="litMeetJoinTitle" /></p>
                <div id="meetJoinTypeText"><asp:Literal runat="server" ID="litMeetJoinType" /></div>
                <div class="meet-join-location"><i class="fas fa-map-marker-alt" aria-hidden="true"></i><div id="meetJoinLocationText"><asp:Literal runat="server" ID="litMeetJoinLocation" /></div></div>
                <div class="meet-join-sub">Bạn là người chủ trì hoặc người tham gia cuộc họp. Xác nhận để không nhận lại lời nhắc trong phiên làm việc.</div>
                <div id="meetJoinWarningText"><asp:Literal runat="server" ID="litMeetJoinWarning" /></div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
            <%-- Nút thuần HTML: luôn dùng AJAX; tuyệt đối không phát sinh __doPostBack làm ExtraModal render lại rỗng. --%>
            <button type="button" id="btnConfirmMeetJoin" class="btn btn-primary" onclick="return MeetJoinReminder.confirm(this, event);"><i class="fas fa-check-circle me-1" aria-hidden="true"></i> Xác nhận tham gia</button>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
    <div id="meetJoinPreparingOverlay" class="meet-join-preparing" role="status" aria-live="polite" aria-label="Đang tải thông tin cuộc họp" hidden>
        <div class="meet-join-preparing-card">
            <div class="meet-join-preparing-spinner"><i class="fas fa-video" aria-hidden="true"></i></div>
            <strong class="meet-join-preparing-title">Đang chuẩn bị cuộc họp</strong>
            <p class="meet-join-preparing-desc">Đang tải thông tin tham gia. Vui lòng chờ trong giây lát...</p>
            <div class="meet-join-preparing-progress" aria-hidden="true"><span></span></div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        // Nhắc trước giờ họp 5 phút: kiểm tra từ server mỗi 20s, không cần refresh trang.
        // Chỉ thay nội dung bên trong ExtraModal; không chỉnh khung/header/nút X bằng CSS.
        (function () {
            if (window.MeetJoinReminder && window.MeetJoinReminder.initialized) return;
            var modalId = '<%= JoinReminderModal.ClientID %>';
            var meetingIdFieldId = '<%= JoinMeetingIdClientField.ClientID %>';
            var confirmButtonId = 'btnConfirmMeetJoin';
            var endpoint = '<%= ResolveUrl("~/fMeets/MeetList.aspx") %>/GetUpcomingJoinReminder';
            var projectId = '<%= CurrentProjectId.ToString("D") %>';
            var polling = false, confirming = false, restoringAfterError = false, lastShownId = '', startAtMs = 0, activeReminder = null;
            var dismissed = Object.create(null);
            var confirmEndpoint = '<%= ResolveUrl("~/fMeets/MeetList.aspx") %>/ConfirmUpcomingJoinReminder';
            var api = window.MeetJoinReminder = { initialized: true, confirm: confirmMeeting };
            function validMeetingUrl(raw) {
                if (!raw) return '';
                try { var u = new URL(raw); return (u.protocol === 'https:' || u.protocol === 'http:') && !u.username && !u.password ? u.href : ''; } catch (e) { return ''; }
            }
            function hideReminderModal() {
                var el = modalElement(); if (!el) return;
                try {
                    if (window.bootstrap && window.bootstrap.Modal) {
                        var instance = window.bootstrap.Modal.getInstance ? window.bootstrap.Modal.getInstance(el) : null;
                        if (instance) { instance.hide(); return; }
                    }
                    if (window.jQuery && window.jQuery.fn && typeof window.jQuery.fn.modal === 'function') { window.jQuery(el).modal('hide'); return; }
                } catch (e) { if (window.console) console.warn('Không đóng được modal theo Bootstrap:',e); }
                // Fallback khi phiên bản ExtraModal/Bootstrap không expose API hide.
                el.classList.remove('show','in'); el.style.display = 'none'; el.setAttribute('aria-hidden','true');
                var others = document.querySelector('.modal.show, .modal.in');
                if (!others) { document.body.classList.remove('modal-open'); document.querySelectorAll('.modal-backdrop').forEach(function (b) { b.parentNode.removeChild(b); }); }
            }
            function setConfirmBusy(button,busy,originalText) {
                if (!button) return;
                if (busy) { button.classList.add('disabled'); button.setAttribute('aria-disabled','true'); button.style.pointerEvents = 'none'; button.innerHTML = '<i class="fas fa-spinner fa-spin me-1" aria-hidden="true"></i> Đang xác nhận...'; }
                else { button.classList.remove('disabled'); button.removeAttribute('aria-disabled'); button.style.pointerEvents = ''; if (originalText) button.innerHTML = originalText; }
            }
            function showConfirmError(message) {
                var warning = document.getElementById('meetJoinWarningText');
                if (warning) { warning.textContent = message || 'Xác nhận chưa thành công. Vui lòng thử lại.'; warning.style.color = '#b91c1c'; warning.setAttribute('role','alert'); }
            }
            function notifyConfirmSuccess() {
                var message = 'Đã xác nhận tham gia cuộc họp thành công.';
                // Đây là AJAX WebMethod: không thể gọi BaseAdminPage.ShowNotify trực tiếp từ request này.
                // Sử dụng toast đang được website cung cấp nếu có.
                if (window.toastr && typeof window.toastr.success === 'function') { window.toastr.success(message); return; }
                var existing = document.getElementById('meetJoinSuccessToast');
                if (existing && existing.parentNode) existing.parentNode.removeChild(existing);
                var toast = document.createElement('div');
                toast.id = 'meetJoinSuccessToast'; toast.setAttribute('role','status'); toast.setAttribute('aria-live','polite');
                toast.style.cssText = 'position:fixed;right:20px;top:20px;z-index:2147480000;display:flex;align-items:center;gap:10px;max-width:calc(100vw - 40px);padding:13px 17px;border-radius:10px;background:#f0fdf4;border:1px solid #bbf7d0;color:#166534;box-shadow:0 8px 28px rgba(0,0,0,.18);font-size:13px;font-weight:700;';
                var icon = document.createElement('i'); icon.className = 'fas fa-check-circle'; icon.setAttribute('aria-hidden','true');
                var label = document.createElement('span'); label.textContent = message;
                toast.appendChild(icon); toast.appendChild(label); document.body.appendChild(toast);
                window.setTimeout(function () { if (toast.parentNode) toast.parentNode.removeChild(toast); }, 3500);
            }
            function confirmMeeting(button, event) {
                if (event && event.preventDefault) event.preventDefault();
                // Đây là button type="button": hoàn toàn không postback, kể cả khi JavaScript gọi lại.
                if (confirming || !activeReminder || !activeReminder.MeetingId) return false;
                var info = activeReminder;
                var id = info.MeetingId;
                var url = validMeetingUrl(info.JoinUrl);
                var joinTab = null;
                // Trang họp online chỉ được điều hướng sau khi server xác nhận thành công.
                if (url) {
                    try { joinTab = window.open('about:blank', '_blank'); if (joinTab) joinTab.opener = null; }
                    catch (e) { joinTab = null; }
                }

                confirming = true;
                restoringAfterError = false;
                dismissed[id] = true;
                lastShownId = id;
                // Ẩn ngay khi nhấn, không đợi fetch/SQL. Giữ snapshot dữ liệu để khôi phục nếu gặp lỗi.
                activeReminder = null;
                hideReminderModal();

                fetch(confirmEndpoint, {
                    method: 'POST', credentials: 'same-origin',
                    headers: { 'Content-Type': 'application/json; charset=utf-8' },
                    body: JSON.stringify({ projectId: projectId, meetingId: id }),
                    cache: 'no-store'
                }).then(function (response) {
                    if (!response.ok) {
                        // PageMethod của WebForms trả HTTP 500 cả khi thiếu method ở code-behind.
                        // Chỉ phân loại lỗi; không đưa stack trace/SQL từ server vào giao diện.
                        return response.text().then(function (raw) {
                            var detail = '';
                            try {
                                var errorObj = JSON.parse(raw);
                                detail = String(errorObj.Message || errorObj.message || '');
                            } catch (e) { detail = String(raw || ''); }
                            var message = response.status === 500
                                ? 'Máy chủ lỗi HTTP 500 khi xác nhận. Kiểm tra MeetList.aspx.cs đã được Rebuild và IIS đã cập nhật DLL.'
                                : 'Máy chủ từ chối xác nhận (HTTP ' + response.status + ').';
                            if (/unknown web method|method.*not found|invalid web service call|could not load type/i.test(detail))
                                message = 'API xác nhận chưa được triển khai đúng trên máy chủ. Cần Rebuild MeetList.aspx.cs và cập nhật DLL.';
                            throw new Error(message);
                        });
                    }
                    return response.json();
                }).then(function (data) {
                    var result = data && data.d ? data.d : data;
                    if (!result || !result.Success)
                        throw new Error(result && result.Message ? result.Message : 'Không thể ghi nhận tham gia.');
                    // Đã lưu trên server: giữ popup đóng và thông báo thành công.
                    notifyConfirmSuccess();
                    var finalUrl = validMeetingUrl(result.JoinUrl);
                    if (finalUrl) {
                        try {
                            if (joinTab && !joinTab.closed) joinTab.location.replace(finalUrl);
                            else window.open(finalUrl, '_blank', 'noopener,noreferrer');
                        } catch (e) { if (joinTab && !joinTab.closed) joinTab.close(); }
                    } else if (joinTab && !joinTab.closed) joinTab.close();
                }).catch(function (error) {
                    if (joinTab && !joinTab.closed) joinTab.close();
                    // Không xác nhận thành công: khôi phục đầy đủ dữ liệu, tuyệt đối không mở popup trống.
                    restoringAfterError = true;
                    delete dismissed[id];
                    if (lastShownId === id) lastShownId = '';
                    var message = error && error.message ? error.message : 'Lỗi mạng khi xác nhận. Vui lòng thử lại.';
                    window.setTimeout(function () {
                        // Đợi Bootstrap hoàn tất animation đóng trước khi mở lại cùng modal.
                        setReminderData(info);
                        showConfirmError(message);
                        showBootstrapModal(0);
                        restoringAfterError = false;
                    }, 430);
                    if (window.console) console.warn('Xác nhận tham gia cuộc họp thất bại:', error);
                }).then(function () {
                    confirming = false;
                });
                return false;
            }
            function modalElement() { return document.getElementById(modalId); }
            function titleForTime() { return Date.now() < startAtMs ? 'Cuộc họp đã mở' : 'Cuộc họp đã bắt đầu'; }
            function syncTitle() {
                if (!activeReminder) return;
                var root = modalElement(), state = titleForTime();
                if (!root) return;
                var heading = root.querySelector('.modal-title, .modal-header h1, .modal-header h2, .modal-header h3, .modal-header h4, .modal-header h5');
                if (heading) heading.textContent = state;
                var lead = document.getElementById('meetJoinTitleText');
                if (lead) { lead.textContent = ''; lead.appendChild(document.createTextNode('Cuộc họp '));
                    var strong = document.createElement('strong'); strong.textContent = activeReminder.MeetingName || ''; lead.appendChild(strong);
                    lead.appendChild(document.createTextNode(state === 'Cuộc họp đã mở' ? ' đã mở' : ' đã bắt đầu')); }
            }
            function setReminderData(info) {
                activeReminder = info;
                lastShownId = info.MeetingId;
                startAtMs = Date.now() + (Number(info.SecondsUntilStart) || 0) * 1000;
                var hidden = document.getElementById(meetingIdFieldId);
                if (hidden) hidden.value = info.MeetingId;
                var type = document.getElementById('meetJoinTypeText');
                if (type) type.innerHTML = info.IsOnline ? '<span class="meet-join-type"><i class="fas fa-video"></i> Họp Online</span>' : '<span class="meet-join-type is-offline"><i class="fas fa-users"></i> Họp trực tiếp</span>';
                var place = document.getElementById('meetJoinLocationText');
                if (place) place.textContent = (info.IsOnline ? 'Liên kết họp: ' : 'Địa điểm: ') + (info.Place || 'Chưa cung cấp');
                var warning = document.getElementById('meetJoinWarningText');
                if (warning) warning.textContent = info.IsOnline && !info.JoinUrl ? 'Liên kết chưa hợp lệ. Vui lòng hỏi người chủ trì để nhận URL họp.' : '';
                var button = document.getElementById(confirmButtonId);
                if (button) { button.removeAttribute('data-join-url'); if (info.JoinUrl) button.setAttribute('data-join-url', info.JoinUrl); button.innerHTML = info.JoinUrl ? '<i class="fas fa-external-link-alt me-1" aria-hidden="true"></i> Tham gia cuộc họp' : '<i class="fas fa-check-circle me-1" aria-hidden="true"></i> Xác nhận tham gia'; }
                syncTitle();
            }
            function showBootstrapModal(attempt) {
                var el = modalElement(); if (!el) return;
                try {
                    if (window.bootstrap && window.bootstrap.Modal) {
                        (window.bootstrap.Modal.getOrCreateInstance ? window.bootstrap.Modal.getOrCreateInstance(el) : new window.bootstrap.Modal(el)).show(); return;
                    }
                    if (window.jQuery && typeof window.jQuery.fn.modal === 'function') { window.jQuery(el).modal('show'); return; }
                } catch (error) { if (window.console) console.warn('Không thể mở nhắc họp:', error); }
                if (attempt < 20) window.setTimeout(function () { showBootstrapModal(attempt + 1); }, 150);
            }
            function openReminder(info, initial) {
                var modal = modalElement();
                if (!modal || modal.classList.contains('show') || modal.classList.contains('in')) return;
                setReminderData(info);
                if (!initial) { showBootstrapModal(0); return; }
                // Giữ loading 3s khi vừa vào trang như giao diện trước đó.
                var overlay = document.getElementById('meetJoinPreparingOverlay');
                if (overlay) { overlay.hidden = false; overlay.classList.remove('is-done'); void overlay.offsetWidth; overlay.classList.add('is-running'); }
                window.setTimeout(function () {
                    if (overlay) overlay.classList.add('is-done');
                    window.setTimeout(function () {
                        if (overlay) { overlay.hidden = true; overlay.classList.remove('is-running', 'is-done'); }
                        if (activeReminder && activeReminder.MeetingId === info.MeetingId && !dismissed[info.MeetingId] && !confirming) { syncTitle(); showBootstrapModal(0); }
                    }, 180);
                }, 3000);
            }
            function check(initial) {
                if (polling || confirming || document.hidden) return;
                polling = true;
                fetch(endpoint, { method:'POST',credentials:'same-origin',headers:{'Content-Type':'application/json; charset=utf-8'},body:JSON.stringify({projectId:projectId,ignoredIds:Object.keys(dismissed)}),cache:'no-store' })
                    .then(function (response) { if (!response.ok) throw new Error('HTTP ' + response.status); return response.json(); })
                    .then(function (response) {
                        var info = response && response.d ? response.d : response;
                        if (!info || !info.Available || !info.MeetingId || dismissed[info.MeetingId] || confirming) return;
                        var modal = modalElement();
                        if (modal && (modal.classList.contains('show') || modal.classList.contains('in'))) return;
                        if (lastShownId === info.MeetingId) return;
                        openReminder(info, initial);
                    })
                    .catch(function (error) { if (window.console) console.warn('Không thể kiểm tra lịch họp sắp diễn ra:', error); })
                    .then(function () { polling = false; });
            }
            function init() {
                var modal = modalElement();
                if (modal) { var hideHandler = function () { if (restoringAfterError) return; if (lastShownId) dismissed[lastShownId] = true; activeReminder = null; }; modal.addEventListener('hidden.bs.modal', hideHandler); if (window.jQuery) window.jQuery(modal).on('hidden.bs.modal.meetreminder', hideHandler); }
                check(true); window.setInterval(function () { check(false); }, 20000);
                window.setInterval(syncTitle, 1000);
                document.addEventListener('visibilitychange', function () { if (!document.hidden) check(false); });
            }
            if (document.readyState === 'complete') init(); else window.addEventListener('load', init, { once:true });
        })();
    </script>
    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
            var $startInput = $('#<%= txtGioBatDau.ClientID %>');
            var $dateInput = $('#<%= txtNgayBatDau.ClientID %>');
            var $durationInput = $('#<%= txtThoiLuong.ClientID %>');
            var $endInput = $('#<%= txtThoiGianKetThuc.ClientID %>');
            var $startTimePicker = $('#startTimePicker');
            var $durationPicker = $('#durationPicker');
            var $selectedStartTime = $('#selectedStartTimeText');
            var $durationMain = $('#durationValueMain');
            var $durationSub = $('#durationValueSub');
            var $meetingEndPreview = $('#meetingEndPreview');

            var $meetingType = $('#<%= MeetingTypeSelect.ClientID %>');
            function syncMeetingMethod() {
                var online = $meetingType.val() === '1';
                $('#meetingLocationLabel').text(online ? 'Liên kết cuộc họp *' : 'Địa điểm họp *');
                $('#meetingLocationHint').text(online ? 'Dán URL Google Meet (https://meet.google.com/...) hoặc link họp HTTPS.' : 'Nhập phòng hoặc địa điểm họp.');
                $('#<%= txtDiaDiemHop.ClientID %>').attr('placeholder', online ? 'https://meet.google.com/...' : 'Phòng họp / địa chỉ...');
            }
            $meetingType.off('change.meettype').on('change.meettype', syncMeetingMethod);
            syncMeetingMethod();

            if (!$startInput.length || !$dateInput.length || !$durationInput.length) return;

            $startInput.prop('readonly', true).addClass('meeting-picker-input');
            $durationInput.prop('readonly', true).addClass('meeting-picker-input');

            function parseDateText() {
                var dateText = ($dateInput.val() || '').trim();
                if (!dateText) return null;
                var dmy = dateText.split('/');
                if (dmy.length !== 3) return null;
                var d = parseInt(dmy[0], 10), m = parseInt(dmy[1], 10) - 1, y = parseInt(dmy[2], 10);
                var date = new Date(y, m, d);
                return isNaN(date.getTime()) ? null : date;
            }

            function pad2(value) { return String(value).padStart(2, '0'); }

            function parseStartTime() {
                var value = ($startInput.val() || '').trim();
                var parts = value.split(':');
                if (parts.length !== 2) return null;
                var hour = parseInt(parts[0], 10), minute = parseInt(parts[1], 10);
                if (isNaN(hour) || isNaN(minute) || hour < 0 || hour > 23 || minute < 0 || minute > 59) return null;
                return { hour: hour, minute: minute };
            }

            function formatDuration(minutes) {
                if (!minutes || minutes <= 0) return { main: '--', sub: '' };
                var h = Math.floor(minutes / 60);
                var m = minutes % 60;
                if (m === 0) return { main: minutes + ' phút', sub: h === 1 ? '1 giờ' : h + ' giờ' };
                if (h === 0) return { main: minutes + ' phút', sub: 'Thời lượng ngắn' };
                return { main: minutes + ' phút', sub: h + ' giờ ' + m + ' phút' };
            }

            function syncDurationPicker(minutes) {
                var safe = parseInt(minutes, 10);
                if (isNaN(safe) || safe <= 0) safe = 60;
                safe = Math.max(15, Math.min(1440, safe));
                $durationInput.val(safe);
                var formatted = formatDuration(safe);
                $durationMain.text(formatted.main);
                $durationSub.text(formatted.sub);
                $('#durationQuickGrid .duration-quick-button').removeClass('active');
                $('#durationQuickGrid .duration-quick-button[data-duration="' + safe + '"]').addClass('active');
            }

            function renderTimeChoices() {
                if ($('#meetingHourGrid').children().length === 0) {
                    for (var h = 0; h < 24; h++) {
                        $('#meetingHourGrid').append('<button type="button" class="meeting-time-choice meeting-hour-choice" data-hour="' + h + '">' + pad2(h) + '</button>');
                    }
                }
                if ($('#meetingMinuteGrid').children().length === 0) {
                    [0, 15, 30, 45].forEach(function (m) {
                        $('#meetingMinuteGrid').append('<button type="button" class="meeting-time-choice meeting-minute-choice" data-minute="' + m + '">' + pad2(m) + '</button>');
                    });
                }
            }

            function syncTimePicker() {
                renderTimeChoices();
                var current = parseStartTime();
                var hour = current ? current.hour : 0;
                var minute = current ? current.minute : 0;
                $selectedStartTime.text(pad2(hour) + ':' + pad2(minute));
                $('#meetingHourGrid .meeting-hour-choice').removeClass('active');
                $('#meetingMinuteGrid .meeting-minute-choice').removeClass('active');
                $('#meetingHourGrid .meeting-hour-choice[data-hour="' + hour + '"]').addClass('active');
                var nearestMinute = [0, 15, 30, 45].reduce(function (best, item) {
                    return Math.abs(item - minute) < Math.abs(best - minute) ? item : best;
                }, 0);
                $('#meetingMinuteGrid .meeting-minute-choice[data-minute="' + nearestMinute + '"]').addClass('active');
            }

            function recalcMeetingTime() {
                var startDate = parseDateText();
                var startTime = parseStartTime();
                var duration = parseInt($durationInput.val(), 10);
                if (!startDate || !startTime || isNaN(duration) || duration <= 0) {
                    $endInput.val('');
                    $meetingEndPreview.text('--/--/---- --:--');
                    return;
                }
                var startDateTime = new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate(), startTime.hour, startTime.minute, 0);
                var endDate = new Date(startDateTime.getTime() + duration * 60000);
                var endStr = pad2(endDate.getDate()) + '/' + pad2(endDate.getMonth() + 1) + '/' + endDate.getFullYear() + ' ' + pad2(endDate.getHours()) + ':' + pad2(endDate.getMinutes());
                $endInput.val(endStr);
                $meetingEndPreview.text(endStr);

                var now = new Date();
                var status = 0;
                if (now > endDate) status = 3;
                else if (now >= startDateTime && now <= endDate) status = 2;
                else {
                    var diffMinutes = (startDateTime - now) / 60000;
                    if (diffMinutes > 0 && diffMinutes <= 15) status = 1;
                }
                $('#<%= ddlTrangThai.ClientID %>').val(status).trigger('change');
            }

            function setTime(hour, minute, shouldRecalc) {
                $startInput.val(pad2(hour) + ':' + pad2(minute));
                syncTimePicker();
                if (shouldRecalc !== false) recalcMeetingTime();
            }

            function getRecommendedStart() {
                var now = new Date();
                var rounded = new Date(now.getTime());
                rounded.setSeconds(0, 0);
                if (rounded.getMinutes() !== 0) rounded.setHours(rounded.getHours() + 1);
                rounded.setMinutes(0, 0, 0);
                return rounded;
            }

            function applyRecommendedStartIfEmpty() {
                if (($startInput.val() || '').trim()) return;
                var recommended = getRecommendedStart();
                var dateText = pad2(recommended.getDate()) + '/' + pad2(recommended.getMonth() + 1) + '/' + recommended.getFullYear();
                $dateInput.val(dateText);
                setTime(recommended.getHours(), 0, false);
                if (!($durationInput.val() || '').trim()) syncDurationPicker(60);
                recalcMeetingTime();
            }

            $('#btnOpenStartTimePicker').off('click.meeting').on('click.meeting', function (e) {
                e.preventDefault();
                syncTimePicker();
                $durationPicker.removeClass('open');
                $startTimePicker.toggleClass('open');
            });
            $('#btnCloseStartTimePicker').off('click.meeting').on('click.meeting', function () { $startTimePicker.removeClass('open'); });
            $('#btnOpenDurationPicker').off('click.meeting').on('click.meeting', function (e) {
                e.preventDefault();
                syncDurationPicker(parseInt($durationInput.val(), 10));
                $startTimePicker.removeClass('open');
                $durationPicker.toggleClass('open');
            });
            $('#btnCloseDurationPicker').off('click.meeting').on('click.meeting', function () { $durationPicker.removeClass('open'); });

            $startInput.off('click.meeting focus.meeting').on('click.meeting focus.meeting', function () {
                syncTimePicker();
                $durationPicker.removeClass('open');
                $startTimePicker.addClass('open');
            });
            $durationInput.off('click.meeting focus.meeting').on('click.meeting focus.meeting', function () {
                syncDurationPicker(parseInt($durationInput.val(), 10));
                $startTimePicker.removeClass('open');
                $durationPicker.addClass('open');
            });

            $(document).off('click.meetingPickers').on('click.meetingPickers', function (e) {
                if (!$(e.target).closest('#meetingStartTimePickerWrap').length) $startTimePicker.removeClass('open');
                if (!$(e.target).closest('#meetingDurationPickerWrap').length) $durationPicker.removeClass('open');
            });

            $(document).off('click.meetingHours', '.meeting-hour-choice').on('click.meetingHours', '.meeting-hour-choice', function () {
                var current = parseStartTime() || { hour: 0, minute: 0 };
                setTime(parseInt($(this).data('hour'), 10), current.minute, true);
            });
            $(document).off('click.meetingMinutes', '.meeting-minute-choice').on('click.meetingMinutes', '.meeting-minute-choice', function () {
                var current = parseStartTime() || { hour: 0, minute: 0 };
                setTime(current.hour, parseInt($(this).data('minute'), 10), true);
            });
            $(document).off('click.durationSteps', '[data-duration-step]').on('click.durationSteps', '[data-duration-step]', function () {
                var current = parseInt($durationInput.val(), 10);
                if (isNaN(current) || current <= 0) current = 60;
                syncDurationPicker(current + parseInt($(this).data('duration-step'), 10));
                recalcMeetingTime();
            });
            $(document).off('click.durationQuick', '.duration-quick-button').on('click.durationQuick', '.duration-quick-button', function () {
                syncDurationPicker(parseInt($(this).data('duration'), 10));
                recalcMeetingTime();
            });

            $dateInput.off('change.meeting blur.meeting').on('change.meeting blur.meeting', function () { recalcMeetingTime(); });

            renderTimeChoices();
            if (!($durationInput.val() || '').trim()) syncDurationPicker(60);
            else syncDurationPicker(parseInt($durationInput.val(), 10));
            syncTimePicker();
            recalcMeetingTime();
            applyRecommendedStartIfEmpty();

            var $selectedMeetingMembers = $('#selectedMeetingMembers');
            var $selectedMeetingMembersSource = $('#<%= txtNhanVienThamGia.ClientID %>');
            if ($selectedMeetingMembers.length && $selectedMeetingMembersSource.length) {
                var selectedMembersHtml = $selectedMeetingMembersSource.val() || '';
                $selectedMeetingMembers.html(selectedMembersHtml || '<div class="selected-meeting-members-empty"><i class="fas fa-user-friends"></i><span>Chưa có nhân viên tham gia cuộc họp.</span></div>');
            }

            var $searchBox = $('#<%= txtSearchSingle.ClientID %>');
            var $selectAll = $('#chkSelectAllEmployees');
            var $chkListRows = $('.member-item-row');
            $searchBox.val('');
            $searchBox.off('keyup.meeting').on('keyup.meeting', function () {
                var value = $(this).val().toLowerCase();
                $chkListRows.filter(function () { $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1); });
                $selectAll.prop('checked', false);
            });
            $selectAll.off('change.meeting').on('change.meeting', function () {
                var isChecked = $(this).is(':checked');
                $chkListRows.filter(':visible').find('input[type="checkbox"]').prop('checked', isChecked);
            });
            $chkListRows.find('input[type="checkbox"]').off('change.meeting').on('change.meeting', function () {
                if (!$(this).is(':checked')) $selectAll.prop('checked', false);
            });
        });
    </script>
</asp:Content>
