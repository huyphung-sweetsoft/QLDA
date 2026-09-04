<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlChonNhanVien.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fUsers.Controls.CtrlChonNhanVien" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    /* CSS CÔ LẬP CHO GIAO DIỆN ACCORDION VÀ LỊCH BỂU TRƯỢT */
    .member-section-title {
        font-size: 13px; font-weight: 700; color: #1e3a8a; padding: 10px 12px; background: #f8fafc;
        border: 1px solid #e2e8f0; border-radius: 6px; margin-top: 10px; margin-bottom: 6px;
        cursor: pointer; display: flex; justify-content: space-between; align-items: center; user-select: none;
    }
    .member-section-title:hover { background: #f1f5f9; }
    .member-section-title .arrow-icon { font-size: 10px; color: #64748b; transition: transform 0.25s ease; }
    .member-accordion-group.open .arrow-icon { transform: rotate(180deg); }

    .member-accordion-content { max-height: 0; overflow: hidden; transition: max-height 0.35s ease-in-out; }
    .member-accordion-group.open .member-accordion-content { max-height: 1200px; overflow-y: auto; overflow-x: hidden; }

    /* ROW NHÂN VIÊN VÀ HIỆU ỨNG SLIDE LỊCH BỂU */
    .member-item-row {
        position: relative; background: white; border: 1px solid #e2e8f0; border-radius: 8px;
        margin-bottom: 8px; overflow: hidden; transition: min-height 0.3s cubic-bezier(0.16, 1, 0.3, 1);
        min-height: 52px; display: flex; align-items: center;
    }
    .member-item-row.show-schedule { border-color: #93c5fd; box-shadow: 0 4px 12px rgba(37, 99, 235, 0.08); }

    .row-default-view { display: flex; align-items: center; justify-content: space-between; padding: 8px 12px; width: 100%; height: 100%; font-size: 13px; }
    .member-info-group { display: flex; align-items: center; gap: 10px; }
    .member-info-group input[type="checkbox"] { width: 16px; height: 16px; cursor: pointer; accent-color: #2563eb; }
    
    .btn-calendar-only {
        background: #ffffff; border: 1px solid #e2e8f0; width: 32px; height: 32px;
        border-radius: 8px; cursor: pointer; display: flex; align-items: center; justify-content: center;
        font-size: 15px; transition: all 0.2s cubic-bezier(0.34, 1.56, 0.64, 1);
    }
    .btn-calendar-only:hover { background: #eff6ff; border-color: #93c5fd; transform: scale(1.1); }

    /* OVERLAY LỊCH BỂU TRƯỢT */
    .row-schedule-overlay {
        position: absolute; inset: 0; background: #ffffff; z-index: 5;
        display: flex; align-items: flex-start; gap: 10px; padding: 8px 12px;
        transform: translateX(100%); transition: transform 0.35s cubic-bezier(0.16, 1, 0.3, 1); height: 100%;
    }
    .member-item-row.show-schedule .row-schedule-overlay { transform: translateX(0); }

    .btn-back-row-slide {
        background: #f1f5f9; border: 1px solid #cbd5e1; width: 30px; height: 30px; border-radius: 6px;
        cursor: pointer; display: flex; align-items: center; justify-content: center;
        font-size: 14px; font-weight: bold; color: #2563eb; flex-shrink: 0; margin-top: 4px;
    }
    .btn-back-row-slide:hover { background: #e0f2fe; border-color: #2563eb; transform: translateX(-2px); }

    .row-sched-timeline-grid-7col { display: grid; grid-template-columns: repeat(7, 1fr); gap: 6px; flex: 1; padding: 2px 0; }
    .sched-day-card { border: 1px solid #cbd5e1; border-radius: 6px; overflow: hidden; display: flex; flex-direction: column; background: white; min-height: 52px; }
    .sd-header { background: #f1f5f9; padding: 3px 2px; text-align: center; font-weight: 800; font-size: 11px; border-bottom: 1px solid #cbd5e1; color: #1e293b; line-height: 1.1; }
    .sd-header small { font-size: 9.5px; font-weight: 600; color: #64748b; display: block; }
    .sd-body { padding: 4px 2px; text-align: center; font-size: 10.5px; font-weight: 700; display: flex; align-items: center; justify-content: center; flex: 1; min-height: 32px; line-height: 1.25; }

    /* 5 MÀU TRẠNG THÁI LỊCH */
    .sd-body.current-task { background-color: #e0f2fe; color: #0369a1; border-top: 2.5px solid #0284c7; }
    .sd-body.other-project { background-color: #f3e8fd; color: #681da8; border-top: 2.5px solid #a855f7; }
    .sd-body.holiday { background-color: #fef3c7; color: #b45309; border-top: 2.5px solid #f59e0b; }
    .sd-body.weekend { background-color: #f8fafc; color: #64748b; }
    .sd-body.free { background-color: #e6f4ea; color: #137333; border-top: 2.5px solid #34a853; }
</style>

<!-- Tiêu đề Modal (Title) sẽ được thiết lập tự động trong file .cs để tái sử dụng -->
<SweetSoft:ExtraModal runat="server" ID="mdlMemberPicker" Type="Primary" DefaultButton="btnSave">
    <ContentTemplate>
        <div class="row js-validation validationEngineContainer p-2">
            
            <!-- KHỐI THÔNG TIN CHUNG -->
            <div class="col-12 mb-3">
                <div style="font-size: 12px; color: #1e40af; background: #eff6ff; padding: 10px 12px; border-radius: 6px; border: 1px solid #bfdbfe;">
                    <asp:Literal runat="server" ID="ltrInfoNote"></asp:Literal>
                </div>
            </div>
             
            <div class="col-12" style="max-height: 60vh; overflow-y: auto; overflow-x: hidden;">
                
                <!-- ACCORDION 1: THÀNH VIÊN ĐÃ THAM GIA DỰ ÁN -->
                <div class="member-accordion-group open" id="accGroupProject" runat="server">
                    <div class="member-section-title" onclick="CMSMasterJs.TogglePickerAccordion(this)">
                        <span>📁 <%= GetResourceText(BackEndResourceKeys.PROJECT_MEMBERS) %> (<asp:Literal ID="ltrCountProj" runat="server">0</asp:Literal>)</span>
                        <span class="arrow-icon">▼</span>
                    </div>
                    <div class="member-accordion-content">
                        <asp:Repeater ID="rptProjectMembers" runat="server">
                            <ItemTemplate>
                                <div class="member-item-row" id='mem-row-<%# Eval("UserId") %>'>
                                    <div class="row-default-view">
                                        <div class="member-info-group">
                                            <asp:CheckBox runat="server" ID="chkSelect" />
                                            <asp:HiddenField runat="server" ID="hdfUserId" Value='<%# Eval("UserId") %>' />
                                            <span class="fw-bold text-dark"><%# Eval("DisplayName") %></span>
                                        </div>
                                        <button type="button" class="btn-calendar-only" onclick="CMSMasterJs.ToggleRowSchedule(this, '<%# Eval("UserId") %>', '<%# Eval("DisplayName") %>', true)">📅</button>
                                    </div>
                                    
                                    <asp:HiddenField runat="server" ID="hdfScheduleJson" Value='<%# Eval("ScheduleJson") %>' />
                                    <div class="row-schedule-overlay" id='overlay-<%# Eval("UserId") %>'>
                                        <button type="button" class="btn-back-row-slide" onclick="CMSMasterJs.ToggleRowSchedule(this, '<%# Eval("UserId") %>', '<%# Eval("DisplayName") %>', false)">←</button>
                                        <div class="pe-2 border-end" style="min-width: 90px; flex-shrink: 0; margin-top: 4px;">
                                            <strong style="font-size: 11.5px;"><%# Eval("DisplayName") %></strong>
                                        </div>
                                        <div class="row-sched-timeline-grid-7col" id='timeline-<%# Eval("UserId") %>'></div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>

                <!-- ACCORDION 2: THÀNH VIÊN CÔNG TY -->
                <div class="member-accordion-group open" id="accGroupCompany" runat="server">
                    <div class="member-section-title" style="background: #f1f5f9;" onclick="CMSMasterJs.TogglePickerAccordion(this)">
                        <span>🏢 <%= GetResourceText(BackEndResourceKeys.PROJECT_MEMBERS) %> (<asp:Literal ID="ltrCountCompany" runat="server">0</asp:Literal>)</span>
                        <span class="arrow-icon">▼</span>
                    </div>
                    <div class="member-accordion-content">
                        <asp:Repeater ID="rptCompanyMembers" runat="server">
                            <ItemTemplate>
                                <div class="member-item-row" id='mem-row-<%# Eval("UserId") %>'>
                                    <div class="row-default-view">
                                        <div class="member-info-group">
                                            <asp:CheckBox runat="server" ID="CheckBox1" />
                                            <asp:HiddenField runat="server" ID="HiddenField1" Value='<%# Eval("UserId") %>' />
                                            <span class="fw-bold text-dark"><%# Eval("DisplayName") %></span>
                                        </div>
                                        <button type="button" class="btn-calendar-only" onclick="CMSMasterJs.ToggleRowSchedule(this, '<%# Eval("UserId") %>', '<%# Eval("DisplayName") %>', true)">📅</button>
                                    </div>
                                    
                                    <asp:HiddenField runat="server" ID="HiddenField2" Value='<%# Eval("ScheduleJson") %>' />
                                    <div class="row-schedule-overlay" id='overlay-<%# Eval("UserId") %>'>
                                        <button type="button" class="btn-back-row-slide" onclick="CMSMasterJs.ToggleRowSchedule(this, '<%# Eval("UserId") %>', '<%# Eval("DisplayName") %>', false)">←</button>
                                        <div class="pe-2 border-end" style="min-width: 90px; flex-shrink: 0; margin-top: 4px;">
                                            <strong style="font-size: 11.5px;"><%# Eval("DisplayName") %></strong>
                                        </div>
                                        <div class="row-sched-timeline-grid-7col" id='timeline-<%# Eval("UserId") %>'></div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div> 

        </div>
    </ContentTemplate>
    
    <FooterTemplate>
        <asp:UpdatePanel runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <SweetSoft:ExtraButton runat="server" ID="btnSave" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" OnClick="btnSave_Click">
                    <%= GetResourceText(BackEndResourceKeys.SAVE) %>
                </SweetSoft:ExtraButton>
            </ContentTemplate>
        </asp:UpdatePanel>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    // Khởi tạo an toàn nếu CMSMasterJs chưa kịp load
    window.CMSMasterJs = window.CMSMasterJs || {};

    $(document).ready(function () {
        CMSMasterJs.TogglePickerAccordion = function (btnElement) {
            $(btnElement).closest('.member-accordion-group').toggleClass('open');
        };

        CMSMasterJs.ToggleRowSchedule = function (btnElement, userId, userName, isShow) {
            var rowEl = $(btnElement).closest('.member-item-row');

            if (isShow) {
                $('.member-item-row.show-schedule').not(rowEl).each(function () {
                    $(this).removeClass('show-schedule').css('min-height', '52px');
                });

                var jsonString = rowEl.find('input[type="hidden"][id*="hdfScheduleJson"]').val();
                var timelineGrid = rowEl.find('#timeline-' + userId);
                timelineGrid.empty();

                if (jsonString) {
                    try {
                        var scheduleData = JSON.parse(jsonString);
                        var countDays = 0;

                        for (var dateKey in scheduleData) {
                            countDays++;
                            var dayData = scheduleData[dateKey];
                            var dateParts = dateKey.split('-');
                            var formattedDate = dateParts[2] + '/' + dateParts[1];

                            var html = '<div class="sched-day-card">' +
                                '<div class="sd-header">' + formattedDate + '<small>' + dayData.dayName + '</small></div>' +
                                '<div class="sd-body ' + dayData.status + '">' + dayData.text + '</div>' +
                                '</div>';
                            timelineGrid.append(html);
                        }

                        var rowCount = Math.ceil(countDays / 7);
                        var calculatedMinHeight = Math.max(88, rowCount * 62 + 20);
                        rowEl.css('min-height', calculatedMinHeight + 'px');
                    } catch (e) {
                        console.error("Lỗi parse JSON lịch biểu: ", e);
                    }
                }

                rowEl.addClass('show-schedule');
            } else {
                rowEl.removeClass('show-schedule');
                rowEl.css('min-height', '52px');
            }
        };
    });
</script>