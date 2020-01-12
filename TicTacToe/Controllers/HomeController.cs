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
        public IActionResult Index()
        {
            var culture = Request.HttpContext.Session.GetString("culture"); // pobranie z sesji języka
            ViewBag.Language = culture; // kolekcja typu słownik która przechowuje wartośc culture
            return View();
        }

        public IActionResult SetCulture(string culture)
        {
            Request.HttpContext.Session.SetString("culture", culture); // ustawienie języka na "PL-pl" lub "ENG-eng"
            return RedirectToAction("Index");
        }

    }
}