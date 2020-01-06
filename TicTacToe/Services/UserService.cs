using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Data;
using TicTacToe.Models;

namespace TicTacToe.Services
{
	public class UserService : IUserService
	{
		private static ConcurrentBag<UserModel> _userStore;
		private DbContextOptions<GameDbContext> _dbContextOptions;

		public UserService(DbContextOptions<GameDbContext> dbContextOptions)
		{
			_dbContextOptions = dbContextOptions;
		}

		public async Task<bool> RegisterUser(UserModel userModel)
		{
			using (var db = new GameDbContext(_dbContextOptions))
			{
				db.UserModel.Add(userModel);
				await db.SaveChangesAsync();
				return true;
			}
		}

		public async Task<UserModel> GetUserByEmail(string email)
		{
			using (var db = new GameDbContext(_dbContextOptions))
			{
				return await db.UserModel.FirstOrDefaultAsync(user => user.Email == email);
			}

		}

		public async Task UpdateUser(UserModel userModel)
		{
			using (var db = new GameDbContext(_dbContextOptions))
			{
				db.UserModel.Update(userModel);
				await db.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<UserModel>> GetTopUsers(int numberOfUsers)
		{
			using (var db = new GameDbContext(_dbContextOptions))
			{
				return await db.UserModel.OrderByDescending(x => x.Score).ToListAsync();
			}

		}
		public async Task<bool> IsUserExisting(string email)
		{
			using (var db = new GameDbContext(_dbContextOptions))
			{
				return await db.UserModel.AnyAsync(user => user.Email == email);
			}
		}
    }
}
