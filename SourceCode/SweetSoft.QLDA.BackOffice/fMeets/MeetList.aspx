<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="MeetList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.MeetList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fMeets/Controls/CtrlMeet.ascx" TagPrefix="SweetSoft" TagName="CtrlMeet" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <!-- BẮT ĐẦU THÊM: CSS CHO AVATAR TRONG POPUP -->
    <style>
        .single-avatar-circle {
            width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; 
            font-size: 11px; font-weight: 700; color: #ffffff; flex-shrink: 0; box-shadow: 0 1px 2px rgba(0,0,0,0.1);
        }
        .member-item-label {
            display: flex; align-items: center; gap: 10px; padding: 10px 12px; cursor: pointer; border-bottom: 1px solid #f1f5f9; transition: background 0.2s;
        }
        .member-item-label:hover { background-color: #f8fafc; }
        .member-item-label input[type="checkbox"] { width: 16px; height: 16px; cursor: pointer; accent-color: #2563eb; margin: 0; }
    </style>
    <!-- KẾT THÚC THÊM -->
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
                            TextMode="Number" min="1" PlaceHolder="Ví dụ: 60">
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

                <!-- BẮT ĐẦU SỬA: TEXTBOX HIỂN THỊ MULTILINE -->
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
                <!-- KẾT THÚC SỬA -->

                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.MEETING_ROOM) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtDiaDiemHop" Required="true"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <div class="col-lg-12">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.CONTENT) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtNoiDungCuocHop" TextMode="MultiLine" Rows="4"></SweetSoft:ExtraTextBox>
                    </div>
                </div>

            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true"
                OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false">Lưu</SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>

    <!-- BẮT ĐẦU SỬA: POPUP CHỌN NHÂN VIÊN (SEARCH + CHỌN TẤT CẢ + REPEATER AVATAR) -->
    <SweetSoft:ExtraModal runat="server" ID="dlChonNhanVien" Type="Info" DefaultButton="btnXacNhanNhanVien">
        <ContentTemplate>
            <div class="row align-items-center mb-3">
                <div class="col-md-7 mb-2 mb-md-0">
                    <div class="input-group">
                        <span class="input-group-text bg-white text-muted"><i class="fas fa-search"></i></span>
                        <input type="text" id="txtSearchEmployee" class="form-control border-start-0 ps-0" placeholder="Tìm tên nhân viên..." autocomplete="off" />
                    </div>
                </div>
                <div class="col-md-5 d-flex justify-content-md-end">
                    <div class="form-check form-switch custom-switch-primary">
                        <input class="form-check-input cursor-pointer" type="checkbox" id="chkSelectAllEmployees">
                        <label class="form-check-label fw-bold cursor-pointer user-select-none" for="chkSelectAllEmployees">Chọn tất cả</label>
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-12">
                    <div style="max-height: 350px; overflow-y: auto; border: 1px solid #dee2e6; border-radius: 6px;">
                        <asp:Repeater ID="rptNhanVien" runat="server" OnItemDataBound="rptNhanVien_ItemDataBound">
                            <ItemTemplate>
                                <label class="member-item-label m-0 w-100">
                                    <asp:CheckBox runat="server" ID="chkSelect" />
                                    <asp:HiddenField runat="server" ID="hdfUserId" Value='<%# Eval("UserId") %>' />
                                    <asp:HiddenField runat="server" ID="hdfDisplayName" Value='<%# Eval("DisplayName") %>' />
                                    
                                    <%# Eval("AvatarHtml") %>
                                    
                                    <span class="fw-bold text-dark"><%# Eval("DisplayName") %></span>
                                </label>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <FooterTemplate>
            <SweetSoft:ExtraButton runat="server" ID="btnXacNhanNhanVien" CssClass="waves-effect waves-light" ButtonStyle="Primary" OnClick="btnXacNhanNhanVien_Click">
                <%= GetResourceText(BackEndResourceKeys.CONFIRM) %>
            </SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>
    <!-- KẾT THÚC SỬA -->
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        // BỌC JS VÀO PageLoaded ĐỂ KHÔNG BỊ LỖI KHI POSTBACK
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
            
            // Hàm tính giờ (Giữ nguyên của bác)
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
                            var status = 0; // Đã lên lịch

                            if (now > endDate) {
                                status = 3; // Kết thúc
                            } else if (now >= startDate && now <= endDate) {
                                status = 2; // Đang diễn ra
                            } else {
                                var diffMinutes = (startDate - now) / 60000;
                                if (diffMinutes > 0 && diffMinutes <= 15) {
                                    status = 1; // Sắp diễn ra
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

            // BẮT ĐẦU SỬA: TÍNH NĂNG TÌM KIẾM VÀ CHỌN TẤT CẢ CHO CheckBoxList
            var $searchBox = $('#txtSearchEmployee');
            var $selectAll = $('#chkSelectAllEmployees');
            var $chkListRows = $('.member-item-label');

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
            // KẾT THÚC SỬA JS
        });
    </script>
</asp:Content>