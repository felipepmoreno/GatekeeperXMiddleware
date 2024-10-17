using GatekeeperX.Middleware;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatekeeperX.Extensions
{
    public static class GatekeeperXMiddlewareExtensions
    {
        public static IApplicationBuilder UseGatekeeperX(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GatekeeperXValidationMiddleware>();
        }
    }
}
