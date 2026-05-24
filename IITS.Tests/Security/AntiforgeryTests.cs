using System.Reflection;
using FluentAssertions;
using IITS.Pages;
using Microsoft.AspNetCore.Mvc;

namespace IITS.Tests.Security;

/// <summary>
/// Pruebas de reflexión: verifica que la remediación CWE-352 se aplicó correctamente.
/// [IgnoreAntiforgeryToken] fue eliminado de ErrorModel para no suprimir la protección CSRF.
/// </summary>
public class AntiforgeryTests
{
    [Fact]
    public void ErrorModel_DoesNotHave_IgnoreAntiforgeryToken_Attribute()
    {
        var attr = typeof(ErrorModel).GetCustomAttribute<IgnoreAntiforgeryTokenAttribute>();

        attr.Should().BeNull(
            "el atributo [IgnoreAntiforgeryToken] fue eliminado como remediación CWE-352; " +
            "su presencia deshabilitaría la validación CSRF en la página de error");
    }

    [Fact]
    public void ErrorModel_Has_ResponseCache_NoStore_Attribute()
    {
        var attr = typeof(ErrorModel).GetCustomAttribute<ResponseCacheAttribute>();

        attr.Should().NotBeNull("el atributo [ResponseCache] debe estar presente");
        attr!.NoStore.Should().BeTrue(
            "NoStore=true previene que la respuesta de error sea almacenada en caché");
        attr.Duration.Should().Be(0);
    }

    [Fact]
    public void ErrorModel_OnGet_ExposesRequestId_NotStackTrace()
    {
        // Verificar vía reflexión que OnGet() solo asigna RequestId,
        // no expone propiedades de stack o exception
        var type = typeof(ErrorModel);

        type.GetProperty("RequestId").Should().NotBeNull(
            "RequestId se usa para correlación de logs, no es info sensible");
        type.GetProperty("StackTrace").Should().BeNull(
            "ErrorModel no debe exponer StackTrace como propiedad pública (CWE-209)");
        type.GetProperty("ExceptionMessage").Should().BeNull(
            "ErrorModel no debe exponer detalles de la excepción (CWE-209)");
    }

    [Fact]
    public void ErrorModel_ShowRequestId_ReturnsFalse_WhenRequestIdIsNull()
    {
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ErrorModel>.Instance;
        var model = new ErrorModel(logger);

        // RequestId es null por defecto
        model.ShowRequestId.Should().BeFalse(
            "ShowRequestId debe ser false cuando no hay RequestId, evitando mostrar info vacía");
    }
}
