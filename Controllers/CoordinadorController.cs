using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Parcial_Vilchez_Cristopher.Data;
using Parcial_Vilchez_Cristopher.Models;

namespace Parcial_Vilchez_Cristopher.Controllers;

// CANDADO: Solo los Coordinadores pueden entrar aquí
[Authorize(Roles = "Coordinador")]
public class CoordinadorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public CoordinadorController(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // 1. LISTA DE CURSOS (Dashboard del Coordinador)
    public async Task<IActionResult> Index()
    {
        var cursos = await _context.Cursos.ToListAsync();
        return View(cursos);
    }

    // 2. CREAR CURSO (GET y POST)
    public IActionResult CrearCurso()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCurso(Curso curso)
    {
        if (ModelState.IsValid)
        {
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();
            
            // REQUISITO P4: Invalidar cache al crear curso
            await _cache.RemoveAsync("CursosActivosList"); 
            
            TempData["Success"] = "Curso creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(curso);
    }

    // 3. DESACTIVAR/ACTIVAR CURSO
    [HttpPost]
    public async Task<IActionResult> CambiarEstadoCurso(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso != null)
        {
            curso.Activo = !curso.Activo;
            await _context.SaveChangesAsync();
            
            // REQUISITO P4: Invalidar cache al editar curso
            await _cache.RemoveAsync("CursosActivosList");
        }
        return RedirectToAction(nameof(Index));
    }

   // 4. VER MATRÍCULAS POR CURSO
    public async Task<IActionResult> Matriculas(int id)
    {
        var curso = await _context.Cursos
            .Include(c => c.Matriculas) // Solo traemos las matrículas
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curso == null) return NotFound();

        return View(curso);
    }

    // 5. CONFIRMAR / CANCELAR MATRÍCULA
    [HttpPost]
    public async Task<IActionResult> CambiarEstadoMatricula(int matriculaId, EstadoMatricula nuevoEstado, int cursoId)
    {
        var matricula = await _context.Matriculas.FindAsync(matriculaId);
        if (matricula != null)
        {
            matricula.Estado = nuevoEstado;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Matrícula actualizada a {nuevoEstado}.";
        }
        return RedirectToAction(nameof(Matriculas), new { id = cursoId });
    }
}