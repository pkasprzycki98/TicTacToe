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

        public  UserService(DbContextOptions<GameDbContext> dbContextOptions)
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

        public Task<UserModel> GetUserByEmail(string email)
        {
            return Task.FromResult(_userStore.FirstOrDefault(u => u.Email == email));
        }

        public Task UpdateUser(UserModel userModel)
        {
            _userStore = new ConcurrentBag<UserModel>(_userStore.Where(u => u.Email != userModel.Email))
                {
                    userModel
                };
            return Task.CompletedTask;
        }

        public Task<IEnumerable<UserModel>> GetTopUsers(int numberOfUsers)
        {
            return Task.Run(() => (IEnumerable<UserModel>)_userStore.OrderBy(x => x.Score).Take(numberOfUsers).ToList());
        }
    }
}
