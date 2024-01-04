using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using sales.domain.Entities;
using sales.infra.DTOs;
using sales.infra.Interfaces;

namespace sales.API.Controllers
{
    [ApiController] 
    [Route("api/[controller]")] 
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IValidator<ProductDto> _validator;

        public ProductsController(IProductRepository productRepository, IValidator<ProductDto> validator)
        {
            _productRepository = productRepository;
            _validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var products = await _productRepository.GetAllAsync();

            var result = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price
            });

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create(ProductDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return ValidationProblem(ModelState);
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price
            };

            var created = await _productRepository.AddAsync(product);

            var result = new ProductDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                Price = created.Price
            };

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, ProductDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("Route id and body id do not match.");
            }

            var validationResult = await _validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return ValidationProblem(ModelState);
            }

            var existing = await _productRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.Price = dto.Price;

            var ok = await _productRepository.UpdateAsync(existing);

            if (!ok)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update product.");
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _productRepository.DeleteAsync(id);

            if (!ok)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
