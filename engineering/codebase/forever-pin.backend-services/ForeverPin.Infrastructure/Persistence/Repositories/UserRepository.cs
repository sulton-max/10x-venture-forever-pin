using Microsoft.EntityFrameworkCore;
using ForeverPin.Application.Identity.Core.Services;
using ForeverPin.Domain.Identity.Entities;
using ForeverPin.Persistence.DataContexts;

namespace ForeverPin.Infrastructure.Persistence.Repositories;

/// <summary>Fetches and persists users via EF Core.</summary>
public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    /// <inheritdoc />
    public Task<UserEntity?> FindByGoogleSubjectAsync(string googleSubject, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.GoogleSubject == googleSubject, ct);

    /// <inheritdoc />
    public Task<UserEntity?> FindByIdAsync(Guid id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    /// <inheritdoc />
    public async Task<UserEntity> AddAsync(UserEntity user, CancellationToken ct)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }
}
