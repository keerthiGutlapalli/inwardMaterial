using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using ERP_API.Data;
using ERP_API.Moduls;

namespace ERP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PmInwardMaterialsController : ControllerBase
    {
        private readonly Lg202324Context _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PmInwardMaterialsController> _logger;

        public PmInwardMaterialsController(Lg202324Context context, IMapper mapper, ILogger<PmInwardMaterialsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/PmInwardMaterials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PmInwardMaterialReadOnlyDto>>> GetPmInwardMaterials()
        {
            try
            {
                if (_context.PmInwardMaterials == null)
                {
                    _logger.LogWarning("Attempted to retrieve PM inward materials, but the entity set was null.");
                    return NotFound();
                }

                var materials = await _context.PmInwardMaterials
                    .Include(m => m.PmInwardMaterialSubs) // Include child entities
                    .OrderByDescending(m => m.InMatId)
                    .ToListAsync();

                var materialsDto = _mapper.Map<IEnumerable<PmInwardMaterialReadOnlyDto>>(materials);

                _logger.LogInformation("Retrieved all PM inward materials with their subs.");
                return Ok(materialsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving PM inward materials.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // GET: api/PmInwardMaterials/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PmInwardMaterialReadOnlyDto>> GetPmInwardMaterial(int id)
        {
            try
            {
                var material = await _context.PmInwardMaterials
                    .Include(m => m.PmInwardMaterialSubs) // Include child entities
                    .FirstOrDefaultAsync(m => m.InMatId == id);

                if (material == null)
                {
                    _logger.LogWarning($"PM inward material with ID {id} was not found.");
                    return NotFound();
                }

                var materialDto = _mapper.Map<PmInwardMaterialReadOnlyDto>(material);

                _logger.LogInformation($"Retrieved PM inward material with ID {id} and its subs.");
                return Ok(materialDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving PM inward material with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // PUT: api/PmInwardMaterials/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPmInwardMaterial(int id, PmInwardMaterialCreateDto materialDto)
        {
            if (id != materialDto.InMatId)
            {
                _logger.LogWarning("PUT request failed due to mismatched IDs.");
                return BadRequest();
            }

            try
            {
                // Load the existing entity, including child entities
                var existingMaterial = await _context.PmInwardMaterials
                    .Include(m => m.PmInwardMaterialSubs)
                    .FirstOrDefaultAsync(m => m.InMatId == id);

                if (existingMaterial == null)
                {
                    _logger.LogWarning($"Raw inward material with ID {id} does not exist.");
                    return NotFound();
                }

                // Map the incoming data to the existing entity
                _mapper.Map(materialDto, existingMaterial);

                // Handle child entities (e.g., update or remove existing, add new)
                if (materialDto.PmInwardMaterialSubs != null)
                {
                    // Remove subs that are not in the incoming DTO
                    var subsToRemove = existingMaterial.PmInwardMaterialSubs
                        .Where(sub => !materialDto.PmInwardMaterialSubs.Any(dtoSub => dtoSub.InMatSubId == sub.InMatSubId))
                        .ToList();
                    _context.PmInwardMaterialSubs.RemoveRange(subsToRemove);

                    // Update or add subs
                    foreach (var dtoSub in materialDto.PmInwardMaterialSubs)
                    {
                        var existingSub = existingMaterial.PmInwardMaterialSubs
                            .FirstOrDefault(sub => sub.InMatSubId == dtoSub.InMatSubId);

                        if (existingSub != null)
                        {
                            // Update existing sub
                            _mapper.Map(dtoSub, existingSub);
                        }
                        else
                        {
                            // Add new sub
                            var newSub = _mapper.Map<PmInwardMaterialSub>(dtoSub);
                            existingMaterial.PmInwardMaterialSubs.Add(newSub);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated raw inward material with ID {id}.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!PmInwardMaterialExists(id))
                {
                    _logger.LogWarning($"Raw inward material with ID {id} does not exist.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, $"Concurrency error occurred while updating raw inward material with ID {id}.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating raw inward material with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // POST: api/PmInwardMaterials
        [HttpPost]
        public async Task<ActionResult<PmInwardMaterialReadOnlyDto>> PostPmInwardMaterial(PmInwardMaterialCreateDto materialDto)
        {
            if (_context.PmInwardMaterials == null)
            {
                _logger.LogWarning("Attempted to create a PM inward material but the entity set was null.");
                return Problem("Entity set 'Lg202324Context.PmInwardMaterials' is null.");
            }

            try
            {
                // Map the incoming DTO to the entity
                var material = _mapper.Map<PmInwardMaterial>(materialDto);

                // Add the material to the context and save changes
                _context.PmInwardMaterials.Add(material);
                await _context.SaveChangesAsync();

                // Retrieve the created material along with its child entities
                var createdMaterial = await _context.PmInwardMaterials
                    .Include(m => m.PmInwardMaterialSubs) // Including related child entities
                    .FirstOrDefaultAsync(m => m.InMatId == material.InMatId);

                if (createdMaterial == null)
                {
                    _logger.LogWarning("Failed to retrieve the created PM inward material.");
                    return NotFound("The newly created PM inward material could not be found.");
                }

                // Map the created material entity back to a read-only DTO for returning in the response
                var createdMaterialDto = _mapper.Map<PmInwardMaterialReadOnlyDto>(createdMaterial);

                _logger.LogInformation($"Created a new PM inward material with ID {material.InMatId}.");

                // Return the created material along with a URI to access it via GET
                return CreatedAtAction(nameof(GetPmInwardMaterial), new { id = material.InMatId }, createdMaterialDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a PM inward material.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // DELETE: api/PmInwardMaterials/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePmInwardMaterial(int id)
        {
            try
            {
                var material = await _context.PmInwardMaterials
                    .Include(m => m.PmInwardMaterialSubs) // Include child entities
                    .FirstOrDefaultAsync(m => m.InMatId == id);

                if (material == null)
                {
                    _logger.LogWarning($"PM inward material with ID {id} was not found.");
                    return NotFound();
                }

                _context.PmInwardMaterials.Remove(material);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted PM inward material with ID {id}.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting PM inward material with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        private bool PmInwardMaterialExists(int id)
        {
            return (_context.PmInwardMaterials?.Any(m => m.InMatId == id)).GetValueOrDefault();
        }
    }
}
