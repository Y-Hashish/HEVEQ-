using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HEVEQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkFieldVerificationToTicketAndMakeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "LinkedEvidenceFormId",
                table: "FieldVerificationForms",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "TicketId",
                table: "FieldVerificationForms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldVerificationForms_TicketId",
                table: "FieldVerificationForms",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_FieldVerificationForms_Tickets_TicketId",
                table: "FieldVerificationForms",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FieldVerificationForms_Tickets_TicketId",
                table: "FieldVerificationForms");

            migrationBuilder.DropIndex(
                name: "IX_FieldVerificationForms_TicketId",
                table: "FieldVerificationForms");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "FieldVerificationForms");

            migrationBuilder.AlterColumn<Guid>(
                name: "LinkedEvidenceFormId",
                table: "FieldVerificationForms",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
