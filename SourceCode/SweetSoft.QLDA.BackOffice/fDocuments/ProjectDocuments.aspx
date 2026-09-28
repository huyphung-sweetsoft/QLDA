<%@ Page Title="" Language="C#"
    MasterPageFile="~/MasterPages/MasterTemplate.Master"
    AutoEventWireup="true"
    CodeBehind="ProjectDocuments.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.ProjectDocuments" %>
<%@ Register Src="~/fDocuments/Controls/CtrlDocuments.ascx"
    TagPrefix="SweetSoft"
    TagName="CtrlDocuments" %>
<%@ Register Src="~/fProjects/Controls/CtrlProjectTabs.ascx"
    TagPrefix="SweetSoft"
    TagName="CtrlProjectTabs" %>

<asp:Content ID="ContentMain"
    ContentPlaceHolderID="cpMain"
    runat="server">

    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation
                    runat="server"
                    ID="Navigation1" />

                <SweetSoft:CtrlProjectTabs
                    runat="server"
                    ID="CtrlProjectTabs1" />

                <SweetSoft:CtrlDocuments
                    runat="server"
                    ID="CtrlProjectDocuments1" />
            </div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="ContentBottomScript"
    ContentPlaceHolderID="cpBottomScript"
    runat="server">
    <script>
        window.addEventListener("pageshow", function (event) {
            if (event.persisted) {
                window.location.reload();
            }
        });
    </script>
</asp:Content>
