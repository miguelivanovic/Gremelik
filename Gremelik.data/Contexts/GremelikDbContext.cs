
using Gremelik.core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gremelik.data.Contexts
{
    public class GremelikDbContext : DbContext
    {
        public GremelikDbContext(DbContextOptions<GremelikDbContext> options) : base(options)
        {
        }

        public DbSet<Escuela> Escuelas { get; set; }
        public DbSet<Alumno> Alumnos { get; set; }
        public DbSet<Tutor> Tutores { get; set; }
        public DbSet<FichaMedica> FichasMedicas { get; set; }
        public DbSet<RelacionAlumnoTutor> RelacionAlumnoTutor { get; set; }

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
            modelBuilder.Entity<RelacionAlumnoTutor>()
                .HasKey(r => new { r.AlumnoId, r.TutorId });
        }
    }
}
