using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_Dojo_App.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerNameToPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Pedidos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Pedidos");
        }
    }
}
