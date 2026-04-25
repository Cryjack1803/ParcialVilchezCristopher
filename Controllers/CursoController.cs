using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial_Vilchez_Cristopher.Data;
using Parcial_Vilchez_Cristopher.Models;

namespace Parcial_Vilchez_Cristopher.Controllers;

public class CursoController : Controller
{
    private readonly ApplicationDbContext _context;

    public CursoController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Curso (Catálogo con filtros)
    public async Task<IActionResult> Index(string buscar, int? minCred, int? maxCred)
    {
        // Solo mostramos cursos activos
        var query = _context.Cursos.Where(c => c.Activo).AsQueryable();

        // Filtro por Nombre o Código
        if (!string.IsNullOrEmpty(buscar))
        {
            query = query.Where(c => c.Nombre.Contains(buscar) || c.Codigo.Contains(buscar));
        }

        // Filtro por rango de Créditos (Validación server-side implícita)
        if (minCred.HasValue) query = query.Where(c => c.Creditos >= minCred.Value);
        if (maxCred.HasValue) query = query.Where(c => c.Creditos <= maxCred.Value);

        return View(await query.ToListAsync());
    }

    // GET: Curso/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var curso = await _context.Cursos.FirstOrDefaultAsync(m => m.Id == id);
        
        if (curso == null) return NotFound();

        return View(curso);
    }
}