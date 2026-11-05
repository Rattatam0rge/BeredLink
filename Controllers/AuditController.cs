using FeedbackMap.Models;
using FeedbackMap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackMap.Controllers;

/// <summary>Skrivebeskyttet visning av audit-loggen for offentlige aktører.</summary>
[Authorize(Roles = Roles.PublicActor)]
public class AuditController : Controller
{
    private readonly IAuditService _audit;

    public AuditController(IAuditService audit) => _audit = audit;

    /// <summary>Viser de 200 siste hendelsene i aktivitetsloggen.</summary>
    [HttpGet]
    public async Task<IActionResult> Index() => View(await _audit.ListRecentAsync(200));
}
