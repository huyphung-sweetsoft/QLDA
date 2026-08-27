<%@ Page Title="" Language="C#"
    MasterPageFile="~/MasterPages/MasterTemplate.Master"
    AutoEventWireup="true"
    CodeBehind="StorageLocations.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.StorageLocations" %>
<%@ Register Src="~/fDocuments/Controls/CtrlStorageLocations.ascx"
    TagPrefix="SweetSoft"
    TagName="CtrlStorageLocations" %>

<asp:Content ID="ContentMain"
    ContentPlaceHolderID="cpMain"
    runat="server">

    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen">
                <SweetSoft:Navigation
                    runat="server"
                    ID="Navigation1" />

                <SweetSoft:CtrlStorageLocations
                    runat="server"
                    ID="CtrlStorageLocations1" />
            </div>
        </div>
    </div>

</asp:Content>
