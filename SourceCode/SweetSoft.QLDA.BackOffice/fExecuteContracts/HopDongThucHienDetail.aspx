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

        .contract-attachments .file-actions { display: none !important; }

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
                                    <div class="d-flex justify-content-end gap-2 border-top pt-3 mt-4">
                                        <button type="button" class="btn btn-outline-primary btn-sm" onclick="OpenContractFiles();">
                                            <i class="fas fa-paperclip me-1"></i>
                                            File đính kèm
                                        </button>
                                        <asp:LinkButton runat="server" ID="lbtRestoreContent"
                                            CssClass="btn btn-outline-secondary btn-sm"
                                            OnClick="lbtRestoreContent_Click" CausesValidation="false"
                                            Visible="false">
                                            <i class="fas fa-undo me-1"></i>Khôi phục bản gốc
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
                                            OnClientClick="var editor = CKEDITOR.instances['<%= txtNoiDungHopDong.ClientID %>']; if (editor) editor.updateElement(); return CMSMasterJs.CheckValid();"
                                            OnClick="lbtSubmit_Click">
                                            Lưu
                                        </SweetSoft:ExtraButton>
                                    </div>

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
    <SweetSoft:ExtraModal runat="server" ID="dlContractFiles" Type="Primary" Size="Large" Title="File đính kèm hợp đồng">
        <ContentTemplate>
            <div class="contract-attachments">
                <SweetSoft:FilesBox runat="server" ID="fbHopDong" IsMultiple="true"
                    AcceptType="application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                    MaxFileSizeBytes="10485760" />
            </div>
            <asp:Panel runat="server" ID="pnlImportContractContent" CssClass="border-top mt-3 pt-3">
                <asp:Label runat="server" AssociatedControlID="ddlContractContentFile" CssClass="form-label" Text="Dùng file làm nội dung hợp đồng" />
                <div class="d-flex flex-wrap gap-2">
                    <asp:DropDownList runat="server" ID="ddlContractContentFile" CssClass="form-select flex-grow-1" style="min-width: 220px;" />
                    <asp:LinkButton ID="lbtResetContent" runat="server" CssClass="btn btn-outline-primary"
                        OnClick="lbtResetContent_Click" CausesValidation="false">
                        <i class="fas fa-file-import me-1"></i>Đưa vào trình soạn thảo
                    </asp:LinkButton>
                </div>
                <small class="text-muted d-block mt-2">Chọn file PDF có văn bản hoặc DOCX. Nội dung đang soạn chỉ thay đổi khi bấm nút trên.</small>
            </asp:Panel>
        </ContentTemplate>
    </SweetSoft:ExtraModal>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
<script type="text/javascript">
    document.addEventListener("DOMContentLoaded", function () {
        InitContractEditorA4();
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

    function OpenContractFiles() {
        var editor = window.CKEDITOR && CKEDITOR.instances["<%= txtNoiDungHopDong.ClientID %>"];
        if (editor) editor.updateElement();
        var $modal = $("#<%= dlContractFiles.ClientID %>");
        var $box = $modal.find(".file-box").first();
        if (!$box.length) return;
        $(".file-box.active").removeClass("active");
        $box.addClass("active");
        CMSMasterJs.OpenDialog("#<%= dlContractFiles.ClientID %>", "File đính kèm hợp đồng");
    }

    if (typeof CMSMasterJs !== "undefined" && CMSMasterJs.AddEndRequest) {
        CMSMasterJs.AddEndRequest(function () {
            InitContractEditorA4();
        });
    }
</script>
</asp:Content>
