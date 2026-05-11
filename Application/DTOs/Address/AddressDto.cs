namespace Application.DTOs.Address
{
    public class AddressDto
    {
        public Guid Id { get; set; }

        public string? Label { get; set; }

        public string City { get; set; } = default!;

        public string District { get; set; } = default!;

        public string Ward { get; set; } = default!;

        public string Detail { get; set; } = default!;

        public double? Lat { get; set; }

        public double? Lng { get; set; }

        public bool IsDefault { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
