using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Support
{
    public class BookingSupportDto
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public long? FinalPrice { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}
