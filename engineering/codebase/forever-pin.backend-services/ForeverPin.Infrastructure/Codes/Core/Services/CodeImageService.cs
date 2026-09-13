using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using ForeverPin.Application.Settings;
using WoW.Two.Sdk.Backend.Beta.Codes;
using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Core.Enums;

using ForeverPin.Domain.Codes.Rules;

namespace ForeverPin.Infrastructure.Codes.Core.Services;

/// <summary>Provides image rendering for a code.</summary>
public sealed class CodeImageService(ICodeRenderer renderer, ApiSettings settings) : ICodeImageService
{
    /// <inheritdoc />
    public RenderedCode Render(CodeEntity code, ImageFormat format)
    {
        var shortUrl = $"{settings.RedirectBaseUrl.TrimEnd('/')}/{code.Slug}";
        var payload = CodePayloadMapper.Resolve(code.Mode, code.Rules, shortUrl);

        // Read the persisted style off the entity, falling back to the default for an empty StyleJson.
        var style = StyleSpecJson.Deserialize(code.StyleJson);

        return renderer.Render(new CodeRenderRequest
        {
            Payload = payload,
            Symbology = code.BarcodeFormat.ToRender(),
            Format = format,
            Style = style,
        });
    }
}
