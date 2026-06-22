using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_Dojo_App.Migrations
{
    /// <inheritdoc />
    public partial class AddColorsToOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrimaryColorId",
                table: "PedidoItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SecondaryColorId",
                table: "PedidoItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryColorId",
                table: "PedidoItems");

            migrationBuilder.DropColumn(
                name: "SecondaryColorId",
                table: "PedidoItems");
        }
    }
}
