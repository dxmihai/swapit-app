using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vinted2.Migrations
{
    /// <inheritdoc />
    public partial class PricePurchaseRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraint first
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Products_ProductId",
                table: "Orders");

            // Rename PriceAtPurchase to Price if PriceAtPurchase exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'PriceAtPurchase')
                AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'Price')
                BEGIN
                    EXEC sp_rename 'Orders.PriceAtPurchase', 'Price', 'COLUMN';
                END
                ELSE IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'PriceAtPurchase')
                AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'Price')
                BEGIN
                    -- Both exist, drop PriceAtPurchase
                    ALTER TABLE Orders DROP COLUMN PriceAtPurchase;
                END
            ");

            // Make ProductId nullable to allow product deletion while keeping orders
            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Re-add foreign key with SetNull delete behavior
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
            // Drop foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Products_ProductId",
                table: "Orders");

            // Make ProductId not nullable again
            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Re-add foreign key with Restrict delete behavior
            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Products_ProductId",
                table: "Orders",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Reverse: rename Price back to PriceAtPurchase if needed
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'Price')
                AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'PriceAtPurchase')
                BEGIN
                    EXEC sp_rename 'Orders.Price', 'PriceAtPurchase', 'COLUMN';
                END
            ");
        }
    }
}
