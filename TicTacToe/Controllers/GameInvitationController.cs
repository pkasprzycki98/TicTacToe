using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Models;
using TicTacToe.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TicTacToe.Controllers
{
    public class GameInvitationController : Controller
    {
        private IStringLocalizer<GameInvitationController> _stringLocalizer;
        private IUserService _userService;
        public GameInvitationController(IUserService userService, IStringLocalizer<GameInvitationController> stringLocalizer) // Wstrzyknięcie zależności
        {
            _userService = userService;
            _stringLocalizer = stringLocalizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string email)
        {
            var gameInvitationModel = new GameInvitationModel { InvitedBy = email, Id = Guid.NewGuid() }; // Stworzenie nowego modelu zaproszenia do gry
            Request.HttpContext.Session.SetString("email", email); // Przechowywanie w sesji email'a
            var user = await _userService.GetUserByEmail(email); // pobranie user'a na podstawie email
            Request.HttpContext.Session.SetString("displayName", $"{user.FirstName} {user.LastName}"); // przechowywanie w sesji displayName
			return View(gameInvitationModel); // zwrócenie widoku gameInvitationModel
        }

        [HttpPost]
        public async Task<IActionResult> Index(GameInvitationModel gameInvitationModel, [FromServices]IEmailService emailService) //[fromservices] Umożliwia wstawianie usługi bezpośrednio do metody akcji bez przy użyciu iniekcji konstruktora:
		{
            var gameInvitationService = Request.HttpContext.RequestServices.GetService<IGameInvitationService>(); //Pobiera lub ustawia IServiceProvider, który zapewnia dostęp do kontenera usług żądania.
			if (ModelState.IsValid) 
            {
                try
                {
                    var invitationModel = new InvitationEmailModel // Stworzenia InivatationEmialModel
                    {
                        DisplayName = $"{gameInvitationModel.EmailTo}",
                        InvitedBy = await _userService.GetUserByEmail(gameInvitationModel.InvitedBy),
                        ConfirmationUrl = Url.Action("ConfirmGameInvitation", "GameInvitation", // Url.Action Generuje ciąg do pełnego adresu URL do metody akcji.
							new { id = gameInvitationModel.Id }, Request.Scheme, Request.Host.ToString()),
                        InvitedDate = gameInvitationModel.ConfirmationDate
                    };

                    var emailRenderService = HttpContext.RequestServices.GetService<IEmailTemplateRenderService>(); // //Pobiera lub ustawia IServiceProvider, który zapewnia dostęp do kontenera usług żądania.
					var message = await emailRenderService.RenderTemplate<InvitationEmailModel>("EmailTemplates/InvitationEmail", invitationModel, Request.Host.ToString());
					//message = wygenerowana wiadomość na podstawie EmailTemplates/InvitationEmail
					//Request.Host()Pobiera lub ustawia wartość nagłówka hosta, która będzie używana w żądaniu HTTP niezależnym od identyfikatora URI żądania.
					await emailService.SendEmail(gameInvitationModel.EmailTo, _stringLocalizer["Zaproszenie do gry w kółko i krzyżyk"], message); // Wysyła email z daną wiadomością 
                }
                catch
                {

                }

                var invitation = gameInvitationService.Add(gameInvitationModel).Result; // dodanie zaproszenia do gry do bazy danych
                return RedirectToAction("GameInvitationConfirmation", new { id = gameInvitationModel.Id }); // przekierowanie do akcji GameInivtationConfirmation z gameinvitationModel.Id
            }
            return View(gameInvitationModel); // w przypadku gdy model nie jest valid zwrócenie akutalnego widoku
        }

        [HttpGet]
        public IActionResult GameInvitationConfirmation(Guid id, [FromServices]IGameInvitationService gameInvitationService)
        {
            var gameInvitation = gameInvitationService.Get(id).Result; // pobranie z bazy danych gameInivtation
            return View(gameInvitation); // Wyświetlenie widoku dla gameInivtation
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmGameInvitation(Guid id, [FromServices]IGameInvitationService gameInvitationService)
        {
            var gameInvitation = await gameInvitationService.Get(id); // pobranie z bazy dancyh gameInivtation
            gameInvitation.IsConfirmed = true; // Potiwerdzenie
            gameInvitation.ConfirmationDate = DateTime.Now; // ustalenie obecnej daty
			await gameInvitationService.Update(gameInvitation); // zaktualizowanie gameInvitation
            Request.HttpContext.Session.SetString("email", gameInvitation.EmailTo); // zapisanie w sesji gameInvitation.EmailTo 
			
            return RedirectToAction("Index", "GameSession", new { id }); // przekierowanie do Akcji Index Controllera "GameSession" z id sessji
        }
    }
}
