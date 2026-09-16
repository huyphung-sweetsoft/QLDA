<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MasterTemplate.Master" CodeBehind="MeetList.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fMeets.MeetList" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.Managers" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fMeets/Controls/CtrlMeet.ascx" TagPrefix="SweetSoft" TagName="CtrlMeet" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx" TagPrefix="SweetSoft" TagName="CtrlProjectTabs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
<style>
    .badge-status {
        padding: 4px 10px !important;
        border-radius: 6px !important;
        font-size: 11.5px !important;
        font-weight: 600 !important;
        display: inline-block !important;
        white-space: nowrap !important;
        line-height: 1.2 !important;
    }

    .badge-status-scheduled {
        background-color: #e0f2fe !important;
        color: #0284c7 !important;
        border: 1px solid #bae6fd !important;
    }

    .badge-status-upcoming {
        background-color: #fffbeb !important;
        color: #b45309 !important;
        border: 1px solid #fde68a !important;
    }

    .badge-status-ongoing {
        background-color: #dcfce7 !important;
        color: #15803d !important;
        border: 1px solid #bbf7d0 !important;
    }

    .badge-status-ended {
        background-color: #f3f4f6 !important;
        color: #4b5563 !important;
        border: 1px solid #e5e7eb !important;
    }

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
    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" DefaultButton="lbtSubmit">
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
                            SingleDatePicker="true" TimePicker="true" TimePicker24Hour="true" Format="dd/MM/yyyy HH:mm" />
                    </div>
                </div>
                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.MEET_DURATION) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtThoiLuong" Required="true" 
                            TextMode="Number" min="1">
                        </SweetSoft:ExtraTextBox>
                    </div>
                </div>

                <div class="col-lg-6">
                    <div class="mb-3">
                        <label class="form-label"><%= GetResourceText(BackEndResourceKeys.END_TIME) %></label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtThoiGianKetThuc" Enabled="false" CssClass="disabled"></SweetSoft:ExtraTextBox>
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
                OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false"><%= GetResourceText(BackEndResourceKeys.SAVE) %></SweetSoft:ExtraButton>
        </FooterTemplate>
    </SweetSoft:ExtraModal>

    <SweetSoft:ExtraModal runat="server" ID="dlChonNhanVien" Type="Info" DefaultButton="btnXacNhanNhanVien">
        <ContentTemplate>
            <div class="row">
                <div class="col-12">
                    <div style="background: #f8f9fa; padding: 10px 15px; font-weight: bold; border: 1px solid #dee2e6; border-bottom: none;">
                        <%= GetResourceText(BackEndResourceKeys.EMPLOYEE_NAME) %>
                    </div>
                    <div style="max-height: 350px; overflow-y: auto; border: 1px solid #dee2e6;">
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
            <SweetSoft:ExtraButton runat="server" ID="btnXacNhanNhanVien" 
                CssClass="waves-effect waves-light" 
                ButtonStyle="Primary" 
                ButtonIcon="Check" 
                OnClick="btnXacNhanNhanVien_Click">
            </SweetSoft:ExtraButton>
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

        $(document).on('change blur focusout keyup', '#<%= txtThoiGianBatDau.ClientID %>, #<%= txtThoiLuong.ClientID %>', function () {
            calcMeetingTime();
        });
    });
    </script>
</asp:Content>