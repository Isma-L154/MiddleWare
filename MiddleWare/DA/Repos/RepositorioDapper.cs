using Autorizacion.Abstracciones.DA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autorizacion.DA.Repos
{
    public class RepositorioDapper : IRepositorioDapper
    {
        private readonly IConfiguration _configutarion;
        private SqlConnection _connection;

        public RepositorioDapper(IConfiguration configutarion)
        {
            _configutarion = configutarion;
            _connection = new SqlConnection(_configutarion.GetConnectionString("BDSeguridad"));
        }

        public SqlConnection ObtenerRepositorioDapper()
        {
            return _connection;
        }
    }
}
