using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MVC.Web.Controllers;

[Authorize]
public class ApiController(IHttpClientFactory httpClientFactory) : Controller
{
    [HttpGet("/call-payments")]
    public async Task<IActionResult> CallPayments()
    {
        var client = httpClientFactory.CreateClient("PaymentsApi");
        var json = await client.GetStringAsync("/api/payments");
        return Content(json, "application/json");
    }


    [HttpGet("/call-accounting")]
    public async Task<IActionResult> CallAccounting()
    {
        var client = httpClientFactory.CreateClient("AccountingApi");
        var json = await client.GetStringAsync("/api/accounting");
        return Content(json, "application/json");
    }
}
