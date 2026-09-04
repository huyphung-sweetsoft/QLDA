<%@ Page Title="" Language="C#"
    MasterPageFile="~/MasterPages/MasterTemplate.Master"
    AutoEventWireup="true"
    CodeBehind="DocumentGroups.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.DocumentGroups" %>
<%@ Register Src="~/fDocuments/Controls/CtrlDocumentGroups.ascx"
    TagPrefix="SweetSoft"
    TagName="CtrlDocumentGroups" %>

<asp:Content ID="ContentMain"
    ContentPlaceHolderID="cpMain"
    runat="server">

    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen">
                <SweetSoft:Navigation
                    runat="server"
                    ID="Navigation1" />

                <SweetSoft:CtrlDocumentGroups
                    runat="server"
                    ID="CtrlDocumentGroups1" />
            </div>
        </div>
    </div>

</asp:Content>
