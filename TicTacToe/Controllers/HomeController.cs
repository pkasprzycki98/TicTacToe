using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TicTacToe.Controllers
{
    public class HomeController : Controller
    {

		// Wzorce MVC,
		// Kompozyt widok który korzysta z PartialView Widzi wiele obiektów za pomocą jednego obiektu
		// Strategia jakiego kolwiek widoku z kontolerem widok pozostawia obsługę reakcji na zdarzenie konkrentej implementacji kontrolera
		// Fasada prawie jest wszędzzie :))
		// Singleton baza danych
		//Scoped i Transient
        public IActionResult Index()
        {
            var culture = Request.HttpContext.Session.GetString("culture");
            ViewBag.Language = culture;
            return View();
        }

        public IActionResult SetCulture(string culture)
        {
            Request.HttpContext.Session.SetString("culture", culture);
            return RedirectToAction("Index");
        }

    }
}