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
    public class ProjectController : ControllerBase
    {
        private readonly ILogger<ProjectController> logger;
        private readonly IProjectService projectService;

        public ProjectController(
            ILogger<ProjectController> logger,
            IProjectService projectService
        )
        {
            this.logger = logger;
            this.projectService = projectService;
        }

        [HttpGet("dbname/{dbname}")]
        public async Task<IActionResult> GetSingle(string dbname, [FromQuery] ProjectGetSingleQuery query)
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

            JointProject project = await projectService.Get(dbname, options);

            if (project is { })
            {
                return Ok(project);
            }

            return NotFound(new object());
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] ProjectGetListQuery query)
        {

            FilterDefinition<JointProject> condition = null;
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

            List<JointProject> projects = (await projectService.Get(condition, options)).ToList();

            return Ok(projects);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddSingle([FromBody] ProjectAddSingleRequest request)
        {
            // validation
            if (request.NewProject is null)
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"""newProject"" field cannot be empty."
                }
                );
            }

            // begin add
            List<CUDMessage> messages = (await projectService.Add(request.NewProject)).ToList();

            bool isAllOkay = true;
            foreach (var message in messages)
            {
                if (!message.OK)
                {
                    logger.LogError($"{message.Message}");
                    isAllOkay = false;
                }
            }

            if (isAllOkay)
            {

                // long minCount = messages.Aggregate((long) 0, (prev, next) => {
                //     if (next.NumAffected < prev)
                //     {
                //         return next.NumAffected;
                //     }
                //     return prev;
                // });

                long minCount = messages.Min(ele => ele.NumAffected);

                return Ok(new CUDMessage()
                {
                    OK = true,
                    NumAffected = minCount,
                    Message = $"Successfuly added Project: {request.NewProject.Name ?? request.NewProject.DBName}."
                });
            }

            Response.StatusCode = 500;

            return new JsonResult(new CUDMessage()
            {
                OK = false,
                NumAffected = 0,
                Message = $"Failed to add Project: {request.NewProject.Name ?? request.NewProject.DBName}."
            });

        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddList([FromBody] ProjectAddListRequest request)
        {
            // validation
            if (request.NewProjects is null)
            {
                return BadRequest(new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = $@"""newProjects"" field cannot be empty."
                }
                );
            }

            // begin add
            List<CUDMessage> messages = (await projectService.Add(request.NewProjects)).ToList();

            bool isAllOkay = true;
            foreach (var message in messages)
            {
                if (!message.OK)
                {
                    logger.LogError($"{message.Message}");
                    isAllOkay = false;
                }
            }

            if (isAllOkay)
            {
                long minCount = messages.Min(ele => ele.NumAffected);

                return Ok(new CUDMessage()
                {
                    OK = true,
                    NumAffected = minCount,
                    Message = $"Successfuly added {minCount} Projects."
                });

            }

            Response.StatusCode = 500;
            return new JsonResult(new CUDMessage()
            {
                OK = false,
                NumAffected = 0,
                Message = $"Failed to add {request.NewProjects.Count} Projects."
            });

        }

        [HttpPatch("dbname/{dbname}")]
        public async Task<IActionResult> UpdateSingle(string dbname, [FromBody] ProjectUpdateSingleRequest request)
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
            UpdateDefinition<JointProject> token = request.Token.RootElement.ToString();
            List<CUDMessage> messages = (await projectService.Update(dbname, token)).ToList();

            bool isAllOkay = true;
            foreach (var message in messages)
            {
                if (!message.OK)
                {
                    logger.LogError($"{message.Message}");
                    isAllOkay = false;
                }
            }

            if (isAllOkay)
            {
                long maxCount = messages.Max(ele => ele.NumAffected);

                if (maxCount <= 0)
                {
                    Response.StatusCode = 404;
                    return new JsonResult(new CUDMessage()
                    {
                        OK = false,
                        NumAffected = maxCount,
                        Message = $"Unable to find a matching {dbname} to update."
                    });
                }

                return Ok(new CUDMessage()
                {
                    OK = true,
                    NumAffected = maxCount,
                    Message = $"Successfuly updated {dbname}."
                });
            }

            Response.StatusCode = 500;

            return new JsonResult(new CUDMessage()
            {
                OK = false,
                NumAffected = 0,
                Message = $"Failed to update {dbname}."
            });

        }

        [HttpPatch]
        public async Task<IActionResult> UpdateList([FromBody] ProjectUpdateListRequest request)
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

            FilterDefinition<JointProject> condition = request.Condition.RootElement.ToString();
            UpdateDefinition<JointProject> token = request.Token.RootElement.ToString();

            // begin updates
            List<CUDMessage> messages = (await projectService.Update(condition, token)).ToList();

            bool isAllOkay = true;
            foreach (var message in messages)
            {
                if (!message.OK)
                {
                    logger.LogError($"{message.Message}");
                    isAllOkay = false;
                }
            }

            if (isAllOkay)
            {
                long maxCount = messages.Max(ele => ele.NumAffected);

                if (maxCount <= 0)
                {
                    Response.StatusCode = 404;
                    return new JsonResult(new CUDMessage()
                    {
                        OK = false,
                        NumAffected = maxCount,
                        Message = $"Unable to find the matching Projects to update."
                    });
                }

                return Ok(new CUDMessage()
                {
                    OK = true,
                    NumAffected = maxCount,
                    Message = $"Successfuly updated {maxCount} Projects."
                });
            }

            Response.StatusCode = 500;

            return new JsonResult(new CUDMessage()
            {
                OK = false,
                NumAffected = 0,
                Message = $"Failed to update the selected Projects."
            });

        }

        [HttpDelete("dbname/{dbname}")]
        public async Task<IActionResult> DeleteSingle(string dbname)
        {
            List<CUDMessage> messages = (await projectService.Delete(dbname)).ToList();

            bool isAllOkay = true;
            foreach (var message in messages)
            {
                if (!message.OK)
                {
                    logger.LogError($"{message.Message}");
                    isAllOkay = false;
                }
            }

            if (isAllOkay)
            {
                long maxCount = messages.Max(ele => ele.NumAffected);

                if (maxCount <= 0)
                {
                    Response.StatusCode = 404;
                    return new JsonResult(new CUDMessage()
                    {
                        OK = false,
                        NumAffected = maxCount,
                        Message = $"Unable to find a matching {dbname} to delete."
                    });
                }

                return Ok(new CUDMessage()
                {
                    OK = true,
                    NumAffected = maxCount,
                    Message = $"Successfuly deleted {dbname}."
                });
            }

            Response.StatusCode = 500;

            return new JsonResult(new CUDMessage()
            {
                OK = false,
                NumAffected = 0,
                Message = $"Failed to delete {dbname}."
            });

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteList([FromBody] ProjectDeleteListRequest request)
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

            FilterDefinition<JointProject> condition = request.Condition.RootElement.ToString();

            // begin delete
            List<CUDMessage> messages = (await projectService.Delete(condition)).ToList();

            bool isAllOkay = true;
            foreach (var message in messages)
            {
                if (!message.OK)
                {
                    logger.LogError($"{message.Message}");
                    isAllOkay = false;
                }
            }

            if (isAllOkay)
            {
                long maxCount = messages.Max(ele => ele.NumAffected);

                if (maxCount <= 0)
                {
                    Response.StatusCode = 404;
                    return new JsonResult(new CUDMessage()
                    {
                        OK = false,
                        NumAffected = maxCount,
                        Message = $"Unable to find the matching Projects to delete."
                    });
                }

                return Ok(new CUDMessage()
                {
                    OK = true,
                    NumAffected = maxCount,
                    Message = $"Successfuly deleted {maxCount} Projects."
                });
            }

            Response.StatusCode = 500;

            return new JsonResult(new CUDMessage()
            {
                OK = false,
                NumAffected = 0,
                Message = $"Failed to delete the selected Projects."
            });

        }

    }

    public class ProjectGetSingleQuery
    {
        public string Options { get; set; }
    }

    public class ProjectGetListQuery
    {
        public string Condition { get; set; }
        public string Options { get; set; }
    }
    public class ProjectAddSingleRequest
    {
        public InputProject NewProject { get; set; }
    }
    public class ProjectAddListRequest
    {
        public IList<InputProject> NewProjects { get; set; }
    }
    public class ProjectUpdateSingleRequest
    {
        public JsonDocument Token { get; set; }
    }
    public class ProjectUpdateListRequest
    {
        public JsonDocument Condition { get; set; }
        public JsonDocument Token { get; set; }
    }

    public class ProjectDeleteListRequest
    {
        public JsonDocument Condition { get; set; }
    }

}