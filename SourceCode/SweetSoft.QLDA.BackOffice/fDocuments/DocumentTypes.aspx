<%@ Page Title="" Language="C#"
    MasterPageFile="~/MasterPages/MasterTemplate.Master"
    AutoEventWireup="true"
    CodeBehind="DocumentTypes.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.DocumentTypes" %>
<%@ Register Src="~/fDocuments/Controls/CtrlDocumentTypes.ascx"
    TagPrefix="SweetSoft"
    TagName="CtrlDocumentTypes" %>

<asp:Content ID="ContentMain"
    ContentPlaceHolderID="cpMain"
    runat="server">

    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen">
                <SweetSoft:Navigation
                    runat="server"
                    ID="Navigation1" />

                <SweetSoft:CtrlDocumentTypes
                    runat="server"
                    ID="CtrlDocumentTypes1" />
            </div>
        </div>
    </div>

</asp:Content>
