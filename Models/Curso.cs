using System.ComponentModel.DataAnnotations;

namespace Parcial_Vilchez_Cristopher.Models; // CAMBIADO

public class Curso
{
    public int Id { get; set; }
    
    [Required]
    public string Codigo { get; set; } = null!;
    
    [Required]
    public string Nombre { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Créditos > 0")]
    public int Creditos { get; set; }
    
    public int CupoMaximo { get; set; }
    public TimeSpan HorarioInicio { get; set; }
    public TimeSpan HorarioFin { get; set; }
    public bool Activo { get; set; }

    public virtual ICollection<Matricula>? Matriculas { get; set; }
}