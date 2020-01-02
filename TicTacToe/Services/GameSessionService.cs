using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Models;
using TicTacToe.Services.Interfaces;

namespace TicTacToe.Services
{
	public class GameSessionService : IGameSessionService
	{
		private IUserService _UserService;
		public GameSessionService(IUserService userService)
		{
			_UserService = userService;
		}
		private static ConcurrentBag<GameSessionModel> _sessions;
		static GameSessionService()
		{
			_sessions = new ConcurrentBag<GameSessionModel>();
		}
		public Task<GameSessionModel> GetGameSession(Guid gameSessionId)
		{
			return Task.Run(() => _sessions.FirstOrDefault(x => x.Id == gameSessionId));
		}
		public async Task<GameSessionModel> CreateGameSession(Guid inivtationId, string invitedByEmail, string invitePlayerEmail)
		{
			var invitedBy = await _UserService.GetUserByEmail(invitedByEmail);
			var invitedPlayer = await _UserService.GetUserByEmail(invitePlayerEmail);
			GameSessionModel session = new GameSessionModel
			{
				User1 = invitedBy,
				User2 = invitedPlayer,
				Id = inivtationId,
				ActiveUser = invitedBy
			};
			_sessions.Add(session);
			return session;
		}
	}
}
