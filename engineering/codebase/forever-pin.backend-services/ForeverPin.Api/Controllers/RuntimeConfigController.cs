using ForeverPin.Application.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForeverPin.Api.Controllers;

/// <summary>Exposes public runtime settings so one image can serve different environments.</summary>
[ApiController]
[Route("api/runtime-config")]
public sealed class RuntimeConfigController(AuthSettings auth, ApiSettings api) : ControllerBase
{
    /// <summary>Gets the public Google audience and redirect origin without returning secrets.</summary>
    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Get() => Ok(new { googleClientId = auth.Google.ClientId, redirectBaseUrl = api.RedirectBaseUrl });
}
