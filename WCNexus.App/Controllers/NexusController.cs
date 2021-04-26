using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using WCNexus.App.Library;
using WCNexus.App.Models;
using WCNexus.App.Services;

namespace WCNexus.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NexusController: ControllerBase
    {
        private readonly ILogger<NexusController> logger;
        private readonly INexusService nexusService;

        public NexusController(
            ILogger<NexusController> logger,
            INexusService nexusService
        )
        {
            this.logger = logger;
            this.nexusService = nexusService;
        }

        [HttpGet("dbname/{dbname}")]
        public async Task<IActionResult> GetSingle(string dbname, [FromQuery] GetSingleQuery query)
        {
            IDBViewOption options = null;

            if (query.Options is {})
            {
                // try deserializing DB View Options
                try
                {
                    options = JsonSerializer.Deserialize<DBViewOption>(
                        query.Options,
                        new JsonSerializerOptions()
                        {
                            PropertyNameCaseInsensitive = true,
                        }
                    );
                }
                catch (Exception)
                {
                    return BadRequest(new CommonMessage()
                        {
                            OK = false,
                            Message = $@"""options"" query parameter is invalid."
                        }
                    );
                }
            }

            Nexus nexus = await nexusService.Get(dbname, options);

            if (nexus is {})
            {
                return Ok(nexus);
            }

            return NotFound(new object());
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] GetListQuery query)
        {

            FilterDefinition<Nexus> condition = null;
            IDBViewOption options = null;

            if (query.Condition is null)
            {
                condition = "{}";
            }
            else
            {
                // try deserializing condition
                try
                {
                    condition = JsonUtil.CreateLiteral(query.Condition);
                }
                catch (Exception)
                {
                    return BadRequest(new CommonMessage()
                        {
                            OK = false,
                            Message = $@"""condition"" query parameter is invalid."
                        }
                    );
                }
            }

            if (query.Options is {})
            {
                // try deserializing DB View Options
                try
                {
                    options = JsonSerializer.Deserialize<DBViewOption>(
                        query.Options,
                        new JsonSerializerOptions()
                        {
                            PropertyNameCaseInsensitive = true,
                        }
                    );
                }
                catch (Exception)
                {
                    return BadRequest(new CommonMessage()
                        {
                            OK = false,
                            Message = $@"""options"" query parameter is invalid."
                        }
                    );
                }
            }

            List<Nexus> nexues = (await nexusService.Get(condition, options)).ToList();

            if (nexues.Count > 0)
            {
                return Ok(nexues);
            }

            return NotFound(nexues);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddSingle([FromBody] AddSingleRequest request)
        {
            // validation
            if (request.NewNexus is null)
            {
                return BadRequest(new CUDMessage()
                    {
                        OK = false,
                        NumAffected = 0,
                        Message = $@"""newNexus"" field cannot be empty."
                    }
                );
            }

            // begin add
            CUDMessage message = await nexusService.Add(request.NewNexus);
            if (message.OK)
            {
                message.Message = $"Successfuly added Nexus: {request.NewNexus.Name ?? request.NewNexus.DBName}.";
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = $"Failed to add Nexus: {request.NewNexus.Name ?? request.NewNexus.DBName}.";
            Response.StatusCode = 500;
            return new JsonResult(message);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddList([FromBody] AddListRequest request)
        {
            // validation
            if (request.NewNexuses is null)
            {
                return BadRequest(new CUDMessage()
                    {
                        OK = false,
                        NumAffected = 0,
                        Message = $@"""newNexuses"" field cannot be empty."
                    }
                );
            }

            // begin add
            CUDMessage message = await nexusService.Add(request.NewNexuses);
            if (message.OK)
            {
                message.Message = $"Successfuly added {message.NumAffected} Nexuses.";
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = $"Failed to add {request.NewNexuses.Count} Nexuses.";
            Response.StatusCode = 500;
            return new JsonResult(message);
        }

        [HttpPatch("dbname/{dbname}")]
        public async Task<IActionResult> UpdateSingle(string dbname, [FromBody] UpdateSingleRequest request)
        {
            CUDMessage message = await nexusService.Update(dbname, request.Token);
            if (message.OK && message.NumAffected > 0)
            {
                message.Message = $"Successfuly updated {dbname}.";
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = $"Failed to update {dbname}.";
            Response.StatusCode = 500;
            return new JsonResult(message);

        }

        [HttpPatch]
        public async Task<IActionResult> UpdateList([FromBody] UpdateListRequest request)
        {
            // validation
            if (request.Condition.RenderToBsonDocument().Elements.Count() <= 0)
            {
                return BadRequest($"Unconditional updating is not allowed.");
            }

            if (request.Token.RenderToBsonDocument().Elements.Count() <= 0)
            {
                return BadRequest($"The update token is missing");
            }

            // begin updates
            CUDMessage message = await nexusService.Update(request.Condition, request.Token);

            if (message.OK && message.NumAffected > 0)
            {
                message.Message = $"Successfuly updated {message.NumAffected} Nexuses";
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = "Failed to update the selected Nexuses.";
            Response.StatusCode = 500;
            return new JsonResult(message);
        }

        [HttpDelete("dbname/{dbname}")]
        public async Task<IActionResult> DeleteSingle(string dbname)
        {
            CUDMessage message = await nexusService.Delete(dbname);
            if (message.OK)
            {
                message.Message = $"Successfuly deleted {dbname}";
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = $"Failed to delete {dbname}.";
            Response.StatusCode = 500;
            return new JsonResult(message);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteList([FromBody] DeleteListRequest request)
        {
            // validation
            if (request.Condition.RenderToBsonDocument().Elements.Count() <= 0)
            {
                return BadRequest($"Unconditional deleting is not allowed.");
            }

            // begin delete
            CUDMessage message = await nexusService.Delete(request.Condition);
            if (message.OK)
            {
                message.Message = $"Successfuly deleted {message.NumAffected} Nexuses";
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = $"Failed to delete the selected Nexuses.";
            Response.StatusCode = 500;
            return new JsonResult(message);
        }
        
        public class GetSingleQuery
        {
            public string Options { get; set; }
        }

        public class GetListQuery 
        {
            public string Condition { get; set; }
            public string Options { get; set; }
        }
        public class AddSingleRequest 
        {
            public InputNexus NewNexus { get; set; }
        }
        public class AddListRequest 
        {
            public IList<InputNexus> NewNexuses { get; set; }
        }
        public class UpdateSingleRequest 
        {
            public UpdateDefinition<Nexus> Token { get; set; }
        }
        public class UpdateListRequest 
        {
            public FilterDefinition<Nexus> Condition { get; set; }
            public UpdateDefinition<Nexus> Token { get; set; }
        }

        public class DeleteListRequest 
        {
            public FilterDefinition<Nexus> Condition { get; set; }
        }

    }



}