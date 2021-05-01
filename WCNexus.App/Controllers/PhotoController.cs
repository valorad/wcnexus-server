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
    public class PhotoController : ControllerBase
    {
        private readonly ILogger<PhotoController> logger;
        private readonly IPhotoService photoService;
        private readonly IProjectService projectService;

        public PhotoController(
            ILogger<PhotoController> logger,
            IPhotoService photoService,
            IProjectService projectService
        )
        {
            this.logger = logger;
            this.photoService = photoService;
            this.projectService = projectService;
        }

        [HttpGet("dbname/{dbname}")]
        public async Task<IActionResult> GetSingle(string dbname, [FromQuery] PhotoGetSingleQuery query)
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

            Photo photo = await photoService.Get(dbname, options);

            if (photo is { })
            {
                return Ok(photo);
            }

            return NotFound(new object());
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] PhotoGetListQuery query)
        {

            FilterDefinition<Photo> condition = null;
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

            List<Photo> photos = (await photoService.Get(condition, options)).ToList();

            return Ok(photos);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddSingle([FromBody] PhotoAddSingleRequest request)
        {
            // validation
            if (request.NewPhoto is null)
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"""newPhoto"" field cannot be empty."
                }
                );
            }

            // begin add
            CUDMessage message = await photoService.Add(request.NewPhoto);
            if (message.OK)
            {
                if (request.ProjectDBName is { })
                {
                    await projectService.AddImage(request.NewPhoto.DBName, request.ProjectDBName);
                    message.Message = $"Successfuly added Photo { request.NewPhoto.Name ?? request.NewPhoto.DBName } to { request.ProjectDBName }.";

                }
                else
                {
                    message.Message = $"Successfuly added Photo { request.NewPhoto.Name ?? request.NewPhoto.DBName }.";
                }
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = $"Failed to add Photo: {request.NewPhoto.Name ?? request.NewPhoto.DBName}.";
            Response.StatusCode = 500;
            return new JsonResult(message);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddList([FromBody] PhotoAddListRequest request)
        {
            // validation
            if (request.NewPhotos is null)
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"""newPhotos"" field cannot be empty."
                }
                );
            }

            // begin add
            CUDMessage message = await photoService.Add(request.NewPhotos);
            if (message.OK)
            {
                if (request.ProjectDBName is { })
                {
                    List<string> photoDBNames = (
                        from photo in request.NewPhotos
                        select photo.DBName
                    ).ToList();

                    await projectService.AddImage(photoDBNames, request.ProjectDBName);
                    message.Message = $"Successfuly added {message.NumAffected} Photos to {request.ProjectDBName}.";

                }
                else
                {
                    message.Message = $"Successfuly added {message.NumAffected} Photos.";
                }

                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = $"Failed to add {request.NewPhotos.Count} Photos.";
            Response.StatusCode = 500;
            return new JsonResult(message);
        }

        [HttpPatch("dbname/{dbname}")]
        public async Task<IActionResult> UpdateSingle(string dbname, [FromBody] PhotoUpdateSingleRequest request)
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
            UpdateDefinition<Photo> token = request.Token.RootElement.ToString();
            CUDMessage message = await photoService.Update(dbname, token);

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
        public async Task<IActionResult> UpdateList([FromBody] PhotoUpdateListRequest request)
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

            if (request.Condition.RootElement.EnumerateObject().Count() <= 0)
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"Unconditional updating is not allowed."
                }
                );
            }

            if (request.Token.RootElement.EnumerateObject().Count() <= 0)
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $"The update token is missing"
                }
                );
            }

            FilterDefinition<Photo> condition = request.Condition.RootElement.ToString();
            UpdateDefinition<Photo> token = request.Token.RootElement.ToString();

            // begin updates
            CUDMessage message = await photoService.Update(condition, token);

            if (message.OK)
            {

                if (message.NumAffected <= 0)
                {
                    message.OK = false;
                    Response.StatusCode = 404;
                    message.Message = $"Unable to find the matching Photos to update.";
                    return new JsonResult(message);
                }

                message.Message = $"Successfuly updated {message.NumAffected} Photos.";
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = "Failed to update the selected Photos.";
            Response.StatusCode = 500;
            return new JsonResult(message);
        }

        [HttpDelete("dbname/{dbname}")]
        public async Task<IActionResult> DeleteSingle(string dbname)
        {
            CUDMessage message = await photoService.Delete(dbname);
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
        public async Task<IActionResult> DeleteList([FromBody] PhotoDeleteListRequest request)
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
            if (request.Condition.RootElement.EnumerateObject().Count() <= 0)
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"Unconditional deleting is not allowed."
                }
                );
            }

            FilterDefinition<Photo> condition = request.Condition.RootElement.ToString();

            // begin delete
            CUDMessage message = await photoService.Delete(condition);
            if (message.OK)
            {
                if (message.NumAffected <= 0)
                {
                    message.OK = false;
                    Response.StatusCode = 404;
                    message.Message = $"Unable to find the matching Photos to delete.";
                    return new JsonResult(message);
                }

                message.Message = $"Successfuly deleted {message.NumAffected} Photos.";
                return Ok(message);
            }

            logger.LogError($"{message.Message}");
            message.Message = $"Failed to delete the selected Photos.";
            Response.StatusCode = 500;
            return new JsonResult(message);
        }

    }

    public class PhotoGetSingleQuery
    {
        public string Options { get; set; }
    }

    public class PhotoGetListQuery
    {
        public string Condition { get; set; }
        public string Options { get; set; }
    }
    public class PhotoAddSingleRequest
    {
        public InputPhoto NewPhoto { get; set; }
        public string ProjectDBName { get; set; }
    }
    public class PhotoAddListRequest
    {
        public IList<InputPhoto> NewPhotos { get; set; }
        public string ProjectDBName { get; set; }
    }
    public class PhotoUpdateSingleRequest
    {
        public JsonDocument Token { get; set; }
    }
    public class PhotoUpdateListRequest
    {
        public JsonDocument Condition { get; set; }
        public JsonDocument Token { get; set; }
    }

    public class PhotoDeleteListRequest
    {
        public JsonDocument Condition { get; set; }
    }

}