using Microsoft.AspNetCore.Mvc;
using WCNexus.App.Models;

namespace WCNexus.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class APIController: ControllerBase
    {
        [HttpGet]
        public CommonMessage ReadyState()
        {
            var message = new CommonMessage() {
                OK = true,
                Message = "API works!",
            };

            return message;
        }
    }
}