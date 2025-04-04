using Autorizacion.Abstracciones.BW;
using Autorizacion.Abstracciones.DA;
using Autorizacion.Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autorizacion.BW
{
    public class AutorizacionBW : IAutorizacionBW
    {
        private ISeguridadDA _seguridadDA;

        public AutorizacionBW(ISeguridadDA seguridadDA)
        {
            _seguridadDA = seguridadDA;
        }
        public async Task<IEnumerable<Perfil>> ObtenerPerfilesxUsuario(Usuario usuario)
        {
            return await _seguridadDA.ObtenerPerfilesxUsuario(usuario);
        }

        public async Task<Usuario> ObtenerUsuario(Usuario usuario)
        {
            return await _seguridadDA.ObtenerUsuario(usuario);
        }
    }
}
