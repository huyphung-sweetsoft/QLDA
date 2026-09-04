<%@ Page Title="" Language="C#"
    MasterPageFile="~/MasterPages/MasterTemplate.Master"
    AutoEventWireup="true"
    CodeBehind="DocumentDetail.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.DocumentDetail" %>
<%@ Register Src="~/fDocuments/Controls/CtrlDocumentDetail.ascx"
    TagPrefix="SweetSoft"
    TagName="CtrlDocumentDetail" %>

<asp:Content ID="ContentMain"
    ContentPlaceHolderID="cpMain"
    runat="server">

    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen">
                <SweetSoft:Navigation
                    runat="server"
                    ID="Navigation1" />

                <SweetSoft:CtrlDocumentDetail
                    runat="server"
                    ID="CtrlDocumentDetail1" />
            </div>
        </div>
    </div>

</asp:Content>
