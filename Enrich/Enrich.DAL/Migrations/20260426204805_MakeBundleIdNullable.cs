using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enrich.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MakeBundleIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSession_Bundle_BundleId",
                table: "TrainingSession");

            migrationBuilder.AlterColumn<int>(
                name: "BundleId",
                table: "TrainingSession",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSession_Bundle_BundleId",
                table: "TrainingSession",
                column: "BundleId",
                principalTable: "Bundle",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingSession_Bundle_BundleId",
                table: "TrainingSession");

            migrationBuilder.AlterColumn<int>(
                name: "BundleId",
                table: "TrainingSession",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingSession_Bundle_BundleId",
                table: "TrainingSession",
                column: "BundleId",
                principalTable: "Bundle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
