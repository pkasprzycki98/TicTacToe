using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using TicTacToe.Models;
using Microsoft.EntityFrameworkCore;
using TicTacToe.Data;

namespace TicTacToe.Controllers
{
    public class GameSessionController : Controller
    {
        private IGameSessionService _gameSessionService;
		private DbContextOptions<GameDbContext> _dbContextOptions;
        public GameSessionController(IGameSessionService gameSessionService, DbContextOptions<GameDbContext> dbContextOptions) // wstrzyknięcie zależności 
        {
			_dbContextOptions = dbContextOptions;
            _gameSessionService = gameSessionService;
        }

        public async Task<IActionResult> Index(Guid id)
        {
            var session = await _gameSessionService.GetGameSession(id); // pobranie sesji z bazy danych 
            if (session == null) // jeżeli sesja jest to stwórz nową sesje
            {
                var gameInvitationService = Request.HttpContext.RequestServices.GetService<IGameInvitationService>(); // //Pobiera lub ustawia IServiceProvider, który zapewnia dostęp do kontenera usług żądania.
				var invitation = await gameInvitationService.Get(id); //pobranie zaproszenia z bazy danych zaproszeń
                session = await _gameSessionService.CreateGameSession(invitation.Id, invitation.InvitedBy, invitation.EmailTo); // stworzenia nowej sesji gry i przekazanie do niej dancyh invitation.Id, inivtation.InvitedBy, invitation.EmialTo
            }
            return View(session); // zwrócenie widoku sesji
        }

        [Produces("application/json")] // Filtr, który określa oczekiwany typ, który zwróci akcja i obsługiwane typy treści odpowiedzi. 
		[HttpPost("/restapi/v1/SetGamePosition/{sessionId}")] // Metoda pobiera wartość elementu do wykonania z treści żądania HTTP.
		public async Task<IActionResult> SetPosition([FromRoute]Guid sessionId) //Określa, że ​​parametr lub właściwość należy powiązać przy użyciu danych trasy z bieżącego żądania
		{
            if (sessionId != Guid.Empty)
            {
				using (var reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true)) // zczytanie dancyh przesłanych przez "/restapi/v1/SetGamePosition/{sessionId}"
																									 ///Zapewnia wygodną składnię, która zapewnia poprawne korzystanie z
				{
					var bodyString = reader.ReadToEnd(); // zczytanie wszystkich danych/

					if (string.IsNullOrEmpty(bodyString)) // sprawdzenie czy bodystring nie jest null
						return BadRequest("Treść jest pusta");

					var turn = JsonConvert.DeserializeObject<TurnModel>(bodyString); //Deserializes the JSON to a .NET object.

					turn.User = await HttpContext.RequestServices.GetService<IUserService>().GetUserByEmail(turn.Email); //Gets or sets the IServiceProvider that provides access to the request's service container 
					turn.UserId = turn.User.Id;
				
					if (turn == null) // sprarwdzenie czy turn jest nullem
						return BadRequest("W treści należy przesłać obiekt TurnModel");

					var gameSession = await _gameSessionService.GetGameSession(sessionId); // pobranie gameSession z ConcurentBag'a

					if (gameSession == null) // sprawdzenie czy sesja istnieje
						return BadRequest($"Nie można znaleźć rozgrywki {sessionId}");

					if (gameSession.ActiveUser.Email != turn.User.Email) // zabezpieczenie przez nieporządanym ruchem
						return BadRequest($"{turn.User.Email} nie ma w tej chwili ruchu w grze");

					gameSession = await _gameSessionService.AddTurn(gameSession.Id, turn.User.Email, turn.X, turn.Y); // dodanie tury do concurent bag'a
					if (gameSession != null && gameSession.ActiveUser.Email != turn.User.Email) // jeżeli wszystko jest ok zwróć OK
						return Ok(gameSession);
					else
						return BadRequest("Nie można zapisać ruchu"); // Bad request w przypadku braku zapisania 
				}
            }
            return BadRequest("Identyfikator Id jest pusty"); // badrequest w przypadku złego id
        }

        [Produces("application/json")] //Filtr
        [HttpGet("/restapi/v1/GetGameSession/{sessionId}")] // dane wysyłane do /restapi/v1/GetGameSession/{sessionId}
        public async Task<IActionResult> GetGameSession(Guid sessionId)
        {
            if (sessionId != Guid.Empty) // sprawdzenie czy sessionId nie jest pusty
            {
                var session = await _gameSessionService.GetGameSession(sessionId); // pobranie sesji z concurent bag'a
                if (session != null) // jeżeli zanalazło to zwróc OK
                {
                    return Ok(session);
                }
                else
                {
                    return NotFound($"nie można odnaleźć rozgrywki {sessionId}"); // gdy session == null zwróc NotFound
                }
            }
            else
            {
                return BadRequest("identyfikator rozgrywki jest pusty"); // gdy Id jest pusty
            }
        }

        [Produces("application/json")] // Filtr jakiego typu oczekuje
        [HttpGet("/restapi/v1/CheckGameSessionIsFinished/{sessionId}")] // dane wysyłane do "/restapi/v1/CheckGameSessionIsFinished/{sessionId}"
        public async Task<IActionResult> CheckGameSessionIsFinished(Guid sessionId)
        {
            if (sessionId != Guid.Empty) // sprawdzenie czy sessionId nie jest pusty
            {
                var session = await _gameSessionService.GetGameSession(sessionId); // pobranie sessji z concurent bag'a
                if (session != null) // jeżeli session nie jest null
                {

					if (session.Turns.Count() == 9) // sprawdza czy rozgrywka się zakończyła 
					{
						//using (var context = new GameDbContext(_dbContextOptions))
						//{
						//	context.GameSessionModels.Add(session);
						//	await context.SaveChangesAsync(); // dodanie zakonczonej rozgrywki do bazy danych rozgrywek
						//}
						return Ok("Gra zakończyła się remisem."); // powiadomienie o tym że rozgrywka sie zakonczyła remisem
					}

					//Pobranie wszystkich wykonanych ruchów użytownika
                    var u1serTurns = session.Turns.Where(x => x.User.Email == session.User1.Email).ToList();

					//wywołanie metody CheckIfUserHasWon
                    var user1Won = CheckIfUserHasWon(session.User1.Email, u1serTurns);
					 
                    if (user1Won) // jeżeli wygrał to
                    {
						//using (var context = new GameDbContext(_dbContextOptions))
						//{
						//	context.GameSessionModels.Add(session);
						//	await context.SaveChangesAsync(); // zapisz game SessionModel
						//}
							return Ok($"{session.User1.Email} wygrał grę."); // zwróc OK
                    }
                    else
                    {
						// w przciwynym wypadku
                        var userTurns = session.Turns.Where(x => x.User == session.User2).ToList(); // pobierz liste turn drugiego gracza
                        var user2Won = CheckIfUserHasWon(session.User2.Email, userTurns); // sprawdzenie czy wygrał

						if (user2Won)
						{
							// jezeli tak to operacja podobno do wygrania user1Won
							//using (var context = new GameDbContext(_dbContextOptions))
							//{
							//	context.GameSessionModels.Add(session);
							//	await context.SaveChangesAsync();
							//}
							return Ok($"{session.User2.Email} wygrał grę.");
						}
                          
                        else
                            return Ok("");
                    }
                }
                else
                {
                    return NotFound($"Nie można odnaleźć rozgrywki {sessionId}.");
                }
            }
            else
            {
                return BadRequest("Identyfikator SessionId jest pusty.");
            }
        }

        private bool CheckIfUserHasWon(string email, List<TurnModel> userTurns) // metoda która sprawdza czy jakiś uzytkownik wygrał
        {
            if (userTurns.Any(x => x.X == 0 && x.Y == 0) && userTurns.Any(x => x.X == 1 && x.Y == 0) && userTurns.Any(x => x.X == 2 && x.Y == 0))
                return true;
            else if (userTurns.Any(x => x.X == 0 && x.Y == 1) && userTurns.Any(x => x.X == 1 && x.Y == 1) && userTurns.Any(x => x.X == 2 && x.Y == 1))
                return true;
            else if (userTurns.Any(x => x.X == 0 && x.Y == 2) && userTurns.Any(x => x.X == 1 && x.Y == 2) && userTurns.Any(x => x.X == 2 && x.Y == 2))
                return true;
            else if (userTurns.Any(x => x.X == 0 && x.Y == 0) && userTurns.Any(x => x.X == 0 && x.Y == 1) && userTurns.Any(x => x.X == 0 && x.Y == 2))
                return true;
            else if (userTurns.Any(x => x.X == 1 && x.Y == 0) && userTurns.Any(x => x.X == 1 && x.Y == 1) && userTurns.Any(x => x.X == 1 && x.Y == 2))
                return true;
            else if (userTurns.Any(x => x.X == 2 && x.Y == 0) && userTurns.Any(x => x.X == 2 && x.Y == 1) && userTurns.Any(x => x.X == 2 && x.Y == 2))
                return true;
            else if (userTurns.Any(x => x.X == 0 && x.Y == 0) && userTurns.Any(x => x.X == 1 && x.Y == 1) && userTurns.Any(x => x.X == 2 && x.Y == 2))
                return true;
            else if (userTurns.Any(x => x.X == 2 && x.Y == 0) && userTurns.Any(x => x.X == 1 && x.Y == 1) && userTurns.Any(x => x.X == 0 && x.Y == 2))
                return true;
            else
                return false;
        }
    }
}
