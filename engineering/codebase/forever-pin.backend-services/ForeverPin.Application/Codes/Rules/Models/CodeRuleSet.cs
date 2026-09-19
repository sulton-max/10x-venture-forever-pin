using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules.Models;

namespace ForeverPin.Application.Codes.Rules.Models;

/// <summary>Represents a code's routing rules, content type, and optional resolution mode.</summary>
/// <param name="Mode">How the code's symbol resolves; absent on update, where mode is immutable.</param>
/// <param name="ContentType">The kind of content every rule must carry.</param>
/// <param name="Rules">The rules being validated.</param>
public sealed record CodeRuleSet(
    ContentMode? Mode,
    CodeContentType ContentType,
    IReadOnlyList<CodeRuleValueObject> Rules);
