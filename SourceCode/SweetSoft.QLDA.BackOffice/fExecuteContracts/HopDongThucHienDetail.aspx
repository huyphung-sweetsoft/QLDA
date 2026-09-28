<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="HopDongThucHienDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fExecuteContracts.HopDongThucHienDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fFilesBox/FilesBox.ascx" TagPrefix="SweetSoft" TagName="FilesBox" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .contract-layout {
            display: grid;
            grid-template-columns: minmax(380px, 430px) minmax(210mm, 1fr);
            gap: 24px;
            align-items: start;
        }

        .contract-info-panel,
        .contract-content-panel {
            min-width: 0;
        }

        .contract-content-panel {
            min-width: 210mm;
        }

        .contract-editor-wrapper {
            display: flex;
            justify-content: center;
            background: #eef0f2;
            padding: 24px;
            overflow-x: auto;
        }

        .contract-editor-page {
            width: 210mm;
            min-height: 297mm;
            background: #fff;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.12);
        }

        .contract-editor-page .cke {
            width: 210mm !important;
        }

        .contract-editor-page .cke_contents {
            width: 210mm !important;
            height: 297mm !important;
            background: #fff !important;
        }

        .contract-editor-page .cke_contents iframe {
            background: #fff;
        }

        .contract-file-upload {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 12px;
            margin-bottom: 12px;
        }

        .contract-file-info {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 12px;
            min-width: 0;
            padding: 8px 12px;
            border: 1px solid #dee2e6;
            border-radius: 4px;
            background: #fff;
        }

        .contract-file-name {
            display: flex;
            align-items: center;
            min-width: 0;
            overflow: hidden;
        }

        .contract-file-name span {
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .contract-file-box-hidden {
            position: fixed;
            left: -10000px;
            top: -10000px;
            width: 1px;
            height: 1px;
            overflow: hidden;
            opacity: 0;
            pointer-events: none;
        }

        .contract-file-box-hidden .file-box {
            width: 1px !important;
            min-width: 1px !important;
        }

        @media (max-width: 1200px) {
            .contract-layout {
                grid-template-columns: 1fr;
            }

            .contract-content-panel {
                min-width: 0;
            }

            .contract-editor-wrapper {
                justify-content: flex-start;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card p-2 min-h-sreen">
                <SweetSoft:Navigation runat="server" ID="Navigation1" />

                <div class="px-3 pb-3">
                    <asp:Panel runat="server" ID="pnlContract" CssClass="js-validation validationEngineContainer">
                        <div class="contract-layout">

                            <%-- Bên trái: Thông tin hợp đồng --%>
                            <div class="contract-info-panel">
                                <div class="border-bottom pb-2 mb-3">
                                    <h5 class="mb-0">Thông tin hợp đồng</h5>
                                </div>

                                <div class="mb-3">
                                    <label class="form-label label-valid">Số hợp đồng</label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtSoHopDong" Required="true" MaxLength="100" PlaceHolder="Nhập số hợp đồng"></SweetSoft:ExtraTextBox>
                                </div>

                                <div class="mb-3">
                                    <label class="form-label label-valid">Tên hợp đồng</label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtTenHopDong" Required="true" MaxLength="250" PlaceHolder="Nhập tên hợp đồng"></SweetSoft:ExtraTextBox>
                                </div>

                                <asp:Panel runat="server" ID="pnlContractDocumentIdentityLocked" CssClass="mb-3" Visible="false">
                                    <div class="alert alert-info py-2 mb-0" role="alert">
                                        <i class="fas fa-info-circle me-1"></i>
                                        <%= GetResourceText(BackEndResourceKeys.CONTRACT_DOCUMENT_IDENTITY_LOCKED) %>
                                    </div>
                                </asp:Panel>

                                <div class="mb-3">
                                    <label class="form-label label-valid">Khách hàng</label>
                                    <SweetSoft:ExtraDropdown runat="server" ID="ddlKhachHang" Required="true" SimpleInit="true" ValueIsOfTypeGUID="true" PlaceHolder="Chọn khách hàng"></SweetSoft:ExtraDropdown>
                                </div>

                                <div class="mb-3">
                                    <label class="form-label label-valid">Giá trị hợp đồng</label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtGiaTriHopDong" Required="true" PlaceHolder="Nhập giá trị hợp đồng"></SweetSoft:ExtraTextBox>
                                </div>

                                <div class="mb-3">
                                    <label class="form-label label-valid">Ngày ký</label>
                                    <asp:TextBox runat="server" ID="txtNgayKy" type="date" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="mb-3">
                                    <label class="form-label">Ngày hiệu lực</label>
                                    <asp:TextBox runat="server" ID="txtNgayHieuLuc" type="date" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="mb-3">
                                    <label class="form-label">Ngày hết hạn</label>
                                    <asp:TextBox runat="server" ID="txtNgayHetHan" type="date" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="mb-3">
                                    <label class="form-label">Mô tả</label>
                                    <SweetSoft:ExtraTextBox runat="server" ID="txtMoTa" TextMode="MultiLine" Rows="5" MaxLength="1000" PlaceHolder="Nhập mô tả"></SweetSoft:ExtraTextBox>
                                </div>
                            </div>

                            <%-- Bên phải: Nội dung hợp đồng --%>
                            <div class="contract-content-panel">
                                <div class="d-flex align-items-center justify-content-between border-bottom pb-2 mb-3">
                                    <h5 class="mb-0">Nội dung hợp đồng</h5>

                                    <button type="button" class="btn btn-outline-primary btn-sm" onclick="OpenContractFileUpload();">
                                        <i class="fas fa-upload me-1"></i>
                                        Tải file
                                    </button>
                                </div>

                                <%-- File đã tải lên --%>
                                <div id="contractFileInfo" class="contract-file-info d-none mb-3">
                                    <div class="contract-file-name">
                                        <i id="contractFileIcon" class="fas fa-file-alt text-primary me-2"></i>
                                        <span id="contractFileName"></span>
                                    </div>

                                    <button type="button" class="btn btn-sm btn-link text-danger p-0" onclick="RemoveContractFile();" title="Xóa file">
                                        <i class="fas fa-trash"></i>
                                    </button>
                                </div>

                                <%-- FilesBox được giữ lại để sử dụng cơ chế upload hiện tại nhưng không hiển thị UI --%>
                                <div id="contractFileBox" class="contract-file-box-hidden">
                                    <SweetSoft:FilesBox
                                        runat="server"
                                        ID="fbHopDong"
                                        IsMultiple="false" />
                                </div>

                                <%-- CKEditor --%>
                                <asp:Panel runat="server" ID="pnlSoanThao" ClientIDMode="Static">
                                    <div class="contract-editor-wrapper">
                                        <div class="contract-editor-page">
                                            <CKEditor:CKEditorControl
                                                runat="server"
                                                ID="txtNoiDungHopDong"
                                                Width="100%"
                                                CssClass="ck-editor"
                                                Toolbar="Full"
                                                Language="vi-VN"
                                                AutoParagraph="false"
                                                BasePath="~/Styles/plugins/ckeditor/"
                                                Height="1000">
                                            </CKEditor:CKEditorControl>
                                        </div>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </asp:Panel>

                    <%-- Buttons --%>
                    <div class="d-flex justify-content-end gap-2 border-top pt-3 mt-4">
                        <asp:LinkButton
                            runat="server"
                            ID="lbtCancel"
                            CssClass="btn btn-light"
                            OnClick="lbtCancel_Click"
                            CausesValidation="false">
                            Hủy
                        </asp:LinkButton>

                        <asp:LinkButton
                            runat="server"
                            ID="lbtExportPdf"
                            CssClass="btn btn-danger"
                            OnClick="lbtExportPdf_Click"
                            CausesValidation="false">
                            <i class="fas fa-file-pdf me-1"></i> Xuất PDF
                        </asp:LinkButton>

                        <SweetSoft:ExtraButton
                            runat="server"
                            ID="lbtSubmit"
                            CssClass="waves-effect waves-light"
                            ButtonStyle="Primary"
                            ButtonIcon="Save"
                            IsPace="true"
                            OnClientClick="return CMSMasterJs.CheckValid();"
                            OnClick="lbtSubmit_Click">
                            Lưu
                        </SweetSoft:ExtraButton>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
<script type="text/javascript">
    document.addEventListener("DOMContentLoaded", function () {
        InitContractEditorA4();
        InitContractFileInfo();
    });

    function InitContractEditorA4() {
        if (typeof CKEDITOR === "undefined") {
            return;
        }

        var editor = CKEDITOR.instances["<%= txtNoiDungHopDong.ClientID %>"];

        if (!editor) {
            return;
        }

        var applyEditorStyle = function () {
            editor.document.appendStyleText(`
                html, body {
                    margin: 0;
                    padding: 0;
                    background: #fff;
                }

                body {
                    padding: 15mm;
                    box-sizing: border-box;
                    min-height: 297mm;
                    font-family: Arial, sans-serif;
                    font-size: 13px;
                    line-height: 1.5;
                    word-wrap: break-word;
                }

                img {
                    max-width: 100%;
                    height: auto;
                }

                table {
                    max-width: 100%;
                    border-collapse: collapse;
                }
            `);
        };

        if (editor.status === "ready") {
            applyEditorStyle();
        } else {
            editor.on("instanceReady", applyEditorStyle);
        }
    }

    function GetContractFileBox() {
        var $box = $("#contractFileBox .file-box");

        if ($box.length === 0) {
            return $();
        }

        $("#contractFileBox .file-box").removeClass("active");
        $box.first().addClass("active");

        return $box.first();
    }

    function OpenContractFileUpload() {
        var $box = GetContractFileBox();

        if ($box.length === 0) {
            return;
        }

        var $input = $box.find(".ipfFile").first();

        if ($input.length === 0) {
            return;
        }

        $input.attr("accept", "application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        $input.attr("data-type", "<%= SweetSoft.QLDA.Core.FileManager.FileUploadTypes.ProjectContract %>");
        $input.removeAttr("multiple");
        $input.trigger("click");
    }

    function InitContractFileInfo() {
        var $box = GetContractFileBox();

        if ($box.length === 0) {
            return;
        }

        var $item = $box.find(".illustration-upload .item.show").first();

        if ($item.length === 0) {
            $("#contractFileInfo").addClass("d-none");
            $("#contractFileName").text("");
            return;
        }

        var fileName = $item.find("input.title").first().val();

        if (!fileName) {
            fileName = $item.attr("data-file-name") || $item.attr("data-name") || "";
        }

        if (!fileName) {
            return;
        }

        ShowContractFileInfo(fileName);
    }

    function ShowContractFileInfo(fileName) {
        if (!fileName) {
            $("#contractFileInfo").addClass("d-none");
            $("#contractFileName").text("");
            return;
        }

        var extension = "";
        var lastDot = fileName.lastIndexOf(".");

        if (lastDot >= 0) {
            extension = fileName.substring(lastDot + 1).toLowerCase();
        }

        var iconClass = "fas fa-file-alt text-primary";

        if (extension === "pdf") {
            iconClass = "fas fa-file-pdf text-danger";
        } else if (extension === "doc" || extension === "docx") {
            iconClass = "fas fa-file-word text-primary";
        }

        $("#contractFileIcon").attr("class", iconClass + " me-2");
        $("#contractFileName").text(fileName);
        $("#contractFileInfo").removeClass("d-none");
    }

    function RemoveContractFile() {
        var $box = GetContractFileBox();

        if ($box.length === 0) {
            return;
        }

        var $item = $box.find(".illustration-upload .item.show").first();

        if ($item.length === 0) {
            $("#contractFileInfo").addClass("d-none");
            $("#contractFileName").text("");
            return;
        }

        var removeButton = $item.find(".remove-item").first();

        if (removeButton.length > 0) {
            FilesBox.RemoveFile(removeButton[0], false);
        }

        $("#contractFileInfo").addClass("d-none");
        $("#contractFileName").text("");
    }

    function SyncContractFileInfo() {
        var $box = GetContractFileBox();

        if ($box.length === 0) {
            return;
        }

        var $item = $box.find(".illustration-upload .item.show").first();

        if ($item.length === 0) {
            $("#contractFileInfo").addClass("d-none");
            $("#contractFileName").text("");
            return;
        }

        var fileName = $item.find("input.title").first().val();

        if (!fileName) {
            fileName = $item.attr("data-file-name") || $item.attr("data-name") || "";
        }

        ShowContractFileInfo(fileName);
    }

    if (typeof CMSMasterJs !== "undefined" && CMSMasterJs.AddEndRequest) {
        CMSMasterJs.AddEndRequest(function () {
            InitContractEditorA4();
            InitContractFileInfo();
        });
    }
</script>
</asp:Content>