using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Models;

namespace TicTacToe.Services
{
    public class GameInvitationService : IGameInvitationService
    {
        private static ConcurrentBag<GameInvitationModel> _gameInvitations; // przechowuje obecne zaproszenia do gry
        public GameInvitationService()
        {
            _gameInvitations = new ConcurrentBag<GameInvitationModel>();
        }

        public Task<GameInvitationModel> Add(GameInvitationModel gameInvitationModel) // dodanie do concurent bag'a zaproszenia do gry
        {
            _gameInvitations.Add(gameInvitationModel);
            return Task.FromResult(gameInvitationModel);
        }

        public Task Update(GameInvitationModel gameInvitationModel) // zaktualizowanie instancji GameInivtationmodel
        {
            _gameInvitations = new ConcurrentBag<GameInvitationModel>(_gameInvitations.Where(x => x.Id != gameInvitationModel.Id))
            {
                gameInvitationModel
            };
            return Task.CompletedTask;
        }

        public Task<GameInvitationModel> Get(Guid id) // zwrócenie konkretnego zaproszenia
        {
            return Task.FromResult(_gameInvitations.FirstOrDefault(x => x.Id == id));
        }

        public Task<IEnumerable<GameInvitationModel>> All() // zwrócenie wszystkich zaproszeń
        {
            return Task.FromResult<IEnumerable<GameInvitationModel>>(_gameInvitations.ToList());
        }

        public Task Delete(Guid id) // usunięcie zaproszenia
        {
            _gameInvitations = new ConcurrentBag<GameInvitationModel>(_gameInvitations.Where(x => x.Id != id));
            return Task.CompletedTask;
        }
    }
}
