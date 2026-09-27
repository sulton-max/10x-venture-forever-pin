using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Application.Codes.Rules.Models;
using ForeverPin.Domain.Codes.Content;
using ForeverPin.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Foundation.Results;
using WoW.Two.Sdk.Backend.Beta.Foundation.Validation;
using WoW.Two.Sdk.Backend.Beta.Web.ExceptionHandling.Extensions;
using Microsoft.AspNetCore.Mvc;
using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Api.Codes.Core.Models;
using ForeverPin.Application.Codes.Core.Queries;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using ForeverPin.Api.Codes.Core.Requests;
using WoW.Two.Sdk.Backend.Beta.Codes;
using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using ForeverPin.Application.Settings;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules.Mappers;
using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;
using WoW.Two.Sdk.Backend.Beta.Mediator;
using WoW.Two.Sdk.Backend.Beta.Web.Contracts;
using WoW.Two.Sdk.Backend.Beta.Web.ErrorMapping;

namespace ForeverPin.Api.Codes.Core.Controllers;

/// <summary>Manages codes.</summary>
[ApiController]
[Route("api/codes")]
public sealed class CodesController(
    ISender sender,
    ICodeRepository repository,
    ICodeImageService imageService,
    ICodeRenderer renderer,
    CodePayloadMapper payloadMapper,
    IValidator<CodeRuleSetModel> ruleSetValidator,
    IValidator<CodeRuleValueObject> ruleValidator,
    ICurrentUserService currentUser,
    IValidationAdvisoryMapper advisories,
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
        if (!Enum.IsDefined(request.Mode) || !Enum.IsDefined(request.ResolveSymbology()))
            return this.ToProblem(AppError.Of(AppErrorType.Validation, "Unsupported content mode or symbology."));
        if (request.Rules is null || request.Rules.Count == 0 || request.Rules.Any(rule => rule is null))
            return this.ToProblem(ValidationError.From([
                new() { Property = "Rules", Message = "At least one non-null rule is required.", Code = "NotEmptyValidator" },
            ]));

        for (var index = 0; index < request.Rules.Count; index++)
        {
            if (ruleValidator.Validate(request.Rules[index]) is { } ruleError)
                return this.ToProblem(ValidationError.From(ruleError.Failures
                    .Select(failure => failure with { Property = $"Rules[{index}].{failure.Property}" }).ToArray()));
        }

        var firstContent = request.Rules.Select(rule => rule switch
        {
            DefaultRuleValueObject fallback => fallback.Content,
            ConditionalRuleValueObject conditional => conditional.Content,
            _ => null,
        }).FirstOrDefault(content => content is not null);
        if (firstContent is null)
            return this.ToProblem(AppError.Of(AppErrorType.Validation, "Preview requires content."));
        var ruleSet = new CodeRuleSetModel(request.Mode, CodeContentValueObject.Subtypes.KindOf(firstContent), request.Rules);
        if (ruleSetValidator.Validate(ruleSet) is { } error) return this.ToProblem(error);

        var payload = payloadMapper.Resolve(request.Mode, request.Rules,
            $"{settings.RedirectBaseUrl.TrimEnd('/')}/{SlugPlaceholder}");
        if (payload is Result<string>.Failure payloadFailure) return this.ToProblem(payloadFailure.Error);
        var rendered = renderer.Render(new CodeRenderRequest
        {
            Payload = ((Result<string>.Success)payload).Value,
            Symbology = request.ResolveSymbology().ToRender(),
            Format = ImageFormat.Svg,
            Style = request.ToStyleSpec(),
        });
        return rendered.Match<IActionResult>(image => File(image.Content, image.ContentType),
            error => this.ToProblem(MapPreviewRenderError(error, request.Mode)));
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
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code.ToDto(), advisories.Map(HttpContext))),
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
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code.ToDto())),
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
            ok => Ok(ApiResponse<IReadOnlyList<CodeDto>>.Ok(ok.Data.Codes.Select(code => code.ToDto()).ToArray())),
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
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code.ToDto(), advisories.Map(HttpContext))),
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
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code.ToDto())),
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

        if (!format.Equals("png", StringComparison.OrdinalIgnoreCase) && !format.Equals("svg", StringComparison.OrdinalIgnoreCase))
            return this.ToProblem(AppError.Of(AppErrorType.Validation, "Supported image formats are svg and png."));
        var imageFormat = format.Equals("png", StringComparison.OrdinalIgnoreCase)
            ? ImageFormat.Png
            : ImageFormat.Svg;

        var rendered = imageService.Render(code, imageFormat);
        return rendered.Match<IActionResult>(image => File(image.Content, image.ContentType), this.ToProblem);
    }

    private static AppError MapPreviewRenderError(AppError error, ContentMode mode)
    {
        if (error is not ValidationError validation) return error;
        var fields = new List<FieldError>();
        foreach (var field in validation.Failures)
        {
            if (field.Property == "Payload")
            {
                if (mode == ContentMode.Static)
                    fields.Add(field with { Property = "Rules[0].Content" });
                continue;
            }
            fields.Add(field.Property == "Symbology" ? field with { Property = "BarcodeFormat" } : field);
        }

        return fields.Count > 0
            ? validation with { Failures = fields }
            : AppError.Of(AppErrorType.Validation, "The generated short link cannot be rendered with the selected barcode format.");
    }
}
