using IITS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace IITS.Services;

public class EmailOutboxHostedService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<EmailOutboxHostedService> _log;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    public EmailOutboxHostedService(IServiceProvider sp, ILogger<EmailOutboxHostedService> log)
    {
        _sp = sp;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (!await db.TableExistsAsync("EmailOutbox", stoppingToken))
                {
                    await Task.Delay(Interval, stoppingToken);
                    continue;
                }
                var count = await db.EmailOutbox.CountAsync(e => e.Status == "Pending", stoppingToken);
                if (count > 0)
                {
                    var svc = scope.ServiceProvider.GetRequiredService<IEmailOutboxService>();
                    await svc.ProcessPendingAsync(20);
                }
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                // Tabla EmailOutbox aún no existe (migraciones pendientes); no loguear.
            }
            catch (OperationCanceledException)
            {
                // Cierre de la aplicación; no loguear.
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "EmailOutbox processor error");
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }
}
