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
        .meeting-time-picker,
        .meeting-duration-picker { position: relative; }
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
        @keyframes meetingPickerIn { from { opacity: 0; transform: translateY(-3px) scale(.995); } to { opacity: 1; transform: translateY(0) scale(1); } }
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
                        <div class="row g-2">
                            <div class="col-md-6">
                                <div class="meeting-form-field">
                                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.END_TIME) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtThoiGianKetThuc" Enabled="false" CssClass="disabled meeting-readonly-input" PlaceHolder="Hệ thống tự tính..."></SweetSoft:ExtraTextBox>
                                    <div class="meeting-time-summary"><i class="fas fa-clock"></i><span>Kết thúc:</span><strong id="meetingEndPreview">--/--/---- --:--</strong></div>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="meeting-form-field">
                                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.MEETING_ROOM) %></label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtDiaDiemHop" Required="true"></SweetSoft:ExtraTextBox>
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
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
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
