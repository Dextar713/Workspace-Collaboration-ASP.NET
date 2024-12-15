using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discord2.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Groups_GroupId",
                table: "Memberships");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Groups_GroupId",
                table: "Memberships",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Groups_GroupId",
                table: "Memberships");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Groups_GroupId",
                table: "Memberships",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
