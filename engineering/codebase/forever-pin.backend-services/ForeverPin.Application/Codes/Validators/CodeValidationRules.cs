namespace ForeverPin.Application.Codes.Validators;

/// <summary>Contains the shared validation predicates.</summary>
internal static class CodeValidationRules
{
    /// <summary>Returns whether the value is an absolute http or https URL.</summary>
    public static bool IsAbsoluteHttpUrl(string? value) =>
        value is not null && !value.Any(char.IsControl)
        && Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        && !string.IsNullOrEmpty(uri.Host)
        && string.IsNullOrEmpty(uri.UserInfo);
}
