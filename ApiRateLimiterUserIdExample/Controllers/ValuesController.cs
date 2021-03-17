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
        private readonly IAPIRateLimiterUserIdHttpService _iapiRateLimiterUserIdHttpService;
        private readonly IMemoryCache _memoryCache;
        public ValuesController(IAPIRateLimiterUserIdHttpService iapiRateLimiterUserIdHttpService, IMemoryCache memoryCache)
        {
            _iapiRateLimiterUserIdHttpService = iapiRateLimiterUserIdHttpService;
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
            _iapiRateLimiterUserIdHttpService.ClearLimitGroup("group1");
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
            await _iapiRateLimiterUserIdHttpService.ClearLimit();
            return Ok();
        }

        [HttpGet("setlimit/{limit}")]
        public async Task<IActionResult> SetLimit(int limit)
        {
            await _iapiRateLimiterUserIdHttpService.SetLimitGroup("0587c92a-67c4-4c2d-86e9-6d4651eac871", "group1", limit);
            return Ok();
        }

        [HttpGet("getreaminiglimit")]
        public async Task<IActionResult> GetRemainingLimit()
        {
            long remainig = await _iapiRateLimiterUserIdHttpService.GetRemainingLimitGroup("0587c92a-67c4-4c2d-86e9-6d4651eac871", "group1");
            return Ok(remainig);
        }

        [HttpGet("GetCurrentCountGroup")]
        public async Task<IActionResult> GetCurrentCountGroup()
        {
            long count = await _iapiRateLimiterUserIdHttpService.GetCurrentCountGroup("0587c92a-67c4-4c2d-86e9-6d4651eac871", "group1");
            return Ok(count);
        }
    }
}
