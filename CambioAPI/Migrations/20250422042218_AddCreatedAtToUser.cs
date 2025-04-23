using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CambioAPI.Migrations
{
    
    public partial class AddCreatedAtToUser : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerLimits_Users_UpdatedByUserId",
                table: "CustomerLimits");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerLimits_Users_UserId",
                table: "CustomerLimits");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CustomerLimits",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "UpdatedByUserId",
                table: "CustomerLimits",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "CustomerLimits",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerLimits_UserId",
                table: "CustomerLimits",
                newName: "IX_CustomerLimits_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerLimits_UpdatedByUserId",
                table: "CustomerLimits",
                newName: "IX_CustomerLimits_CreatedByUserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CustomerLimitId",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "Limit",
                table: "CustomerLimits",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId1",
                table: "CustomerLimits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "CustomerLimits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastUpdatedByUserId",
                table: "CustomerLimits",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Currencies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "Currencies",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Currencies",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdate",
                table: "Currencies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Document = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    Phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsCompany = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    FromCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    ToCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    FinalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeOperations_Currencies_FromCurrencyId",
                        column: x => x.FromCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeOperations_Currencies_ToCurrencyId",
                        column: x => x.ToCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeOperations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeOperations_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CustomerLimitId",
                table: "Users",
                column: "CustomerLimitId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLimits_CustomerId1",
                table: "CustomerLimits",
                column: "CustomerId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLimits_LastUpdatedByUserId",
                table: "CustomerLimits",
                column: "LastUpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedByUserId",
                table: "Customers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Document",
                table: "Customers",
                column: "Document",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeOperations_CreatedByUserId",
                table: "ExchangeOperations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeOperations_CustomerId",
                table: "ExchangeOperations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeOperations_FromCurrencyId",
                table: "ExchangeOperations",
                column: "FromCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeOperations_ToCurrencyId",
                table: "ExchangeOperations",
                column: "ToCurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerLimits_Customers_CustomerId",
                table: "CustomerLimits",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerLimits_Customers_CustomerId1",
                table: "CustomerLimits",
                column: "CustomerId1",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerLimits_Users_CreatedByUserId",
                table: "CustomerLimits",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerLimits_Users_LastUpdatedByUserId",
                table: "CustomerLimits",
                column: "LastUpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_CustomerLimits_CustomerLimitId",
                table: "Users",
                column: "CustomerLimitId",
                principalTable: "CustomerLimits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerLimits_Customers_CustomerId",
                table: "CustomerLimits");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerLimits_Customers_CustomerId1",
                table: "CustomerLimits");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerLimits_Users_CreatedByUserId",
                table: "CustomerLimits");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerLimits_Users_LastUpdatedByUserId",
                table: "CustomerLimits");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_CustomerLimits_CustomerLimitId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ExchangeOperations");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Users_CustomerLimitId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_CustomerLimits_CustomerId1",
                table: "CustomerLimits");

            migrationBuilder.DropIndex(
                name: "IX_CustomerLimits_LastUpdatedByUserId",
                table: "CustomerLimits");

            migrationBuilder.DropIndex(
                name: "IX_Currencies_Code",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CustomerLimitId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CustomerId1",
                table: "CustomerLimits");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "CustomerLimits");

            migrationBuilder.DropColumn(
                name: "LastUpdatedByUserId",
                table: "CustomerLimits");

            migrationBuilder.DropColumn(
                name: "LastUpdate",
                table: "Currencies");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "CustomerLimits",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "CustomerLimits",
                newName: "UpdatedByUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "CustomerLimits",
                newName: "LastUpdated");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerLimits_CustomerId",
                table: "CustomerLimits",
                newName: "IX_CustomerLimits_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerLimits_CreatedByUserId",
                table: "CustomerLimits",
                newName: "IX_CustomerLimits_UpdatedByUserId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Limit",
                table: "CustomerLimits",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Currencies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "Currencies",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Currencies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerLimits_Users_UpdatedByUserId",
                table: "CustomerLimits",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerLimits_Users_UserId",
                table: "CustomerLimits",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
