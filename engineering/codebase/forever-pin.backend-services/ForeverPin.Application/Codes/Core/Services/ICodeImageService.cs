using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Core.Enums;

namespace ForeverPin.Application.Codes.Core.Services;

/// <summary>Defines rendering a code's printable image.</summary>
public interface ICodeImageService
{
    /// <summary>Renders the code's image in the requested format.</summary>
    RenderedCode Render(CodeEntity code, ImageFormat format);
}
