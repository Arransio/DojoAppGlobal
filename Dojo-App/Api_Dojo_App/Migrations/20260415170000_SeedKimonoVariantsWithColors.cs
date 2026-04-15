using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api_Dojo_App.Migrations
{
    /// <inheritdoc />
    public partial class SeedKimonoVariantsWithColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Listas base
            var sizes = new[] { "A1", "A2", "A3", "A4" };
            var secondaryColors = new[] { 1, 2, 3, 4, 5, 6, 7 }; // todos los colores
            int primaryColor = 1; // negro

            int variantId = 1;
            int variantColorId = 1;

            foreach (var size in sizes)
            {
                foreach (var secondary in secondaryColors)
                {
                    // Insertar ProductVariant
                    migrationBuilder.InsertData(
                        table: "ProductVariants",
                        columns: new[] { "Id", "ProductId", "Size" },
                        values: new object[] { variantId, 1, size }
                    );

                    // Insertar Color Primario (Negro)
                    migrationBuilder.InsertData(
                        table: "ProductVariantColors",
                        columns: new[] { "Id", "ProductVariantId", "ColorId", "Role" },
                        values: new object[] { variantColorId, variantId, primaryColor, "Primary" }
                    );
                    variantColorId++;

                    // Insertar Color Secundario
                    migrationBuilder.InsertData(
                        table: "ProductVariantColors",
                        columns: new[] { "Id", "ProductVariantId", "ColorId", "Role" },
                        values: new object[] { variantColorId, variantId, secondary, "Secondary" }
                    );
                    variantColorId++;

                    variantId++;
                }
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar todos los ProductVariants con ProductId = 1 (Kimono)
            migrationBuilder.DeleteData(
                table: "ProductVariantColors",
                keyColumn: "Id",
                keyValues: Enumerable.Range(1, 4 * 7 * 2).Cast<object>().ToArray()
            );

            migrationBuilder.DeleteData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValues: Enumerable.Range(1, 4 * 7).Cast<object>().ToArray()
            );
        }
    }
}
