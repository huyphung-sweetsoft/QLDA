<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/MasterTemplate.Master" AutoEventWireup="true" CodeBehind="LichBieu.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice.fLichBieu.LichBieu" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>
<%@ Register Src="~/fLichBieu/Controls/CtrlLichNgoaiLe.ascx" TagPrefix="SweetSoft" TagName="CtrlLichNgoaiLe" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cpHeadVendor" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cpHead" runat="server">
    <style>
        .cursor-pointer {
            cursor: pointer;
        }
        .time-input {
            max-width: 140px;
            text-align: center;
        }
        .accordion-header {
            transition: background-color 0.2s ease;
        }
        .accordion-header:hover {
            background-color: #e9ecef !important;
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cpMain" runat="server">
    <div class="row">
        <div class="col-xl-12">
            <div class="card min-h-sreen p-2">
                
                <!-- BREADCRUMB -->
                <SweetSoft:Navigation runat="server" ID="Navigation1"  />
                
                <!-- VÙNG TABS HEADER -->
                <div class="flex-between flex-between-xl gap-4 mb-4">
                    <div class="tabs-horizontal">
                        <ul class="nav nav-pills card-header-pills" role="tablist">
                            <li class="nav-item">
                                <a class="nav-link px-3 active fw-bold" data-bs-toggle="tab" href="#tuan-lam-viec" role="tab">
                                    <i class="fas fa-calendar-week me-2"></i><%= GetResourceText(BackEndResourceKeys.WORKING_WEEK_CONFIG) %>
                                </a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link px-3 fw-bold" data-bs-toggle="tab" href="#lich-ngoai-le" role="tab">
                                    <i class="fas fa-umbrella-beach me-2"></i><%= GetResourceText(BackEndResourceKeys.HOLIDAY_AND_MAKEUP_WORK) %>
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
                
                <!-- VÙNG NỘI DUNG TABS -->
                <div class="card-body p-0">
                    <div class="tab-content text-muted tab-overide">
                        
                        <!-- TAB 1: CẤU HÌNH TUẦN LÀM VIỆC CỐ ĐỊNH -->
                        <div class="tab-pane active js-validation validationEngineContainer" id="tuan-lam-viec" role="tabpanel">
                            <asp:UpdatePanel runat="server" ID="upnlTuanLamViec" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <fieldset class="fieldset-box mb-3">
                                        <legend class="text-primary fw-bold"><%= GetResourceText(BackEndResourceKeys.STANDARD_WORKING_HOURS) %></legend>
                                        
                                        <div class="accordion" id="accordionTuanLamViec">
                                            <!-- REPEATER TỰ ĐỘNG SINH 7 NGÀY -->
                                            <asp:Repeater ID="rptTuanLamViec" runat="server" OnItemDataBound="rptTuanLamViec_ItemDataBound">
                                                <ItemTemplate>
                                                    <div class="accordion-item mb-3 border rounded shadow-sm">
                                                        
                                                        <!-- ACCORDION HEADER -->
                                                        <div class="accordion-header d-flex justify-content-between align-items-center p-3 bg-light cursor-pointer" 
                                                             data-bs-toggle="collapse" 
                                                             data-bs-target='#collapse_<%# Eval("NgayTrongTuan") %>'>
                                                            
                                                            <div class="d-flex align-items-center gap-3">
                                                                <i class="fas fa-chevron-down text-muted" style="font-size: 12px;"></i>
                                                                <span class="fw-bold fs-6 text-dark">
                                                                    <asp:Literal ID="ltrTenThu" runat="server"></asp:Literal>
                                                                </span>
                                                            </div>
                                                            
                                                            <div class="switch-container" onclick="event.stopPropagation();">
                                                             
                                                                <SweetSoft:ExtraCheckbox runat="server" ID="chkIsWorking" 
                                                                    OnText='<%# GetResourceText(BackEndResourceKeys.WORKING) %>' 
                                                                    OffText='<%# GetResourceText(BackEndResourceKeys.DAY_OFF) %>' 
                                                                    Checked='<%# Eval("LaNgayLamViec") %>' 
                                                                    onchange='<%# "CMSMasterJs.ToggleDayConfig(this, \"collapse_" + Eval("NgayTrongTuan") + "\");" %>' />
                                                                
                                                                <asp:HiddenField runat="server" ID="hdfNgayTrongTuan" Value='<%# Eval("NgayTrongTuan") %>' />
                                                                <asp:HiddenField runat="server" ID="hdfIdCauHinh" Value='<%# Eval("IdCauHinh") %>' />
                                                            </div>
                                                        </div>
                                                        
                                                        <!-- ACCORDION BODY -->
                                                        <div id='collapse_<%# Eval("NgayTrongTuan") %>' 
                                                             class='accordion-collapse collapse <%# Convert.ToBoolean(Eval("LaNgayLamViec")) ? "show" : "" %>'>
                                                            
                                                            <div class="accordion-body border-top bg-white">
                                                                <div class="row g-4 justify-content-center">
                                                                    <!-- CA SÁNG -->
                                                                    <div class="col-xl-5 col-lg-6">
                                                                        <label class="form-label text-primary fw-bold"><i class="fas fa-sun me-2"></i><%= GetResourceText(BackEndResourceKeys.MORNING_SHIFT) %></label>
                                                                        <div class="input-group">
                                                                            <span class="input-group-text bg-light fw-bold"><%= GetResourceText(BackEndResourceKeys.FROM) %></span>
                                                                            <input type="time" runat="server" id="txtGioBatDauSang" class="form-control time-input" />
                                                                            <span class="input-group-text bg-light fw-bold"><%= GetResourceText(BackEndResourceKeys.TO) %></span>
                                                                            <input type="time" runat="server" id="txtGioKetThucSang" class="form-control time-input" />
                                                                        </div>
                                                                    </div>
                                                                    
                                                                    <!-- CA CHIỀU -->
                                                                    <div class="col-xl-5 col-lg-6">
                                                                        <label class="form-label text-info fw-bold"><i class="fas fa-cloud-sun me-2"></i><%= GetResourceText(BackEndResourceKeys.AFTERNOON_SHIFT) %></label>
                                                                        <div class="input-group">
                                                                            <span class="input-group-text bg-light fw-bold"><%= GetResourceText(BackEndResourceKeys.FROM) %></span>
                                                                            <input type="time" runat="server" id="txtGioBatDauChieu" class="form-control time-input" />
                                                                            <span class="input-group-text bg-light fw-bold"><%= GetResourceText(BackEndResourceKeys.TO) %></span>
                                                                            <input type="time" runat="server" id="txtGioKetThucChieu" class="form-control time-input" />
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div> 
                                                        </div>

                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </div>
                                    </fieldset>
                                    
                                    <div class="text-end mb-4">
                                        <SweetSoft:ExtraButton runat="server" ID="btnSaveTuan" 
                                            ButtonStyle="Primary" ButtonIcon="Save" IsPace="true" 
                                            OnClick="btnSaveTuan_Click">
                                            <%= GetResourceText(BackEndResourceKeys.SAVE_WEEK_CONFIG) %>
                                        </SweetSoft:ExtraButton>
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                        
                        <!-- TAB 2: LỄ TẾT VÀ LÀM BÙ -->
                        <!-- ========================================== -->
                        <div class="tab-pane" id="lich-ngoai-le" role="tabpanel">
                            <!-- Gọi User Control Lưới dữ liệu vào đây -->
                            <SweetSoft:CtrlLichNgoaiLe runat="server" ID="CtrlLichNgoaiLe1" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="cpModalMain" runat="server">
    

    <SweetSoft:ExtraModal runat="server" ID="dlDetail" Type="Primary" Title="Cập nhật lịch ngoại lệ" DefaultButton="lbtSubmit">
        
   
        <ContentTemplate>
            <div class="row js-validation validationEngineContainer">
           
                <asp:HiddenField ID="hdfIdNgoaiLe" runat="server" />

                <div class="col-lg-12">
                    <fieldset class="fieldset-box mb-3">
                        <legend class="text-primary fw-bold"><%= GetResourceText(BackEndResourceKeys.INFORMATION) %></legend>
                        <div class="row">
                            
                            <div class="col-lg-12 mb-3">
                                <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.EVENT_NAME) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtTenNgoaiLe" Required="true" MaxLength="255" PlaceHolder="VD: Nghỉ Tết Nguyên Đán..."></SweetSoft:ExtraTextBox>
                            </div>
                            
                            <div class="col-lg-6 mb-3">
                                <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.FROM_DATE) %></label>
                                <asp:TextBox runat="server" ID="txtNgayBatDau" type="date" CssClass="form-control validate[required]"></asp:TextBox>
                            </div>
                            
                            <div class="col-lg-6 mb-3">
                                <label class="form-label label-valid"><%= GetResourceText(BackEndResourceKeys.TO_DATE) %></label>
                                <asp:TextBox runat="server" ID="txtNgayKetThuc" type="date" CssClass="form-control validate[required]"></asp:TextBox>
                            </div>

                            <div class="col-lg-12 mb-3">
                                <label class="form-label"><%= GetResourceText(BackEndResourceKeys.DESCRIPTION) %></label>
                                <SweetSoft:ExtraTextBox runat="server" ID="txtMoTa" TextMode="MultiLine" Rows="3" PlaceHolder="Nhập ghi chú hoặc lý do nghỉ..."></SweetSoft:ExtraTextBox>
                            </div>

                        </div>
                    </fieldset>
                </div>
            </div>
        </ContentTemplate>
        
        
        <FooterTemplate>
            <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <SweetSoft:ExtraButton runat="server" ID="lbtSubmit" CssClass="waves-effect waves-light" ButtonStyle="Primary" ButtonIcon="Save" IsPace="true"
                        OnClientClick="return CMSMasterJs.CheckValid();" OnClick="lbtSubmit_Click" Visible="false">
                        <%= GetResourceText(BackEndResourceKeys.SAVE) %>
                    </SweetSoft:ExtraButton>
                </ContentTemplate>
            </asp:UpdatePanel>
        </FooterTemplate>

    </SweetSoft:ExtraModal>

</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="cpVendorScript" runat="server">
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="cpBottomScript" runat="server">
    <script type="text/javascript">
        // Hàm của Tab 1 (Giữ nguyên)
        CMSMasterJs.ToggleDayConfig = function (checkboxElement, collapseId) {
            var isChecked = $(checkboxElement).is(':checked');
            var $collapseTarget = $('#' + collapseId);

            if (isChecked) {
                $collapseTarget.collapse('show');
                $collapseTarget.find('.time-input').prop('disabled', false);
            } else {
                $collapseTarget.collapse('hide');
                $collapseTarget.find('.time-input').prop('disabled', true).val('');
            }
        };

        $(document).ready(function () {
            // HÀM TRỊ BUG TÀNG HÌNH LƯỚI Ở TAB 2
            $('a[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
                // Lấy ID của tab vừa được mở lên
                var targetTab = $(e.target).attr("href");

                // Nếu đúng là tab Lịch ngoại lệ
                if (targetTab === "#lich-ngoai-le") {
                    // Ép trình duyệt kích hoạt sự kiện "resize" ngầm (như lúc bạn bấm F12)
                    // để GridViewExtension tự động tính toán lại và hiện ra!
                    setTimeout(function () {
                        $(window).trigger('resize');
                    }, 100); // Đợi 100ms cho tab mở hẳn rồi mới resize cho mượt
                }
            });

            CMSMasterJs.AddEndRequest(function () {
                // Khởi tạo lại các hàm nếu có update panel
            });
        });
    </script>
</asp:Content>