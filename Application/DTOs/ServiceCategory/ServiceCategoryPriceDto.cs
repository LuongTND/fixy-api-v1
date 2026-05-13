namespace Application.DTOs.ServiceCategory
{
    public class ServiceCategoryPriceDto
    {
        public Guid CategoryId { get; set; }
        public long? MinPrice { get; set; }
        public long? MaxPrice { get; set; }
    }
}
