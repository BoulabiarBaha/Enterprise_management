using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Myapp.Models;
using MyApp.GeneralClass;

namespace Myapp.Billings
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        private readonly BillingService _billingService;

        public BillingController(BillingService billingService)
        {
            _billingService = billingService;
        }

        // POST: api/billings
        [Authorize(Roles = "user,admin")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<BillingDTO>>> CreateBilling([FromBody] Billing billing, [FromQuery] Guid transactionId)
        {
            try
            {
                var createdBilling = await _billingService.CreateBillingAsyncManually(billing, transactionId);
                var mappedCreatedBilling = _billingService.MapToBillingDTO(createdBilling);
                var response = new ApiResponse<BillingDTO>(
                    success: true,
                    message: "Billing created successfully.",
                    data: mappedCreatedBilling
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<BillingDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // PUT: api/billings/{id}
        [Authorize(Roles = "user,admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<BillingDTO>>> UpdateBilling(Guid id, [FromBody] Billing billing)
        {
            try
            {
                if (id != billing.Id)
                {
                    var badRequestResponse = new ApiResponse<BillingDTO>(
                        success: false,
                        message: "Billing ID mismatch.",
                        data: null
                    );
                    return BadRequest(badRequestResponse);
                }

                await _billingService.UpdateBillingAsync(id, billing);
                var updatedBilling = await _billingService.GetBillingAsync(id);
                var mappedupdatedBilling = _billingService.MapToBillingDTO(updatedBilling);
                var response = new ApiResponse<BillingDTO>(
                    success: true,
                    message: "Billing updated successfully.",
                    data: mappedupdatedBilling
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<BillingDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // DELETE: api/billings/{id}
        [Authorize(Roles = "user,admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<BillingDTO>>> DeleteBilling(Guid id)
        {
            try
            {
                await _billingService.DeleteBillingAsync(id);
                var response = new ApiResponse<BillingDTO>(
                    success: true,
                    message: "Billing deleted successfully.",
                    data: null
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<BillingDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // GET: api/billings/transactions/{transactionId}/billing
        [Authorize(Roles = "user,admin")]
        [HttpGet("transactions/{transactionId}/billing")]
        public async Task<ActionResult<ApiResponse<BillingDTO>>> GetBillingByTransactionId(Guid transactionId)
        {
            try
            {
                var billing = await _billingService.GetBillingByTransactionIdAsync(transactionId);
                var billingDTO = _billingService.MapToBillingDTO(billing);
                var response = new ApiResponse<BillingDTO>(
                    success: true,
                    message: "Billing retrieved successfully.",
                    data: billingDTO
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<BillingDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}