using Api.Middleware.Telemetry;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api")]
    public class TestController : Controller
    {
        private Telemetry Telemetry { get; } = new Telemetry();

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (Telemetry.TrackAsDependency("Delay", true))
            {
                await Task.Delay(500);
            }

            return Json(Enumerable.Repeat("ABC", 10000));
        }
    }
}