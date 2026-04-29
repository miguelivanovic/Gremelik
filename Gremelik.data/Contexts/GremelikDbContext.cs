using Gremelik.core.Entities;
using Gremelik.core.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gremelik.data.Contexts
{
    // Heredamos de IdentityDbContext<ApplicationUser> para tener usuarios y roles listos
    public class GremelikDbContext(DbContextOptions<GremelikDbContext> options, CurrentTenantService? tenantService = null)
        : IdentityDbContext<ApplicationUser>(options)
    {
        // Guardamos el servicio. Si es null (ej. en migraciones), no pasa nada.
        private readonly CurrentTenantService? _tenantService = tenantService;

        // --- DBSETS (Tus Tablas) ---
        public DbSet<Escuela> Escuelas { get; set; } = null!;
        public DbSet<Alumno> Alumnos { get; set; } = null!;
         public DbSet<Tutor> Tutores { get; set; } = null!; // (Comentado si aún no creas la clase Tutor)
         public DbSet<FichaMedica> FichasMedicas { get; set; } = null!; // (Igual aquí)
         public DbSet<RelacionAlumnoTutor> RelacionAlumnoTutor { get; set; } = null!;

        // Estructura Académica
        public DbSet<Plantel> Planteles { get; set; } = null!;
        public DbSet<CicloEscolar> CiclosEscolares { get; set; } = null!;
        public DbSet<NivelEducativo> NivelesEducativos { get; set; } = null!;
        public DbSet<UsuarioPlantel> UsuariosPlanteles { get; set; } = null!;
        public DbSet<Grado> Grados { get; set; } = null!;
        public DbSet<Grupo> Grupos { get; set; } = null!;
        public DbSet<CostoInscripcion> CostosInscripcion { get; set; } = null!;
        public DbSet<ReglaDescuento> ReglasDescuento { get; set; } = null!;

        // El Nuevo Módulo
        public DbSet<Inscripcion> Inscripciones { get; set; } = null!;

        // ... otros DbSets ...
        public DbSet<ConceptoPago> ConceptosPago { get; set; }
        public DbSet<PlanPago> PlanesPago { get; set; }

        public DbSet<Beca> Becas { get; set; }

        public DbSet<CuentaPorCobrar> CuentasPorCobrar { get; set; }

        public DbSet<Pago> Pagos { get; set; }
        public DbSet<DetallePago> DetallesPagos { get; set; }

        public DbSet<TransaccionBancaria> TransaccionesBancarias { get; set; }

        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<ObservacionAlumno> Observaciones { get; set; }

        public DbSet<Materia> Materias { get; set; }
        public DbSet<AsignacionMaestro> AsignacionesMaestros { get; set; }

        public DbSet<BitacoraAsistencia> BitacorasAsistencia { get; set; }

        public DbSet<ConfiguracionAcademica> ConfiguracionesAcademicas { get; set; }
        public DbSet<PeriodoInterno> PeriodosInternos { get; set; }
        public DbSet<CalificacionInterna> CalificacionesInternas { get; set; }
        public DbSet<CalificacionSEP> CalificacionesSEP { get; set; }

        public DbSet<ReporteConducta> ReportesConducta { get; set; }

        public DbSet<ConfiguracionRecargo> ConfiguracionesRecargo { get; set; }
        public DbSet<ExcepcionCaja> ExcepcionesCaja { get; set; }

        public DbSet<Factura> Facturas { get; set; }

        // --- LOGICA DE GUARDADO AUTOMÁTICO ---
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                        e.State == EntityState.Added ||
                        e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;
                entity.FUM = DateTime.Now; // Fecha Última Modificación

                // Si la entidad es nueva y no tiene usuario, asignamos "Sistema" por defecto
                if (string.IsNullOrEmpty(entity.Usuario))
                {
                    entity.Usuario = "Sistema";
                }

                // TRUCO SAAS: Asignación automática de EscuelaId
                // Esto soluciona el error de "Guid a int" asegurando que ambos sean compatibles.
                if (entityEntry.State == EntityState.Added &&
                    _tenantService?.TenantId != null)
                {
                    // 1. Si es ALUMNO
                    if (entity is Alumno alumno)
                    {
                        alumno.EscuelaId = _tenantService.TenantId.Value;
                    }
                    // 2. Si es INSCRIPCIÓN (Opcional, pero recomendado automatizarlo también)
                    else if (entity is Inscripcion inscripcion)
                    {
                        // Solo asignamos si no venía ya lleno
                        if (inscripcion.Alumno != null)
                        {
                            // La inscripción toma el dato del alumno
                        }
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        // --- CONFIGURACIÓN DE FILTROS Y RELACIONES ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Renombrar tablas de Identity para que sean más bonitas en SQL
            modelBuilder.Entity<ApplicationUser>().ToTable("Usuarios");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UsuariosRoles");

            // =========================================================================
            //  MURO DE SEGURIDAD (Global Query Filters)
            //  IMPORTANTE: Todos los IDs aquí se tratan como GUIDs gracias a tus clases.
            // =========================================================================

            // 1. ALUMNOS
            modelBuilder.Entity<Alumno>().HasQueryFilter(a =>
                _tenantService == null ||
                _tenantService.TenantId == null ||
                a.EscuelaId == _tenantService.TenantId
            );

            // 2. USUARIOS (ApplicationUser)
            modelBuilder.Entity<ApplicationUser>().HasQueryFilter(u =>
                _tenantService == null ||
                _tenantService.TenantId == null ||
                u.EscuelaId == _tenantService.TenantId ||
                u.EscuelaId == null // Global Admins
            );

            // 3. PLANTELES
            modelBuilder.Entity<Plantel>().HasQueryFilter(p =>
                _tenantService == null ||
                _tenantService.TenantId == null ||
                p.EscuelaId == _tenantService.TenantId
            );

            // 4. CICLOS ESCOLARES
            modelBuilder.Entity<CicloEscolar>().HasQueryFilter(c =>
                _tenantService == null ||
                _tenantService.TenantId == null ||
                c.EscuelaId == _tenantService.TenantId
            );

            // 5. NIVELES EDUCATIVOS (Viaja a través de Plantel)
            modelBuilder.Entity<NivelEducativo>().HasQueryFilter(n =>
                _tenantService == null ||
                _tenantService.TenantId == null ||
                n.Plantel!.EscuelaId == _tenantService.TenantId
            );

            // 6. USUARIOS PLANTELES
            modelBuilder.Entity<UsuarioPlantel>().HasQueryFilter(up =>
                _tenantService == null ||
                _tenantService.TenantId == null ||
                up.Plantel!.EscuelaId == _tenantService.TenantId
            );

            // 7. GRADOS (Viaja Nivel -> Plantel)
            modelBuilder.Entity<Grado>().HasQueryFilter(g =>
                _tenantService == null ||
                _tenantService.TenantId == null ||
                g.NivelEducativo!.Plantel!.EscuelaId == _tenantService.TenantId
            );

            // 8. GRUPOS (Viaja por Ciclo Escolar)
            modelBuilder.Entity<Grupo>().HasQueryFilter(gr =>
                _tenantService == null ||
                _tenantService.TenantId == null ||
                gr.CicloEscolar!.EscuelaId == _tenantService.TenantId
            );

            // 9. INSCRIPCIONES (Nuevo - Viaja por Alumno)
            modelBuilder.Entity<Inscripcion>().HasQueryFilter(i =>
                _tenantService == null ||
                _tenantService.TenantId == null ||
                i.Alumno!.EscuelaId == _tenantService.TenantId
            );

            // =========================================================================
            //  CONFIGURACIONES EXTRAS
            // =========================================================================

            // Índice para evitar duplicados de CURP en la misma escuela
            modelBuilder.Entity<Alumno>()
                .HasIndex(a => new { a.CURP, a.EscuelaId })
                .IsUnique();

            // Si tienes la tabla RelacionAlumnoTutor activa:
            /*
            modelBuilder.Entity<RelacionAlumnoTutor>()
                .HasKey(rat => new { rat.AlumnoId, rat.TutorId });
            */

            // EVITAR ERROR DE CASCADA CÍCLICA (SQL Server Multiple Cascade Paths)
            // Esto es vital para que te deje crear la base de datos.
            modelBuilder.Entity<Grupo>()
                .HasOne(g => g.Grado)
                .WithMany()
                .HasForeignKey(g => g.GradoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Hacemos lo mismo con CicloEscolar por seguridad
            modelBuilder.Entity<Grupo>()
                .HasOne(g => g.CicloEscolar)
                .WithMany()
                .HasForeignKey(g => g.CicloEscolarId)
                .OnDelete(DeleteBehavior.Restrict);

            // ... (Tus códigos anteriores) ...

            // --- SOLUCIÓN ERROR DE CASCADA EN INSCRIPCIONES ---
            // Rompemos el ciclo de borrado automático.
            // Si intentas borrar un Ciclo, Grupo o Plantel que tiene alumnos inscritos,
            // la BD te detendrá (Restrict) en lugar de borrar todo en cadena.

            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.CicloEscolar)
                .WithMany()
                .HasForeignKey(i => i.CicloEscolarId)
                .OnDelete(DeleteBehavior.Restrict); // <--- ESTO ARREGLA EL ERROR

            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.Grupo)
                .WithMany()
                .HasForeignKey(i => i.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.Plantel)
                .WithMany()
                .HasForeignKey(i => i.PlantelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReglaDescuento>().HasQueryFilter(r =>
                _tenantService == null || _tenantService.TenantId == null || r.EscuelaId == _tenantService.TenantId
);
            // CostoInscripcion se filtra por CicloEscolar, así que ya está protegido indirectamente.

            modelBuilder.Entity<Pago>()
                .Property(p => p.Folio)
                .ValueGeneratedOnAdd();

            // --- SOLUCIÓN DEL ERROR DE CICLOS ---

            // 1. Si borras una Deuda (CuentaPorCobrar), NO borres el historial de pagos (Restrict)
            modelBuilder.Entity<DetallePago>()
                .HasOne(d => d.CuentaPorCobrar)
                .WithMany()
                .HasForeignKey(d => d.CuentaPorCobrarId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Si borras un Pago (Ticket), SÍ borra sus detalles (Cascade) - Esto está bien
            modelBuilder.Entity<DetallePago>()
                .HasOne(d => d.Pago)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.PagoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Evitar bucle de borrado en Asignaciones de Maestros
            modelBuilder.Entity<AsignacionMaestro>()
                .HasOne(a => a.Materia)
                .WithMany()
                .HasForeignKey(a => a.MateriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AsignacionMaestro>()
                .HasOne(a => a.Grupo)
                .WithMany()
                .HasForeignKey(a => a.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Evitar bucle de borrado en Asistencias
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Materia)
                .WithMany()
                .HasForeignKey(a => a.MateriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Grupo)
                .WithMany()
                .HasForeignKey(a => a.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ObservacionAlumno>()
                .HasOne(o => o.Materia)
                .WithMany()
                .HasForeignKey(o => o.MateriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ObservacionAlumno>()
                .HasOne(o => o.Grupo)
                .WithMany()
                .HasForeignKey(o => o.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BitacoraAsistencia>()
                .HasOne(b => b.Materia)
                .WithMany()
                .HasForeignKey(b => b.MateriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BitacoraAsistencia>()
                .HasOne(b => b.Grupo)
                .WithMany()
                .HasForeignKey(b => b.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- REGLAS ANTI-CASCADA PARA CALIFICACIONES ---

            // 1. Calificaciones Internas (Mes/Bimestre)
            modelBuilder.Entity<CalificacionInterna>()
                .HasOne(c => c.Alumno)
                .WithMany()
                .HasForeignKey(c => c.AlumnoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalificacionInterna>()
                .HasOne(c => c.Grupo)
                .WithMany()
                .HasForeignKey(c => c.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalificacionInterna>()
                .HasOne(c => c.Materia)
                .WithMany()
                .HasForeignKey(c => c.MateriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalificacionInterna>()
                .HasOne(c => c.Periodo)
                .WithMany()
                .HasForeignKey(c => c.PeriodoInternoId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Calificaciones SEP (Oficiales)
            modelBuilder.Entity<CalificacionSEP>()
                .HasOne(c => c.Alumno)
                .WithMany()
                .HasForeignKey(c => c.AlumnoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalificacionSEP>()
                .HasOne(c => c.Grupo)
                .WithMany()
                .HasForeignKey(c => c.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalificacionSEP>()
                .HasOne(c => c.Materia)
                .WithMany()
                .HasForeignKey(c => c.MateriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Periodos Internos (Para evitar el error de cascada múltiple)
            modelBuilder.Entity<PeriodoInterno>()
                .HasOne(p => p.NivelEducativo)
                .WithMany()
                .HasForeignKey(p => p.NivelEducativoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PeriodoInterno>()
                .HasOne(p => p.CicloEscolar)
                .WithMany()
                .HasForeignKey(p => p.CicloEscolarId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- REGLA ANTI-CASCADA PARA ASISTENCIAS ---
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.CicloEscolar)
                .WithMany()
                .HasForeignKey(a => a.CicloEscolarId)
                .OnDelete(DeleteBehavior.NoAction); // Evita el error de Múltiples Rutas en Cascada

            // --- REGLA ANTI-CASCADA PARA ASISTENCIAS ---
            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.CicloEscolar)
                .WithMany()
                .HasForeignKey(a => a.CicloEscolarId)
                .OnDelete(DeleteBehavior.NoAction);

            // --- REGLA ANTI-CASCADA PARA REPORTES DE CONDUCTA ---
            modelBuilder.Entity<ReporteConducta>()
                .HasOne(r => r.CicloEscolar)
                .WithMany()
                .HasForeignKey(r => r.CicloEscolarId)
                .OnDelete(DeleteBehavior.NoAction);

            // --- CORRECCIÓN DEL ERROR DE CICLOS MÚLTIPLES ---
            // Si tu error marcaba ExcepcionesCaja, agregamos esta regla:
            modelBuilder.Entity<ExcepcionCaja>()
                .HasOne(e => e.CuentaPorCobrar)
                .WithMany() // O si tienes una lista en CuentasPorCobrar, pon el nombre de la lista adentro
                .HasForeignKey(e => e.CuentaPorCobrarId)
                .OnDelete(DeleteBehavior.Restrict); // Esto apaga el borrado en cascada

            // Si la Factura también da lata con el PagoId o TutorId:
            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Pago)
                .WithMany()
                .HasForeignKey(f => f.PagoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Tutor)
                .WithMany()
                .HasForeignKey(f => f.TutorId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}