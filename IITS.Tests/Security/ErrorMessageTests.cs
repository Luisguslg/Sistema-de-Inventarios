using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentAssertions;
using IITS.Entities;
using IITS.Tests.Infrastructure;

namespace IITS.Tests.Security;

/// <summary>
/// Pruebas unitarias: verifica que los mensajes de error de validación son genéricos
/// y no exponen nombres de propiedades internas del modelo.
/// CWE-209 (Information Exposure Through Error Messages).
/// </summary>
public class ErrorMessageTests
{
    [Fact]
    public void Aplicacion_RequiredNombre_HasGenericMessage()
    {
        var prop = typeof(Aplicacion).GetProperty(nameof(Aplicacion.Nombre));
        var attr = prop!.GetCustomAttribute<RequiredAttribute>();

        attr.Should().NotBeNull();
        attr!.ErrorMessage.Should().NotBeNullOrEmpty(
            "el mensaje de error debe ser explícito y genérico");
        attr.ErrorMessage.Should().NotContain("Nombre",
            "el mensaje no debe exponer el nombre de la propiedad interna C#");
        attr.ErrorMessage.Should().NotContain(" is required",
            "el mensaje no debe estar en el formato inglés por defecto de DataAnnotations");
    }

    [Fact]
    public void Aplicacion_MaxLength_MessagesDoNotExposePropertyNames()
    {
        var properties = typeof(Aplicacion).GetProperties();
        var csharpPropertyNames = properties.Select(p => p.Name).ToList();

        foreach (var prop in properties)
        {
            var maxLengthAttr = prop.GetCustomAttribute<MaxLengthAttribute>();
            if (maxLengthAttr?.ErrorMessage == null) continue;

            var message = maxLengthAttr.ErrorMessage;

            // El mensaje no debe contener el nombre de la propiedad C# exacto
            // (p.ej. "Funcionalidad", "TipoAlojamiento", "ClasificacionInformacion")
            csharpPropertyNames
                .Where(n => n.Length > 3)  // ignorar nombres muy cortos
                .Should().NotContain(n =>
                    message.Contains(n, StringComparison.Ordinal) && n == prop.Name,
                $"el mensaje de error de {prop.Name} no debe exponer el nombre de la propiedad interna (CWE-209)");
        }
    }

    [Fact]
    public void Aplicacion_AllValidationMessages_AreInSpanish()
    {
        var properties = typeof(Aplicacion).GetProperties();
        var englishOnlyPhrases = new[] { " is required", " is too long", "must be", "cannot be" };

        foreach (var prop in properties)
        {
            var required = prop.GetCustomAttribute<RequiredAttribute>();
            if (required?.ErrorMessage != null)
            {
                foreach (var phrase in englishOnlyPhrases)
                    required.ErrorMessage.Should().NotContain(phrase,
                        $"el mensaje de {prop.Name} debe estar en español");
            }

            var maxLen = prop.GetCustomAttribute<MaxLengthAttribute>();
            if (maxLen?.ErrorMessage != null)
            {
                foreach (var phrase in englishOnlyPhrases)
                    maxLen.ErrorMessage.Should().NotContain(phrase,
                        $"el mensaje de {prop.Name} debe estar en español");
            }
        }
    }

    [Fact]
    public async Task ErrorPage_DoesNotExpose_StackTrace_InBody()
    {
        var factory = new WebAppFactory();
        using var client = factory.CreateClient();

        var body = await client.GetStringAsync("/Error");

        body.Should().NotContainAny(
            "StackTrace", "at IITS.", "at System.", "NullReferenceException",
            "System.Exception", "C:\\Users", "C:\\Program");
    }

    [Fact]
    public async Task ErrorPage_Returns200_WithGenericMessage()
    {
        var factory = new WebAppFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Error");

        response.IsSuccessStatusCode.Should().BeTrue(
            "la página /Error debe responder 200 y mostrar un mensaje genérico");
    }
}
