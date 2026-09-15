using ForeverPin.Common.Domain.Codes.Core.Enums;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using ForeverPin.Application.Codes.Core.Models;
using ForeverPin.Domain.Codes.Rules.Models;
using ForeverPin.Domain.Codes.Core.Enums;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace ForeverPin.Application.Codes.Core.Commands;

/// <summary>Represents a command to create a code.</summary>
public sealed record CodeCreateCommand
    : ICommand<AppResult<CodeCreateResult.Success>>
{
    /// <summary>Gets the id of the user creating the code.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Gets the display name of the code.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the rendering symbology of the code.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Gets the kind of content every rule carries.</summary>
    public required CodeContentType ContentType { get; init; }

    /// <summary>Gets the code's content resolution mode.</summary>
    public required ContentMode Mode { get; init; }

    /// <summary>Gets the code's collection of content-bearing routing rules.</summary>
    public required IReadOnlyList<CodeRuleValueObject> Rules { get; init; }

    /// <summary>Gets the style the code renders with.</summary>
    public required StyleSpec Style { get; init; }

}
