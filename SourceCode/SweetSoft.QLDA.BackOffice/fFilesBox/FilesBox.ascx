<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FilesBox.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fFilesBox.FilesBox" %>

<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<div class="<%=this.IsMultiple? "":"file-box-single"%> file-box" onmouseover="FilesBox.FocusFileBox(this);" data-clientid="<%=this.ClientID %>">
    <div class="loading" style="display: none;">
        <div class="lds-hourglass"></div>
    </div>
    <asp:UpdatePanel runat="server" ID="upListFile" UpdateMode="Conditional" ChildrenAsTriggers="false">
        <ContentTemplate>
            <div class="rel-contant">
                <%--control--%>
                <div runat="server" id="divControls" class="control">
                    <div class="d-flex">
                        <label runat="server" id="lbBoxTitle" visible="false">Files box</label>
                        <a href="javascript:FilesBox.AddFile();" data-bs-original-title="Accept type: <%=AcceptType %>" class="btn btn-outline-secondary waves-effect btn-add-file me-2"><i class="fas fa-plus-square me-1" aria-hidden="true"></i><%= GetResourceText(BackEndResourceKeys.UPLOAD) %></a>
                        <a runat="server" id="btnDiscardFile" onserverclick="btnDiscardFile_ServerClick" class="btn btn-danger waves-effect waves-light me-2"><i class="fas fa-trash me-1"></i></a>
                        <div class="form-check sorting-control me-2">
                            <input class="chkEnableSorting form-check-input" onchange="FilesBox.CheckLayoutFile(); FilesBox.EnableSorting(this);" type="checkbox">
                            <label class="form-check-label"><%= GetResourceText(BackEndResourceKeys.SORT) %></label>
                        </div>
                        <a runat="server" id="btnApplyFile" data-selector="btnApplyFile" class="btn btn-success d-none btn-apply-file waves-effect waves-light me-2" onserverclick="btnApplyFile_ServerClick"><i class="fas fa-check-double me-1"></i></a>
                    </div>
                    <i class="control-help"><%= GetResourceText(BackEndResourceKeys.CLICK_APPLY_TO_SAVE_CHANGES_CLICK_CANCEL_TO_REVERT_THE_CHANGES) %></i>
                </div>
                <%--content--%>
                <div id="uploaded-content" class="uploaded-content illustration-upload pswp-gallery mt-2 row">
                    <asp:Literal runat="server" ID="ltrCurrentFiles"></asp:Literal>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <%--Hidden input--%>
    <div style="display: none;">
        <input onchange="FilesBox.SelectedFile(this);" class="ipfFile" accept="<%=AcceptType %>" data-max-size="1048576" data-max-count="70" type="file"
            <%=this.IsMultiple? "multiple='multiple'":""%> />
        <input runat="server" id="txtArFileRemove" data-selector="txtArFileRemove" />
    </div>
    <input type="hidden" runat="server" id="hdfFilePermission" data-selector="hdfFilePermission" value="[]" />
</div>

<%--htmlFormatFile--%>
<div runat="server" visible="false" id="htmlFormatFile">
    <div class="col-md-2 col-sm-3 col-xs-4 mb-2 item show" data-ar="{7}" data-pswp-src="{0}">
        <div class="bg-body rounded rounded-3">
            <div class="d-flex">
                <div data-fileId="{7}" class="file-actions ms-2 me-auto">
                    <div class="dropdown" data-bs-auto-close="outside">
                        <button class="btn btn-sm btn-outline-secondary" data-bs-toggle="dropdown" title="Phân quyền truy cập file">
                            <i class="fas fa-ellipsis-v"></i>
                        </button>
                        <ul class="dropdown-menu">
                            <li>
                                <div class="form-check dropdown-item">
                                    <input class="form-check-input chk-host" id="chk-host-{7}" type="checkbox" value="single" {13}>
                                    <label class="form-check-label ms-1" for="chk-host-{7}">Người chủ trì</label>
                                </div>
                            </li>
                            <li>
                                <div class="form-check dropdown-item">
                                    <input class="form-check-input chk-secretary" id="chk-tk-{7}" type="checkbox" value="single" {14}>
                                    <label class="form-check-label ms-1" for="chk-tk-{7}">Thư ký</label>
                                </div>
                            </li>
                            <li>
                                <div class="form-check dropdown-item">
                                    <input class="form-check-input chk-participant" id="chk-km-{7}" type="checkbox" value="single" {15}>
                                    <label class="form-check-label ms-1" for="chk-km-{7}">Khách mời / đại biểu</label>
                                </div>
                            </li>
                        </ul>
                    </div>
                </div>
                <i onclick="FilesBox.RemoveFile(this);" class="remove-item fas fa-trash font-size-16 ms-auto {12}" aria-hidden="true"></i>
                <i class="sort-item fas fa-arrows-alt font-size-16 me-auto" aria-hidden="true"></i>
            </div>
            <div class="img-container {2}">
                <img src="{0}" alt="{1}" class="img-responsive" />
                <a title="Replace" href="javascript:void(0);" class="img-control left {12} hidden" onclick="FilesBox.ReplaceFile(this);" data-key="{9}" data-hdf="{10}"><i class="fas fa-cloud-upload-alt"></i></a>
                <a title="View" href="javascript:void(0);" class="img-control right d-none hidden" onclick="FilesBox.LayoutFilePopUp(this);" data-path="{11}"><i class="fa fa-search"></i></a>
            </div>
            <input name="{3}" data-selector="{3}" data-default="{5}" title="{1}" class="title" value="{1}">
            <input name="{6}" data-selector="{6}" value="{4}" type="number" title="Display order" class="order" />
            <input id="{10}" data-selector="{10}" name="{8}" value="{11}" title="Path" class="file-path" />
            <div style="display: none;" class="progress-content">
                <div class="progress">
                    <div class="number"></div>
                    <div class="bar"></div>
                </div>
            </div>
        </div>
    </div>
</div>

<%--file popup--%>
<%--<div id="file-pop" class="modal fade" tabindex="-1" role="dialog">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="btn btn-default" onclick="FilesBox.ClosePopUp();">x</button>
            </div>
            <div class="modal-body body-preview">
            </div>
        </div>
    </div>
</div>--%>