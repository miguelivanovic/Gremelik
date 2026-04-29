using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Becas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Porcentaje = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontoFijo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AplicaEnInscripcion = table.Column<bool>(type: "boolean", nullable: false),
                    AplicaEnColegiatura = table.Column<bool>(type: "boolean", nullable: false),
                    ReglaHermano = table.Column<int>(type: "integer", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Becas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConceptosPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Frecuencia = table.Column<int>(type: "integer", nullable: false),
                    AplicaBeca = table.Column<bool>(type: "boolean", nullable: false),
                    Obligatorio = table.Column<bool>(type: "boolean", nullable: false),
                    PlantelId = table.Column<Guid>(type: "uuid", nullable: true),
                    NivelEducativoId = table.Column<Guid>(type: "uuid", nullable: true),
                    GradoId = table.Column<int>(type: "integer", nullable: true),
                    NombrePlantel = table.Column<string>(type: "text", nullable: true),
                    NombreNivel = table.Column<string>(type: "text", nullable: true),
                    NombreGrado = table.Column<string>(type: "text", nullable: true),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EsFacturable = table.Column<bool>(type: "boolean", nullable: false),
                    GeneraRecargos = table.Column<bool>(type: "boolean", nullable: false),
                    ClaveProdServ = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ClaveUnidad = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ObjetoImpuesto = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    EsExentoIva = table.Column<bool>(type: "boolean", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptosPago", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesRecargo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreConcepto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DiasGracia = table.Column<int>(type: "integer", nullable: false),
                    MontoFijo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    AplicaIva = table.Column<bool>(type: "boolean", nullable: false),
                    IvaIncluido = table.Column<bool>(type: "boolean", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesRecargo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Escuelas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Subdominio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FondoLoginUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ColorPrimario = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    ColorSecundario = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Slogan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SitioWeb = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escuelas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FichasMedicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoSangre = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Alergias = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NombreContactoEmergencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TelefonoContactoEmergencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichasMedicas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RelacionAlumnoTutor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Parentesco = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelacionAlumnoTutor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tutores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrimerApellido = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SegundoApellido = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CorreoElectronico = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TelefonoMovil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DireccionFisica = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RFC = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    RegimenFiscal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CodigoPostal = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    UsoCFDI = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tutores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    NombreCompleto = table.Column<string>(type: "text", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanesPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NumeroPagos = table.Column<int>(type: "integer", nullable: false),
                    DiaLimitePago = table.Column<int>(type: "integer", nullable: false),
                    MesInicioCobro = table.Column<int>(type: "integer", nullable: false),
                    MesesDobleCobro = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecargoMonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RecargoPorcentaje = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    ConceptoPagoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanesPago_ConceptosPago_ConceptoPagoId",
                        column: x => x.ConceptoPagoId,
                        principalTable: "ConceptosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alumnos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    PrimerApellido = table.Column<string>(type: "text", nullable: false),
                    SegundoApellido = table.Column<string>(type: "text", nullable: true),
                    Matricula = table.Column<string>(type: "text", nullable: false),
                    CURP = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    NIA = table.Column<string>(type: "text", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sexo = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estatus = table.Column<int>(type: "integer", nullable: false),
                    SaldoAFavor = table.Column<decimal>(type: "numeric", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumnos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alumnos_Escuelas_EscuelaId",
                        column: x => x.EscuelaId,
                        principalTable: "Escuelas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CiclosEscolares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estatus = table.Column<int>(type: "integer", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CiclosEscolares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CiclosEscolares_Escuelas_EscuelaId",
                        column: x => x.EscuelaId,
                        principalTable: "Escuelas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Planteles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Calle = table.Column<string>(type: "text", nullable: false),
                    Numero = table.Column<string>(type: "text", nullable: false),
                    Colonia = table.Column<string>(type: "text", nullable: false),
                    CodigoPostal = table.Column<string>(type: "text", nullable: false),
                    Municipio = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    CCT = table.Column<string>(type: "text", nullable: false),
                    ZonaEscolar = table.Column<string>(type: "text", nullable: false),
                    JefaturaSector = table.Column<string>(type: "text", nullable: false),
                    RazonSocial = table.Column<string>(type: "text", nullable: false),
                    RFC = table.Column<string>(type: "text", nullable: false),
                    CodigoPostalFiscal = table.Column<string>(type: "text", nullable: false),
                    RegimenFiscal = table.Column<string>(type: "text", nullable: false),
                    EsMatriz = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planteles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Planteles_Escuelas_EscuelaId",
                        column: x => x.EscuelaId,
                        principalTable: "Escuelas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CuentasPorCobrar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    ConceptoNombre = table.Column<string>(type: "text", nullable: false),
                    ConceptoPagoId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MontoBase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DescuentoBeca = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RecargosAcumulados = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPagado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    NumeroDePago = table.Column<int>(type: "integer", nullable: false),
                    BecaId = table.Column<Guid>(type: "uuid", nullable: true),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EsFacturable = table.Column<bool>(type: "boolean", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasPorCobrar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_ConceptosPago_ConceptoPagoId",
                        column: x => x.ConceptoPagoId,
                        principalTable: "ConceptosPago",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Folio = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPagado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DineroRecibido = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Cambio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MetodoPago = table.Column<int>(type: "integer", nullable: false),
                    Comentarios = table.Column<string>(type: "text", nullable: true),
                    RequiereFactura = table.Column<bool>(type: "boolean", nullable: false),
                    TutorId = table.Column<Guid>(type: "uuid", nullable: true),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    Banco = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TerminacionTarjeta = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    Autorizacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagos_Tutores_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Tutores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReglasDescuento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontoFijo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaInicioValidez = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaFinValidez = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasDescuento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglasDescuento_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportesConducta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    FechaIncidencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportadoPorId = table.Column<string>(type: "text", nullable: false),
                    NombreReportador = table.Column<string>(type: "text", nullable: false),
                    Gravedad = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    AccionTomada = table.Column<string>(type: "text", nullable: true),
                    Estatus = table.Column<int>(type: "integer", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesConducta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportesConducta_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportesConducta_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NivelesEducativos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    PlantelId = table.Column<Guid>(type: "uuid", nullable: false),
                    RVOE = table.Column<string>(type: "text", nullable: false),
                    FechaRVOE = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NivelSAT = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NivelesEducativos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NivelesEducativos_Planteles_PlantelId",
                        column: x => x.PlantelId,
                        principalTable: "Planteles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosPlanteles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<string>(type: "text", nullable: false),
                    PlantelId = table.Column<Guid>(type: "uuid", nullable: false),
                    EsCoordinador = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosPlanteles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosPlanteles_Planteles_PlantelId",
                        column: x => x.PlantelId,
                        principalTable: "Planteles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosPlanteles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesPagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PagoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CuentaPorCobrarId = table.Column<Guid>(type: "uuid", nullable: false),
                    MontoAbonado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ConceptoNombreSnapshot = table.Column<string>(type: "text", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesPagos_CuentasPorCobrar_CuentaPorCobrarId",
                        column: x => x.CuentaPorCobrarId,
                        principalTable: "CuentasPorCobrar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesPagos_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExcepcionesCaja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PagoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CuentaPorCobrarId = table.Column<Guid>(type: "uuid", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    BecaRestauradaMonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RecargoPerdonadoMonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcepcionesCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcepcionesCaja_CuentasPorCobrar_CuentaPorCobrarId",
                        column: x => x.CuentaPorCobrarId,
                        principalTable: "CuentasPorCobrar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExcepcionesCaja_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PagoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MetodoPagoSAT = table.Column<string>(type: "text", nullable: false),
                    FormaPagoSAT = table.Column<string>(type: "text", nullable: false),
                    Estatus = table.Column<string>(type: "text", nullable: false),
                    XmlCrudo = table.Column<string>(type: "text", nullable: false),
                    Uuid = table.Column<string>(type: "text", nullable: true),
                    XmlTimbrado = table.Column<string>(type: "text", nullable: true),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facturas_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facturas_Tutores_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Tutores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransaccionesBancarias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Banco = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferenciaBancaria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClaveRastreo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Estatus = table.Column<int>(type: "integer", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: true),
                    PagoGeneradoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaccionesBancarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransaccionesBancarias_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransaccionesBancarias_Escuelas_EscuelaId",
                        column: x => x.EscuelaId,
                        principalTable: "Escuelas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransaccionesBancarias_Pagos_PagoGeneradoId",
                        column: x => x.PagoGeneradoId,
                        principalTable: "Pagos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesAcademicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NivelEducativoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoPeriodoInterno = table.Column<int>(type: "integer", nullable: false),
                    UsaDecimales = table.Column<bool>(type: "boolean", nullable: false),
                    CalificacionAprobatoria = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    EscalaMinima = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    EscalaMaxima = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesAcademicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesAcademicas_NivelesEducativos_NivelEducativoId",
                        column: x => x.NivelEducativoId,
                        principalTable: "NivelesEducativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Grados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    NivelEducativoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grados_NivelesEducativos_NivelEducativoId",
                        column: x => x.NivelEducativoId,
                        principalTable: "NivelesEducativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosInternos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    NivelEducativoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrimestreSEP = table.Column<int>(type: "integer", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    AbiertoParaCaptura = table.Column<bool>(type: "boolean", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosInternos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodosInternos_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosInternos_NivelesEducativos_NivelEducativoId",
                        column: x => x.NivelEducativoId,
                        principalTable: "NivelesEducativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CostosInscripcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    NivelEducativoId = table.Column<Guid>(type: "uuid", nullable: true),
                    GradoId = table.Column<int>(type: "integer", nullable: true),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Concepto = table.Column<string>(type: "text", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostosInscripcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostosInscripcion_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CostosInscripcion_Grados_GradoId",
                        column: x => x.GradoId,
                        principalTable: "Grados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CostosInscripcion_NivelesEducativos_NivelEducativoId",
                        column: x => x.NivelEducativoId,
                        principalTable: "NivelesEducativos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Turno = table.Column<string>(type: "text", nullable: false),
                    CupoMaximo = table.Column<int>(type: "integer", nullable: false),
                    GradoId = table.Column<int>(type: "integer", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    MaestroTutorId = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grupos_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Grupos_Grados_GradoId",
                        column: x => x.GradoId,
                        principalTable: "Grados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Grupos_Usuarios_MaestroTutorId",
                        column: x => x.MaestroTutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Inscripciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupoId = table.Column<int>(type: "integer", nullable: true),
                    GradoId = table.Column<int>(type: "integer", nullable: true),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    PlantelId = table.Column<Guid>(type: "uuid", nullable: false),
                    MontoBase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontoFinal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReglaDescuentoId = table.Column<Guid>(type: "uuid", nullable: true),
                    MotivoDescuentoManual = table.Column<string>(type: "text", nullable: true),
                    EsNuevoIngreso = table.Column<bool>(type: "boolean", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inscripciones_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Grados_GradoId",
                        column: x => x.GradoId,
                        principalTable: "Grados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Inscripciones_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Planteles_PlantelId",
                        column: x => x.PlantelId,
                        principalTable: "Planteles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inscripciones_ReglasDescuento_ReglaDescuentoId",
                        column: x => x.ReglaDescuentoId,
                        principalTable: "ReglasDescuento",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    ClaveOficial = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CampoFormativo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PlantelId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradoId = table.Column<int>(type: "integer", nullable: true),
                    GrupoId = table.Column<int>(type: "integer", nullable: true),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materias_Grados_GradoId",
                        column: x => x.GradoId,
                        principalTable: "Grados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Materias_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Materias_Planteles_PlantelId",
                        column: x => x.PlantelId,
                        principalTable: "Planteles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesMaestros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaestroId = table.Column<string>(type: "text", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesMaestros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesMaestros_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsignacionesMaestros_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionesMaestros_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Asistencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: true),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estatus = table.Column<int>(type: "integer", nullable: false),
                    Comentarios = table.Column<string>(type: "text", nullable: true),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistencias_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Asistencias_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Asistencias_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistencias_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BitacorasAsistencia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaestroId = table.Column<string>(type: "text", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacorasAsistencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BitacorasAsistencia_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BitacorasAsistencia_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalificacionesInternas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodoInternoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nota = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionesInternas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalificacionesInternas_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesInternas_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesInternas_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesInternas_PeriodosInternos_PeriodoInternoId",
                        column: x => x.PeriodoInternoId,
                        principalTable: "PeriodosInternos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalificacionesSEP",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "integer", nullable: false),
                    Trimestre = table.Column<int>(type: "integer", nullable: false),
                    PromedioSugerido = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    NotaFinal = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionesSEP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalificacionesSEP_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesSEP_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesSEP_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesSEP_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Observaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUM = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Observaciones_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Observaciones_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Observaciones_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alumnos_CURP_EscuelaId",
                table: "Alumnos",
                columns: new[] { "CURP", "EscuelaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alumnos_EscuelaId",
                table: "Alumnos",
                column: "EscuelaId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesMaestros_CicloEscolarId",
                table: "AsignacionesMaestros",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesMaestros_GrupoId",
                table: "AsignacionesMaestros",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesMaestros_MateriaId",
                table: "AsignacionesMaestros",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_AlumnoId",
                table: "Asistencias",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_CicloEscolarId",
                table: "Asistencias",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_GrupoId",
                table: "Asistencias",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_MateriaId",
                table: "Asistencias",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BitacorasAsistencia_GrupoId",
                table: "BitacorasAsistencia",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_BitacorasAsistencia_MateriaId",
                table: "BitacorasAsistencia",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesInternas_AlumnoId",
                table: "CalificacionesInternas",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesInternas_GrupoId",
                table: "CalificacionesInternas",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesInternas_MateriaId",
                table: "CalificacionesInternas",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesInternas_PeriodoInternoId",
                table: "CalificacionesInternas",
                column: "PeriodoInternoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesSEP_AlumnoId",
                table: "CalificacionesSEP",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesSEP_CicloEscolarId",
                table: "CalificacionesSEP",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesSEP_GrupoId",
                table: "CalificacionesSEP",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesSEP_MateriaId",
                table: "CalificacionesSEP",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CiclosEscolares_EscuelaId",
                table: "CiclosEscolares",
                column: "EscuelaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesAcademicas_NivelEducativoId",
                table: "ConfiguracionesAcademicas",
                column: "NivelEducativoId");

            migrationBuilder.CreateIndex(
                name: "IX_CostosInscripcion_CicloEscolarId",
                table: "CostosInscripcion",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_CostosInscripcion_GradoId",
                table: "CostosInscripcion",
                column: "GradoId");

            migrationBuilder.CreateIndex(
                name: "IX_CostosInscripcion_NivelEducativoId",
                table: "CostosInscripcion",
                column: "NivelEducativoId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_AlumnoId",
                table: "CuentasPorCobrar",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_ConceptoPagoId",
                table: "CuentasPorCobrar",
                column: "ConceptoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPagos_CuentaPorCobrarId",
                table: "DetallesPagos",
                column: "CuentaPorCobrarId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPagos_PagoId",
                table: "DetallesPagos",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionesCaja_CuentaPorCobrarId",
                table: "ExcepcionesCaja",
                column: "CuentaPorCobrarId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionesCaja_PagoId",
                table: "ExcepcionesCaja",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_PagoId",
                table: "Facturas",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_TutorId",
                table: "Facturas",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Grados_NivelEducativoId",
                table: "Grados",
                column: "NivelEducativoId");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_CicloEscolarId",
                table: "Grupos",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_GradoId",
                table: "Grupos",
                column: "GradoId");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_MaestroTutorId",
                table: "Grupos",
                column: "MaestroTutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_AlumnoId",
                table: "Inscripciones",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_CicloEscolarId",
                table: "Inscripciones",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_GradoId",
                table: "Inscripciones",
                column: "GradoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_GrupoId",
                table: "Inscripciones",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_PlantelId",
                table: "Inscripciones",
                column: "PlantelId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_ReglaDescuentoId",
                table: "Inscripciones",
                column: "ReglaDescuentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Materias_GradoId",
                table: "Materias",
                column: "GradoId");

            migrationBuilder.CreateIndex(
                name: "IX_Materias_GrupoId",
                table: "Materias",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Materias_PlantelId",
                table: "Materias",
                column: "PlantelId");

            migrationBuilder.CreateIndex(
                name: "IX_NivelesEducativos_PlantelId",
                table: "NivelesEducativos",
                column: "PlantelId");

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_AlumnoId",
                table: "Observaciones",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_GrupoId",
                table: "Observaciones",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_MateriaId",
                table: "Observaciones",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_AlumnoId",
                table: "Pagos",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_TutorId",
                table: "Pagos",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosInternos_CicloEscolarId",
                table: "PeriodosInternos",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosInternos_NivelEducativoId",
                table: "PeriodosInternos",
                column: "NivelEducativoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanesPago_ConceptoPagoId",
                table: "PlanesPago",
                column: "ConceptoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Planteles_EscuelaId",
                table: "Planteles",
                column: "EscuelaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasDescuento_CicloEscolarId",
                table: "ReglasDescuento",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesConducta_AlumnoId",
                table: "ReportesConducta",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesConducta_CicloEscolarId",
                table: "ReportesConducta",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesBancarias_AlumnoId",
                table: "TransaccionesBancarias",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesBancarias_EscuelaId",
                table: "TransaccionesBancarias",
                column: "EscuelaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesBancarias_PagoGeneradoId",
                table: "TransaccionesBancarias",
                column: "PagoGeneradoId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Usuarios",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Usuarios",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPlanteles_PlantelId",
                table: "UsuariosPlanteles",
                column: "PlantelId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPlanteles_UsuarioId",
                table: "UsuariosPlanteles",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosRoles_RoleId",
                table: "UsuariosRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionesMaestros");

            migrationBuilder.DropTable(
                name: "Asistencias");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Becas");

            migrationBuilder.DropTable(
                name: "BitacorasAsistencia");

            migrationBuilder.DropTable(
                name: "CalificacionesInternas");

            migrationBuilder.DropTable(
                name: "CalificacionesSEP");

            migrationBuilder.DropTable(
                name: "ConfiguracionesAcademicas");

            migrationBuilder.DropTable(
                name: "ConfiguracionesRecargo");

            migrationBuilder.DropTable(
                name: "CostosInscripcion");

            migrationBuilder.DropTable(
                name: "DetallesPagos");

            migrationBuilder.DropTable(
                name: "ExcepcionesCaja");

            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "FichasMedicas");

            migrationBuilder.DropTable(
                name: "Inscripciones");

            migrationBuilder.DropTable(
                name: "Observaciones");

            migrationBuilder.DropTable(
                name: "PlanesPago");

            migrationBuilder.DropTable(
                name: "RelacionAlumnoTutor");

            migrationBuilder.DropTable(
                name: "ReportesConducta");

            migrationBuilder.DropTable(
                name: "TransaccionesBancarias");

            migrationBuilder.DropTable(
                name: "UsuariosPlanteles");

            migrationBuilder.DropTable(
                name: "UsuariosRoles");

            migrationBuilder.DropTable(
                name: "PeriodosInternos");

            migrationBuilder.DropTable(
                name: "CuentasPorCobrar");

            migrationBuilder.DropTable(
                name: "ReglasDescuento");

            migrationBuilder.DropTable(
                name: "Materias");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ConceptosPago");

            migrationBuilder.DropTable(
                name: "Grupos");

            migrationBuilder.DropTable(
                name: "Alumnos");

            migrationBuilder.DropTable(
                name: "Tutores");

            migrationBuilder.DropTable(
                name: "CiclosEscolares");

            migrationBuilder.DropTable(
                name: "Grados");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "NivelesEducativos");

            migrationBuilder.DropTable(
                name: "Planteles");

            migrationBuilder.DropTable(
                name: "Escuelas");
        }
    }
}
