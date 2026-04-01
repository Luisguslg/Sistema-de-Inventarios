using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    public partial class OperacionCamposActivos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(name: "AlojamientoId", table: "Operaciones", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "OwnerAreaId", table: "Operaciones", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Funcion", table: "Operaciones", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>(name: "TipoInfraestructura", table: "Operaciones", type: "nvarchar(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Host", table: "Operaciones", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>(name: "RAM", table: "Operaciones", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<int>(name: "CantidadCPU", table: "Operaciones", type: "int", nullable: true);
            migrationBuilder.AddColumn<string>(name: "VelocidadCPU", table: "Operaciones", type: "nvarchar(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CapacidadDAS", table: "Operaciones", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CapacidadSAN", table: "Operaciones", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Firmware", table: "Operaciones", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "GarantiaExpira", table: "Operaciones", type: "datetime2", nullable: true);

            migrationBuilder.CreateIndex(name: "IX_Operaciones_AlojamientoId", table: "Operaciones", column: "AlojamientoId");
            migrationBuilder.CreateIndex(name: "IX_Operaciones_OwnerAreaId", table: "Operaciones", column: "OwnerAreaId");
            migrationBuilder.AddForeignKey(name: "FK_Operaciones_Alojamientos_AlojamientoId", table: "Operaciones", column: "AlojamientoId", principalTable: "Alojamientos", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
            migrationBuilder.AddForeignKey(name: "FK_Operaciones_Areas_OwnerAreaId", table: "Operaciones", column: "OwnerAreaId", principalTable: "Areas", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Operaciones_Alojamientos_AlojamientoId", table: "Operaciones");
            migrationBuilder.DropForeignKey(name: "FK_Operaciones_Areas_OwnerAreaId", table: "Operaciones");
            migrationBuilder.DropIndex(name: "IX_Operaciones_AlojamientoId", table: "Operaciones");
            migrationBuilder.DropIndex(name: "IX_Operaciones_OwnerAreaId", table: "Operaciones");
            migrationBuilder.DropColumn(name: "AlojamientoId", table: "Operaciones");
            migrationBuilder.DropColumn(name: "OwnerAreaId", table: "Operaciones");
            migrationBuilder.DropColumn(name: "Funcion", table: "Operaciones");
            migrationBuilder.DropColumn(name: "TipoInfraestructura", table: "Operaciones");
            migrationBuilder.DropColumn(name: "Host", table: "Operaciones");
            migrationBuilder.DropColumn(name: "RAM", table: "Operaciones");
            migrationBuilder.DropColumn(name: "CantidadCPU", table: "Operaciones");
            migrationBuilder.DropColumn(name: "VelocidadCPU", table: "Operaciones");
            migrationBuilder.DropColumn(name: "CapacidadDAS", table: "Operaciones");
            migrationBuilder.DropColumn(name: "CapacidadSAN", table: "Operaciones");
            migrationBuilder.DropColumn(name: "Firmware", table: "Operaciones");
            migrationBuilder.DropColumn(name: "GarantiaExpira", table: "Operaciones");
        }
    }
}
