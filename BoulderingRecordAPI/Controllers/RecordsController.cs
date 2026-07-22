using Microsoft.AspNetCore.Mvc;

namespace BoulderingRecordAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecordsController : ControllerBase
{
    [HttpPost]
    public IActionResult Upload()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        throw new NotImplementedException();
    }
}
