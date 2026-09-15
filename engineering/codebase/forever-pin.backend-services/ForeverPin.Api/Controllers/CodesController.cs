using Microsoft.AspNetCore.Mvc;
using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Core.Models;
using ForeverPin.Application.Codes.Core.Queries;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using ForeverPin.Api.Requests.Codes;
using WoW.Two.Sdk.Backend.Beta.Codes;
using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using ForeverPin.Application.Settings;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules;
using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;
using WoW.Two.Sdk.Backend.Beta.Mediator;
using WoW.Two.Sdk.Backend.Beta.Web.Contracts;

namespace ForeverPin.Api.Controllers;

/// <summary>Manages codes.</summary>
[ApiController]
[Route("api/codes")]
public sealed class CodesController(
    ISender sender,
    ICodeRepository repository,
    ICodeImageService imageService,
    ICodeRenderer renderer,
    ICurrentUser currentUser,
    ApiSettings settings) : ControllerBase
{
    // Placeholder slug for an unsaved dynamic preview.
    private const string SlugPlaceholder = "preview";


    /// <summary>Renders an unsaved SVG preview from the supplied style.</summary>
    /// <remarks>Anonymous-or-guest allowed: it is a pure render with no ownership.</remarks>
    [HttpPost("preview")]
    [Produces("image/svg+xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Preview([FromBody] PreviewCodeApiRequest request)
    {
        // Use the saved-image resolver with a placeholder short URL for dynamic previews.
        var rendered = renderer.Render(new CodeRenderRequest
        {
            Payload = CodePayloadMapper.Resolve(
                request.Mode,
                request.Rules,
                $"{settings.RedirectBaseUrl.TrimEnd('/')}/{SlugPlaceholder}"),
            Symbology = request.ResolveSymbology().ToRender(),
            Format = ImageFormat.Svg,
            Style = request.ToStyleSpec(),
        });

        return File(rendered.Content, rendered.ContentType);
    }

    /// <summary>Creates a code.</summary>
    [HttpPost]
    [ProducesResponseType<ApiResponse<CodeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<IActionResult> Create([FromBody] CreateCodeApiRequest request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var command = request.ToCommand(userId);
        var result = await sender.SendAsync(command, ct);

        return result.Match<IActionResult>(
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code)),
            fail => this.ToProblem(fail.Error));
    }

    /// <summary>Gets a code by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<CodeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var query = new CodeGetByIdQuery { Id = id, UserId = userId };
        var result = await sender.SendAsync(query, ct);

        return result.Match<IActionResult>(
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code)),
            fail => this.ToProblem(fail.Error));
    }

    /// <summary>Gets all codes.</summary>
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<CodeDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken ct, [FromQuery] string? q = null)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var query = new CodeListQuery { UserId = userId, Q = q };
        var result = await sender.SendAsync(query, ct);

        return result.Match<IActionResult>(
            ok => Ok(ApiResponse<IReadOnlyList<CodeDto>>.Ok(ok.Data.Codes)),
            fail => this.ToProblem(fail.Error));
    }

    /// <summary>Updates a code by id.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<CodeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateById(Guid id, [FromBody] UpdateCodeApiRequest request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var command = request.ToCommand(id, userId);
        var result = await sender.SendAsync(command, ct);

        return result.Match<IActionResult>(
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code)),
            fail => this.ToProblem(fail.Error));
    }

    /// <summary>Sets a code's active state by id.</summary>
    [HttpPatch("{id:guid}/active")]
    [ProducesResponseType<ApiResponse<CodeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActiveById(
        Guid id,
        [FromBody] SetActiveCodeApiRequest request,
        CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var command = request.ToCommand(id, userId);
        var result = await sender.SendAsync(command, ct);

        return result.Match<IActionResult>(
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code)),
            fail => this.ToProblem(fail.Error));
    }

    /// <summary>Deletes a code by id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteById(Guid id, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var command = new CodeDeleteCommand { Id = id, UserId = userId };
        var result = await sender.SendAsync(command, ct);

        return result.Match<IActionResult>(
            NoContent,
            fail => this.ToProblem(fail.Error));
    }

    /// <summary>Gets a code's rendered image by id.</summary>
    [HttpGet("{id:guid}/image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImageById(Guid id, CancellationToken ct, [FromQuery] string format = "svg")
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var code = await repository.GetByIdForUserAsync(id, userId, ct);
        if (code is null)
            return NotFound();

        var imageFormat = format.Equals("png", StringComparison.OrdinalIgnoreCase)
            ? ImageFormat.Png
            : ImageFormat.Svg;

        var rendered = imageService.Render(code, imageFormat);
        return File(rendered.Content, rendered.ContentType);
    }
}
