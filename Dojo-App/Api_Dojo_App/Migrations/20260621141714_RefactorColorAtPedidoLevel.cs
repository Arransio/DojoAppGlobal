using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_Dojo_App.Migrations
{
    /// <inheritdoc />
    public partial class RefactorColorAtPedidoLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Limpieza de datos previa al cambio de esquema ---
            // 1) Se descartan los pedidos existentes (el color pasa a vivir en
            //    PedidoItem y las filas antiguas no tienen color asignado).
            migrationBuilder.Sql("DELETE FROM PedidoItems;");
            migrationBuilder.Sql("DELETE FROM Pedidos;");

            // 2) Reseed de variantes (opción A): conservar una sola variante por
            //    (ProductId, Size) y borrar las duplicadas creadas por color.
            //    El borrado arrastra en cascada sus filas de ProductVariantColors.
            migrationBuilder.Sql(@"
                DELETE FROM ProductVariants
                WHERE Id NOT IN (
                    SELECT MIN(Id) FROM ProductVariants GROUP BY ProductId, Size
                );");

            migrationBuilder.DropTable(
                name: "ProductVariantColors");

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

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItems_PrimaryColorId",
                table: "PedidoItems",
                column: "PrimaryColorId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItems_SecondaryColorId",
                table: "PedidoItems",
                column: "SecondaryColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoItems_Colors_PrimaryColorId",
                table: "PedidoItems",
                column: "PrimaryColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoItems_Colors_SecondaryColorId",
                table: "PedidoItems",
                column: "SecondaryColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoItems_Colors_PrimaryColorId",
                table: "PedidoItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidoItems_Colors_SecondaryColorId",
                table: "PedidoItems");

            migrationBuilder.DropIndex(
                name: "IX_PedidoItems_PrimaryColorId",
                table: "PedidoItems");

            migrationBuilder.DropIndex(
                name: "IX_PedidoItems_SecondaryColorId",
                table: "PedidoItems");

            migrationBuilder.DropColumn(
                name: "PrimaryColorId",
                table: "PedidoItems");

            migrationBuilder.DropColumn(
                name: "SecondaryColorId",
                table: "PedidoItems");

            migrationBuilder.CreateTable(
                name: "ProductVariantColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ColorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductVariantId = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariantColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariantColors_Colors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Colors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductVariantColors_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantColors_ColorId",
                table: "ProductVariantColors",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantColors_ProductVariantId",
                table: "ProductVariantColors",
                column: "ProductVariantId");
        }
    }
}
