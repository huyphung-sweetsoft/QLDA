<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" Async="true" CodeBehind="HopDongThucHienDetail.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fExecuteContracts.HopDongThucHienDetail" %><%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
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
    position: relative;
    width: 100%;
    height: auto;
    overflow: visible;
    opacity: 1;
    pointer-events: auto;
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
                                    <div class="d-flex gap-2 pt-3 mt-4">
                                        <button type="button" class="btn btn-outline-primary btn-sm" onclick="OpenContractFileUpload();">
                                            <i class="fas fa-upload me-1"></i>
                                            Tải file
                                        </button>
                                        <asp:LinkButton ID="lbtResetContent" runat="server"
                                            CssClass="btn btn-secondary"
                                            OnClick="lbtResetContent_Click"
                                            CausesValidation="false">
                                            <i class="fas fa-undo"></i> Khôi phục nội dung từ file gốc
                                        </asp:LinkButton>

                                        <asp:LinkButton
                                            runat="server"
                                            ID="lbtExportPdf"
                                            CssClass="btn btn-danger"
                                            OnClick="lbtExportPdf_Click"
                                            CausesValidation="false">
                                            <i class="fas fa-file-pdf me-1"></i> Xuất PDF
                                        </asp:LinkButton>
                                        <asp:LinkButton
                                            runat="server"
                                            ID="lbtExportDocx"
                                            CssClass="btn btn-info"
                                            OnClick="lbtExportDocx_Click"
                                            OnClientClick="var editor = CKEDITOR.instances['<%= txtNoiDungHopDong.ClientID %>']; if (editor) editor.updateElement();"
                                            CausesValidation="false">
                                            <i class="fas fa-file-word me-1"></i> Xuất DOCX
                                        </asp:LinkButton>
                                        <asp:LinkButton
                                            runat="server"
                                            ID="lbtCancel"
                                            CssClass="btn btn-light"
                                            OnClick="lbtCancel_Click"
                                            CausesValidation="false">
                                            Hủy
                                        </asp:LinkButton>

                                        <SweetSoft:ExtraButton
                                            runat="server"
                                            ID="lbtSubmit"
                                            CssClass="waves-effect waves-light"
                                            ButtonStyle="Primary"
                                            ButtonIcon="Save"
                                            IsPace="true"
                                            OnClientClick="return SubmitContractWithFiles();"
                                            OnClick="lbtSubmit_Click">
                                            Lưu
                                        </SweetSoft:ExtraButton>
                                    </div>

                                </div>

                                <%-- File đã tải lên --%>
                                <div id="contractFileInfo" class="mb-3">
                                    <div class="d-flex align-items-center justify-content-between mb-2">
                                        <strong>File hợp đồng</strong>
                                        <small class="text-muted">Chọn “Nạp vào trình soạn thảo” để lấy nội dung từ file</small>
                                    </div>
                                    <div id="contractFileList" class="list-group"></div>
                                </div>

                                <asp:HiddenField runat="server" ID="hdfSelectedContractFileId" />
                                <asp:HiddenField runat="server" ID="hdfSubmitAfterFileApply" Value="0" />

                                <asp:LinkButton runat="server" ID="lbtLoadContractFile"
                                    OnClick="lbtLoadContractFile_Click"
                                    CausesValidation="false"
                                    Style="display:none;" />

                                <div id="contractFileBox" class="contract-file-box-hidden">
                                    <SweetSoft:FilesBox
                                        runat="server"
                                        ID="fbHopDong"
                                        IsMultiple="true" />
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
        InitContractAutoUpload();
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
        var $box = $("#contractFileBox .file-box").first();
        if ($box.length) {
            $("#contractFileBox .file-box").removeClass("active");
            $box.addClass("active");
        }
        return $box;
    }

    function OpenContractFileUpload() {
        var $box = GetContractFileBox();
        var $input = $box.find(".ipfFile").first();

        if (!$input.length) return;

        $input.attr("accept", "application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        $input.attr("data-type", "<%= SweetSoft.QLDA.Core.FileManager.FileUploadTypes.ProjectContract %>");
        $input.attr("multiple", "multiple");
        $input.trigger("click");
    }

    var contractFileUploadStarted = false;

    function InitContractAutoUpload() {
        if (typeof FilesBox === "undefined" || typeof FilesBox.SelectedFile !== "function")
            return;

        // Không bọc SelectedFile nhiều lần.
        if (FilesBox.SelectedFile.__contractWrapped)
            return;

        var originalSelectedFile = FilesBox.SelectedFile;

        FilesBox.SelectedFile = function (input) {
            // Để FilesBox xử lý lựa chọn file trước.
            var result = originalSelectedFile.apply(this, arguments);

            // Chờ FilesBox cập nhật ValidatedFile và PendingUploadIds.
            window.setTimeout(function () {
                if (contractFileUploadStarted || FilesBox.UploadInProgress)
                    return;

                if (!FilesBox.ValidatedFile || FilesBox.ValidatedFile.length === 0)
                    return;

                var $box = GetContractFileBox();
                if (!$box.length)
                    return;

          <%--      var refType = "<%= SweetSoft.QLDA.Core.FileManager.FileUploadTypes.ProjectContract %>"; --%>
          <%--  var refId = "<%= QueryId == Guid.Empty ? TempContractFileRefId : QueryId %>"; --%>

            contractFileUploadStarted = true;

            FilesBox.SimpleUploadComplete = function () {
                console.log("[Contract] Upload complete");

                var $contractBox = GetContractFileBox();
                var applyButton = $contractBox.find('[data-selector="btnApplyFile"]')[0];

                if (!applyButton) {
                    contractFileUploadStarted = false;
                    console.error("[Contract] Không tìm thấy nút Apply.");
                    return;
                }

                console.log("[Contract] Clicking Apply");
                applyButton.click();
            };

            console.log("[Contract] Starting upload:", FilesBox.ValidatedFile.length, "file(s)");
            FilesBox.SaveFile(refType, refId);
        }, 0);

        return result;
    };

    FilesBox.SelectedFile.__contractWrapped = true;
}

    function GetContractFileName($item) {
        var name = $item.find("input.title").first().val();
        if (!name) name = $item.attr("data-file-name") || $item.attr("data-name") || "";
        return name;
    }

    function RenderContractFileList() {
        var $box = GetContractFileBox();
        var $list = $("#contractFileList").empty();

        if (!$box.length) return;

        $box.find(".illustration-upload .item.show").each(function () {
            var $item = $(this);
            var fileId = $item.attr("data-ar");
            var fileName = GetContractFileName($item);
            var $row = $("<div/>", { "class": "list-group-item d-flex align-items-center justify-content-between gap-2" });
            var $name = $("<span/>", { "class": "text-truncate", "text": fileName || "File chưa có tên" });
            var $actions = $("<div/>", { "class": "d-flex gap-2 flex-shrink-0" });

            if (fileId && fileId !== "00000000-0000-0000-0000-000000000000") {
                $("<button/>", {
                    type: "button",
                    "class": "btn btn-sm btn-outline-primary",
                    text: "Nạp vào trình soạn thảo",
                    click: function () {
                        $("#<%= hdfSelectedContractFileId.ClientID %>").val(fileId);
                    $("#<%= lbtLoadContractFile.ClientID %>").trigger("click");
                }
            }).appendTo($actions);
        }

        $("<button/>", {
            type: "button",
            "class": "btn btn-sm btn-outline-danger",
            title: "Xóa file",
            html: '<i class="fas fa-trash"></i>',
            click: function () {
                var removeButton = $item.find(".remove-item").first();
                if (removeButton.length) {
                    FilesBox.RemoveFile(removeButton[0], false);
                    window.setTimeout(RenderContractFileList, 100);
                }
            }
        }).appendTo($actions);

        $row.append($name, $actions);
        $list.append($row);
    });

        if (!$list.children().length) {
            $list.append($("<div/>", {
                "class": "list-group-item text-muted",
                text: "Chưa có file hợp đồng."
            }));
        }
    }

    function InitContractFileInfo() {
        RenderContractFileList();
    }

    function SyncContractFileInfo() {
        RenderContractFileList();
    }

    function RemoveContractFile() {
        RenderContractFileList();
    }

    if (typeof CMSMasterJs !== "undefined" && CMSMasterJs.AddEndRequest) {
        CMSMasterJs.AddEndRequest(function () {
            InitContractEditorA4();
            InitContractFileInfo();
            InitContractAutoUpload();
        });
    }

    function SubmitContractWithFiles() {
        return CMSMasterJs.CheckValid();
    }
</script>
</asp:Content>