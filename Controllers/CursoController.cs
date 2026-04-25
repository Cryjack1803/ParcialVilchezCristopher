using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial_Vilchez_Cristopher.Data;
using Parcial_Vilchez_Cristopher.Models;
using Microsoft.Extensions.Caching.Distributed; // Necesario para Redis
using System.Text.Json; // Necesario para serializar la lista

namespace Parcial_Vilchez_Cristopher.Controllers;

public class CursoController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache; // Inyectamos el Cache

    public CursoController(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // GET: Curso (Cache de 60s + Filtros)
    public async Task<IActionResult> Index(string buscar, int? minCred, int? maxCred)
    {
        string cacheKey = "CursosActivosList";
        List<Curso> cursos;

        // 1. Intentar obtener la lista de Redis
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (cachedData != null)
        {
            // Si existe en cache, lo deserializamos
            cursos = JsonSerializer.Deserialize<List<Curso>>(cachedData)!;
        }
        else
        {
            // 2. Si no existe, vamos a la DB y filtramos por Activos
            cursos = await _context.Cursos.Where(c => c.Activo).ToListAsync();

            // 3. Guardamos en Redis por 60 segundos (Requisito P4)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
            
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(cursos), cacheOptions);
        }

        // 4. Aplicamos los filtros sobre la lista (ya sea de cache o de DB)
        var query = cursos.AsQueryable();

        if (!string.IsNullOrEmpty(buscar))
        {
            query = query.Where(c => c.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase) || 
                                     c.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase));
        }

        if (minCred.HasValue) query = query.Where(c => c.Creditos >= minCred.Value);
        if (maxCred.HasValue) query = query.Where(c => c.Creditos <= maxCred.Value);

        return View(query.ToList());
    }

    // GET: Curso/Details/5 (Sesión para el Layout)
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var curso = await _context.Cursos.FirstOrDefaultAsync(m => m.Id == id);
        
        if (curso == null) return NotFound();

        // GUARDAR EN SESIÓN (Requisito P4)
        // Esto lo leerá el _Layout.cshtml que configuramos antes
        HttpContext.Session.SetString("LastCourseName", curso.Nombre);
        HttpContext.Session.SetInt32("LastCourseId", curso.Id);

        return View(curso);
    }
}