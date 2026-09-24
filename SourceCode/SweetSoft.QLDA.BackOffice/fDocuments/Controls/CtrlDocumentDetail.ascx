<%@ Control Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDocumentDetail.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.Controls.CtrlDocumentDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx"
    TagPrefix="SweetSoft"
    TagName="FilesBox" %>

<style>
    .document-file-set .sorting-control,
    .document-file-set .sort-item,
    .document-file-set .file-actions { display: none !important; }
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

    .document-content-view { overflow-wrap: anywhere; overflow-x: auto; max-height: 420px; }
    .document-content-view table { border-collapse: collapse; max-width: 100%; }
    .document-content-view td, .document-content-view th { border: 1px solid #d9dee3; padding: .4rem; }
    .document-content-view pre { white-space: pre-wrap; }

    .document-file-history { position: relative; padding: .25rem 0 .25rem 1.5rem; }
    .document-file-history::before {
        content: ""; position: absolute; top: .5rem; bottom: .5rem; left: .45rem;
        width: 2px; background: #e5d8f4;
    }
    .document-file-history__item { position: relative; padding: 0 0 1rem 1rem; }
    .document-file-history__dot {
        position: absolute; z-index: 1; top: .85rem; left: -.02rem;
        width: .8rem; height: .8rem; border-radius: 50%;
        background: #4d0f91; box-shadow: 0 0 0 4px #f6f1fb;
    }
    .document-file-history__card {
        border: 1px solid #e4d9f0; border-radius: .55rem; background: #fff;
        padding: .85rem 1rem; box-shadow: 0 2px 6px rgba(35, 18, 56, .04);
    }
    .document-file-history__head {
        display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem;
    }
    .document-file-history__version { color: #4d0f91; font-weight: 600; }
    .document-file-history__meta { color: #667085; font-size: .82rem; margin-top: .2rem; }
    .document-file-history__summary { color: #344054; font-weight: 500; margin-top: .7rem; }
    .document-file-history__description { color: #667085; font-size: .9rem; margin-top: .25rem; }
    .document-file-history__actions { display: flex; flex-wrap: wrap; gap: .35rem; justify-content: flex-end; }
    .document-version-file-list { display: grid; gap: .6rem; }
    .document-version-file {
        display: flex; align-items: center; justify-content: space-between; gap: 1rem;
        border: 1px solid #e4e7ec; border-radius: .45rem; padding: .7rem .8rem;
    }
    .document-version-file__name { font-weight: 500; overflow-wrap: anywhere; }
    .document-version-file__meta { color: #667085; font-size: .8rem; margin-top: .15rem; }
    @media (max-width: 575.98px) {
        .document-file-history__head, .document-version-file { display: block; }
        .document-file-history__actions { justify-content: flex-start; margin-top: .65rem; }
        .document-version-file .btn { margin-top: .55rem; }
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
    <div class="d-flex justify-content-end mb-2">
        <asp:Button runat="server" ID="btnDocumentPermissions" Text="Cấp quyền" Visible="false"
            CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnDocumentPermissions_Click" />
    </div>

    <div class="document-detail__header">
        <div class="document-detail__identity">
            <div class="document-detail__icon">
                <i class="fas fa-file-alt"></i>
            </div>
            <div>
                <span class="document-detail__scope">
                    <i class="<%= DocumentScopeIconCss %>"></i>
                    <%= DocumentScopeText %>
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

            <div class="document-detail__section">
                <div class="document-detail__section-title">
                    <%= GetResourceText(BackEndResourceKeys.BASIC_INFORMATION) %>
                </div>
                <div class="row g-0 gx-lg-4">
                    <div class="col-lg-4 col-md-6" runat="server" visible="false">
                        <div class="document-detail__field">
                            <span class="document-detail__field-label"><%= GetResourceText(BackEndResourceKeys.DOCUMENT_GROUP) %></span>
                            <asp:Label runat="server" ID="lblDocumentGroup" CssClass="document-detail__field-value" />
                        </div>
                    </div>
                    <div class="col-lg-4 col-md-6">
                        <div class="document-detail__field">
                            <span class="document-detail__field-label"><%= "Loại hồ sơ" %></span>
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
                    <div class="col-12" runat="server" visible="false">
                        <div class="document-detail__field border-0">
                            <span class="document-detail__field-label"><%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %></span>
                            <asp:Label runat="server" ID="lblDescription" CssClass="document-detail__field-value" />
                        </div>
                    </div>
                </div>
            </div>
    <div class="document-detail__section mb-3">
        <div class="document-detail__section-title">Nội dung hồ sơ</div>
        <div class="document-content-view"><asp:Literal runat="server" ID="litDocumentContent" /></div>
    </div>
    <ul class="nav nav-pills document-detail__tabs" role="tablist">
        <li class="nav-item" role="presentation">
            <button class="nav-link active" data-bs-toggle="tab"
                data-bs-target="#document-versions" type="button" role="tab">
                <i class="fas fa-layer-group me-1"></i>
                Các file hồ sơ
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
        <div class="tab-pane fade show active" id="document-versions" role="tabpanel">
            <div class="document-detail__section">
                <div class="document-detail__section-title">Các file hồ sơ</div>
                <asp:Panel
                    runat="server"
                    ID="pnlVersionUploader"
                    CssClass="border rounded bg-light p-3 mb-3 document-file-box">
                    <h6 class="text-primary mb-2">
                        <i class="fas fa-cloud-upload-alt me-1"></i>
                        Cập nhật bộ file hồ sơ
                    </h6>
                    <div class="alert alert-info py-2 mb-3">
                        Thêm hoặc gỡ file rồi bấm Lưu thay đổi để tạo một phiên bản chứa cả bộ file.
                        File đã gỡ vẫn được giữ trong phiên bản cũ. Hủy bỏ không tạo phiên bản mới.
                        Hiện trình ký và gửi khách chỉ hỗ trợ phiên bản có một file;
                        xử lý nhiều file sẽ được bổ sung ở chặng tiếp theo.
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
                <asp:Panel runat="server" ID="pnlVersions" CssClass="document-file-history">
                    <asp:Repeater
                        runat="server"
                        ID="rptVersions"
                        OnItemCommand="rptVersions_ItemCommand">
                        <ItemTemplate>
                            <article class="document-file-history__item">
                                <div class="document-file-history__dot"></div>
                                <div class="document-file-history__card">
                                    <div class="document-file-history__head">
                                        <div>
                                            <div class="document-file-history__version">
                                                Mốc v<%#: Eval("SoPhienBan") %>
                                                <asp:Label runat="server"
                                                    Visible='<%# Convert.ToBoolean(Eval("LaPhienBanHienTai")) %>'
                                                    Text="Hiện tại"
                                                    CssClass="badge bg-success ms-1" />
                                            </div>
                                            <div class="document-file-history__meta">
                                                <%#: FormatDate(Eval("NgayTao")) %>
                                                · <%#: GetValueText(Eval("TenNguoiTao")) %>
                                                · <%#: GetVersionSourceText(Eval("NguonTao")) %>
                                            </div>
                                        </div>
                                        <div class="document-file-history__actions">
                                            <asp:LinkButton runat="server"
                                                CommandName="VIEW_VERSION_FILES"
                                                CommandArgument='<%# Eval("IdPhienBanTaiLieu") %>'
                                                Text="Xem file"
                                                CausesValidation="false"
                                                CssClass="btn btn-sm btn-outline-primary" />
                                            <asp:LinkButton runat="server"
                                                Visible='<%# CanRestoreVersion(Eval("LaPhienBanHienTai")) %>'
                                                CommandName="RESTORE_VERSION"
                                                CommandArgument='<%# Eval("IdPhienBanTaiLieu") %>'
                                                Text="Khôi phục mốc này"
                                                CausesValidation="false"
                                                CssClass="btn btn-sm btn-outline-warning" />
                                            <asp:LinkButton runat="server"
                                                Visible='<%# Convert.ToInt32(Eval("FileCount")) == 1 && CanSetOfficialFile(Eval("IdFile"), Eval("FileUrl")) %>'
                                                CommandName="SET_OFFICIAL_FILE"
                                                CommandArgument='<%# Eval("IdPhienBanTaiLieu") %>'
                                                Text='<%# GetResourceText(BackEndResourceKeys.SET_AS_OFFICIAL_FILE) %>'
                                                CausesValidation="false"
                                                CssClass="btn btn-sm btn-outline-success" />
                                            <asp:LinkButton runat="server"
                                                Visible='<%# Convert.ToInt32(Eval("FileCount")) == 1 && CanClearOfficialFile(Eval("IdFile")) %>'
                                                CommandName="CLEAR_OFFICIAL_FILE"
                                                CommandArgument='<%# Eval("IdPhienBanTaiLieu") %>'
                                                Text='<%# GetResourceText(BackEndResourceKeys.CLEAR_OFFICIAL_FILE) %>'
                                                CausesValidation="false"
                                                CssClass="btn btn-sm btn-outline-danger" />
                                        </div>
                                    </div>
                                    <div class="document-file-history__summary">
                                        <i class="fas fa-layer-group me-1"></i>
                                        <%#: GetVersionFileSummary(Eval("FileCount")) %>
                                    </div>
                                    <div class="document-file-history__description">
                                        <%#: GetValueText(Eval("MoTaPhienBan")) %>
                                    </div>
                                </div>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>
            </div>
        </div>

        <asp:PlaceHolder runat="server" ID="phSigningPane">
            <div class="tab-pane fade" id="document-signing" role="tabpanel">
                <div class="document-detail__section">
                    <div class="document-detail__section-title d-flex flex-wrap align-items-center justify-content-between gap-2">
                        <span><%= GetResourceText(BackEndResourceKeys.SIGNING_HISTORY) %></span>
                        <asp:Panel runat="server" ID="pnlSigningActions">
                            <SweetSoft:ExtraButton
                                runat="server"
                                ID="btnOpenSubmitSigning"
                                OnClick="btnOpenSubmitSigning_Click"
                                ButtonStyle="Primary"
                                ButtonSize="Small"
                                ButtonIcon="Send"
                                IsSubmit="false" />
                        </asp:Panel>
                    </div>
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
                            <tbody><asp:Repeater
                                runat="server"
                                ID="rptSigning"
                                OnItemCommand="rptSigning_ItemCommand"><ItemTemplate><tr>
                                <td>v<%#: Eval("SoPhienBan") %></td>
                                <td><%#: GetValueText(Eval("TenNguoiGui")) %></td>
                                <td><%#: GetValueText(Eval("TenNguoiKyHienThi")) %></td>
                                <td><%#: GetSigningMethodText(Eval("HinhThucKy")) %></td>
                                <td><asp:Label runat="server"
                                    Text='<%# GetSigningStatusText(Eval("TrangThaiTrinhKy")) %>'
                                    CssClass='<%# GetSigningStatusCss(Eval("TrangThaiTrinhKy")) %>' /></td>
                                <td><%#: GetDateRange(Eval("NgayGui"), Eval("NgayNhanLai")) %></td>
                                <td><%#: GetValueText(Eval("GhiChu")) %></td>
                                <td><div class="d-flex flex-wrap gap-1">
                                    <asp:HyperLink runat="server"
                                        Visible='<%# HasValue(Eval("IdFileSauKy")) && CanOpenFile(Eval("FileSauKyUrl")) %>'
                                        NavigateUrl='<%# GetFileUrl(Eval("FileSauKyUrl")) %>'
                                        Text='<%# GetResourceText(BackEndResourceKeys.OPEN_FILE) %>'
                                        Target="_blank" CssClass="btn btn-sm btn-outline-primary" />
                                    <asp:LinkButton runat="server"
                                        Visible='<%# CanManagePendingSigning(Eval("TrangThaiTrinhKy")) %>'
                                        CommandName="CONFIRM_SIGNED"
                                        CommandArgument='<%# Eval("IdTrinhKyTaiLieu") %>'
                                        Text='<%# GetResourceText(BackEndResourceKeys.CONFIRM_SIGNED) %>'
                                        CausesValidation="false"
                                        CssClass="btn btn-sm btn-outline-success" />
                                    <asp:LinkButton runat="server"
                                        Visible='<%# CanManagePendingSigning(Eval("TrangThaiTrinhKy")) %>'
                                        CommandName="REQUEST_CHANGES"
                                        CommandArgument='<%# Eval("IdTrinhKyTaiLieu") %>'
                                        Text='<%# GetResourceText(BackEndResourceKeys.REQUEST_CHANGES) %>'
                                        CausesValidation="false"
                                        CssClass="btn btn-sm btn-outline-warning" />
                                </div></td>
                            </tr></ItemTemplate></asp:Repeater></tbody>
                        </table>
                    </asp:Panel>
                </div>
            </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder runat="server" ID="phCustomerPane">
            <div class="tab-pane fade" id="document-customer" role="tabpanel">
                <div class="document-detail__section">
                    <div class="document-detail__section-title d-flex flex-wrap align-items-center justify-content-between gap-2">
                        <span><%= GetResourceText(BackEndResourceKeys.CUSTOMER_DELIVERY_HISTORY) %></span>
                        <asp:Panel runat="server" ID="pnlCustomerActions">
                            <SweetSoft:ExtraButton
                                runat="server"
                                ID="btnOpenCustomerDelivery"
                                OnClick="btnOpenCustomerDelivery_Click"
                                ButtonStyle="Primary"
                                ButtonSize="Small"
                                ButtonIcon="Send"
                                IsSubmit="false" />
                        </asp:Panel>
                    </div>
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
                            <tbody><asp:Repeater
                                runat="server"
                                ID="rptCustomer"
                                OnItemCommand="rptCustomer_ItemCommand"><ItemTemplate><tr>
                                <td>v<%#: Eval("SoPhienBan") %></td>
                                <td><%#: GetValueText(Eval("TenKhachHang")) %></td>
                                <td><%#: GetRecipientText(Eval("TenNguoiNhan"), Eval("EmailNguoiNhan")) %></td>
                                <td><%#: GetCustomerDeliveryChannelText(Eval("KenhGui")) %></td>
                                <td><span class='<%# GetCustomerStatusCss(Eval("TrangThai")) %>'><%#: GetCustomerStatusText(true, Eval("TrangThai")) %></span></td>
                                <td><%#: GetDateRange(Eval("NgayGui"), Eval("NgayNhanLai")) %></td>
                                <td><%#: FormatDate(Eval("HanPhanHoi")) %></td>
                                <td><div class="d-flex flex-wrap gap-1">
                                    <asp:LinkButton
                                        runat="server"
                                        Visible='<%# CanManageCustomerDelivery() %>'
                                        CommandName="UPDATE_CUSTOMER_DELIVERY"
                                        CommandArgument='<%# Eval("IdGuiNhanKhachHang") %>'
                                        Text='<%# GetResourceText(BackEndResourceKeys.UPDATE) %>'
                                        CausesValidation="false"
                                        CssClass="btn btn-sm btn-outline-primary" />
                                    <asp:HyperLink runat="server"
                                        Visible='<%# HasValue(Eval("IdFileNhanLai")) %>'
                                        NavigateUrl='<%# GetFileUrl(Eval("FileNhanLaiUrl")) %>'
                                        Text='<%# GetResourceText(BackEndResourceKeys.OPEN_FILE) %>'
                                        Target="_blank" CssClass="btn btn-sm btn-outline-secondary" />
                                </div></td>
                            </tr></ItemTemplate></asp:Repeater></tbody>
                        </table>
                    </asp:Panel>
                </div>
            </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder runat="server" ID="phStoragePane">
            <div class="tab-pane fade" id="document-storage" role="tabpanel">
                <div class="document-detail__section">
                    <div class="document-detail__section-title d-flex flex-wrap align-items-center justify-content-between gap-2">
                        <span><%= GetResourceText(BackEndResourceKeys.PHYSICAL_STORAGE_HISTORY) %></span>
                        <asp:Panel runat="server" ID="pnlPhysicalStorageActions">
                            <SweetSoft:ExtraButton
                                runat="server"
                                ID="btnOpenPhysicalStorage"
                                OnClick="btnOpenPhysicalStorage_Click"
                                ButtonStyle="Primary"
                                ButtonSize="Small"
                                ButtonIcon="QRCode"
                                IsSubmit="false" />
                        </asp:Panel>
                    </div>
                    <asp:Panel runat="server" ID="pnlNoStorage" CssClass="document-detail__empty">
                        <i class="fas fa-archive"></i>
                        <%= GetResourceText(BackEndResourceKeys.NO_PHYSICAL_STORAGE_HISTORY) %>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlStorage" CssClass="table-responsive">
                        <table class="table table-bordered table-hover document-detail__table">
                            <thead><tr>
                                <th><%= GetResourceText(BackEndResourceKeys.STORAGE_LOCATION) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.PHYSICAL_STORAGE_CODE) %></th>
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
                            <td><%#: GetActivityTypeText(Eval("LoaiHanhDong")) %></td>
                            <td><%#: GetActorText(Eval("TenNguoiThucHien"), Eval("NguoiTao")) %></td>
                            <td><%#: GetActivityDescription(Eval("MoTa"), Eval("NoiDungThayDoi")) %></td>
                            <td><%#: GetActivityReferenceText(Eval("LoaiThamChieu")) %></td>
                        </tr></ItemTemplate></asp:Repeater></tbody>
                    </table>
                </asp:Panel>
            </div>
        </div>
    </div>
</div>
    </ContentTemplate>
</asp:UpdatePanel>

<SweetSoft:ExtraModal
    runat="server"
    ID="mdlSubmitSigning"
    Type="Primary"
    Size="Normal"
    FooterButtonClose="false"
    EnsureChildControlsOnPostback="true">
    <ContentTemplate>
        <asp:Panel runat="server" ID="pnlSubmitSigningForm" CssClass="validationEngineContainer">
            <asp:HiddenField runat="server" ID="hdfSubmitSigningDocumentId" />
            <asp:HiddenField runat="server" ID="hdfSubmitSigningSigner" />
            <div class="alert alert-light border py-2">
                <div class="small text-muted"><%= GetResourceText(BackEndResourceKeys.SIGNING_VERSION) %></div>
                <asp:Label runat="server" ID="lblSubmitSigningVersion" CssClass="fw-semibold" />
                <span class="mx-1">·</span>
                <asp:Label runat="server" ID="lblSubmitSigningMethod" CssClass="fw-semibold" />
            </div>
            <div class="mb-3">
                <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.SIGNER) %></label>
                <SweetSoft:ExtraDropdown
                    runat="server"
                    ID="ddlSubmitSigningSigner"
                    ValueIsOfTypeGUID="true"
                    SimpleInit="true"
                    CssClass="form-select" />
            </div>
            <div class="mb-1">
                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SIGNING_NOTE) %></label>
                <SweetSoft:ExtraTextBox
                    runat="server"
                    ID="txtSubmitSigningNote"
                    TextMode="MultiLine"
                    Rows="3"
                    MaxLength="500" />
            </div>
        </asp:Panel>
    </ContentTemplate>
    <FooterTemplate>
        <div class="d-flex gap-2">
            <asp:Button
                runat="server"
                ID="btnSubmitSigning"
                OnClick="btnSubmitSigning_Click"
                OnClientClick="CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-primary btn-sm waves-effect waves-light" />
            <asp:Button
                runat="server"
                ID="btnCancelSubmitSigning"
                OnClick="btnCancelSubmitSigning_Click"
                OnClientClick="CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-outline-secondary btn-sm waves-effect waves-light" />
        </div>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<SweetSoft:ExtraModal runat="server" ID="mdlDocumentPermissions" Title="Cấp quyền hồ sơ" Size="Large"
    Position="modal-dialog-scrollable" FooterButtonClose="true"
    EnsureChildControlsOnPostback="true">
    <ContentTemplate>
        <style type="text/css">
            .document-permission-intro { background: linear-gradient(135deg,#f6f3ff,#f5fbff); border: 1px solid #e5e7eb; border-radius: 12px; padding: 14px 16px; color: #475467; }
            .document-permission-intro strong { color: #4c1d95; }
            .document-permission-toolbar { display:flex; align-items:center; justify-content:space-between; gap:12px; margin: 14px 0 10px; }
            .document-permission-toolbar__title { font-weight: 700; color:#344054; }
            .document-permission-toolbar__hint { color:#667085; font-size:.82rem; }
            .document-permission-list { max-height:55vh; overflow:auto; display:grid; gap:10px; padding-right:4px; }
            .document-permission-card { border:1px solid #e4e7ec; border-radius:12px; background:#fff; overflow:hidden; box-shadow:0 2px 7px rgba(16,24,40,.04); }
            .document-permission-card[open] { border-color:#b692f6; box-shadow:0 4px 14px rgba(105,56,239,.12); }
            .document-permission-card__summary { cursor:pointer; list-style:none; display:flex; align-items:center; gap:12px; padding:12px 14px; }
            .document-permission-card__summary::-webkit-details-marker { display:none; }
            .document-permission-card__avatar { width:34px; height:34px; display:inline-flex; align-items:center; justify-content:center; border-radius:50%; background:#ede9fe; color:#5b21b6; font-weight:700; flex:0 0 auto; }
            .document-permission-card__name { font-weight:600; color:#344054; flex:1 1 auto; min-width:0; }
            .document-permission-card__badges { display:flex; flex-wrap:wrap; justify-content:flex-end; gap:4px; }
            .document-permission-card__chevron { color:#98a2b3; transition:transform .15s ease; }
            .document-permission-card[open] .document-permission-card__chevron { transform:rotate(180deg); }
            .document-permission-card__body { padding:0 14px 14px; border-top:1px solid #f0f2f5; }
            .document-permission-grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(175px,1fr)); gap:8px; padding-top:12px; }
            .document-permission-item { border:1px solid #eaecf0; border-radius:9px; padding:9px 10px; background:#fcfcfd; }
            .document-permission-item .form-check { margin:0; }
            .document-permission-item small { display:block; color:#667085; margin-left:24px; margin-top:2px; line-height:1.25; }
            .document-permission-item.is-view { background:#f5f3ff; border-color:#ddd6fe; }
            .document-permission-item.is-danger { background:#fff7f7; border-color:#fecaca; }
            .document-permission-locked { color:#98a2b3; font-size:.78rem; margin-top:10px; }
            .document-permission-scope { display:flex; align-items:center; justify-content:space-between; gap:16px; padding:11px 13px; margin-top:14px; border:1px solid #d0d5dd; border-radius:11px; background:linear-gradient(180deg,#fff 0%,#fbfaff 100%); box-shadow:0 2px 8px rgba(16,24,40,.04); }
            .document-permission-scope__copy { min-width:0; }
            .document-permission-scope__eyebrow { display:block; color:#667085; font-size:.72rem; font-weight:700; letter-spacing:.04em; text-transform:uppercase; }
            .document-permission-scope__hint { display:block; color:#667085; font-size:.8rem; margin-top:2px; }
            .document-permission-scope__switch { display:flex; align-items:center; gap:8px; flex:0 0 auto; padding:4px 6px; border:1px solid #eaecf0; border-radius:999px; background:#f8fafc; color:#475467; font-size:.82rem; font-weight:600; }
            .document-permission-scope__switch > span { white-space:nowrap; padding:4px 2px; }
            .document-permission-scope__switch > span:first-child { color:#475467; }
            .document-permission-scope__switch > span:last-child { color:#98a2b3; }
            .document-permission-scope__switch .document-permission-scope__input { display:inline-flex; align-items:center; margin:0; line-height:0; }
            .document-permission-scope__switch input[type=checkbox] { appearance:none !important; -webkit-appearance:none !important; -moz-appearance:none !important; box-sizing:border-box; flex:0 0 46px; width:46px !important; min-width:46px; height:26px !important; min-height:26px; padding:0 !important; margin:0 !important; cursor:pointer; border:1px solid #cbd5e1 !important; border-radius:999px !important; background-color:#d0d5dd !important; background-image:radial-gradient(circle at 13px 13px,#fff 0 9px,rgba(255,255,255,.98) 9px 9.5px,transparent 10px) !important; background-repeat:no-repeat !important; background-position:0 0 !important; box-shadow:inset 0 1px 2px rgba(16,24,40,.12); outline:none; transition:background-color .18s ease,border-color .18s ease,box-shadow .18s ease,background-image .18s ease; }
            .document-permission-scope__switch input[type=checkbox]:checked { border-color:#4c1d95 !important; background-color:#5b21b6 !important; background-image:radial-gradient(circle at 33px 13px,#fff 0 9px,rgba(255,255,255,.98) 9px 9.5px,transparent 10px) !important; box-shadow:0 2px 6px rgba(91,33,182,.28); }
            .document-permission-scope__switch input[type=checkbox]:not(:disabled):hover { border-color:#7c3aed !important; box-shadow:0 0 0 4px rgba(124,58,237,.12); }
            .document-permission-scope__switch input[type=checkbox]:focus-visible { box-shadow:0 0 0 4px rgba(124,58,237,.2); }
            .document-permission-scope__switch input[type=checkbox]:disabled { opacity:.6; cursor:not-allowed; }
            .document-permission-scope__empty { padding:24px 14px; border:1px dashed #d0d5dd; border-radius:10px; color:#667085; text-align:center; background:#fcfcfd; }
            @media (max-width:575.98px) { .document-permission-toolbar { display:block; } .document-permission-toolbar__hint { margin-top:4px; } .document-permission-card__badges { justify-content:flex-start; } }
            @media (max-width:575.98px) { .document-permission-scope { display:block; } .document-permission-scope__switch { margin-top:9px; justify-content:flex-end; } }
        </style>
        <div class="document-permission-intro">
            <div class="mb-1"><strong>Phân quyền theo từng hồ sơ</strong></div>
            Chọn một nhân viên rồi bấm mở rộng để cấp đúng thao tác cần thiết.
            <strong>Xem</strong> bao gồm mở hồ sơ và tải file hiện có xuống;
            các quyền sửa còn lại được tách riêng. Nhóm người dùng cần có quyền
            <strong>Xem</strong> và <strong>Cập nhật</strong> thì mới nhận được quyền thao tác trên từng hồ sơ.
        </div>
        <asp:Panel runat="server" ID="pnlGrantExternalUsers" CssClass="document-permission-scope">
            <div class="document-permission-scope__copy">
                <span class="document-permission-scope__eyebrow">Đối tượng cấp quyền</span>
                <span class="document-permission-scope__hint">Chuyển danh sách giữa thành viên dự án và nhân viên chưa tham gia dự án.</span>
            </div>
            <div class="document-permission-scope__switch" title="Chỉ PM hệ thống được chọn nhân viên ngoài dự án">
                <span>Trong dự án</span>
                <asp:CheckBox runat="server" ID="chkGrantExternalUsers"
                    CssClass="document-permission-scope__input"
                    Text=""
                    AutoPostBack="true"
                    OnCheckedChanged="chkGrantExternalUsers_CheckedChanged" />
                <span>Ngoài dự án</span>
            </div>
        </asp:Panel>
        <div class="document-permission-toolbar">
            <div class="document-permission-toolbar__title"><i class="fas fa-users me-1"></i> Nhân viên được cấp quyền</div>
            <div class="document-permission-toolbar__hint">Bấm vào từng tên để mở danh sách quyền</div>
        </div>
        <div class="document-permission-list">
            <asp:Repeater runat="server" ID="rptDocumentPermissions">
                <ItemTemplate>
                    <details class="document-permission-card">
                        <summary class="document-permission-card__summary">
                            <span class="document-permission-card__avatar"><%#: GetPermissionInitial(Eval("DisplayName")) %></span>
                            <span class="document-permission-card__name"><%#: Eval("DisplayName") %></span>
                            <span class="document-permission-card__badges">
                                <asp:Label runat="server" Visible='<%# Convert.ToBoolean(Eval("IsResponsibleDefault")) %>' CssClass="badge bg-light text-dark" Text="Người phụ trách" />
                                <asp:Label runat="server" Visible='<%# !Convert.ToBoolean(Eval("IsProjectMember")) %>' CssClass="badge bg-info text-dark" Text="Ngoài dự án" />
                            </span>
                            <i class="fas fa-chevron-down document-permission-card__chevron"></i>
                            <asp:HiddenField runat="server" ID="grantUserId" Value='<%# Eval("UserId") %>' />
                        </summary>
                        <div class="document-permission-card__body">
                            <div class="document-permission-grid">
                                <div class="document-permission-item is-view">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantView"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; !Convert.ToBoolean(Eval("IsResponsibleDefault")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanView")) &amp;&amp; Convert.ToBoolean(Eval("MaxView")) %>' Text="Xem hồ sơ" /></div>
                                    <small>Mở hồ sơ và tải file hiện có</small>
                                </div>
                                <div class="document-permission-item">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantUpdateInfo"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; Convert.ToBoolean(Eval("MaxUpdateInfo")) &amp;&amp; !Convert.ToBoolean(Eval("IsResponsibleDefault")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanUpdateInfo")) &amp;&amp; Convert.ToBoolean(Eval("MaxUpdateInfo")) %>' Text="Thông tin chung" /></div>
                                    <small>Sửa tên, mã, loại và nội dung hồ sơ</small>
                                </div>
                                <div class="document-permission-item">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantManageFiles"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; Convert.ToBoolean(Eval("MaxManageFiles")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanManageFiles")) &amp;&amp; Convert.ToBoolean(Eval("MaxManageFiles")) %>' Text="Quản lý file" /></div>
                                    <small>Thêm, thay, gỡ file và khôi phục mốc</small>
                                </div>
                                <div class="document-permission-item">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantSigning"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; Convert.ToBoolean(Eval("MaxSigning")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanSigning")) &amp;&amp; Convert.ToBoolean(Eval("MaxSigning")) %>' Text="Trình ký" /></div>
                                    <small>Gửi trình ký, nhận kết quả và yêu cầu chỉnh</small>
                                </div>
                                <div class="document-permission-item">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantCustomerDelivery"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; Convert.ToBoolean(Eval("MaxCustomerDelivery")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanCustomerDelivery")) &amp;&amp; Convert.ToBoolean(Eval("MaxCustomerDelivery")) %>' Text="Gửi khách hàng" /></div>
                                    <small>Gửi hồ sơ và cập nhật phản hồi</small>
                                </div>
                                <div class="document-permission-item">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantPhysicalStorage"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; Convert.ToBoolean(Eval("MaxPhysicalStorage")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanPhysicalStorage")) &amp;&amp; Convert.ToBoolean(Eval("MaxPhysicalStorage")) %>' Text="Lưu bản cứng" /></div>
                                    <small>Ghi nhận nơi lưu trữ và mã lưu trữ</small>
                                </div>
                                <div class="document-permission-item is-danger">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantDelete"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; Convert.ToBoolean(Eval("MaxDelete")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanDelete")) &amp;&amp; Convert.ToBoolean(Eval("MaxDelete")) %>' Text="Xóa hồ sơ" /></div>
                                    <small>Xóa mềm hồ sơ theo chính sách hệ thống</small>
                                </div>
                            </div>
                            <asp:Label runat="server" Visible='<%# Convert.ToBoolean(Eval("IsResponsibleDefault")) %>' CssClass="document-permission-locked" Text="Người phụ trách mặc định được xem và sửa thông tin chung nếu nhóm có quyền Xem và Cập nhật. Các thao tác khác vẫn cần tích riêng." />
                        </div>
                    </details>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <p class="text-muted small mt-3 mb-0">Quyền nhóm là mức trần. Khi chọn một quyền thao tác, hệ thống tự yêu cầu quyền xem; bỏ toàn bộ quyền sẽ thu hồi cấp riêng trên hồ sơ.</p>
    </ContentTemplate>
    <FooterTemplate>
        <asp:Button runat="server" ID="btnSaveDocumentPermissions" Text="Lưu quyền" CssClass="btn btn-primary"
            CausesValidation="false" OnClick="btnSaveDocumentPermissions_Click" />
    </FooterTemplate>
</SweetSoft:ExtraModal>

<SweetSoft:ExtraModal runat="server" ID="mdlVersionFiles" Title="Các file trong mốc lịch sử" Size="Large"
    Position="modal-dialog-scrollable" FooterButtonClose="true"
    EnsureChildControlsOnPostback="true">
    <ContentTemplate>
        <asp:Label runat="server" ID="lblVersionFilesSummary" CssClass="text-muted d-block mb-3" />
        <asp:Panel runat="server" ID="pnlVersionFiles" CssClass="document-version-file-list">
            <asp:Repeater runat="server" ID="rptVersionFiles">
                <ItemTemplate>
                    <div class="document-version-file">
                        <div>
                            <div class="document-version-file__name">
                                <i class="fas fa-file-alt text-primary me-1"></i>
                                <%#: GetFileName(Eval("TenFileGoc"), Eval("TenFile")) %>
                            </div>
                            <div class="document-version-file__meta">
                                <%#: FormatFileSize(Eval("FileSize")) %>
                            </div>
                        </div>
                        <asp:HyperLink runat="server"
                            Visible='<%# CanOpenFile(Eval("FileUrl")) %>'
                            NavigateUrl='<%# GetFileUrl(Eval("FileUrl")) %>'
                            Text="Xem file"
                            Target="_blank"
                            CssClass="btn btn-sm btn-outline-primary" />
                        <asp:Label runat="server"
                            Visible='<%# HasValue(Eval("IdFile")) && !CanOpenFile(Eval("FileUrl")) %>'
                            Text="Không còn tệp vật lý"
                            CssClass="badge bg-warning text-dark" />
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlNoVersionFiles" CssClass="document-detail__empty">
            <i class="fas fa-file-circle-xmark"></i>
            Mốc lịch sử này không có file.
        </asp:Panel>
    </ContentTemplate>
</SweetSoft:ExtraModal>

<SweetSoft:ExtraModal
    runat="server"
    ID="mdlCustomerDelivery"
    Type="Primary"
    Size="Normal"
    FooterButtonClose="false"
    EnsureChildControlsOnPostback="true">
    <ContentTemplate>
        <asp:Panel runat="server" ID="pnlCustomerDeliveryForm" CssClass="validationEngineContainer">
            <asp:HiddenField runat="server" ID="hdfCustomerDeliveryDocumentId" />
            <asp:HiddenField runat="server" ID="hdfCustomerDeliveryVersion" />
            <asp:HiddenField runat="server" ID="hdfCustomerDeliveryCustomer" />
            <asp:HiddenField runat="server" ID="hdfCustomerDeliveryChannel" />
            <asp:HiddenField runat="server" ID="hdfCustomerDeliverySubmissionToken" />
            <div class="alert alert-light border py-2 small">
                <i class="fas fa-info-circle me-1"></i>
                <%= GetResourceText(BackEndResourceKeys.CUSTOMER_DELIVERY_RECORD_NOTICE) %>
            </div>
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CUSTOMER_DELIVERY_VERSION) %></label>
                    <SweetSoft:ExtraDropdown
                        runat="server"
                        ID="ddlCustomerDeliveryVersion"
                        ValueIsOfTypeGUID="true"
                        SimpleInit="true" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CUSTOMER) %></label>
                    <SweetSoft:ExtraDropdown
                        runat="server"
                        ID="ddlCustomerDeliveryCustomer"
                        ValueIsOfTypeGUID="true"
                        SimpleInit="true" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.RECIPIENT) %></label>
                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtCustomerDeliveryRecipient"
                        MaxLength="150" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.TO_EMAIL) %></label>
                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtCustomerDeliveryEmail"
                        MaxLength="256" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CHANNEL) %></label>
                    <SweetSoft:ExtraDropdown
                        runat="server"
                        ID="ddlCustomerDeliveryChannel"
                        SimpleInit="true" />
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.RESPONSE_DEADLINE) %></label>
                    <SweetSoft:ExtraDateTime
                        runat="server"
                        ID="dtCustomerDeliveryDeadline"
                        SingleDatePicker="true"
                        AllowNullDate="true"
                        DateFormat="DD/MM/YYYY"
                        Opens="Left"
                        Drops="Down" />
                </div>
                <div class="col-12 mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.ALLOW_SEND_BEFORE_SIGNING) %></label>
                    <div class="mt-2">
                        <SweetSoft:ExtraCheckbox
                            runat="server"
                            ID="chkCustomerDeliveryBeforeSigning" />
                    </div>
                    <div class="form-text text-warning">
                        <i class="fas fa-exclamation-triangle me-1"></i>
                        <%= GetResourceText(BackEndResourceKeys.SEND_BEFORE_SIGNING_NOTICE) %>
                    </div>
                </div>
                <div class="col-12 mb-1">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.NOTE) %></label>
                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtCustomerDeliveryNote"
                        TextMode="MultiLine"
                        Rows="3"
                        MaxLength="500" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
    <FooterTemplate>
        <div class="d-flex gap-2">
            <asp:Button
                runat="server"
                ID="btnCustomerDeliverySend"
                OnClick="btnCustomerDeliverySend_Click"
                OnClientClick="if (this.getAttribute('data-submitting') === 'true') { return false; } this.setAttribute('data-submitting', 'true'); var submitButton = this; window.setTimeout(function () { submitButton.disabled = true; }, 0); CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-primary btn-sm waves-effect waves-light" />
            <asp:Button
                runat="server"
                ID="btnCancelCustomerDelivery"
                OnClick="btnCancelCustomerDelivery_Click"
                OnClientClick="CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-outline-secondary btn-sm waves-effect waves-light" />
        </div>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<SweetSoft:ExtraModal
    runat="server"
    ID="mdlPhysicalStorage"
    Type="Primary"
    Size="Normal"
    FooterButtonClose="false"
    EnsureChildControlsOnPostback="true">
    <ContentTemplate>
        <asp:Panel runat="server" ID="pnlPhysicalStorageForm" CssClass="validationEngineContainer">
            <asp:HiddenField runat="server" ID="hdfPhysicalStorageDocumentId" />
            <asp:HiddenField runat="server" ID="hdfPhysicalStorageLocation" />
            <div class="alert alert-light border py-2 small">
                <i class="fas fa-info-circle me-1"></i>
                <%= GetResourceText(BackEndResourceKeys.PHYSICAL_STORAGE_CODE_NOTICE) %>
            </div>
            <div class="row">
                <div class="col-12 mb-3">
                    <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.STORAGE_LOCATION) %></label>
                    <SweetSoft:ExtraDropdown
                        runat="server"
                        ID="ddlPhysicalStorageLocation"
                        ValueIsOfTypeGUID="true"
                        SimpleInit="true"
                        MinimumResultsForSearch="0"
                        DropdownCssClass="document-storage-dropdown" />
                    <asp:Panel
                        runat="server"
                        ID="pnlPhysicalStorageLocationPath"
                        CssClass="mt-2 px-3 py-2 rounded border bg-light small text-muted"
                        style="display: none;">
                        <i class="fas fa-map-marker-alt text-primary me-2"></i>
                        <asp:Label
                            runat="server"
                            ID="lblPhysicalStorageLocationPath" />
                    </asp:Panel>
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.USE_MANUAL_STORAGE_CODE) %></label>
                    <div class="mt-2">
                        <SweetSoft:ExtraCheckbox
                            runat="server"
                            ID="chkPhysicalStorageManualCode" />
                    </div>
                </div>
                <div class="col-md-6 mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.PHYSICAL_STORAGE_CODE) %></label>
                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtPhysicalStorageCode"
                        MaxLength="100" />
                </div>
                <div class="col-12 mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.ORIGINAL_COPY_CONDITION) %></label>
                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtPhysicalStorageOriginalCondition"
                        MaxLength="255" />
                </div>
                <div class="col-12 mb-1">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.NOTE) %></label>
                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtPhysicalStorageNote"
                        TextMode="MultiLine"
                        Rows="3"
                        MaxLength="500" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
    <FooterTemplate>
        <div class="d-flex gap-2">
            <asp:Button
                runat="server"
                ID="btnPhysicalStorageSave"
                OnClick="btnPhysicalStorageSave_Click"
                OnClientClick="CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-primary btn-sm waves-effect waves-light" />
            <asp:Button
                runat="server"
                ID="btnCancelPhysicalStorage"
                OnClick="btnCancelPhysicalStorage_Click"
                OnClientClick="CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-outline-secondary btn-sm waves-effect waves-light" />
        </div>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<style type="text/css">
    .document-storage-dropdown .select2-results__option {
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }
</style>

<SweetSoft:ExtraModal
    runat="server"
    ID="mdlCustomerDeliveryStatus"
    Type="Primary"
    Size="Normal"
    FooterButtonClose="false"
    EnsureChildControlsOnPostback="true">
    <ContentTemplate>
        <asp:Panel runat="server" ID="pnlCustomerDeliveryStatusForm" CssClass="validationEngineContainer">
            <asp:HiddenField runat="server" ID="hdfCustomerDeliveryStatusDocumentId" />
            <asp:HiddenField runat="server" ID="hdfCustomerDeliveryStatusId" />
            <asp:HiddenField runat="server" ID="hdfCustomerDeliveryStatus" />
            <div class="alert alert-light border py-2">
                <div class="small text-muted"><%= GetResourceText(BackEndResourceKeys.CUSTOMER_DELIVERY_VERSION) %></div>
                <asp:Label runat="server" ID="lblCustomerDeliveryStatusVersion" CssClass="fw-semibold" />
                <span class="mx-1">·</span>
                <asp:Label runat="server" ID="lblCustomerDeliveryStatusCustomer" CssClass="fw-semibold" />
            </div>
            <div class="mb-3">
                <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.CUSTOMER_SEND_STATUS) %></label>
                <SweetSoft:ExtraDropdown
                    runat="server"
                    ID="ddlCustomerDeliveryStatus"
                    SimpleInit="true" />
            </div>
            <div class="mb-1">
                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.NOTE) %></label>
                <SweetSoft:ExtraTextBox
                    runat="server"
                    ID="txtCustomerDeliveryStatusNote"
                    TextMode="MultiLine"
                    Rows="3"
                    MaxLength="500" />
            </div>
        </asp:Panel>
    </ContentTemplate>
    <FooterTemplate>
        <div class="d-flex gap-2">
            <asp:Button
                runat="server"
                ID="btnUpdateCustomerDeliveryStatus"
                OnClick="btnUpdateCustomerDeliveryStatus_Click"
                OnClientClick="CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-primary btn-sm waves-effect waves-light" />
            <asp:Button
                runat="server"
                ID="btnCancelCustomerDeliveryStatus"
                OnClick="btnCancelCustomerDeliveryStatus_Click"
                OnClientClick="CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-outline-secondary btn-sm waves-effect waves-light" />
        </div>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<SweetSoft:ExtraModal
    runat="server"
    ID="mdlSigningResult"
    Type="Primary"
    Size="Large"
    FooterButtonClose="false"
    EnsureChildControlsOnPostback="true">
    <ContentTemplate>
        <asp:HiddenField runat="server" ID="hdfSigningResultId" />
        <div class="small text-muted mb-2"><%= GetResourceText(BackEndResourceKeys.SIGNING_RESULT_FILE_HINT) %></div>
        <div class="document-file-box">
            <SweetSoft:FilesBox
                runat="server"
                ID="fbSigningResult"
                IsMultiple="false" />
        </div>
        <div class="mt-3">
            <label class="form-label"><%= GetResourceText(BackEndResourceKeys.SIGNING_NOTE) %></label>
            <SweetSoft:ExtraTextBox
                runat="server"
                ID="txtSigningResultNote"
                TextMode="MultiLine"
                Rows="2"
                MaxLength="500" />
        </div>
    </ContentTemplate>
    <FooterTemplate>
        <div class="d-flex gap-2">
            <asp:Button
                runat="server"
                ID="btnCompleteSigning"
                OnClick="btnCompleteSigning_Click"
                OnClientClick="CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-primary btn-sm waves-effect waves-light" />
            <asp:Button
                runat="server"
                ID="btnCancelSigningResult"
                OnClick="btnCancelSigningResult_Click"
                OnClientClick="CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-outline-secondary btn-sm waves-effect waves-light" />
        </div>
    </FooterTemplate>
</SweetSoft:ExtraModal>

<SweetSoft:ExtraModal
    runat="server"
    ID="mdlSigningChanges"
    Type="Primary"
    Size="Normal"
    FooterButtonClose="false"
    EnsureChildControlsOnPostback="true">
    <ContentTemplate>
        <asp:HiddenField runat="server" ID="hdfSigningChangesId" />
        <div class="mb-1">
            <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.REQUEST_CHANGES) %></label>
            <SweetSoft:ExtraTextBox
                runat="server"
                ID="txtSigningChangeReason"
                TextMode="MultiLine"
                Rows="4"
                MaxLength="500" />
        </div>
    </ContentTemplate>
    <FooterTemplate>
        <div class="d-flex gap-2">
            <asp:Button
                runat="server"
                ID="btnRequestSigningChanges"
                OnClick="btnRequestSigningChanges_Click"
                OnClientClick="CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-primary btn-sm waves-effect waves-light" />
            <asp:Button
                runat="server"
                ID="btnCancelSigningChanges"
                OnClick="btnCancelSigningChanges_Click"
                OnClientClick="CMSMasterJs.DisableContentChanged();"
                CausesValidation="false"
                UseSubmitBehavior="true"
                CssClass="btn btn-outline-secondary btn-sm waves-effect waves-light" />
        </div>
    </FooterTemplate>
</SweetSoft:ExtraModal>
