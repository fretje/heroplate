#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Heroplate.Api.Infrastructure.Migrator.Migrations.Tenant;

public partial class Initial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "MultiTenancy");

        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Identifier = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                AdminEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                ValidUpto = table.Column<DateTime>(type: "datetime2", nullable: false),
                Issuer = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            schema: "MultiTenancy",
            constraints: table => table.PrimaryKey("PK_Tenants", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Tenants_Identifier",
            table: "Tenants",
            column: "Identifier",
            schema: "MultiTenancy",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Tenants",
            schema: "MultiTenancy");
    }
}