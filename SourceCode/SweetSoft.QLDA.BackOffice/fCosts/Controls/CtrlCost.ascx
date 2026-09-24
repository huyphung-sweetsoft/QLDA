<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlCost.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fCosts.Controls.CtrlCost" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Assembly="SweetSoft.QLDA.Controls" Namespace="SweetSoft.QLDA.Controls" TagPrefix="SweetSoft" %>

<div class="card-header">
    <div class="d-flex flex-column flex-xl-row gap-3">
        <asp:UpdatePanel runat="server" ID="pnlSearchDropdowns" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlSearchDefaultStatus">
                    <div class="d-flex">
                        <SweetSoft:BootstrapDropdown ID="ddlSearchTrangThaiChiPhi" runat="server"
                            Text="Trạng thái" AllowClear="true" AutoPostBack="true" SearchColumn="TrangThai"
                            CssClass="border-top-left-radius-1 border-bottom-left-radius-1"
                            OnSelectedValueChanged="bootstrapDropdown_SelectedValueChanged">
                        </SweetSoft:BootstrapDropdown>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        
        <div class="input-group max-w-500 mb-3">
            <a class="btn btn-info font-mobile-small btn-search-filter" onclick="CMSMasterJs.ShowOffcanvasSearch();" href="javascript:;">
                <i class='fas fa-filter me-1'></i><%= GetResourceText(BackEndResourceKeys.FILTER) %>
            </a>
            <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSingle" CssClass="border-primary input-search-filter"></SweetSoft:ExtraTextBox>
            <SweetSoft:ExtraButton runat="server" ID="lbtSearchSingle" CssClass="btn-outline-primary btn-search-filter" IsCustomClass="false" ButtonIcon="Search" OnClick="btnSearch_ServerClick"></SweetSoft:ExtraButton>
        </div>
        
        <div runat="server" id="tagOther" class="d-flex justify-content-end gap-3 w-full flex-wrap">
            <asp:UpdatePanel runat="server" ID="pnlButtons" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="d-flex">
                        <SweetSoft:ExtraButton runat="server" ID="lbtAdd" OnClick="lbtAdd_Click" CssClass="waves-effect waves-light font-mobile-small" ButtonStyle="Info" ButtonIcon="Add">Thêm mới</SweetSoft:ExtraButton>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <div class="listSearchTagBox">
        <asp:UpdatePanel ID="upSearchTagBox" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <SweetSoft:ExtraSearchBox ID="searchTagBox" runat="server" OnTagClosed="searchTagBox_TagClosed"></SweetSoft:ExtraSearchBox>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</div>

<div class="card-body p-0">
    <asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <SweetSoft:GridviewExtension ID="grvData" runat="server"
                AllowSorting="true" ShowHeader="true" ShowHeaderWhenEmpty="true" AutoGenerateColumns="false"
                CssClass="table-bordered table-hover align-middle" FocusBtnIcon="fas fa-compress-arrows-alt"
                DataKeyNames="IdChiPhi" ValueField="IdChiPhi" DataNameField="TenKhoanChi" GridLines="None"
                IsEnableSelectColumn="false" 
                OnNeedDataSource="grvData_NeedDataSource" OnRowCommand="grvData_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="CostCode" HeaderStyle-Width="120px" HeaderStyle-CssClass="text-center" SortExpression="MaChiPhi" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("MaChiPhi") != DBNull.Value ? Eval("MaChiPhi") : "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="CostName" SortExpression="TenKhoanChi">
                        <ItemTemplate><%# Eval("TenKhoanChi") != DBNull.Value ? Eval("TenKhoanChi") : "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Price" SortExpression="DonGia" HeaderStyle-Width="120px" HeaderStyle-CssClass="text-end" ItemStyle-CssClass="text-end">
                        <ItemTemplate><%# Eval("DonGia") != DBNull.Value ? Convert.ToDecimal(Eval("DonGia")).ToString("N0") : "0" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Quantity" SortExpression="SoLuong" HeaderStyle-Width="70px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("SoLuong") != DBNull.Value ? Eval("SoLuong") : "0" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="TotalAmount" SortExpression="SoTien" HeaderStyle-Width="130px" HeaderStyle-CssClass="text-end" ItemStyle-CssClass="text-end fw-bold">
                        <ItemTemplate><%# Eval("SoTien") != DBNull.Value ? Convert.ToDecimal(Eval("SoTien")).ToString("N0") : "0" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Requester" SortExpression="NhanVienYeuCau" HeaderStyle-Width="150px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("NhanVienYeuCau") != DBNull.Value ? Eval("NhanVienYeuCau") : "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="DateCreated" SortExpression="NgayTao" HeaderStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("NgayTao") != DBNull.Value ? Convert.ToDateTime(Eval("NgayTao")).ToString("dd/MM/yyyy") : "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status" SortExpression="TrangThai" HeaderStyle-Width="120px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                        <ItemTemplate><%# Eval("TrangThai") != DBNull.Value ? GetTrangThaiChiPhiText(Eval("TrangThai")) : "—" %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderStyle-Width="180px">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center align-items-center gap-1">
                                
                                <asp:LinkButton runat="server"
                                    ID="lbtApprove"
                                    CommandName="OPEN_APPROVE_MODAL"
                                    CommandArgument='<%# Eval("IdChiPhi") %>'
                                    Visible='<%# this.IsPM && Eval("TrangThai") != DBNull.Value && Eval("TrangThai").ToString() == "0" %>'
                                    CssClass="btn btn-outline-success btn-sm text-center btn-smart-link"
                                    ToolTip="Xử lý (Duyệt/Từ chối)">
                                    <i class="fas fa-check-circle"></i>
                                </asp:LinkButton>

                                <asp:LinkButton runat="server"
                                    ID="lbtCostFiles"
                                    Visible='<%# this.IsView %>'
                                    CommandName="COST_FILES"
                                    CommandArgument='<%# Eval("IdChiPhi") %>'
                                    CausesValidation="false"
                                    CssClass="btn btn-outline-primary btn-sm text-center btn-smart-link"
                                    ToolTip="File đính kèm chi phí">
                                    <i class="fas fa-folder-open"></i>
                                </asp:LinkButton>

                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsView %>' 
                                    ID="lbtDetail" CommandName="ITEM_DETAIL" CssClass="btn-grid-action text-decoration-underline" 
                                    ResourceKey='<%# this.IsEdit ? BackEndResourceKeys.EDIT : BackEndResourceKeys.VIEW %>' 
                                    ButtonIcon='<%# this.IsView ? "fas fa-pencil-alt" : "fas fa-eye" %>'></SweetSoft:SmartLinkButton>

                                <SweetSoft:SmartLinkButton runat="server" VisibleConditionKey='<%# this.IsDelete %>' 
                                    ID="lbtDelete" CommandName="ITEM_DELETE" CssClass="btn-grid-action text-decoration-underline text-danger" 
                                    ResourceKey='<%# BackEndResourceKeys.DELETE %>' ButtonIcon="fas fa-trash"></SweetSoft:SmartLinkButton>
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

<!-- POPUP DUYỆT / TỪ CHỐI NHANH -->
<SweetSoft:ExtraModal runat="server" ID="mdlFastApprove" Type="Primary" Title="Xử lý khoản chi">
    <ContentTemplate>
        <asp:HiddenField ID="hdfApproveCostId" runat="server" />
        
        <div id="divApproveMode" class="p-2">
            <div class="text-center mb-2 mt-2" id="grpActionButtons">
                <h5 class="text-primary mb-2">Xác nhận xử lý khoản chi</h5>
                <p class="text-muted">Vui lòng chọn hành động "Duyệt" hoặc "Từ chối" cho khoản chi này.</p>
                
                <div class="d-flex justify-content-center gap-3 mt-4">
                    <asp:LinkButton ID="btnQuickApprove" runat="server" CssClass="btn btn-success px-4" OnClick="btnQuickApprove_Click">
                        <i class="fas fa-check me-2"></i> Duyệt
                    </asp:LinkButton>
                    
                    <button type="button" class="btn btn-danger px-4" onclick="$('#grpActionButtons').hide(); $('#divRejectReason').fadeIn();">
                        <i class="fas fa-times me-2"></i> Từ chối
                    </button>
                </div>
            </div>

            <div id="divRejectReason" style="display: none;" class="mt-3 border-top pt-3">
                <div class="mb-3">
                    <label class="form-label text-danger fw-bold">Lý do từ chối (Bắt buộc) <span class="text-danger">*</span></label>
                    <asp:TextBox runat="server" ID="txtRejectReason" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Nhập lý do chi tiết để nhân viên điều chỉnh..."></asp:TextBox>
                </div>
                <div class="d-flex justify-content-end gap-2">
                    <button type="button" class="btn btn-outline-secondary" onclick="$('#divRejectReason').hide(); $('#grpActionButtons').fadeIn(); $('#<%= txtRejectReason.ClientID %>').val('');">Quay lại</button>
                    
                    <asp:LinkButton ID="btnConfirmReject" runat="server" CssClass="btn btn-danger" OnClick="btnConfirmReject_Click">
                        <i class="fas fa-paper-plane me-1"></i> Xác nhận Từ chối
                    </asp:LinkButton>
                </div>
            </div>
        </div>
    </ContentTemplate>
</SweetSoft:ExtraModal>

<div class="offcanvas offcanvas-end offcanvas-form-search" id="search-offcanvas" aria-hidden="true">
    <div class="offcanvas-header">
        <div class="flex flex-column flex-md-row align-items-center gap-3">
            <h5 class="offcanvas-title"><%= GetResourceText(BackEndResourceKeys.ADVANCED_SEARCH) %></h5>
        </div>
        <button class="btn-close" type="button" data-bs-dismiss="offcanvas" aria-label="Close"></button>
    </div>
    <div class="div offcanvas-body pt-0">
        <div class="card shadow-none card-body text-muted mb-0">
            <asp:UpdatePanel runat="server" ID="pnlSearch" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel runat="server" ID="pnlSearchPopup">
                        <div class="row g-3 mb-3">
                            <div class="col-md-6">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.COST_NAME) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchTenKhoanChi" SearchColumn="TenKhoanChi"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.REQUESTER) %></label>
                                <asp:DropDownList ID="ddlSearchNhanVienYeuCau" runat="server" CssClass="form-select" SearchColumn="IdNhanVienDeNghi"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="row g-3 mb-3">
                            <div class="col-md-6">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.LOWEST_TOTAL_AMOUNT) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSoTienMin" SearchColumn="SoTienMin" CssClass="format-currency"></SweetSoft:ExtraTextBox>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.HIGHEST_TOTAL_AMOUNT) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSearchSoTienMax" SearchColumn="SoTienMax" CssClass="format-currency"></SweetSoft:ExtraTextBox>
                            </div>
                        </div>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            <div class="d-flex align-items-center gap-2 mt-3">
                <SweetSoft:ExtraButton runat="server" ID="lbtSearchAdvanced" CssClass="flex-btn" ButtonStyle="Primary" ButtonIcon="Search" OnClick="btnSearchAdvanced_ServerClick"><%= GetResourceText(BackEndResourceKeys.APPLY) %></SweetSoft:ExtraButton>
                <SweetSoft:ExtraButton runat="server" ID="lbtCancel" CssClass="flex-btn" ButtonStyle="OutLineSecondary" ButtonIcon="Refresh" OnClick="btnCancel_Click"><%= GetResourceText(BackEndResourceKeys.REFRESH) %></SweetSoft:ExtraButton>
            </div>
        </div>
    </div>
</div>
