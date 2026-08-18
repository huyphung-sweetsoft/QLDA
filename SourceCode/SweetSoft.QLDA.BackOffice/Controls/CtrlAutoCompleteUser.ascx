<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlAutoCompleteUser.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.CtrlAutoCompleteUser" %>

<%@ Register Src="~/Controls/AutoComplete/CtrExtraAutoComplete.ascx" TagPrefix="SweetSoft" TagName="CtrExtraAutoComplete" %>

<SweetSoft:CtrExtraAutoComplete runat="server" ID="acbbUser" ComboStyle="Default" OnClientChange="ChangeUser" />

<div class="bg-warning-subtle text-warning fc-event" style="margin: 5px 0; text-align: left;">
    <i style="width: 25px;" class="fas fa-mail-bulk"></i><span style="color: #000" data-selector="<%=acbbUser.ClientID%>Email"><%=ItemEmail %></span>
</div>
<div class="bg-warning-subtle text-warning fc-event" style="margin: 5px 0; text-align: left;">
    <i style="width: 25px;" class="fas fa-address-card"></i><span style="color: #000" data-selector="<%=acbbUser.ClientID%>FullName"><%=ItemFullName %></span>
</div>
<script>
    const renderItem = () => {
        const val = $('#<%=acbbUser.ClientID%>_hdfValue').val();
        if (typeof (val) == 'undefined' || val == '')
            return;
        const data = JSON.parse(val);
        if (!data)
            return;
        const otherData = JSON.parse(data[0].OtherData);
        if (!otherData)
            return;
        $('[data-selector="<%=acbbUser.ClientID%>Email"]').text(otherData.Email);
        $('[data-selector="<%=acbbUser.ClientID%>FullName"]').text(otherData.FullName);
    }

    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(renderItem);
</script>