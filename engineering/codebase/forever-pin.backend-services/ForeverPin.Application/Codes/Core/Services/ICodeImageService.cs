using WoW.Two.Sdk.Beta.Be.Foundation.Results;
using WoW.Two.Sdk.Beta.Be.Codes.Models;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Core.Enums;

namespace ForeverPin.Application.Codes.Core.Services;

/// <summary>Defines rendering a code's printable image.</summary>
public interface ICodeImageService
{
    /// <summary>Renders the code's image in the requested format.</summary>
    Result<RenderedCode> Render(CodeEntity code, ImageFormat format);
}
