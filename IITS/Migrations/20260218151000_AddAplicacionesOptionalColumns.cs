using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    /// <summary>
    /// Añade columnas opcionales a Aplicaciones si no existen (AlojamientoId, PropietarioId, ResponsableId).
    /// Idempotente: no falla si ya fueron creadas por otra migración. Sin FKs para no depender de Alojamientos/Partes.
    /// </summary>
    [Migration("20260218151000_AddAplicacionesOptionalColumns")]
    public partial class AddAplicacionesOptionalColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'AlojamientoId')
    ALTER TABLE [Aplicaciones] ADD [AlojamientoId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'PropietarioId')
    ALTER TABLE [Aplicaciones] ADD [PropietarioId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'ResponsableId')
    ALTER TABLE [Aplicaciones] ADD [ResponsableId] uniqueidentifier NULL;
");
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Aplicaciones_AlojamientoId' AND object_id = OBJECT_ID('Aplicaciones'))
    CREATE INDEX [IX_Aplicaciones_AlojamientoId] ON [Aplicaciones]([AlojamientoId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Aplicaciones_PropietarioId' AND object_id = OBJECT_ID('Aplicaciones'))
    CREATE INDEX [IX_Aplicaciones_PropietarioId] ON [Aplicaciones]([PropietarioId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Aplicaciones_ResponsableId' AND object_id = OBJECT_ID('Aplicaciones'))
    CREATE INDEX [IX_Aplicaciones_ResponsableId] ON [Aplicaciones]([ResponsableId]);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Aplicaciones_AlojamientoId' AND object_id = OBJECT_ID('Aplicaciones'))
    DROP INDEX [IX_Aplicaciones_AlojamientoId] ON [Aplicaciones];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Aplicaciones_PropietarioId' AND object_id = OBJECT_ID('Aplicaciones'))
    DROP INDEX [IX_Aplicaciones_PropietarioId] ON [Aplicaciones];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Aplicaciones_ResponsableId' AND object_id = OBJECT_ID('Aplicaciones'))
    DROP INDEX [IX_Aplicaciones_ResponsableId] ON [Aplicaciones];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'AlojamientoId')
    ALTER TABLE [Aplicaciones] DROP COLUMN [AlojamientoId];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'PropietarioId')
    ALTER TABLE [Aplicaciones] DROP COLUMN [PropietarioId];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'ResponsableId')
    ALTER TABLE [Aplicaciones] DROP COLUMN [ResponsableId];
");
        }
    }
}
