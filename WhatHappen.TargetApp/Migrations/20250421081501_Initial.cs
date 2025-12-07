using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using WhatHappen.TargetApp.Context;

#nullable disable

namespace WhatHappen.TargetApp.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Datas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    InfoValues = table.Column<List<InfoValue>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Datas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Datas");
        }
    }
}
