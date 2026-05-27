using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IVoucherCampaignRepository : IRepository<VoucherCampaign>
    {
        Task<(List<VoucherCampaign> Items, int TotalCount)> GetPagedAsync(string? searchTerm,int pageNumber,int pageSize,CancellationToken cancellationToken = default);
    }
}
