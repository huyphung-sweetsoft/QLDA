<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="MeetList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.MeetList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fMeets/Controls/CtrlMeet.ascx" TagPrefix="SweetSoft" TagName="CtrlMeet" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server"></asp:Content>

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
                            <asp:TextBox runat="server" ID="txtNhanVienThamGia" CssClass="form-control" ReadOnly="true"></asp:TextBox>
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
    <SweetSoft:ExtraModal runat="server" ID="dlChonNhanVien" Type="Info" DefaultButton="btnXacNhanNhanVien">
        <ContentTemplate>
            <div class="row">
                <div class="col-12">
                    <table class="table table-bordered mb-0">
                        <thead class="table-light">
                            <tr>
                                <th><%= GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME) %></th>
                            </tr>
                        </thead>
                    </table>
                
                    <div style="max-height: 350px; overflow-y: auto; border: 1px solid #dee2e6; border-top: none;">
                        <asp:CheckBoxList runat="server" ID="cblNhanVien" Width="100%" 
                            CssClass="table table-hover table-borderless mb-0" 
                            RepeatLayout="Table" 
                            RepeatColumns="1" 
                            RepeatDirection="Vertical">
                        </asp:CheckBoxList>
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
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server"></asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
$(document).ready(function() {
    function calcMeetingTime() {
            var startText = $('#<%= txtThoiGianBatDau.ClientID %>').val();
            var durationText = $('#<%= txtThoiLuong.ClientID %>').val();

            if (startText && durationText) {
                // Tách chuỗi dd/MM/yyyy HH:mm
                var parts = startText.split(' ');
                var dmy = parts[0].split('/');
                var hm = parts[1].split(':');

                if (dmy.length === 3 && hm.length === 2) {
                    // Tạo đối tượng Date (Năm, Tháng (0-11), Ngày, Giờ, Phút)
                    var startDate = new Date(dmy[2], parseInt(dmy[1]) - 1, dmy[0], hm[0], hm[1]);
                    var minutes = parseInt(durationText, 10);

                    if (!isNaN(minutes) && minutes > 0) {
                        // 1. Cộng phút để tính Thời gian kết thúc
                        var endDate = new Date(startDate.getTime());
                        endDate.setMinutes(endDate.getMinutes() + minutes);

                        // Format lại thành chuỗi xuất ra UI
                        var endStr = String(endDate.getDate()).padStart(2, '0') + '/' + 
                                     String(endDate.getMonth() + 1).padStart(2, '0') + '/' + 
                                     endDate.getFullYear() + ' ' + 
                                     String(endDate.getHours()).padStart(2, '0') + ':' + 
                                     String(endDate.getMinutes()).padStart(2, '0');
                        
                        $('#<%= txtThoiGianKetThuc.ClientID %>').val(endStr);

                        // 2. Tính luôn trạng thái cuộc họp ngay trên màn hình
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

        $(document).on('change blur focusout keyup', '#<%= txtThoiGianBatDau.ClientID %>, #<%= txtThoiLuong.ClientID %>', function () {
            calcMeetingTime();
        });
    });
    </script>
</asp:Content>