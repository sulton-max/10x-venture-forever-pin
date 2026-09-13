using FluentValidation;
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
using ForeverPin.Domain.Codes.Content;

namespace ForeverPin.Application.Codes.Content.Validators;

/// <summary>Validates the content a rule carries, dispatching to the matching per-type validator.</summary>
/// <remarks>Register a new content type here and in its own validator.</remarks>
public sealed class CodeContentValidator : AbstractValidator<CodeContentValueObject>
{
    /// <summary>Builds the per-type content dispatch.</summary>
    public CodeContentValidator()
    {
        RuleFor(content => content).SetInheritanceValidator(v =>
        {
            v.Add(new UrlContentValidator());
            v.Add(new MobileAppLinkContentValidator());
            v.Add(new TextContentValidator());
            v.Add(new EmailContentValidator());
            v.Add(new SmsContentValidator());
            v.Add(new PhoneContentValidator());
            v.Add(new GeoContentValidator());
            v.Add(new WifiContentValidator());
            v.Add(new VCardContentValidator());
            v.Add(new CalendarContentValidator());
        });
    }
}
