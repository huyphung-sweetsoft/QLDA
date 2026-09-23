<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlXemNhanVienTask.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlXemNhanVienTask" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    /* BỘ CSS LỊCH BIỂU (Rút gọn cho View) */
    .member-item-row { position: relative; background: white; border: 1px solid #e2e8f0; border-radius: 8px; margin-bottom: 8px; overflow: hidden; transition: min-height 0.3s cubic-bezier(0.16, 1, 0.3, 1); min-height: 52px; display: flex; align-items: center; }
    .member-item-row.show-schedule { border-color: #93c5fd; box-shadow: 0 4px 12px rgba(37, 99, 235, 0.08); }
    .row-default-view { display: flex; align-items: center; justify-content: space-between; padding: 8px 12px; width: 100%; height: 100%; font-size: 13px; }
    .member-info-group { display: flex; align-items: center; gap: 10px; }
    
    .btn-calendar-only { background: #ffffff; border: 1px solid #e2e8f0; width: 32px; height: 32px; border-radius: 8px; cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 15px; transition: all 0.2s cubic-bezier(0.34, 1.56, 0.64, 1); }
    .btn-calendar-only:hover { background: #eff6ff; border-color: #93c5fd; transform: scale(1.1); }

    /* CSS CHO AVATAR RIÊNG LẺ (Không bị đè mép như danh sách) */
    .single-avatar-circle {
        width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; 
        font-size: 11px; font-weight: 700; color: #ffffff; flex-shrink: 0; box-shadow: 0 1px 2px rgba(0,0,0,0.1);
    }

    /* OVERLAY LỊCH BIỂU TRƯỢT */
    .row-schedule-overlay { position: absolute; inset: 0; background: #ffffff; z-index: 5; display: flex; align-items: flex-start; gap: 10px; padding: 8px 12px; transform: translateX(100%); transition: transform 0.35s cubic-bezier(0.16, 1, 0.3, 1); height: 100%; }
    .member-item-row.show-schedule .row-schedule-overlay { transform: translateX(0); }
    .btn-back-row-slide { background: #f1f5f9; border: 1px solid #cbd5e1; width: 30px; height: 30px; border-radius: 6px; cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 14px; font-weight: bold; color: #2563eb; flex-shrink: 0; margin-top: 4px; }
    .btn-back-row-slide:hover { background: #e0f2fe; border-color: #2563eb; transform: translateX(-2px); }

    .row-sched-timeline-grid-7col { display: grid; grid-template-columns: repeat(7, 1fr); gap: 6px; flex: 1; padding: 2px 0; }
    .sched-day-card { border: 1px solid #cbd5e1; border-radius: 6px; overflow: hidden; display: flex; flex-direction: column; background: white; min-height: 52px; }
    .sd-header { background: #f1f5f9; padding: 3px 2px; text-align: center; font-weight: 800; font-size: 11px; border-bottom: 1px solid #cbd5e1; color: #1e293b; line-height: 1.1; }
    .sd-header small { font-size: 9.5px; font-weight: 600; color: #64748b; display: block; }
    .sd-body { padding: 4px 2px; text-align: center; font-size: 10.5px; font-weight: 700; display: flex; align-items: center; justify-content: center; flex: 1; min-height: 32px; line-height: 1.25; }

    /* MÀU TRẠNG THÁI LỊCH */
    .sd-body.holiday { background-color: #fef3c7; color: #b45309; border-top: 2.5px solid #f59e0b; }
    .sd-body.weekend { background-color: #f8fafc; color: #64748b; }
    .sd-body.free { background-color: #e6f4ea; color: #137333; border-top: 2.5px solid #34a853; }
    .sd-body.busy { background-color: #fee2e2; color: #b91c1c; border-top: 2.5px solid #ef4444; }
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlViewTaskMember" Type="Primary" HideFooter="true">
    <ContentTemplate>
        <div class="row p-2">
            <!-- THÔNG BÁO THỜI GIAN CÔNG VIỆC -->
            <div class="col-12 mb-3">
                <div style="font-size: 12px; color: #1e40af; background: #eff6ff; padding: 10px 12px; border-radius: 6px; border: 1px solid #bfdbfe;">
                    <asp:Literal runat="server" ID="ltrTaskInfoNote"></asp:Literal>
                </div>
            </div>

            <div class="col-12" style="max-height: 60vh; overflow-y: auto; overflow-x: hidden;">
                <!-- DANH SÁCH NHÂN SỰ ĐÃ GÁN -->
                <asp:Repeater ID="rptAssignedMembers" runat="server">
                    <ItemTemplate>
                        <div class="member-item-row" id='mem-view-<%# Eval("UserId") %>'>
                            <div class="row-default-view">
                                <div class="member-info-group">
                                    <!-- HIỂN THỊ AVATAR THAY CHO DẤU TICK -->
                                    <%# Eval("AvatarHtml") %>
                                    
                                    <span class="fw-bold text-dark ms-1"><%# Eval("DisplayName") %></span>
                                    <%# Convert.ToBoolean(Eval("IsPM")) ? "<span class='badge bg-danger ms-2' style='font-size: 10px; padding: 2px 6px; border-radius: 4px;'>PM</span>" : "" %>
                                </div>
                                <button type="button" class="btn-calendar-only" onclick="CMSMasterJs.ToggleRowScheduleView(this, '<%# Eval("UserId") %>', true)">📅</button>
                            </div>
                            
                            <asp:HiddenField runat="server" ID="hdfScheduleJson" Value='<%# Eval("ScheduleJson") %>' />
                            <div class="row-schedule-overlay" id='overlay-view-<%# Eval("UserId") %>'>
                                <button type="button" class="btn-back-row-slide" onclick="CMSMasterJs.ToggleRowScheduleView(this, '<%# Eval("UserId") %>', false)">←</button>
                                <div class="pe-2 border-end" style="min-width: 90px; flex-shrink: 0; margin-top: 4px;">
                                    <strong style="font-size: 11.5px;"><%# Eval("DisplayName") %></strong>
                                </div>
                                <div class="row-sched-timeline-grid-7col" id='timeline-view-<%# Eval("UserId") %>'></div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <!-- THÔNG BÁO NẾU CHƯA CÓ AI -->
                <div runat="server" id="divEmpty" visible="false" class="text-center p-4 text-muted border rounded bg-light">
                    <i class="fas fa-user-times fs-3 mb-2"></i><br />
                    <%= GetResourceText(BackEndResourceKeys.TASK_HAS_NO_ASSIGNEE) ?? "Công việc này chưa có nhân viên phụ trách." %>
                </div>
            </div> 
        </div>
    </ContentTemplate>
</SweetSoft:ExtraModal>

<script type="text/javascript">
    window.CMSMasterJs = window.CMSMasterJs || {};
    $(document).ready(function () {
        CMSMasterJs.ToggleRowScheduleView = function (btnElement, userId, isShow) {
            var rowEl = $(btnElement).closest('.member-item-row');

            if (isShow) {
                $('.member-item-row.show-schedule').not(rowEl).each(function () {
                    $(this).removeClass('show-schedule').css('min-height', '52px');
                });

                var jsonString = rowEl.find('input[type="hidden"][id*="hdfScheduleJson"]').val();
                var timelineGrid = rowEl.find('#timeline-view-' + userId);
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