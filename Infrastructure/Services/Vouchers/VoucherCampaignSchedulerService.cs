using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Vouchers
{
    public class VoucherCampaignSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<VoucherCampaignSchedulerService> _logger;
        private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(1); // Check every 1 minute

        public VoucherCampaignSchedulerService(
            IServiceScopeFactory scopeFactory,
            ILogger<VoucherCampaignSchedulerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("VoucherCampaignSchedulerService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateCampaignStatusesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing voucher campaign scheduler job.");
                }

                await Task.Delay(ScanInterval, stoppingToken);
            }
        }

        private async Task UpdateCampaignStatusesAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var campaignRepository = scope.ServiceProvider.GetRequiredService<IVoucherCampaignRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var now = DateTime.UtcNow.AddHours(7); // Vietnamese Time (GMT+7)

                // 1. Tự động Kích Hoạt Chiến Dịch (Draft -> Active)
                var pendingCampaigns = await campaignRepository.FindAsync(
                    c => c.Status == CampaignStatus.Draft 
                         && c.StartsAt <= now 
                         && c.ExpiresAt >= now 
                         && c.IsDeleted == false, 
                    cancellationToken);

                if (pendingCampaigns.Any())
                {
                    foreach (var campaign in pendingCampaigns)
                    {
                        campaign.Status = CampaignStatus.Active;
                        campaignRepository.Update(campaign);
                        _logger.LogInformation("Voucher Campaign '{Name}' ({Id}) has been automatically ACTIVATED.", campaign.Name, campaign.Id);
                    }
                }

                // 2. Tự động Kết Thúc Chiến Dịch (Active/Draft -> Ended)
                var activeCampaigns = await campaignRepository.FindAsync(
                    c => (c.Status == CampaignStatus.Active || c.Status == CampaignStatus.Draft) 
                         && c.ExpiresAt < now 
                         && c.IsDeleted == false, 
                    cancellationToken);

                if (activeCampaigns.Any())
                {
                    foreach (var campaign in activeCampaigns)
                    {
                        campaign.Status = CampaignStatus.Ended;
                        campaignRepository.Update(campaign);
                        _logger.LogInformation("Voucher Campaign '{Name}' ({Id}) has been automatically ENDED.", campaign.Name, campaign.Id);
                    }
                }

                if (pendingCampaigns.Any() || activeCampaigns.Any())
                {
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
