using Aspel.CoreFiscal.Cancelacion.Application.Interfaces;
using Aspel.CoreFiscal.Cancelacion.Domain.Entities;
using Aspel.CoreFiscal.Cancelacion.Domain.Enums;
using Aspel.CoreFiscal.Cancelacion.Domain.ValueObjects;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Aspel.CoreFiscal.Cancelacion.Infrastructure.Persitence
{
    public class CfdiCancellationRepository : ICfdiCancellationRepository
    {
        private readonly string _connectionString;

        public CfdiCancellationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new System.ArgumentNullException("La cadena de conexión 'DefaultConnection' no está configurada.");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<CfdiDocument?> GetByUuidAsync(string uuid, CancellationToken cancellationToken)
        {
            using var connection = CreateConnection();
            const string sql = "SELECT Id, Uuid, RfcEmisor, RfcReceptor, PacId, State, Acuse, VersionCfdi FROM Cancelaciones WHERE Uuid = @Uuid";

            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Uuid = uuid });

            if (result == null) return null;

            return new CfdiDocument
            {
                Id = result.Id,
                Uuid = result.Uuid,
                RfcEmisor = new Rfc((string)result.RfcEmisor),
                RfcReceptor = new Rfc((string)result.RfcReceptor),
                PacId = result.PacId,
                State = (CfdiState)result.State,
                Acuse = result.Acuse,
                VersionCfdi = result.VersionCfdi
            };
        }

        public async Task SaveAsync(CfdiDocument document, CancellationToken cancellationToken)
        {
            using var connection = CreateConnection();
            // Mapeo directo al Stored Procedure legacy "ActualizaCancelaciones"
            var parameters = new DynamicParameters();
            parameters.Add("@Uuid", document.Uuid);
            parameters.Add("@RfcEmisor", document.RfcEmisor.Value);
            parameters.Add("@PacId", document.PacId);
            parameters.Add("@NuevoEstado", (int)document.State);
            parameters.Add("@Acuse", document.Acuse);

            await connection.ExecuteAsync("ActualizaCancelaciones", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateStatusAsync(CfdiDocument document, CancellationToken cancellationToken)
        {
            using var connection = CreateConnection();
            const string sql = "UPDATE Cancelaciones SET State = @State, Acuse = @Acuse, PacId = @PacId WHERE Uuid = @Uuid";

            await connection.ExecuteAsync(sql, new
            {
                State = (int)document.State,
                Acuse = document.Acuse,
                PacId = document.PacId,
                Uuid = document.Uuid
            });
        }
    }
}
