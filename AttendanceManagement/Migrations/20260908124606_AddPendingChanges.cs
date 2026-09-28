using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pointages_Equipements_EquipementId",
                table: "Pointages");

            migrationBuilder.DropIndex(
                name: "IX_Pointages_EquipementId",
                table: "Pointages");

            migrationBuilder.DropColumn(
                name: "EquipementId",
                table: "Pointages");

            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Employes");

            migrationBuilder.DropColumn(
                name: "Prenom",
                table: "Employes");

            migrationBuilder.DropColumn(
                name: "Service",
                table: "Employes");

            migrationBuilder.AddColumn<string>(
                name: "Equipement",
                table: "Pointages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HeureModifiee",
                table: "Pointages",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "CodeExterne",
                table: "Equipements",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Equipement",
                table: "Pointages");

            migrationBuilder.DropColumn(
                name: "HeureModifiee",
                table: "Pointages");

            migrationBuilder.DropColumn(
                name: "CodeExterne",
                table: "Equipements");

            migrationBuilder.AddColumn<int>(
                name: "EquipementId",
                table: "Pointages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Employes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Prenom",
                table: "Employes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Service",
                table: "Employes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pointages_EquipementId",
                table: "Pointages",
                column: "EquipementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pointages_Equipements_EquipementId",
                table: "Pointages",
                column: "EquipementId",
                principalTable: "Equipements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
