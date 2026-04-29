using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public enum FrecuenciaPago
    {
        PagoUnico = 1,
        Mensual = 2,
        AnualDivisible = 3
    }

    public class ConceptoPago : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; } // El precio de ESTE concepto específico

        public FrecuenciaPago Frecuencia { get; set; } = FrecuenciaPago.PagoUnico;
        public bool AplicaBeca { get; set; } = true;
        public bool Obligatorio { get; set; } = true;

        // --- FILTROS DE APLICACIÓN (AQUÍ MISMO) ---
        // Si están nulos, aplica a todos. Si tienen valor, aplica solo a eso.

        public Guid? PlantelId { get; set; }
        // Nota: No ponemos navegación compleja al Plantel para evitar ciclos, 
        // pero sí guardamos el ID para filtrar.

        public Guid? NivelEducativoId { get; set; } // Opcional
        public int? GradoId { get; set; }           // Opcional

        // Nombres auxiliares para mostrar en la lista sin hacer tantos Joins
        public string? NombrePlantel { get; set; }
        public string? NombreNivel { get; set; }
        public string? NombreGrado { get; set; }

        public int CicloEscolarId { get; set; }
        public Guid EscuelaId { get; set; }

        public bool EsFacturable { get; set; } = false;

        // --- NUEVO: CONTROL DE MOROSIDAD ---
        public bool GeneraRecargos { get; set; } = false;
        // Cuando des de alta una "Colegiatura" le pones true. A los "Libros" le dejas false.

        // --- DATOS FISCALES SAT ---
        [StringLength(10)]
        public string ClaveProdServ { get; set; } = "86121500"; // Por defecto: Servicios Educativos

        [StringLength(5)]
        public string ClaveUnidad { get; set; } = "E48"; // Por defecto: Unidad de servicio

        [StringLength(3)]
        public string ObjetoImpuesto { get; set; } = "02"; // "02" = Sí objeto de impuesto (Requisito CFDI 4.0)

        // Para saber si desglosamos IVA o lo mandamos como Exento
        public bool EsExentoIva { get; set; } = true;
    }
}