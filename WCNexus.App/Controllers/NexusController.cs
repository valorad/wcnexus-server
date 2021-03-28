using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WCNexus.App.Models;

namespace WCNexus.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NexusController: ControllerBase
    {
        [HttpGet("{dbname}")]
        public async Task<IActionResult> GetSingle([FromQuery] GetSingleQuery query)
        {
            return BadRequest("This endpoint has not been implemented");
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] GetListQuery query)
        {
            return BadRequest("This endpoint has not been implemented");
        }

        [HttpPost("{dbname}")]
        public async Task<IActionResult> AddSingle([FromBody] AddSingleRequest request)
        {
            return BadRequest("This endpoint has not been implemented");
        }

        [HttpPost]
        public async Task<IActionResult> AddList([FromBody] AddListRequest request)
        {
            return BadRequest("This endpoint has not been implemented");
        }

        [HttpPatch("{dbname}")]
        public async Task<IActionResult> UpdateSingle([FromBody] UpdateSingleRequest request)
        {
            return BadRequest("This endpoint has not been implemented");
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateList([FromBody] UpdateListRequest request)
        {
            return BadRequest("This endpoint has not been implemented");
        }

        [HttpDelete("{dbname}")]
        public async Task<IActionResult> DeleteSingle([FromBody] DeleteSingleRequest request)
        {
            return BadRequest("This endpoint has not been implemented");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteList([FromBody] DeleteListRequest request)
        {
            return BadRequest("This endpoint has not been implemented");
        }
        
        public class GetSingleQuery
        {
            public IDBViewOption Options { get; set; }
        }

        public class GetListQuery 
        {
            public FilterDefinition<Nexus> Condition { get; set; }
            public IDBViewOption Options { get; set; }
        }
        public class AddSingleRequest 
        {
            public InputNexus NewNexus { get; set; }
        }
        public class AddListRequest 
        {
            public IEnumerable<InputNexus> NewNexuses { get; set; }
        }
        public class UpdateSingleRequest 
        {
            public string DBName { get; set; }
            public UpdateDefinition<Nexus> Token { get; set; }
        }
        public class UpdateListRequest 
        {
            public FilterDefinition<Nexus> Condition { get; set; }
            public UpdateDefinition<Nexus> Token { get; set; }
        }
        public class DeleteSingleRequest 
        {
            public string DBName { get; set; }
        }

        public class DeleteListRequest 
        {
            public FilterDefinition<Nexus> Condition { get; set; }
        }

    }



}