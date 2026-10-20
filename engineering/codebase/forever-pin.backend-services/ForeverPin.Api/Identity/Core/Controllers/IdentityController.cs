using ForeverPin.Api.Configurations;
using ForeverPin.Api.Identity.Core.Models;
using ForeverPin.Api.Identity.Core.Requests;
using ForeverPin.Application.Identity.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using WoW.Two.Sdk.Beta.Be.Identity.CurrentUser;
using WoW.Two.Sdk.Beta.Be.Identity.Guest.Services;
using WoW.Two.Sdk.Beta.Be.Mediator;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;
using WoW.Two.Sdk.Beta.Be.Web.Contracts;
using WoW.Two.Sdk.Beta.Be.Web.ExceptionHandling.Extensions;

namespace ForeverPin.Api.Identity.Core.Controllers;

/// <summary>Exposes identity over HTTP.</summary>
[ApiController]
[Route("api/identity")]
public sealed class IdentityController(
    ISender sender,
    ICurrentUserService currentUser,
    IGuestSessionService guestSession) : ControllerBase
{
    /// <summary>Gets the current identity.</summary>
    [HttpGet("me")]
    [ProducesResponseType<ApiResponse<CurrentUserDto>>(StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var current = currentUser.Kind switch
        {
            UserKind.Guest => new CurrentUserDto(UserKind.Guest, User: null),
            UserKind.User  => new CurrentUserDto(UserKind.User, ReadUserFromClaims()),
            _              => new CurrentUserDto(UserKind.Anonymous, User: null),
        };

        return Ok(ApiResponse<CurrentUserDto>.Ok(current));
    }

    /// <summary>Begins sign-in.</summary>
    /// <remarks>Exchanges a Google ID token for the session cookie, claiming the guest's codes.</remarks>
    [EnableRateLimiting(RateLimitPolicies.Sessions)]
    [HttpPost("sign-in")]
    [ProducesResponseType<ApiResponse<CurrentUserDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn([FromBody] GoogleSignInApiRequest request, CancellationToken ct)
    {
        // Read the guest id (when present) before sign-in so the handler can claim the guest's codes.
        var guestId = currentUser.Kind == UserKind.Guest ? currentUser.Id : null;

        var command = request.ToCommand(guestId);
        var result = await sender.SendAsync(command, ct);

        return await result.Match<Task<IActionResult>>(
            async ok =>
            {
                await SignInAsync(ok.Data.User);
                guestSession.Clear();   // the auth cookie is now the identity — drop the redundant guest cookie.
                return Ok(ApiResponse<CurrentUserDto>.Ok(new CurrentUserDto(UserKind.User, ok.Data.User.ToDto())));
            },
            fail => Task.FromResult<IActionResult>(this.ToProblem(fail.Error)));
    }

    /// <summary>Signs the caller out.</summary>
    [HttpPost("sign-out")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SignOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        guestSession.Clear();   // also drop the guest cookie so a guest can sign out back to anonymous.
        return NoContent();
    }

    /// <summary>Provisions a guest session.</summary>
    [EnableRateLimiting(RateLimitPolicies.Sessions)]
    [HttpPost("guest")]
    [ProducesResponseType<ApiResponse<CurrentUserDto>>(StatusCodes.Status200OK)]
    public IActionResult Guest()
    {
        guestSession.EnsureGuest();

        var current = new CurrentUserDto(UserKind.Guest, User: null);

        return Ok(ApiResponse<CurrentUserDto>.Ok(current));
    }

    /// <summary>Builds the signed-in user's summary from the cookie-auth claims.</summary>
    private UserSummaryDto? ReadUserFromClaims()
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            return null;

        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var name = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        return new UserSummaryDto(userId, name, email);
    }

    /// <summary>Issues the session cookie for the signed-in account.</summary>
    private Task SignInAsync(UserSummaryModel user)
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
        ];

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}
