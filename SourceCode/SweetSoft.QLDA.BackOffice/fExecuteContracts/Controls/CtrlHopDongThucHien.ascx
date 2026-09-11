<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlHopDongThucHien.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fExecuteContracts.Controls.CtrlHopDongThucHien" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="card-header">
    <asp:UpdatePanel runat="server" ID="upnlSearchDefault" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel runat="server" ID="pnlSearchDefault">
                <div class="row g-2 align-items-end">

                    <%-- Khách hàng --%>
                    <div class="col-md-6 col-xl-3">
                        <label class="form-label">Khách hàng</label>
                        <SweetSoft:ExtraDropdown runat="server" ID="ddlSearchKhachHang" SimpleInit="true" ValueIsOfTypeGUID="true" PlaceHolder="Chọn khách hàng"></SweetSoft:ExtraDropdown>
                    </div>

                    <%-- Giá trị từ --%>
                    <div class="col-md-6 col-xl-2">
                        <label class="form-label">Giá trị từ</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtGiaTriHopDongTu" TextMode="Number" PlaceHolder="0"></SweetSoft:ExtraTextBox>
                    </div>

                    <%-- Giá trị đến --%>
                    <div class="col-md-6 col-xl-2">
                        <label class="form-label">Giá trị đến</label>
                        <SweetSoft:ExtraTextBox runat="server" ID="txtGiaTriHopDongDen" TextMode="Number" PlaceHolder="0"></SweetSoft:ExtraTextBox>
                    </div>

                    <%-- Ngày ký từ --%>
                    <div class="col-md-6 col-xl-2">
                        <label class="form-label">Ngày ký từ</label>
                        <SweetSoft:ExtraDateTime runat="server" ID="txtNgayKyTu" SingleDatePicker="true" AutoUpdateInput="false" AutoApply="true"></SweetSoft:ExtraDateTime>
                    </div>

                    <%-- Ngày ký đến --%>
                    <div class="col-md-6 col-xl-2">
                        <label class="form-label">Ngày ký đến</label>
                        <SweetSoft:ExtraDateTime runat="server" ID="txtNgayKyDen" SingleDatePicker="true" AutoUpdateInput="false" AutoApply="true"></SweetSoft:ExtraDateTime>
                    </div>

                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <div class="d-flex flex-column flex-xl-row gap-2 mt-3">
        <div class="input-group max-w-500">
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" PlaceHolder="Nhập khách hàng, số hoặc tên hợp đồng..." CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>


        <div class="ms-xl-auto">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <SweetSoft:ExtraButton runat="server" ID="lbtAdd" ButtonStyle="Info" ButtonIcon="Add" Visible="false" OnClick="lbtAdd_Click">Thêm mới</SweetSoft:ExtraButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <div class="listSearchTagBox">
        <asp:UpdatePanel runat="server" ID="upSearchTagBox" UpdateMode="Conditional">
            <ContentTemplate>
                <SweetSoft:ExtraSearchBox runat="server" ID="searchTagBox" OnTagClosed="searchTagBox_TagClosed"></SweetSoft:ExtraSearchBox>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</div>

<div class="card-body p-0">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <SweetSoft:GridviewExtension ID="grvData" runat="server" AllowSorting="true" ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false" CssClass="table-bordered table-hover" FocusBtnIcon="fas fa-compress-arrows-alt" DataKeyNames="IdHopDongThucHien" GridLines="None" IsEnableSelectColumn="false" OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <%-- Số hợp đồng --%>
                    <asp:TemplateField HeaderText="Số hợp đồng" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-left" SortExpression="SoHopDong">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" ID="lbtView" CssClass="card-link fw-bold text-primary" Visible='<%# this.IsEdit %>' CommandName="ITEM_DETAIL" Text='<%# Eval("SoHopDong") %>'></asp:LinkButton>
                            <span runat="server" visible='<%# !this.IsEdit %>' class="fw-bold text-primary"><%# Eval("SoHopDong") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Tên hợp đồng --%>
                    <asp:TemplateField HeaderText="Tên hợp đồng" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-left" SortExpression="TenHopDong">
                        <ItemTemplate>
                            <%# Eval("TenHopDong") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Khách hàng --%>
                    <asp:TemplateField HeaderText="Khách hàng" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-left" SortExpression="TenKhachHang">
                        <ItemTemplate>
                            <%# Eval("TenKhachHang") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Giá trị hợp đồng --%>
                    <asp:TemplateField HeaderText="Giá trị hợp đồng" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-end" SortExpression="GiaTriHopDong">
                        <ItemTemplate>
                            <%# Eval("GiaTriHopDong") == DBNull.Value ? string.Empty : string.Format("{0:N0}", Eval("GiaTriHopDong")) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Ngày ký --%>
                    <asp:TemplateField HeaderText="Ngày ký" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" SortExpression="NgayKy">
                        <ItemTemplate>
                            <%# this.ConvertDateTimeToString(Eval("NgayKy"), false) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Ngày hiệu lực --%>
                    <asp:TemplateField HeaderText="Ngày hiệu lực" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" SortExpression="NgayHieuLuc">
                        <ItemTemplate>
                            <%# this.ConvertDateTimeToString(Eval("NgayHieuLuc"), false) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Ngày hết hạn --%>
                    <asp:TemplateField HeaderText="Ngày hết hạn" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" SortExpression="NgayHetHan">
                        <ItemTemplate>
                            <%# this.ConvertDateTimeToString(Eval("NgayHetHan"), false) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <%-- Hành động --%>
                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="150px">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center gap-2">
                                <SweetSoft:SmartLinkButton runat="server" ID="lbtDetail" VisibleConditionKey='<%# this.IsEdit %>' CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline" ResourceKey='<%# BackEndResourceKeys.EDIT %>' ButtonIcon="fas fa-pencil-alt"></SweetSoft:SmartLinkButton>
                                <SweetSoft:SmartLinkButton runat="server" ID="lbtDelete" VisibleConditionKey='<%# this.IsDelete %>' CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger" ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash"></SweetSoft:SmartLinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>

                <EmptyDataTemplate>
                    <%= GetResourceText(BackEndResourceKeys.NO_DATA) %>
                </EmptyDataTemplate>
            </SweetSoft:GridviewExtension>

            <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging" OnPageChanged="ctrlGridviewPaging_PageChanged" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>