using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIKnowledgeAssistant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChunkEmbedding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmbeddingJson",
                table: "Chunks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmbeddingJson",
                table: "Chunks");
        }
    }
}
