using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Controllers;

public class AssetsController : ApiController
{
    
    [HttpPost] 
    public IActionResult Get()
    {
        return Ok("Jeg er et brille 👓 af magnus");
    }
    
    [HttpGet] 
    public IActionResult Get2()
    {
        return Ok("Jeg er en get");
    }
    
}


public class RequestParametre
{

    public string navn;
    public string magnus;

}