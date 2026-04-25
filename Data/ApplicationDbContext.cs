using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parcial_Vilchez_Cristopher.Models; // CAMBIADO

namespace Parcial_Vilchez_Cristopher.Data; // CAMBIADO

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Curso> Cursos { get; set; } = null!;
    public DbSet<Matricula> Matriculas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Restricción: Código de curso único
        builder.Entity<Curso>().HasIndex(c => c.Codigo).IsUnique();

        // Restricción: Usuario único por curso
        builder.Entity<Matricula>()
            .HasIndex(m => new { m.CursoId, m.UsuarioId }).IsUnique();
    }
}