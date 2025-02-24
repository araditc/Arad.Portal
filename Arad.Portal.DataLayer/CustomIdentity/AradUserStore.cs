using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Microsoft.AspNetCore.Identity;

using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.CustomIdentity;

public class AradUserStore(IdentityErrorDescriber describer, IUserRepository userRepository)
    : UserStoreBase<ApplicationUser, string, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>>(describer)
{
    public override IQueryable<ApplicationUser> Users { get; } = userRepository.GetAll().AsQueryable();

    private readonly IMongoCollection<ApplicationUser> _applicationUsers = userRepository.DbContext.GetCollection<ApplicationUser>();

    private Task SaveChanges(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        await userRepository.InsertAsync(user, cancellationToken);
        await SaveChanges(cancellationToken);

        return IdentityResult.Success;
    }

    public override async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        string oldStamp = user.ConcurrencyStamp;
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        ReplaceOneResult updateRes = await _applicationUsers.ReplaceOneAsync(x => x.Id.Equals(user.Id) && x.ConcurrencyStamp.Equals(oldStamp), user, cancellationToken: cancellationToken);

        return updateRes.ModifiedCount == 0 ? IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure()) : IdentityResult.Success;
    }

    public override async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        string oldStamp = user.ConcurrencyStamp;
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        Result<ApplicationUser> deleteRes = await userRepository.DeleteAsync(x => x.Id.Equals(user.Id) && x.ConcurrencyStamp.Equals(oldStamp), cancellationToken);

        return !deleteRes.Succeeded ? IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure()) : IdentityResult.Success;
    }

    public override async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        string id = userId;

        return await userRepository.GetByIdAsync(id, cancellationToken);
    }

    public override async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return await userRepository.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    protected override async Task<ApplicationUser> FindUserAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return await userRepository.GetByIdAsync(userId, cancellationToken);
    }

    protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken = new())
    {
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult<IList<Claim>>(new List<Claim>());
    }

    public override Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public override Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public override Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public override Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    protected override Task<IdentityUserToken<string>> FindTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task AddUserTokenAsync(IdentityUserToken<string> token)
    {
        throw new NotImplementedException();
    }

    protected override Task RemoveUserTokenAsync(IdentityUserToken<string> token)
    {
        throw new NotImplementedException();
    }

    public override Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public override Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public override Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public override async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return await userRepository.FirstAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public override Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        if (stamp == null)
        {
            throw new ArgumentNullException(nameof(stamp));
        }

        if (user.SecurityStamp != stamp)
        {
            user.SecurityStamp = stamp;
            userRepository.UpdateAsync(user, e => e.SecurityStamp, user.SecurityStamp, cancellationToken);
        }
        return Task.CompletedTask;
    }

    public override Task<string> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        return Task.FromResult(user.SecurityStamp);
    }

    public override async Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (user.PasswordHash != passwordHash)
        {
            user.PasswordHash = passwordHash;
            await userRepository.UpdateAsync(user, e => e.PasswordHash, user.PasswordHash, cancellationToken);
        }
    }

    /// <summary>
    /// Gets the password hash for a user.
    /// </summary>
    /// <param name="user">The user to retrieve the password hash for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the password hash for the user.</returns>
    public override Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        return Task.FromResult(user.PasswordHash);
    }

    /// <summary>
    /// Returns a flag indicating if the specified user has a password.
    /// </summary>
    /// <param name="user">The user to retrieve the password hash for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a flag indicating if the specified user has a password. If the
    /// user has a password the returned value with be true, otherwise it will be false.</returns>
    public override Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.PasswordHash != null);
    }

}