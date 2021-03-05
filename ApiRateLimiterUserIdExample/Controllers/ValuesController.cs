using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIRateLimiterUserId.AspNetCore.AltairCA;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ApiRateLimiterUserIdExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [APIRateLimiterUserIdHttp]
    public class ValuesController : ControllerBase
    {
        private readonly APIRateLimiterUserIdHttpService _apiRateLimiterUserIdHttpService;
        private readonly IMemoryCache _memoryCache;
        public ValuesController(APIRateLimiterUserIdHttpService apiRateLimiterUserIdHttpService, IMemoryCache memoryCache)
        {
            _apiRateLimiterUserIdHttpService = apiRateLimiterUserIdHttpService;
            _memoryCache = memoryCache;
        }

        // GET api/values
        [HttpGet]
        
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
        [APIRateLimiterUserIdHttp(10*60,2,"group1" )]
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        [HttpGet("clearlimit")]
        public void RemoveLimit()
        {
            _apiRateLimiterUserIdHttpService.ClearLimit("group1");
        }
        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            await _apiRateLimiterUserIdHttpService.ClearLimit();
            return Ok();
        }
    }
}
