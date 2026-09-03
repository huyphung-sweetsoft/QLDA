SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.aspnet_Functions
        WHERE FunctionCode = 'fDocument'
    )
    BEGIN
        THROW 50001, N'Không tìm thấy nhóm menu fDocument.', 1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.aspnet_Functions
        WHERE FunctionCode = 'DocumentTemplate'
    )
    BEGIN
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
            NEWID(),
            'DocumentTemplate',
            'fDocument',
            N'DOCUMENT_TEMPLATE',
            '/Document-templates',
            30,
            'fas fa-file-word',
            1
        );
    END;

    DECLARE @FunctionId UNIQUEIDENTIFIER;

    SELECT @FunctionId = Id
    FROM dbo.aspnet_Functions
    WHERE FunctionCode = 'DocumentTemplate';

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.aspnet_Permission
        WHERE PermissionKey = N'DocumentTemplate.View'
    )
    BEGIN
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
        VALUES
        (
            NEWID(),
            N'DocumentTemplate.View',
            N'Xem mẫu tài liệu',
            @FunctionId,
            N'',
            1,
            N'[Script]',
            GETDATE(),
            N'[Script]',
            GETDATE(),
            0,
            1
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.aspnet_Permission
        WHERE PermissionKey = N'DocumentTemplate.Create'
    )
    BEGIN
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
        VALUES
        (
            NEWID(),
            N'DocumentTemplate.Create',
            N'Tạo mẫu tài liệu',
            @FunctionId,
            N'',
            2,
            N'[Script]',
            GETDATE(),
            N'[Script]',
            GETDATE(),
            0,
            1
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.aspnet_Permission
        WHERE PermissionKey = N'DocumentTemplate.Update'
    )
    BEGIN
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
        VALUES
        (
            NEWID(),
            N'DocumentTemplate.Update',
            N'Cập nhật mẫu tài liệu',
            @FunctionId,
            N'',
            3,
            N'[Script]',
            GETDATE(),
            N'[Script]',
            GETDATE(),
            0,
            1
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.aspnet_Permission
        WHERE PermissionKey = N'DocumentTemplate.Delete'
    )
    BEGIN
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
        VALUES
        (
            NEWID(),
            N'DocumentTemplate.Delete',
            N'Xóa mẫu tài liệu',
            @FunctionId,
            N'',
            4,
            N'[Script]',
            GETDATE(),
            N'[Script]',
            GETDATE(),
            0,
            1
        );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
