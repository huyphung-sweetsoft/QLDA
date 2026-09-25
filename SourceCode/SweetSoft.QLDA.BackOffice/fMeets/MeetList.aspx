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

        .member-item-row { position: relative; background: white; border: 1px solid #e2e8f0; border-radius: 8px; margin-bottom: 8px; overflow: hidden; display: flex; flex-direction: column; align-items: stretch; transition: border-color 0.2s, box-shadow 0.2s; cursor: pointer; }
        .member-item-row:hover { border-color: #93c5fd; background-color: #f8fafc; }
        .member-item-row:last-child { margin-bottom: 0; }
        
        .row-default-view { display: flex; align-items: center; padding: 8px 12px; width: 100%; min-height: 52px; box-sizing: border-box; }
        .member-info-group { display: flex; align-items: center; gap: 12px; width: 100%; min-width: 0; }
        .member-info-group input[type="checkbox"] { width: 18px; height: 18px; cursor: pointer; accent-color: #2563eb; margin: 0; border-radius: 4px; border: 1px solid #cbd5e1; }
        .single-avatar-circle { width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 12px; font-weight: 700; color: #ffffff; flex-shrink: 0; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
        .member-name-block { display: flex; flex-direction: column; min-width: 0; line-height: 1.25; }
        .member-name-block .fw-bold { font-size: 14px; font-weight: 600 !important; color: #1e293b; }
        .member-email { font-size: 12px; color: #64748b; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; margin-top: 3px; }
        .member-email:empty { display: none; }
        .record-attachments .file-actions { display: none !important; }

        .daterangepicker.wizard-mode { 
            width: 520px !important; /* Mở rộng tối đa popup để chứa đủ 10 cột */
            padding: 0; 
            overflow: hidden; 
            border-radius: 12px; 
            box-shadow: 0 10px 30px rgba(0,0,0,0.15); 
            border: none; 
        }
        
        .daterangepicker.wizard-mode .calendar-time { display: none !important; }
        .daterangepicker.wizard-mode .drp-buttons { display: none !important; }
        
        /* Ép khung chứa hiển thị full 100% width, không bị rớt sang trái */
        .daterangepicker.wizard-mode .drp-calendar.left { 
            max-width: none !important; 
            width: 100% !important; 
            padding: 20px !important; 
            float: none !important; 
            clear: both !important;
        }

        /* Căn giữa cái lịch ngày */
        .daterangepicker.wizard-mode .calendar-table {
            display: flex;
            justify-content: center;
            width: 100%;
        }
        .daterangepicker.wizard-mode .calendar-table table {
            min-width: 350px;
        }
        
        .wizard-step-hour, .wizard-step-minute { display: none; width: 100%; }
        
        .wizard-header { 
            font-size: 16px; 
            font-weight: 700; 
            color: #1e293b; 
            text-align: center; 
            margin-bottom: 20px; 
            text-transform: uppercase; 
            padding-bottom: 12px; 
            border-bottom: 1px dashed #cbd5e1; 
            letter-spacing: 0.5px; 
        }
        
        /* CẤU TRÚC LƯỚI TRÀN VIỀN */
        .time-grid-hour { 
            display: grid; 
            grid-template-columns: repeat(8, 1fr); /* Đẹp nhất: 8 cột (3 hàng x 8 = 24) */
            gap: 12px; 
            width: 100%;
            max-height: 320px; 
            overflow-y: auto; 
            padding: 5px; 
        }
        
        .time-grid-minute { 
            display: grid; 
            grid-template-columns: repeat(10, 1fr); /* Đẹp nhất: 10 cột (6 hàng x 10 = 60) */
            gap: 10px; 
            width: 100%;
            max-height: 320px; 
            overflow-y: auto; 
            padding: 5px; 
        }
        
        .time-grid-hour::-webkit-scrollbar, .time-grid-minute::-webkit-scrollbar { width: 0px; /* Ẩn luôn scrollbar vì lưới đã đủ chỗ hiển thị toàn bộ */ }
        
        /* Nút bấm tinh tế, vừa vặn */
        .time-btn { 
            padding: 12px 0; 
            text-align: center; 
            border: 1px solid #e2e8f0; 
            border-radius: 8px; /* Bo góc nhẹ nhìn hiện đại hơn viên thuốc */
            background: #ffffff; 
            cursor: pointer; 
            font-size: 14px; 
            font-weight: 600; 
            color: #475569; 
            transition: all 0.2s ease; 
            box-shadow: 0 1px 3px rgba(0,0,0,0.05); 
            user-select: none; 
        }
        .time-btn:hover { 
            background: #f1f5f9; 
            border-color: #94a3b8; 
            transform: translateY(-2px); 
            box-shadow: 0 4px 6px rgba(0,0,0,0.05); 
        }
        .time-btn.selected { 
            background: #3b82f6 !important; 
            color: #ffffff !important; 
            border-color: #2563eb !important; 
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.35) !important; 
            transform: translateY(-1px); 
        }
        
        .wizard-footer { 
            display: flex; 
            justify-content: space-between; 
            align-items: center; 
            padding: 15px 20px; 
            background: #f8fafc; 
            border-top: 1px solid #e2e8f0; 
            clear: both; 
        }
        .wiz-btn { 
            padding: 10px 20px; 
            border-radius: 8px; 
            font-weight: 600; 
            font-size: 14px; 
            border: none; 
            cursor: pointer; 
            transition: 0.2s; 
            display: flex; 
            align-items: center; 
            gap: 8px; 
            box-shadow: 0 2px 4px rgba(0,0,0,0.05); 
            outline: none; 
        }
        .wiz-back { background: #e2e8f0; color: #475569; }
        .wiz-back:hover { background: #cbd5e1; }
        .wiz-next { background: #3b82f6; color: white; margin-left: auto; }
        .wiz-next:hover { background: #2563eb; }
        .wiz-finish { background: #10b981; color: white; margin-left: auto; }
        .wiz-finish:hover { background: #059669; }
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
            <div class="row js-validation validationEngineContainer">
        
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.MEETING_NAME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenCuocHop" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

               <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.START_TIME) %></label>
                        <SweetSoft:ExtraDateTime runat="server" ID="txtThoiGianBatDau" Required="true" 
                            SingleDatePicker="true" TimePicker="true" TimePicker24Hour="true" Format="dd/MM/yyyy HH:mm" PlaceHolder="Chọn ngày giờ bắt đầu..." />
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.DURATION) %>(Minutes)</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtThoiLuong" Required="true" 
                            TextMode="Number" min="1">
                        </SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.END_TIME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtThoiGianKetThuc" Enabled="false" CssClass="disabled" PlaceHolder="Hệ thống tự tính..."></SweetSoft:ExtraTextBox>
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.STATUS) %></label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlTrangThai" SimpleInit="true" Enabled="false" CssClass="disabled"></SweetSoft:ExtraDropdown>
                    </div>
                </div>

                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME) %></label>
                        <div class="input-group">
                            <asp:TextBox runat="server" ID="txtNhanVienThamGia" CssClass="form-control bg-white" ReadOnly="true" TextMode="MultiLine" Rows="2" Style="resize: none;"></asp:TextBox>
                            <asp:HiddenField runat="server" ID="hdfNhanVienIds" />
                            <asp:LinkButton runat="server" ID="btnMoPopupNhanVien" CssClass="btn btn-secondary" OnClick="btnMoPopupNhanVien_Click">
                                <i class="fa fa-users"></i> <%= GetResourceText(BackEndResourceKeys.SELECT_EMPLOYEE) %>
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>

                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.MEETING_ROOM) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtDiaDiemHop" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CONTENT) %></label>
                        <CKEditor:CKEditorControl runat="server" ID="txtNoiDungCuocHop" Width="100%"
                            CssClass="ck-editor" Toolbar="Full" Language="vi-VN"
                            AutoParagraph="false" BasePath="~/Styles/plugins/ckeditor/" Height="200" />
                        <div class="form-text">File đính kèm được tải bằng nút thư mục của cuộc họp trong danh sách.</div>
                    </div>
                </div>

            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true"
                OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false">Lưu</SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>

    <SweetSoft:ExtraModal runat="server" ID="dlMeetingFiles" Type="Primary" Size="Small" Title="File đính kèm lịch họp">
        <ContentTemplate>
            <div class="record-attachments">
                <SweetSoft:FilesBox runat="server" ID="fbMeetingFiles" IsMultiple="false" MaxFileSizeBytes="10485760" />
            </div>
        </ContentTemplate>
    </SweetSoft:ExtraModal>

    <!-- POPUP CHỌN NHÂN VIÊN -->
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
            <SweetSoft:ExtraButton runat="server" ID="btnXacNhanNhanVien" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" OnClick="btnXacNhanNhanVien_Click">
                Xác nhận
            </SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
            
            // --- HÀM TÍNH THỜI GIAN KẾT THÚC ---
            function calcMeetingTime() {
                var startText = $('#<%= txtThoiGianBatDau.ClientID %>').val();
                var durationText = $('#<%= txtThoiLuong.ClientID %>').val();

                if (startText && durationText) {
                    var parts = startText.split(' ');
                    var dmy = parts[0].split('/');
                    var hm = parts[1].split(':');

                    if (dmy.length === 3 && hm.length === 2) {
                        var startDate = new Date(dmy[2], parseInt(dmy[1]) - 1, dmy[0], hm[0], hm[1]);
                        var minutes = parseInt(durationText, 10);

                        if (!isNaN(minutes) && minutes > 0) {
                            var endDate = new Date(startDate.getTime());
                            endDate.setMinutes(endDate.getMinutes() + minutes);

                            var endStr = String(endDate.getDate()).padStart(2, '0') + '/' +
                                String(endDate.getMonth() + 1).padStart(2, '0') + '/' +
                                endDate.getFullYear() + ' ' +
                                String(endDate.getHours()).padStart(2, '0') + ':' +
                                String(endDate.getMinutes()).padStart(2, '0');

                            $('#<%= txtThoiGianKetThuc.ClientID %>').val(endStr);

                            var now = new Date();
                            var status = 0;

                            if (now > endDate) {
                                status = 3; 
                            } else if (now >= startDate && now <= endDate) {
                                status = 2; 
                            } else {
                                var diffMinutes = (startDate - now) / 60000;
                                if (diffMinutes > 0 && diffMinutes <= 15) {
                                    status = 1;
                                }
                            }
                            $('#<%= ddlTrangThai.ClientID %>').val(status).trigger('change');
                        }
                    }
                }
            }

            $(document).off('change blur focusout keyup', '#<%= txtThoiGianBatDau.ClientID %>, #<%= txtThoiLuong.ClientID %>')
                     .on('change blur focusout keyup', '#<%= txtThoiGianBatDau.ClientID %>, #<%= txtThoiLuong.ClientID %>', function () {
                    calcMeetingTime();
                 });

            // =================================================================================
            // SIÊU PHẨM UI: WIZARD STEP-BY-STEP (NGÀY -> GIỜ -> PHÚT)
            // =================================================================================
            var $timeInput = $('#<%= txtThoiGianBatDau.ClientID %>');
            
            $timeInput.off('show.daterangepicker.wizard').on('show.daterangepicker.wizard', function(ev, picker) {
                var $dp = picker.container;
                
                // Chỉ render HTML một lần duy nhất
                if ($dp.find('.wizard-footer').length === 0) {
                    $dp.addClass('wizard-mode'); 
                    
                    // 1. Tạo Lưới 24 Giờ (8 Cột)
                    var hourHtml = '<div class="wizard-step-hour"><div class="wizard-header">Chọn Giờ</div><div class="time-grid-hour">';
                    for(var h = 0; h < 24; h++) {
                        var txtH = h < 10 ? '0' + h : h;
                        hourHtml += '<div class="time-btn hour-btn" data-val="'+h+'">' + txtH + '</div>';
                    }
                    hourHtml += '</div></div>';
                    
                    // 2. Tạo Lưới 60 Phút (10 Cột)
                    var minHtml = '<div class="wizard-step-minute"><div class="wizard-header">Chọn Phút</div><div class="time-grid-minute">';
                    for(var m = 0; m < 60; m++) {
                        var txtM = m < 10 ? '0' + m : m;
                        minHtml += '<div class="time-btn min-btn" data-val="'+m+'">' + txtM + '</div>';
                    }
                    minHtml += '</div></div>';
                    
                    // 3. Tạo Footer chứa các nút điều hướng
                    var footerHtml = '<div class="wizard-footer">' +
                        '<button type="button" class="wiz-btn wiz-back"><i class="fa fa-arrow-left"></i> Quay lại</button>' +
                        '<button type="button" class="wiz-btn wiz-next">Chọn giờ <i class="fa fa-arrow-right"></i></button>' +
                        '<button type="button" class="wiz-btn wiz-finish">Hoàn tất <i class="fa fa-check"></i></button>' +
                        '</div>';
                        
                    // Ép HTML vào đúng vị trí của bộ chọn lịch
                    $dp.find('.drp-calendar.left').append(hourHtml + minHtml);
                    $dp.append(footerHtml);
                    
                    // Khởi tạo Trạng thái (0: Ngày, 1: Giờ, 2: Phút)
                    $dp.data('wiz-step', 0);
                    
                    // HÀM ĐIỀU CHỈNH GIAO DIỆN DỰA TRÊN BƯỚC (STEP)
                    function updateWizardUI() {
                        var step = $dp.data('wiz-step');
                        
                        // Ẩn hiện các màn hình
                        $dp.find('.calendar-table').toggle(step === 0);
                        $dp.find('.wizard-step-hour').toggle(step === 1);
                        $dp.find('.wizard-step-minute').toggle(step === 2);
                        
                        // Ẩn hiện nút Footer
                        $dp.find('.wiz-back').toggle(step > 0);
                        $dp.find('.wiz-next').toggle(step < 2);
                        $dp.find('.wiz-finish').toggle(step === 2);
                        
                        // Đổi Text nút Next
                        if (step === 0) $dp.find('.wiz-next').html('Chọn giờ <i class="fa fa-arrow-right"></i>');
                        if (step === 1) $dp.find('.wiz-next').html('Chọn phút <i class="fa fa-arrow-right"></i>');
                    }
                    
                    // XỬ LÝ SỰ KIỆN NÚT FOOTER
                    $dp.on('click', '.wiz-next', function() {
                        var s = $dp.data('wiz-step');
                        if (s < 2) { $dp.data('wiz-step', s + 1); updateWizardUI(); }
                    });
                    
                    $dp.on('click', '.wiz-back', function() {
                        var s = $dp.data('wiz-step');
                        if (s > 0) { $dp.data('wiz-step', s - 1); updateWizardUI(); }
                    });
                    
                    $dp.on('click', '.wiz-finish', function() {
                        $dp.find('.applyBtn').click(); // Bấm nút Apply ngầm để lưu
                    });
                    
                    // SỰ KIỆN KHI BẤM CHỌN MỘT GIỜ
                    $dp.on('click', '.hour-btn', function() {
                        $dp.find('.hour-btn').removeClass('selected');
                        $(this).addClass('selected');
                        $dp.find('.hourselect').val($(this).data('val')).trigger('change');
                        
                        // Tự động lướt sang màn hình chọn Phút
                        setTimeout(function() { $dp.find('.wiz-next').click(); }, 150);
                    });
                    
                    // SỰ KIỆN KHI BẤM CHỌN MỘT PHÚT
                    $dp.on('click', '.min-btn', function() {
                        $dp.find('.min-btn').removeClass('selected');
                        $(this).addClass('selected');
                        $dp.find('.minuteselect').val($(this).data('val')).trigger('change');
                    });
                }
                
                // MỖI KHI BẬT POPUP LÊN LÀ RESET VỀ BƯỚC 0 (CHỌN NGÀY)
                $dp.data('wiz-step', 0);
                
                // Gán CSS cho nút đang được chọn sẵn
                var curHour = $dp.find('.hourselect').val() || 0;
                $dp.find('.hour-btn').removeClass('selected').filter('[data-val="'+parseInt(curHour)+'"]').addClass('selected');
                
                var curMin = $dp.find('.minuteselect').val() || 0;
                $dp.find('.min-btn').removeClass('selected').filter('[data-val="'+parseInt(curMin)+'"]').addClass('selected');
                
                // Kích hoạt lại View Bước 0
                $dp.find('.calendar-table').show();
                $dp.find('.wizard-step-hour, .wizard-step-minute, .wiz-back, .wiz-finish').hide();
                $dp.find('.wiz-next').show().html('Chọn giờ <i class="fa fa-arrow-right"></i>');
            });


            // --- XỬ LÝ CHỌN NHÂN VIÊN ---
            var $searchBox = $('#<%= txtSearchSingle.ClientID %>');
            var $selectAll = $('#chkSelectAllEmployees');
            var $chkListRows = $('.member-item-row');

            $searchBox.val('');
            $selectAll.prop('checked', false);

            $searchBox.off('keyup').on('keyup', function () {
                var value = $(this).val().toLowerCase();
                $chkListRows.filter(function () {
                    $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
                });
                $selectAll.prop('checked', false);
            });

            $selectAll.off('change').on('change', function () {
                var isChecked = $(this).is(':checked');
                $chkListRows.filter(':visible').find('input[type="checkbox"]').prop('checked', isChecked);
            });

            $chkListRows.find('input[type="checkbox"]').off('change').on('change', function () {
                if (!$(this).is(':checked')) {
                    $selectAll.prop('checked', false);
                }
            });
        });
    </script>
</asp:Content>