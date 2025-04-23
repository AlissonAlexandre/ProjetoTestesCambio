using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CambioAPI.Migrations
{
    
    public partial class RemoveUserCustomerLimitRelationship : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_CustomerLimits_CustomerLimitId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CustomerLimitId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CustomerLimitId",
                table: "Users");
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerLimitId",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CustomerLimitId",
                table: "Users",
                column: "CustomerLimitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_CustomerLimits_CustomerLimitId",
                table: "Users",
                column: "CustomerLimitId",
                principalTable: "CustomerLimits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
