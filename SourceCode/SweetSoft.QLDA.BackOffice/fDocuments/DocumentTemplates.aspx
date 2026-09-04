<%@ Page Title="" Language="C#"
    MasterPageFile="~/MasterPages/MasterTemplate.Master"
    AutoEventWireup="true"
    CodeBehind="DocumentTemplates.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.DocumentTemplates" %>
<%@ Register Src="~/fDocuments/Controls/CtrlDocumentTemplates.ascx"
    TagPrefix="SweetSoft"
    TagName="CtrlDocumentTemplates" %>

<asp:Content ID="ContentMain"
    ContentPlaceHolderID="cpMain"
    runat="server">

    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen">
                <SweetSoft:Navigation
                    runat="server"
                    ID="Navigation1" />

                <SweetSoft:CtrlDocumentTemplates
                    runat="server"
                    ID="CtrlDocumentTemplates1" />
            </div>
        </div>
    </div>

</asp:Content>
