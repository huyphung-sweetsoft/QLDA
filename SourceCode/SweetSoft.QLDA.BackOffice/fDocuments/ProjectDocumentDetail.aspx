<%@ Page Title="" Language="C#"
    MasterPageFile="~/MasterPages/MasterTemplate.Master"
    AutoEventWireup="true"
    CodeBehind="ProjectDocumentDetail.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.ProjectDocumentDetail" %>
<%@ Register Src="~/fDocuments/Controls/CtrlDocumentDetail.ascx"
    TagPrefix="SweetSoft"
    TagName="CtrlDocumentDetail" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx"
    TagPrefix="SweetSoft"
    TagName="CtrlProjectTabs" %>

<asp:Content ID="ContentMain"
    ContentPlaceHolderID="cpMain"
    runat="server">

    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen">
                <SweetSoft:Navigation
                    runat="server"
                    ID="Navigation1" />

                <SweetSoft:CtrlProjectTabs
                    runat="server"
                    ID="CtrlProjectTabs1" />

                <SweetSoft:CtrlDocumentDetail
                    runat="server"
                    ID="CtrlProjectDocumentDetail1" />
            </div>
        </div>
    </div>

</asp:Content>
