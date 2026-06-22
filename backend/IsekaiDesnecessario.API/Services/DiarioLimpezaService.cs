using IsekaiDesnecessario.API.Data;
using Microsoft.EntityFrameworkCore;

namespace IsekaiDesnecessario.API.Services;

public class DiarioLimpezaService(IServiceScopeFactory scopeFactory, ILogger<DiarioLimpezaService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var agora = DateTime.UtcNow;
            // Próxima meia-noite UTC
            var proximaMeia = agora.Date.AddDays(1);
            await Task.Delay(proximaMeia - agora, ct);

            if (ct.IsCancellationRequested) break;

            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var corte = DateTime.UtcNow.AddHours(-24);
                var removidos = await db.DiarioAcoes
                    .Where(d => d.Data < corte)
                    .ExecuteDeleteAsync(ct);
                logger.LogInformation("DiarioLimpeza: {n} entrada(s) removida(s).", removidos);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "DiarioLimpeza: erro ao limpar entradas antigas.");
            }
        }
    }
}
