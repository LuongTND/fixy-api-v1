using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Application.Common;
using Application.DTOs.Enum;
using Application.Interfaces.Services;
using Domain.Enum;

namespace Infrastructure.Services
{
    public class EnumService : IEnumService
    {
        private static Dictionary<string, List<EnumValueDto>>? _cachedEnums;
        private static readonly object _lock = new();

        public Task<OperationResult<Dictionary<string, List<EnumValueDto>>>> GetEnumsAsync(CancellationToken cancellationToken)
        {
            var enums = EnsureEnumsLoaded();
            return Task.FromResult(OperationResult<Dictionary<string, List<EnumValueDto>>>.Success(enums, "Get all enums successfully"));
        }

        public Task<OperationResult<List<string>>> GetEnumNamesAsync(CancellationToken cancellationToken)
        {
            var enums = EnsureEnumsLoaded();
            var names = enums.Keys.OrderBy(k => k).ToList();
            return Task.FromResult(OperationResult<List<string>>.Success(names, "Get enum names successfully"));
        }

        public Task<OperationResult<List<EnumValueDto>>> GetEnumValuesAsync(string enumName, CancellationToken cancellationToken)
        {
            var enums = EnsureEnumsLoaded();
            if (enums.TryGetValue(enumName.ToLowerInvariant(), out var values))
            {
                return Task.FromResult(OperationResult<List<EnumValueDto>>.Success(values, $"Get enum '{enumName}' values successfully"));
            }

            return Task.FromResult(OperationResult<List<EnumValueDto>>.Failure($"Enum '{enumName}' not found"));
        }

        private Dictionary<string, List<EnumValueDto>> EnsureEnumsLoaded()
        {
            if (_cachedEnums != null) return _cachedEnums;

            lock (_lock)
            {
                if (_cachedEnums != null) return _cachedEnums;

                var result = new Dictionary<string, List<EnumValueDto>>();
                
                // Get all types in the assembly of a representative enum from Domain.Enum namespace
                var enumTypes = typeof(BookingStatus).Assembly.GetTypes()
                    .Where(t => t.IsEnum && t.Namespace == "Domain.Enum")
                    .ToList();

                foreach (var type in enumTypes)
                {
                    var values = new List<EnumValueDto>();
                    foreach (var value in Enum.GetValues(type))
                    {
                        var name = Enum.GetName(type, value);
                        if (name == null) continue;

                        var field = type.GetField(name);
                        var displayName = name;

                        if (field != null)
                        {
                            displayName = GetDisplayName(field);
                        }

                        values.Add(new EnumValueDto
                        {
                            Value = (int)value,
                            Name = name,
                            DisplayName = displayName
                        });
                    }

                    // Key in lowercase for case-insensitive lookup
                    result[type.Name.ToLowerInvariant()] = values;
                }

                _cachedEnums = result;
                return _cachedEnums;
            }
        }

        private string GetDisplayName(FieldInfo field)
        {
            // 1. Try DescriptionAttribute
            var descAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr != null) return descAttr.Description;

            // 2. Try DisplayAttribute
            var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
            if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name)) return displayAttr.Name;

            // 3. Fallback: Split PascalCase (e.g. InProgress -> In Progress)
            return System.Text.RegularExpressions.Regex.Replace(field.Name, "([a-z])([A-Z])", "$1 $2");
        }
    }
}
