using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchasingDocumentExpeditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CashierDivisionBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CashierDivisionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Division = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinanceDivisionBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinanceDivisionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Position = table.Column<int>(type: "int", nullable: false),
                    SendToCashierDivisionBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendToCashierDivisionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SendToFinanceDivisionBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendToFinanceDivisionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SendToPurchasingDivisionBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendToPurchasingDivisionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SendToVerificationDivisionBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendToVerificationDivisionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitPaymentOrderNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    VerificationDivisionBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerificationDivisionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    _CreatedAgent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    _CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    _CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    _DeletedAgent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    _DeletedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    _DeletedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    _LastModifiedAgent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    _LastModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    _LastModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchasingDocumentExpeditions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchasingDocumentExpeditions");
        }
    }
}
