using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Myapp.GeneralClass;
using MyApp.GeneralClass;

namespace MyApp.Products
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : BaseController
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

       //GET: api/products
       [Authorize(Roles = "admin")]
       [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProductDTO>>>> GetProducts()
        {
            try
            {
                var products = await _productService.GetProductsAsync();
                var productDTOs = _productService.MapToListDTOs(products);

                var response = new ApiResponse<List<ProductDTO>>(
                    success: true,
                    message: "Products retrieved successfully.",
                    data: productDTOs
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<ProductDTO>>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }



        // GET: api/products/my-products
        [Authorize(Roles = "user,admin,service")]
        [HttpGet("my-products")]
        public async Task<ActionResult<ApiResponse<List<ProductDTO>>>> GetMyProducts()
        {
            try
            {
                var userId = GetCurrentUserId();
                var products = await _productService.GetMyProductsAsync(userId);
                
                var productDTOs = _productService.MapToListDTOs(products);

                var response = new ApiResponse<List<ProductDTO>>(
                    success: true,
                    message: "Products retrieved successfully.",
                    data: productDTOs
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<ProductDTO>>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }



        // GET: api/products/{id}
        [Authorize(Roles = "user,admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDTO>>> GetProduct(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var product = await _productService.GetProductByIdAsync(id);
                if (product == null || product.CreatedBy != userId)
                {
                    var notFoundResponse = new ApiResponse<ProductDTO>(
                        success: false,
                        message: "Product not found or access denied.",
                        data: null
                    );
                    return BadRequest(notFoundResponse);
                }


                var productDTO = _productService.MapToDTO(product);

                var response = new ApiResponse<ProductDTO>(
                    success: true,
                    message: "Product retrieved successfully.",
                    data: productDTO
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<ProductDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // POST: api/products
        [Authorize(Roles = "user,admin,service")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductDTO>>> CreateProduct([FromBody] ProductRequest request)
        {
            try
            {   
                //product.Id = Guid.NewGuid();
                Product product;

                var userId = GetCurrentUserId();
                request.CreatedBy = userId;

                product = await _productService.CreateProductAsync(request);
                var productDTO = _productService.MapToDTO(product);

                var response = new ApiResponse<ProductDTO>(
                    success: true,
                    message: "Product created successfully.",
                    data: productDTO
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<ProductDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // PUT: api/products/{id}
        [Authorize(Roles = "user,admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDTO>>> UpdateProduct(Guid id, [FromBody] Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    var badRequestResponse = new ApiResponse<ProductDTO>(
                        success: false,
                        message: "Product ID mismatch.",
                        data: null
                    );
                    return BadRequest(badRequestResponse);
                }

                var userId = GetCurrentUserId();

                var existingProduct = await _productService.GetProductByIdAsync(product.Id);
                if (existingProduct == null || existingProduct.CreatedBy != userId)
                {
                    var notFoundResponse = new ApiResponse<ProductDTO>(
                         success: false,
                         message: "Product not found or access denied.",
                         data: null
                    );
                    return BadRequest(notFoundResponse);
                }

                await _productService.UpdateProductAsync(id, product);

                var productDTO = _productService.MapToDTO(product);

                var response = new ApiResponse<ProductDTO>(
                    success: true,
                    message: "Product updated successfully.",
                    data: productDTO
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<ProductDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // DELETE: api/products/{id}
        [Authorize(Roles = "user,admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteProduct(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var product = await _productService.GetProductByIdAsync(id);
                if (product == null || product.CreatedBy != userId)
                {
                    var notFoundResponse = new ApiResponse<string>(
                        success: false,
                        message: "Product not found or access denied.",
                        data: null
                    );
                    return NotFound(notFoundResponse);
                }

                await _productService.DeleteProductAsync(id);

                var response = new ApiResponse<string>(
                    success: true,
                    message: "Product deleted successfully.",
                    data: null
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}