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
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await Task.Delay(500);

            return Json(Enumerable.Repeat("ABC", 10000));
        }
    }
}