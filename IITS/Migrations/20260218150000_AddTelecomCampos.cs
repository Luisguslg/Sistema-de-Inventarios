using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    [Migration("20260218150000_AddTelecomCampos")]
    /// <inheritdoc />
    public partial class AddTelecomCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Telecoms') AND name = 'Descripcion')
    ALTER TABLE [Telecoms] ADD [Descripcion] nvarchar(500) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Telecoms') AND name = 'Ubicacion')
    ALTER TABLE [Telecoms] ADD [Ubicacion] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Telecoms') AND name = 'Tipo')
    ALTER TABLE [Telecoms] ADD [Tipo] nvarchar(100) NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Telecoms') AND name = 'Descripcion')
    ALTER TABLE [Telecoms] DROP COLUMN [Descripcion];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Telecoms') AND name = 'Ubicacion')
    ALTER TABLE [Telecoms] DROP COLUMN [Ubicacion];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Telecoms') AND name = 'Tipo')
    ALTER TABLE [Telecoms] DROP COLUMN [Tipo];
");
        }
    }
}
