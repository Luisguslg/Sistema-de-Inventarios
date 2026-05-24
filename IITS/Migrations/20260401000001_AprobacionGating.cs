using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    /// <inheritdoc />
    public partial class AprobacionGating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aprobaciones') AND name = 'TipoAccion')
    ALTER TABLE [Aprobaciones] ADD [TipoAccion] nvarchar(20) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aprobaciones') AND name = 'DatosPropuestos')
    ALTER TABLE [Aprobaciones] ADD [DatosPropuestos] nvarchar(max) NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aprobaciones') AND name = 'TipoAccion')
    ALTER TABLE [Aprobaciones] DROP COLUMN [TipoAccion];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aprobaciones') AND name = 'DatosPropuestos')
    ALTER TABLE [Aprobaciones] DROP COLUMN [DatosPropuestos];
");
        }
    }
}
