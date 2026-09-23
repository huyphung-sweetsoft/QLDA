<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="MeetList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.MeetList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fMeets/Controls/CtrlMeet.ascx" TagPrefix="SweetSoft" TagName="CtrlMeet" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .single-avatar-circle {
            width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; 
            font-size: 12px; font-weight: 700; color: #ffffff; flex-shrink: 0; box-shadow: 0 2px 4px rgba(0,0,0,0.08);
        }
        
        .member-item-label {
            display: flex; align-items: center; gap: 12px; padding: 10px 14px; cursor: pointer; 
            border-bottom: 1px solid #f1f5f9; transition: background-color 0.2s ease; background-color: #fff;
        }
        .member-item-label:last-child { border-bottom: none; }
        .member-item-label:hover { background-color: #f8fafc; }
        .member-item-label input[type="checkbox"] { 
            width: 18px; height: 18px; cursor: pointer; accent-color: #2563eb; margin: 0; border-radius: 4px;
        }
        
        .employee-scroll-container::-webkit-scrollbar { width: 6px; }
        .employee-scroll-container::-webkit-scrollbar-track { background: #f1f5f9; border-radius: 6px; }
        .employee-scroll-container::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 6px; }
        .employee-scroll-container::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
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

    <SweetSoft:ExtraModal runat="server" ID="dlChonNhanVien" Type="Info" DefaultButton="btnXacNhanNhanVien" Title="Chọn nhân viên tham gia">
        <ContentTemplate>
            <div class="p-1">
                <div class="row align-items-center mb-3 g-2">
                    <div class="col-md-7">
                        <div class="input-group shadow-sm rounded-pill overflow-hidden border">
                            <span class="input-group-text bg-white border-0 text-muted ps-3"><i class="fas fa-search"></i></span>
                            <input type="text" id="txtSearchEmployee" class="form-control border-0 shadow-none ps-2" placeholder="Tìm kiếm theo tên nhân viên..." autocomplete="off" />
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
                        <div class="employee-scroll-container" style="max-height: 380px; overflow-y: auto; border: 1px solid #e2e8f0; border-radius: 10px; background-color: #fff; box-shadow: inset 0 1px 3px rgba(0,0,0,0.02);">
                            <asp:Repeater ID="rptNhanVien" runat="server" OnItemDataBound="rptNhanVien_ItemDataBound">
                                <ItemTemplate>
                                    <label class="member-item-label m-0 w-100">
                                        <asp:CheckBox runat="server" ID="chkSelect" />
                                        <asp:HiddenField runat="server" ID="hdfUserId" Value='<%# Eval("UserId") %>' />
                                        <asp:HiddenField runat="server" ID="hdfDisplayName" Value='<%# Eval("DisplayName") %>' />
                                        
                                        <%# Eval("AvatarHtml") %>
                                        
                                        <span class="fw-semibold text-dark"><%# Eval("DisplayName") %></span>
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
        });
    </script>
</asp:Content>