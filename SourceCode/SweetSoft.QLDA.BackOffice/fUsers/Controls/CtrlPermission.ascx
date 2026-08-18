<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlPermission.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fUsers.Controls.CtrlPermission" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<style>
    .table-permission thead th {
        position: sticky;
        top: 0;
        background: white;
        z-index: 10;
    }
</style>
<fieldset class="fieldset-box">
    <legend class="text-primary fw-bold">
        <span><%= GetResourceText(BackEndResourceKeys.PERMISSION) %></span>
        <a runat="server" id="btnAddPermission" visible="false" onserverclick="btnAddPermission_ServerClick" class="btn btn-info btn-outline-info p-1"><i class="icon fas fa-plus me-1"></i><%=GetResourceText(BackEndResourceKeys.ADD_NEW) %></a>
    </legend>
    <asp:UpdatePanel runat="server" ID="pnlPermission" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="table-extra table-responsive">
                <table class="table table-hover table-permission mb-0">
                    <thead>
                        <tr>
                            <th>Chức năng</th>
                            <asp:Literal runat="server" ID="ltrHeader" EnableViewState="false"></asp:Literal>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Literal runat="server" ID="ltrPermission" EnableViewState="false"></asp:Literal>
                    </tbody>
                </table>
                <div runat="server" id="itemTemplateHeader" visible="false" enableviewstate="false">
                    <th class="text-center" width="180px"><span class="fw-bold {0}">{1}</span></th>
                </div>
                <div runat="server" id="itemTemplate" visible="false" enableviewstate="false">
                    <tr data-id="{0}">
                        <td class="ps-4">--{1}</td>
                        {2}
                    </tr>
                </div>
                <div runat="server" id="itemTemplateParent" visible="false" enableviewstate="false">
                    <tr data-id="{0}">
                        <td colspan="{1}"><span class="text-primary fw-bold">{2}</span></td>
                    </tr>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnAddPermission" />
        </Triggers>
    </asp:UpdatePanel>
</fieldset>
<input runat="server" id="hdfPermission" type="hidden" data-selector="hdfPermission" value="[]" />
<SweetSoft:ExtraModal runat="server" ID="dlAddPermission" Type="Primary" Title="Thêm mới module">
    <ContentTemplate>
        <div class="row js-validation validationEngineContainer overflow-hidden">
            <div class="col-sm-6 mb-3">
                <label class="form-label label-valid">Function code</label>
                <SweetSoft:ExtraTextBox runat="server" ID="txtKey"></SweetSoft:ExtraTextBox>
            </div>
            <div class="col-sm-6 mb-3">
                <label class="form-label label-valid">Parent code</label>
                <SweetSoft:ExtraDropdown runat="server" ID="ddlParentCode" SimpleInit="true"></SweetSoft:ExtraDropdown>
            </div>
            <div class="col-sm-12 mb-3">
                <label class="form-label label-valid">Tên</label>
                <SweetSoft:ExtraTextBox runat="server" ID="txtName"></SweetSoft:ExtraTextBox>
            </div>
            <div class="col-sm-6 mb-3">
                <label class="form-label label-valid">Url</label>
                <SweetSoft:ExtraTextBox runat="server" ID="txtPageUrl"></SweetSoft:ExtraTextBox>
            </div>
            <div class="col-sm-6 mb-3">
                <label class="form-label label-valid">Icon</label>
                <SweetSoft:ExtraTextBox runat="server" ID="txtIcon"></SweetSoft:ExtraTextBox>
            </div>
            <div class="col-sm-12 mb-3">
                <div class="d-flex flex-between">
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" runat="server" id="chkView" checked="">
                        <label class="form-check-label" for="<%= chkView.ClientID %>">
                            Xem
                        </label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" runat="server" id="chkCreate" checked="">
                        <label class="form-check-label" for="<%= chkCreate.ClientID %>">
                            Tạo mới
                        </label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" runat="server" id="chkUpdate" checked="">
                        <label class="form-check-label" for="<%= chkUpdate.ClientID %>">
                            Cập nhật
                        </label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" runat="server" id="chkDelete" checked="">
                        <label class="form-check-label" for="<%= chkDelete.ClientID %>">
                            Xóa
                        </label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" runat="server" id="chkExport">
                        <label class="form-check-label" for="<%= chkExport.ClientID %>">
                            Export Excel
                        </label>
                    </div>
                    <%--<div class="form-check">
                        <input class="form-check-input" type="checkbox" runat="server" id="Checkbox1">
                        <label class="form-check-label" for="<%= chkExport.ClientID %>">
                            Export Excel
                        </label>
                    </div>--%>
                </div>
            </div>
        </div>
    </ContentTemplate>
    <FooterTemplate>
        <SweetSoft:ExtraButton runat="server" ID="btnSubmit" OnClientClick="return CMSMasterJs.CheckValid();" OnClick="btnSubmit_Click"
            CssClass="waves-effect waves-light flex-btn font-mobile-small" ButtonStyle="Info" ButtonIcon="Add" Text="Thêm"></SweetSoft:ExtraButton>
    </FooterTemplate>
</SweetSoft:ExtraModal>
<script>
    var PermissionJs = {
        selectedPermissions: [],

        init: function () {
            const table = document.querySelector('.table-permission');
            if (!table) return;

            table.addEventListener('change', function (e) {
                const checkbox = e.target;
                if (!checkbox.classList.contains('form-check-input')) return;

                const row = checkbox.closest('tr');
                const checkboxes = row.querySelectorAll('input[type="checkbox"]:not(:disabled)');
                const allCheckbox = checkboxes[0]; // Checkbox "Tất cả"
                const permissionCheckboxes = Array.from(checkboxes).slice(1); // Các checkbox quyền cụ thể

                if (checkbox === allCheckbox) {
                    permissionCheckboxes.forEach(cb => cb.checked = allCheckbox.checked);
                } else {
                    const allChecked = permissionCheckboxes.every(cb => cb.checked);
                    allCheckbox.checked = allChecked;
                }

                PermissionJs.updateSelectedPermissions();
            });

            PermissionJs.updateSelectedPermissions();
        },

        updateSelectedPermissions: function () {
            const allChecked = document.querySelectorAll('.table-permission tbody input[type="checkbox"]:checked:not(:disabled)');
            const values = Array.from(allChecked).map(cb => cb.name);
            PermissionJs.selectedPermissions = values;
        },
        saveJson: function () {
            const json = PermissionJs.selectedPermissions.map(permissionKey => ({
                PermissionKey: permissionKey,
                IsAllowed: 1,
            }));
            console.log({ json });
        },
        bindPermissions: function (savedPermissions) {
            const permissionMap = {};
            savedPermissions.forEach(p => {
                permissionMap[p.PermissionKey] = p.IsAllowed;
            });
            const checkboxes = document.querySelectorAll('.table-permission input[type="checkbox"]');
            checkboxes.forEach(cb => {
                if (cb.name.endsWith('_All')) return;

                cb.checked = !!permissionMap[cb.name];
                // Thêm class ở đây nếu được chọn và bị disabeld thì thêm class "input-default"
                if (cb.checked && cb.disabled) {
                    cb.classList.add('input-default');
                }
            });

            let isCheckboxAvailabel = false;
            document.querySelectorAll('.table-permission tbody tr').forEach(row => {
                const allCb = row.querySelector('input[name$="_All"]');
                if (!allCb) return;

                // Lấy tất cả các checkbox con (không phải _All)
                const otherCheckboxes = Array.from(row.querySelectorAll('input[type="checkbox"]'))
                    .filter(cb => !cb.name.endsWith('_All'));

                // Xét các checkbox được tính:
                // - nếu không disabled
                // - hoặc có class "input-default" (dù disabled vẫn tính)
                const consideredCheckboxes = otherCheckboxes.filter(cb => !cb.disabled || cb.classList.contains('input-default'));

                const ignoreCheckboxes = otherCheckboxes.filter(cb => cb.classList.contains('ignore-checkbox'));
                // Tất cả các checkbox này đều phải được checked
                const allChecked = consideredCheckboxes.length > 0
                    && consideredCheckboxes.length == otherCheckboxes.length - ignoreCheckboxes.length
                    && consideredCheckboxes.every(cb => cb.checked);

                allCb.checked = allChecked;

                // Nếu All checkbox được checked và bị disabled thì thêm class
                if (allCb.checked && allCb.disabled) {
                    allCb.classList.add('input-default');
                }
            });
        },
        exportAllPermissions: function () {
            const checkboxes = document.querySelectorAll('.table-permission input[type="checkbox"]');
            const result = [];

            checkboxes.forEach(cb => {
                if (!cb.name.includes('.')) return;
                if (cb.classList.contains('ignore-checkbox')) return;
                result.push({
                    PermissionKey: cb.name,
                    IsAllowed: cb.checked ? 1 : 0,
                });
            });
            $('[data-selector="hdfPermission"]').val(JSON.stringify(result));
        },
        initData: function () {
            const val = $('[data-selector="hdfPermission"]').val();
            if (typeof (val) == 'undefined' || val == null)
                return;
            PermissionJs.bindPermissions(JSON.parse(val));
        }
    };
    PermissionJs.OriginalDoPostback = __doPostBack;

    __doPostBack = function (p1, p2) {
        PermissionJs.exportAllPermissions();
        PermissionJs.OriginalDoPostback(p1, p2);
    };
    document.addEventListener('DOMContentLoaded', function () {
        PermissionJs.init();
    });
    $(document).ready(function () {
        setTimeout(() => {
            PermissionJs.initData();
        }, 500);
        CMSMasterJs.AddEndRequest(PermissionJs.init);
        CMSMasterJs.AddEndRequest(PermissionJs.initData)
    });
</script>
