using System.Text.Json;
using ForeverPin.Domain.Codes.Serialization.Factories;
using ForeverPin.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using ForeverPin.Domain.Codes.Core.Entities;

namespace ForeverPin.Infrastructure.Codes.Core.Extensions;

/// <summary>Extends <see cref="CodeEntity"/> for application model projection.</summary>
public static class CodeMappingExtensions
{
    /// <summary>Projects the code entity to its application model.</summary>
    public static CodeModel ToModel(this CodeEntity e, string redirectBaseUrl) => new()
    {
        Id = e.Id,
        Slug = e.Slug,
        ShortUrl = e.Slug is { } slug ? $"{redirectBaseUrl.TrimEnd('/')}/{slug}" : null,
        Name = e.Name,
        BarcodeFormat = e.BarcodeFormat,
        Mode = e.Mode,
        ContentType = e.ContentType,
        IsActive = e.IsActive,
        ScanCount = e.ScanCount,
        CreatedAt = e.CreatedAt,
        Rules = [.. e.Rules],
        Style = ReadStyle(e.StyleJson),
    };
    private static StyleSpec ReadStyle(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return StyleSpec.Default;
        try { return JsonSerializer.Deserialize<StyleSpec>(json, CodeJsonOptionsFactory.CreateStyle()) ?? StyleSpec.Default; }
        catch (JsonException) { return StyleSpec.Default; }
    }
}
