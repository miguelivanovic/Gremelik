using Gremelik.core.Entities;
using Gremelik.data.Contexts;
using Microsoft.EntityFrameworkCore;
using Gremelik.core.DTOs;

namespace Gremelik.API.Services
{
    public class CalculadoraDeudasService
    {
        private readonly GremelikDbContext _context;

        public CalculadoraDeudasService(GremelikDbContext context)
        {
            _context = context;
        }

        public async Task<List<DeudaCalculadaDto>> CalcularDeudasAsync(Guid alumnoId, int cicloId, DateTime fechaCalculo)
        {
            // 1. Traemos deudas pendientes (PAGOS PARCIALES o PENDIENTES)
            var deudas = await _context.CuentasPorCobrar
                .Include(c => c.ConceptoRelacionado)
                .Where(c => c.AlumnoId == alumnoId && c.CicloEscolarId == cicloId && c.Activo && c.Estado != "PAGADO")
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();

            var recargosConfig = await _context.ConfiguracionesRecargo
                .Where(r => r.CicloEscolarId == cicloId && r.Activo)
                .ToListAsync();

            var excepcionesPrevias = await _context.ExcepcionesCaja
                .Where(e => e.Pago!.AlumnoId == alumnoId && e.Activo)
                .ToListAsync();

            // 2. BUSQUEDA CRÍTICA: Traemos TODOS los recargos que el alumno ha tenido (PAGADOS O NO)
            // Esto sirve como "vacuna" contra la duplicidad.
            var historicoRecargos = await _context.CuentasPorCobrar
                .Where(c => c.AlumnoId == alumnoId
                         && c.CicloEscolarId == cicloId
                         && c.Activo
                         && c.ConceptoNombre.StartsWith("Recargo - "))
                .Select(c => new { c.ConceptoNombre, c.Estado, c.Id })
                .ToListAsync();

            var resultados = new List<DeudaCalculadaDto>();

            foreach (var d in deudas)
            {
                // A) Si la deuda que estamos procesando YA es un recargo real que quedó parcial...
                // A) Si la deuda que estamos procesando YA es un recargo real que quedó parcial...
                if (d.ConceptoNombre.StartsWith("Recargo - "))
                {
                    // CORRECCIÓN: Buscamos a su "Deuda Madre" quitándole la palabra "Recargo - " para enlazarlos
                    var nombreMadre = d.ConceptoNombre.Replace("Recargo - ", "").Replace(" (IVA Inc.)", "").Trim();
                    var deudaMadre = deudas.FirstOrDefault(p => p.ConceptoNombre.Trim() == nombreMadre);

                    resultados.Add(new DeudaCalculadaDto
                    {
                        CuentaPorCobrarId = d.Id,
                        ConceptoNombre = d.ConceptoNombre,
                        NumeroDePago = d.NumeroDePago,
                        FechaVencimiento = d.FechaVencimiento,
                        MontoBase = d.MontoBase,
                        DescuentoBecaOriginal = 0,
                        DescuentoBecaAplicado = 0,
                        TotalPagado = d.TotalPagado,
                        Estado = d.Estado,
                        EsFacturable = d.EsFacturable,
                        EsRecargoVirtual = true,
                        DeudaOriginalId = deudaMadre?.Id // <--- EL ESLABÓN PERDIDO REPARADO
                    });
                    continue;
                }

                // B) Si es una deuda normal (Colegiatura)...
                bool becaPerdida = false;
                decimal becaAplicada = d.DescuentoBeca;
                var excepcionLocal = excepcionesPrevias.FirstOrDefault(e => e.CuentaPorCobrarId == d.Id);

                int diasAtraso = (fechaCalculo.Date - d.FechaVencimiento.Date).Days;

                // VALIDACIÓN DE ORO: ¿Existe ya un recargo en el historial para ESTA colegiatura?
                // Buscamos si hay algún registro que se llame "Recargo - [Nombre de la deuda]"
                // VALIDACIÓN DE ORO
                bool yaTieneRecargoRegistrado = historicoRecargos.Any(r => r.ConceptoNombre.Contains(d.ConceptoNombre.Trim()));

                DeudaCalculadaDto? recargoVirtualDto = null;

                // Solo intentamos generar recargo si:
                // 1. Hay atraso.
                // 2. El concepto genera recargos.
                // 3. NO existe ya un recargo (pagado o parcial) en la base de datos para esta deuda.
                if (diasAtraso > 0 && d.ConceptoRelacionado != null && d.ConceptoRelacionado.GeneraRecargos && !yaTieneRecargoRegistrado)
                {
                    var regla = recargosConfig.Where(r => diasAtraso > r.DiasGracia).OrderByDescending(r => r.DiasGracia).FirstOrDefault();

                    if (regla != null)
                    {
                        // Pérdida de beca
                        if (becaAplicada > 0 && (excepcionLocal == null || excepcionLocal.BecaRestauradaMonto == 0))
                        {
                            becaPerdida = true;
                            becaAplicada = 0;
                        }

                        // Cálculo de monto
                        if (excepcionLocal == null || excepcionLocal.RecargoPerdonadoMonto == 0)
                        {
                            decimal montoRecargoMensual = regla.Porcentaje > 0 ? (d.MontoBase * (regla.Porcentaje / 100)) : regla.MontoFijo;
                            decimal recargoTotal = montoRecargoMensual;

                            if (regla.Tipo == TipoAplicacionRecargo.MensualAcumulativo)
                            {
                                int mesesAtraso = ((fechaCalculo.Year - d.FechaVencimiento.Year) * 12) + fechaCalculo.Month - d.FechaVencimiento.Month;
                                if (mesesAtraso < 1) mesesAtraso = 1;
                                recargoTotal = montoRecargoMensual * mesesAtraso;
                            }

                            if (regla.AplicaIva && !regla.IvaIncluido) recargoTotal = recargoTotal * 1.16m;

                            recargoVirtualDto = new DeudaCalculadaDto
                            {
                                CuentaPorCobrarId = Guid.NewGuid(),
                                ConceptoNombre = $"Recargo - {d.ConceptoNombre}",
                                NumeroDePago = 0,
                                FechaVencimiento = d.FechaVencimiento,
                                MontoBase = recargoTotal,
                                TotalPagado = 0,
                                Estado = "PENDIENTE",
                                EsFacturable = true,
                                EsRecargoVirtual = true,
                                DeudaOriginalId = d.Id,
                                ConceptoPagoId = d.ConceptoPagoId
                            };
                        }
                    }
                }

                // Agregamos la deuda principal
                resultados.Add(new DeudaCalculadaDto
                {
                    CuentaPorCobrarId = d.Id,
                    ConceptoNombre = d.ConceptoNombre,
                    NumeroDePago = d.NumeroDePago,
                    FechaVencimiento = d.FechaVencimiento,
                    MontoBase = d.MontoBase,
                    DescuentoBecaOriginal = d.DescuentoBeca,
                    DescuentoBecaAplicado = becaAplicada,
                    TotalPagado = d.TotalPagado,
                    Estado = d.Estado,
                    EsFacturable = d.EsFacturable,
                    BecaPerdidaPorAtraso = becaPerdida,
                    EsRecargoVirtual = false
                });

                if (recargoVirtualDto != null) resultados.Add(recargoVirtualDto);
            }

            return resultados;
        }
    }
}