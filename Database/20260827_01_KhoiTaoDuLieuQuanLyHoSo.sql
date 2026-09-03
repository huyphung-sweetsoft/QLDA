/*
    KHOI TAO DU LIEU NEN CHO PHAN QUAN LY HO SO

    Huong dan:
    1. Sao luu database dich.
    2. Trong SSMS, chon DUNG database cua dong nghiep o hop chon database.
    3. Chay toan bo file nay. File co the chay lai nhieu lan.

    Du lieu duoc dong bo:
    - 8 nhom tai lieu va 46 loai tai lieu.
    - 8 noi luu tru theo cay Van phong -> Khu vuc -> Tu -> Ke.
    - Menu Quan ly ho so va 5 menu con.
    - Quyen Xem/Them/Cap nhat/Xoa cho 5 menu con.

    Co y KHONG chen:
    - Ban ghi trong TblMauTaiLieu va file mau trong TblUploadFile.
    - Tai lieu, phien ban, lich su, trinh ky, gui khach va luu tru vat ly.
    - aspnet_AssignRoles, vi Id nhom nguoi dung khac nhau tren moi database.

    Nguyen tac:
    - Giu nguyen Id cua ban ghi dang hoat dong neu da ton tai.
    - Khong xoa cung va khong tai su dung ban ghi da xoa mem.
    - Neu co loi, toan bo thay doi trong file nay se duoc rollback.
    - File co y khong co lenh USE; database dich do nguoi chay chon trong SSMS.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.TblNhomTaiLieu', N'U') IS NULL
        THROW 51000, N'Khong tim thay bang dbo.TblNhomTaiLieu.', 1;

    IF OBJECT_ID(N'dbo.TblLoaiTaiLieu', N'U') IS NULL
        THROW 51001, N'Khong tim thay bang dbo.TblLoaiTaiLieu.', 1;

    DECLARE @NguoiThucHien NVARCHAR(150) = N'[Seed.QuanLyHoSo]';
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


    /* Kiem tra day du schema Quan ly ho so truoc khi chen du lieu. */
    IF OBJECT_ID(N'dbo.TblMauTaiLieu', N'U') IS NULL
        THROW 51005, N'Khong tim thay bang dbo.TblMauTaiLieu. Hay cap nhat schema truoc.', 1;

    IF OBJECT_ID(N'dbo.TblTaiLieu', N'U') IS NULL
        THROW 51006, N'Khong tim thay bang dbo.TblTaiLieu. Hay cap nhat schema truoc.', 1;

    IF OBJECT_ID(N'dbo.TblPhienBanTaiLieu', N'U') IS NULL
        THROW 51007, N'Khong tim thay bang dbo.TblPhienBanTaiLieu. Hay cap nhat schema truoc.', 1;

    IF OBJECT_ID(N'dbo.TblLichSuChinhSua', N'U') IS NULL
        THROW 51008, N'Khong tim thay bang dbo.TblLichSuChinhSua. Hay cap nhat schema truoc.', 1;

    IF OBJECT_ID(N'dbo.TblLichSuTaiLieu', N'U') IS NULL
        THROW 51009, N'Khong tim thay bang dbo.TblLichSuTaiLieu. Hay cap nhat schema truoc.', 1;

    IF OBJECT_ID(N'dbo.TblTrinhKyTaiLieu', N'U') IS NULL
        THROW 51010, N'Khong tim thay bang dbo.TblTrinhKyTaiLieu. Hay cap nhat schema truoc.', 1;

    IF OBJECT_ID(N'dbo.TblGuiNhanKhachHang', N'U') IS NULL
        THROW 51011, N'Khong tim thay bang dbo.TblGuiNhanKhachHang. Hay cap nhat schema truoc.', 1;

    IF OBJECT_ID(N'dbo.TblNoiLuuTru', N'U') IS NULL
        THROW 51012, N'Khong tim thay bang dbo.TblNoiLuuTru. Hay cap nhat schema truoc.', 1;

    IF OBJECT_ID(N'dbo.TblLuuTruVatLy', N'U') IS NULL
        THROW 51013, N'Khong tim thay bang dbo.TblLuuTruVatLy. Hay cap nhat schema truoc.', 1;

    IF OBJECT_ID(N'dbo.TblUploadFile', N'U') IS NULL
        THROW 51014, N'Khong tim thay bang dbo.TblUploadFile. Hay cap nhat schema truoc.', 1;

    IF OBJECT_ID(N'dbo.aspnet_Functions', N'U') IS NULL
        THROW 51015, N'Khong tim thay bang dbo.aspnet_Functions.', 1;

    IF OBJECT_ID(N'dbo.aspnet_Permission', N'U') IS NULL
        THROW 51016, N'Khong tim thay bang dbo.aspnet_Permission.', 1;

    /* Noi luu tru: ma chi dung lam khoa tu nhien de dong bo va noi cay cha-con. */
    DECLARE @NoiLuuTru TABLE
    (
        MaNoiLuuTru VARCHAR(50) NOT NULL PRIMARY KEY,
        MaNoiLuuTruCha VARCHAR(50) NULL,
        TenNoiLuuTru NVARCHAR(150) NOT NULL,
        CapLuuTru VARCHAR(30) NOT NULL,
        MoTa NVARCHAR(500) NULL,
        ThuTuHienThi INT NOT NULL
    );

    INSERT INTO @NoiLuuTru
    (
        MaNoiLuuTru,
        MaNoiLuuTruCha,
        TenNoiLuuTru,
        CapLuuTru,
        MoTa,
        ThuTuHienThi
    )
    VALUES
        ('VP-VCN', NULL, N'Phòng lưu trữ', 'VAN_PHONG', NULL, 0),
        ('KV-VCN-HC', 'VP-VCN', N'Khu vực Hành chính – Lưu trữ', 'PHONG', NULL, 0),
        ('TU-VCN-A', 'KV-VCN-HC', N'Tủ A – Hợp đồng và Pháp lý', 'TU', NULL, 0),
        ('KE-VCN-A1', 'TU-VCN-A', N'Kệ A1 – Hợp đồng đã ký', 'KE', NULL, 0),
        ('KE-VCN-A2', 'TU-VCN-A', N'Kệ A2 – Hồ sơ pháp lý', 'KE', NULL, 0),
        ('TU-VCN-B', 'KV-VCN-HC', N'Tủ B – Công văn và hồ sơ nội bộ', 'TU', NULL, 0),
        ('KE-VCN-B1', 'TU-VCN-B', N'Kệ B1 – Công văn', 'KE', NULL, 0),
        ('KE-VCN-B2', 'TU-VCN-B', N'Kệ B2 – Hồ sơ nội bộ', 'KE', NULL, 0);

    IF EXISTS
    (
        SELECT n.MaNoiLuuTru
        FROM dbo.TblNoiLuuTru n
        INNER JOIN @NoiLuuTru d ON d.MaNoiLuuTru = n.MaNoiLuuTru
        WHERE n.DaXoa = 0
        GROUP BY n.MaNoiLuuTru
        HAVING COUNT(*) > 1
    )
        THROW 51017, N'Co ma noi luu tru dang hoat dong bi trung. Da huy transaction.', 1;

    UPDATE n
    SET n.TenNoiLuuTru = d.TenNoiLuuTru,
        n.CapLuuTru = d.CapLuuTru,
        n.MoTa = d.MoTa,
        n.ThuTuHienThi = d.ThuTuHienThi,
        n.KichHoat = 1,
        n.NguoiCapNhat = @NguoiThucHien,
        n.NgayCapNhat = @NgayThucHien
    FROM dbo.TblNoiLuuTru n
    INNER JOIN @NoiLuuTru d ON d.MaNoiLuuTru = n.MaNoiLuuTru
    WHERE n.DaXoa = 0;

    INSERT INTO dbo.TblNoiLuuTru
    (
        IdNoiLuuTru,
        IdNoiLuuTruCha,
        MaNoiLuuTru,
        TenNoiLuuTru,
        CapLuuTru,
        IdNhanVienPhuTrach,
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
           NULL,
           d.MaNoiLuuTru,
           d.TenNoiLuuTru,
           d.CapLuuTru,
           NULL,
           d.MoTa,
           d.ThuTuHienThi,
           1,
           0,
           @NguoiThucHien,
           @NgayThucHien,
           NULL,
           NULL
    FROM @NoiLuuTru d
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.TblNoiLuuTru n
        WHERE n.MaNoiLuuTru = d.MaNoiLuuTru
          AND n.DaXoa = 0
    );

    IF EXISTS
    (
        SELECT 1
        FROM @NoiLuuTru d
        WHERE d.MaNoiLuuTruCha IS NOT NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.TblNoiLuuTru cha
              WHERE cha.MaNoiLuuTru = d.MaNoiLuuTruCha
                AND cha.DaXoa = 0
          )
    )
        THROW 51018, N'Khong tim thay noi luu tru cha de tao cay. Da huy transaction.', 1;

    UPDATE con
    SET con.IdNoiLuuTruCha = cha.IdNoiLuuTru,
        con.NguoiCapNhat = @NguoiThucHien,
        con.NgayCapNhat = @NgayThucHien
    FROM dbo.TblNoiLuuTru con
    INNER JOIN @NoiLuuTru d ON d.MaNoiLuuTru = con.MaNoiLuuTru
    LEFT JOIN dbo.TblNoiLuuTru cha
        ON cha.MaNoiLuuTru = d.MaNoiLuuTruCha
       AND cha.DaXoa = 0
    WHERE con.DaXoa = 0;

    /* Menu Quan ly ho so. FunctionName la khoa resource, khong phai chu hien thi truc tiep. */
    DECLARE @ChucNang TABLE
    (
        FunctionCode VARCHAR(50) NOT NULL PRIMARY KEY,
        ParentCode VARCHAR(50) NOT NULL,
        FunctionName NVARCHAR(250) NOT NULL,
        PageUrl VARCHAR(150) NOT NULL,
        DisplayOrder INT NOT NULL,
        Icon VARCHAR(50) NOT NULL
    );

    INSERT INTO @ChucNang
    (
        FunctionCode,
        ParentCode,
        FunctionName,
        PageUrl,
        DisplayOrder,
        Icon
    )
    VALUES
        ('fDocument', '', N'DOCUMENT_MANAGEMENT', '', 0, 'fas fa-folder-open'),
        ('Document', 'fDocument', N'DOCUMENT_LIST', '/Documents', 10, 'fas fa-folder-open'),
        ('DocumentGroup', 'fDocument', N'DOCUMENT_GROUP', '/Document-groups', 20, 'fas fa-layer-group'),
        ('DocumentType', 'fDocument', N'DOCUMENT_TYPE', '/Document-types', 30, 'fas fa-tags'),
        ('DocumentTemplate', 'fDocument', N'DOCUMENT_TEMPLATE', '/Document-templates', 40, 'fas fa-file-word'),
        ('DocumentStorageLocation', 'fDocument', N'DOCUMENT_STORAGE_LOCATION', '/Document-storage-locations', 50, 'fas fa-archive');

    IF EXISTS
    (
        SELECT f.FunctionCode
        FROM dbo.aspnet_Functions f
        INNER JOIN @ChucNang d ON d.FunctionCode = f.FunctionCode
        GROUP BY f.FunctionCode
        HAVING COUNT(*) > 1
    )
        THROW 51019, N'Co FunctionCode Quan ly ho so bi trung. Da huy transaction.', 1;

    UPDATE f
    SET f.ParentCode = d.ParentCode,
        f.FunctionName = d.FunctionName,
        f.PageUrl = d.PageUrl,
        f.DisplayOrder = d.DisplayOrder,
        f.Icon = d.Icon,
        f.IsActivated = 1
    FROM dbo.aspnet_Functions f
    INNER JOIN @ChucNang d ON d.FunctionCode = f.FunctionCode;

    INSERT INTO dbo.aspnet_Functions
    (
        Id,
        FunctionCode,
        ParentCode,
        FunctionName,
        PageUrl,
        DisplayOrder,
        Icon,
        IsActivated
    )
    SELECT NEWID(),
           d.FunctionCode,
           d.ParentCode,
           d.FunctionName,
           d.PageUrl,
           d.DisplayOrder,
           d.Icon,
           1
    FROM @ChucNang d
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.aspnet_Functions f
        WHERE f.FunctionCode = d.FunctionCode
    );

    /* Quyen chuc nang. Khong chen aspnet_AssignRoles vi role cua moi database co Id rieng. */
    DECLARE @Quyen TABLE
    (
        FunctionCode VARCHAR(50) NOT NULL,
        PermissionKey NVARCHAR(100) NOT NULL PRIMARY KEY,
        PermissionName NVARCHAR(255) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        DisplayOrder INT NOT NULL
    );

    INSERT INTO @Quyen
    (
        FunctionCode,
        PermissionKey,
        PermissionName,
        Description,
        DisplayOrder
    )
    VALUES
        ('Document', N'Document.View', N'Xem danh sách hồ sơ', N'', 1),
        ('Document', N'Document.Create', N'Tạo hồ sơ', N'', 2),
        ('Document', N'Document.Update', N'Cập nhật hồ sơ', N'', 3),
        ('Document', N'Document.Delete', N'Xóa hồ sơ', N'', 4),

        ('DocumentGroup', N'DocumentGroup.View', N'Xem nhóm tài liệu', N'', 1),
        ('DocumentGroup', N'DocumentGroup.Create', N'Tạo nhóm tài liệu', N'', 2),
        ('DocumentGroup', N'DocumentGroup.Update', N'Cập nhật nhóm tài liệu', N'', 3),
        ('DocumentGroup', N'DocumentGroup.Delete', N'Xóa nhóm tài liệu', N'', 4),

        ('DocumentType', N'DocumentType.View', N'Xem loại tài liệu', N'', 1),
        ('DocumentType', N'DocumentType.Create', N'Tạo loại tài liệu', N'', 2),
        ('DocumentType', N'DocumentType.Update', N'Cập nhật loại tài liệu', N'', 3),
        ('DocumentType', N'DocumentType.Delete', N'Xóa loại tài liệu', N'', 4),

        ('DocumentStorageLocation', N'DocumentStorageLocation.View', N'Xem nơi lưu trữ', N'', 1),
        ('DocumentStorageLocation', N'DocumentStorageLocation.Create', N'Tạo nơi lưu trữ', N'', 2),
        ('DocumentStorageLocation', N'DocumentStorageLocation.Update', N'Cập nhật nơi lưu trữ', N'', 3),
        ('DocumentStorageLocation', N'DocumentStorageLocation.Delete', N'Xóa nơi lưu trữ', N'', 4),

        ('DocumentTemplate', N'DocumentTemplate.View', N'Xem mẫu tài liệu', N'', 1),
        ('DocumentTemplate', N'DocumentTemplate.Create', N'Tạo mẫu tài liệu', N'', 2),
        ('DocumentTemplate', N'DocumentTemplate.Update', N'Cập nhật mẫu tài liệu', N'', 3),
        ('DocumentTemplate', N'DocumentTemplate.Delete', N'Xóa mẫu tài liệu', N'', 4);

    UPDATE p
    SET p.PermissionName = d.PermissionName,
        p.FunctionId = f.Id,
        p.Description = d.Description,
        p.DisplayOrder = d.DisplayOrder,
        p.UpdatedBy = @NguoiThucHien,
        p.UpdatedDate = @NgayThucHien,
        p.IsDeleted = 0,
        p.IsActivated = 1
    FROM dbo.aspnet_Permission p
    INNER JOIN @Quyen d ON d.PermissionKey = p.PermissionKey
    INNER JOIN dbo.aspnet_Functions f ON f.FunctionCode = d.FunctionCode;

    INSERT INTO dbo.aspnet_Permission
    (
        Id,
        PermissionKey,
        PermissionName,
        FunctionId,
        Description,
        DisplayOrder,
        CreatedBy,
        CreatedDate,
        UpdatedBy,
        UpdatedDate,
        IsDeleted,
        IsActivated
    )
    SELECT NEWID(),
           d.PermissionKey,
           d.PermissionName,
           f.Id,
           d.Description,
           d.DisplayOrder,
           @NguoiThucHien,
           @NgayThucHien,
           @NguoiThucHien,
           @NgayThucHien,
           0,
           1
    FROM @Quyen d
    INNER JOIN dbo.aspnet_Functions f ON f.FunctionCode = d.FunctionCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.aspnet_Permission p
        WHERE p.PermissionKey = d.PermissionKey
    );

    /* Kiem tra ket qua truoc khi commit. */
    IF
    (
        SELECT COUNT(*)
        FROM dbo.TblNhomTaiLieu n
        INNER JOIN @Nhom d ON d.TenNhom = n.TenNhom
        WHERE n.DaXoa = 0
    ) <> 8
        THROW 51020, N'Khong dong bo du 8 nhom tai lieu. Da huy transaction.', 1;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.TblLoaiTaiLieu l
        INNER JOIN @Loai d ON d.TenLoai = l.TenLoai
        INNER JOIN dbo.TblNhomTaiLieu n
            ON n.IdNhomTaiLieu = l.IdNhomTaiLieu
           AND n.TenNhom = d.TenNhom
           AND n.DaXoa = 0
        WHERE l.DaXoa = 0
    ) <> 46
        THROW 51021, N'Khong dong bo du 46 loai tai lieu. Da huy transaction.', 1;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.TblNoiLuuTru n
        INNER JOIN @NoiLuuTru d ON d.MaNoiLuuTru = n.MaNoiLuuTru
        WHERE n.DaXoa = 0
    ) <> 8
        THROW 51022, N'Khong dong bo du 8 noi luu tru. Da huy transaction.', 1;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.aspnet_Functions f
        INNER JOIN @ChucNang d ON d.FunctionCode = f.FunctionCode
        WHERE f.IsActivated = 1
    ) <> 6
        THROW 51023, N'Khong dong bo du 6 menu Quan ly ho so. Da huy transaction.', 1;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.aspnet_Permission p
        INNER JOIN @Quyen d ON d.PermissionKey = p.PermissionKey
        WHERE p.IsDeleted = 0
          AND p.IsActivated = 1
    ) <> 20
        THROW 51024, N'Khong dong bo du 20 quyen Quan ly ho so. Da huy transaction.', 1;

    COMMIT TRANSACTION;

    SELECT N'Nhóm tài liệu' AS HangMuc, COUNT(*) AS SoBanGhi
    FROM dbo.TblNhomTaiLieu n
    INNER JOIN @Nhom d ON d.TenNhom = n.TenNhom
    WHERE n.DaXoa = 0

    UNION ALL

    SELECT N'Loại tài liệu', COUNT(*)
    FROM dbo.TblLoaiTaiLieu l
    INNER JOIN @Loai d ON d.TenLoai = l.TenLoai
    INNER JOIN dbo.TblNhomTaiLieu n
        ON n.IdNhomTaiLieu = l.IdNhomTaiLieu
       AND n.TenNhom = d.TenNhom
       AND n.DaXoa = 0
    WHERE l.DaXoa = 0

    UNION ALL

    SELECT N'Nơi lưu trữ', COUNT(*)
    FROM dbo.TblNoiLuuTru n
    INNER JOIN @NoiLuuTru d ON d.MaNoiLuuTru = n.MaNoiLuuTru
    WHERE n.DaXoa = 0

    UNION ALL

    SELECT N'Menu quản lý hồ sơ', COUNT(*)
    FROM dbo.aspnet_Functions f
    INNER JOIN @ChucNang d ON d.FunctionCode = f.FunctionCode
    WHERE f.IsActivated = 1

    UNION ALL

    SELECT N'Quyền chức năng', COUNT(*)
    FROM dbo.aspnet_Permission p
    INNER JOIN @Quyen d ON d.PermissionKey = p.PermissionKey
    WHERE p.IsDeleted = 0
      AND p.IsActivated = 1;

    SELECT N'Không chèn dữ liệu mẫu tài liệu, file upload, hồ sơ nghiệp vụ hoặc phân quyền role.' AS GhiChu;

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
