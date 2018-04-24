using JwtSample.Services;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Middlewares
{
    public class JwtSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private IAuthorizeService _auth;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="next"></param>
        public JwtSessionMiddleware(IAuthorizeService auth, RequestDelegate next)
        {
            _auth = auth;
            _next = next;
        }

        /// <summary>
        /// Middleware usado para renovar o token JWT em cada request
        /// Envia no Response Header um novo token para o client
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext httpContext)
        {
            bool useBearer = false;

            if (httpContext.Request.Headers.Keys.Contains("Authorization"))
            {
                var authValue = httpContext.Request.Headers["Authorization"].ToString();

                useBearer = authValue.ToLower().StartsWith("bearer");
            }

            if (useBearer)
            {
                var encodedJwt = _auth.GenerateToken(httpContext.User.Claims.ToArray());

                httpContext.Response.Headers.Add("jwtsession", encodedJwt);
            }

            await _next(httpContext);
        }
    }
}

