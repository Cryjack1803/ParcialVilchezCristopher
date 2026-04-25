using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parcial_Vilchez_Cristopher.Services; // <-- CORREGIDO: agregamos el Vilchez_
using System.Security.Claims;

namespace Parcial_Vilchez_Cristopher.Controllers; // <-- CORREGIDO: agregamos el Vilchez_

[Authorize]
public class MatriculaController : Controller
{
    private readonly IMatriculaService _matriculaService;

    public MatriculaController(IMatriculaService matriculaService)
    {
        _matriculaService = matriculaService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscribir(int cursoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Llamamos al servicio con el nombre correcto
        var result = await _matriculaService.InscribirAlumnoAsync(cursoId, userId!);

        if (result.Success)
        {
            TempData["Success"] = result.Message;
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction("Details", "Curso", new { id = cursoId });
    }
}