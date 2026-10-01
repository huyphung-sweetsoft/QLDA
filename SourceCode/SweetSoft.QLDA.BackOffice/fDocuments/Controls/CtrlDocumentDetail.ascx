<%@ Control Language="C#"
    AutoEventWireup="true"
    CodeBehind="CtrlDocumentDetail.ascx.cs"
    Inherits="SweetSoft.QLDA.BackOffice.fDocuments.Controls.CtrlDocumentDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fDocuments/Controls/CtrlDocuments.ascx" TagPrefix="SweetSoft" TagName="DocumentEditor" %>
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

    .document-detail__actions {
        align-items: center;
        display: flex;
        flex: 0 0 auto;
        flex-wrap: wrap;
        gap: .5rem;
        justify-content: flex-end;
    }

    .document-detail__identity {
        align-items: center;
        display: flex;
        gap: .75rem;
        min-width: 0;
    }

    .document-detail__identity > div:last-child { min-width: 0; }

    .document-detail__icon {
        align-items: center;
        background: var(--document-purple-soft);
        border-radius: 8px;
        color: var(--document-purple);
        display: flex;
        flex: 0 0 40px;
        font-size: 1.1rem;
        height: 40px;
        justify-content: center;
    }

    .document-detail__title {
        color: #1f2937;
        font-size: 1.15rem;
        font-weight: 600;
        line-height: 1.4;
        margin: 0 0 .35rem;
        overflow-wrap: anywhere;
    }

    .document-detail__meta {
        align-items: center;
        display: flex;
        flex-wrap: wrap;
        gap: .4rem .75rem;
    }

    .document-detail__scope {
        background: var(--document-purple-soft);
        border-radius: 4px;
        color: var(--document-purple);
        display: inline-flex;
        align-items: center;
        gap: .35rem;
        font-size: .75rem;
        font-weight: 600;
        padding: .2rem .5rem;
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

    .document-delivery { width: 100%; min-width: 0; }
    .document-delivery .document-delivery__table { width: 100%; min-width: 0; table-layout: fixed; }
    .document-delivery .document-delivery__table th,
    .document-delivery .document-delivery__table td {
        white-space: normal; overflow-wrap: anywhere; word-break: normal;
        padding: .75rem; vertical-align: top; border-color: #e5e7eb;
    }
    .document-delivery__files { list-style: none; padding: 0; margin: 0; }
    .document-delivery__files li + li { border-top: 1px solid #eef0f4; margin-top: .55rem; padding-top: .55rem; }
    .document-delivery__meta { display: block; font-size: .8rem; color: #667085; margin-top: .25rem; line-height: 1.5; }
    .document-delivery__table .badge { white-space: normal; }
    @media (max-width: 991.98px) {
        .document-delivery__table colgroup, .document-delivery__table thead { display: none; }
        .document-delivery .document-delivery__table,
        .document-delivery__table tbody, .document-delivery__table tr { display: block; width: 100%; }
        .document-delivery__table tr + tr { margin-top: 1rem; }
        .document-delivery .document-delivery__table td { display: block; width: 100%; box-sizing: border-box; }
        .document-delivery__table td:before { content: attr(data-label); display: block; color: #4d0f91; font-weight: 600; font-size: .8rem; margin-bottom: .4rem; }
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
            margin-top: 0;
            width: auto;
        }

        .document-detail__actions {
            justify-content: flex-start;
            margin-top: 1rem;
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

    <div class="document-detail__header">
        <div class="document-detail__identity">
            <div class="document-detail__icon">
                <i class="fas fa-file-alt"></i>
            </div>
            <div>
                <h2 class="document-detail__title">
                    <asp:Label runat="server" ID="lblDocumentName" />
                </h2>
                <div class="document-detail__meta">
                    <span class="document-detail__scope">
                        <i class="<%= DocumentScopeIconCss %>"></i>
                        <%= DocumentScopeText %>
                    </span>
                    <span class="document-detail__code">
                        <%= GetResourceText(BackEndResourceKeys.DOCUMENT_CODE) %>:
                        <asp:Label runat="server" ID="lblDocumentCode" />
                    </span>
                    <asp:Label runat="server" ID="lblDocumentStatus" Visible="false" />
                </div>
            </div>
        </div>

        <div class="document-detail__actions">
            <asp:Button runat="server" ID="btnEditDocumentInfo" Text="Chỉnh sửa thông tin" Visible="false"
                CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnEditDocumentInfo_Click" />
            <asp:Button runat="server" ID="btnDocumentPermissions" Text="Cấp quyền" Visible="false"
                CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnDocumentPermissions_Click" />
            <SweetSoft:ExtraButton
                runat="server"
                ID="btnBack"
                NavigateUrl="/Documents"
                CssClass="btn-outline-secondary waves-effect"
                ButtonIcon="Reply"
                IsSubmit="false" />
        </div>
    </div>

    <div class="row g-3" runat="server" visible="false">
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

            <details class="mb-3">
                <summary class="text-primary py-2">Thông tin hồ sơ và nơi lưu trữ</summary>
            <div class="document-detail__section">
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
                    <div class="col-lg-4 col-md-6">
                        <div class="document-detail__field">
                            <span class="document-detail__field-label">Nơi lưu trữ</span>
                            <asp:Label runat="server" ID="lblWorkspaceStorage" CssClass="document-detail__field-value" />
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
    <div class="document-detail__section mb-3" runat="server" ID="pnlWorkspaceContent">
        <div class="document-detail__section-title">Nội dung hồ sơ</div>
        <div class="document-content-view"><asp:Literal runat="server" ID="litDocumentContent" /></div>
    </div>

    </details>
    <div class="document-detail__section">
        <div class="d-flex flex-wrap align-items-center justify-content-between gap-2 mb-3">
            <span class="document-detail__section-title mb-0">Các file hồ sơ</span>
            <div class="d-flex flex-wrap gap-2">
                <asp:Button runat="server" ID="btnWorkspaceAdd" Text="Thêm file" CssClass="btn btn-info" CausesValidation="false" OnClick="btnWorkspaceAdd_Click" />
                <asp:Button runat="server" ID="btnWorkspaceSign" Text="Trình ký" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnOpenSubmitSigning_Click" />
                <asp:Button runat="server" ID="btnWorkspaceSend" Text="Gửi khách" CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnOpenCustomerDelivery_Click" />
                <button type="button" class="btn btn-outline-secondary" data-bs-toggle="collapse" data-bs-target="#workspaceHistories">Lịch sử thao tác</button>
            </div>
        </div>
        <div class="d-flex flex-wrap gap-2 mb-3">
            <div style="width:220px;max-width:100%">
            <SweetSoft:ExtraDropdown runat="server" ID="ddlWorkspaceStatus" PlaceHolder="Tất cả trạng thái" EmptyItemValue="-1" SimpleInit="true" AutoPostBack="true" OnSelectedIndexChanged="WorkspaceFilterChanged">
                <asp:ListItem Value="ALL" Text="Tất cả trạng thái" />
                <asp:ListItem Value="CHUA_TRINH" Text="Chưa trình ký" />
                <asp:ListItem Value="DANG_TRINH" Text="Chờ ký" />
                <asp:ListItem Value="YEU_CAU_DIEU_CHINH" Text="Yêu cầu chỉnh sửa" />
                <asp:ListItem Value="DA_KY" Text="Đã ký" />
                <asp:ListItem Value="THU_HOI" Text="Đã thu hồi yêu cầu" />
            </SweetSoft:ExtraDropdown>
            </div>
            <div class="input-group" style="max-width:420px">
                <asp:TextBox runat="server" ID="txtWorkspaceSearch" CssClass="form-control" placeholder="Tìm tên file..." />
                <asp:Button runat="server" ID="btnWorkspaceFilter" Text="Tìm kiếm" CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="WorkspaceFilterChanged" />
            </div>
        </div>
        <asp:GridView runat="server" ID="grdWorkspace" AutoGenerateColumns="false" DataKeyNames="IdFile,IdChuoiFile"
            GridLines="None" CssClass="table table-bordered table-hover w-100" AllowPaging="true" PageSize="10"
            OnPageIndexChanging="grdWorkspace_PageIndexChanging" OnRowCommand="grdWorkspace_RowCommand"
            EmptyDataText="Chưa có file phù hợp." ShowHeaderWhenEmpty="true" PagerStyle-CssClass="text-center">
            <Columns>
                <asp:TemplateField HeaderStyle-Width="40px">
                    <HeaderTemplate><input type="checkbox" aria-label="Chọn các file trên trang" onclick="var selected=this.checked;this.closest('table').querySelectorAll('tbody input[type=checkbox]').forEach(function(c){if(!c.disabled)c.checked=selected;});" /></HeaderTemplate>
                    <ItemTemplate><asp:CheckBox runat="server" ID="chkWorkspaceFile" data-workspace-select="true" /></ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Tên file">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" CommandName="FILE_DETAIL" CommandArgument='<%# Eval("IdFile") %>'
                            Text='<%#: Eval("TenFile") %>' CausesValidation="false" CssClass="text-primary"
                            style="overflow-wrap:anywhere" />
                        <small class="text-muted d-block"><%#: FormatFileSize(Eval("FileSize")) %></small>
                        <asp:HyperLink runat="server" Text="Bản đã ký" Target="_blank" CssClass="text-success d-inline-block mt-1"
                            NavigateUrl='<%# GetFileUrl(Eval("SignedFileUrl")) %>' Visible='<%# HasValue(Eval("SignedFileUrl")) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="FileVersion" HeaderText="Phiên bản" HeaderStyle-Width="95px" ItemStyle-CssClass="text-center" />
                <asp:TemplateField HeaderText="Trạng thái ký" HeaderStyle-Width="170px">
                    <ItemTemplate><span class='<%# GetSigningStatusCss(Eval("TrangThai")) %>'><%#: WorkspaceStatusText(Eval("TrangThai")) %></span></ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Cập nhật" HeaderStyle-Width="170px"><ItemTemplate><%#: FormatDate(Eval("NgayCapNhat")) %></ItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Thao tác" HeaderStyle-Width="90px" ItemStyle-CssClass="text-center">
                    <ItemTemplate><asp:LinkButton runat="server" CommandName="FILE_DETAIL" CommandArgument='<%# Eval("IdFile") %>' CausesValidation="false" CssClass="btn btn-sm btn-outline-success" ToolTip="Chi tiết và lịch sử"><i class="fas fa-eye"></i></asp:LinkButton></ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:Label runat="server" ID="lblWorkspaceCount" CssClass="text-muted small" />
    </div>

    <ul class="nav nav-pills document-detail__tabs d-none" role="tablist">
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

    <div class="collapse" id="workspaceHistories">
        <div class="mb-3" id="document-versions" role="tabpanel">
            <div class="document-detail__section">
                <div class="document-detail__section-title">Lịch sử thay đổi bộ file</div>
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
                            <article class="document-file-history__item" style='<%# Container.ItemIndex >= 5 ? "display:none" : "" %>'>
                                <div class="document-file-history__dot"></div>
                                <div class="document-file-history__card">
                                    <div class="document-file-history__head">
                                        <div>
                                            <div class="document-file-history__version">
                                                <%#: FormatDate(Eval("NgayTao")) %>
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
                                                Text="Khôi phục cả bộ"
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
                        <FooterTemplate>
                            <div class="text-center mt-2" runat="server" Visible='<%# rptVersions.Items.Count > 5 %>'>
                                <button type="button" class="btn btn-sm btn-outline-primary"
                                    onclick="var items = this.closest('.document-file-history').querySelectorAll('article.document-file-history__item'); var shown = 0; var remaining = 0; for (var i = 0; i &lt; items.length; i++) { if (items[i].style.display === 'none') { if (shown &lt; 5) { items[i].style.display = ''; shown++; } else { remaining++; } } } if (!remaining) this.parentElement.style.display = 'none';">
                                    Xem thêm 5 lần thay đổi
                                </button>
                            </div>
                        </FooterTemplate>
                    </asp:Repeater>
                </asp:Panel>
            </div>
        </div>

        <asp:PlaceHolder runat="server" ID="phSigningPane">
            <div class="mb-3" id="document-signing" role="tabpanel">
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
                                <th>File trình ký</th>
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
                                <td><asp:HyperLink runat="server"
                                    Visible='<%# HasValue(Eval("IdFileNguon")) && CanOpenFile(Eval("FileNguonUrl")) %>'
                                    NavigateUrl='<%# GetFileUrl(Eval("FileNguonUrl")) %>'
                                    Text='<%# GetValueText(Eval("TenFileNguonGoc")) %>'
                                    Target="_blank" CssClass="text-decoration-none" />
                                    <asp:Label runat="server"
                                        Visible='<%# !HasValue(Eval("IdFileNguon")) %>'
                                        Text="File nguồn không còn khả dụng" CssClass="text-muted" />
                                </td>
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
                                        Visible='<%# CanManagePendingSigning(Eval("TrangThaiTrinhKy"), Eval("IdNguoiKy"), Eval("IdTrinhKyTaiLieuFile")) %>'
                                        CommandName="CONFIRM_SIGNED"
                                        CommandArgument='<%# Eval("IdTrinhKyTaiLieuFile") %>'
                                        Text='<%# GetResourceText(BackEndResourceKeys.CONFIRM_SIGNED) %>'
                                        CausesValidation="false"
                                        CssClass="btn btn-sm btn-outline-success" />
                                    <asp:LinkButton runat="server"
                                        Visible='<%# CanManagePendingSigning(Eval("TrangThaiTrinhKy"), Eval("IdNguoiKy"), Eval("IdTrinhKyTaiLieuFile")) %>'
                                        CommandName="REQUEST_CHANGES"
                                        CommandArgument='<%# Eval("IdTrinhKyTaiLieuFile") %>'
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
            <div class="mb-3" id="document-customer" role="tabpanel">
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
                    <asp:Panel runat="server" ID="pnlCustomer" CssClass="document-delivery">
                        <table class="table table-bordered table-hover document-detail__table document-delivery__table">
                            <colgroup><col style="width:22%" /><col style="width:28%" /><col style="width:20%" /><col style="width:20%" /><col style="width:10%" /></colgroup>
                            <thead><tr>
                                <th>File đã gửi</th>
                                <th>Khách hàng / Người nhận</th>
                                <th>Thông tin gửi</th>
                                <th><%= GetResourceText(BackEndResourceKeys.STATUS) %></th>
                                <th><%= GetResourceText(BackEndResourceKeys.ACTION) %></th>
                            </tr></thead>
                            <tbody><asp:Repeater
                                runat="server"
                                ID="rptCustomer"
                                OnItemCommand="rptCustomer_ItemCommand"><ItemTemplate><tr>
                                <td data-label="File đã gửi">
                                    <asp:LinkButton runat="server" CommandName="VIEW_DELIVERY_FILES"
                                        CommandArgument='<%# Eval("IdGuiNhanKhachHang") %>' CausesValidation="false"
                                        CssClass="btn btn-sm btn-outline-primary text-wrap text-start"
                                        Text='<%# DeliveryFilesSummary(Eval("DanhSachFileGuiJson")) %>' />
                                </td>
                                <td data-label="Khách hàng / Người nhận"><%#: GetValueText(Eval("TenKhachHang")) %>
                                    <span class="document-delivery__meta">Người nhận: <%#: GetValueText(Eval("TenNguoiNhan")) %></span>
                                    <span class="document-delivery__meta"><%#: Convert.ToString(Eval("EmailNguoiNhan")) %></span>
                                </td>
                                <td data-label="Thông tin gửi"><%#: FormatDate(Eval("NgayGui")) %>
                                    <span class="document-delivery__meta">Kênh: <%#: GetCustomerDeliveryChannelText(Eval("KenhGui")) %></span>
                                    <span class="document-delivery__meta">Người gửi: <%#: GetValueText(Eval("TenNguoiThucHien")) %></span>
                                </td>
                                <td data-label="Trạng thái"><span class='<%# GetCustomerStatusCss(Eval("TrangThai")) %>'><%#: GetCustomerStatusText(true, Eval("TrangThai")) %></span>
                                    <span class="document-delivery__meta">Hạn phản hồi: <%#: FormatDate(Eval("HanPhanHoi")) %></span>
                                    <span class="document-delivery__meta">Nhận lại: <%#: FormatDate(Eval("NgayNhanLai")) %></span>
                                </td>
                                <td data-label="Hành động"><div class="d-flex flex-wrap gap-1">
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
            <div class="mb-3" id="document-storage" role="tabpanel">
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

        <asp:UpdatePanel runat="server" ID="upActivity" UpdateMode="Conditional" ChildrenAsTriggers="false">
        <ContentTemplate>
        <div class="mb-3" id="document-activity" role="tabpanel">
            <style>
                #document-activity .document-activity-table { width:100%; table-layout:fixed; margin-bottom:0; }
                #document-activity .document-activity-table th,
                #document-activity .document-activity-table td { white-space:normal; overflow-wrap:anywhere; vertical-align:middle; }
            </style>
            <div class="document-detail__section">
                <div class="document-detail__section-title"><%= GetResourceText(BackEndResourceKeys.DOCUMENT_ACTIVITY_HISTORY) %></div>
                <asp:Panel runat="server" ID="pnlActivityContent">
                <div class="d-flex justify-content-end mb-2">
                    <asp:LinkButton runat="server" ID="btnRefreshActivity" Text="Làm mới" CssClass="btn btn-sm btn-outline-secondary" CausesValidation="false" OnClick="btnRefreshActivity_Click" />
                </div>
                <asp:Panel runat="server" ID="pnlNoActivity" CssClass="document-detail__empty">
                    <i class="fas fa-history"></i>
                    <%= GetResourceText(BackEndResourceKeys.NO_DOCUMENT_ACTIVITY) %>
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlActivity" CssClass="table-responsive">
                    <table class="extra-gridview table table-bordered table-hover align-middle document-activity-table">
                        <colgroup><col style="width:16%" /><col style="width:15%" /><col style="width:17%" /><col style="width:40%" /><col style="width:12%" /></colgroup>
                        <thead class="text-center"><tr>
                            <th scope="col">Thời gian</th>
                            <th><%= GetResourceText(BackEndResourceKeys.ACTION_TYPE) %></th>
                            <th scope="col">Người thực hiện</th>
                            <th><%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %></th>
                            <th><%= GetResourceText(BackEndResourceKeys.REFERENCE_TYPE) %></th>
                        </tr></thead>
                        <tbody><asp:Repeater runat="server" ID="rptActivity"><ItemTemplate><tr>
                            <td class="text-center"><%#: FormatDate(Eval("NgayTao")) %></td>
                            <td><%#: GetActivityTypeText(Eval("LoaiHanhDong")) %></td>
                            <td><%#: GetActorText(Eval("TenNguoiThucHien"), Eval("NguoiTao")) %></td>
                            <td><%#: GetActivityDescription(Eval("MoTa"), Eval("NoiDungThayDoi")) %></td>
                            <td><%#: GetActivityReferenceText(Eval("LoaiThamChieu")) %></td>
                        </tr></ItemTemplate></asp:Repeater></tbody>
                    </table>
                </asp:Panel>
                <div class="text-center mt-2">
                    <asp:LinkButton runat="server" ID="btnMoreActivity" Text="Xem thêm 20 hoạt động" CssClass="btn btn-sm btn-outline-primary" CausesValidation="false" OnClientClick="window.documentActivityScrollY = window.scrollY;" OnClick="btnMoreActivity_Click" />
                </div>
                </asp:Panel>
            </div>
        </div>
        </ContentTemplate>
        </asp:UpdatePanel>
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
            <div class="border rounded p-2 mb-3">
                <div class="small text-muted">File trình ký</div>
                <asp:Label runat="server" ID="lblSubmitSigningVersion" CssClass="fw-semibold signing-selection-count" />
                <span class="mx-1">·</span>
                <asp:Label runat="server" ID="lblSubmitSigningMethod" CssClass="fw-semibold" />
            </div>
            <div class="mb-3">
                <label class="form-label label-valid">Chọn file cần trình ký</label>
                <asp:CheckBoxList
                    runat="server"
                    ID="cblSubmitSigningFiles"
                    RepeatDirection="Vertical"
                    RepeatLayout="UnorderedList"
                    onchange="this.closest('.validationEngineContainer').querySelector('.signing-selection-count').textContent = this.querySelectorAll('input[type=checkbox]:checked').length + ' file đã chọn';"
                    CssClass="document-signing-file-list" />
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

<SweetSoft:ExtraModal runat="server" ID="mdlDocumentPermissions" Title="Cấp quyền hồ sơ" Size="Large" Type="Primary"
    Position="modal-dialog-scrollable" FooterButtonClose="true"
    EnsureChildControlsOnPostback="true">
    <ContentTemplate>
        <style type="text/css">
            .document-permission-intro { background: var(--bs-light); border: 1px solid var(--bs-border-color); border-radius: 4px; padding: 10px 12px; color: #475467; }
            .document-permission-intro strong { color: #4c1d95; }
            .document-permission-toolbar { display:flex; align-items:center; justify-content:space-between; gap:12px; margin: 14px 0 10px; }
            .document-permission-toolbar__title { font-weight: 700; color:#344054; }
            .document-permission-toolbar__hint { color:#667085; font-size:.82rem; }
            .document-permission-list { max-height:55vh; overflow:auto; display:grid; gap:6px; padding-right:4px; }
            .document-permission-card { border:1px solid var(--bs-border-color); border-radius:4px; background:#fff; overflow:hidden; }
            .document-permission-card[open] { border-color:var(--bs-primary); }
            .document-permission-card__summary { cursor:pointer; list-style:none; display:flex; align-items:center; gap:10px; padding:8px 12px; background:var(--bs-light); }
            .document-permission-card__summary::-webkit-details-marker { display:none; }
            .document-permission-card__avatar { width:34px; height:34px; display:inline-flex; align-items:center; justify-content:center; border-radius:50%; background:#ede9fe; color:#5b21b6; font-weight:700; flex:0 0 auto; }
            .document-permission-card__name { font-weight:600; color:#344054; flex:1 1 auto; min-width:0; }
            .document-permission-card__badges { display:flex; flex-wrap:wrap; justify-content:flex-end; gap:4px; }
            .document-permission-card__chevron { color:#98a2b3; transition:transform .15s ease; }
            .document-permission-card[open] .document-permission-card__chevron { transform:rotate(180deg); }
            .document-permission-card__body { padding:0 14px 14px; border-top:1px solid #f0f2f5; }
            .document-permission-grid { display:grid; grid-template-columns:repeat(3,minmax(0,1fr)); gap:10px; padding-top:12px; }
            .document-permission-item { position:relative; border:1px solid #dce1e8; border-radius:6px; padding:12px; background:#fff; transition:border-color .15s,background-color .15s; }
            .document-permission-item .form-check { margin:0; padding:0; min-height:20px; }
            .document-permission-item .form-check > span { display:flex; align-items:center; gap:9px; }
            .document-permission-item input[type=checkbox] { appearance:auto; width:18px; height:18px; margin:0; flex:0 0 18px; accent-color:var(--bs-primary); cursor:pointer; }
            .document-permission-item label { margin:0; font-weight:600; font-size:.9rem; line-height:1.4; cursor:pointer; color:#344054; }
            .document-permission-item small { display:block; color:#667085; margin-left:27px; margin-top:5px; font-size:.78rem; line-height:1.45; }
            .document-permission-item:has(input:checked) { background:#f5f2fc; border-color:#b8a1df; }
            .document-permission-item:has(input:focus-visible) { outline:2px solid var(--bs-primary); outline-offset:2px; }
            .document-permission-item:has(input:not(:disabled)):hover { border-color:var(--bs-primary); }
            .document-permission-item:has(input:disabled) { background:#f8f9fa; border-color:#e5e7eb; }
            .document-permission-item input:disabled, .document-permission-item input:disabled + label { cursor:not-allowed; }
            .document-permission-item input:disabled + label { color:#8993a1; }
            .document-permission-item.is-danger label { color:#dc3545; }
            .document-permission-item.is-danger:has(input:checked) { background:#fff5f5; border-color:#edb4ba; }
            @media (max-width:767.98px) { .document-permission-grid { grid-template-columns:repeat(2,minmax(0,1fr)); } }
            @media (max-width:479.98px) { .document-permission-grid { grid-template-columns:1fr; } }
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
            Chọn thành viên trong dự án và tích các quyền cần cấp cho hồ sơ này.
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
            <div class="document-permission-toolbar__title"><i class="fas fa-users me-1"></i> Thành viên dự án</div>
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
                                    <small>Sửa tên, mã, loại, nội dung và nơi lưu trữ hồ sơ</small>
                                </div>
                                <div class="document-permission-item">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantManageFiles"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; Convert.ToBoolean(Eval("MaxManageFiles")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanManageFiles")) &amp;&amp; Convert.ToBoolean(Eval("MaxManageFiles")) %>' Text="Quản lý file" /></div>
                                    <small>Thêm, thay, gỡ file và khôi phục bản trước</small>
                                </div>
                                <div class="document-permission-item">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantSigning"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; Convert.ToBoolean(Eval("MaxSigning")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanSigning")) &amp;&amp; Convert.ToBoolean(Eval("MaxSigning")) %>' Text="Trình ký" /></div>
                                    <small>Gửi yêu cầu ký cho các file đã chọn</small>
                                </div>
                                <div class="document-permission-item">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantCustomerDelivery"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; Convert.ToBoolean(Eval("MaxCustomerDelivery")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanCustomerDelivery")) &amp;&amp; Convert.ToBoolean(Eval("MaxCustomerDelivery")) %>' Text="Gửi khách hàng" /></div>
                                    <small>Gửi hồ sơ và cập nhật phản hồi</small>
                                </div>
                                <div class="document-permission-item" runat="server" visible="false">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantPhysicalStorage"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; Convert.ToBoolean(Eval("MaxPhysicalStorage")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanPhysicalStorage")) &amp;&amp; Convert.ToBoolean(Eval("MaxPhysicalStorage")) %>' Text="Lưu bản cứng" /></div>
                                    <small>Ghi nhận nơi lưu trữ và mã lưu trữ</small>
                                </div>
                                <div class="document-permission-item is-danger">
                                    <div class="form-check"><asp:CheckBox runat="server" ID="grantDelete"
                                        Enabled='<%# Convert.ToBoolean(Eval("MaxView")) &amp;&amp; Convert.ToBoolean(Eval("MaxDelete")) %>'
                                        Checked='<%# Convert.ToBoolean(Eval("CanDelete")) &amp;&amp; Convert.ToBoolean(Eval("MaxDelete")) %>' Text="Xóa hồ sơ" /></div>
                                    <small>Gỡ hồ sơ khỏi danh sách sử dụng</small>
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

<SweetSoft:ExtraModal runat="server" ID="mdlVersionFiles" Title="File tại thời điểm thay đổi" Size="Large" Type="Primary"
    Position="modal-dialog-scrollable" FooterButtonClose="true"
    EnsureChildControlsOnPostback="true">
    <ContentTemplate>
        <asp:Label runat="server" ID="lblVersionFilesSummary" CssClass="text-muted d-block mb-3" />
        <asp:Panel runat="server" ID="pnlVersionFiles" CssClass="document-version-file-list">
            <asp:Repeater runat="server" ID="rptVersionFiles" OnItemCommand="rptVersionFiles_ItemCommand">
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
                        <div class="d-flex gap-2 flex-wrap">
                            <asp:LinkButton runat="server" Text="Khôi phục file này" CommandName="RESTORE_SNAPSHOT_FILE"
                                CommandArgument='<%# Eval("IdFile") %>' CausesValidation="false"
                                Visible='<%# CURRENT_PAGE.IsEdit && CanManageFiles() && HasValue(Eval("IdFile")) && CanOpenFile(Eval("FileUrl")) %>'
                                CssClass="btn btn-sm btn-outline-warning" />
                            <asp:HyperLink runat="server"
                                Visible='<%# CanOpenFile(Eval("FileUrl")) %>'
                                NavigateUrl='<%# GetFileUrl(Eval("FileUrl")) %>'
                                data-path='<%# GetFileUrl(Eval("FileUrl")) %>'
                                onclick="FilesBox.LayoutFilePopUp(this); return false;"
                                ToolTip="Xem trước" aria-label="Xem trước"
                                Text="<i class='fas fa-eye' aria-hidden='true'></i>"
                                CssClass="btn btn-sm btn-outline-success" />
                            <asp:HyperLink runat="server"
                                Visible='<%# CanOpenFile(Eval("FileUrl")) %>'
                                NavigateUrl='<%# GetWorkspaceDownloadUrl(Eval("FileUrl")) %>'
                                ToolTip="Tải về" aria-label="Tải về"
                                Text="<i class='fas fa-download' aria-hidden='true'></i>"
                                CssClass="btn btn-sm btn-outline-primary" />
                        </div>
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
            Không có file tại thời điểm này.
        </asp:Panel>
    </ContentTemplate>
</SweetSoft:ExtraModal>

<style type="text/css">
    .document-signing-file-list { list-style:none; padding:0; margin:0; max-height:280px; overflow-y:auto; border:1px solid var(--bs-border-color,#dee2e6); border-radius:4px; }
    .document-signing-file-list > li { display:flex; align-items:flex-start; gap:10px; padding:10px 12px; margin:0; border-bottom:1px solid var(--bs-border-color,#dee2e6); }
    .document-signing-file-list > li:last-child { border-bottom:0; }
    .document-signing-file-list > li:has(input:checked) { background:var(--bs-light,#f8f9fa); }
    .document-signing-file-list input[type=checkbox] { flex:0 0 17px; width:17px; height:17px; margin:3px 0 0; accent-color:var(--bs-primary); cursor:pointer; }
    .document-signing-file-list label { flex:1; min-width:0; margin:0; white-space:normal; overflow-wrap:anywhere; text-align:left; line-height:1.5; cursor:pointer; }
    .document-signing-file-list input:focus-visible { outline:2px solid var(--bs-primary); outline-offset:2px; }
</style>

<style>
    #<%= mdlCustomerDelivery.ClientID %> .modal-dialog { max-width:min(900px,calc(100vw - 24px)); height:calc(100vh - 32px); height:calc(100dvh - 32px); min-height:0; margin:16px auto; }
    #<%= mdlCustomerDelivery.ClientID %> .modal-dialog > div,
    #<%= mdlCustomerDelivery.ClientID %> .modal-content { display:flex; flex-direction:column; width:100%; max-width:100%; max-height:100%; min-width:0; min-height:0; }
    #<%= mdlCustomerDelivery.ClientID %> .modal-content { overflow:hidden; }
    #<%= mdlCustomerDelivery.ClientID %> .modal-header,
    #<%= mdlCustomerDelivery.ClientID %> .modal-footer { flex:0 0 auto; }
    #<%= mdlCustomerDelivery.ClientID %> .modal-body { min-width:0; min-height:0; max-width:100%; overflow-x:hidden; overflow-y:auto; }
    #<%= mdlCustomerDelivery.ClientID %> .modal-body > div,
    #<%= mdlCustomerDelivery.ClientID %> .validationEngineContainer { min-width:0; max-width:100%; }
    #<%= mdlCustomerDelivery.ClientID %> .validationEngineContainer > .row { margin-left:0; margin-right:0; }
    #<%= mdlCustomerDelivery.ClientID %> .row > div { min-width:0; }
    #<%= mdlCustomerDelivery.ClientID %> .modal-header .btn-close { transform:none; flex-shrink:0; }
    #<%= mdlCustomerDelivery.ClientID %> .mb-3 { margin-bottom:.65rem!important; }
    #<%= mdlCustomerDelivery.ClientID %> .delivery-files { width:100%; }
    #<%= mdlCustomerDelivery.ClientID %> .delivery-file-list { list-style:none; padding:0; margin:0; max-height:170px; overflow-y:auto; border:1px solid var(--bs-border-color,#dee2e6); border-radius:4px; }
    #<%= mdlCustomerDelivery.ClientID %> .delivery-file-list li { display:flex; align-items:flex-start; gap:.6rem; padding:.5rem .75rem; border-bottom:1px solid #eee; }
    #<%= mdlCustomerDelivery.ClientID %> .delivery-file-list li:last-child { border-bottom:0; }
    #<%= mdlCustomerDelivery.ClientID %> .delivery-file-list input { flex-shrink:0; margin-top:.3rem; width:16px; height:16px; }
    #<%= mdlCustomerDelivery.ClientID %> .delivery-file-list label { flex:1; margin:0; min-width:0; white-space:normal; overflow-wrap:anywhere; cursor:pointer; }
</style>
<SweetSoft:DocumentEditor runat="server" ID="documentInfoEditor" EditOnly="true" />
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
            <div class="border rounded p-2 mb-3 small">
                <i class="fas fa-info-circle me-1"></i>
                <%= GetResourceText(BackEndResourceKeys.CUSTOMER_DELIVERY_RECORD_NOTICE) %>
            </div>
            <div class="row">
                <div class="col-12 mb-3 delivery-files">
                    <label class="form-label">File chuẩn bị gửi — bỏ tích file không muốn gửi</label>
                    <asp:Literal runat="server" ID="litWorkspaceDeliveryFiles" Visible="false" />
                    <asp:CheckBoxList runat="server" ID="cblCustomerDeliveryFiles" RepeatLayout="UnorderedList" RepeatDirection="Vertical" CssClass="delivery-file-list" />
                    <SweetSoft:ExtraDropdown
                        runat="server"
                        ID="ddlCustomerDeliveryVersion" Visible="false"
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
                <div class="col-md-6 mb-3">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.ALLOW_SEND_BEFORE_SIGNING) %></label>
                    <div class="mt-2">
                        <SweetSoft:ExtraCheckbox
                            runat="server"
                            ID="chkCustomerDeliveryBeforeSigning" />
                    </div>
                </div>
                <div class="col-12 mb-1">
                    <label class="form-label"><%= GetResourceText(BackEndResourceKeys.NOTE) %></label>
                    <SweetSoft:ExtraTextBox
                        runat="server"
                        ID="txtCustomerDeliveryNote"
                        TextMode="MultiLine"
                        Rows="2"
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
            <div class="border rounded p-2 mb-3 small">
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
            <div class="border rounded p-2 mb-3">
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

<style>
    /* ExtraModal's UpdatePanel sits between dialog and content; constrain it too. */
    #<%= mdlWorkspaceFile.ClientID %> .modal-dialog {
        height: calc(100vh - 32px);
        height: calc(100dvh - 32px);
        min-height: 0;
        margin: 16px auto;
        max-width: min(1000px, calc(100vw - 24px));
    }
    #<%= mdlWorkspaceFile.ClientID %> .modal-dialog > div {
        display: flex; flex-direction: column; width: 100%;
        max-height: 100%; min-height: 0;
    }
    #<%= mdlWorkspaceFile.ClientID %> .modal-content {
        display: flex; flex-direction: column;
        max-height: 100%; min-height: 0; overflow: hidden;
    }
    #<%= mdlWorkspaceFile.ClientID %> .modal-header,
    #<%= mdlWorkspaceFile.ClientID %> .modal-footer { flex: 0 0 auto; }
    #<%= mdlWorkspaceFile.ClientID %> .modal-body {
        flex: 1 1 auto; min-height: 0; overflow-y: auto;
        overflow-wrap: anywhere;
    }
    #<%= mdlWorkspaceFile.ClientID %> .modal-body th,
    #<%= mdlWorkspaceFile.ClientID %> .modal-body td { white-space: normal; }
</style>
<SweetSoft:ExtraModal runat="server" ID="mdlWorkspaceFile" Title="Chi tiết file" Size="Large" Type="Primary"
    Position="modal-dialog-centered modal-dialog-scrollable" EnsureChildControlsOnPostback="true">
    <ContentTemplate>
        <asp:Label runat="server" ID="lblWorkspaceFileName" CssClass="fw-semibold d-block mb-2" />
        <div class="d-flex flex-wrap gap-2 mb-3">
            <asp:HyperLink runat="server" ID="lnkWorkspaceView" Text="Xem bản hiện tại" onclick="FilesBox.LayoutFilePopUp(this); return false;" CssClass="btn btn-outline-primary" />
            <asp:HyperLink runat="server" ID="lnkWorkspaceDownload" Text="Tải về" CssClass="btn btn-outline-primary" />
            <asp:HyperLink runat="server" ID="lnkWorkspaceSigned" Text="Xem bản đã ký" onclick="FilesBox.LayoutFilePopUp(this); return false;" CssClass="btn btn-outline-success" />
            <asp:HyperLink runat="server" ID="lnkWorkspaceSignedDownload" Text="Tải bản đã ký" CssClass="btn btn-outline-success" />
            <asp:Button runat="server" ID="btnWorkspaceReplace" Text="Tải bản mới" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnWorkspaceReplace_Click" />
            <asp:Button runat="server" ID="btnWorkspaceRemove" Text="Gỡ file" CssClass="btn btn-outline-danger" CausesValidation="false" UseSubmitBehavior="false" OnClick="btnWorkspaceRemove_Click" />
        </div>
        <asp:Label runat="server" ID="lblWorkspaceLocked" Text="File đã ký được khóa để giữ nguyên nội dung và lịch sử." CssClass="border border-success rounded text-success p-3 mb-3 d-block" />
        <asp:CheckBox runat="server" ID="chkWorkspaceRecall" Text="Thu hồi yêu cầu ký đang chờ của file này khi thay đổi hoặc khôi phục." CssClass="d-block mb-3" />
        <asp:Panel runat="server" ID="pnlWorkspaceConfirm" Visible="false" CssClass="border border-warning rounded p-3 mb-3">
            <asp:Label runat="server" ID="lblWorkspaceConfirm" CssClass="d-block mb-2" />
            <asp:LinkButton runat="server" ID="btnWorkspaceConfirm" Text="Xác nhận" CssClass="btn btn-warning btn-sm" CausesValidation="false" OnClick="btnWorkspaceConfirm_Click" />
            <asp:LinkButton runat="server" ID="btnWorkspaceCancelConfirm" Text="Hủy" CssClass="btn btn-outline-secondary btn-sm" CausesValidation="false" OnClick="btnWorkspaceCancelConfirm_Click" />
        </asp:Panel>
        <h6>Lịch sử phiên bản</h6>
        <asp:GridView runat="server" ID="grdFileTimeline" AutoGenerateColumns="false" GridLines="None"
            CssClass="table table-bordered table-hover" OnRowCommand="grdFileTimeline_RowCommand">
            <Columns>
                <asp:BoundField DataField="FileVersion" HeaderText="Phiên bản" />
                <asp:BoundField DataField="HanhDong" HeaderText="Thao tác" />
                <asp:BoundField DataField="NguoiThucHien" HeaderText="Người thực hiện" />
                <asp:TemplateField HeaderText="Thời gian"><ItemTemplate><%#: FormatDate(Eval("NgayTao")) %></ItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="File">
                    <ItemTemplate>
                        <asp:HyperLink runat="server" NavigateUrl='<%# GetFileUrl(Eval("FileUrl")) %>' data-path='<%# GetFileUrl(Eval("FileUrl")) %>' onclick="FilesBox.LayoutFilePopUp(this); return false;" Text="Xem" CssClass="btn btn-sm btn-outline-primary" Visible='<%# HasValue(Eval("FileUrl")) %>' />
                        <asp:HyperLink runat="server" NavigateUrl='<%# GetWorkspaceDownloadUrl(Eval("FileUrl")) %>' Text="Tải về" CssClass="btn btn-sm btn-outline-primary" Visible='<%# HasValue(Eval("FileUrl")) %>' />
                        <asp:LinkButton runat="server" Text="Khôi phục" CommandName="RESTORE_FILE" CommandArgument='<%# Eval("IdFile") %>'
                            CausesValidation="false" CssClass="btn btn-sm btn-outline-warning"
                            Visible='<%# WorkspaceCanRestore(Eval("IdFile")) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:Panel runat="server" ID="pnlWorkspaceSigningHistory" Visible="false" CssClass="mt-3">
            <h6>Thông tin trình ký của file</h6>
            <asp:Repeater runat="server" ID="rptWorkspaceSigningHistory"><ItemTemplate>
                <div class="border-top py-2">
                    <span class='<%# GetSigningStatusCss(Eval("TrangThaiTrinhKy")) %>'><%#: WorkspaceStatusText(Eval("TrangThaiTrinhKy")) %></span>
                    <span class="ms-2"><%#: GetValueText(Eval("TenNguoiKyHienThi")) %></span>
                    <div class="small text-muted">Gửi lúc <%#: FormatDate(Eval("NgayGui")) %> · Xử lý lúc <%#: FormatDate(Eval("NgayNhanLai")) %></div>
                    <div class="small text-break"><%#: GetValueText(Eval("GhiChu")) %></div>
                    <asp:HyperLink runat="server" Text="Xem bản đã ký" data-path='<%# GetFileUrl(Eval("FileSauKyUrl")) %>' onclick="FilesBox.LayoutFilePopUp(this); return false;" CssClass="text-success me-2"
                        NavigateUrl='<%# GetFileUrl(Eval("FileSauKyUrl")) %>' Visible='<%# HasValue(Eval("FileSauKyUrl")) %>' />
                    <asp:HyperLink runat="server" Text="Tải bản đã ký" CssClass="text-success"
                        NavigateUrl='<%# GetWorkspaceDownloadUrl(Eval("FileSauKyUrl")) %>' Visible='<%# HasValue(Eval("FileSauKyUrl")) %>' />
                </div>
            </ItemTemplate></asp:Repeater>
        </asp:Panel>
    </ContentTemplate>
</SweetSoft:ExtraModal>
<style>
    #<%= mdlDeliveryFiles.ClientID %> .modal-dialog,
    #<%= mdlSnapshotRestore.ClientID %> .modal-dialog {
        width: calc(100% - 2rem); max-width: 900px;
        height: calc(100vh - 2rem); height: calc(100dvh - 2rem);
        min-height: 0; margin: 1rem auto; padding: 0 !important;
        display: flex; align-items: center; justify-content: center;
    }
    /* ExtraModal inserts an UpdatePanel between the flex dialog and its content. */
    #<%= mdlDeliveryFiles.ClientID %> .modal-dialog > div,
    #<%= mdlSnapshotRestore.ClientID %> .modal-dialog > div {
        width: 100%; min-width: 0; min-height: 0; max-height: 100%;
        display: flex; flex-direction: column;
    }
    #<%= mdlDeliveryFiles.ClientID %> .modal-content,
    #<%= mdlSnapshotRestore.ClientID %> .modal-content {
        width: 100%; min-height: 0; max-height: 100%; overflow: hidden;
        display: flex; flex-direction: column;
    }
    #<%= mdlDeliveryFiles.ClientID %> .modal-header,
    #<%= mdlDeliveryFiles.ClientID %> .modal-footer,
    #<%= mdlSnapshotRestore.ClientID %> .modal-header,
    #<%= mdlSnapshotRestore.ClientID %> .modal-footer { flex: 0 0 auto; }
    #<%= mdlSnapshotRestore.ClientID %> .modal-body { min-height: 0; overflow-y: auto; }
    #<%= mdlDeliveryFiles.ClientID %> .modal-header { padding: .65rem 1rem !important; }
    #<%= mdlDeliveryFiles.ClientID %> .modal-header .btn-close { transform: none; flex-shrink: 0; }
    #<%= mdlDeliveryFiles.ClientID %> .modal-body { flex: 1 1 auto; min-height: 0; overflow-y: auto; padding: 1rem; background: #f8f9fc; }
    #<%= mdlDeliveryFiles.ClientID %> .modal-footer { padding: .65rem 1rem; background: #fff; }
    .delivery-files-context { display: flex; gap: .75rem; align-items: flex-start; padding: .9rem 1rem; background: #fff; border: 1px solid #e5e7eb; border-radius: 8px; margin-bottom: 1rem; }
    .delivery-files-context > i { color: var(--bs-primary, #4d0f91); margin-top: .2rem; }
    .delivery-files-context > div { min-width: 0; overflow-wrap: anywhere; }
    .delivery-files-context__title { color: #273142; font-weight: 500; line-height: 1.5; }
    .delivery-files-context__hint { color: #667085; font-size: .8rem; margin-top: .35rem; }
    .delivery-file-row { display: grid; grid-template-columns: 40px minmax(0, 1fr) auto; align-items: center; gap: .85rem; padding: 1rem; margin-top: .65rem; background: #fff; border: 1px solid #e5e7eb; border-radius: 8px; }
    .delivery-file-row__icon { display: flex; align-items: center; justify-content: center; width: 40px; height: 46px; border-radius: 6px; background: #f4effa; color: var(--bs-primary, #4d0f91); font-size: 1.25rem; }
    .delivery-file-row__name { min-width: 0; overflow-wrap: anywhere; line-height: 1.5; }
    .delivery-file-row__name a { white-space: normal; color: #273142; font-weight: 500; }
    .delivery-file-row__name a:hover { color: var(--bs-primary, #4d0f91); text-decoration: underline; }
    .delivery-file-row__name .document-delivery__meta { font-size: .78rem; margin-top: .4rem; }
    .delivery-file-row__actions { display: flex; gap: .4rem; flex-wrap: wrap; }
    .delivery-file-row__actions .btn { white-space: nowrap; }
    @media (max-width: 575.98px) {
        .delivery-file-row { grid-template-columns: 32px minmax(0, 1fr); padding: .75rem; gap: .65rem; }
        .delivery-file-row__icon { width: 32px; height: 38px; }
        .delivery-file-row__actions { grid-column: 2; }
    }
</style>
<SweetSoft:ExtraModal runat="server" ID="mdlDeliveryFiles" Title="Danh sách file đã gửi" Size="Large" Type="Primary"
    EnsureChildControlsOnPostback="true" FooterButtonClose="true" Position="modal-dialog-centered modal-dialog-scrollable">
    <ContentTemplate>
        <div class="delivery-files-context">
            <i class="fas fa-paper-plane" aria-hidden="true"></i>
            <div>
                <asp:Label runat="server" ID="lblDeliveryFilesContext" CssClass="d-block delivery-files-context__title" />
                <div class="delivery-files-context__hint">Các file bên dưới là bản đã gửi tại thời điểm này.</div>
            </div>
        </div>
        <asp:Repeater runat="server" ID="rptDeliveryFiles"><ItemTemplate>
            <div class="delivery-file-row">
                <span class="delivery-file-row__icon" aria-hidden="true"><i class="far fa-file-alt"></i></span>
                <div class="delivery-file-row__name">
                    <asp:HyperLink runat="server" Text='<%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("Name"))) %>'
                        NavigateUrl='<%# GetFileUrl(Eval("FileUrl")) %>' data-path='<%# GetFileUrl(Eval("FileUrl")) %>'
                        Visible='<%# HasValue(Eval("FileUrl")) %>' onclick="FilesBox.LayoutFilePopUp(this); return false;" />
                    <asp:Label runat="server" Text='<%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("Name"))) %>' Visible='<%# !HasValue(Eval("FileUrl")) %>' />
                    <span class="document-delivery__meta"><%#: Eval("Details") %></span>
                    <asp:Label runat="server" CssClass="small text-muted" Text="Không có file lưu để mở lại." Visible='<%# !HasValue(Eval("FileUrl")) %>' />
                </div>
                <div class="delivery-file-row__actions">
                    <asp:HyperLink runat="server" Text="Xem trước" CssClass="btn btn-sm btn-outline-primary"
                        NavigateUrl='<%# GetFileUrl(Eval("FileUrl")) %>' data-path='<%# GetFileUrl(Eval("FileUrl")) %>'
                        Visible='<%# HasValue(Eval("FileUrl")) %>' onclick="FilesBox.LayoutFilePopUp(this); return false;" />
                    <asp:HyperLink runat="server" Text="Tải về" CssClass="btn btn-sm btn-outline-primary"
                        NavigateUrl='<%# GetWorkspaceDownloadUrl(Eval("FileUrl")) %>' Visible='<%# HasValue(Eval("FileUrl")) %>' />
                </div>
            </div>
        </ItemTemplate></asp:Repeater>
    </ContentTemplate>
</SweetSoft:ExtraModal>
<SweetSoft:ExtraModal runat="server" ID="mdlSnapshotRestore" Title="Xác nhận khôi phục" Type="Primary" Size="Large"
    EnsureChildControlsOnPostback="true" FooterButtonClose="true" Position="modal-dialog-centered modal-dialog-scrollable">
    <ContentTemplate>
        <asp:Label runat="server" ID="lblSnapshotRestoreSummary" CssClass="d-block mb-3" />
        <div class="alert alert-warning">Chỉ thay đổi danh sách file hiện tại, không xóa lịch sử cũ hay bản đã gửi khách.
            File đã ký bị khóa không được thay/gỡ. Nếu có file đang chờ ký bị ảnh hưởng, cần thu hồi yêu cầu trước khi khôi phục.</div>
        <div style="max-height:45vh;overflow-y:auto;overflow-wrap:anywhere">
            <asp:Repeater runat="server" ID="rptSnapshotRestoreChanges"><ItemTemplate>
                <div class="border-bottom py-2"><span class="d-block small text-muted"><%#: Eval("Action") %></span><%#: Eval("Name") %></div>
            </ItemTemplate></asp:Repeater>
        </div>
    </ContentTemplate>
    <FooterTemplate>
        <asp:Button runat="server" ID="btnSnapshotRestoreConfirm" Text="Xác nhận khôi phục" CssClass="btn btn-primary"
            CausesValidation="false" UseSubmitBehavior="false" OnClick="btnSnapshotRestoreConfirm_Click" />
    </FooterTemplate>
</SweetSoft:ExtraModal>
<SweetSoft:ExtraModal runat="server" ID="mdlWorkspaceUpload" Title="Tải file" Size="Normal" Type="Primary"
    EnsureChildControlsOnPostback="true" FooterButtonClose="true">
    <ContentTemplate>
        <label class="form-label">Chọn file</label>
        <asp:FileUpload runat="server" ID="fuWorkspaceFiles" AllowMultiple="true" CssClass="form-control" />
        <asp:Label runat="server" ID="lblWorkspaceUploadHint" CssClass="text-muted small d-block mt-2" />
    </ContentTemplate>
    <FooterTemplate>
        <asp:Button runat="server" ID="btnWorkspaceUpload" Text="Tải lên và lưu" CssClass="btn btn-primary" CausesValidation="false" UseSubmitBehavior="false" OnClientClick="CMSMasterJs.DisableContentChanged();" OnClick="btnWorkspaceUpload_Click" />
    </FooterTemplate>
</SweetSoft:ExtraModal>
