using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("reports")]
    public class ReportsController : ControllerBase
    {
        private readonly Random _random = new();

        [Authorize(Policy = "ProtheticUsersPolicy")]
        [HttpGet]
        public IEnumerable<double> Get()
        {
            return [.. Enumerable.Range(1, 100).Select(_ => _random.NextDouble())];
        }
    }
}
