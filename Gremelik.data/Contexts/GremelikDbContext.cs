using Gremelik.core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gremelik.data.Contexts
{
    public class GremelikDbContext(DbContextOptions<GremelikDbContext> options) : DbContext(options)
    {
        public DbSet<Escuela> Escuelas { get; set; } = null!;
        public DbSet<Alumno> Alumnos { get; set; } = null!;
        public DbSet<Tutor> Tutores { get; set; } = null!;
        public DbSet<FichaMedica> FichasMedicas { get; set; } = null!;
        public DbSet<RelacionAlumnoTutor> RelacionAlumnoTutor { get; set; } = null!;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                        e.State == EntityState.Added ||
                        e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).FUM = DateTime.Now;
                ((BaseEntity)entityEntry.Entity).Usuario = "Sistema";
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ESTO ES LO NUEVO:
            // Permite que la CURP se repita, PERO NO en la misma escuela.
            modelBuilder.Entity<Alumno>()
                .HasIndex(a => new { a.CURP, a.EscuelaId })
                .IsUnique();

            // Configuración de la relación Alumno-Tutor (Llave compuesta)
            modelBuilder.Entity<RelacionAlumnoTutor>()
                .HasKey(rat => new { rat.AlumnoId, rat.TutorId });
        }
    }
}
