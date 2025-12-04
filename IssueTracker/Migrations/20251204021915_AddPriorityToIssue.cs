using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssueTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityToIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Issues",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Issues");
        }
    }
}
