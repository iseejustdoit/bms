﻿using bms.Leaf;
using bms.Leaf.Common;
using bms.WebApi.Exceptions;
using bms.WebApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace bms.WebApi.Controllers
{
    [ApiController]
    public class SegmentController : ControllerBase
    {
        private readonly IDGen _idGen;
        public SegmentController(IDGenFactory idGenFactory)
        {
            _idGen = idGenFactory.Get("Segment");
        }
        [HttpGet("api/segment/get/{key}")]
        public async Task<IActionResult> GetSegmentId(string key)
        {
            try
            {
                return Ok(Get(key, await _idGen.GetAsync(key)));
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

        private string Get(string key, Result id)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new NoKeyException();
            }
            if (id.Status == Status.EXCEPTION)
            {
                throw new LeafServerException(id.ToString());
            }
            return id.Id.ToString();
        }
    }
}