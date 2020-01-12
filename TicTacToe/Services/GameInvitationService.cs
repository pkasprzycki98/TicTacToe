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
    public class GameInvitationService : IGameInvitationService
    {
        private static ConcurrentBag<GameInvitationModel> _gameInvitations;
		private DbContextOptions<GameDbContext> _GameDbContext;
        public GameInvitationService(DbContextOptions<GameDbContext> _dbContextOptions)
        {
				_GameDbContext = _dbContextOptions;
				_gameInvitations = new ConcurrentBag<GameInvitationModel>();
        }

        public Task<GameInvitationModel> Add(GameInvitationModel gameInvitationModel)
        {

			_gameInvitations.Add(gameInvitationModel);
		    using (var inivtation = new GameDbContext(_GameDbContext))
			{
				gameInvitationModel.InvitedByUser = inivtation.UserModels.FirstOrDefault(u => u.Email == gameInvitationModel.EmailTo);
				gameInvitationModel.ConfirmationDate = DateTime.Now;
				inivtation.GameInvitationModels.AddRange(_gameInvitations);
				inivtation.SaveChanges();
			}
			return Task.FromResult(gameInvitationModel);
        }

        public Task Update(GameInvitationModel gameInvitationModel)
        {

			_gameInvitations = new ConcurrentBag<GameInvitationModel>(_gameInvitations.Where(x => x.Id != gameInvitationModel.Id))
			{
				gameInvitationModel
			};
			using (var context = new GameDbContext(_GameDbContext))
			{
				var user = context.GameInvitationModels.FirstOrDefault(u => u.Id == gameInvitationModel.Id);
				user = gameInvitationModel;
				context.SaveChanges();

			}
			return Task.CompletedTask;
        }

        public Task<GameInvitationModel> Get(Guid id)
        {
			using (var context = new GameDbContext(_GameDbContext))
			{
				return Task.FromResult(context.GameInvitationModels.FirstOrDefault(x => x.Id == id));
			}	
        }

        public Task<IEnumerable<GameInvitationModel>> All()
        {
            return Task.FromResult<IEnumerable<GameInvitationModel>>(_gameInvitations.ToList());
        }

        public Task Delete(Guid id)
        {
            _gameInvitations = new ConcurrentBag<GameInvitationModel>(_gameInvitations.Where(x => x.Id != id));

			using (var context = new GameDbContext(_GameDbContext))
			{
				context.Remove(_gameInvitations);
			}
				return Task.CompletedTask;
        }
    }
}
