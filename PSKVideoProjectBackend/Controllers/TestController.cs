using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Properties;
using PSKVideoProjectBackend.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PSKVideoProjectBackend.Hubs;
using PSKVideoProjectBackend.Managers;

namespace PSKVideoProjectBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly SignalRManager _signalRManager;

        public TestController(ILogger<TestController> logger, SignalRManager signalRManager)
        {
            _logger = logger;
            _signalRManager = signalRManager;
        }

        /// <summary>
        /// Sends a message to all connected clients
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("Test")]
        public async Task<ActionResult> Test(string message)
        {
            try
            {
                var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<NotificationHub>>();

                if (String.IsNullOrEmpty(message)) return StatusCode(StatusCodes.Status400BadRequest);

                await hubContext.Clients.All.SendAsync("ReceiveNotification", message);

                return Ok();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }

        /// <summary>
        /// Sends a message only to the client that called the endpoint
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("TestIndividual")]
        public async Task<ActionResult> TestIndividual(string message)
        {
            try
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated) return StatusCode(StatusCodes.Status401Unauthorized);

                if (String.IsNullOrEmpty(message)) return StatusCode(StatusCodes.Status400BadRequest);

                var userIdStr = User.FindFirst(ClaimTypes.Name)!.Value;

                await _signalRManager.SendMessageToUser(userIdStr, message);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, Resources.ErrInsertToDB);
            }
        }
    }
}