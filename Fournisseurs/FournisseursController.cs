using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Myapp.GeneralClass;
using MyApp.GeneralClass;

namespace Myapp.Fournisseurs
{
    [ApiController]
    [Route("api/[controller]")]
    public class FournisseursController : BaseController
    {
        private readonly FournisseurService _fournisseurService;

        public FournisseursController(FournisseurService fournisseurService)
        {
            _fournisseurService = fournisseurService;
        }

        // GET: api/fournisseurs
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<FournisseurDTO>>>> GetFournisseurs()
        {
            try
            {
                var fournisseurs = await _fournisseurService.GetFournisseursAsync();
                var dtos = _fournisseurService.MapToListDTOs(fournisseurs);

                var response = new ApiResponse<List<FournisseurDTO>>(
                    success: true,
                    message: "Fournisseurs retrieved successfully.",
                    data: dtos
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<FournisseurDTO>>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // GET: api/fournisseurs/my-fournisseurs
        [Authorize(Roles = "user,admin,service")]
        [HttpGet("my-fournisseurs")]
        public async Task<ActionResult<ApiResponse<List<FournisseurDTO>>>> GetMyFournisseurs()
        {
            try
            {
                var userId = GetCurrentUserId();
                var fournisseurs = await _fournisseurService.GetMyFournisseursAsync(userId);
                var dtos = _fournisseurService.MapToListDTOs(fournisseurs);

                var response = new ApiResponse<List<FournisseurDTO>>(
                    success: true,
                    message: "Fournisseurs retrieved successfully.",
                    data: dtos
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<FournisseurDTO>>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // GET: api/fournisseurs/{id}
        [Authorize(Roles = "user,admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FournisseurDTO>>> GetFournisseur(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var fournisseur = await _fournisseurService.GetFournisseurByIdAsync(id);
                if (fournisseur == null || fournisseur.CreatedBy != userId)
                {
                    var notFoundResponse = new ApiResponse<FournisseurDTO>(
                        success: false,
                        message: "Fournisseur not found or access denied.",
                        data: null
                    );
                    return BadRequest(notFoundResponse);
                }

                var dto = _fournisseurService.MapToDTO(fournisseur);

                var response = new ApiResponse<FournisseurDTO>(
                    success: true,
                    message: "Fournisseur retrieved successfully.",
                    data: dto
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<FournisseurDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // POST: api/fournisseurs
        [Authorize(Roles = "user,admin,service")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<FournisseurDTO>>> CreateFournisseur([FromBody] FournisseurRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                request.CreatedBy = userId;

                var fournisseur = await _fournisseurService.CreateFournisseurAsync(request);
                var dto = _fournisseurService.MapToDTO(fournisseur);

                var response = new ApiResponse<FournisseurDTO>(
                    success: true,
                    message: "Fournisseur created successfully.",
                    data: dto
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<FournisseurDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // PUT: api/fournisseurs/{id}
        [Authorize(Roles = "user,admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<FournisseurDTO>>> UpdateFournisseur(Guid id, [FromBody] Fournisseur fournisseur)
        {
            try
            {
                if (id != fournisseur.Id)
                {
                    var badRequestResponse = new ApiResponse<FournisseurDTO>(
                        success: false,
                        message: "Fournisseur ID mismatch.",
                        data: null
                    );
                    return BadRequest(badRequestResponse);
                }

                var userId = GetCurrentUserId();

                var existing = await _fournisseurService.GetFournisseurByIdAsync(fournisseur.Id);
                if (existing == null || existing.CreatedBy != userId)
                {
                    var notFoundResponse = new ApiResponse<FournisseurDTO>(
                        success: false,
                        message: "Fournisseur not found or access denied.",
                        data: null
                    );
                    return BadRequest(notFoundResponse);
                }

                await _fournisseurService.UpdateFournisseurAsync(id, fournisseur);

                var dto = _fournisseurService.MapToDTO(fournisseur);

                var response = new ApiResponse<FournisseurDTO>(
                    success: true,
                    message: "Fournisseur updated successfully.",
                    data: dto
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<FournisseurDTO>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        // DELETE: api/fournisseurs/{id}
        [Authorize(Roles = "user,admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteFournisseur(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var fournisseur = await _fournisseurService.GetFournisseurByIdAsync(id);
                if (fournisseur == null || fournisseur.CreatedBy != userId)
                {
                    var notFoundResponse = new ApiResponse<string>(
                        success: false,
                        message: "Fournisseur not found or access denied.",
                        data: null
                    );
                    return NotFound(notFoundResponse);
                }

                await _fournisseurService.DeleteFournisseurAsync(id);

                var response = new ApiResponse<string>(
                    success: true,
                    message: "Fournisseur deleted successfully.",
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
