<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="LichBieu.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fLichBieu.LichBieu" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fLichBieu/Controls/CtrlLichNgoaiLe.ascx" TagPrefix="SweetSoft" TagName="CtrlLichNgoaiLe" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        /* Bảng cấu hình tuần: giữ fieldset và màu tím theo giao diện chung */
        .schedule-table-wrap {
            overflow-x: auto;
            border: 1px solid #e5d8f4;
            border-radius: 10px;
            background: #fff;
        }

        .schedule-config-table {
            min-width: 980px;
            margin-bottom: 0;
            border-collapse: separate;
            border-spacing: 0;
        }

        .schedule-config-table thead th {
            padding: .85rem .8rem;
            color: #4b168c;
            background: #f4eefb;
            border-bottom: 1px solid #dfd0ef;
            font-size: .82rem;
            font-weight: 700;
            letter-spacing: .025em;
            white-space: nowrap;
            vertical-align: middle;
        }

        .schedule-config-table tbody td {
            padding: .65rem .7rem;
            border-color: #eee7f5;
            vertical-align: middle;
        }

        .schedule-config-table tbody tr:hover > td {
            background-color: #fcfaff;
        }

        .schedule-day-cell {
            width: 155px;
            min-width: 155px;
            background: #fbf8ff;
        }

        .schedule-day-name {
            color: #4b168c;
            font-weight: 700;
            white-space: nowrap;
        }

        .schedule-day-status {
            display: inline-flex;
            align-items: center;
            margin-top: .4rem;
            padding: .18rem .5rem;
            border-radius: 999px;
            font-size: .72rem;
            font-weight: 600;
            white-space: nowrap;
        }

        .schedule-day-status.is-working {
            color: #5b21b6;
            background: #eee5fa;
        }

        .schedule-day-status.is-day-off {
            color: #64748b;
            background: #eef0f3;
        }

        .schedule-shift-cell {
            min-width: 360px;
            padding: .65rem .7rem;
            border: 1px solid #eee7f5;
            border-radius: 8px;
            background: #fff;
        }

        .schedule-shift-heading {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: .75rem;
            margin-bottom: .55rem;
        }

        .schedule-shift-title {
            color: #5b21b6;
            font-size: .88rem;
            font-weight: 700;
            white-space: nowrap;
        }

        .schedule-shift-title.afternoon {
            color: #318acb;
        }

        .schedule-shift-switch {
            display: flex;
            align-items: center;
            justify-content: flex-end;
            flex-shrink: 0;
        }

        .schedule-time-group {
            display: flex;
            flex-wrap: nowrap;
            align-items: stretch;
            width: 100%;
        }

        .schedule-time-group .input-group-text {
            padding: .35rem .5rem;
            color: #514263;
            background: #f8f5fc;
            border-color: #ded5e9;
            font-size: .8rem;
            font-weight: 600;
        }

        .schedule-time-input {
            width: 1%;
            min-width: 0;
            max-width: none;
            flex: 1 1 0;
            padding: .35rem .4rem;
            text-align: center;
            border-color: #ded5e9;
            font-size: .9rem;
        }

        .schedule-time-input:disabled {
            color: #94a3b8;
            background: #f1f3f5;
            cursor: not-allowed;
            opacity: .8;
        }

        @media (max-width: 767.98px) {
            .schedule-config-table {
                min-width: 940px;
            }
            .schedule-config-table tbody td {
                padding: .5rem;
            }
            .schedule-shift-cell {
                min-width: 345px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen p-2">
                
                <!-- BREADCRUMB -->
                <SweetSoft:Navigation runat="server" ID="Navigation1"  />
                
                <!-- VÙNG TABS HEADER -->
                <div class="flex-between flex-between-xl gap-4 mb-4">
                    <div class="tabs-horizontal">
                        <ul class="nav nav-pills card-header-pills" role="tablist">
                            <li class="nav-item">
                                <a class="nav-link px-3 active fw-bold" data-bs-toggle="tab" href="#tuan-lam-viec" role="tab">
                                    <i class="fas fa-calendar-week me-2"></i><%= GetResourceText(BackEndResourceKeys.WORKING_WEEK_CONFIG) %>
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link px-3 fw-bold" data-bs-toggle="tab" href="#lich-ngoai-le" role="tab">
                                    <i class="fas fa-umbrella-beach me-2"></i><%= GetResourceText(BackEndResourceKeys.HOLIDAY) %>
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
                
                <!-- VÙNG NỘI DUNG TABS -->
                <div class="card-body p-0">
                    <div class="tab-content text-muted tab-overide">
                        
                        <!-- TAB 1: CẤU HÌNH TUẦN LÀM VIỆC CỐ ĐỊNH -->
                        <div class="tab-pane active js-validation validationEngineContainer" id="tuan-lam-viec" role="tabpanel">
                            <asp:UpdatePanel runat="server" ID="upnlTuanLamViec" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <asp:HiddenField ID="hdfDefaultMorningStart" runat="server" />
                                    <asp:HiddenField ID="hdfDefaultMorningEnd" runat="server" />
                                    <asp:HiddenField ID="hdfDefaultAfternoonStart" runat="server" />
                                    <asp:HiddenField ID="hdfDefaultAfternoonEnd" runat="server" />
                                    <fieldset class="fieldset-box mb-3">
                                        <legend class="text-primary fw-bold"><%= GetResourceText(BackEndResourceKeys.STANDARD_WORKING_HOURS) %></legend>
                                        
                                        <div class="table-responsive schedule-table-wrap">
                                            <table id="tblScheduleWeek" class="table schedule-config-table align-middle">
                                                <thead>
                                                    <tr>
                                                        <th scope="col">Ngày trong tuần</th>
                                                        <th scope="col"><i class="fas fa-sun me-2"></i>Ca sáng</th>
                                                        <th scope="col"><i class="fas fa-cloud-sun me-2"></i>Ca chiều</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <asp:Repeater ID="rptTuanLamViec" runat="server" OnItemDataBound="rptTuanLamViec_ItemDataBound">
                                                        <ItemTemplate>
                                                            <tr class="schedule-day-row">
                                                                <td class="schedule-day-cell">
                                                                    <div class="schedule-day-name">
                                                                        <asp:Literal ID="ltrTenThu" runat="server"></asp:Literal>
                                                                    </div>
                                                                    <span class="schedule-day-status">
                                                                        <asp:Literal ID="ltrTrangThaiNgay" runat="server"></asp:Literal>
                                                                    </span>
                                                                    <asp:HiddenField runat="server" ID="hdfIdCauHinh" Value='<%# Eval("IdCauHinh") %>' />
                                                                </td>
                                                                <td>
                                                                    <div class="schedule-shift-cell">
                                                                        <div class="schedule-shift-heading">
                                                                            <span class="schedule-shift-title"><i class="fas fa-sun me-1"></i>Ca sáng</span>
                                                                            <div class="schedule-shift-switch">
                                                                                <SweetSoft:ExtraCheckbox runat="server"
                                                                                    ID="chkCaSang"
                                                                                    SwitchType="Primary"
                                                                                    CssClass="js-shift-morning"
                                                                                    OnText="Bật"
                                                                                    OffText="Tắt"
                                                                                    OnChange="CMSMasterJs.ToggleShiftConfig(this, 'morning');" />
                                                                            </div>
                                                                        </div>
                                                                        <div class="input-group input-group-sm schedule-time-group">
                                                                            <span class="input-group-text"><%= GetResourceText(BackEndResourceKeys.FROM) %></span>
                                                                            <input type="time" runat="server" id="txtGioBatDauSang" class="form-control schedule-time-input time-morning-start" />
                                                                            <span class="input-group-text"><%= GetResourceText(BackEndResourceKeys.TO) %></span>
                                                                            <input type="time" runat="server" id="txtGioKetThucSang" class="form-control schedule-time-input time-morning-end" />
                                                                        </div>
                                                                    </div>
                                                                </td>
                                                                <td>
                                                                    <div class="schedule-shift-cell">
                                                                        <div class="schedule-shift-heading">
                                                                            <span class="schedule-shift-title afternoon"><i class="fas fa-cloud-sun me-1"></i>Ca chiều</span>
                                                                            <div class="schedule-shift-switch">
                                                                                <SweetSoft:ExtraCheckbox runat="server"
                                                                                    ID="chkCaChieu"
                                                                                    SwitchType="Info"
                                                                                    CssClass="js-shift-afternoon"
                                                                                    OnText="Bật"
                                                                                    OffText="Tắt"
                                                                                    OnChange="CMSMasterJs.ToggleShiftConfig(this, 'afternoon');" />
                                                                            </div>
                                                                        </div>
                                                                        <div class="input-group input-group-sm schedule-time-group">
                                                                            <span class="input-group-text"><%= GetResourceText(BackEndResourceKeys.FROM) %></span>
                                                                            <input type="time" runat="server" id="txtGioBatDauChieu" class="form-control schedule-time-input time-afternoon-start" />
                                                                            <span class="input-group-text"><%= GetResourceText(BackEndResourceKeys.TO) %></span>
                                                                            <input type="time" runat="server" id="txtGioKetThucChieu" class="form-control schedule-time-input time-afternoon-end" />
                                                                        </div>
                                                                    </div>
                                                                </td>
                                                            </tr>
                                                        </ItemTemplate>
                                                    </asp:Repeater>
                                                </tbody>
                                            </table>
                                        </div>
                                    </fieldset>
                                    
                                   <div class="text-end mb-4 d-flex justify-content-end gap-2">

                                        <SweetSoft:ExtraButton runat="server"
                                            ID="btnApplyDefaultHours"
                                            ButtonStyle="Secondary"
                                            ButtonIcon="Refresh"
                                            OnClientClick="CMSMasterJs.ApplyDefaultWorkingHours(); return false;">
                                            Áp dụng giờ làm việc mặc định
                                        </SweetSoft:ExtraButton>

                                        <SweetSoft:ExtraButton runat="server"
                                            ID="btnSaveTuan"
                                            ButtonStyle="Primary"
                                            ButtonIcon="Save"
                                            IsPace="true"
                                            OnClick="btnSaveTuan_Click">
                                            <%= GetResourceText(BackEndResourceKeys.SAVE_WEEK_CONFIG) %>
                                        </SweetSoft:ExtraButton>

                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        
                        <!-- TAB 2: LỄ TẾT VÀ LÀM BÙ -->
                        <!-- ========================================== -->
                        <div class="tab-pane" id="lich-ngoai-le" role="tabpanel">
                            <!-- Gọi User Control Lưới dữ liệu vào đây -->
                            <SweetSoft:CtrlLichNgoaiLe runat="server" ID="CtrlLichNgoaiLe1" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    

    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Cập nhật lịch ngoại lệ" DefaultButton="lbtSubmit">
        
   
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">
           
                <asp:HiddenField ID="hdfIdNgoaiLe" runat="server" />

                <div class="col-lg-12">
                    <fieldset class="fieldset-box mb-3">
                        <legend class="text-primary fw-bold"><%= GetResourceText(BackEndResourceKeys.INFORMATION) %></legend>
                        <div class="row">
                            
                            <div class="col-lg-12 mb-3">
                                <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.EVENT_NAME) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtTenNgoaiLe" Required="true" MaxLength="255" PlaceHolder="VD: Nghỉ Tết Nguyên Đán..."></SweetSoft:ExtraTextBox>
                            </div>
                            
                            <div class="col-lg-6 mb-3">
                                <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.FROM_DATE) %></label>
                                <asp:TextBox runat="server" ID="txtNgayBatDau" type="date" CssClass="form-control validate[required]"></asp:TextBox>
                            </div>
                            
                            <div class="col-lg-6 mb-3">
                                <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.TO_DATE) %></label>
                                <asp:TextBox runat="server" ID="txtNgayKetThuc" type="date" CssClass="form-control validate[required]"></asp:TextBox>
                            </div>

                            <div class="col-lg-12 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtMoTa" TextMode="MultiLine" Rows="3" PlaceHolder="Nhập ghi chú hoặc lý do nghỉ..."></SweetSoft:ExtraTextBox>
                            </div>

                        </div>
                    </fieldset>
                </div>
            </div>
        </ContentTemplate>
        
        
        <FooterTemplate>
            <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true"
                        OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false">
                        <%= GetResourceText(BackEndResourceKeys.SAVE) %>
                    </SweetSoft:ExtraButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>

    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        CMSMasterJs.UpdateScheduleDayStatus = function ($row) {
            var morningOn = $row.find('input.js-shift-morning:checked').length > 0;
            var afternoonOn = $row.find('input.js-shift-afternoon:checked').length > 0;
            var isWorking = morningOn || afternoonOn;
            var $status = $row.find('.schedule-day-status');

            $status.text(isWorking ? 'Ngày làm việc' : 'Ngày nghỉ')
                .toggleClass('is-working', isWorking)
                .toggleClass('is-day-off', !isWorking);
        };

        CMSMasterJs.ToggleShiftConfig = function (checkboxElement, shiftType) {
            var $checkbox = $(checkboxElement);
            var isChecked = $checkbox.is(':checked');
            var $row = $checkbox.closest('tr.schedule-day-row');
            var $startInput;
            var $endInput;
            var defaultStart;
            var defaultEnd;

            if (shiftType === 'morning') {
                $startInput = $row.find('.time-morning-start');
                $endInput = $row.find('.time-morning-end');
                defaultStart = $('#<%= hdfDefaultMorningStart.ClientID %>').val();
                defaultEnd = $('#<%= hdfDefaultMorningEnd.ClientID %>').val();
            }
            else {
                $startInput = $row.find('.time-afternoon-start');
                $endInput = $row.find('.time-afternoon-end');
                defaultStart = $('#<%= hdfDefaultAfternoonStart.ClientID %>').val();
                defaultEnd = $('#<%= hdfDefaultAfternoonEnd.ClientID %>').val();
            }

            if (isChecked) {
                $startInput.val(defaultStart || '').prop('disabled', false).removeAttr('disabled');
                $endInput.val(defaultEnd || '').prop('disabled', false).removeAttr('disabled');
            }
            else {
                $startInput.val('').prop('disabled', true).attr('disabled', 'disabled');
                $endInput.val('').prop('disabled', true).attr('disabled', 'disabled');
            }

            CMSMasterJs.UpdateScheduleDayStatus($row);
        };

        CMSMasterJs.ApplyDefaultWorkingHours = function () {
            $('#tblScheduleWeek .schedule-day-row').each(function () {
                var $row = $(this);

                if ($row.find('input.js-shift-morning:checked').length > 0) {
                    $row.find('.time-morning-start')
                        .val($('#<%= hdfDefaultMorningStart.ClientID %>').val())
                        .prop('disabled', false).removeAttr('disabled');
                    $row.find('.time-morning-end')
                        .val($('#<%= hdfDefaultMorningEnd.ClientID %>').val())
                        .prop('disabled', false).removeAttr('disabled');
                }

                if ($row.find('input.js-shift-afternoon:checked').length > 0) {
                    $row.find('.time-afternoon-start')
                        .val($('#<%= hdfDefaultAfternoonStart.ClientID %>').val())
                        .prop('disabled', false).removeAttr('disabled');
                    $row.find('.time-afternoon-end')
                        .val($('#<%= hdfDefaultAfternoonEnd.ClientID %>').val())
                        .prop('disabled', false).removeAttr('disabled');
                }
            });
        };

        CMSMasterJs.InitializeShiftInputs = function () {
            $('#tblScheduleWeek .schedule-day-row').each(function () {
                var $row = $(this);
                var morningOn = $row.find('input.js-shift-morning:checked').length > 0;
                var afternoonOn = $row.find('input.js-shift-afternoon:checked').length > 0;

                $row.find('.time-morning-start, .time-morning-end')
                    .prop('disabled', !morningOn);
                $row.find('.time-afternoon-start, .time-afternoon-end')
                    .prop('disabled', !afternoonOn);

                CMSMasterJs.UpdateScheduleDayStatus($row);
            });
        };
        $(document).ready(function () {
            // HÀM TRỊ BUG TÀNG HÌNH LƯỚI Ở TAB 2
            $('a[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
                // Lấy ID của tab vừa được mở lên
                var targetTab = $(e.target).attr("href");

                // Nếu đúng là tab Lịch ngoại lệ
                if (targetTab === "#lich-ngoai-le") {
                    // Ép trình duyệt kích hoạt sự kiện "resize" ngầm (như lúc bạn bấm F12)
                    // để GridViewExtension tự động tính toán lại và hiện ra!
                    setTimeout(function () {
                        $(window).trigger('resize');
                    }, 100); // Đợi 100ms cho tab mở hẳn rồi mới resize cho mượt
                }
            });

            CMSMasterJs.InitializeShiftInputs();

            CMSMasterJs.AddEndRequest(function () {
                CMSMasterJs.InitializeShiftInputs();
            });
        });
    </script>
</asp:Content>