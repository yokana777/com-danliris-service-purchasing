using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class Alter_GarmentPR_IsValidate_to_IsValidated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValidate",
                table: "GarmentPurchaseRequests");

            migrationBuilder.AddColumn<bool>(
                name: "IsValidated",
                table: "GarmentPurchaseRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValidated",
                table: "GarmentPurchaseRequests");

            migrationBuilder.AddColumn<bool>(
                name: "IsValidate",
                table: "GarmentPurchaseRequests",
                nullable: false,
                defaultValue: false);
        }
    }
}
