using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gremelik.core.DTOs
{
    public class CancelarPagoDto
    {
        public Guid PagoId { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }
}
