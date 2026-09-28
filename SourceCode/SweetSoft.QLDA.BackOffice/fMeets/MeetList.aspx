<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="MeetList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.MeetList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fMeets/Controls/CtrlMeet.ascx" TagPrefix="SweetSoft" TagName="CtrlMeet" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        /* ==========================================================
           1. ĐỊNH DẠNG CUỘN VÀ DANH SÁCH NHÂN VIÊN
           ========================================================== */
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

        /* =========================================================================
           2. SIÊU PHẨM UI: BÁNH XE CHỌN THỜI LƯỢNG (VÒNG TRÒN)
           ========================================================================= */
        .wheel-popup {
            position: absolute;
            top: calc(100% + 15px);
            left: 50%;
            transform: translateX(-50%);
            width: 320px;
            height: 320px;
            background: #ffffff;
            border: 1px solid #e2e8f0;
            border-radius: 50%;
            z-index: 9999;
            box-shadow: 0 10px 40px rgba(0,0,0,0.15);
            display: none;
        }
        .wheel-center {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 70px;
            height: 70px;
            background: #f8fafc;
            border-radius: 50%;
            box-shadow: inset 0 2px 6px rgba(0,0,0,0.1);
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 700;
            color: #475569;
            font-size: 13px;
            text-align: center;
            pointer-events: none;
            border: 2px solid #e2e8f0;
        }
        .wheel-item {
            width: 70px;
            height: 70px;
            border-radius: 50%;
            background: #f1f5f9;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 12px;
            font-weight: 600;
            color: #1e293b;
            cursor: pointer;
            transition: 0.2s cubic-bezier(0.4, 0, 0.2, 1);
            text-align: center;
            border: 1px solid #cbd5e1;
            line-height: 1.2;
        }
        .wheel-item:hover {
            background: #3b82f6;
            color: white;
            border-color: #2563eb;
            transform: scale(1.15);
            box-shadow: 0 5px 15px rgba(59, 130, 246, 0.4);
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
            <div class="row js-validation validationEngineContainer">
        
                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.MEETING_NAME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtTenCuocHop" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <!-- ĐÃ TÁCH: Ô CHỌN NGÀY -->
                <div class="col-lg-3">
                    <div class="mb-3">
                        <label class="form-label label-valid">Ngày bắt đầu</label>
                        <SweetSoft:ExtraDateTime runat="server" ID="txtNgayBatDau" Required="true" 
                            SingleDatePicker="true" Format="dd/MM/yyyy" PlaceHolder="Ngày..." />
                    </div>
                </div>

                <!-- ĐÃ TÁCH: Ô CHỌN GIỜ (TEXT THUẦN) -->
                <div class="col-lg-3">
                    <div class="mb-3">
                        <label class="form-label label-valid">Giờ (HH:mm)</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtGioBatDau" Required="true" PlaceHolder="VD: 09:00" MaxLength="5"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <!-- THỜI LƯỢNG KÈM BÁNH XE -->
                <div class="col-lg-6">
                    <div class="mb-3" style="position: relative;">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.DURATION) %>(Minutes)</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtThoiLuong" Required="true" AutoCompleteType="Disabled" PlaceHolder="Bấm vào để chọn..."></SweetSoft:ExtraTextBox>
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
                var dateText = $('#<%= txtNgayBatDau.ClientID %>').val();
                var timeText = $('#<%= txtGioBatDau.ClientID %>').val();
                var durationText = $('#<%= txtThoiLuong.ClientID %>').val();

                if (dateText && timeText && durationText) {
                    var dmy = dateText.split('/');
                    var hm = timeText.split(':');

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

                            if (now > endDate) { status = 3; } 
                            else if (now >= startDate && now <= endDate) { status = 2; } 
                            else {
                                var diffMinutes = (startDate - now) / 60000;
                                if (diffMinutes > 0 && diffMinutes <= 15) { status = 1; }
                            }
                            $('#<%= ddlTrangThai.ClientID %>').val(status).trigger('change');
                        }
                    }
                }
            }

            // Gắn sự kiện thay đổi dữ liệu
            $(document).off('change blur focusout keyup', '#<%= txtNgayBatDau.ClientID %>, #<%= txtGioBatDau.ClientID %>, #<%= txtThoiLuong.ClientID %>')
                     .on('change blur focusout keyup', '#<%= txtNgayBatDau.ClientID %>, #<%= txtGioBatDau.ClientID %>, #<%= txtThoiLuong.ClientID %>', function () {
                    calcMeetingTime();
                 });

            // =================================================================================
            // LOGIC DEFAULT: MẶC ĐỊNH NGÀY HÔM NAY - GIỜ LÀM TRÒN LÊN TỚI ĐỈNH (VD 11:29 -> 12:00)
            // =================================================================================
            if (!$('#<%= txtNgayBatDau.ClientID %>').val()) {
                var now = new Date();
                var d = String(now.getDate()).padStart(2, '0');
                var mo = String(now.getMonth() + 1).padStart(2, '0');
                var y = now.getFullYear();
                $('#<%= txtNgayBatDau.ClientID %>').val(d + '/' + mo + '/' + y);

                var h = now.getHours();
                var m = now.getMinutes();
                if (m > 0) h++; // Phút > 0 thì nhảy thẳng lên giờ tiếp theo
                if (h > 23) h = 0; 
                $('#<%= txtGioBatDau.ClientID %>').val(String(h).padStart(2, '0') + ':00');
            }

            // =================================================================================
            // VALIDATE TEXTBOX GIỜ: Chỉ cho nhập số, tự nhảy ":", chặn trên 23h, 59p
            // =================================================================================
            $('#<%= txtGioBatDau.ClientID %>').on('input', function() {
                var val = $(this).val().replace(/[^0-9]/g, ''); // Xóa chữ, chỉ để số
                if (val.length >= 3) {
                    val = val.substring(0, 2) + ':' + val.substring(2, 4);
                }
                $(this).val(val);
            }).on('blur', function() {
                var val = $(this).val();
                if (val) {
                    var parts = val.split(':');
                    var h = parseInt(parts[0]) || 0;
                    var m = parseInt(parts[1]) || 0;
                    if (h > 23) h = 23;
                    if (m > 59) m = 59;
                    $(this).val(String(h).padStart(2, '0') + ':' + String(m).padStart(2, '0'));
                    calcMeetingTime(); // Buộc tính lại kết thúc
                }
            });

            // =================================================================================
            // SIÊU PHẨM UI: BÁNH XE THỜI LƯỢNG (CIRCLE WHEEL GIỐNG BÁNH XE CUỘC ĐỜI)
            // =================================================================================
            var $thoiLuongInput = $('#<%= txtThoiLuong.ClientID %>');
            
            if ($('#durationWheel').length === 0) {
                var durHtml = '<div id="durationWheel" class="wheel-popup"><div class="wheel-center">Thời<br/>Lượng</div>';
                var steps = [15, 30, 45, 60, 75, 90, 105, 120, 135, 150, 165, 180];
                
                steps.forEach(function(val, index) {
                    // Logic Format Text (VD: 165p -> 2 tiếng 45p)
                    var h = Math.floor(val / 60);
                    var m = val % 60;
                    var label = "";
                    if (h === 0) label = m + 'p';
                    else if (m === 0) label = h + ' tiếng';
                    else label = h + ' tiếng<br/>' + m + 'p';
                    
                    // Toán học tọa độ hình tròn (Bắt đầu từ trên cùng (-90 độ) quay theo chiều kim đồng hồ)
                    var angle = (index * 30 - 90) * (Math.PI / 180);
                    var radius = 110; 
                    var x = radius * Math.cos(angle);
                    var y = radius * Math.sin(angle);
                    
                    durHtml += '<div style="position:absolute; top:125px; left:125px; transform: translate('+x+'px, '+y+'px);">' +
                               '<div class="wheel-item" data-val="'+val+'">' + label + '</div>' +
                               '</div>';
                });
                durHtml += '</div>';
                $thoiLuongInput.after(durHtml);
            }

            $thoiLuongInput.on('focus click', function(e) {
                $('#durationWheel').fadeIn(150);
            });

            $(document).on('click', '.wheel-item', function(e) {
                var selectedValue = $(this).data('val');
                $thoiLuongInput.val(selectedValue).trigger('change'); 
                $('#durationWheel').fadeOut(150);
            });

            $(document).on('click', function(e) {
                if(!$(e.target).closest('#durationWheel, #<%= txtThoiLuong.ClientID %>').length) {
                    $('#durationWheel').fadeOut(150);
                }
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