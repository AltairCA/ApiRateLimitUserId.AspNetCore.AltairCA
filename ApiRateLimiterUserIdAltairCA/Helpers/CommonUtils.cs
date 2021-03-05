using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using APIRateLimiterUserId.AspNetCore.AltairCA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Helpers
{
    internal static class CommonUtils
    {
        /// <summary>
        /// Get Path using httpContext
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetPath(IHttpContextAccessor httpContext)
        {
            var rd = httpContext.HttpContext.GetRouteData();
            string currentController = rd.Values["controller"].ToString();
            string currentAction = rd.Values["action"].ToString();
            string path = string.Concat(currentController, "/", currentAction);
            string method = httpContext.HttpContext.Request.Method;
            return string.Concat($"{method}:",path);
        }
        /// <summary>
        /// Get Client Ip using HttpContextAccessor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetUserId(APIRateLimiterUserIdOptions settings, IHttpContextAccessor httpContext)
        {
            string ip = string.Empty;
            if (httpContext.HttpContext?.User?.Identity?.IsAuthenticated ?? false && !string.IsNullOrWhiteSpace(settings.UserIdClaim))
            {
                ip = httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == settings.UserIdClaim)?.Value;
            }
            return ip;
        }
        public static string GetKey(string clientId)
        {
            using (var algorithm = SHA512.Create()) //or MD5 SHA256 etc.
            {
                var hashedBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(clientId)));

                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
