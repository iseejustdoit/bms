using bms.Leaf;
using bms.WebApi.Common;
using bms.WebApi.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace bms.WebApi.Controllers
{
    [ApiController]
    public class LeafController : ControllerBase
    {
        private readonly IIDGen _segmentIdGen;
        private readonly IIDGen _snowflakeIdGen;
        public LeafController(IEnumerable<IIDGen> idGens)
        {
            _segmentIdGen = idGens.FirstOrDefault(p => p.Name == "Segment");
            _snowflakeIdGen = idGens.FirstOrDefault(p => p.Name == "Snowflake");
        }
        [HttpGet("api/segment/get/{key}")]
        public async Task<IActionResult> GetSegmentId(string key)
        {
            return await ParseAsync(async () =>
            {
                return Ok(ResultParser.ParseResult(key, await _segmentIdGen.GetAsync(key)));
            });
        }

        [HttpGet("api/snowflake/get/{key}")]
        public async Task<IActionResult> GetSnowflakeId(string key)
        {
            return await ParseAsync(async () =>
            {
                return Ok(ResultParser.ParseResult(key, await _snowflakeIdGen.GetAsync(key)));
            });
        }

        private async Task<IActionResult> ParseAsync(Func<Task<IActionResult>> func)
        {
            try
            {
                return await func.Invoke();
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
