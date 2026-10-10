<%@ Page Title="Dashboard nhân viên" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="EmployeeDashboard.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fEmployeeDashboard.EmployeeDashboard" %>
<%@ Register Src="~/fEmployeeDashboard/Controls/CtrlEmployeeDashboard.ascx" TagPrefix="uc" TagName="CtrlEmployeeDashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpMain" runat="server">
    <uc:CtrlEmployeeDashboard ID="CtrlEmployeeDashboard1" runat="server" />
</asp:Content>
