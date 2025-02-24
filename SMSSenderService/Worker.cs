using NotificationService.Services;

using Serilog;
using static System.Formats.Asn1.AsnWriter;

namespace NotificationService;

public class Worker(IServiceProvider serviceProvider) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("NotificationWorker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                SmsSenderService smsSenderService = scope.ServiceProvider.GetRequiredService<SmsSenderService>();
                EmailSenderService emailSenderService = scope.ServiceProvider.GetRequiredService<EmailSenderService>();

                await Task.WhenAll(
                    smsSenderService.StartTimer(stoppingToken),
                    emailSenderService.StartTimer(stoppingToken)
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while executing background worker.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }

        Log.Information("NotificationWorker is stopping.");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("NotificationWorker is stopping.");
        await base.StopAsync(cancellationToken);
    }

}