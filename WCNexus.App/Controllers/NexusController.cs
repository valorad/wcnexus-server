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
    public class NexusController : ControllerBase
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
        public async Task<IActionResult> GetSingle(string dbname, [FromQuery] NexusGetSingleQuery query)
        {
            IDBViewOption options = null;

            if (query.Options is { })
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

            if (nexus is { })
            {
                return Ok(nexus);
            }

            return NotFound(new object());
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] NexusGetListQuery query)
        {
            IDBViewOption options = null;

            FilterDefinition<Nexus> condition;
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

            if (query.Options is { })
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

            return Ok(nexues);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddSingle([FromBody] NexusAddSingleRequest request)
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
        public async Task<IActionResult> AddList([FromBody] NexusAddListRequest request)
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
        public async Task<IActionResult> UpdateSingle(string dbname, [FromBody] NexusUpdateSingleRequest request)
        {

            // validation
            if (request.Token is null)
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"""token"" field cannot be empty."
                }
                );
            }

            // try deserializing the update token
            UpdateDefinition<Nexus> token = request.Token.RootElement.ToString();
            CUDMessage message = await nexusService.Update(dbname, token);

            if (message.OK)
            {

                if (message.NumAffected <= 0)
                {
                    message.OK = false;
                    Response.StatusCode = 404;
                    message.Message = $"Unable to find a matching {dbname} to update.";
                    return new JsonResult(message);
                }

                message.Message = $"Successfuly updated {dbname}.";
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = $"Failed to update {dbname}.";
            Response.StatusCode = 500;
            return new JsonResult(message);

        }

        [HttpPatch]
        public async Task<IActionResult> UpdateList([FromBody] NexusUpdateListRequest request)
        {
            // validation
            if (request.Condition is null)
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"""condition"" field cannot be empty."
                }
                );
            }

            if (request.Token is null)
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"""token"" field cannot be empty."
                }
                );
            }

            if (!request.Condition.RootElement.EnumerateObject().Any())
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"Unconditional updating is not allowed."
                }
                );
            }

            if (!request.Token.RootElement.EnumerateObject().Any())
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $"The update token is missing"
                }
                );
            }

            FilterDefinition<Nexus> condition = request.Condition.RootElement.ToString();
            UpdateDefinition<Nexus> token = request.Token.RootElement.ToString();

            // begin updates
            CUDMessage message = await nexusService.Update(condition, token);

            if (message.OK)
            {

                if (message.NumAffected <= 0)
                {
                    message.OK = false;
                    Response.StatusCode = 404;
                    message.Message = $"Unable to find the matching Nexuses to update.";
                    return new JsonResult(message);
                }

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
                if (message.NumAffected <= 0)
                {
                    message.OK = false;
                    Response.StatusCode = 404;
                    message.Message = $"Unable to find a matching {dbname} to delete.";
                    return new JsonResult(message);
                }

                message.Message = $"Successfuly deleted {dbname}";
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = $"Failed to delete {dbname}.";
            Response.StatusCode = 500;
            return new JsonResult(message);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteList([FromBody] NexusDeleteListRequest request)
        {
            // validation
            if (request.Condition is null)
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"""condition"" field cannot be empty."
                }
                );
            }
            if (!request.Condition.RootElement.EnumerateObject().Any())
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"Unconditional deleting is not allowed."
                }
                );
            }

            FilterDefinition<Nexus> condition = request.Condition.RootElement.ToString();

            // begin delete
            CUDMessage message = await nexusService.Delete(condition);
            if (message.OK)
            {
                if (message.NumAffected <= 0)
                {
                    message.OK = false;
                    Response.StatusCode = 404;
                    message.Message = $"Unable to find the matching Nexuses to delete.";
                    return new JsonResult(message);
                }

                message.Message = $"Successfuly deleted {message.NumAffected} Nexuses";
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = $"Failed to delete the selected Nexuses.";
            Response.StatusCode = 500;
            return new JsonResult(message);
        }

    }


    public class NexusGetSingleQuery
    {
        public string Options { get; set; }
    }

    public class NexusGetListQuery
    {
        public string Condition { get; set; }
        public string Options { get; set; }
    }
    public class NexusAddSingleRequest
    {
        public InputNexus NewNexus { get; set; }
    }
    public class NexusAddListRequest
    {
        public IList<InputNexus> NewNexuses { get; set; }
    }
    public class NexusUpdateSingleRequest
    {
        public JsonDocument Token { get; set; }
    }
    public class NexusUpdateListRequest
    {
        public JsonDocument Condition { get; set; }
        public JsonDocument Token { get; set; }
    }

    public class NexusDeleteListRequest
    {
        public JsonDocument Condition { get; set; }
    }


}