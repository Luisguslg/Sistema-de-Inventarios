using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    [Migration("20260218160000_AddCuentasAplicacionId")]
    public partial class AddCuentasAplicacionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'AplicacionId')
    ALTER TABLE [CuentasPrivilegiadas] ADD [AplicacionId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'AplicacionId')
    ALTER TABLE [CuentasServicio] ADD [AplicacionId] uniqueidentifier NULL;
");
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CuentasPrivilegiadas_Aplicaciones_AplicacionId')
BEGIN
    ALTER TABLE [CuentasPrivilegiadas] ADD CONSTRAINT [FK_CuentasPrivilegiadas_Aplicaciones_AplicacionId]
        FOREIGN KEY ([AplicacionId]) REFERENCES [Aplicaciones]([Id]) ON DELETE NO ACTION;
END
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CuentasPrivilegiadas_AplicacionId' AND object_id = OBJECT_ID('CuentasPrivilegiadas'))
    CREATE INDEX [IX_CuentasPrivilegiadas_AplicacionId] ON [CuentasPrivilegiadas]([AplicacionId]);
");
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CuentasServicio_Aplicaciones_AplicacionId')
BEGIN
    ALTER TABLE [CuentasServicio] ADD CONSTRAINT [FK_CuentasServicio_Aplicaciones_AplicacionId]
        FOREIGN KEY ([AplicacionId]) REFERENCES [Aplicaciones]([Id]) ON DELETE NO ACTION;
END
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CuentasServicio_AplicacionId' AND object_id = OBJECT_ID('CuentasServicio'))
    CREATE INDEX [IX_CuentasServicio_AplicacionId] ON [CuentasServicio]([AplicacionId]);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CuentasPrivilegiadas_Aplicaciones_AplicacionId')
    ALTER TABLE [CuentasPrivilegiadas] DROP CONSTRAINT [FK_CuentasPrivilegiadas_Aplicaciones_AplicacionId];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CuentasPrivilegiadas_AplicacionId' AND object_id = OBJECT_ID('CuentasPrivilegiadas'))
    DROP INDEX [IX_CuentasPrivilegiadas_AplicacionId] ON [CuentasPrivilegiadas];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'AplicacionId')
    ALTER TABLE [CuentasPrivilegiadas] DROP COLUMN [AplicacionId];
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CuentasServicio_Aplicaciones_AplicacionId')
    ALTER TABLE [CuentasServicio] DROP CONSTRAINT [FK_CuentasServicio_Aplicaciones_AplicacionId];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CuentasServicio_AplicacionId' AND object_id = OBJECT_ID('CuentasServicio'))
    DROP INDEX [IX_CuentasServicio_AplicacionId] ON [CuentasServicio];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'AplicacionId')
    ALTER TABLE [CuentasServicio] DROP COLUMN [AplicacionId];
");
        }
    }
}
