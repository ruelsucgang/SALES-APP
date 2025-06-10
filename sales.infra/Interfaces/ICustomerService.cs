using sales.infra.DTOs;

namespace sales.infra.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerDto> RegisterCustomerAsync(CustomerRegisterRequestDto request);
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto?> GetCustomerByUserIdAsync(int userId);
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto> UpdateCustomerAsync(int userId, CustomerUpdateDto updateDto);
        Task DeleteCustomerAsync(int id);
    }
}