CREATE TABLE [dbo].[GroupPermissions] (
    [GroupId]            BIGINT  IDENTITY (1, 1) NOT NULL,
    [PermissionId]       TINYINT NOT NULL,
    [GroupPermissionsID] TINYINT NOT NULL,
    CONSTRAINT [PK_GroupPermissions] PRIMARY KEY CLUSTERED ([GroupPermissionsID] ASC)
);


