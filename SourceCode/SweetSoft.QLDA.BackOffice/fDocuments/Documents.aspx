<%@ Page Title="" Language="C#"
    MasterPageFile="~/MasterPages/MasterTemplate.Master"
    AutoEventWireup="true"
    CodeBehind="Documents.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.Documents" %>
<%@ Register Src="~/fDocuments/Controls/CtrlDocuments.ascx"
    TagPrefix="SweetSoft"
    TagName="CtrlDocuments" %>

<asp:Content ID="ContentMain"
    ContentPlaceHolderID="cpMain"
    runat="server">

    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen">
                <SweetSoft:Navigation
                    runat="server"
                    ID="Navigation1" />

                <SweetSoft:CtrlDocuments
                    runat="server"
                    ID="CtrlDocuments1" />
            </div>
        </div>
    </div>

</asp:Content>
