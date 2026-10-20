using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Common.Domain.Codes.Content.Wifi.Enums;
using ForeverPin.Domain.Codes.Content;
using ForeverPin.Domain.Codes.Content.Calendar.Models;
using ForeverPin.Domain.Codes.Content.Email.Models;
using ForeverPin.Domain.Codes.Content.Geo.Models;
using ForeverPin.Domain.Codes.Content.MobileApp.Models;
using ForeverPin.Domain.Codes.Content.Phone.Models;
using ForeverPin.Domain.Codes.Content.Sms.Models;
using ForeverPin.Domain.Codes.Content.Text.Models;
using ForeverPin.Domain.Codes.Content.Url.Models;
using ForeverPin.Domain.Codes.Content.VCard.Models;
using ForeverPin.Domain.Codes.Content.Wifi.Models;
using ForeverPin.Domain.Codes.Rules.Models;
using NodaTime;
using WoW.Two.Sdk.Beta.Be.Codes.Payloads.Exporters;
using WoW.Two.Sdk.Beta.Be.Codes.Payloads.Models;
using WoW.Two.Sdk.Beta.Be.Codes.Payloads.Serializers;
using WoW.Two.Sdk.Beta.Be.Foundation.Results;
using WoW.Two.Sdk.Beta.Be.Foundation.Validation;

namespace ForeverPin.Domain.Codes.Rules.Mappers;

/// <summary>Maps product content to SDK payload formats or the product short link.</summary>
public sealed class CodePayloadMapper(
    ICodePayloadSerializer serializer,
    IVCardExporter cards,
    IFloatingCalendarEventExporter events)
{
    /// <summary>Maps dynamic codes to their short link and static codes to one content payload.</summary>
    public Result<string> Resolve(ContentMode mode, IReadOnlyList<CodeRuleValueObject> rules, string shortUrl)
    {
        if (mode == ContentMode.Dynamic) return HttpUrl(shortUrl);
        if (mode != ContentMode.Static || rules is not { Count: 1 }) return Invalid("Rules", "A static code requires one content rule.");
        return Map(rules[0] switch
        {
            ConditionalRuleValueObject rule => rule.Content,
            DefaultRuleValueObject rule => rule.Content,
            _ => null,
        });
    }

    /// <summary>Maps one product content value through the SDK protocol implementations.</summary>
    public Result<string> Map(CodeContentValueObject? content) => content switch
    {
        UrlContentValueObject value => HttpUrl(value.Url),
        MobileAppLinkContentValueObject value => HttpUrl(value.Url),
        TextContentValueObject value => string.IsNullOrEmpty(value.Text)
            ? Invalid("Text", "Text content is required.")
            : Result<string>.Ok(value.Text),
        PhoneContentValueObject value => serializer.Serialize(new PhonePayloadModel { Phone = value.Phone }),
        SmsContentValueObject value => serializer.Serialize(new SmsPayloadModel { Phone = value.Phone, Message = value.Message }),
        EmailContentValueObject value => serializer.Serialize(new MailPayloadModel { Recipient = value.To, Subject = value.Subject, Body = value.Body }),
        GeoContentValueObject value => serializer.Serialize(new GeoPayloadModel { Latitude = value.Latitude, Longitude = value.Longitude }),
        WifiContentValueObject value => serializer.Serialize(new WifiPayloadModel
        {
            Ssid = value.Ssid, Password = value.Password, Authentication = value.Encryption switch
            {
                WifiEncryption.Wpa => WifiAuthentication.Wpa, WifiEncryption.Wep => WifiAuthentication.Wep,
                WifiEncryption.None => WifiAuthentication.None, _ => (WifiAuthentication)(-1),
            }, Hidden = value.Hidden,
        }),
        VCardContentValueObject value => Result<string>.Ok(cards.Export(new ContactCardModel
        {
            FirstName = value.FirstName, LastName = value.LastName, Organization = value.Org, Title = value.Title,
            Phone = value.Phone, Email = value.Email, Url = value.Url, Address = value.Address, Note = value.Note,
        })),
        CalendarContentValueObject value => events.Export(new FloatingCalendarEventModel
        {
            Title = value.Title, Start = LocalDateTime.FromDateTime(value.Start),
            End = value.End is { } end ? LocalDateTime.FromDateTime(end) : null,
            Location = value.Location, Description = value.Description,
        }),
        _ => Invalid("Content", "A supported content value is required."),
    };

    private static Result<string> HttpUrl(string? value) => value is not null
        && !value.Any(char.IsControl)
        && Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https"
        && !string.IsNullOrEmpty(uri.Host)
        && string.IsNullOrEmpty(uri.UserInfo)
            ? Result<string>.Ok(value)
            : Invalid("Url", "Use an absolute HTTP or HTTPS URL without credentials.");

    private static Result<string> Invalid(string property, string message) => Result<string>.Fail(ValidationError.From(
        [new FieldError { Property = property, Message = message, Code = "CodePayloadFormat" }]));
}
