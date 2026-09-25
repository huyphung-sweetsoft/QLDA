<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master"
    AutoEventWireup="true" CodeBehind="SigningInbox.aspx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.SigningInbox" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>

<asp:Content ID="ContentMain" ContentPlaceHolderID="cpMain" runat="server">
    <asp:UpdatePanel runat="server" ID="upAssignedFiles" UpdateMode="Conditional">
        <ContentTemplate>
    <asp:HiddenField runat="server" ID="hdfAppliedKeyword" />
    <asp:HiddenField runat="server" ID="hdfAppliedStatus" />
    <asp:HiddenField runat="server" ID="hdfPageIndex" />
    <asp:HiddenField runat="server" ID="hdfPageSize" />
    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen">
                <div class="card-header d-flex flex-wrap justify-content-between align-items-start gap-3">
                    <div>
                        <h4 class="text-primary mb-1">File được giao ký</h4>
                        <p class="text-muted mb-0">Các file chờ bạn xử lý và kết quả ký đã hoàn tất được nhóm theo từng hồ sơ.</p>
                    </div>
                    <span class="badge bg-light text-primary border"><i class="fas fa-user-check me-1"></i> Việc của tôi</span>
                </div>

                <div class="card-body pb-0">
                    <div class="row g-3" aria-label="Tổng quan việc trình ký">
                    <div class="col-md-4">
                        <div class="card border h-100 mb-0"><div class="card-body d-flex align-items-center gap-3">
                            <span class="rounded-circle bg-light text-primary p-3"><i class="fas fa-pen-nib"></i></span>
                            <div><div class="text-muted small">Chờ ký</div><asp:Label runat="server" ID="lblPendingCount" CssClass="fw-bold fs-5" Text="0" /></div>
                        </div></div>
                    </div>
                    <div class="col-md-4">
                        <div class="card border h-100 mb-0"><div class="card-body d-flex align-items-center gap-3">
                            <span class="rounded-circle bg-light text-success p-3"><i class="fas fa-check"></i></span>
                            <div><div class="text-muted small">Đã ký</div><asp:Label runat="server" ID="lblSignedCount" CssClass="fw-bold fs-5" Text="0" /></div>
                        </div></div>
                    </div>
                    <div class="col-md-4">
                        <div class="card border h-100 mb-0"><div class="card-body d-flex align-items-center gap-3">
                            <span class="rounded-circle bg-light text-warning p-3"><i class="fas fa-comment-dots"></i></span>
                            <div><div class="text-muted small">Yêu cầu chỉnh sửa</div><asp:Label runat="server" ID="lblChangesCount" CssClass="fw-bold fs-5" Text="0" /></div>
                        </div></div>
                    </div>
                    </div>
                </div>

                <asp:Panel runat="server" ID="pnlFilters" CssClass="card-header border-top">
                    <div class="d-flex flex-column flex-xl-row gap-3">
                        <div class="d-flex flex-column flex-xl-row gap-3">
                            <asp:Panel runat="server" ID="pnlStatusFilter">
                                <SweetSoft:BootstrapDropdown runat="server" ID="ddlSigningStatus"
                                    Text="Trạng thái trình ký" AutoPostBack="true"
                                    CssClass="border-top-left-radius-1 border-bottom-left-radius-1 border-top-right-radius-1 border-bottom-right-radius-1"
                                    OnSelectedValueChanged="ddlSigningStatus_SelectedValueChanged" />
                            </asp:Panel>
                            <div class="input-group max-w-500">
                                <SweetSoft:ExtraTextBox runat="server" ID="txtSigningSearch"
                                    CssClass="border-primary input-search-filter"
                                    PlaceHolder="Tìm mã hồ sơ, tên hồ sơ hoặc tên file" />
                                <SweetSoft:ExtraButton runat="server" ID="btnApplyFilters"
                                    CssClass="btn-outline-primary btn-search-filter"
                                    IsCustomClass="false" ButtonIcon="Search"
                                    OnClick="btnApplyFilters_Click" />
                                <SweetSoft:ExtraButton runat="server" ID="btnResetFilters"
                                    CssClass="btn-outline-secondary btn-search-filter"
                                    IsCustomClass="false" ButtonIcon="Refresh"
                                    OnClick="btnResetFilters_Click" />
                            </div>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="pnlEmpty" CssClass="card-body text-center text-muted py-5">
                    <i class="fas fa-inbox fs-2 d-block mb-2"></i>
                    Bạn chưa có file nào được giao ký.
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlNoMatches" CssClass="card-body text-center text-muted py-5">
                    <i class="fas fa-search fs-2 d-block mb-2"></i>
                    Không tìm thấy hồ sơ hoặc file phù hợp với điều kiện lọc.
                </asp:Panel>
                <asp:Repeater runat="server" ID="rptAssignedDocuments" OnItemDataBound="rptAssignedDocuments_ItemDataBound">
                    <ItemTemplate>
                        <section class="card mx-3 mb-3">
                            <div class="card-header bg-light d-flex flex-wrap justify-content-between align-items-center gap-2">
                                <div class="d-flex align-items-start gap-2">
                                    <i class="fas fa-folder-open text-primary mt-1"></i>
                                    <div>
                                        <div class="fw-semibold text-primary text-break"><%#: Eval("TenTaiLieu") %></div>
                                        <div class="small text-muted">Mã hồ sơ: <%#: Eval("MaTaiLieu") %></div>
                                    </div>
                                </div>
                                <div class="d-flex flex-wrap gap-1 justify-content-end">
                                    <span class="badge bg-light text-secondary border"><%#: Eval("FileCount") %> file</span>
                                    <asp:Panel runat="server" Visible='<%# Convert.ToInt32(Eval("PendingCount")) > 0 %>'>
                                        <span class="badge bg-info"><%#: Eval("PendingCount") %> chờ ký</span>
                                    </asp:Panel>
                                    <asp:Panel runat="server" Visible='<%# Convert.ToInt32(Eval("SignedCount")) > 0 %>'>
                                        <span class="badge bg-success"><%#: Eval("SignedCount") %> đã ký</span>
                                    </asp:Panel>
                                    <asp:Panel runat="server" Visible='<%# Convert.ToInt32(Eval("ChangesCount")) > 0 %>'>
                                        <span class="badge bg-warning text-dark"><%#: Eval("ChangesCount") %> cần chỉnh sửa</span>
                                    </asp:Panel>
                                </div>
                            </div>
                            <div class="list-group list-group-flush">
                            <asp:Repeater runat="server" ID="rptAssignedFiles" OnItemCommand="rptAssignedFiles_ItemCommand">
                                <ItemTemplate>
                                    <div class="list-group-item">
                                        <div class="d-flex flex-wrap justify-content-between align-items-center gap-3">
                                        <div class="d-flex gap-2 flex-grow-1">
                                            <i class="fas fa-file-alt text-muted mt-1"></i>
                                            <div class="min-w-0">
                                                <div class="d-flex flex-wrap align-items-center gap-2">
                                                    <span class="fw-semibold text-break"><%#: FileName(Eval("TenFileNguonGoc"), Eval("TenFileNguon")) %></span>
                                                    <span class='<%# StatusCss(Eval("TrangThai")) %>'><%#: StatusText(Eval("TrangThai")) %></span>
                                                </div>
                                                <div class="small text-muted d-flex flex-wrap gap-3 mt-1">
                                                    <span><i class="far fa-paper-plane me-1"></i>Gửi lúc <%#: FormatDate(Eval("NgayGui")) %></span>
                                                    <asp:PlaceHolder runat="server" Visible='<%# HasText(Eval("GhiChuYeuCau")) %>'><span><i class="far fa-sticky-note me-1"></i><%#: Eval("GhiChuYeuCau") %></span></asp:PlaceHolder>
                                                </div>
                                                <div class="alert alert-light border-start border-primary mt-2 mb-0 py-2" runat="server" visible='<%# HasText(Eval("GhiChu")) %>'>
                                                    <strong>Phản hồi:</strong> <%#: Eval("GhiChu") %>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="d-flex flex-wrap justify-content-end gap-2">
                                            <asp:HyperLink runat="server" CssClass="btn btn-sm btn-outline-primary"
                                                Target="_blank" Text="<i class='fas fa-download me-1'></i>Tải file ký"
                                                NavigateUrl='<%# FileUrl(Eval("FileNguonUrl")) %>' />
                                            <asp:HyperLink runat="server" CssClass="btn btn-sm btn-outline-success"
                                                Target="_blank" Text="<i class='fas fa-file-download me-1'></i>Bản đã ký"
                                                Visible='<%# HasText(Eval("FileSauKyUrl")) %>'
                                                NavigateUrl='<%# FileUrl(Eval("FileSauKyUrl")) %>' />
                                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-primary"
                                                Visible='<%# IsPending(Eval("TrangThai")) %>'
                                                CommandName="UPLOAD_RESULT" CommandArgument='<%# Eval("IdTrinhKyTaiLieuFile") %>'
                                                CausesValidation="false" Text="<i class='fas fa-upload me-1'></i>Tải bản đã ký lên" />
                                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-warning"
                                                Visible='<%# IsPending(Eval("TrangThai")) %>'
                                                CommandName="REQUEST_CHANGES" CommandArgument='<%# Eval("IdTrinhKyTaiLieuFile") %>'
                                                CausesValidation="false" Text="<i class='fas fa-comment-dots me-1'></i>Yêu cầu chỉnh sửa" />
                                        </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            </div>
                        </section>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel runat="server" ID="pnlPagination" CssClass="card-body pt-0">
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
            <p class="small text-muted">Tải bản đã ký (PDF, JPG hoặc PNG), rồi bấm Xác nhận đã ký.</p>
            <SweetSoft:FilesBox runat="server" ID="fbSigningResult" IsMultiple="false" />
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
