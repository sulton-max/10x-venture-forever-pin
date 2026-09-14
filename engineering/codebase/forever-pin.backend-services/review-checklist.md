# v0.9 model re-design — backend review checklist

> Historical review snapshot from 2026-07-28; links follow current filenames.
> Current tasks: [v0.9](../../planning/version-track/v0.9/v0.9.md). Historical verdicts do not establish current completion.

*Last updated: 2026-07-28*

> The backend half of the content-model **v2** (CM15) sweep. In Rider it sits under the **`docs`** solution folder (registered as a `<File>` in `forever-pin.backend-services.slnx`, since the Solution view only lists what the solution declares). Every path below is relative to the solution root, so the links resolve in-IDE.
> Frontend half: `../forever-pin.frontend-services/review-checklist.md`. Plan of record: `../../planning/version-track/v0.9/v0.9.md`.
>
> Range: `8477648` (*re-designed rule models and serialization*) → working tree, from `git diff --name-status`.
> `f3dc03f` (drop `CodeType`) and `0b33136` (drop expiry) preceded the sweep and are excluded.

## How to use

- The sweep's rule was **symmetry per layer**, so a layer isn't reviewed until its frontend half is read too — layers 1–6 and 9 exist on both sides.
- **Deleted** components are listed per layer: an incomplete deletion is the likeliest defect a file-by-file pass misses.
- Layer 7 is backend-only; layer 8 is frontend-only.

---

## 1 · Content

- [ ] [CodeContentValueObject.cs](ForeverPin.Domain/Codes/Content/CodeContentValueObject.cs) — abstract base + `SubtypeRegistry`
- [ ] 10 per-type models — [Url](ForeverPin.Domain/Codes/Content/Url/Models/UrlContentValueObject.cs) · [MobileAppLink](ForeverPin.Domain/Codes/Content/MobileApp/Models/MobileAppLinkContentValueObject.cs) · [Text](ForeverPin.Domain/Codes/Content/Text/Models/TextContentValueObject.cs) · [Email](ForeverPin.Domain/Codes/Content/Email/Models/EmailContentValueObject.cs) · [Sms](ForeverPin.Domain/Codes/Content/Sms/Models/SmsContentValueObject.cs) · [Phone](ForeverPin.Domain/Codes/Content/Phone/Models/PhoneContentValueObject.cs) · [Geo](ForeverPin.Domain/Codes/Content/Geo/Models/GeoContentValueObject.cs) · [Wifi](ForeverPin.Domain/Codes/Content/Wifi/Models/WifiContentValueObject.cs) · [VCard](ForeverPin.Domain/Codes/Content/VCard/Models/VCardContentValueObject.cs) · [Calendar](ForeverPin.Domain/Codes/Content/Calendar/Models/CalendarContentValueObject.cs)
- [ ] [CodeContentJson.cs](ForeverPin.Domain/Codes/Content/CodeContentJson.cs) · [ContentEncodingExtensions.cs](ForeverPin.Domain/Codes/Content/ContentEncodingExtensions.cs)
- [ ] Per-family extensions — [Wifi](ForeverPin.Domain/Codes/Content/Wifi/Extensions/WifiContentExtensions.cs) · [Sms](ForeverPin.Domain/Codes/Content/Sms/Extensions/SmsContentExtensions.cs)
- [ ] [CodeContentType.cs](ForeverPin.Domain/Codes/Core/Enums/CodeContentType.cs) — trimmed 26 → 10
- [ ] [MobileAppStoreType.cs](ForeverPin.Common.Domain/Codes/Content/MobileApp/Enums/MobileAppStoreType.cs) · [WifiEncryption.cs](ForeverPin.Common.Domain/Codes/Content/Wifi/Enums/WifiEncryption.cs)
- **Deleted:** `IContentTypeSpec` · `ContentTypes` · `MobileAppLinkContentSpec` · `CodeContentPolymorphism` · `CodeContentTypeExtensions` · `MobileAppStore` (→ `MobileAppStoreType`)
- *Re-scan verdict 2026-07-28: symmetric with FE, 10/10, optionality matches.*

## 2 · Condition

- [ ] [RuleConditionType.cs](ForeverPin.Domain/Codes/Core/Enums/RuleConditionType.cs) — `Default` removed as a condition (F4)
- *Re-scan verdict: 4 values both sides.*

## 3 · Rule

- [ ] [CodeRuleValueObject.cs](ForeverPin.Domain/Codes/Rules/Models/CodeRuleValueObject.cs) · [ConditionalRuleValueObject.cs](ForeverPin.Domain/Codes/Rules/Models/ConditionalRuleValueObject.cs) · [DefaultRuleValueObject.cs](ForeverPin.Domain/Codes/Rules/Models/DefaultRuleValueObject.cs) · [DefaultPointerRuleValueObject.cs](ForeverPin.Domain/Codes/Rules/Models/DefaultPointerRuleValueObject.cs)
- [ ] [CodeRuleType.cs](ForeverPin.Domain/Codes/Rules/Enums/CodeRuleType.cs) · [CodeRuleJson.cs](ForeverPin.Domain/Codes/Rules/CodeRuleJson.cs) · [CodePayloadMapper.cs](ForeverPin.Domain/Codes/Rules/CodePayloadMapper.cs)
- **Deleted:** `RoutingRuleEntity` · `RoutingRuleEntityConfiguration` · `RuleDto` · `RuleApiRequest`
- *Re-scan verdict: 3 roles symmetric. Round-trip locked by `CodeRuleJsonTests`.*

## 4 · Code + entity

- [ ] [CodeEntity.cs](ForeverPin.Domain/Codes/Core/Entities/CodeEntity.cs) — `Content` dropped · `Mode` · `ContentType` · nullable `Slug` · `List<CodeRule> Rules` · **`StyleJson` now `required`**
- [ ] [ScanEventEntity.cs](ForeverPin.Domain/Codes/Core/Entities/ScanEventEntity.cs) — `MatchedRuleId` → `MatchedRuleOrder`
- [ ] [CodeDto.cs](ForeverPin.Application/Codes/Core/Models/CodeDto.cs)
- [ ] [ContentMode.cs](ForeverPin.Common.Domain/Codes/Core/Enums/ContentMode.cs)
- [ ] Persistence — [CodeEntityConfiguration.cs](ForeverPin.Persistence/Configurations/CodeEntityConfiguration.cs) · [AppDbContext.cs](ForeverPin.Persistence/DataContexts/AppDbContext.cs) · migration [010-rules-jsonb](ForeverPin.Persistence/Migrations/010-rules-jsonb)
- [ ] [SubtypeRegistry.cs](ForeverPin.Common.Domain/Serialization/SubtypeRegistry.cs) · [SubtypeRegistryJsonExtensions.cs](ForeverPin.Common.Domain/Serialization/Json/SubtypeRegistryJsonExtensions.cs) · [JsonbOptions.cs](ForeverPin.Common.Domain/Serialization/Json/JsonbOptions.cs)
- *Re-scan verdict: symmetric. CS8618 on `StyleJson` fixed.*

## 5 · Requests + serialization

- [ ] [CreateCodeApiRequest.cs](ForeverPin.Api/Requests/Codes/CreateCodeApiRequest.cs) · [UpdateCodeApiRequest.cs](ForeverPin.Api/Requests/Codes/UpdateCodeApiRequest.cs) · [PreviewCodeApiRequest.cs](ForeverPin.Api/Requests/Codes/PreviewCodeApiRequest.cs) — **`Style` required on all three**
- [ ] [StyleApiRequest.cs](ForeverPin.Api/Requests/Codes/StyleApiRequest.cs) + `ToStyleSpec` — defaults nothing; only `Logo`/`Gradient`/`Emoji` nullable
- [ ] [CodeCreateCommand.cs](ForeverPin.Application/Codes/Core/Commands/CodeCreateCommand.cs) · [CodeUpdateCommand.cs](ForeverPin.Application/Codes/Core/Commands/CodeUpdateCommand.cs) — **`Style` required**
- [ ] [CodeListQuery.cs](ForeverPin.Application/Codes/Core/Queries/CodeListQuery.cs) · [ICodeRepository.cs](ForeverPin.Application/Codes/Core/Services/ICodeRepository.cs) · [CodeRepository.cs](ForeverPin.Infrastructure/Persistence/Repositories/CodeRepository.cs) · [CodeMappingExtensions.cs](ForeverPin.Infrastructure/Codes/Core/Extensions/CodeMappingExtensions.cs)
- [ ] [CodeCreateCommandHandler.cs](ForeverPin.Infrastructure/Codes/Core/CommandHandlers/CodeCreateCommandHandler.cs) · [CodeUpdateCommandHandler.cs](ForeverPin.Infrastructure/Codes/Core/CommandHandlers/CodeUpdateCommandHandler.cs) — style branches removed
- [ ] [CodesController.cs](ForeverPin.Api/Controllers/CodesController.cs) · [HostConfiguration.Extensions.cs](ForeverPin.Api/Configurations/HostConfiguration.Extensions.cs)
- [ ] [CodeImageService.cs](ForeverPin.Infrastructure/Codes/Core/Services/CodeImageService.cs) — payload via `CodePayloadMapper.Resolve`

## 6 · Validation

- [ ] [CodeCreateCommandValidator.cs](ForeverPin.Application/Codes/Core/Validators/CodeCreateCommandValidator.cs) · [CodeUpdateCommandValidator.cs](ForeverPin.Application/Codes/Core/Validators/CodeUpdateCommandValidator.cs) — near-identical **by nature**, not duplication (P4); don't merge
- [ ] [CodeRuleValidator.cs](ForeverPin.Application/Codes/Rules/Validators/CodeRuleValidator.cs) — dispatch table only
- [ ] [ConditionalRuleValidator.cs](ForeverPin.Application/Codes/Rules/Validators/ConditionalRuleValidator.cs) · [DefaultRuleValidator.cs](ForeverPin.Application/Codes/Rules/Validators/DefaultRuleValidator.cs) · [DefaultPointerRuleValidator.cs](ForeverPin.Application/Codes/Rules/Validators/DefaultPointerRuleValidator.cs)
- [ ] [CodeRuleSetValidator.cs](ForeverPin.Application/Codes/Rules/Validators/CodeRuleSetValidator.cs) — every rule carries an explicit `OverridePropertyName`; the names are a **wire contract**
- [ ] [CodeRuleSet.cs](ForeverPin.Application/Codes/Rules/Models/CodeRuleSet.cs) — the shared validation subject (exclusive-members bar: exempt)
- [ ] [CodeContentValidator.cs](ForeverPin.Application/Codes/Content/Validators/CodeContentValidator.cs) + [10 per-type validators](ForeverPin.Application/Codes/Content/Validators)
- [ ] [CodeValidationRules.cs](ForeverPin.Application/Codes/Validators/CodeValidationRules.cs)
- **Deleted:** `Codes/Core/Validation/` (old `CodeCreate`/`CodeUpdate`/`ContentValidation`/`RuleDtoValidator`) · `MobileApp/Validation/MobileAppLinkContentValidator`
- **Folder rule:** `Validation/` → `Validators/` everywhere

## 7 · Routing (backend-only)

- [ ] [RoutingResult.cs](ForeverPin.Redirect.Api/Application/Routing/Models/RoutingResult.cs) — union replacing `RouteDecision` + `RouteOutcome`
- [ ] [RoutingService.cs](ForeverPin.Redirect.Api/Infrastructure/Routing/RoutingService.cs) · [IRoutingService.cs](ForeverPin.Redirect.Api/Application/Routing/Services/IRoutingService.cs)
- [ ] [RedirectEndpoints.cs](ForeverPin.Redirect.Api/Endpoints/RedirectEndpoints.cs)
- [ ] [ScanRecord.cs](ForeverPin.Redirect.Api/Application/Analytics/Models/ScanRecord.cs) · [ScanFlushBackgroundService.cs](ForeverPin.Redirect.Api/Infrastructure/Analytics/ScanFlushBackgroundService.cs) · [CachedRedirectCodeRepository.cs](ForeverPin.Redirect.Api/Infrastructure/Routing/CachedRedirectCodeRepository.cs) · [DbRedirectCodeRepository.cs](ForeverPin.Redirect.Api/Infrastructure/Routing/DbRedirectCodeRepository.cs)
- **Deleted:** `RouteDecision` · `RouteOutcome`
- ⚠ **CM9 rewrites this layer next** — review for correctness, not for polish.

## 9 · Tests

- [ ] Unit — [CodeRuleJsonTests](ForeverPin.Tests.Unit/CodeRuleJsonTests.cs) **(new)** · [CodeValidationPathTests](ForeverPin.Tests.Unit/CodeValidationPathTests.cs) **(new)** · [CodeContentJsonTests](ForeverPin.Tests.Unit/CodeContentJsonTests.cs) · [CodeContentEncodeTests](ForeverPin.Tests.Unit/CodeContentEncodeTests.cs) · [CodeImageServiceTests](ForeverPin.Tests.Unit/CodeImageServiceTests.cs) · [RoutingServiceTests](ForeverPin.Tests.Unit/RoutingServiceTests.cs)
- [ ] Integration — [CodeRepositoryTests](ForeverPin.Tests.Integration/Tests/CodeRepositoryTests.cs) · [RedirectResolutionTests](ForeverPin.Tests.Integration/Tests/RedirectResolutionTests.cs)
- [ ] E2E — [HttpExtensions](ForeverPin.Tests.E2E/Support/HttpExtensions.cs) (`CodeRequests.Style()`) · [ApiContracts](ForeverPin.Tests.E2E/Support/ApiContracts.cs) · [AuthTests](ForeverPin.Tests.E2E/Tests/AuthTests.cs) · [BillingTests](ForeverPin.Tests.E2E/Tests/BillingTests.cs) · [CodeImageTests](ForeverPin.Tests.E2E/Tests/CodeImageTests.cs) · [CodePreviewTests](ForeverPin.Tests.E2E/Tests/CodePreviewTests.cs) · [CodesCrudTests](ForeverPin.Tests.E2E/Tests/CodesCrudTests.cs) · [RedirectWedgeTests](ForeverPin.Tests.E2E/Tests/RedirectWedgeTests.cs)
- **Deleted:** `MobileAppLinkContentSpecTests`
- Historical counts: Unit **114** · Integration **18** · E2E **61**. Current results: [verification](../../operations/verification.md).

---

## Remaining work

The [current-work analysis](../../planning/current-work.md) supersedes the old known-open list.
Review checkboxes above remain unchecked; renaming their links does not certify the review.
