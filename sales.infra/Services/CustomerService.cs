using sales.domain;
using sales.domain.Entities;
using sales.infra.DTOs;
using sales.infra.Interfaces;

namespace sales.infra.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;

        public CustomerService(ICustomerRepository customerRepository, IUserRepository userRepository)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
        }

        public async Task<CustomerDto> RegisterCustomerAsync(CustomerRegisterRequestDto request)
        {
            // Check if email already exists (any role)
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already registered.");
            }

            // Create User record (Role = Customer, no password)
            var user = new User
            {
                Email = request.Email,
                Role = "Customer",
                PasswordHash = null, // No password for OTP login
                IsApproved = true,   // Auto-approve customers
                IsBlocked = false
            };

            var createdUser = await _userRepository.AddAsync(user);

            // Create Customer record
            var customer = new Customer
            {
                UserId = createdUser.Id,
                FullName = request.FullName,
                Email = request.Email,
                BillingAddress = request.BillingAddress,
                ContactNumber = request.ContactNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var createdCustomer = await _customerRepository.CreateAsync(customer);

            return MapToDto(createdCustomer);
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            return customer == null ? null : MapToDto(customer);
        }

        public async Task<CustomerDto?> GetCustomerByUserIdAsync(int userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            return customer == null ? null : MapToDto(customer);
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Select(MapToDto);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(int userId, CustomerUpdateDto updateDto)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
            {
                throw new InvalidOperationException("Customer not found.");
            }

            // Update fields
            customer.FullName = updateDto.FullName;
            customer.BillingAddress = updateDto.BillingAddress;
            customer.ContactNumber = updateDto.ContactNumber;
            customer.UpdatedAt = DateTime.UtcNow;

            var updatedCustomer = await _customerRepository.UpdateAsync(customer);
            return MapToDto(updatedCustomer);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            await _customerRepository.DeleteAsync(id);
        }

        private CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                UserId = customer.UserId,
                FullName = customer.FullName,
                Email = customer.Email,
                BillingAddress = customer.BillingAddress,
                ContactNumber = customer.ContactNumber,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }
    }
}