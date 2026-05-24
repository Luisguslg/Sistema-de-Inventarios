using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    public partial class CatalogosFormulario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alojamientos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Alojamientos", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Partes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Partes", x => x.Id));

            migrationBuilder.AddColumn<Guid>(
                name: "AlojamientoId",
                table: "Aplicaciones",
                type: "uniqueidentifier",
                nullable: true);
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

            migrationBuilder.CreateIndex(name: "IX_Aplicaciones_AlojamientoId", table: "Aplicaciones", column: "AlojamientoId");
            migrationBuilder.CreateIndex(name: "IX_Aplicaciones_PropietarioId", table: "Aplicaciones", column: "PropietarioId");
            migrationBuilder.CreateIndex(name: "IX_Aplicaciones_ResponsableId", table: "Aplicaciones", column: "ResponsableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Aplicaciones_Alojamientos_AlojamientoId",
                table: "Aplicaciones",
                column: "AlojamientoId",
                principalTable: "Alojamientos",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
            migrationBuilder.AddForeignKey(
                name: "FK_Aplicaciones_Partes_PropietarioId",
                table: "Aplicaciones",
                column: "PropietarioId",
                principalTable: "Partes",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
            migrationBuilder.AddForeignKey(
                name: "FK_Aplicaciones_Partes_ResponsableId",
                table: "Aplicaciones",
                column: "ResponsableId",
                principalTable: "Partes",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Aplicaciones_Alojamientos_AlojamientoId", table: "Aplicaciones");
            migrationBuilder.DropForeignKey(name: "FK_Aplicaciones_Partes_PropietarioId", table: "Aplicaciones");
            migrationBuilder.DropForeignKey(name: "FK_Aplicaciones_Partes_ResponsableId", table: "Aplicaciones");
            migrationBuilder.DropIndex(name: "IX_Aplicaciones_AlojamientoId", table: "Aplicaciones");
            migrationBuilder.DropIndex(name: "IX_Aplicaciones_PropietarioId", table: "Aplicaciones");
            migrationBuilder.DropIndex(name: "IX_Aplicaciones_ResponsableId", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "AlojamientoId", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "PropietarioId", table: "Aplicaciones");
            migrationBuilder.DropColumn(name: "ResponsableId", table: "Aplicaciones");
            migrationBuilder.DropTable(name: "Alojamientos");
            migrationBuilder.DropTable(name: "Partes");
        }
    }
}
