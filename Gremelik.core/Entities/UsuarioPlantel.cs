using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class UsuarioPlantel
    {
        public int Id { get; set; }

        public string UsuarioId { get; set; } = string.Empty;
        [ForeignKey("UsuarioId")]
        public ApplicationUser? Usuario { get; set; }

        public Guid PlantelId { get; set; }
        [ForeignKey("PlantelId")]
        public Plantel? Plantel { get; set; }

        // Aquí podrías poner permisos específicos: Ej: "EsDirectorDePlantel"
        public bool EsCoordinador { get; set; } = false;
    }
}
