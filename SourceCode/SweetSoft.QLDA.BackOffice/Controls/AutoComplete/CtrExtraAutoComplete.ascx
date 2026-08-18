<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrExtraAutoComplete.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.AutoComplete.CtrExtraAutoComplete" %>

<div data-autocomplete="true" class="autocomplete-elm" <%=RenderAttributes %>>
    <asp:TextBox type="hidden" runat="server" ID="hdfValue" OnTextChanged="hdfValue_ServerChange" AutoPostBack="true" CssClass="validate"></asp:TextBox>
    <div style="display: flex; width: 100%;">
    </div>
</div>
