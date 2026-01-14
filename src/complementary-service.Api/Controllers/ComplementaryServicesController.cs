using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComplementaryServices.Application.DTOs;
using ComplementaryServices.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ComplementaryServices.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplementaryServicesController : ControllerBase
    {
        private readonly IComplementaryServiceAppService _service;

        public ComplementaryServicesController(IComplementaryServiceAppService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> RequestService([FromBody] ServiceRequestDto request, [FromHeader(Name = "X-User-Id")] Guid userId, [FromHeader(Name = "X-Event-Id")] Guid eventId)
        {
            var id = await _service.RequestServiceAsync(request, userId, eventId);
            return Ok(id);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ServiceStatusDto>>> GetUserServices(Guid userId, [FromQuery] Guid? reservationId)
        {
            var services = await _service.GetUserServicesAsync(userId, reservationId);
            return Ok(services);
        }

        [HttpPost("{serviceId}/confirm")]
        public async Task<ActionResult> ConfirmService(Guid serviceId, [FromBody] ConfirmServiceRequest request)
        {
            await _service.ConfirmServiceAsync(serviceId, request.ProviderId, request.Price, request.Message, request.EstimatedTime);
            return NoContent();
        }

        [HttpPost("{serviceId}/reject")]
        public async Task<ActionResult> RejectService(Guid serviceId, [FromBody] RejectServiceRequest request)
        {
            await _service.RejectServiceAsync(serviceId, request.Reason);
            return NoContent();
        }
    }

    public class ConfirmServiceRequest
    {
        public string ProviderId { get; set; }
        public decimal Price { get; set; }
        public string Message { get; set; }
        public DateTime? EstimatedTime { get; set; }
    }

    public class RejectServiceRequest
    {
        public string Reason { get; set; }
    }
}
