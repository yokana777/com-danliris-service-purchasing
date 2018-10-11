using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class Edit_IsUsed_GarmentPurchaseRequestItem_AND_PRId_GarmentInternalPurchaseOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "GarmentPurchaseRequestItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "PRId",
                table: "GarmentInternalPurchaseOrders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "GarmentPurchaseRequestItems");

            migrationBuilder.DropColumn(
                name: "PRId",
                table: "GarmentInternalPurchaseOrders");
        }
    }
}
