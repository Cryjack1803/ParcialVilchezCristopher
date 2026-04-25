using System.ComponentModel.DataAnnotations;

namespace Parcial_Vilchez_Cristopher.Models; // CAMBIADO

public enum EstadoMatricula { Pendiente, Confirmada, Cancelada }

public class Matricula
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public string UsuarioId { get; set; } = null!;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;

    public virtual Curso? Curso { get; set; }
}