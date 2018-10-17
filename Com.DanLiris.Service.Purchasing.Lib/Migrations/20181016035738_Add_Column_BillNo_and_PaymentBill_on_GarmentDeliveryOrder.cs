using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class Add_Column_BillNo_and_PaymentBill_on_GarmentDeliveryOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "CustomsId",
                table: "GarmentDeliveryOrders",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillNo",
                table: "GarmentDeliveryOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentBill",
                table: "GarmentDeliveryOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillNo",
                table: "GarmentDeliveryOrders");

            migrationBuilder.DropColumn(
                name: "PaymentBill",
                table: "GarmentDeliveryOrders");

            migrationBuilder.AlterColumn<string>(
                name: "CustomsId",
                table: "GarmentDeliveryOrders",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
