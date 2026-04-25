using Microsoft.EntityFrameworkCore;
using Parcial_Vilchez_Cristopher.Data;
using Parcial_Vilchez_Cristopher.Models;

namespace Parcial_Vilchez_Cristopher.Services;

// EL CONTRATO
public interface IMatriculaService
{
    Task<(bool Success, string Message)> InscribirAlumnoAsync(int cursoId, string userId);
}

// LA IMPLEMENTACIÓN
public class MatriculaService : IMatriculaService
{
    private readonly ApplicationDbContext _context;

    public MatriculaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message)> InscribirAlumnoAsync(int cursoId, string userId)
    {
        var curso = await _context.Cursos.FindAsync(cursoId);
        if (curso == null) return (false, "Curso no encontrado.");

        // 1. Validar si ya está inscrito
        var yaInscrito = await _context.Matriculas
            .AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada);
        if (yaInscrito) return (false, "Ya tienes una inscripción para este curso.");

        // 2. Validar Cupos
        int inscritos = await _context.Matriculas
            .CountAsync(m => m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);
        if (inscritos >= curso.CupoMaximo) return (false, "No hay cupos disponibles.");

        // 3. Validar Choques de Horario
        var misCursos = await _context.Matriculas
            .Where(m => m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada)
            .Select(m => m.Curso).ToListAsync();

        foreach (var c in misCursos)
        {
            if (curso.HorarioInicio < c.HorarioFin && c.HorarioInicio < curso.HorarioFin)
                return (false, $"Choque de horario con: {c.Nombre}");
        }

        // 4. Crear matrícula en PENDIENTE (Requisito P3)
        var matricula = new Matricula
        {
            CursoId = cursoId,
            UsuarioId = userId,
            FechaRegistro = DateTime.Now,
            Estado = EstadoMatricula.Pendiente 
        };

        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();

        return (true, "Inscripción enviada. Estado: Pendiente.");
    }
}