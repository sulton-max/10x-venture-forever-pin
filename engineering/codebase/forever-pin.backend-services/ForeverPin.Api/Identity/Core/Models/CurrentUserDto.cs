using ForeverPin.Application.Identity.Core.Models;
using WoW.Two.Sdk.Beta.Be.Identity.CurrentUser;

namespace ForeverPin.Api.Identity.Core.Models;

/// <summary>Represents the current identity state.</summary>
/// <param name="Kind">How the caller is identified.</param>
/// <param name="User">Populated only for <see cref="UserKind.User"/>; <c>null</c> for guest/anonymous.</param>
public sealed record CurrentUserDto(UserKind Kind, UserSummaryDto? User);

/// <summary>Maps the application CurrentUserModel into its HTTP projection.</summary>
public static class CurrentUserDtoExtensions
{
    /// <summary>Maps the application fields without changing the wire contract.</summary>
    public static CurrentUserDto ToDto(this CurrentUserModel model) => new(model.Kind, model.User?.ToDto());
}
