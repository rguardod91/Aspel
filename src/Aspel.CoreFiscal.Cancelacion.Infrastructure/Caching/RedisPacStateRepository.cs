using Aspel.CoreFiscal.Cancelacion.Application.Interfaces;
using Aspel.CoreFiscal.Cancelacion.Domain.Entities;
using StackExchange.Redis;
using System.Text.Json;

namespace Aspel.CoreFiscal.Cancelacion.Infrastructure.Caching
{
    public class RedisPacStateRepository : IPacStateRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private const string PacHashKey = "CoreFiscal:PacStates";

        public RedisPacStateRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<IReadOnlyList<PacNode>> GetAllPacsAsync(CancellationToken cancellationToken)
        {
            var db = _redis.GetDatabase();
            var entries = await db.HashGetAllAsync(PacHashKey);

            if (entries.Length == 0) return InitializeDefaultPacs();

            return entries
                .Where(e => e.Value.HasValue)
                .Select(e => JsonSerializer.Deserialize<PacNode>(e.Value.ToString()!))
                .Where(n => n != null)
                .ToList()!;
        }

        public async Task<PacNode?> GetPacByIdAsync(int pacId, CancellationToken cancellationToken)
        {
            var db = _redis.GetDatabase();
            var entry = await db.HashGetAsync(PacHashKey, pacId.ToString());

            if (!entry.HasValue) return null;

            return JsonSerializer.Deserialize<PacNode>(entry.ToString()!);
        }

        public async Task UpdatePacAsync(PacNode pacNode, CancellationToken cancellationToken)
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(pacNode);

            await db.HashSetAsync(PacHashKey, pacNode.Id.ToString(), json);
        }

        // Método de apoyo para inyectar la configuración inicial si Redis está vacío
        private IReadOnlyList<PacNode> InitializeDefaultPacs()
        {
            return new List<PacNode>
        {
            new PacNode { Id = 7, Name = "Aspel", AssignedPercentage = 80, CurrentScore = 0 },
            new PacNode { Id = 6, Name = "ComercioDigital", AssignedPercentage = 10, CurrentScore = 0 },
            new PacNode { Id = 5, Name = "Pegaso", AssignedPercentage = 10, CurrentScore = 0 }
        };
        }
    }
}
