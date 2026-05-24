using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IITS.Migrations
{
    public partial class AddAssetManagedAccountCatalogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "Areas", columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            }, constraints: table => table.PrimaryKey("PK_Areas", x => x.Id));

            migrationBuilder.CreateTable(name: "Offices", columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
            }, constraints: table => table.PrimaryKey("PK_Offices", x => x.Id));

            migrationBuilder.CreateTable(name: "Environments", columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            }, constraints: table => table.PrimaryKey("PK_Environments", x => x.Id));

            migrationBuilder.CreateTable(name: "Criticalities", columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            }, constraints: table => table.PrimaryKey("PK_Criticalities", x => x.Id));

            migrationBuilder.CreateTable(name: "Categories", columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            }, constraints: table => table.PrimaryKey("PK_Categories", x => x.Id));

            migrationBuilder.CreateTable(name: "Vendors", columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
            }, constraints: table => table.PrimaryKey("PK_Vendors", x => x.Id));

            migrationBuilder.CreateTable(name: "DeviceModels", columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ManufacturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_DeviceModels", x => x.Id);
                table.ForeignKey(name: "FK_DeviceModels_Vendors_ManufacturerId", column: x => x.ManufacturerId, principalTable: "Vendors", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            });
            migrationBuilder.CreateIndex(name: "IX_DeviceModels_ManufacturerId", table: "DeviceModels", column: "ManufacturerId");

            migrationBuilder.CreateTable(name: "Assets", columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OfficeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DeviceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Hostname = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                OperationEnvironmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                OwnerAreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CriticalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                EnvironmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ManufacturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeviceModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ApprovalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Assets", x => x.Id);
                table.ForeignKey(name: "FK_Assets_Offices_OfficeId", column: x => x.OfficeId, principalTable: "Offices", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "FK_Assets_Areas_AreaId", column: x => x.AreaId, principalTable: "Areas", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "FK_Assets_Estatus_StatusId", column: x => x.StatusId, principalTable: "Estatus", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "FK_Assets_Environments_OperationEnvironmentId", column: x => x.OperationEnvironmentId, principalTable: "Environments", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
                table.ForeignKey(name: "FK_Assets_Areas_OwnerAreaId", column: x => x.OwnerAreaId, principalTable: "Areas", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
                table.ForeignKey(name: "FK_Assets_Criticalities_CriticalityId", column: x => x.CriticalityId, principalTable: "Criticalities", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
                table.ForeignKey(name: "FK_Assets_Environments_EnvironmentId", column: x => x.EnvironmentId, principalTable: "Environments", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
                table.ForeignKey(name: "FK_Assets_Categories_CategoryId", column: x => x.CategoryId, principalTable: "Categories", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
                table.ForeignKey(name: "FK_Assets_Vendors_ManufacturerId", column: x => x.ManufacturerId, principalTable: "Vendors", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
                table.ForeignKey(name: "FK_Assets_DeviceModels_DeviceModelId", column: x => x.DeviceModelId, principalTable: "DeviceModels", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
            });
            migrationBuilder.CreateIndex(name: "IX_Assets_OfficeId", table: "Assets", column: "OfficeId");
            migrationBuilder.CreateIndex(name: "IX_Assets_AreaId", table: "Assets", column: "AreaId");
            migrationBuilder.CreateIndex(name: "IX_Assets_StatusId", table: "Assets", column: "StatusId");
            migrationBuilder.CreateIndex(name: "IX_Assets_OperationEnvironmentId", table: "Assets", column: "OperationEnvironmentId");
            migrationBuilder.CreateIndex(name: "IX_Assets_OwnerAreaId", table: "Assets", column: "OwnerAreaId");
            migrationBuilder.CreateIndex(name: "IX_Assets_CriticalityId", table: "Assets", column: "CriticalityId");
            migrationBuilder.CreateIndex(name: "IX_Assets_EnvironmentId", table: "Assets", column: "EnvironmentId");
            migrationBuilder.CreateIndex(name: "IX_Assets_CategoryId", table: "Assets", column: "CategoryId");
            migrationBuilder.CreateIndex(name: "IX_Assets_ManufacturerId", table: "Assets", column: "ManufacturerId");
            migrationBuilder.CreateIndex(name: "IX_Assets_DeviceModelId", table: "Assets", column: "DeviceModelId");

            migrationBuilder.CreateTable(name: "ManagedAccounts", columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Responsible = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                AccountType = table.Column<int>(type: "int", nullable: false),
                Origin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                RelatedService = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                ChangeConfigType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ChangeIntervalDays = table.Column<int>(type: "int", nullable: true),
                EstatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ManagedAccounts", x => x.Id);
                table.ForeignKey(name: "FK_ManagedAccounts_Areas_AreaId", column: x => x.AreaId, principalTable: "Areas", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
                table.ForeignKey(name: "FK_ManagedAccounts_Estatus_EstatusId", column: x => x.EstatusId, principalTable: "Estatus", principalColumn: "Id", onDelete: ReferentialAction.NoAction);
            });
            migrationBuilder.CreateIndex(name: "IX_ManagedAccounts_AreaId", table: "ManagedAccounts", column: "AreaId");
            migrationBuilder.CreateIndex(name: "IX_ManagedAccounts_EstatusId", table: "ManagedAccounts", column: "EstatusId");

            migrationBuilder.CreateTable(name: "ManagedAccountSecurityGroups", columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ManagedAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                GroupName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ManagedAccountSecurityGroups", x => x.Id);
                table.ForeignKey(name: "FK_ManagedAccountSecurityGroups_ManagedAccounts_ManagedAccountId", column: x => x.ManagedAccountId, principalTable: "ManagedAccounts", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateIndex(name: "IX_ManagedAccountSecurityGroups_ManagedAccountId", table: "ManagedAccountSecurityGroups", column: "ManagedAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ManagedAccountSecurityGroups");
            migrationBuilder.DropTable(name: "ManagedAccounts");
            migrationBuilder.DropTable(name: "Assets");
            migrationBuilder.DropTable(name: "DeviceModels");
            migrationBuilder.DropTable(name: "Vendors");
            migrationBuilder.DropTable(name: "Categories");
            migrationBuilder.DropTable(name: "Criticalities");
            migrationBuilder.DropTable(name: "Environments");
            migrationBuilder.DropTable(name: "Offices");
            migrationBuilder.DropTable(name: "Areas");
        }
    }
}
