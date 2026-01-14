using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ComplementaryServices.Infrastructure.Notifications
{
    /// <summary>
    /// Proveedor personalizado para obtener el UserId desde los claims del token
    /// Integración con Keycloak
    /// </summary>
    public class SignalRUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Obtener el ID del usuario desde los claims del token de Keycloak
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? connection.User?.FindFirst("sub")?.Value; // 'sub' es común en tokens JWT
        }
    }
}
