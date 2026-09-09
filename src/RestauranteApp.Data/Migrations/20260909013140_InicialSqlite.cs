using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestauranteApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasMenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasMenu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meseros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    NumeroMesero = table.Column<int>(type: "INTEGER", nullable: false),
                    PinHash = table.Column<string>(type: "TEXT", nullable: false),
                    Rol = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meseros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosImpresion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenciaId = table.Column<int>(type: "INTEGER", nullable: false),
                    NombreImpresora = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    FechaIntento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Exitoso = table.Column<bool>(type: "INTEGER", nullable: false),
                    Error = table.Column<string>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosImpresion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Restaurantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    ZonaHoraria = table.Column<string>(type: "TEXT", nullable: false),
                    Moneda = table.Column<string>(type: "TEXT", nullable: false),
                    PorcentajeImpuesto = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurantes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zonas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zonas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemsMenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Estacion = table.Column<int>(type: "INTEGER", nullable: false),
                    Disponible = table.Column<bool>(type: "INTEGER", nullable: false),
                    Alergenos = table.Column<string>(type: "TEXT", nullable: true),
                    CategoriaMenuId = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsMenu_CategoriasMenu_CategoriaMenuId",
                        column: x => x.CategoriaMenuId,
                        principalTable: "CategoriasMenu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModificadoresItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    PrecioExtra = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ItemMenuId = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModificadoresItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModificadoresItem_ItemsMenu_ItemMenuId",
                        column: x => x.ItemMenuId,
                        principalTable: "ItemsMenu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comandas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdenId = table.Column<int>(type: "INTEGER", nullable: false),
                    Estacion = table.Column<int>(type: "INTEGER", nullable: false),
                    HoraEnvio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EstadoImpresion = table.Column<int>(type: "INTEGER", nullable: false),
                    IntentosImpresion = table.Column<int>(type: "INTEGER", nullable: false),
                    UltimoIntentoImpresion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UltimoErrorImpresion = table.Column<string>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comandas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cuentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdenId = table.Column<int>(type: "INTEGER", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Impuestos = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Propina = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    HoraCobro = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstadoImpresionTicket = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuentas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CuentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Metodo = table.Column<int>(type: "INTEGER", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Cuentas_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemsOrden",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdenId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemMenuId = table.Column<int>(type: "INTEGER", nullable: false),
                    NombreItem = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Notas = table.Column<string>(type: "TEXT", nullable: true),
                    Estacion = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    ModificadorIdsCsv = table.Column<string>(type: "TEXT", nullable: true),
                    ComandaId = table.Column<int>(type: "INTEGER", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsOrden", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsOrden_Comandas_ComandaId",
                        column: x => x.ComandaId,
                        principalTable: "Comandas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemsOrden_ItemsMenu_ItemMenuId",
                        column: x => x.ItemMenuId,
                        principalTable: "ItemsMenu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mesas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Capacidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    ZonaId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrdenActivaId = table.Column<int>(type: "INTEGER", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mesas_Zonas_ZonaId",
                        column: x => x.ZonaId,
                        principalTable: "Zonas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ordenes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Folio = table.Column<int>(type: "INTEGER", nullable: false),
                    MesaId = table.Column<int>(type: "INTEGER", nullable: false),
                    MeseroId = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    HoraApertura = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoraCierre = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HistorialTraspasos = table.Column<string>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordenes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ordenes_Mesas_MesaId",
                        column: x => x.MesaId,
                        principalTable: "Mesas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ordenes_Meseros_MeseroId",
                        column: x => x.MeseroId,
                        principalTable: "Meseros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comandas_OrdenId",
                table: "Comandas",
                column: "OrdenId");

            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_OrdenId",
                table: "Cuentas",
                column: "OrdenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemsMenu_CategoriaMenuId",
                table: "ItemsMenu",
                column: "CategoriaMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsOrden_ComandaId",
                table: "ItemsOrden",
                column: "ComandaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsOrden_ItemMenuId",
                table: "ItemsOrden",
                column: "ItemMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsOrden_OrdenId",
                table: "ItemsOrden",
                column: "OrdenId");

            migrationBuilder.CreateIndex(
                name: "IX_Mesas_OrdenActivaId",
                table: "Mesas",
                column: "OrdenActivaId");

            migrationBuilder.CreateIndex(
                name: "IX_Mesas_ZonaId",
                table: "Mesas",
                column: "ZonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Meseros_NumeroMesero",
                table: "Meseros",
                column: "NumeroMesero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModificadoresItem_ItemMenuId",
                table: "ModificadoresItem",
                column: "ItemMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_Folio",
                table: "Ordenes",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_MesaId",
                table: "Ordenes",
                column: "MesaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_MeseroId",
                table: "Ordenes",
                column: "MeseroId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_CuentaId",
                table: "Pagos",
                column: "CuentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comandas_Ordenes_OrdenId",
                table: "Comandas",
                column: "OrdenId",
                principalTable: "Ordenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cuentas_Ordenes_OrdenId",
                table: "Cuentas",
                column: "OrdenId",
                principalTable: "Ordenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsOrden_Ordenes_OrdenId",
                table: "ItemsOrden",
                column: "OrdenId",
                principalTable: "Ordenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Mesas_Ordenes_OrdenActivaId",
                table: "Mesas",
                column: "OrdenActivaId",
                principalTable: "Ordenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mesas_Ordenes_OrdenActivaId",
                table: "Mesas");

            migrationBuilder.DropTable(
                name: "ItemsOrden");

            migrationBuilder.DropTable(
                name: "ModificadoresItem");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "RegistrosImpresion");

            migrationBuilder.DropTable(
                name: "Restaurantes");

            migrationBuilder.DropTable(
                name: "Comandas");

            migrationBuilder.DropTable(
                name: "ItemsMenu");

            migrationBuilder.DropTable(
                name: "Cuentas");

            migrationBuilder.DropTable(
                name: "CategoriasMenu");

            migrationBuilder.DropTable(
                name: "Ordenes");

            migrationBuilder.DropTable(
                name: "Mesas");

            migrationBuilder.DropTable(
                name: "Meseros");

            migrationBuilder.DropTable(
                name: "Zonas");
        }
    }
}
