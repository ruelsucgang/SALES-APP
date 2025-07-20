using System.Collections.Generic;

namespace sales.infra.DTOs
{
    public class CreateOrderDto
    {
        public List<AddOrderItemDto> OrderItems { get; set; } = new List<AddOrderItemDto>();
    }
}