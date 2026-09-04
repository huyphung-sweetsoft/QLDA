<%@ Control Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDocumentDetail.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.Controls.CtrlDocumentDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx"
    TagPrefix="SweetSoft"
    TagName="FilesBox" %>

<style>
    .document-detail {
        --document-purple: #4d0f91;
        --document-purple-soft: #f6f1fb;
        --document-border: #e5e7eb;
        color: #273142;
        padding: 0 1.25rem 1.25rem;
    }

    .document-detail__header {
        align-items: flex-start;
        display: flex;
        gap: 1rem;
        justify-content: space-between;
        padding: .4rem 0 1rem;
    }

    .document-detail__identity {
        align-items: flex-start;
        display: flex;
        gap: .9rem;
        min-width: 0;
    }

    .document-detail__icon {
        align-items: center;
        background: var(--document-purple-soft);
        border-radius: 14px;
        color: var(--document-purple);
        display: flex;
        flex: 0 0 48px;
        font-size: 1.25rem;
        height: 48px;
        justify-content: center;
    }

    .document-detail__title {
        color: #1f2937;
        font-size: 1.35rem;
        font-weight: 700;
        line-height: 1.35;
        margin: .15rem 0 .45rem;
    }

    .document-detail__meta {
        align-items: center;
        display: flex;
        flex-wrap: wrap;
        gap: .45rem;
    }

    .document-detail__scope {
        background: #eef2ff;
        border-radius: 999px;
        color: #4338ca;
        font-size: .75rem;
        font-weight: 600;
        padding: .28rem .65rem;
    }

    .document-detail__code {
        color: #6b7280;
        font-size: .82rem;
    }

    .document-detail__hero-card,
    .document-detail__summary-card,
    .document-detail__section {
        background: #fff;
        border: 1px solid var(--document-border);
        border-radius: 12px;
    }

    .document-detail__hero-card {
        height: 100%;
        padding: 1.15rem;
    }

    .document-detail__official {
        align-items: center;
        display: flex;
        gap: .9rem;
    }

    .document-detail__file-icon {
        align-items: center;
        background: #eef8f1;
        border-radius: 12px;
        color: #22a447;
        display: flex;
        flex: 0 0 44px;
        font-size: 1.15rem;
        height: 44px;
        justify-content: center;
    }

    .document-detail__summary-card {
        height: 100%;
        padding: 1rem;
    }

    .document-detail__summary-label,
    .document-detail__field-label {
        color: #777e90;
        display: block;
        font-size: .75rem;
        font-weight: 600;
        letter-spacing: .02em;
        margin-bottom: .35rem;
        text-transform: uppercase;
    }

    .document-detail__summary-value {
        color: #242731;
        font-size: .92rem;
        font-weight: 600;
    }

    .document-detail__tabs {
        border-bottom: 1px solid var(--document-border);
        display: flex;
        flex-wrap: nowrap;
        gap: .25rem;
        margin: 1.25rem 0 1rem;
        overflow-x: auto;
    }

    .document-detail__tabs .nav-link {
        border-bottom: 2px solid transparent;
        border-radius: 0;
        color: #667085;
        font-weight: 600;
        padding: .75rem .9rem;
        white-space: nowrap;
    }

    .document-detail__tabs .nav-link.active {
        background: transparent;
        border-bottom-color: var(--document-purple);
        color: var(--document-purple);
    }

    .document-detail__section {
        overflow: hidden;
        padding: 1.1rem;
    }

    .document-detail__section-title {
        color: #344054;
        font-size: 1rem;
        font-weight: 700;
        margin-bottom: 1rem;
    }

    .document-detail__field {
        border-bottom: 1px dashed #e8e9ec;
        min-height: 70px;
        padding: .55rem 0;
    }

    .document-detail__field-value {
        color: #222b45;
        overflow-wrap: anywhere;
    }

    .document-detail__empty {
        color: #7a8291;
        padding: 2.5rem 1rem;
        text-align: center;
    }

    .document-detail__empty i {
        color: #b3b8c2;
        display: block;
        font-size: 1.75rem;
        margin-bottom: .7rem;
    }

    .document-detail__table {
        margin-bottom: 0;
        min-width: 900px;
    }

    .document-detail__table thead th {
        background: #f8f7fb;
        border-bottom-width: 1px;
        color: #4d0f91;
        font-size: .78rem;
        white-space: nowrap;
    }

    .document-detail__table td {
        color: #344054;
        vertical-align: middle;
    }

    @media (max-width: 767.98px) {
        .document-detail {
            padding-left: .75rem;
            padding-right: .75rem;
        }

        .document-detail__header {
            display: block;
        }

        .document-detail__header .btn {
            margin-top: 1rem;
            width: 100%;
        }
    }
</style>

<asp:UpdatePanel
    runat="server"
    ID="upDetail"
    UpdateMode="Conditional"
    ChildrenAsTriggers="false">
    <ContentTemplate>
<div class="document-detail">
    <asp:HiddenField runat="server" ID="hdfIdTaiLieu" />

    <div class="document-detail__header">
        <div class="document-detail__identity">
            <div class="document-detail__icon">
                <i class="fas fa-file-alt"></i>
            </div>
            <div>
                <span class="document-detail__scope">
                    <i class="fas fa-building me-1"></i>
                    <%= GetResourceText(BackEndResourceKeys.COMPANY_DOCUMENT) %>
                </span>
                <h2 class="document-detail__title">
                    <asp:Label runat="server" ID="lblDocumentName" />
                </h2>
                <div class="document-detail__meta">
                    <span class="document-detail__code">
                        <%= GetResourceText(BackEndResourceKeys.DOCUMENT_CODE) %>:
                        <asp:Label runat="server" ID="lblDocumentCode" />
                    </span>
                    <asp:Label runat="server" ID="lblDocumentStatus" />
                </div>
            </div>
        </div>

        <SweetSoft:ExtraButton
            runat="server"
            ID="btnBack"
            NavigateUrl="/Documents"
            CssClass="btn-outline-secondary waves-effect"
            ButtonIcon="Reply"
            IsSubmit="false" />
    </div>

    <div class="row g-3">
        <div class="col-xl-6">
            <div class="document-detail__hero-card">
                <span class="document-detail__summary-label">
                    <%= GetResourceText(BackEndResourceKeys.OFFICIAL_FILE) %>
                </span>

                <asp:Panel runat="server" ID="pnlOfficialFile">
                    <div class="document-detail__official">
                        <div class="document-detail__file-icon">
                            <i class="fas fa-file-signature"></i>
                        </div>
                        <div class="flex-grow-1 min-w-0">
                            <asp:HyperLink
                                runat="server"
                                ID="lnkOfficialFile"
                                Target="_blank"
                                CssClass="fw-semibold text-primary text-decoration-underline d-block text-break" />
                            <small class="text-muted">
                                <asp:Label runat="server" ID="lblOfficialFileMeta" />
                            </small>
                            <small class="text-muted d-block">
                                <%= GetResourceText(BackEndResourceKeys.OFFICIAL_DOCUMENT_FILE_HINT) %>
                            </small>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="pnlNoOfficialFile">
                    <div class="document-detail__official">
                        <div class="document-detail__file-icon bg-light text-secondary">
                            <i class="fas fa-file-alt"></i>
                        </div>
                        <div>
                            <div class="fw-semibold">
                                <%= GetResourceText(BackEndResourceKeys.FILE_NOT_UPLOADED) %>
                            </div>
                            <small class="text-muted">
                                <%= GetResourceText(BackEndResourceKeys.NO_OFFICIAL_DOCUMENT_FILE) %>
                            </small>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <div class="col-xl-2 col-md-4">
            <div class="document-detail__summary-card">
                <span class="document-detail__summary-label">
                    <%= GetResourceText(BackEndResourceKeys.SIGNING_HISTORY) %>
                </span>
                <asp:Label runat="server" ID="lblSigningSummary"
                    CssClass="document-detail__summary-value" />
            </div>
        </div>
        <div class="col-xl-2 col-md-4">
            <div class="document-detail__summary-card">
                <span class="document-detail__summary-label">
                    <%= GetResourceText(BackEndResourceKeys.CUSTOMER_DELIVERY_HISTORY) %>
                </span>
                <asp:Label runat="server" ID="lblCustomerSummary"
                    CssClass="document-detail__summary-value" />
            </div>
        </div>
        <div class="col-xl-2 col-md-4">
            <div class="document-detail__summary-card">
                <span class="document-detail__summary-label">
                    <%= GetResourceText(BackEndResourceKeys.PHYSICAL_STORAGE_HISTORY) %>
                </span>
                <asp:Label runat="server" ID="lblStorageSummary"
                    CssClass="document-detail__summary-value" />
            </div>
        </div>
    </div>

    <ul class="nav nav-pills document-detail__tabs" role="tablist">
        <li class="nav-item" role="presentation">
            <button class="nav-link active" data-bs-toggle="tab"
                data-bs-target="#document-overview" type="button" role="tab">
                <i class="fas fa-info-circle me-1"></i>
                <%= GetResourceText(BackEndResourceKeys.OVERVIEW) %>
            </button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" data-bs-toggle="tab"
                data-bs-target="#document-versions" type="button" role="tab">
                <i class="fas fa-layer-group me-1"></i>
                <%= GetResourceText(BackEndResourceKeys.DOCUMENT_VERSIONS) %>
                <asp:Label runat="server" ID="lblVersionCount"
                    CssClass="badge bg-light text-dark ms-1" />
            </button>
        </li>
        <asp:PlaceHolder runat="server" ID="phSigningTab">
            <li class="nav-item" role="presentation">
                <button class="nav-link" data-bs-toggle="tab"
                    data-bs-target="#document-signing" type="button" role="tab">
                    <i class="fas fa-signature me-1"></i>
                    <%= GetResourceText(BackEndResourceKeys.SIGNING_HISTORY) %>
                </button>
            </li>
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" ID="phCustomerTab">
            <li class="nav-item" role="presentation">
                <button class="nav-link" data-bs-toggle="tab"
                    data-bs-target="#document-customer" type="button" role="tab">
                    <i class="fas fa-paper-plane me-1"></i>
                    <%= GetResourceText(BackEndResourceKeys.CUSTOMER_DELIVERY_HISTORY) %>
                </button>
            </li>
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" ID="phStorageTab">
            <li class="nav-item" role="presentation">
                <button class="nav-link" data-bs-toggle="tab"
                    data-bs-target="#document-storage" type="button" role="tab">
                    <i class="fas fa-archive me-1"></i>
                    <%= GetResourceText(BackEndResourceKeys.PHYSICAL_STORAGE_HISTORY) %>
                </button>
            </li>
        </asp:PlaceHolder>
        <li class="nav-item" role="presentation">
            <button class="nav-link" data-bs-toggle="tab"
                data-bs-target="#document-activity" type="button" role="tab">
                <i class="fas fa-history me-1"></i>
                <%= GetResourceText(BackEndResourceKeys.DOCUMENT_ACTIVITY_HISTORY) %>
            </button>
        </li>
    </ul>

    <div class="tab-content">
        <div class="tab-pane fade show active" id="document-overview" role="tabpanel">
            <div class="document-detail__section">
                <div class="document-detail__section-title">
                    <%= GetResourceText(BackEndResourceKeys.BASIC_INFORMATION) %>
                </div>
                <div class="row g-0 gx-lg-4">
                    <div class="col-lg-4 col-md-6">
                        <div class="document-detail__field">
                            <span class="document-detail__field-label"><%= GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP) %></span>
                            <asp:Label runat="server" ID="lblDocumentGroup" CssClass="document-detail__field-value" />
                        </div>
                    </div>
                    <div class="col-lg-4 col-md-6">
                        <div class="document-detail__field">
                            <span class="document-detail__field-label"><%= GetResourceText(BackEndResourceKeys.DOCUMENT_TYPE) %></span>
                            <asp:Label runat="server" ID="lblDocumentType" CssClass="document-detail__field-value" />
                        </div>
                    </div>
                    <div class="col-lg-4 col-md-6">
                        <div class="document-detail__field">
                            <span class="document-detail__field-label"><%= GetResourceText(BackEndResourceKeys.RESPONSIBLE_EMPLOYEE) %></span>
                            <asp:Label runat="server" ID="lblResponsibleEmployee" CssClass="document-detail__field-value" />
                        </div>
                    </div>
                    <div class="col-lg-4 col-md-6">
                        <div class="document-detail__field">
                            <span class="document-detail__field-label"><%= GetResourceText(BackEndResourceKeys.CREATED_BY) %></span>
                            <asp:Label runat="server" ID="lblCreatedBy" CssClass="document-detail__field-value" />
                        </div>
                    </div>
                    <div class="col-lg-4 col-md-6">
                        <div class="document-detail__field">
                            <span class="document-detail__field-label"><%= GetResourceText(BackEndResourceKeys.CREATED_DATE) %></span>
                            <asp:Label runat="server" ID="lblCreatedDate" CssClass="document-detail__field-value" />
                        </div>
                    </div>
                    <div class="col-lg-4 col-md-6">
                        <div class="document-detail__field">
                            <span class="document-detail__field-label"><%= GetResourceText(BackEndResourceKeys.UPDATED_DATE) %></span>
                            <asp:Label runat="server" ID="lblUpdatedDate" CssClass="document-detail__field-value" />
                        </div>
                    </div>
                    <div class="col-12">
                        <div class="document-detail__field border-0">
                            <span class="document-detail__field-label"><%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %></span>
                            <asp:Label runat="server" ID="lblDescription" CssClass="document-detail__field-value" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="tab-pane fade" id="document-versions" role="tabpanel">
            <div class="document-detail__section">
                <div class="document-detail__section-title"><%= GetResourceText(BackEndResourceKeys.DOCUMENT_VERSIONS) %></div>
                <asp:Panel
                    runat="server"
                    ID="pnlVersionUploader"
                    CssClass="border rounded bg-light p-3 mb-3">
                    <h6 class="text-primary mb-2">
                        <i class="fas fa-cloud-upload-alt me-1"></i>
                        <%= GetResourceText(BackEndResourceKeys.UPLOAD_NEW_VERSION) %>
                    </h6>
                    <div class="alert alert-info py-2 mb-3">
                        <%= GetResourceText(BackEndResourceKeys.VERSION_MANAGEMENT_NOTICE) %>
                    </div>
                    <SweetSoft:FilesBox
                        runat="server"
                        ID="fbVersions"
                        IsMultiple="true" />
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlNoVersions" CssClass="document-detail__empty">
                    <i class="fas fa-file-medical"></i>
                    <%= GetResourceText(BackEndResourceKeys.NO_DOCUMENT_VERSIONS) %>
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlVersions" CssClass="table-responsive">
                    <table class="table table-bordered table-hover document-detail__table">
                        <thead><tr>
                            <th><%= GetResourceText(BackEndResourceKeys.VERSION_NUMBER) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.FILE_NAME) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.FILE_SIZE) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.SOURCE) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.CREATED_BY) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.CREATED_DATE) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                        </tr></thead>
                        <tbody>
                            <asp:Repeater runat="server" ID="rptVersions">
                                <ItemTemplate><tr>
                                    <td>
                                        v<%#: Eval("SoPhienBan") %>
                                        <asp:Label runat="server"
                                            Visible='<%# Convert.ToBoolean(Eval("LaPhienBanHienTai")) %>'
                                            Text='<%# GetResourceText(BackEndResourceKeys.CURRENT_VERSION) %>'
                                            CssClass="badge bg-success ms-1" />
                                    </td>
                                    <td><%#: GetFileName(Eval("TenFileGoc"), Eval("TenFile")) %></td>
                                    <td><%#: FormatFileSize(Eval("FileSize")) %></td>
                                    <td><%#: GetVersionSourceText(Eval("NguonTao")) %></td>
                                    <td><%#: GetValueText(Eval("MoTaPhienBan")) %></td>
                                    <td><%#: GetValueText(Eval("TenNguoiTao")) %></td>
                                    <td><%#: FormatDate(Eval("NgayTao")) %></td>
                                    <td>
                                        <asp:HyperLink runat="server"
                                            Visible='<%# CanOpenFile(Eval("FileUrl")) %>'
                                            NavigateUrl='<%# GetFileUrl(Eval("FileUrl")) %>'
                                            Text='<%# GetResourceText(BackEndResourceKeys.OPEN_FILE) %>'
                                            Target="_blank"
                                            CssClass="btn btn-sm btn-outline-primary" />
                                        <asp:Label runat="server"
                                            Visible='<%# !CanOpenFile(Eval("FileUrl")) %>'
                                            Text='<%# GetResourceText(BackEndResourceKeys.FILE_NOT_AVAILABLE) %>'
                                            CssClass="badge bg-warning text-dark" />
                                    </td>
                                </tr></ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </asp:Panel>
            </div>
        </div>

        <asp:PlaceHolder runat="server" ID="phSigningPane">
            <div class="tab-pane fade" id="document-signing" role="tabpanel">
                <div class="document-detail__section">
                    <div class="document-detail__section-title"><%= GetResourceText(BackEndResourceKeys.SIGNING_HISTORY) %></div>
                    <asp:Panel runat="server" ID="pnlNoSigning" CssClass="document-detail__empty">
                        <i class="fas fa-file-signature"></i>
                        <%= GetResourceText(BackEndResourceKeys.NO_SIGNING_HISTORY) %>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlSigning" CssClass="table-responsive">
                        <table class="table table-bordered table-hover document-detail__table">
                            <thead><tr>
                                <th><%= GetResourceText(BackEndResourceKeys.VERSION_NUMBER) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.SENT_BY) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.SIGNER) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.SIGNING_METHOD) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.DATE) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.NOTE) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                            </tr></thead>
                            <tbody><asp:Repeater runat="server" ID="rptSigning"><ItemTemplate><tr>
                                <td>v<%#: Eval("SoPhienBan") %></td>
                                <td><%#: GetValueText(Eval("TenNguoiGui")) %></td>
                                <td><%#: GetValueText(Eval("TenNguoiKyHienThi")) %></td>
                                <td><%#: GetSigningMethodText(Eval("HinhThucKy")) %></td>
                                <td><%#: GetValueText(Eval("TrangThaiTrinhKy")) %></td>
                                <td><%#: GetDateRange(Eval("NgayGui"), Eval("NgayNhanLai")) %></td>
                                <td><%#: GetValueText(Eval("GhiChu")) %></td>
                                <td><asp:HyperLink runat="server"
                                    Visible='<%# HasValue(Eval("IdFileSauKy")) %>'
                                    NavigateUrl='<%# GetFileUrl(Eval("FileSauKyUrl")) %>'
                                    Text='<%# GetResourceText(BackEndResourceKeys.OPEN_FILE) %>'
                                    Target="_blank" CssClass="btn btn-sm btn-outline-primary" /></td>
                            </tr></ItemTemplate></asp:Repeater></tbody>
                        </table>
                    </asp:Panel>
                </div>
            </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder runat="server" ID="phCustomerPane">
            <div class="tab-pane fade" id="document-customer" role="tabpanel">
                <div class="document-detail__section">
                    <div class="document-detail__section-title"><%= GetResourceText(BackEndResourceKeys.CUSTOMER_DELIVERY_HISTORY) %></div>
                    <asp:Panel runat="server" ID="pnlNoCustomer" CssClass="document-detail__empty">
                        <i class="fas fa-paper-plane"></i>
                        <%= GetResourceText(BackEndResourceKeys.NO_CUSTOMER_DELIVERY_HISTORY) %>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlCustomer" CssClass="table-responsive">
                        <table class="table table-bordered table-hover document-detail__table">
                            <thead><tr>
                                <th><%= GetResourceText(BackEndResourceKeys.VERSION_NUMBER) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.CUSTOMER) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.RECIPIENT) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.CHANNEL) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.DATE) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.RESPONSE_DEADLINE) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                            </tr></thead>
                            <tbody><asp:Repeater runat="server" ID="rptCustomer"><ItemTemplate><tr>
                                <td>v<%#: Eval("SoPhienBan") %></td>
                                <td><%#: GetValueText(Eval("TenKhachHang")) %></td>
                                <td><%#: GetRecipientText(Eval("TenNguoiNhan"), Eval("EmailNguoiNhan")) %></td>
                                <td><%#: GetValueText(Eval("KenhGui")) %></td>
                                <td><%#: GetValueText(Eval("TrangThai")) %></td>
                                <td><%#: GetDateRange(Eval("NgayGui"), Eval("NgayNhanLai")) %></td>
                                <td><%#: FormatDate(Eval("HanPhanHoi")) %></td>
                                <td><asp:HyperLink runat="server"
                                    Visible='<%# HasValue(Eval("IdFileNhanLai")) %>'
                                    NavigateUrl='<%# GetFileUrl(Eval("FileNhanLaiUrl")) %>'
                                    Text='<%# GetResourceText(BackEndResourceKeys.OPEN_FILE) %>'
                                    Target="_blank" CssClass="btn btn-sm btn-outline-primary" /></td>
                            </tr></ItemTemplate></asp:Repeater></tbody>
                        </table>
                    </asp:Panel>
                </div>
            </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder runat="server" ID="phStoragePane">
            <div class="tab-pane fade" id="document-storage" role="tabpanel">
                <div class="document-detail__section">
                    <div class="document-detail__section-title"><%= GetResourceText(BackEndResourceKeys.PHYSICAL_STORAGE_HISTORY) %></div>
                    <asp:Panel runat="server" ID="pnlNoStorage" CssClass="document-detail__empty">
                        <i class="fas fa-archive"></i>
                        <%= GetResourceText(BackEndResourceKeys.NO_PHYSICAL_STORAGE_HISTORY) %>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlStorage" CssClass="table-responsive">
                        <table class="table table-bordered table-hover document-detail__table">
                            <thead><tr>
                                <th><%= GetResourceText(BackEndResourceKeys.STORAGE_LOCATION) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.DOCUMENT_CODE) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.ORIGINAL_COPY_CONDITION) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.CURRENT_LOCATION) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.DATE) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.RESPONSIBLE_EMPLOYEE) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.NOTE) %></th>
                            </tr></thead>
                            <tbody><asp:Repeater runat="server" ID="rptStorage"><ItemTemplate><tr>
                                <td><%#: GetStorageLocationText(Eval("MaNoiLuuTru"), Eval("TenNoiLuuTru")) %></td>
                                <td><%#: GetValueText(Eval("MaLuuTru")) %></td>
                                <td><%#: GetPhysicalStorageStatusText(true, Eval("TrangThaiLuuTru")) %></td>
                                <td><%#: GetValueText(Eval("TinhTrangBanGoc")) %></td>
                                <td><%#: GetYesNoText(Eval("LaViTriHienTai")) %></td>
                                <td><%#: GetStorageDateText(Eval("NgayLuu"), Eval("NgayLayRa"), Eval("NgayHoanTra")) %></td>
                                <td><%#: GetValueText(Eval("TenNguoiThucHien")) %></td>
                                <td><%#: GetValueText(Eval("GhiChu")) %></td>
                            </tr></ItemTemplate></asp:Repeater></tbody>
                        </table>
                    </asp:Panel>
                </div>
            </div>
        </asp:PlaceHolder>

        <div class="tab-pane fade" id="document-activity" role="tabpanel">
            <div class="document-detail__section">
                <div class="document-detail__section-title"><%= GetResourceText(BackEndResourceKeys.DOCUMENT_ACTIVITY_HISTORY) %></div>
                <asp:Panel runat="server" ID="pnlNoActivity" CssClass="document-detail__empty">
                    <i class="fas fa-history"></i>
                    <%= GetResourceText(BackEndResourceKeys.NO_DOCUMENT_ACTIVITY) %>
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlActivity" CssClass="table-responsive">
                    <table class="table table-bordered table-hover document-detail__table">
                        <thead><tr>
                            <th><%= GetResourceText(BackEndResourceKeys.DATE) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.ACTION_TYPE) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.RESPONSIBLE_EMPLOYEE) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.REFERENCE_TYPE) %></th>
                        </tr></thead>
                        <tbody><asp:Repeater runat="server" ID="rptActivity"><ItemTemplate><tr>
                            <td><%#: FormatDate(Eval("NgayTao")) %></td>
                            <td><%#: GetValueText(Eval("LoaiHanhDong")) %></td>
                            <td><%#: GetActorText(Eval("TenNguoiThucHien"), Eval("NguoiTao")) %></td>
                            <td><%#: GetActivityDescription(Eval("MoTa"), Eval("NoiDungThayDoi")) %></td>
                            <td><%#: GetValueText(Eval("LoaiThamChieu")) %></td>
                        </tr></ItemTemplate></asp:Repeater></tbody>
                    </table>
                </asp:Panel>
            </div>
        </div>
    </div>
</div>
    </ContentTemplate>
</asp:UpdatePanel>
