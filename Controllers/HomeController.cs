using System.Diagnostics;
using FeedbackMap.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackMap.Controllers;

/// <summary>Startside (kart for innloggede, ellers presentasjonsside), personvern og feilside.</summary>
public class HomeController : Controller
{
    /// <summary>Kart for innloggede brukere, ellers presentasjonsside.</summary>
    public IActionResult Index() => View();

    /// <summary>Personvernside.</summary>
    public IActionResult Privacy() => View();

    /// <summary>Feilside.</summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
