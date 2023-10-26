using API.JsonResults;
using API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetInfo")]
        public IActionResult Get([FromQuery] OrderGetInfoDto orderGetInfoDto)
        {
            OrderService orderService = new OrderService();

            try
            {
                OrderGetInfoResult result = orderService.GetInfo(orderGetInfoDto);

                return Ok(result);
            }
            catch(Exception ex)
            { 
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}