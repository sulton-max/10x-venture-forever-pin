using System.Text.Json;
using ForeverPin.Domain.Codes.Serialization.Factories;
using WoW.Two.Sdk.Backend.Beta.Foundation.Results;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using ForeverPin.Application.Settings;
using WoW.Two.Sdk.Backend.Beta.Codes;
using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Core.Enums;

using ForeverPin.Domain.Codes.Rules.Mappers;

namespace ForeverPin.Infrastructure.Codes.Core.Services;

/// <summary>Provides image rendering for a code.</summary>
public sealed class CodeImageService(ICodeRenderer renderer, ApiSettings settings, CodePayloadMapper payloads) : ICodeImageService
{
    /// <inheritdoc />
    public Result<RenderedCode> Render(CodeEntity code, ImageFormat format)
    {
        var shortUrl = $"{settings.RedirectBaseUrl.TrimEnd('/')}/{code.Slug}";
        var payload = payloads.Resolve(code.Mode, code.Rules, shortUrl);

        if (payload is Result<string>.Failure failure) return Result<RenderedCode>.Fail(failure.Error);

        var style = StyleSpec.Default;
        if (!string.IsNullOrWhiteSpace(code.StyleJson))
        {
            try { style = JsonSerializer.Deserialize<StyleSpec>(code.StyleJson, CodeJsonOptionsFactory.CreateStyle()) ?? StyleSpec.Default; }
            catch (JsonException) { /* Legacy malformed styles retain the default recovery contract. */ }
        }

        return renderer.Render(new CodeRenderRequest
        {
            Payload = ((Result<string>.Success)payload).Value,
            Symbology = code.BarcodeFormat.ToRender(),
            Format = format,
            Style = style,
        });
    }
}
