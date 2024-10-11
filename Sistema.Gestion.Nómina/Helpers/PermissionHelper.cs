using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public static class PermissionHelper
{
    private static IHttpContextAccessor _httpContextAccessor;
    // Método para inicializar el HttpContextAccessor
    public static void Configure(IHttpContextAccessor contextAccessor)
    {
        _httpContextAccessor = contextAccessor;
    }

    // Método para verificar si el usuario tiene un permiso específico
    public static bool HasPermission(string permission)
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        var userClaims = httpContext?.User?.FindAll("Permission");
        return userClaims != null && userClaims.Any(p => p.Value == permission);
    }

    // Método para verificar si el usuario tiene un rol específico
    public static bool HasRole(string role)
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        var userRole = httpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
        return userRole != null && userRole == role;
    }
}

