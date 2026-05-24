using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogItemsAndCuentasFields : Migration
    {
        /// <inheritdoc />
        /// <remarks>Idempotente: solo agrega columnas/tabla si no existen. No depende de Areas.</remarks>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'AreaId')
    ALTER TABLE [CuentasServicio] ADD [AreaId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'Origen')
    ALTER TABLE [CuentasServicio] ADD [Origen] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'Responsable')
    ALTER TABLE [CuentasServicio] ADD [Responsable] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'ServicioRelacionado')
    ALTER TABLE [CuentasServicio] ADD [ServicioRelacionado] nvarchar(300) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'AreaId')
    ALTER TABLE [CuentasPrivilegiadas] ADD [AreaId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'Origen')
    ALTER TABLE [CuentasPrivilegiadas] ADD [Origen] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'Responsable')
    ALTER TABLE [CuentasPrivilegiadas] ADD [Responsable] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'ServicioRelacionado')
    ALTER TABLE [CuentasPrivilegiadas] ADD [ServicioRelacionado] nvarchar(300) NULL;
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[CatalogItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [CatalogItems] (
        [Id] uniqueidentifier NOT NULL,
        [Kind] nvarchar(80) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        CONSTRAINT [PK_CatalogItems] PRIMARY KEY ([Id])
    );
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "CuentasServicio");

            migrationBuilder.DropColumn(
                name: "Origen",
                table: "CuentasServicio");

            migrationBuilder.DropColumn(
                name: "Responsable",
                table: "CuentasServicio");

            migrationBuilder.DropColumn(
                name: "ServicioRelacionado",
                table: "CuentasServicio");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "CuentasPrivilegiadas");

            migrationBuilder.DropColumn(
                name: "Origen",
                table: "CuentasPrivilegiadas");

            migrationBuilder.DropColumn(
                name: "Responsable",
                table: "CuentasPrivilegiadas");

            migrationBuilder.DropColumn(
                name: "ServicioRelacionado",
                table: "CuentasPrivilegiadas");
        }
    }
}
