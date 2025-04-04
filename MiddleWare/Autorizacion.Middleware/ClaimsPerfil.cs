using Autorizacion.Abstracciones.BW;
using Autorizacion.Abstracciones.Modelos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Autorizacion.Middleware
{
    public class ClaimsPerfil
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private IAutorizacionBW _autorizacionBW;

        public ClaimsPerfil(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }
        //Capturamos lo que hay e intercepta el flujo
        public async Task InvokeAsync(HttpContext httpContext, IAutorizacionBW autorizacionBW)
        {
            _autorizacionBW = autorizacionBW;
            ClaimsIdentity appIdentity = await ValidarAutorizacion(httpContext);
            httpContext.User.AddIdentity(appIdentity); //Se inyecta un Claim al Usuario
            await _next(httpContext); // Se lo mandamos a la aplicacion para que continue
        }

        private async Task<ClaimsIdentity> ValidarAutorizacion(HttpContext httpContext)
        {
            var claims = new List<Claim>();
            if (httpContext.User != null && httpContext.User.Identity.IsAuthenticated) //Si el usuario esta autenticado, obtenemos la info del usuario 
            {
                await ObtenerUsuario(httpContext, claims);
                await ObtenerPerfiles(httpContext, claims);
            }
            var appIdentity = new ClaimsIdentity(claims); //Le mandamos la info previamente recolectada
            return appIdentity;
        }

        //Perfiles ----
        private async Task ObtenerPerfiles(HttpContext httpContext, List<Claim> claims)
        {
            var perfiles = await obtenerInformacionPerfiles(httpContext);
            if (perfiles != null && perfiles.Any())
            {
                foreach (var perfil in perfiles)
                {
                    //Realizamos un Claim del ID/Rol que tiene 
                    claims.Add(new Claim(ClaimTypes.Role, perfil.Id.ToString()));
                }
            }
        }

        private async Task<IEnumerable<Perfil>> obtenerInformacionPerfiles(HttpContext httpContext)
        {
            //Obtenemos la info del perfil por medio del flujo del BW
            return await _autorizacionBW.ObtenerPerfilesxUsuario(new Abstracciones.Modelos.Usuario { NombreUsuario = httpContext.User.Claims.Where(c => c.Type == "usuario").FirstOrDefault().Value });
        }

        //Usuarios ----
        private async Task ObtenerUsuario(HttpContext httpContext, List<Claim> claims)
        {
            var usuario = await obtenerInformacionUsuario(httpContext);
            if (usuario is not null && !string.IsNullOrEmpty(usuario.Id.ToString()) && !string.IsNullOrEmpty(usuario.NombreUsuario.ToString()) && !string.IsNullOrEmpty(usuario.CorreoElectronico.ToString()))
            {
                //Si todo es correcto, y ninguna info del user es null, añadimos un claim por cada atributo que sea necesario
                claims.Add(new Claim(ClaimTypes.Email, usuario.CorreoElectronico));
                claims.Add(new Claim(ClaimTypes.Name, usuario.NombreUsuario));
                claims.Add(new Claim("IdUsuario", usuario.Id.ToString()));
            }
        }

        private async Task<Usuario> obtenerInformacionUsuario(HttpContext httpContext)
        {
            //Obtenemos la info del usuario por medio del flujo del BW
            return await _autorizacionBW.ObtenerUsuario(new Abstracciones.Modelos.Usuario { NombreUsuario = httpContext.User.Claims.Where(c => c.Type == "usuario").FirstOrDefault().Value }); 
        }
    }
    public static class ClaimsUsuarioMiddlewareExtensions
    {
        public static IApplicationBuilder AutorizacionClaims(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClaimsPerfil>();
        }
    }
}
