using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;

namespace ForeverPin.Application.Identity.Core.Models;

/// <summary>Represents the current identity state.</summary>
/// <param name="Kind">How the caller is identified.</param>
/// <param name="User">Populated only for <see cref="UserKind.User"/>; <c>null</c> for guest/anonymous.</param>
public sealed record CurrentUserDto(UserKind Kind, UserSummaryDto? User);

/// <summary>Represents a registered user's profile.</summary>
/// <param name="Id">Stable user identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="Email">Primary email address.</param>
public sealed record UserSummaryDto(Guid Id, string Name, string Email);
