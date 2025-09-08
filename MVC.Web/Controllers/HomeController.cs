using Microsoft.AspNetCore.Mvc;
using MVC.Web.Models;
using System.Diagnostics;

namespace MVC.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Privacy() => View();
}
