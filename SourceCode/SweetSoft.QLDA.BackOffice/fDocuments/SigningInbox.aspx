<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master"
    AutoEventWireup="true" CodeBehind="SigningInbox.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.SigningInbox" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>

<asp:Content ID="ContentStyles" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .signing-inbox .signing-document-grid th { color: var(--bs-primary); text-align: center; }
        .signing-inbox .signing-document-grid td { vertical-align: middle; }
        .signing-inbox .signing-document-grid tr > :first-child { width: 50px; min-width: 50px; white-space: nowrap; text-align: center; }
        .signing-inbox .signing-document-grid .signing-document-text { white-space: normal; overflow-wrap: anywhere; }
        .signing-inbox .signing-document-grid .signing-document-name { color: inherit; font-weight: normal; }
        .signing-inbox .signing-document-grid .signing-document-name:hover { color: var(--bs-primary); text-decoration: underline; }
        @media (max-width: 991.98px) { .signing-inbox .signing-document-grid { min-width: 960px; } }
        .signing-inbox .signing-batch > summary { cursor: pointer; list-style: none; }
        .signing-inbox .signing-batch > summary::-webkit-details-marker { display: none; }
        .signing-inbox .signing-batch:not([open]) .signing-chevron { transform: rotate(-90deg); }
        .signing-inbox .signing-file-table { border-color: var(--bs-border-color); }
        .signing-inbox .signing-file-table > :not(caption) > * > * { border-color: var(--bs-border-color); }
        .signing-inbox .signing-file-table th { color: var(--bs-primary); text-align: center; white-space: normal; }
        .signing-inbox .signing-file-table td { vertical-align: middle; }
        .signing-result-upload { border: 1px solid var(--bs-border-color); border-radius: .375rem; padding: 1rem; }
        .signing-result-upload .file-box.file-box-single .control,
        .signing-result-upload .file-actions,
        .signing-result-upload .item[data-ar="00000000-0000-0000-0000-000000000000"] { display: none !important; }
        .signing-result-upload .file-box.file-box-single .uploaded-content { margin: 0; }
        .signing-result-upload .file-box.file-box-single .uploaded-content .item { max-width: none; margin: .75rem 0 0; }
        .signing-result-upload .file-box .item .bg-body { background: var(--bs-light) !important; border: 1px solid var(--bs-border-color); padding: .5rem; }
        .signing-result-upload .file-box.file-box-single .uploaded-content .img-container { width: 80px; height: 80px; margin: 0 auto; }
        .signing-result-upload .file-box.file-box-single .uploaded-content .img-container > img { max-height: 60px; max-width: 80px; object-fit: contain; }
        .signing-result-upload .file-box.file-box-single .uploaded-content .img-control.right { display: block; opacity: 1; }
        .signing-result-upload .file-box.file-box-single .uploaded-content .item .title { display: block; width: 100%; text-align: center; border: 0; background: transparent; text-overflow: ellipsis; }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="cpMain" runat="server">
    <asp:UpdatePanel runat="server" ID="upAssignedFiles" UpdateMode="Conditional">
        <ContentTemplate>
    <asp:HiddenField runat="server" ID="hdfAppliedKeyword" />
    <asp:HiddenField runat="server" ID="hdfAppliedStatus" />
    <asp:HiddenField runat="server" ID="hdfAppliedProject" />
    <asp:HiddenField runat="server" ID="hdfPageIndex" />
    <asp:HiddenField runat="server" ID="hdfPageSize" />
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen signing-inbox">
                <SweetSoft:Navigation runat="server" ID="Navigation1" MainTitle="Hồ sơ trình ký" />

                <asp:Panel runat="server" ID="pnlDetailHeader" CssClass="d-flex flex-wrap align-items-center justify-content-between gap-2 border-bottom pb-2 mb-2">
                    <div class="min-w-0">
                        <h5 class="text-primary mb-1 text-break"><asp:Label runat="server" ID="lblDetailTitle" /></h5>
                        <asp:Label runat="server" ID="lblDetailScope" CssClass="small text-muted" />
                    </div>
                    <div class="d-flex flex-wrap gap-2">
                        <asp:HyperLink runat="server" ID="lnkOriginalDocument" Text="Mở hồ sơ gốc" CssClass="btn btn-sm btn-outline-secondary" />
                        <asp:HyperLink runat="server" ID="lnkBackToInbox" Text="Quay lại danh sách" CssClass="btn btn-sm btn-outline-primary" />
                    </div>
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlFilters" CssClass="card-header mb-2">
                    <div class="d-flex flex-column flex-xl-row align-items-xl-start gap-3">
                        <div class="d-flex flex-wrap flex-shrink-0 align-items-start">
                            <div class="flex-shrink-0">
                            <SweetSoft:BootstrapDropdown runat="server" ID="ddlProject" Text="Dự án" AutoPostBack="true" EnableSearch="true" SearchPlaceholder="Tìm dự án..."
                                CssClass="max-w-500 text-wrap border-top-left-radius-1 border-bottom-left-radius-1"
                                OnSelectedValueChanged="ddlProject_SelectedValueChanged" />
                            </div>
                            <asp:Panel runat="server" ID="pnlStatusFilter" CssClass="flex-shrink-0">
                                <SweetSoft:BootstrapDropdown runat="server" ID="ddlSigningStatus"
                                    Text="Hồ sơ cần xử lý" AutoPostBack="true"
                                    CssClass="text-nowrap border-top-right-radius-1 border-bottom-right-radius-1"
                                    OnSelectedValueChanged="ddlSigningStatus_SelectedValueChanged" />
                            </asp:Panel>
                        </div>
                            <div class="input-group max-w-500 flex-nowrap align-self-start">
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSigningSearch"
                                    CssClass="border-primary input-search-filter"
                                    PlaceHolder="Nhập từ khóa tìm kiếm..." ToolTip="Tìm mã hồ sơ, tên hồ sơ hoặc tên file" />
                                <SweetSoft:ExtraButton runat="server" ID="btnApplyFilters"
                                    CssClass="btn-outline-primary btn-search-filter"
                                    IsCustomClass="false" ButtonIcon="Search"
                                    OnClick="btnApplyFilters_Click" />
                                <SweetSoft:ExtraButton runat="server" ID="btnResetFilters"
                                    CssClass="btn-outline-secondary btn-search-filter"
                                    IsCustomClass="false" ButtonIcon="Refresh"
                                    ToolTip="Xóa bộ lọc"
                                    OnClick="btnResetFilters_Click" />
                            </div>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="pnlEmpty" CssClass="card-body text-center text-muted py-5">
                    <i class="fas fa-inbox fs-2 d-block mb-2"></i>
                    Bạn chưa có hồ sơ nào được giao trình ký.
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlNoMatches" CssClass="card-body text-center text-muted py-5">
                    <i class="fas fa-search fs-2 d-block mb-2"></i>
                    Không tìm thấy hồ sơ hoặc file phù hợp với điều kiện lọc.
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlDocumentList" CssClass="card-body p-0">
                    <SweetSoft:GridviewExtension runat="server" ID="grvDocumentList"
                        AutoGenerateColumns="false" AllowSorting="false" AllowPaging="false"
                        IsEnableSelectColumn="false" IsFixedLastColumn="true" GridLines="None"
                        CssClass="table-bordered table-hover align-middle signing-document-grid" style="table-layout:fixed">
                        <Columns>
                            <asp:TemplateField HeaderText="Mã hồ sơ" HeaderStyle-Width="180px" ItemStyle-CssClass="signing-document-text">
                                <ItemTemplate><a href='<%# DetailUrl(Eval("Id")) %>' class="fw-bold text-primary"><%#: Eval("MaTaiLieu") %></a></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Tên hồ sơ" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="signing-document-text">
                                <ItemTemplate><a href='<%# DetailUrl(Eval("Id")) %>' class="signing-document-name"><%#: Eval("TenTaiLieu") %></a></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Dự án" HeaderStyle-Width="23%" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="signing-document-text">
                                <ItemTemplate><%#: Eval("ProjectName") %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Cần xử lý" HeaderStyle-Width="180px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center signing-document-text">
                                <ItemTemplate><%#: Convert.ToInt32(Eval("PendingCount")) > 0 ? Eval("PendingBatches") + " đợt · " + Eval("PendingCount") + " file chờ ký" : "Không có file chờ ký" %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Lần gửi gần nhất" HeaderStyle-Width="200px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center signing-document-text">
                                <ItemTemplate><%#: FormatDate(Eval("LastSent")) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Hành động" HeaderStyle-Width="110px" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center">
                                <ItemTemplate><a class="btn btn-sm btn-outline-primary" href='<%# DetailUrl(Eval("Id")) %>' title='<%# Convert.ToInt32(Eval("PendingCount")) > 0 ? "Xử lý trình ký" : "Xem các đợt trình ký" %>' aria-label='<%# Convert.ToInt32(Eval("PendingCount")) > 0 ? "Xử lý trình ký" : "Xem các đợt trình ký" %>'><i class="fas fa-folder-open" aria-hidden="true"></i></a></ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </SweetSoft:GridviewExtension>
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlDetail" CssClass="flex-grow-1">
                <asp:Repeater runat="server" ID="rptAssignedDocuments" OnItemDataBound="rptAssignedDocuments_ItemDataBound">
                    <ItemTemplate>
                        <details class="signing-batch border rounded mb-2" <%# Convert.ToBoolean(Eval("Expanded")) ? "open" : "" %>>
                            <summary class="p-2 bg-light d-flex flex-wrap justify-content-between align-items-center gap-2">
                                <div class="d-flex align-items-start gap-2">
                                    <i class="fas fa-chevron-down signing-chevron text-primary mt-1" aria-hidden="true"></i>
                                    <div>
                                        <div class="fw-semibold text-primary text-break">Gửi ngày <%#: FormatDate(Eval("LastSent")) %></div>
                                        <div class="small text-muted">Người gửi: <%#: Eval("Sender") %></div>
                                    </div>
                                </div>
                                <div class="d-flex flex-wrap gap-1 justify-content-end">
                                    <span class="badge bg-light text-secondary border"><%#: Convert.ToInt32(Eval("FileCount")) == Convert.ToInt32(Eval("TotalFileCount")) ? Eval("FileCount") + " file" : Eval("FileCount") + "/" + Eval("TotalFileCount") + " file phù hợp" %></span>
                                    <asp:Panel runat="server" Visible='<%# Convert.ToInt32(Eval("PendingCount")) > 0 %>'>
                                        <span class="badge bg-info"><%#: Eval("PendingCount") %> chờ ký</span>
                                    </asp:Panel>
                                    <asp:Panel runat="server" Visible='<%# Convert.ToInt32(Eval("SignedCount")) > 0 %>'>
                                        <span class="badge bg-success"><%#: Eval("SignedCount") %> đã ký</span>
                                    </asp:Panel>
                                    <asp:Panel runat="server" Visible='<%# Convert.ToInt32(Eval("ChangesCount")) > 0 %>'>
                                        <span class="badge bg-warning text-dark"><%#: Eval("ChangesCount") %> cần chỉnh sửa</span>
                                    </asp:Panel>
                                    <asp:Panel runat="server" Visible='<%# Convert.ToInt32(Eval("RecalledCount")) > 0 %>'>
                                        <span class="badge bg-secondary"><%#: Eval("RecalledCount") %> đã thu hồi</span>
                                    </asp:Panel>
                                </div>
                            </summary>
                            <div class="border-top">
                            <asp:Panel runat="server" CssClass="small text-muted p-2 border-bottom" Visible='<%# HasText(Eval("Note")) %>'><%#: Eval("Note") %></asp:Panel>
                            <div class="table-rep-plugin"><div class="table-responsive"><table class="signing-file-table extra-gridview table w-100 table-bordered table-hover align-middle mb-0" style="table-layout:fixed"><colgroup><col style="width:38%" /><col style="width:24%" /><col style="width:38%" /></colgroup><thead><tr><th>File được giao ký</th><th>Trạng thái</th><th>Hành động</th></tr></thead><tbody>
                            <asp:Repeater runat="server" ID="rptAssignedFiles" OnItemCommand="rptAssignedFiles_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td class="text-break">
                                            <div class="min-w-0">
                                                <div class="d-flex flex-wrap align-items-center gap-2">
                                                    <i class="fas fa-file-alt text-muted"></i>
                                                    <asp:HyperLink runat="server" CssClass="fw-semibold text-break" Target="_blank"
                                                        ToolTip="Xem trước file được giao ký" data-path='<%# FileUrl(Eval("FileNguonUrl")) %>'
                                                        onclick="FilesBox.LayoutFilePopUp(this); return false;"
                                                        NavigateUrl='<%# FileUrl(Eval("FileNguonUrl")) %>'
                                                        Text='<%# System.Web.HttpUtility.HtmlEncode(FileName(Eval("TenFileNguonGoc"), Eval("TenFileNguon"))) %>' />
                                                </div>
                                            </div>
                                        </td>
                                        <td><span class='<%# StatusCss(Eval("TrangThai")) %>'><%#: StatusText(Eval("TrangThai")) %></span><asp:Panel runat="server" CssClass="small text-break mt-1" Visible='<%# HasText(Eval("GhiChu")) %>'><%#: Eval("GhiChu") %></asp:Panel>
                                            <asp:PlaceHolder runat="server" Visible='<%# Eval("NgayNhanLai") != DBNull.Value %>'><div class="small text-muted mt-1">Xử lý lúc <%#: FormatDate(Eval("NgayNhanLai")) %></div></asp:PlaceHolder>
                                        </td>
                                        <td>
                                        <div class="d-flex flex-wrap justify-content-center gap-1 py-1">
                                            <asp:HyperLink runat="server" CssClass="btn btn-sm btn-outline-primary"
                                                Text="<i class='fas fa-eye me-1'></i>Xem trước"
                                                NavigateUrl='<%# FileUrl(Eval("FileNguonUrl")) %>'
                                                data-path='<%# FileUrl(Eval("FileNguonUrl")) %>'
                                                onclick="FilesBox.LayoutFilePopUp(this); return false;" />
                                            <asp:HyperLink runat="server" CssClass="btn btn-sm btn-outline-primary"
                                                Target="_blank" Text="<i class='fas fa-download me-1'></i>Tải file ký"
                                                NavigateUrl='<%# FileUrl(Eval("FileNguonUrl")) %>' />
                                            <asp:HyperLink runat="server" CssClass="btn btn-sm btn-outline-success"
                                                Target="_blank" Text="<i class='fas fa-file-download me-1'></i>Bản đã ký"
                                                Visible='<%# HasText(Eval("FileSauKyUrl")) %>'
                                                NavigateUrl='<%# FileUrl(Eval("FileSauKyUrl")) %>' />
                                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-primary"
                                                Visible='<%# IsEdit && IsPending(Eval("TrangThai")) %>'
                                                CommandName="UPLOAD_RESULT" CommandArgument='<%# Eval("IdTrinhKyTaiLieuFile") %>'
                                                CausesValidation="false" Text="<i class='fas fa-upload me-1'></i>Tải bản đã ký lên" />
                                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-warning"
                                                Visible='<%# IsEdit && IsPending(Eval("TrangThai")) %>'
                                                CommandName="REQUEST_CHANGES" CommandArgument='<%# Eval("IdTrinhKyTaiLieuFile") %>'
                                                CausesValidation="false" Text="<i class='fas fa-comment-dots me-1'></i>Yêu cầu chỉnh sửa" />
                                        </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                            </tbody></table></div></div>
                            </div>
                        </details>
                    </ItemTemplate>
                </asp:Repeater>
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlPagination" CssClass="mt-auto pt-2 flex-shrink-0">
                    <SweetSoft:Paging runat="server" ID="ctrlGridviewPaging"
                        OnPageChanged="ctrlGridviewPaging_PageChanged" />
                </asp:Panel>
            </div>
        </div>
    </div>

        </ContentTemplate>
    </asp:UpdatePanel>

    <SweetSoft:ExtraModal runat="server" ID="mdlSigningResult" Type="Primary"
        Size="Normal" FooterButtonClose="false" EnsureChildControlsOnPostback="true">
        <ContentTemplate>
            <asp:HiddenField runat="server" ID="hdfResultDocumentId" />
            <asp:HiddenField runat="server" ID="hdfResultFileId" />
            <div class="mb-3">
                <div class="small text-muted mb-1">File được giao ký</div>
                <asp:HyperLink runat="server" ID="lnkResultSource" CssClass="fw-semibold text-break"
                    ToolTip="Xem trước file được giao ký" onclick="FilesBox.LayoutFilePopUp(this); return false;" />
            </div>
            <div class="signing-result-upload">
                <div class="d-flex flex-wrap align-items-center justify-content-between gap-2">
                    <div><div class="fw-semibold">Bản đã ký</div><div class="small text-muted">Word, PDF, JPG hoặc PNG · 1 file, tối đa 1 MB.</div></div>
                    <button type="button" class="btn btn-sm btn-outline-primary"
                        onclick="FilesBox.FocusFileBox(this.closest('.signing-result-upload').querySelector('.file-box')); FilesBox.AddFile();">
                        <i class="fas fa-upload me-1" aria-hidden="true"></i>Chọn / thay file
                    </button>
                </div>
                <SweetSoft:FilesBox runat="server" ID="fbSigningResult" IsMultiple="false" />
            </div>
            <div class="small text-muted mt-2">Kiểm tra bản đã ký trước khi bấm Xác nhận đã ký.</div>
            <div class="mt-3">
                <label class="form-label">Ghi chú (nếu có)</label>
                <asp:TextBox runat="server" ID="txtResultNote" TextMode="MultiLine"
                    Rows="2" MaxLength="500" CssClass="form-control" />
            </div>
        </ContentTemplate>
        <FooterTemplate>
            <asp:Button runat="server" ID="btnConfirmSigned" Text="Xác nhận đã ký"
                CssClass="btn btn-primary btn-sm" CausesValidation="false"
                OnClick="btnConfirmSigned_Click" />
            <asp:Button runat="server" ID="btnCancelResult" Text="Hủy"
                CssClass="btn btn-outline-secondary btn-sm" CausesValidation="false"
                OnClick="btnCancelResult_Click" />
        </FooterTemplate>
    </SweetSoft:ExtraModal>

    <SweetSoft:ExtraModal runat="server" ID="mdlSigningChanges" Type="Primary"
        Size="Normal" FooterButtonClose="false" EnsureChildControlsOnPostback="true">
        <ContentTemplate>
            <asp:HiddenField runat="server" ID="hdfChangesDocumentId" />
            <asp:HiddenField runat="server" ID="hdfChangesFileId" />
            <label class="form-label">Nội dung cần chỉnh sửa</label>
            <asp:TextBox runat="server" ID="txtChangesReason" TextMode="MultiLine"
                Rows="4" MaxLength="500" CssClass="form-control" />
        </ContentTemplate>
        <FooterTemplate>
            <asp:Button runat="server" ID="btnSendChanges" Text="Gửi yêu cầu"
                CssClass="btn btn-primary btn-sm" CausesValidation="false"
                OnClick="btnSendChanges_Click" />
            <asp:Button runat="server" ID="btnCancelChanges" Text="Hủy"
                CssClass="btn btn-outline-secondary btn-sm" CausesValidation="false"
                OnClick="btnCancelChanges_Click" />
        </FooterTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>
