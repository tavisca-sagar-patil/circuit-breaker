using Microsoft.AspNetCore.Mvc;

namespace ResiliencyTestApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TestApiController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("API is working");
    }
}