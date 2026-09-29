using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AttendanceManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Matricule = table.Column<string>(type: "text", nullable: false),
                    Nom = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodeExterne = table.Column<string>(type: "text", nullable: true),
                    Nom = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoriqueActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeId = table.Column<int>(type: "integer", nullable: false),
                    DatePointage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TypeAction = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DateAction = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriqueActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pointages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DatePointage = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HeurePointage = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Porte = table.Column<string>(type: "text", nullable: true),
                    IdEquipement = table.Column<string>(type: "text", nullable: true),
                    Equipement = table.Column<string>(type: "text", nullable: false),
                    Evenement = table.Column<string>(type: "text", nullable: false),
                    HeureModifiee = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    EmployeId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pointages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pointages_Employes_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "Employes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_EmployeId",
                table: "Pointages",
                column: "EmployeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Equipements");

            migrationBuilder.DropTable(
                name: "HistoriqueActions");

            migrationBuilder.DropTable(
                name: "Pointages");

            migrationBuilder.DropTable(
                name: "Employes");
        }
    }
}
