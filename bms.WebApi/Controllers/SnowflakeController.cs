using bms.Leaf;
using bms.WebApi.Common;
using bms.WebApi.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace bms.WebApi.Controllers
{
    [ApiController]
    public class SnowflakeController : ControllerBase
    {
        private readonly IIDGen _idGen;
        public SnowflakeController(IEnumerable<IIDGen> idGens)
        {
            _idGen = idGens.FirstOrDefault(p => p.Name == "Snowflake");
        }
        [HttpGet("api/snowflake/get/{key}")]
        public async Task<IActionResult> GetSnowflakeId(string key)
        {
            try
            {
                return Ok(ResultParser.ParseResult(key, await _idGen.GetAsync(key)));
            }
            catch (NoKeyException)
            {
                return BadRequest("Key is none");
            }
            catch (LeafServerException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
