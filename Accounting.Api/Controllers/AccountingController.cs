using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class AccountingController : Controller
{
    [HttpGet]
    [Authorize(Policy = "payments.read")]
    public IActionResult GetPayments() => Ok(new[] { new { id = 1, amount = 500 } });

    [HttpPost]
    [Authorize(Policy = "payments.write")]
    public IActionResult CreatePayment([FromBody] object payload) => Created(string.Empty, new { ok = true });
}