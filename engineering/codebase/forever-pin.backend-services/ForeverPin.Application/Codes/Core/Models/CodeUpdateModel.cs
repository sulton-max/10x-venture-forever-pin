using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Common.Domain.Codes.Core.Enums;

namespace ForeverPin.Application.Codes.Core.Models;

/// <summary>Pairs update input with the resolution mode loaded from the owner-scoped row.</summary>
public sealed record CodeUpdateModel(ContentMode Mode, CodeUpdateCommand Command);
