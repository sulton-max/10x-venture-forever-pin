using ForeverPin.Application.Identity.Core.Models;

namespace ForeverPin.Api.Identity.Core.Models;

/// <summary>Represents a registered user's profile.</summary>
/// <param name="Id">Stable user identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="Email">Primary email address.</param>
public sealed record UserSummaryDto(Guid Id, string Name, string Email);

/// <summary>Maps the application UserSummaryModel into its HTTP projection.</summary>
public static class UserSummaryDtoExtensions
{
    /// <summary>Maps the application fields without changing the wire contract.</summary>
    public static UserSummaryDto ToDto(this UserSummaryModel model) => new(model.Id, model.Name, model.Email);
}
