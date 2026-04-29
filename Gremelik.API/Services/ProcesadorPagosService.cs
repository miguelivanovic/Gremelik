using Gremelik.core.DTOs;
using Gremelik.core.Entities;
using Gremelik.data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Gremelik.API.Services // <-- Camiamos el namespace al API
{
    public class ProcesadorPagosService
    {
        private readonly GremelikDbContext _context;

        public ProcesadorPagosService(GremelikDbContext context)
        {
            _context = context;
        }

        // CAMBIO 1: Agregamos el parámetro 'banco'
        public async Task<ResultadoConciliacionDto> ProcesarLoteBancarioAsync(
            Guid escuelaId,
            string banco,
            List<TransaccionBancariaDto> transacciones,
            string usuario)
        {
            var resultado = new ResultadoConciliacionDto();

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var tx in transacciones)
                {
                    // --- 1. CANDADO ANTI-DUPLICADOS ---
                    bool yaExiste = await _context.TransaccionesBancarias
                        .AnyAsync(t => t.EscuelaId == escuelaId && t.ClaveRastreo == tx.ClaveRastreo && t.Banco == banco);

                    if (yaExiste)
                    {
                        var duplicada = RegistrarTransaccionBase(escuelaId, banco, tx, usuario, EstatusTransaccion.Duplicada);
                        _context.TransaccionesBancarias.Add(duplicada);

                        resultado.Advertencias.Add($"Fila omitida: El rastreo '{tx.ClaveRastreo}' ya fue procesado anteriormente.");
                        continue;
                    }

                    // Preparamos el registro histórico por defecto como Huérfano
                    var registroBitacora = RegistrarTransaccionBase(escuelaId, banco, tx, usuario, EstatusTransaccion.Huerfana);

                    // --- 2. BUSCAR AL ALUMNO ---
                    var alumno = await _context.Alumnos
                        .FirstOrDefaultAsync(a => a.EscuelaId == escuelaId && a.Matricula == tx.Referencia && a.Activo);

                    if (alumno == null)
                    {
                        _context.TransaccionesBancarias.Add(registroBitacora);
                        resultado.Advertencias.Add($"Pago Huérfano: Matrícula '{tx.Referencia}' no encontrada. Esperando conciliación manual.");
                        continue;
                    }

                    // --- 3. NUEVA REGLA: VALIDACIÓN ESTRICTA DE MONTO EXACTO ---
                    var hoy = DateTime.Today;

                    // Buscamos todas las deudas del alumno que vencen hoy o antes
                    var deudasPendientes = await _context.CuentasPorCobrar
                        .Where(c => c.EscuelaId == escuelaId
                                 && c.AlumnoId == alumno.Id
                                 && c.Estado != "PAGADO"
                                 && c.Activo
                                 && c.FechaVencimiento <= hoy)
                        .OrderBy(c => c.FechaVencimiento).ToListAsync();

                    if (!deudasPendientes.Any())
                    {
                        // El alumno existe, pero no tiene deudas exigibles. Se queda huérfano.
                        registroBitacora.AlumnoId = alumno.Id; // Lo asociamos para que en la revisión manual sea más fácil
                        _context.TransaccionesBancarias.Add(registroBitacora);
                        resultado.Advertencias.Add($"Requiere Revisión: Matrícula '{tx.Referencia}' depositó ${tx.Monto:N2} pero no tiene deudas vencidas.");
                        continue;
                    }

                    // Aquí viene la magia estricta: Vamos sumando las deudas más viejas 
                    // a ver si "empatan" con el depósito exacto.
                    decimal sumaDeudasAEmpatar = 0;
                    var deudasAAplicar = new List<CuentaPorCobrar>();
                    bool montoExactoEncontrado = false;

                    foreach (var deuda in deudasPendientes)
                    {
                        decimal deudaReal = (deuda.MontoBase - deuda.DescuentoBeca + deuda.RecargosAcumulados) - deuda.TotalPagado;
                        sumaDeudasAEmpatar += deudaReal;
                        deudasAAplicar.Add(deuda);

                        if (sumaDeudasAEmpatar == tx.Monto)
                        {
                            montoExactoEncontrado = true;
                            break; // ¡Empató al centavo! Dejamos de buscar.
                        }
                        else if (sumaDeudasAEmpatar > tx.Monto)
                        {
                            // Ya nos pasamos. El depósito no cuadra con una combinación exacta de deudas.
                            break;
                        }
                    }

                    if (!montoExactoEncontrado)
                    {
                        // Si no empató, lo mandamos a la congeladora (Huerfana)
                        registroBitacora.AlumnoId = alumno.Id;
                        _context.TransaccionesBancarias.Add(registroBitacora);
                        resultado.Advertencias.Add($"Requiere Revisión: Matrícula '{tx.Referencia}' depositó ${tx.Monto:N2}, pero sus próximas deudas suman ${sumaDeudasAEmpatar:N2}. Monto inexacto.");
                        continue;
                    }

                    // --- 4. SI LLEGAMOS AQUÍ, EL MONTO FUE EXACTO. PROCEDEMOS A COBRAR ---
                    bool tieneTutorValido = await _context.RelacionAlumnoTutor.AnyAsync(r => r.AlumnoId == alumno.Id && r.Activo);
                    bool requiereFacturaGlobal = false;
                    int cicloDelPago = deudasAAplicar.First().CicloEscolarId;
                    var detallesNuevos = new List<DetallePago>();

                    foreach (var deuda in deudasAAplicar)
                    {
                        decimal deudaReal = (deuda.MontoBase - deuda.DescuentoBeca + deuda.RecargosAcumulados) - deuda.TotalPagado;

                        deuda.TotalPagado += deudaReal; // Se paga completa
                        deuda.Estado = "PAGADO";
                        deuda.FechaPago = tx.FechaPago;

                        if (deuda.EsFacturable && tieneTutorValido) requiereFacturaGlobal = true;

                        detallesNuevos.Add(new DetallePago
                        {
                            CuentaPorCobrarId = deuda.Id,
                            ConceptoNombreSnapshot = deuda.ConceptoNombre,
                            MontoAbonado = deudaReal,
                            Usuario = usuario,
                            FechaRegistro = DateTime.Now,
                            Activo = true
                        });
                    }

                    // Generar Recibo de Pago Oficial
                    var nuevoPago = new Pago
                    {
                        EscuelaId = escuelaId,
                        AlumnoId = alumno.Id,
                        CicloEscolarId = cicloDelPago,
                        FechaPago = tx.FechaPago,
                        TotalPagado = tx.Monto,
                        MetodoPago = MetodoPago.Transferencia, // Según tu Enum
                        Comentarios = $"Layout Automático (Monto Exacto). Rastreo: {tx.ClaveRastreo}",
                        Usuario = usuario,
                        RequiereFactura = requiereFacturaGlobal,
                        Activo = true,
                        Detalles = detallesNuevos
                    };

                    _context.Pagos.Add(nuevoPago);

                    // Actualizar la bitácora
                    registroBitacora.Estatus = EstatusTransaccion.Aplicada;
                    registroBitacora.AlumnoId = alumno.Id;
                    registroBitacora.PagoGenerado = nuevoPago;
                    _context.TransaccionesBancarias.Add(registroBitacora);

                    resultado.ProcesadosExitosamente++;
                    resultado.MontoTotalAplicado += nuevoPago.TotalPagado;
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new Exception($"Falló la BD: {(ex.InnerException != null ? ex.InnerException.Message : ex.Message)}");
            }

            return resultado;
        }

        // Método auxiliar para no repetir código al crear la bitácora
        private TransaccionBancaria RegistrarTransaccionBase(Guid escuelaId, string banco, TransaccionBancariaDto tx, string usuario, EstatusTransaccion estatus)
        {
            return new TransaccionBancaria
            {
                EscuelaId = escuelaId,
                Banco = banco,
                ReferenciaBancaria = tx.Referencia,
                Monto = tx.Monto,
                FechaPago = tx.FechaPago,
                ClaveRastreo = tx.ClaveRastreo,
                Estatus = estatus,
                Usuario = usuario,
                FechaRegistro = DateTime.Now,
                Activo = true
            };
        }
    }
}