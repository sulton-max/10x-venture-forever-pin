namespace ForeverPin.Application.Identity.Core.Models;

/// <summary>Represents a registered user's profile.</summary>
/// <param name="Id">Stable user identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="Email">Primary email address.</param>
public sealed record UserSummaryModel(Guid Id, string Name, string Email);
