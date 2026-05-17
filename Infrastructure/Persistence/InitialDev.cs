using Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Persistence;

public partial class InitialDev : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// DEV-ONLY: start from a clean slate.
		migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Users"" CASCADE");
		migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Units"" CASCADE;");
		migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Properties"" CASCADE");
		migrationBuilder.Sql(@"DROP TYPE IF EXISTS t_user_role CASCADE");
		migrationBuilder.Sql(@"DROP TYPE IF EXISTS t_technician_capability CASCADE");

		// Re-create enum types
		migrationBuilder.AlterDatabase()
			.Annotation("Npgsql:Enum:t_user_role", "manager,technician,tenant")
			.Annotation("Npgsql:Enum:t_technician_capability", "dry_wall,electrical,general,hvac,machinery,painting,plumbing");

		// Properties
		migrationBuilder.CreateTable(
			name: "Properties",
			columns: table => new
			{
				id = table.Column<Guid>(type: "uuid", nullable: false),
				name = table.Column<string>(type: "text", nullable: false),
				street_name = table.Column<string>(type: "text", nullable: false),
				street_number = table.Column<int>(type: "integer", nullable: false),
				city = table.Column<string>(type: "text", nullable: false),
				postal_code = table.Column<string>(type: "text", nullable: false),
				country = table.Column<string>(type: "text", nullable: false),
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Properties", x => x.id);
			}
		);

		migrationBuilder.CreateTable(
			name: "Units",
			columns: table => new
			{
				id = table.Column<Guid>(type: "uuid", nullable: false),
				property_id = table.Column<Guid>(type: "uuid", nullable: false),
				number = table.Column<int>(type: "integer", nullable: false),
				bedrooms = table.Column<int>(type: "integer", nullable: false),
				bathrooms = table.Column<int>(type: "integer", nullable: false),
				area_m2 = table.Column<float>(type: "real", nullable: false),
				monthly_rate = table.Column<decimal>(type: "numberic(12,2)", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Units", x => x.id);
				table.ForeignKey(
					name: "FK_Units_Properties",
					column: x => x.property_id,
					principalTable: "Properties",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade
				);
			}
		);

		migrationBuilder.CreateTable(
			name: "Users",
			columns: table => new
			{
				id = table.Column<Guid>(type: "uuid", nullable: false),
				
				// Native Postgres enum discriminator column (UserRole)
				role = table.Column<UserRole>(type: "t_user_role", nullable: false),
				
				first_name = table.Column<string>(type: "text", nullable: false),
				last_name = table.Column<string>(type: "text", nullable: false),
				phone_number = table.Column<string>(type: "text", nullable: false),
				hashed_password = table.Column<string>(type: "text", nullable: false),
				created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				
				// Shadow FK to Property (unidirectional Property.Users)
				property_id = table.Column<Guid>(type: "uuid", nullable: true),
				
				// Tenant -> Unit FK sits here (only meaningful for tenant rows).
				unit_id = table.Column<Guid>(type: "uuid", nullable: true),

				// Manager/Technician fields (meaningful for those rows).
				hourly_rate = table.Column<decimal>(type: "numeric(10,2)", nullable: true),

				// Technician capabilities as native Postgres enum array
				capabilities = table.Column<TechnicianCapability[]>(type: "t_technician_capability[]", nullable: true),

				// Tenant fields
				moved_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Users", x => x.id);

				table.ForeignKey(
					name: "FK_Users_Properties",
					column: x => x.property_id,
					principalTable: "Properties",
					principalColumn: "Id",
					onDelete: ReferentialAction.Restrict
				);

				table.ForeignKey(
					name: "FK_Users_Units",
					column: x => x.unit_id,
					principalTable: "Units",
					principalColumn: "id",
					onDelete: ReferentialAction.Restrict
				);
			}
		);

		migrationBuilder.CreateIndex(
			name: "IX_Units_PropertyId",
			table: "Units",
			column: "property_id"
		);

		migrationBuilder.CreateIndex(
			name: "IX_Users_Email",
			table: "Users",
			column: "email",
			unique: true
		);

		migrationBuilder.CreateIndex(
			name: "IX_Users_PropertyId",
			table: "Users",
			column: "property_id"
		);

		// Enforce 1:1 Tenant <-> Unit by unique UnitId
		// (null allowed for non-tenant rows)
		migrationBuilder.CreateIndex(
			name: "IX_Users_UnitId",
			table: "Users",
			column: "unit_id",
			unique: true
		);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		// Revert schema changes.
		migrationBuilder.DropTable(name: "Users");
		migrationBuilder.DropTable(name: "Units");
		migrationBuilder.DropTable(name: "Properties");

		// Drop enum types
		migrationBuilder.Sql(@"DROP TYPE IF EXISTS t_user_role CASCADE");
		migrationBuilder.Sql(@"DROP TYPE IF EXISTS t_technician_capability CASCADE");
	}
}