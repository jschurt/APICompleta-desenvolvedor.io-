using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevIO.Api.Extensions
{

    /// <summary>
    /// Classe para criar healthchecks personalizados (neste caso, um check para o numero de registros na tabela produtos)
    /// </summary>
    public class SqlServerHealthCheckExtensions : IHealthCheck
    {
        readonly string _connection;

        public SqlServerHealthCheckExtensions(string connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Teste de saude ficticio. A aplicacao estara saudavel se contiver dados na tabela produtos, caso contrario nao estara.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                using (var connection = new SqlConnection(_connection))
                {
                    await connection.OpenAsync(cancellationToken);

                    var command = connection.CreateCommand();
                    command.CommandText = "select count(id) from produtos";

                    return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) < 0 ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
                }
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy();
            }
        }
    }

}
