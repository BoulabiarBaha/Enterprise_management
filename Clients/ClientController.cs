using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Myapp.GeneralClass;
using Myapp.Models;
using MyApp.GeneralClass;
using MyApp.Products;

namespace Myapp.Clients
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : BaseController
    {
        private readonly ClientService _clientService;

        public ClientsController(ClientService clientService)
        {
            _clientService = clientService;
        }

        //GET: api/clients
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ClientDTO>>>> GetClients(){
            try{
                var clients = await _clientService.GetClientsAsync();
                var clientDTOs = _clientService.MapToListDTOs(clients);

                var response = new ApiResponse<List<ClientDTO>>(
                    success: true,
                    message: "Clients retrieved successfully.",
                    data: clientDTOs
                );
                return Ok(response);
            }
            catch(Exception ex){
                var errorResponse = new ApiResponse<List<Client>>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }


        //GET: api/clients
        [Authorize(Roles = "user,admin,service")]
        [HttpGet("my-clients")]
        public async Task<ActionResult<ApiResponse<List<ClientDTO>>>> GetMyClients()
        {
            try
            {
                var userId = GetCurrentUserId();
                var clients = await _clientService.GetMyClientsAsync(userId);
                var clientDTOs = _clientService.MapToListDTOs(clients);

                var response = new ApiResponse<List<ClientDTO>>(
                    success: true,
                    message: "Clients retrieved successfully.",
                    data: clientDTOs
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<Client>>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }


        //GET: api/clients/{id}
        [Authorize(Roles = "user,admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ClientDTO>>> GetClient(Guid id)
        {
            try{

                var userId = GetCurrentUserId();
                var client = await _clientService.GetClientAsync(id);
                if (client == null || client.CreatedBy != userId)
                {
                        var notFoundResponse = new ApiResponse<ClientDTO>(
                        success: false,
                        message: "Client not found or access denied.",
                        data: null
                    );
                    return BadRequest(notFoundResponse);
                }
                var clientDTO = _clientService.MapToClientDTO(client);
                var response = new ApiResponse<ClientDTO>(
                    success: true,
                    message: "Client retrieved successfully.",
                    data: clientDTO
                );
                return Ok(response);
            }
            catch(Exception ex){
                var errorResponse = new ApiResponse<ClientDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        //POST: api/clients
        [Authorize(Roles = "user,admin,service")]
        [HttpPost]
        public async Task<ActionResult<Client>> CreateClient([FromBody] Client client)
        {
            try
            {
                client.CreatedBy = GetCurrentUserId();
                await _clientService.CreateClientAsync(client);

                var clientDTO = _clientService.MapToClientDTO(client);
                var response = new ApiResponse<ClientDTO>(
                    success: true,
                    message: "Client created successfully.",
                    data: clientDTO
                );
                return Ok(response);
            }

            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<ClientDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // PUT: api/clients/{id}
        [Authorize(Roles = "user,admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ClientDTO>>> UpdateClient(Guid id, [FromBody] Client client)
        {
            try{
                if (id != client.Id)
                {
                    var badRequestResponse = new ApiResponse<ClientDTO>(
                        success: false,
                        message: "Client ID mismatch.",
                        data: null
                    );
                    return BadRequest(badRequestResponse);
                }

                var userId = GetCurrentUserId();

                var existingClient = await _clientService.GetClientAsync(id);
                if (existingClient == null || existingClient.CreatedBy != userId)
                {
                    var notFoundResponse = new ApiResponse<ProductDTO>(
                         success: false,
                         message: "Client not found or access denied.",
                         data: null
                    );
                    return BadRequest(notFoundResponse);
                }

                await _clientService.UpdateClientAsync(id, client);
                var clientDTO = _clientService.MapToClientDTO(client);

                var response = new ApiResponse<ClientDTO>(
                    success: true,
                    message: "Client updated successfully.",
                    data: clientDTO
                );
                return Ok(response);
            }
            catch(Exception ex){
                    var errorResponse = new ApiResponse<ClientDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // PATCH: api/clients/{id}
        [Authorize(Roles = "user,admin")]
        [HttpPatch("{id}")]
        public async Task<ActionResult<ApiResponse<ClientDTO>>> UpdateClientPartial(Guid id, [FromBody] UpdateClientDTO updateClientDTO)
        {
            try
            {
                var userId = GetCurrentUserId();

                var existingClient = await _clientService.GetClientAsync(id);
                if (existingClient == null || existingClient.CreatedBy != userId)
                {
                    var notFoundResponse = new ApiResponse<ProductDTO>(
                         success: false,
                         message: "Client not found or access denied.",
                         data: null
                    );
                    return BadRequest(notFoundResponse);
                }
                // Mettre à jour partiellement le client
                await _clientService.UpdateClientPartialAsync(id, updateClientDTO);

                // Récupérer le client mis à jour
                var updatedClient = await _clientService.GetClientAsync(id);
                if (updatedClient == null)
                {
                    var notFoundResponse = new ApiResponse<ClientDTO>(
                        success: false,
                        message: "Client not found after update.",
                        data: null
                    );
                    return NotFound(notFoundResponse);
                }

                // Mapper le client mis à jour en DTO
                var clientDTO = _clientService.MapToClientDTO(updatedClient);

                // Retourner la réponse
                var response = new ApiResponse<ClientDTO>(
                    success: true,
                    message: "Client updated partially successfully.",
                    data: clientDTO
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<ClientDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // DELETE: api/clients/{id}
        [Authorize(Roles = "user,admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var existingClient = await _clientService.GetClientAsync(id);
                if (existingClient == null || existingClient.CreatedBy != userId)
                {
                    var notFoundResponse = new ApiResponse<ProductDTO>(
                         success: false,
                         message: "Client not found or access denied.",
                         data: null
                    );
                    return BadRequest(notFoundResponse);
                }

                await _clientService.DeleteClientAsync(id);

                var response = new ApiResponse<string>(
                    success: true,
                    message: "Client deleted successfully.",
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