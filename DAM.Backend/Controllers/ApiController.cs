using Microsoft.AspNetCore.Mvc;

namespace DAM.Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiController : ControllerBase;