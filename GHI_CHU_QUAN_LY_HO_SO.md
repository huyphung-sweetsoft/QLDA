# Ghi chú tồn đọng - Chức năng quản lý hồ sơ

Ngày ghi chú: 20/08/2026

## 1. Người tạo tài liệu

- Hiện tại `TblTaiLieu` chỉ lưu `NguoiTao` dạng chuỗi.
- Tạm thời giữ nguyên theo quyết định hiện tại.
- Khi cần hoàn thiện tính toàn vẹn dữ liệu, xem xét bổ sung `IdNhanVienTao` hoặc `UserId` làm khóa ngoại; `NguoiTao` có thể giữ lại làm tên hiển thị tại thời điểm phát sinh.

## 2. Lịch sử chỉnh sửa nội dung

- Tài liệu phân tích có đề cập `TblLichSuChinhSua`, gắn với từng `TblPhienBanTaiLieu`.
- Database `SweetSoft_QLDA` hiện chưa có bảng này; hiện chỉ có `TblLichSuTaiLieu`, là nhật ký hoạt động chung ở cấp tài liệu.
- Khi được yêu cầu, cần chuẩn bị file query để tạo bảng lịch sử chỉnh sửa theo phiên bản và các khóa ngoại/chỉ mục liên quan. Không gộp snapshot nội dung chi tiết vào nhật ký hoạt động chung nếu cần autosave và khôi phục từng mốc.

## 3. Việc cần nhớ sau khi chốt database

- Sinh lại lớp `SweetSoft.QLDA.DataAccess` từ database vì source hiện chưa có model cho các bảng quản lý hồ sơ mới.
