﻿using Microsoft.AspNetCore.Mvc;
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

namespace TicTacToe.Controllers
{
	public class GameSessionController : Controller
	{
		private IGameSessionService _gameSessionService;
		public GameSessionController(IGameSessionService gameSessionService)
		{
			_gameSessionService = gameSessionService;
		}

		public async Task<IActionResult> Index(Guid id)
		{
			var session = await _gameSessionService.GetGameSession(id);
			if (session == null)
			{
				var gameInvitationService = Request.HttpContext.RequestServices.GetService<IGameInvitationService>();
				var invitation = await gameInvitationService.Get(id);
				session = await _gameSessionService.CreateGameSession(invitation.Id, invitation.InvitedBy, invitation.EmailTo);
			}
			return View(session);
		}
		[Produces("application/json")]
		[HttpPost("/restapi/v1/SetGamePosition/{sessionId}")]
		public async Task<IActionResult> SetPosition([FromRoute]Guid sessionId)
		{
			if (sessionId != Guid.Empty)
			{
				using (var reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true))
				{
					var bodyString = reader.ReadToEnd();
					if (string.IsNullOrEmpty(bodyString))
						return BadRequest("Treść jest pusta");

					var turn = JsonConvert.DeserializeObject<TurnModel>(bodyString);

					turn.User = await HttpContext.RequestServices.GetService<IUserService>().GetUserByEmail(turn.Email);
					turn.UserId = turn.User.Id;
					if (turn == null)
						return BadRequest("W treści należy przesłać obiekt TurnModel");

					var gameSession = await _gameSessionService.GetGameSession(sessionId);

					if (gameSession == null)
						return BadRequest($"Nie można znaleźć rozgrywki {sessionId}");

					if (gameSession.ActiveUser.Email == turn.User.Email)
						return BadRequest($"{turn.User.Email} nie ma w tej chwili ruchu w grze");

					gameSession = await _gameSessionService.AddTurn(gameSession.Id, turn.User.Email, turn.X, turn.Y);
					if (gameSession != null && gameSession.ActiveUser.Email != turn.User.Email)
						return Ok(gameSession);
					else
						return BadRequest("Nie można zapisać ruchu");
				}
			}
			return BadRequest("Identyfikator Id jest pusty");
		}

	}
}