using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YugiDeck.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCardPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmazonPrice",
                table: "Cards",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CardmarketPrice",
                table: "Cards",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EbayPrice",
                table: "Cards",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TcgplayerPrice",
                table: "Cards",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmazonPrice",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "CardmarketPrice",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "EbayPrice",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "TcgplayerPrice",
                table: "Cards");
        }
    }
}
