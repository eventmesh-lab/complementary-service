using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ComplementaryServices.Application.DTOs;
using ComplementaryServices.Application.Services;
using System.Security.Claims;

namespace ComplementaryServices.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize] // Asegurar que solo usuarios autenticados accedan
    public class ComplementaryServicesController : ControllerBase
    {
        private readonly IComplementaryServiceAppService _appService;
        private readonly ILogger<ComplementaryServicesController> _logger;

        public ComplementaryServicesController(
            IComplementaryServiceAppService appService,
            ILogger<ComplementaryServicesController> logger)
        {
            _appService = appService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                // Para testing local si no hay JWT, permitir un header
                var headerId = Request.Headers["X-User-Id"].ToString();
                if (!string.IsNullOrEmpty(headerId)) return Guid.Parse(headerId);
                
                throw new UnauthorizedAccessException("User ID claim not found.");
            }
            return Guid.Parse(userIdClaim);
        }

        /// <summary>
        /// Solicita un nuevo servicio complementario (Transporte, Catering, etc.)
        /// </summary>
        [HttpPost("request")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestService([FromBody] ServiceRequestDto request)
        {
            var userId = GetUserId();
            var serviceId = await _appService.RequestServiceAsync(request, userId);
            
            _logger.LogInformation("Service requested: {ServiceId} for Reservation {ReservationId}", 
                serviceId, request.ReservationId);

            return AcceptedAtAction(nameof(GetServiceStatus), new { serviceId = serviceId }, new { ServiceId = serviceId });
        }

        /// <summary>
        /// Obtiene el estado de un servicio específico del usuario
        /// </summary>
        [HttpGet("{serviceId}")]
        [ProducesResponseType(typeof(ServiceStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetServiceStatus(Guid serviceId)
        {
            var userId = GetUserId();
            try
            {
                var status = await _appService.GetServiceByIdAsync(serviceId, userId);
                return Ok(status);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Lista todos los servicios del usuario actual
        /// </summary>
        [HttpGet("my-services")]
        public async Task<ActionResult<List<ServiceStatusDto>>> GetMyServices([FromQuery] Guid? reservationId)
        {
            var userId = GetUserId();
            var services = await _appService.GetUserServicesAsync(userId, reservationId);
            return Ok(services);
        }

        /// <summary>
        /// Cancela un servicio pendiente
        /// </summary>
        [HttpPost("{serviceId}/cancel")]
        public async Task<IActionResult> CancelService(Guid serviceId)
        {
            var userId = GetUserId();
            var success = await _appService.CancelServiceAsync(serviceId, userId);
            return success ? NoContent() : BadRequest();
        }

        /// <summary>
        /// Endpoint para administradores/gestores: Servicios por Evento
        /// </summary>
        [HttpGet("by-event/{eventId}")]
        public async Task<ActionResult<List<ServiceStatusDto>>> GetByEvent(Guid eventId)
        {
            var services = await _appService.GetServicesByEventAsync(eventId);
            return Ok(services);
        }

        /// <summary>
        /// Métricas generales del sistema (Dashboard)
        /// </summary>
        [HttpGet("metrics")]
        public async Task<ActionResult<ServiceMetricsDto>> GetMetrics()
        {
            var metrics = await _appService.GetMetricsAsync();
            return Ok(metrics);
        }
    }
}
