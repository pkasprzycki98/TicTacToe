using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Models;

namespace TicTacToe.Services.Interfaces
{
	public interface IGameSessionService
	{
		Task<GameSessionModel> GetGameSession(Guid gameSessionId);
		Task<GameSessionModel> CreateGameSession(Guid inivtationId, string invitedByEmail, string invitePlayerEmail);
	}
}
