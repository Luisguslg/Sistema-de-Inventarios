using IITS.Entities;

namespace IITS.Services;

/// <summary>
/// Servicio de aplicaciones (lógica tipo repositorio + auditoría).
/// Las páginas usan este servicio en lugar de acceder al DbContext directamente.
/// </summary>
public interface IAplicacionService
{
    Task<List<Aplicacion>> GetAllAsync();
    Task<Aplicacion?> GetByIdAsync(Guid id);
    Task<List<Estatus>> GetEstatusListAsync();
    Task<Dictionary<Guid, Estatus>> GetEstatusDictionaryAsync();
    Task<Aplicacion> CreateAsync(Aplicacion entity);
    Task UpdateAsync(Aplicacion entity);
    Task DeleteAsync(Guid id);
}
