using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityMvc.Controllers;

public class OrderController:Controller
{
    [Authorize(Policy = "OrderPermissionForReadOrDelete")]
    public IActionResult Index()
    {
        return View();
    }
}