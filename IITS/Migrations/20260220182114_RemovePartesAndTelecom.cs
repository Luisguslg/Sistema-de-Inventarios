using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    /// <inheritdoc />
    public partial class RemovePartesAndTelecom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FKs e índices pueden no existir si la BD se creó con DbSeed opcional. Usar SQL idempotente.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Aplicaciones_Partes_PropietarioId')
    ALTER TABLE [Aplicaciones] DROP CONSTRAINT [FK_Aplicaciones_Partes_PropietarioId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Aplicaciones_Partes_ResponsableId')
    ALTER TABLE [Aplicaciones] DROP CONSTRAINT [FK_Aplicaciones_Partes_ResponsableId];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Aplicaciones_PropietarioId' AND object_id = OBJECT_ID('Aplicaciones'))
    DROP INDEX [IX_Aplicaciones_PropietarioId] ON [Aplicaciones];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Aplicaciones_ResponsableId' AND object_id = OBJECT_ID('Aplicaciones'))
    DROP INDEX [IX_Aplicaciones_ResponsableId] ON [Aplicaciones];

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'PropietarioId')
    ALTER TABLE [Aplicaciones] DROP COLUMN [PropietarioId];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'ResponsableId')
    ALTER TABLE [Aplicaciones] DROP COLUMN [ResponsableId];

IF OBJECT_ID(N'[dbo].[Partes]', 'U') IS NOT NULL
    DROP TABLE [Partes];

IF OBJECT_ID(N'[dbo].[Telecoms]', 'U') IS NOT NULL
    DROP TABLE [Telecoms];
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PropietarioId",
                table: "Aplicaciones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsableId",
                table: "Aplicaciones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Partes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Telecoms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Telecoms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Telecoms_Estatus_EstatusId",
                        column: x => x.EstatusId,
                        principalTable: "Estatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aplicaciones_PropietarioId",
                table: "Aplicaciones",
                column: "PropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Aplicaciones_ResponsableId",
                table: "Aplicaciones",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Telecoms_EstatusId",
                table: "Telecoms",
                column: "EstatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Aplicaciones_Partes_PropietarioId",
                table: "Aplicaciones",
                column: "PropietarioId",
                principalTable: "Partes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Aplicaciones_Partes_ResponsableId",
                table: "Aplicaciones",
                column: "ResponsableId",
                principalTable: "Partes",
                principalColumn: "Id");
        }
    }
}
