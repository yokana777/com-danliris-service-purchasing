using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class Add_RemainingBudget_GarmentInternalPurchaseOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpectedDeliveryDate",
                table: "GarmentInternalPurchaseOrders",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset));

            migrationBuilder.AddColumn<double>(
                name: "RemainingBudget",
                table: "GarmentInternalPurchaseOrderItems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingBudget",
                table: "GarmentInternalPurchaseOrderItems");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ExpectedDeliveryDate",
                table: "GarmentInternalPurchaseOrders",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);
        }
    }
}
