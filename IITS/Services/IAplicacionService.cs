using IITS.Entities;

namespace IITS.Services;

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
