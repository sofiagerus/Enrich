using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Enrich.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Category", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Tag", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ThemePreference = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    LocalePreference = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_User", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "Bundle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DifficultyLevels = table.Column<string[]>(type: "text[]", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ShareCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByAdminId = table.Column<int>(type: "integer", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Bundle", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_Bundle_User_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    _ = table.ForeignKey(
                        name: "FK_Bundle_User_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            _ = migrationBuilder.CreateTable(
                name: "Word",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Term = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Translation = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Transcription = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Meaning = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PartOfSpeech = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Example = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DifficultyLevel = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    IsGlobal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatorId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Word", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_Word_User_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            _ = migrationBuilder.CreateTable(
                name: "BundleCategory",
                columns: table => new
                {
                    BundleId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_BundleCategory", x => new { x.BundleId, x.CategoryId });
                    _ = table.ForeignKey(
                        name: "FK_BundleCategory_Bundle_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_BundleCategory_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "BundleTag",
                columns: table => new
                {
                    BundleId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_BundleTag", x => new { x.BundleId, x.TagId });
                    _ = table.ForeignKey(
                        name: "FK_BundleTag_Bundle_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_BundleTag_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "TrainingSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BundleId = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    FinishedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TotalCards = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    KnownCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UnknownCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_TrainingSession", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_TrainingSession_Bundle_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_TrainingSession_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "UserBundle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BundleId = table.Column<int>(type: "integer", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_UserBundle", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_UserBundle_Bundle_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_UserBundle_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "BundleWord",
                columns: table => new
                {
                    BundleId = table.Column<int>(type: "integer", nullable: false),
                    WordId = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_BundleWord", x => new { x.BundleId, x.WordId });
                    _ = table.ForeignKey(
                        name: "FK_BundleWord_Bundle_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_BundleWord_Word_WordId",
                        column: x => x.WordId,
                        principalTable: "Word",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "UserWord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WordId = table.Column<int>(type: "integer", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_UserWord", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_UserWord_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_UserWord_Word_WordId",
                        column: x => x.WordId,
                        principalTable: "Word",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "WordCategory",
                columns: table => new
                {
                    WordId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_WordCategory", x => new { x.WordId, x.CategoryId });
                    _ = table.ForeignKey(
                        name: "FK_WordCategory_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_WordCategory_Word_WordId",
                        column: x => x.WordId,
                        principalTable: "Word",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "WordProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WordId = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsLearned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_WordProgress", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_WordProgress_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_WordProgress_Word_WordId",
                        column: x => x.WordId,
                        principalTable: "Word",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "WordTag",
                columns: table => new
                {
                    WordId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_WordTag", x => new { x.WordId, x.TagId });
                    _ = table.ForeignKey(
                        name: "FK_WordTag_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_WordTag_Word_WordId",
                        column: x => x.WordId,
                        principalTable: "Word",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "SessionResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    WordId = table.Column<int>(type: "integer", nullable: false),
                    IsKnown = table.Column<bool>(type: "boolean", nullable: false),
                    PointsAwarded = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_SessionResult", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_SessionResult_TrainingSession_SessionId",
                        column: x => x.SessionId,
                        principalTable: "TrainingSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_SessionResult_Word_WordId",
                        column: x => x.WordId,
                        principalTable: "Word",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            _ = migrationBuilder.CreateIndex(
                name: "IX_Bundle_OwnerId",
                table: "Bundle",
                column: "OwnerId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Bundle_ReviewedByAdminId",
                table: "Bundle",
                column: "ReviewedByAdminId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Bundle_ShareCode",
                table: "Bundle",
                column: "ShareCode",
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_BundleCategory_CategoryId",
                table: "BundleCategory",
                column: "CategoryId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_BundleTag_TagId",
                table: "BundleTag",
                column: "TagId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_BundleWord_WordId",
                table: "BundleWord",
                column: "WordId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Category_Name",
                table: "Category",
                column: "Name",
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_SessionResult_SessionId",
                table: "SessionResult",
                column: "SessionId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_SessionResult_WordId",
                table: "SessionResult",
                column: "WordId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Tag_Name",
                table: "Tag",
                column: "Name",
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_TrainingSession_BundleId",
                table: "TrainingSession",
                column: "BundleId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_TrainingSession_UserId",
                table: "TrainingSession",
                column: "UserId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_UserBundle_BundleId",
                table: "UserBundle",
                column: "BundleId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_UserBundle_UserId_BundleId",
                table: "UserBundle",
                columns: ["UserId", "BundleId"],
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_UserWord_UserId_WordId",
                table: "UserWord",
                columns: ["UserId", "WordId"],
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_UserWord_WordId",
                table: "UserWord",
                column: "WordId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Word_CreatorId",
                table: "Word",
                column: "CreatorId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_WordCategory_CategoryId",
                table: "WordCategory",
                column: "CategoryId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_WordProgress_UserId_WordId",
                table: "WordProgress",
                columns: ["UserId", "WordId"],
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_WordProgress_WordId",
                table: "WordProgress",
                column: "WordId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_WordTag_TagId",
                table: "WordTag",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "BundleCategory");

            _ = migrationBuilder.DropTable(
                name: "BundleTag");

            _ = migrationBuilder.DropTable(
                name: "BundleWord");

            _ = migrationBuilder.DropTable(
                name: "SessionResult");

            _ = migrationBuilder.DropTable(
                name: "UserBundle");

            _ = migrationBuilder.DropTable(
                name: "UserWord");

            _ = migrationBuilder.DropTable(
                name: "WordCategory");

            _ = migrationBuilder.DropTable(
                name: "WordProgress");

            _ = migrationBuilder.DropTable(
                name: "WordTag");

            _ = migrationBuilder.DropTable(
                name: "TrainingSession");

            _ = migrationBuilder.DropTable(
                name: "Category");

            _ = migrationBuilder.DropTable(
                name: "Tag");

            _ = migrationBuilder.DropTable(
                name: "Word");

            _ = migrationBuilder.DropTable(
                name: "Bundle");

            _ = migrationBuilder.DropTable(
                name: "User");
        }
    }
}
