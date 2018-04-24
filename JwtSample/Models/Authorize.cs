using System.Collections.Generic;

namespace JwtSample.Models
{
    public class JwtRole
    {
        public int nu_role { get; set; }
        public string tx_role { get; set; }
    }

    public class AuthorizeRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class AuthorizeResponse
    {
        public int id_usuario { get; set; }
        public string tx_apelido { get; set; }
        public string tx_email { get; set; }
    }

    public class LogonResult
    {
        public int id_usuario { get; set; }
        public string tx_apelido { get; set; }
        public string tx_email { get; set; }
        public int id_perfilacesso { get; set; }
        public string tx_perfilacesso { get; set; }
        public int[] nu_roles { get; set; }
    }
}
