using Microsoft.AspNetCore.Mvc;

namespace PSKVideoProjectBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {

        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "Test")]
        public string Get()
        {
            return "hey";
        }
    }
}