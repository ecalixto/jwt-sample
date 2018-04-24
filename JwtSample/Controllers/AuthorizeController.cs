using JwtSample.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace JwtSample.Controllers
{
    [Produces("application/json")]
    public class AuthorizeController : Controller
    {
        private IAuthorizeService _auth;

        public AuthorizeController(IAuthorizeService auth)
        {
            _auth = auth;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/authorize")]
        public IActionResult Post([FromBody] Models.AuthorizeRequest request)
        {
            var obj = _auth.Logon(request.username, request.password);

            if (obj == null)
            {
                return BadRequest();
            }

            Models.AuthorizeResponse objData = new Models.AuthorizeResponse()
            {
                id_usuario = obj.id_usuario,
                tx_apelido = obj.tx_apelido,
                tx_email = obj.tx_email,
            };

            var claims = _auth.GetRolesClaims(obj.nu_roles);
            claims.InsertRange(0,new Claim[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        new Claim(JwtRegisteredClaimNames.UniqueName, obj.tx_email),
                        new Claim(ClaimTypes.Actor, JsonConvert.SerializeObject(objData)),
                });

            var response = new
            {
                data = objData,
                access_token = _auth.GenerateToken(claims.ToArray())
            };

            return new OkObjectResult(response);
        }
    }
}