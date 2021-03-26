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
        public async Task<IActionResult> GetSingle([FromBody] GetSingleRequest request)
        {
            return BadRequest("This endpoint has not been implemented");
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromBody] GetListRequest request)
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
        
        public class GetSingleRequest 
        {
            public string dbname { get; set; }
        }

        public class GetListRequest 
        {
            public FilterDefinition<Nexus> condition { get; set; }
        }
        public class AddSingleRequest 
        {
            public InputNexus newItem { get; set; }
        }
        public class AddListRequest 
        {
            public IEnumerable<InputNexus> newItems { get; set; }
        }
        public class UpdateSingleRequest 
        {
            public string dbname { get; set; }
            public UpdateDefinition<Nexus> token { get; set; }
        }
        public class UpdateListRequest 
        {
            public FilterDefinition<Nexus> condition { get; set; }
            public UpdateDefinition<Nexus> token { get; set; }
        }
        public class DeleteSingleRequest 
        {
            public string dbname { get; set; }
        }

        public class DeleteListRequest 
        {
            public IEnumerable<string> dbnames { get; set; }
        }

    }



}