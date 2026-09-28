<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlViewTaskDetail.ascx.cs" Inherits="SweetSoft.QLDA.BackOffice.fTasks.Controls.CtrlViewTaskDetail" %>
<%@ Import Namespace="SweetSoft.QLDA.Core.ResourceTexts" %>

<style>
    /* ===================================================================
       CSS CHUYÊN DỤNG CHO VIEW DETAIL (ĐÃ TỐI ƯU ĐỘ TƯƠNG PHẢN)
       =================================================================== */
    .view-card { background: #ffffff; border-radius: 8px; border: 1px solid #cbd5e1; padding: 22px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05); }
    .view-header { border-bottom: 2px dashed #cbd5e1; padding-bottom: 16px; margin-bottom: 20px; }
    
    .view-title { font-size: 21px; font-weight: 800; color: #0f172a; margin-top: 10px; line-height: 1.4; }
    .view-code-badge { font-size: 14.5px; font-weight: 700; color: #ffffff; background: #4f46e5; padding: 5px 12px; border-radius: 6px; display: inline-block; letter-spacing: 0.5px; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }
    
    .view-label { font-size: 12.5px; font-weight: 700; color: #475569; text-transform: uppercase; margin-bottom: 6px; display: block; letter-spacing: 0.5px; }
    
    /* Box hiển thị thông tin chung (Đã làm đậm viền và rõ chữ) */
    .view-value-box { 
        background: #f1f5f9; 
        border: 1px solid #cbd5e1; 
        border-radius: 6px; 
        padding: 10px 14px; 
        font-size: 16px; 
        font-weight: 600; 
        color: #0f172a; /* Text gần như đen tuyệt đối */
        min-height: 44px; 
        display: flex; 
        align-items: center;
        box-shadow: inset 0 1px 2px rgba(0,0,0,0.02);
    }
    
    .view-desc-box { 
        background: #f8fafc; 
        border: 1px solid #cbd5e1; 
        border-radius: 6px; 
        padding: 16px; 
        font-size: 15px; 
        color: #1e293b; 
        line-height: 1.6; 
        min-height: 90px; 
        white-space: pre-wrap; 
    }
    
    /* Box Ngày tháng (Tăng độ tương phản màu xanh) */
    .view-date-box { 
        background-color: #dcfce7; /* Xanh lá nhạt hơn nhưng sắc nét */
        border: 1px solid #4ade80; /* Viền xanh lá rõ rệt */
        border-radius: 6px; 
        padding: 12px 14px; 
        height: 100%; 
        display: flex; 
        flex-direction: column; 
        justify-content: center; 
    }
    .view-date-box.bg-gray { 
        background-color: #f1f5f9; 
        border-color: #cbd5e1; 
    }
    
    /* Label ngày tháng */
    .view-date-label { font-size: 12px; color: #166534; font-weight: 700; text-transform: uppercase; margin-bottom: 4px; letter-spacing: 0.5px;}
    .view-date-label.text-gray { color: #475569; }
    
    /* Value ngày tháng */
    .view-date-val { font-size: 18px; color: #14532d; font-weight: 800; } /* Xanh rêu cực đậm */
    .view-date-val.text-gray { color: #0f172a; }

    /* Box trạng thái (Đã làm đậm để tôn cái Badge lên) */
    .status-box { 
        background-color: #ffffff; 
        border: 1px solid #cbd5e1; 
        border-radius: 6px; 
        height: 100%; 
        display: flex; 
        flex-direction: column; 
        align-items: center; 
        justify-content: center; 
        text-align: center; 
        padding: 12px 8px; 
        box-shadow: 0 1px 2px rgba(0,0,0,0.03);
    }
    
    /* ĐỊNH DẠNG LẠI CÁC BADGE STATUS ĐỂ CHỮ NỔI LÊN TRÊN NỀN */
    .status-box .badge-pill-custom { 
        font-size: 14.5px !important; 
        padding: 8px 16px !important; 
        border-width: 2px !important;
        font-weight: 700 !important;
    }
    
    /* Ghi đè CSS gốc để Badge Trạng thái trong View Detail rõ ràng hơn */
    .status-box .badge-status-done {
        background-color: #dcfce7 !important;
        color: #14532d !important; /* Xanh rêu đậm */
        border-color: #22c55e !important;
    }
    .status-box .badge-status-doing {
        background-color: #dbeafe !important;
        color: #1e3a8a !important; /* Xanh biển đậm */
        border-color: #3b82f6 !important;
    }
    .status-box .badge-status-todo {
        background-color: #f1f5f9 !important;
        color: #334155 !important;
        border-color: #94a3b8 !important;
    }

    /* Chữ Trễ hạn */
    .status-box .late-label { 
        font-size: 14px !important; 
        margin-top: 8px !important; 
        color: #b91c1c !important; /* Đỏ gạch đậm */
        letter-spacing: 0.5px;
    }

    .empty-val { color: #64748b; font-style: italic; font-weight: 500; }

    /* MỞ RỘNG POPUP */
    @media (min-width: 768px) {
        #<%= mdlViewTask.ClientID %> .modal-dialog {
            max-width: 800px !important;
            width: 90% !important;
        }
    }
</style>

<SweetSoft:ExtraModal runat="server" ID="mdlViewTask" Type="Primary" Title="Chi tiết công việc">
    <ContentTemplate>
        <asp:UpdatePanel ID="upViewTask" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="p-3">
                    <div class="view-card">
                        
                        <!-- HEADER: Đưa Độ ưu tiên lên góc phải -->
                        <div class="view-header">
                            <div class="d-flex justify-content-between align-items-start">
                                <div>
                                    <span class="view-code-badge"><asp:Literal ID="ltrMaCV" runat="server"></asp:Literal></span>
                                </div>
                                <div class="text-end d-flex align-items-center gap-2">
                                    <span class="text-muted fw-bold" style="font-size: 11px;">ƯU TIÊN:</span>
                                    <asp:Literal ID="ltrDoUuTien" runat="server"></asp:Literal>
                                </div>
                            </div>
                            <div class="view-title">
                                <asp:Literal ID="ltrTenCV" runat="server"></asp:Literal>
                            </div>
                            <div class="mt-2" style="font-size: 14px; color: #475569;">
                                <i class="fas fa-layer-group text-muted me-1"></i> <asp:Literal ID="ltrGiaiDoan" runat="server"></asp:Literal>
                            </div>
                        </div>

                       <!-- HÀNG 1: Tỷ lệ 4 / 8 (Ngày bắt đầu - Công việc cha) -->
                        <div class="row g-3 mb-4">
                            <div class="col-md-4">
                                <!-- ĐÃ SỬA: Gom cả Tiêu đề và Giá trị vào chung Box xám -->
                                <div class="view-date-box bg-gray">
                                    <div class="view-date-label text-gray"><i class="far fa-calendar-alt me-1"></i>Ngày bắt đầu</div>
                                    <div class="view-date-val text-gray"><asp:Literal ID="ltrNgayBatDau" runat="server"></asp:Literal></div>
                                </div>
                            </div>
                            <div class="col-md-8">
                                <span class="view-label"><i class="fas fa-sitemap text-muted me-1"></i>Công việc cha</span>
                                <div class="view-value-box"><asp:Literal ID="ltrCongViecCha" runat="server"></asp:Literal></div>
                            </div>
                        </div>

                        <!-- HÀNG 2: Tỷ lệ 4 / 8 (Thời hạn - Phụ thuộc) -->
                        <div class="row g-3 mb-4">
                            <div class="col-md-4">
                                <!-- ĐÃ SỬA: Gom cả Tiêu đề và Giá trị vào chung Box xám -->
                                <div class="view-date-box bg-gray">
                                    <div class="view-date-label text-gray"><i class="fas fa-hourglass-half me-1"></i>Thời hạn</div>
                                    <div class="view-date-val text-gray"><asp:Literal ID="ltrThoiHan" runat="server"></asp:Literal></div>
                                </div>
                            </div>
                            <div class="col-md-8">
                                <span class="view-label"><i class="fas fa-link text-muted me-1"></i>Phụ thuộc</span>
                                <div class="view-value-box"><asp:Literal ID="ltrPhuThuoc" runat="server"></asp:Literal></div>
                            </div>
                        </div>

                        <!-- HÀNG 3: Tỷ lệ 4 / 4 / 4 (Dự kiến HT - Thực tế HT - Trạng thái) -->
                        <div class="row g-3 mb-4">
                            <div class="col-md-4">
                                <div class="view-date-box bg-gray">
                                    <div class="view-date-label text-gray"><i class="far fa-calendar-alt me-1"></i>Dự kiến HT</div>
                                    <div class="view-date-val text-gray"><asp:Literal ID="ltrNgayKetThuc" runat="server"></asp:Literal></div>
                                </div>
                            </div>
                            <div class="col-md-4">
                                <div class="view-date-box">
                                    <div class="view-date-label"><i class="far fa-calendar-check me-1"></i>Thực tế HT</div>
                                    <div class="view-date-val"><asp:Literal ID="ltrNgayHoanThanhThucTe" runat="server"></asp:Literal></div>
                                </div>
                            </div>
                            <div class="col-md-4">
                                <div class="status-box">
                                    <asp:Literal ID="ltrTrangThai" runat="server"></asp:Literal>
                                </div>
                            </div>
                        </div>

                        <!-- MÔ TẢ -->
                        <div>
                            <span class="view-label"><i class="fas fa-align-left text-muted me-1"></i>Mô tả / Tóm tắt</span>
                            <div class="view-desc-box"><asp:Literal ID="ltrMoTa" runat="server"></asp:Literal></div>
                        </div>

                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </ContentTemplate>
    <FooterTemplate>
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
    </FooterTemplate>
</SweetSoft:ExtraModal>