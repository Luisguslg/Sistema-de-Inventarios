using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    /// <inheritdoc />
    public partial class RevertAplicacionCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Aplicaciones_TiposLicenciamiento_TipoLicenciamientoId", table: "Aplicaciones");
            migrationBuilder.DropForeignKey(name: "FK_Aplicaciones_Alojamientos_AlojamientoId", table: "Aplicaciones");
            migrationBuilder.DropIndex(name: "IX_Aplicaciones_TipoLicenciamientoId", table: "Aplicaciones");
            migrationBuilder.DropIndex(name: "IX_Aplicaciones_AlojamientoId", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "TipoLicenciamientoId", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "AlojamientoId", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "CostoAnual", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "FechaAdquisicion", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "VersionActual", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "SLA", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "RPO", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "RTO", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "Autenticacion", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "ActiveDirectory", table: "Aplicaciones");
            migrationBuilder.DropTable(name: "TiposLicenciamiento");
            migrationBuilder.DropTable(name: "Alojamientos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposLicenciamiento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_TiposLicenciamiento", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Alojamientos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Alojamientos", x => x.Id));

            migrationBuilder.AddColumn<Guid>(name: "TipoLicenciamientoId", table: "Aplicaciones", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "AlojamientoId", table: "Aplicaciones", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "CostoAnual", table: "Aplicaciones", type: "decimal(18,2)", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "FechaAdquisicion", table: "Aplicaciones", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<string>(name: "VersionActual", table: "Aplicaciones", type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "SLA", table: "Aplicaciones", type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "RPO", table: "Aplicaciones", type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "RTO", table: "Aplicaciones", type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "Autenticacion", table: "Aplicaciones", type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<bool>(name: "ActiveDirectory", table: "Aplicaciones", type: "bit", nullable: false, defaultValue: false);

            migrationBuilder.CreateIndex(name: "IX_Aplicaciones_TipoLicenciamientoId", table: "Aplicaciones", column: "TipoLicenciamientoId");
            migrationBuilder.CreateIndex(name: "IX_Aplicaciones_AlojamientoId", table: "Aplicaciones", column: "AlojamientoId");
            migrationBuilder.AddForeignKey(name: "FK_Aplicaciones_TiposLicenciamiento_TipoLicenciamientoId", table: "Aplicaciones",
                column: "TipoLicenciamientoId", principalTable: "TiposLicenciamiento", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
            migrationBuilder.AddForeignKey(name: "FK_Aplicaciones_Alojamientos_AlojamientoId", table: "Aplicaciones",
                column: "AlojamientoId", principalTable: "Alojamientos", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
        }
    }
}
