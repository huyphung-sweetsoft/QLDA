/*
    Chuan hoa danh muc nhom/loai tai lieu cho phan Quan ly ho so.

    Nguyen tac:
    - Giu nguyen Id cua cac nhom cu khi doi ten.
    - Khong xoa cung va khong tai su dung ban ghi da xoa mem.
    - Script co the chay lai: cap nhat ban ghi da co, chi them ban ghi con thieu.
    - Loai tai lieu la nghiep vu; file Word/PDF/am thanh/video khong phai loai tai lieu.
*/

USE [SweetSoft_QLDA];

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.TblNhomTaiLieu', N'U') IS NULL
        THROW 51000, N'Khong tim thay bang dbo.TblNhomTaiLieu.', 1;

    IF OBJECT_ID(N'dbo.TblLoaiTaiLieu', N'U') IS NULL
        THROW 51001, N'Khong tim thay bang dbo.TblLoaiTaiLieu.', 1;

    DECLARE @NguoiThucHien NVARCHAR(150) = N'[Seed.DanhMucTaiLieu]';
    DECLARE @NgayThucHien DATETIME = GETUTCDATE();

    /* Doi ten cac nhom cu, giu nguyen khoa chinh. */
    UPDATE dbo.TblNhomTaiLieu
    SET TenNhom = N'Quản trị dự án',
        MoTa = N'Kế hoạch, biên bản, báo cáo, yêu cầu thay đổi và tài liệu điều hành dự án.',
        ThuTuHienThi = 30,
        KichHoat = 1,
        NguoiCapNhat = @NguoiThucHien,
        NgayCapNhat = @NgayThucHien
    WHERE DaXoa = 0
      AND TenNhom = N'Hồ sơ dự án';

    UPDATE dbo.TblNhomTaiLieu
    SET TenNhom = N'Pháp lý & Hợp đồng',
        MoTa = N'Hợp đồng, phụ lục, thỏa thuận và các tài liệu pháp lý có giá trị xác nhận.',
        ThuTuHienThi = 20,
        KichHoat = 1,
        NguoiCapNhat = @NguoiThucHien,
        NgayCapNhat = @NgayThucHien
    WHERE DaXoa = 0
      AND TenNhom = N'Pháp lý và thương mại';

    UPDATE dbo.TblNhomTaiLieu
    SET TenNhom = N'Quản trị nội bộ',
        MoTa = N'Quy định, quy trình, chính sách, hướng dẫn và biểu mẫu dùng trong nội bộ công ty.',
        ThuTuHienThi = 80,
        KichHoat = 1,
        NguoiCapNhat = @NguoiThucHien,
        NgayCapNhat = @NgayThucHien
    WHERE DaXoa = 0
      AND TenNhom = N'Tài liệu nội bộ công ty';

    UPDATE dbo.TblNhomTaiLieu
    SET TenNhom = N'Kỹ thuật & Chất lượng',
        MoTa = N'Khảo sát, yêu cầu, đặc tả, thiết kế, kiểm thử, triển khai và vận hành hệ thống.',
        ThuTuHienThi = 40,
        KichHoat = 1,
        NguoiCapNhat = @NguoiThucHien,
        NgayCapNhat = @NgayThucHien
    WHERE DaXoa = 0
      AND TenNhom = N'Tài liệu kỹ thuật';

    UPDATE dbo.TblNhomTaiLieu
    SET TenNhom = N'Tài chính & Thanh toán',
        MoTa = N'Dự toán, đề nghị thanh toán, hóa đơn, chứng từ và đối chiếu công nợ.',
        ThuTuHienThi = 60,
        KichHoat = 1,
        NguoiCapNhat = @NguoiThucHien,
        NgayCapNhat = @NgayThucHien
    WHERE DaXoa = 0
      AND TenNhom = N'Tài chính và thanh toán';

    DECLARE @Nhom TABLE
    (
        TenNhom NVARCHAR(150) NOT NULL PRIMARY KEY,
        MoTa NVARCHAR(500) NOT NULL,
        ThuTuHienThi INT NOT NULL
    );

    INSERT INTO @Nhom (TenNhom, MoTa, ThuTuHienThi)
    VALUES
        (N'Kinh doanh & Thương mại',
         N'Đề xuất giải pháp, báo giá, thương thảo và thỏa thuận trước khi ký hợp đồng.', 10),
        (N'Pháp lý & Hợp đồng',
         N'Hợp đồng, phụ lục, thỏa thuận và các tài liệu pháp lý có giá trị xác nhận.', 20),
        (N'Quản trị dự án',
         N'Kế hoạch, biên bản, báo cáo, yêu cầu thay đổi và tài liệu điều hành dự án.', 30),
        (N'Kỹ thuật & Chất lượng',
         N'Khảo sát, yêu cầu, đặc tả, thiết kế, kiểm thử, triển khai và vận hành hệ thống.', 40),
        (N'Nghiệm thu & Bàn giao',
         N'Tài liệu xác nhận kết quả, khối lượng hoàn thành và việc bàn giao sản phẩm.', 50),
        (N'Tài chính & Thanh toán',
         N'Dự toán, đề nghị thanh toán, hóa đơn, chứng từ và đối chiếu công nợ.', 60),
        (N'Hành chính & Công văn',
         N'Công văn đến, công văn đi, quyết định, thông báo và tờ trình.', 70),
        (N'Quản trị nội bộ',
         N'Quy định, quy trình, chính sách, hướng dẫn và biểu mẫu dùng trong nội bộ công ty.', 80);

    /* Them nhom con thieu. Ban ghi cung ten nhung da xoa mem khong bi khoi phuc. */
    INSERT INTO dbo.TblNhomTaiLieu
    (
        IdNhomTaiLieu,
        TenNhom,
        MoTa,
        ThuTuHienThi,
        KichHoat,
        DaXoa,
        NguoiTao,
        NgayTao,
        NguoiCapNhat,
        NgayCapNhat
    )
    SELECT NEWID(),
           d.TenNhom,
           d.MoTa,
           d.ThuTuHienThi,
           1,
           0,
           @NguoiThucHien,
           @NgayThucHien,
           NULL,
           NULL
    FROM @Nhom d
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.TblNhomTaiLieu n
        WHERE n.TenNhom = d.TenNhom
          AND n.DaXoa = 0
    );

    /* Dong bo mo ta va thu tu neu script duoc chay lai. */
    UPDATE n
    SET n.MoTa = d.MoTa,
        n.ThuTuHienThi = d.ThuTuHienThi,
        n.KichHoat = 1,
        n.NguoiCapNhat = @NguoiThucHien,
        n.NgayCapNhat = @NgayThucHien
    FROM dbo.TblNhomTaiLieu n
    INNER JOIN @Nhom d ON d.TenNhom = n.TenNhom
    WHERE n.DaXoa = 0;

    IF EXISTS
    (
        SELECT n.TenNhom
        FROM dbo.TblNhomTaiLieu n
        INNER JOIN @Nhom d ON d.TenNhom = n.TenNhom
        WHERE n.DaXoa = 0
        GROUP BY n.TenNhom
        HAVING COUNT(*) > 1
    )
        THROW 51002, N'Co nhieu hon mot nhom dang hoat dong cung ten. Da huy transaction.', 1;

    DECLARE @Loai TABLE
    (
        TenNhom NVARCHAR(150) NOT NULL,
        TenLoai NVARCHAR(150) NOT NULL,
        MoTa NVARCHAR(500) NOT NULL,
        CanTrinhKy BIT NOT NULL,
        HinhThucKyMacDinh VARCHAR(20) NULL,
        CanGuiKhachHang BIT NOT NULL,
        CanLuuVatLy BIT NOT NULL,
        ThuTuHienThi INT NOT NULL,
        PRIMARY KEY (TenNhom, TenLoai)
    );

    INSERT INTO @Loai
    (
        TenNhom, TenLoai, MoTa,
        CanTrinhKy, HinhThucKyMacDinh,
        CanGuiKhachHang, CanLuuVatLy, ThuTuHienThi
    )
    VALUES
        /* Kinh doanh & Thuong mai */
        (N'Kinh doanh & Thương mại', N'Đề xuất giải pháp',
         N'Tài liệu đề xuất phương án hoặc giải pháp cung cấp cho khách hàng.', 0, NULL, 1, 0, 10),
        (N'Kinh doanh & Thương mại', N'Báo giá',
         N'Tài liệu báo giá sản phẩm, dịch vụ hoặc phạm vi triển khai.', 0, NULL, 1, 0, 20),
        (N'Kinh doanh & Thương mại', N'Biên bản thương thảo',
         N'Ghi nhận nội dung thương thảo trước khi các bên ký kết hợp đồng.', 1, 'GIAY', 1, 1, 30),
        (N'Kinh doanh & Thương mại', N'Bản ghi nhớ (MOU)',
         N'Ghi nhận các nguyên tắc hoặc thỏa thuận sơ bộ giữa các bên.', 1, 'GIAY', 1, 1, 40),

        /* Phap ly & Hop dong */
        (N'Pháp lý & Hợp đồng', N'Hợp đồng',
         N'Văn bản thỏa thuận chính thức xác lập quyền và nghĩa vụ giữa các bên.', 1, 'GIAY', 1, 1, 10),
        (N'Pháp lý & Hợp đồng', N'Phụ lục hợp đồng',
         N'Văn bản bổ sung hoặc điều chỉnh nội dung của hợp đồng.', 1, 'GIAY', 1, 1, 20),
        (N'Pháp lý & Hợp đồng', N'Thỏa thuận bảo mật (NDA)',
         N'Thỏa thuận quy định trách nhiệm bảo mật thông tin giữa các bên.', 1, 'GIAY', 1, 1, 30),
        (N'Pháp lý & Hợp đồng', N'Văn bản ủy quyền',
         N'Văn bản xác nhận phạm vi và thời hạn ủy quyền.', 1, 'GIAY', 0, 1, 40),
        (N'Pháp lý & Hợp đồng', N'Biên bản thanh lý hợp đồng',
         N'Xác nhận việc hoàn thành nghĩa vụ và kết thúc hợp đồng.', 1, 'GIAY', 1, 1, 50),

        /* Quan tri du an */
        (N'Quản trị dự án', N'Kế hoạch dự án',
         N'Kế hoạch phạm vi, tiến độ, nguồn lực và cách thức triển khai dự án.', 0, NULL, 0, 0, 10),
        (N'Quản trị dự án', N'Biên bản họp',
         N'Nội dung, kết luận và đầu việc của một cuộc họp; ghi âm, video là tệp đính kèm.', 0, NULL, 0, 0, 20),
        (N'Quản trị dự án', N'Biên bản làm việc',
         N'Ghi nhận chính thức nội dung làm việc và xác nhận giữa các bên.', 1, 'GIAY', 1, 1, 30),
        (N'Quản trị dự án', N'Báo cáo tiến độ',
         N'Báo cáo tình hình, kết quả và vướng mắc của dự án theo kỳ.', 0, NULL, 1, 0, 40),
        (N'Quản trị dự án', N'Yêu cầu thay đổi',
         N'Đề nghị thay đổi phạm vi, yêu cầu, tiến độ hoặc chi phí của dự án.', 1, 'GIAY', 1, 1, 50),
        (N'Quản trị dự án', N'Báo cáo tổng kết dự án',
         N'Tổng hợp kết quả, bài học và các nội dung còn lại khi kết thúc dự án.', 0, NULL, 1, 0, 60),

        /* Ky thuat & Chat luong */
        (N'Kỹ thuật & Chất lượng', N'Tài liệu khảo sát hiện trạng',
         N'Ghi nhận quy trình, dữ liệu, hệ thống và nhu cầu hiện tại của khách hàng.', 0, NULL, 1, 0, 10),
        (N'Kỹ thuật & Chất lượng', N'Tài liệu yêu cầu nghiệp vụ',
         N'Mô tả nhu cầu, quy trình và quy tắc nghiệp vụ cần đáp ứng.', 0, NULL, 1, 0, 20),
        (N'Kỹ thuật & Chất lượng', N'Đặc tả yêu cầu phần mềm',
         N'Đặc tả chi tiết các yêu cầu chức năng và phi chức năng của phần mềm.', 0, NULL, 1, 0, 30),
        (N'Kỹ thuật & Chất lượng', N'Tài liệu thiết kế hệ thống',
         N'Mô tả kiến trúc, dữ liệu, thành phần và thiết kế kỹ thuật của hệ thống.', 0, NULL, 0, 0, 40),
        (N'Kỹ thuật & Chất lượng', N'Tài liệu API',
         N'Mô tả giao diện tích hợp, dữ liệu đầu vào, đầu ra và cách sử dụng API.', 0, NULL, 0, 0, 50),
        (N'Kỹ thuật & Chất lượng', N'Kế hoạch kiểm thử',
         N'Xác định phạm vi, phương pháp, môi trường và lịch trình kiểm thử.', 0, NULL, 0, 0, 60),
        (N'Kỹ thuật & Chất lượng', N'Kịch bản kiểm thử',
         N'Tập hợp các trường hợp, bước thực hiện và kết quả mong đợi khi kiểm thử.', 0, NULL, 0, 0, 70),
        (N'Kỹ thuật & Chất lượng', N'Báo cáo kết quả kiểm thử',
         N'Tổng hợp kết quả kiểm thử, lỗi phát hiện và đánh giá chất lượng.', 0, NULL, 1, 0, 80),
        (N'Kỹ thuật & Chất lượng', N'Hướng dẫn triển khai',
         N'Hướng dẫn cài đặt, cấu hình và đưa hệ thống vào hoạt động.', 0, NULL, 1, 0, 90),
        (N'Kỹ thuật & Chất lượng', N'Hướng dẫn sử dụng',
         N'Hướng dẫn người dùng thao tác và sử dụng các chức năng của hệ thống.', 0, NULL, 1, 0, 100),
        (N'Kỹ thuật & Chất lượng', N'Tài liệu vận hành',
         N'Hướng dẫn giám sát, sao lưu, phục hồi và vận hành hệ thống.', 0, NULL, 1, 0, 110),

        /* Nghiem thu & Ban giao */
        (N'Nghiệm thu & Bàn giao', N'Biên bản nghiệm thu',
         N'Xác nhận kết quả nghiệm thu theo giai đoạn hoặc toàn bộ dự án.', 1, 'GIAY', 1, 1, 10),
        (N'Nghiệm thu & Bàn giao', N'Biên bản xác nhận khối lượng',
         N'Xác nhận khối lượng công việc hoặc sản phẩm đã hoàn thành.', 1, 'GIAY', 1, 1, 20),
        (N'Nghiệm thu & Bàn giao', N'Biên bản bàn giao',
         N'Xác nhận việc bàn giao sản phẩm, tài khoản, dữ liệu hoặc tài sản.', 1, 'GIAY', 1, 1, 30),
        (N'Nghiệm thu & Bàn giao', N'Danh mục bàn giao',
         N'Danh sách chi tiết các thành phần, tài liệu hoặc tài sản được bàn giao.', 0, NULL, 1, 0, 40),

        /* Tai chinh & Thanh toan */
        (N'Tài chính & Thanh toán', N'Dự toán dự án',
         N'Tài liệu dự kiến chi phí, ngân sách và nguồn lực tài chính của dự án.', 0, NULL, 0, 0, 10),
        (N'Tài chính & Thanh toán', N'Đề nghị thanh toán',
         N'Văn bản đề nghị thực hiện thanh toán theo hợp đồng hoặc khối lượng hoàn thành.', 1, 'GIAY', 1, 1, 20),
        (N'Tài chính & Thanh toán', N'Hóa đơn',
         N'Hóa đơn phát sinh trong quá trình cung cấp sản phẩm hoặc dịch vụ.', 0, NULL, 1, 0, 30),
        (N'Tài chính & Thanh toán', N'Chứng từ thanh toán',
         N'Chứng từ xác nhận việc thu, chi hoặc chuyển khoản đã thực hiện.', 0, NULL, 1, 0, 40),
        (N'Tài chính & Thanh toán', N'Biên bản đối chiếu công nợ',
         N'Xác nhận số liệu công nợ giữa công ty và khách hàng hoặc đối tác.', 1, 'GIAY', 1, 1, 50),

        /* Hanh chinh & Cong van */
        (N'Hành chính & Công văn', N'Công văn đến',
         N'Văn bản do cơ quan, khách hàng hoặc đối tác gửi đến công ty.', 0, NULL, 0, 1, 10),
        (N'Hành chính & Công văn', N'Công văn đi',
         N'Văn bản chính thức do công ty phát hành đến tổ chức hoặc cá nhân khác.', 1, 'GIAY', 0, 1, 20),
        (N'Hành chính & Công văn', N'Quyết định nội bộ',
         N'Văn bản quyết định do người có thẩm quyền trong công ty ban hành.', 1, 'GIAY', 0, 1, 30),
        (N'Hành chính & Công văn', N'Thông báo',
         N'Văn bản thông tin chính thức đến cá nhân hoặc bộ phận liên quan.', 0, NULL, 0, 0, 40),
        (N'Hành chính & Công văn', N'Tờ trình',
         N'Văn bản đề xuất nội dung cần người có thẩm quyền xem xét, phê duyệt.', 1, 'GIAY', 0, 1, 50),

        /* Quan tri noi bo */
        (N'Quản trị nội bộ', N'Quy định',
         N'Các yêu cầu và nguyên tắc bắt buộc áp dụng trong công ty.', 1, 'GIAY', 0, 1, 10),
        (N'Quản trị nội bộ', N'Quy trình',
         N'Trình tự, trách nhiệm và cách thực hiện một nghiệp vụ nội bộ.', 1, 'GIAY', 0, 1, 20),
        (N'Quản trị nội bộ', N'Chính sách',
         N'Định hướng và nguyên tắc quản trị được công ty ban hành.', 1, 'GIAY', 0, 1, 30),
        (N'Quản trị nội bộ', N'Hướng dẫn nội bộ',
         N'Tài liệu hướng dẫn nhân viên thực hiện công việc hoặc sử dụng hệ thống.', 0, NULL, 0, 0, 40),
        (N'Quản trị nội bộ', N'Biểu mẫu dùng chung',
         N'Biểu mẫu chuẩn được các bộ phận sử dụng trong công việc.', 0, NULL, 0, 0, 50),
        (N'Quản trị nội bộ', N'Tài liệu đào tạo',
         N'Tài liệu phục vụ đào tạo, phổ biến kiến thức và hướng dẫn nhân viên.', 0, NULL, 0, 0, 60);

    /* Khong tu dong gop khi du lieu da co trung ten o nhieu nhom. */
    IF EXISTS
    (
        SELECT l.TenLoai
        FROM dbo.TblLoaiTaiLieu l
        INNER JOIN @Loai d ON d.TenLoai = l.TenLoai
        WHERE l.DaXoa = 0
        GROUP BY l.TenLoai
        HAVING COUNT(*) > 1
    )
        THROW 51003, N'Co loai tai lieu dang hoat dong trung ten o nhieu nhom. Da huy transaction.', 1;

    /* Chuyen loai cung ten ve dung nhom, dong thoi giu nguyen Id neu da ton tai. */
    UPDATE l
    SET l.IdNhomTaiLieu = n.IdNhomTaiLieu,
        l.MoTa = d.MoTa,
        l.CanTrinhKy = d.CanTrinhKy,
        l.HinhThucKyMacDinh = d.HinhThucKyMacDinh,
        l.CanGuiKhachHang = d.CanGuiKhachHang,
        l.CanLuuVatLy = d.CanLuuVatLy,
        l.ThuTuHienThi = d.ThuTuHienThi,
        l.KichHoat = 1,
        l.NguoiCapNhat = @NguoiThucHien,
        l.NgayCapNhat = @NgayThucHien
    FROM dbo.TblLoaiTaiLieu l
    INNER JOIN @Loai d ON d.TenLoai = l.TenLoai
    INNER JOIN dbo.TblNhomTaiLieu n
        ON n.TenNhom = d.TenNhom
       AND n.DaXoa = 0
    WHERE l.DaXoa = 0;

    INSERT INTO dbo.TblLoaiTaiLieu
    (
        IdLoaiTaiLieu,
        IdNhomTaiLieu,
        TenLoai,
        MoTa,
        CanTrinhKy,
        HinhThucKyMacDinh,
        CanGuiKhachHang,
        CanLuuVatLy,
        ThuTuHienThi,
        KichHoat,
        DaXoa,
        NguoiTao,
        NgayTao,
        NguoiCapNhat,
        NgayCapNhat
    )
    SELECT NEWID(),
           n.IdNhomTaiLieu,
           d.TenLoai,
           d.MoTa,
           d.CanTrinhKy,
           d.HinhThucKyMacDinh,
           d.CanGuiKhachHang,
           d.CanLuuVatLy,
           d.ThuTuHienThi,
           1,
           0,
           @NguoiThucHien,
           @NgayThucHien,
           NULL,
           NULL
    FROM @Loai d
    INNER JOIN dbo.TblNhomTaiLieu n
        ON n.TenNhom = d.TenNhom
       AND n.DaXoa = 0
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.TblLoaiTaiLieu l
        WHERE l.TenLoai = d.TenLoai
          AND l.IdNhomTaiLieu = n.IdNhomTaiLieu
          AND l.DaXoa = 0
    );

    IF EXISTS
    (
        SELECT 1
        FROM dbo.TblLoaiTaiLieu
        WHERE CanTrinhKy = 0
          AND HinhThucKyMacDinh IS NOT NULL
          AND DaXoa = 0
    )
        THROW 51004, N'Loai khong can trinh ky khong duoc co hinh thuc ky mac dinh.', 1;

    COMMIT TRANSACTION;

    SELECT n.TenNhom,
           n.ThuTuHienThi,
           COUNT(l.IdLoaiTaiLieu) AS SoLoaiTaiLieu
    FROM dbo.TblNhomTaiLieu n
    LEFT JOIN dbo.TblLoaiTaiLieu l
        ON l.IdNhomTaiLieu = n.IdNhomTaiLieu
       AND l.DaXoa = 0
    WHERE n.DaXoa = 0
    GROUP BY n.TenNhom, n.ThuTuHienThi
    ORDER BY n.ThuTuHienThi, n.TenNhom;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
