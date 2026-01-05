using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vinted2.Migrations
{
    /// <inheritdoc />
    public partial class AllowProductDeletionWithOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Products_ProductId",
                table: "Orders");

            // Handle PriceAtPurchase column: rename to Price if PriceAtPurchase exists and Price doesn't
            // If both exist, drop PriceAtPurchase. If only Price exists, do nothing.
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'PriceAtPurchase')
                AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'Price')
                BEGIN
                    -- Rename PriceAtPurchase to Price
                    EXEC sp_rename 'Orders.PriceAtPurchase', 'Price', 'COLUMN';
                END
                ELSE IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'PriceAtPurchase')
                AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'Price')
                BEGIN
                    -- Both columns exist, drop PriceAtPurchase
                    ALTER TABLE Orders DROP COLUMN PriceAtPurchase;
                END
            ");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Products_ProductId",
                table: "Orders",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Products_ProductId",
                table: "Orders");

            // Note: Price column rename removed from Down migration as well

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Products_ProductId",
                table: "Orders",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
