using Application.Common;
using Application.DTOs.Enum;

namespace Application.Interfaces.Services
{
    public interface IEnumService
    {
        Task<OperationResult<Dictionary<string, List<EnumValueDto>>>> GetEnumsAsync(CancellationToken cancellationToken);
        Task<OperationResult<List<string>>> GetEnumNamesAsync(CancellationToken cancellationToken);
        Task<OperationResult<List<EnumValueDto>>> GetEnumValuesAsync(string enumName, CancellationToken cancellationToken);
    }
}
