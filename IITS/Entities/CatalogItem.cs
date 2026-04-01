using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

/// <summary>Catálogo genérico para valores que se pueden agregar "en el acto" desde formularios (TipoDispositivo, Función, TipoInfraestructura, SistemaOperativo).</summary>
public class CatalogItem
{
    public Guid Id { get; set; }
    /// <summary>Kind: TipoDispositivo, Funcion, TipoInfraestructura, SistemaOperativo</summary>
    [Required, MaxLength(80)]
    public string Kind { get; set; } = "";
    [Required, MaxLength(200)]
    public string Name { get; set; } = "";
}
