<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GridviewPaging.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.Controls.GridviewPaging" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<div>
    <asp:UpdatePanel runat="server" ID="pnlPaging" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="d-flex justify-content-between mt-2 gap-2 table-paging">
                <div class="text-small text-color-1 d-flex align-items-center">
                    <%=GetResourceText(BackEndResourceKeys.SHOW) %>&nbsp;
        <asp:Label ID="lblCurrentPage" runat="server"></asp:Label>
                    &nbsp;<%= GetResourceText(BackEndResourceKeys.TO) %>&nbsp;
        <%--<asp:Label runat="server" ID="lbTotalRow"></asp:Label>--%>
                    <div style="width: 70px">
                        <asp:DropDownList runat="server" ID="ddlPageSize" CssClass="form-control text-center"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="DropDownListPageSize_SelectedIndexChanged">
                            <asp:ListItem Value="10" />
                            <asp:ListItem Value="20" />
                            <asp:ListItem Value="30" />
                            <asp:ListItem Value="50" />
                            <asp:ListItem Value="100" />
                            <asp:ListItem Value="200" />
                            <asp:ListItem Value="300" />
                            <asp:ListItem Value="500" />
                        </asp:DropDownList>
                    </div>
                    &nbsp;<%=GetResourceText(BackEndResourceKeys.OF) %>&nbsp;
        <asp:Label ID="lblTotalPages" runat="server"></asp:Label>
                    &nbsp;<%=GetResourceText(BackEndResourceKeys.ITEM) %>
                </div>
                <div class="wrapperListPaging">
                    <div class=" flex-center">
                        <ul class="clearfix wrapPaggingList mb-0">
                            <asp:Literal ID="ltrLink" runat="server"></asp:Literal>
                        </ul>
                    </div>
                </div>
                <div class="col-xs-hidden table-custom-show-column"></div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
