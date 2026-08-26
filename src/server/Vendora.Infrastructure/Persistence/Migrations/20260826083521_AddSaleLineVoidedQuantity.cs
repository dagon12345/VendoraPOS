using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendora.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleLineVoidedQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VoidedQuantity",
                table: "SaleLines",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoidedQuantity",
                table: "SaleLines");
        }
    }
}
