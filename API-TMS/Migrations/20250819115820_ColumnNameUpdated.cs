using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_TMS.Migrations
{
    /// <inheritdoc />
    public partial class ColumnNameUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TemaplteName",
                table: "EmailTemplates",
                newName: "TemplateName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TemplateName",
                table: "EmailTemplates",
                newName: "TemaplteName");
        }
    }
}
