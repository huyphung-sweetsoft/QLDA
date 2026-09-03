/*
    Them menu Danh sach ho so va bo quyen tuong ung.

    Luu y:
    - Chon dung database trong SSMS truoc khi chay.
    - Script co the chay lai nhieu lan.
    - Khong tu dong gan quyen vao nhom nguoi dung.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.aspnet_Functions', N'U') IS NULL
        THROW 52000, N'Khong tim thay bang dbo.aspnet_Functions.', 1;

    IF OBJECT_ID(N'dbo.aspnet_Permission', N'U') IS NULL
        THROW 52001, N'Khong tim thay bang dbo.aspnet_Permission.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.aspnet_Functions
        WHERE FunctionCode = 'fDocument'
    )
        THROW 52002, N'Khong tim thay menu goc fDocument.', 1;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.aspnet_Functions
        WHERE FunctionCode = 'Document'
    ) > 1
        THROW 52003, N'FunctionCode Document dang bi trung.', 1;

    DECLARE @FunctionId UNIQUEIDENTIFIER;
    DECLARE @NguoiThucHien NVARCHAR(150) = N'[Script.Document]';
    DECLARE @NgayThucHien DATETIME = GETUTCDATE();

    SELECT @FunctionId = Id
    FROM dbo.aspnet_Functions
    WHERE FunctionCode = 'Document';

    IF @FunctionId IS NULL
    BEGIN
        SET @FunctionId = NEWID();

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
        VALUES
        (
            @FunctionId,
            'Document',
            'fDocument',
            N'DOCUMENT_LIST',
            '/Documents',
            10,
            'fas fa-folder-open',
            1
        );
    END
    ELSE
    BEGIN
        UPDATE dbo.aspnet_Functions
        SET ParentCode = 'fDocument',
            FunctionName = N'DOCUMENT_LIST',
            PageUrl = '/Documents',
            DisplayOrder = 10,
            Icon = 'fas fa-folder-open',
            IsActivated = 1
        WHERE Id = @FunctionId;
    END;

    /* Chuan hoa thu tu cac menu con cua Quan ly ho so. */
    UPDATE dbo.aspnet_Functions
    SET DisplayOrder =
        CASE FunctionCode
            WHEN 'DocumentGroup' THEN 20
            WHEN 'DocumentType' THEN 30
            WHEN 'DocumentTemplate' THEN 40
            WHEN 'DocumentStorageLocation' THEN 50
        END
    WHERE FunctionCode IN
    (
        'DocumentGroup',
        'DocumentType',
        'DocumentTemplate',
        'DocumentStorageLocation'
    );

    DECLARE @Quyen TABLE
    (
        PermissionKey NVARCHAR(100) NOT NULL PRIMARY KEY,
        PermissionName NVARCHAR(255) NOT NULL,
        DisplayOrder INT NOT NULL
    );

    INSERT INTO @Quyen
    (
        PermissionKey,
        PermissionName,
        DisplayOrder
    )
    VALUES
        (N'Document.View', N'Xem danh sách hồ sơ', 1),
        (N'Document.Create', N'Tạo hồ sơ', 2),
        (N'Document.Update', N'Cập nhật hồ sơ', 3),
        (N'Document.Delete', N'Xóa hồ sơ', 4);

    UPDATE p
    SET p.PermissionName = q.PermissionName,
        p.FunctionId = @FunctionId,
        p.Description = N'',
        p.DisplayOrder = q.DisplayOrder,
        p.UpdatedBy = @NguoiThucHien,
        p.UpdatedDate = @NgayThucHien,
        p.IsDeleted = 0,
        p.IsActivated = 1
    FROM dbo.aspnet_Permission p
    INNER JOIN @Quyen q ON q.PermissionKey = p.PermissionKey;

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
           q.PermissionKey,
           q.PermissionName,
           @FunctionId,
           N'',
           q.DisplayOrder,
           @NguoiThucHien,
           @NgayThucHien,
           @NguoiThucHien,
           @NgayThucHien,
           0,
           1
    FROM @Quyen q
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.aspnet_Permission p
        WHERE p.PermissionKey = q.PermissionKey
    );

    IF
    (
        SELECT COUNT(*)
        FROM dbo.aspnet_Permission p
        INNER JOIN @Quyen q ON q.PermissionKey = p.PermissionKey
        WHERE p.FunctionId = @FunctionId
          AND p.IsDeleted = 0
          AND p.IsActivated = 1
    ) <> 4
        THROW 52004, N'Khong tao du 4 quyen cho Danh sach ho so.', 1;

    COMMIT TRANSACTION;

    SELECT f.FunctionCode,
           f.FunctionName,
           f.PageUrl,
           p.PermissionKey,
           p.PermissionName
    FROM dbo.aspnet_Functions f
    INNER JOIN dbo.aspnet_Permission p ON p.FunctionId = f.Id
    WHERE f.FunctionCode = 'Document'
      AND p.IsDeleted = 0
    ORDER BY p.DisplayOrder;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
