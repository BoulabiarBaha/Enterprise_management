using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Myapp.Models;
using MyApp.GeneralClass;

namespace Myapp.Transactions
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionsController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // POST: api/transactions
        [Authorize(Roles = "user,admin")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TransactionDTO>>> CreateTransaction([FromBody] TransactionRequest transactionReq)
        {
            try
            {
                var transaction = new Transaction{
                    ClientId = transactionReq.ClientId,
                };
                transactionReq.SoldProducts.ForEach( item => transaction.SoldProducts.Add(item));
                var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
                var mappedTransaction = _transactionService.MapToTransactionDTO(createdTransaction);
                var response = new ApiResponse<TransactionDTO>(
                    success: true,
                    message: "Transaction created successfully.",
                    data: mappedTransaction
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<TransactionDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // GET: api/transactions
        [Authorize(Roles = "user,admin")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TransactionDTO>>>> GetTransactions()
        {
            try
            {
                var transactions = await _transactionService.GetTransactionsAsync();
                var transactionDtos = _transactionService.MapToListDTOs(transactions);
                var response = new ApiResponse<List<TransactionDTO>>(
                    success: true,
                    message: "Transactions retrieved successfully.",
                    data: transactionDtos
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<TransactionDTO>>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // GET: api/transactions/{id}
        [Authorize(Roles = "user,admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TransactionDTO>>> GetTransaction(Guid id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionAsync(id);
                if (transaction == null)
                {
                    var notFoundResponse = new ApiResponse<TransactionDTO>(
                        success: false,
                        message: "Transaction not found.",
                        data: null
                    );
                    return BadRequest(notFoundResponse);
                }
                var mappedTransaction = _transactionService.MapToTransactionDTO(transaction);
                var response = new ApiResponse<TransactionDTO>(
                    success: true,
                    message: "Transaction retrieved successfully.",
                    data: mappedTransaction
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<Transaction>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // DELETE: api/transactions/{id}
        [Authorize(Roles = "user,admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(Guid id)
        {
            try
            {
                await _transactionService.DeleteTransactionAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<Transaction>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}