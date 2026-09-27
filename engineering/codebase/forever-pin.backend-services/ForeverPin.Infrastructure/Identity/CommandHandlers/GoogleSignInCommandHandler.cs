using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Application.Identity.Core.Commands;
using ForeverPin.Application.Identity.Core.Models;
using ForeverPin.Application.Identity.Core.Services;
using ForeverPin.Domain.Identity.Entities;
using ForeverPin.Persistence.DataContexts;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Identity.OAuth.Google.Authenticators;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace ForeverPin.Infrastructure.Identity.CommandHandlers;

/// <summary>Authenticates an account and atomically claims its validated guest's codes.</summary>
public sealed class GoogleSignInCommandHandler(
    IEnumerable<IGoogleIdTokenAuthenticator> authenticators,
    IUserRepository users,
    ICodeRepository codes,
    AppDbContext db)
    : ICommandHandler<GoogleSignInCommand, AppResult<GoogleSignInResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<GoogleSignInResult.Success>> HandleAsync(
        GoogleSignInCommand request, CancellationToken ct)
    {
        var authenticator = authenticators.SingleOrDefault();
        if (authenticator is null)
            return AppResult<GoogleSignInResult.Success>.Fail(AppError.Of(
                AppErrorType.ExternalUnavailable, "Google sign-in is not configured."));
        var identity = await authenticator.AuthenticateAsync(request.IdToken, ct);
        if (identity is null)
            return AppResult<GoogleSignInResult.Success>.Fail(AppError.Of(
                AppErrorType.Unauthorized, "The Google token could not be verified."));

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // Serialize one provider subject before reading it; owner locks use their own acquisition phase.
        if (db.Database.IsNpgsql())
        {
            var key = BinaryPrimitives.ReadInt64LittleEndian(
                SHA256.HashData(Encoding.UTF8.GetBytes($"google-subject:{identity.Subject}")));
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({key})", ct);
        }

        var account = await users.FindByGoogleSubjectAsync(identity.Subject, ct);
        var accountId = account?.Id ?? Guid.NewGuid();
        await db.AcquireOwnerLocksAsync(request.GuestId is { } guest
            ? [guest, accountId] : [accountId], ct);
        // A guest capability can never authorize moving an existing account's data.
        if (request.GuestId is { } claimedGuest && await users.FindByIdAsync(claimedGuest, ct) is not null)
            return AppResult<GoogleSignInResult.Success>.Fail(AppError.Of(
                AppErrorType.Unauthorized, "A registered account cannot be claimed as a guest."));

        if (account is null)
        {
            // Never promote a guest ID: its signed guest capability must not become account authorization.
            account = await users.AddAsync(new UserEntity
            {
                Id = accountId,
                GoogleSubject = identity.Subject,
                Email = identity.Email,
                Name = identity.Name,
                AvatarUrl = identity.Picture,
            }, ct);
        }
        if (request.GuestId is { } guestId)
            await codes.ReassignOwnerAsync(guestId, accountId, ct);
        await transaction.CommitAsync(ct);
        return AppResult<GoogleSignInResult.Success>.Ok(new GoogleSignInResult.Success(
            new UserSummaryModel(account.Id, account.Name, account.Email)));
    }
}
