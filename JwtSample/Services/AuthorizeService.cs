using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using JwtSample.Models;

namespace JwtSample.Services
{
    public interface IAuthorizeService
    {
        LogonResult Logon(string userName, string password);
        List<Claim> GetRolesClaims(int[] roles);
        string GenerateToken(Claim[] claims);
    }

    public class AuthorizeService : IAuthorizeService
    {
        private IConfiguration _cfg;
        private List<JwtRole> _roles;

        public AuthorizeService(IConfiguration configuration)
        {
            _cfg = configuration;
            GetRoles();
        }

        private void GetRoles()
        {
            // Voce pode pegar as roles existentes de um banco de dados
            _roles = new List<JwtRole>();
            for (int i = 0; i < 10; i++)
            {
                _roles.Add(new JwtRole() { nu_role = i, tx_role = string.Format("MyRole{0}", i) });
            }
        }

        public List<Claim> GetRolesClaims(int[] roles)
        {
            var claims = new List<Claim>();

            for(int i = 0; i < roles.Length; i++)
            {
                var obj = (from role in _roles where role.nu_role == roles[i] select role).FirstOrDefault();

                if (obj != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, obj.tx_role));
                }
            }
            return claims;
        }

        public LogonResult Logon(string userName, string password)
        {
            try
            {
                if (userName != "jwtuser" && password != "jwtpwd")
                {
                    return null;
                }
                // O conceito correto eh que o perfil de acesso esteja associado a varias roles
                // Ao efetuar o login do usuário voce, atraves de seu perfil de acesso, busca todas as roles associadas ao perfil
                return new LogonResult()
                {
                    id_usuario = 1,
                    id_perfilacesso = 1,
                    tx_apelido = "John Doe",
                    tx_perfilacesso = "Editor",
                    tx_email = "johndoe@noemail.com",
                    nu_roles = new int[3] { 2, 4, 6 }
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GenerateToken(Claim[] claims)
        {
            // JWT CONFIGURATION - START
            var jwtConfiguration = _cfg.GetSection(nameof(JwtBearerOptions));

            var jwt = new JwtSecurityToken(
                issuer: jwtConfiguration["Issuer"],
                audience: jwtConfiguration["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtConfiguration["TokenLifetimeInMinutes"])),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration["SecretKey"])), SecurityAlgorithms.HmacSha256)
            );

            var jwtToDelivery = new JwtSecurityTokenHandler().WriteToken(jwt);

            return jwtToDelivery;
        }
    }
}
