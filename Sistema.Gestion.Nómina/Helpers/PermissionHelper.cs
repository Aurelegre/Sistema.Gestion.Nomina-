using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public static class PermissionHelper
{
    // Método para verificar si el usuario tiene un permiso específico
    public static bool HasPermission(HttpContext httpContext, string permission)
    {
        var userClaims = httpContext.User?.FindAll("Permission");
        return userClaims != null && userClaims.Any(p => p.Value == permission);
    }

    // Método para verificar si el usuario tiene un rol específico
    public static bool HasRole(HttpContext httpContext, string role)
    {
        var userRole = httpContext.User?.FindFirst(ClaimTypes.Role)?.Value;
        return userRole != null && userRole == role;
    }
}
