using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CrossSolar.Controllers
{
    [Produces("application/json")]
    [Route("api/Values")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public async Task<IActionResult> GetValues()
        {
            var values = new string[] { "value1", "value2" };
            return Ok(values);
        }
    }
}