using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class Add_Table_Intern_Note : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNoteItem_Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNote_INNo",
                table: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNoteItem");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNote_TempId",
                table: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNote");

            migrationBuilder.DropColumn(
                name: "TempId",
                table: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNote");

            migrationBuilder.RenameTable(
                name: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNoteItem",
                newName: "InternNoteItem");

            migrationBuilder.RenameTable(
                name: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNote",
                newName: "InternNote");

            migrationBuilder.AlterColumn<long>(
                name: "INNo",
                table: "InternNoteItem",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "InternNoteItem",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "InternNoteItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CreatedAgent",
                table: "InternNoteItem",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "InternNoteItem",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "InternNoteItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedAgent",
                table: "InternNoteItem",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "InternNoteItem",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedUtc",
                table: "InternNoteItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "GarmentINId",
                table: "InternNoteItem",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "INVDate",
                table: "InternNoteItem",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "INVNOId",
                table: "InternNoteItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "INVName",
                table: "InternNoteItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "InternNoteItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedAgent",
                table: "InternNoteItem",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "InternNoteItem",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedUtc",
                table: "InternNoteItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "TotalAmount",
                table: "InternNoteItem",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "InternNote",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "InternNote",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CreatedAgent",
                table: "InternNote",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "InternNote",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "InternNote",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "InternNote",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyId",
                table: "InternNote",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrencyRate",
                table: "InternNote",
                type: "float",
                maxLength: 1000,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "DeletedAgent",
                table: "InternNote",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "InternNote",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedUtc",
                table: "InternNote",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "INNo",
                table: "InternNote",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "InternNote",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedAgent",
                table: "InternNote",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "InternNote",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedUtc",
                table: "InternNote",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "InternNote",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierCode",
                table: "InternNote",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierId",
                table: "InternNote",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                table: "InternNote",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UId",
                table: "InternNote",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InternNoteItem",
                table: "InternNoteItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InternNote",
                table: "InternNote",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "InternNoteDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAgent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DONo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAgent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DeletedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EPONo = table.Column<long>(type: "bigint", nullable: false),
                    GarmentDOId = table.Column<long>(type: "bigint", nullable: false),
                    GarmentINDetailId = table.Column<long>(type: "bigint", nullable: false),
                    INDetailId = table.Column<long>(type: "bigint", nullable: false),
                    INItemId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedAgent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    POSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PricePerDealUnit = table.Column<double>(type: "float", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProductId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    RONo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TermOfPayment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UnitId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UnitName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternNoteDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternNoteDetail_InternNoteItem_INItemId",
                        column: x => x.INItemId,
                        principalTable: "InternNoteItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InternNoteItem_INNo",
                table: "InternNoteItem",
                column: "INNo");

            migrationBuilder.CreateIndex(
                name: "IX_InternNoteDetail_INItemId",
                table: "InternNoteDetail",
                column: "INItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternNoteItem_InternNote_INNo",
                table: "InternNoteItem",
                column: "INNo",
                principalTable: "InternNote",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternNoteItem_InternNote_INNo",
                table: "InternNoteItem");

            migrationBuilder.DropTable(
                name: "InternNoteDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InternNoteItem",
                table: "InternNoteItem");

            migrationBuilder.DropIndex(
                name: "IX_InternNoteItem_INNo",
                table: "InternNoteItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InternNote",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "CreatedAgent",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "DeletedAgent",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "DeletedUtc",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "GarmentINId",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "INVDate",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "INVNOId",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "INVName",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "LastModifiedAgent",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "LastModifiedUtc",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "InternNoteItem");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "CreatedAgent",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "CurrencyRate",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "DeletedAgent",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "DeletedUtc",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "INNo",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "LastModifiedAgent",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "LastModifiedUtc",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "SupplierCode",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                table: "InternNote");

            migrationBuilder.DropColumn(
                name: "UId",
                table: "InternNote");

            migrationBuilder.RenameTable(
                name: "InternNoteItem",
                newName: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNoteItem");

            migrationBuilder.RenameTable(
                name: "InternNote",
                newName: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNote");

            migrationBuilder.AlterColumn<int>(
                name: "INNo",
                table: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNoteItem",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "TempId",
                table: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNote",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNote_TempId",
                table: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNote",
                column: "TempId");

            migrationBuilder.AddForeignKey(
                name: "FK_Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNoteItem_Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNote_INNo",
                table: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNoteItem",
                column: "INNo",
                principalTable: "Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel.InternNote",
                principalColumn: "TempId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
