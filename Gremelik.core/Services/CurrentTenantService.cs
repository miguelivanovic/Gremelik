namespace Gremelik.core.Services
{
    // Esta clase es una cajita simple para guardar quién es el usuario actual
    public class CurrentTenantService
    {
        public string? Subdominio { get; set; }

        public Guid? TenantId { get; set; }
    }
}
