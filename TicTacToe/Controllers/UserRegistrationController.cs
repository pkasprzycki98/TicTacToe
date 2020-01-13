using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using TicTacToe.Models;
using TicTacToe.Services;
using Microsoft.Extensions.DependencyInjection;
using static TicTacToe.Models.UserModel;

namespace TicTacToe.Controllers
{
    public class UserRegistrationController : Controller
    {
        readonly IUserService _userService;
        readonly IEmailService _emailService;
        readonly ILogger<UserRegistrationController> _logger;
        public UserRegistrationController(IUserService userService, IEmailService emailService, ILogger<UserRegistrationController> logger) // wstrzykiwane zależności
        {
            _userService = userService;
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult Index() // zwrócenie widoku
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserModel userModel) // zapytanie Post, które pobiera userModel, sprawdza czy jest poprawny jeśli tak to zarejstruj w przciwnym wypadku zwróć widok
        {
            if (ModelState.IsValid)
            {
                await _userService.RegisterUser(userModel);
                return RedirectToAction(nameof(EmailConfirmation), new { userModel.Email });
            }
            else
            {
                return View(userModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EmailConfirmation(string email) // Metoda która wysyła potwierdzenie email
        {
            _logger.LogInformation($"##Start## Proces potwierdzenia adresu {email}"); // zapisanie w log'a
            var user = await _userService.GetUserByEmail(email); // pobranie user'a na podstawie podane emial'a
            var urlAction = new UrlActionContext // swtorzenie nowego urlAction
            {
                Action = "ConfirmEmail", //Akcja która zostanie wykonana
                Controller = "UserRegistration", // Controller
                Values = new { email }, // wartość 
                Protocol = Request.Scheme,  // pobiera obecny adres arl
                Host = Request.Host.ToString() // ustwienie host'a
            };

            var userRegistrationEmail = new UserRegistrationEmailModel // swtorzene UserRegistrarionEmailModel
            {
                DisplayName = $"{user.FirstName} {user.LastName}", 
                Email = email,
                ActionUrl = Url.Action(urlAction)
            };

            var emailRenderService = HttpContext.RequestServices.GetService<IEmailTemplateRenderService>(); // //Gets or sets the IServiceProvider that provides access to the request's service container  
			var message = await emailRenderService.RenderTemplate("EmailTemplates/UserRegistrationEmail", userRegistrationEmail, Request.Host.ToString()); // wygenerowanie emial template który tworzy się na podstawie EmialTemplate i zapisuje w www/root/emails
            try
            {
                _emailService.SendEmail(email, "Potwierdzenie adresu e-mail w grze Kółko i krzyżyk", message).Wait(); // podejmuje próbuje wysłanie mail'a na wskazany Email
            }
            catch (Exception e)
            {
            }

            if (user?.IsEmailConfirmed == true)
                return RedirectToAction("Index", "GameInvitation", new { email = email }); // jezeli user był juz zarejstrowany i ma potwierdzony e mial przekieruj do Index GameInivtatin 

            ViewBag.Email = email;

            return View();
        }

        public async Task<IActionResult> ConfirmEmail(string email) // metoda potwierdzająca emial
        {
            var user = await _userService.GetUserByEmail(email); // pobrnie user'a
            if (user != null)
            {
                user.IsEmailConfirmed = true; // potwierdzenie mail'a
                user.EmailConfirmationDate = DateTime.Now; // ustalenie daty
                await _userService.UpdateUser(user); // zaktualizowanie 
                return RedirectToAction("Index", "Home"); // przekierowanie 
            }
            return BadRequest();
        }
    }
}