using Gremelik.core.DTOs;

namespace Gremelik.core.Interfaces
{
    public interface ILectorLayoutBanco
    {
        // Recibe el archivo (Stream) y devuelve la lista de transacciones limpias
        List<TransaccionBancariaDto> ProcesarArchivo(Stream archivo);
    }
}