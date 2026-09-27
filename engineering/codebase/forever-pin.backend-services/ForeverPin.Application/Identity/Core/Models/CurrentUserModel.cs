using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;

namespace ForeverPin.Application.Identity.Core.Models;

/// <summary>Represents the current identity state.</summary>
/// <param name="Kind">How the caller is identified.</param>
/// <param name="User">Populated only for <see cref="UserKind.User"/>; <c>null</c> for guest/anonymous.</param>
public sealed record CurrentUserModel(UserKind Kind, UserSummaryModel? User);
