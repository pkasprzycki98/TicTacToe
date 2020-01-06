using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Data;
using TicTacToe.Models;

namespace TicTacToe.Managers
{
	 class ApplicationUserManager : UserManager<UserModel>
	{
		private IUserStore<UserModel> _store;
		DbContextOptions<GameDbContext> _dbContextOptions;
		public ApplicationUserManager(DbContextOptions<GameDbContext> dbContextOptions,
			IUserStore<UserModel> store, IOptions<IdentityOptions> optionsAccessor,
			IPasswordHasher<UserModel> passwordHasher,
			IEnumerable<IUserValidator<UserModel>> userValidators, IEnumerable<IPasswordValidator<UserModel>> passwordValidators,
			ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
			IServiceProvider services, ILogger<UserManager<UserModel>> logger) :
			base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
		{
			_store = store;
			_dbContextOptions = dbContextOptions;
		}
		public override async Task<TicTacToe.Models.UserModel> FindByEmailAsync(string email)
		{
			using (var db = new GameDbContext(_dbContextOptions))
			{
				return await db.Set<UserModel>().FirstOrDefaultAsync(x => x.Email == email);
			}
		}
		public override async Task<UserModel> FindByIdAsync(string userId)
		{
			using (var db = new GameDbContext(_dbContextOptions))
			{
				Guid id = Guid.Parse(userId);
				return await db.Set<UserModel>().FirstOrDefaultAsync(x => x.Id == id);
			}
		}
		public override async Task<IdentityResult> UpdateAsync(UserModel user)
		{
			using (var db = new GameDbContext(_dbContextOptions))
			{
				var current = await db.Set<UserModel>().FirstOrDefaultAsync(x => x.Id == user.Id);
				{
					current.AccessFailedCount = user.AccessFailedCount;
					current.ConcurrencyStamp = user.ConcurrencyStamp;
					current.Email = current.Email;
					current.EmailConfirmationDate = user.EmailConfirmationDate;
					current.EmailConfirmed = user.EmailConfirmed;
					current.FirstName = user.FirstName;
					current.LastName = user.LastName;
					current.NormalizedEmail = user.NormalizedEmail;
					current.NormalizedUserName = user.NormalizedUserName;
					current.PhoneNumber = user.PhoneNumber;
					current.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
					current.Score = user.Score;
					current.SecurityStamp = user.SecurityStamp;
					current.TwoFactorEnabled = user.TwoFactorEnabled;
					current.UserName = user.UserName;

					await db.SaveChangesAsync();
				}
				return IdentityResult.Success;
			}
		}


	}
}
