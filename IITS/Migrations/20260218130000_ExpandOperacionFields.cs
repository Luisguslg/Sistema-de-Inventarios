using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    public partial class ExpandOperacionFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OfficeId",
                table: "Operaciones",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "AreaId",
                table: "Operaciones",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "EnvironmentId",
                table: "Operaciones",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "CriticalityId",
                table: "Operaciones",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Operaciones",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "ManufacturerId",
                table: "Operaciones",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<Guid>(
                name: "DeviceModelId",
                table: "Operaciones",
                type: "uniqueidentifier",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "IP",
                table: "Operaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "MAC",
                table: "Operaciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "SistemaOperativo",
                table: "Operaciones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "TipoDispositivo",
                table: "Operaciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Operaciones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(name: "IX_Operaciones_OfficeId", table: "Operaciones", column: "OfficeId");
            migrationBuilder.CreateIndex(name: "IX_Operaciones_AreaId", table: "Operaciones", column: "AreaId");
            migrationBuilder.CreateIndex(name: "IX_Operaciones_EnvironmentId", table: "Operaciones", column: "EnvironmentId");
            migrationBuilder.CreateIndex(name: "IX_Operaciones_CriticalityId", table: "Operaciones", column: "CriticalityId");
            migrationBuilder.CreateIndex(name: "IX_Operaciones_CategoryId", table: "Operaciones", column: "CategoryId");
            migrationBuilder.CreateIndex(name: "IX_Operaciones_ManufacturerId", table: "Operaciones", column: "ManufacturerId");
            migrationBuilder.CreateIndex(name: "IX_Operaciones_DeviceModelId", table: "Operaciones", column: "DeviceModelId");

            migrationBuilder.AddForeignKey(name: "FK_Operaciones_Offices_OfficeId", table: "Operaciones", column: "OfficeId", principalTable: "Offices", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
            migrationBuilder.AddForeignKey(name: "FK_Operaciones_Areas_AreaId", table: "Operaciones", column: "AreaId", principalTable: "Areas", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
            migrationBuilder.AddForeignKey(name: "FK_Operaciones_Environments_EnvironmentId", table: "Operaciones", column: "EnvironmentId", principalTable: "Environments", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
            migrationBuilder.AddForeignKey(name: "FK_Operaciones_Criticalities_CriticalityId", table: "Operaciones", column: "CriticalityId", principalTable: "Criticalities", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
            migrationBuilder.AddForeignKey(name: "FK_Operaciones_Categories_CategoryId", table: "Operaciones", column: "CategoryId", principalTable: "Categories", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
            migrationBuilder.AddForeignKey(name: "FK_Operaciones_Vendors_ManufacturerId", table: "Operaciones", column: "ManufacturerId", principalTable: "Vendors", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
            migrationBuilder.AddForeignKey(name: "FK_Operaciones_DeviceModels_DeviceModelId", table: "Operaciones", column: "DeviceModelId", principalTable: "DeviceModels", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Operaciones_Offices_OfficeId", table: "Operaciones");
            migrationBuilder.DropForeignKey(name: "FK_Operaciones_Areas_AreaId", table: "Operaciones");
            migrationBuilder.DropForeignKey(name: "FK_Operaciones_Environments_EnvironmentId", table: "Operaciones");
            migrationBuilder.DropForeignKey(name: "FK_Operaciones_Criticalities_CriticalityId", table: "Operaciones");
            migrationBuilder.DropForeignKey(name: "FK_Operaciones_Categories_CategoryId", table: "Operaciones");
            migrationBuilder.DropForeignKey(name: "FK_Operaciones_Vendors_ManufacturerId", table: "Operaciones");
            migrationBuilder.DropForeignKey(name: "FK_Operaciones_DeviceModels_DeviceModelId", table: "Operaciones");
            migrationBuilder.DropIndex(name: "IX_Operaciones_OfficeId", table: "Operaciones");
            migrationBuilder.DropIndex(name: "IX_Operaciones_AreaId", table: "Operaciones");
            migrationBuilder.DropIndex(name: "IX_Operaciones_EnvironmentId", table: "Operaciones");
            migrationBuilder.DropIndex(name: "IX_Operaciones_CriticalityId", table: "Operaciones");
            migrationBuilder.DropIndex(name: "IX_Operaciones_CategoryId", table: "Operaciones");
            migrationBuilder.DropIndex(name: "IX_Operaciones_ManufacturerId", table: "Operaciones");
            migrationBuilder.DropIndex(name: "IX_Operaciones_DeviceModelId", table: "Operaciones");
            migrationBuilder.DropColumn(name: "OfficeId", table: "Operaciones");
            migrationBuilder.DropColumn(name: "AreaId", table: "Operaciones");
            migrationBuilder.DropColumn(name: "EnvironmentId", table: "Operaciones");
            migrationBuilder.DropColumn(name: "CriticalityId", table: "Operaciones");
            migrationBuilder.DropColumn(name: "CategoryId", table: "Operaciones");
            migrationBuilder.DropColumn(name: "ManufacturerId", table: "Operaciones");
            migrationBuilder.DropColumn(name: "DeviceModelId", table: "Operaciones");
            migrationBuilder.DropColumn(name: "IP", table: "Operaciones");
            migrationBuilder.DropColumn(name: "MAC", table: "Operaciones");
            migrationBuilder.DropColumn(name: "SistemaOperativo", table: "Operaciones");
            migrationBuilder.DropColumn(name: "TipoDispositivo", table: "Operaciones");
            migrationBuilder.DropColumn(name: "Observaciones", table: "Operaciones");
        }
    }
}
