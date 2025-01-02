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
using static MudBlazor.Icons;

namespace ERP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RawInwardMaterialsController : ControllerBase
    {
        private readonly Lg202324Context _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RawInwardMaterialsController> _logger;

        public RawInwardMaterialsController(Lg202324Context context, IMapper mapper, ILogger<RawInwardMaterialsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/RawInwardMaterials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RawInwardMaterialReadOnlyDto>>> GetRawInwardMaterials()
        {
            try
            {
                if (_context.RawInwardMaterials == null)
                {
                    _logger.LogWarning("Attempted to retrieve raw inward materials, but the entity set was null.");
                    return NotFound();
                }

                var materials = await _context.RawInwardMaterials
                    .Include(m => m.RawInwardMaterialSubs)
                     .Include(m => m.Storeadds)
                     .ThenInclude(m => m.Storeaddsubs)
                    .OrderByDescending(m => m.InMatId)
                    .ToListAsync();

                var materialsDto = _mapper.Map<IEnumerable<RawInwardMaterialReadOnlyDto>>(materials);

                _logger.LogInformation("Retrieved all raw inward materials with their subs.");
                return Ok(materialsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving raw inward materials.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // GET: api/RawInwardMaterials/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RawInwardMaterialReadOnlyDto>> GetRawInwardMaterial(int id)
        {
            try
            {
                var material = await _context.RawInwardMaterials
                    .Include(m => m.RawInwardMaterialSubs)
                    .Include(m => m.Storeadds)
                    .ThenInclude(m => m.Storeaddsubs)// Include child entities
                    .FirstOrDefaultAsync(m => m.InMatId == id);

                if (material == null)
                {
                    _logger.LogWarning($"Raw inward material with ID {id} was not found.");
                    return NotFound();
                }

                var materialDto = _mapper.Map<RawInwardMaterialReadOnlyDto>(material);

                _logger.LogInformation($"Retrieved raw inward material with ID {id} and its subs.");
                return Ok(materialDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving raw inward material with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutRawInwardMaterial(int id, RawInwardMaterialCreateOnlyDto materialDto)
        {
            if (id != materialDto.InMatId)
            {
                return BadRequest();
            }

            try
            {
                _context.ChangeTracker.Clear();

                var existingMaterial = await _context.RawInwardMaterials
                    .Include(m => m.RawInwardMaterialSubs)
                    .Include(m => m.Storeadds)
                        .ThenInclude(m => m.Storeaddsubs)
                    .FirstOrDefaultAsync(m => m.InMatId == id);

                if (existingMaterial == null)
                {
                    return NotFound();
                }

                // Update main entity
                _mapper.Map(materialDto, existingMaterial);

                // Handle RawInwardMaterialSubs
                if (materialDto.RawInwardMaterialSubs != null)
                {
                    var currentSubIds = existingMaterial.RawInwardMaterialSubs
                        .Where(x => x.InMatSubId > 0)
                        .Select(x => x.InMatSubId)
                        .ToList();
                    var updatedSubIds = materialDto.RawInwardMaterialSubs
                        .Where(x => x.InMatSubId.HasValue)
                        .Select(x => x.InMatSubId.Value)
                        .ToList();

                    // Remove subs that are not in the incoming DTO
                    var subsToRemove = existingMaterial.RawInwardMaterialSubs
                        .Where(x => !updatedSubIds.Contains(x.InMatSubId))
                        .ToList();

                    foreach (var sub in subsToRemove)
                    {
                        existingMaterial.RawInwardMaterialSubs.Remove(sub);
                        _context.RawInwardMaterialSubs.Remove(sub);
                    }

                    foreach (var subDto in materialDto.RawInwardMaterialSubs)
                    {
                        if (subDto.InMatSubId.HasValue)
                        {
                            var existingSub = existingMaterial.RawInwardMaterialSubs
                                .FirstOrDefault(x => x.InMatSubId == subDto.InMatSubId.Value);
                            if (existingSub != null)
                            {
                                _mapper.Map(subDto, existingSub);
                            }
                        }
                        else
                        {
                            var newSub = _mapper.Map<RawInwardMaterialSub>(subDto);
                            newSub.InMatId = id;
                            existingMaterial.RawInwardMaterialSubs.Add(newSub);
                        }
                    }
                }

                // Handle Storeadds and their subs
                if (materialDto.Storeadds != null)
                {
                    var updatedStoreAddIds = materialDto.Storeadds
                        .Where(x => x.StoreAddId.HasValue)
                        .Select(x => x.StoreAddId.Value)
                        .ToList();

                    // Remove store adds that are not in the incoming DTO
                    var storeAddsToRemove = existingMaterial.Storeadds
                        .Where(x => !updatedStoreAddIds.Contains(x.StoreAddId.Value))
                        .ToList();

                    foreach (var storeAdd in storeAddsToRemove)
                    {
                        existingMaterial.Storeadds.Remove(storeAdd);
                        _context.Storeadds.Remove(storeAdd);
                    }

                    foreach (var storeAddDto in materialDto.Storeadds)
                    {
                        if (storeAddDto.StoreAddId.HasValue)
                        {
                            var existingStoreAdd = existingMaterial.Storeadds
                                .FirstOrDefault(x => x.StoreAddId == storeAddDto.StoreAddId.Value);

                            if (existingStoreAdd != null)
                            {
                                _mapper.Map(storeAddDto, existingStoreAdd);

                                // Handle Storeaddsubs
                                if (storeAddDto.Storeaddsubs != null)
                                {
                                    var updatedSubIds = storeAddDto.Storeaddsubs
                                        .Where(x => x.storeAddSubId.HasValue)
                                        .Select(x => x.storeAddSubId.Value)
                                        .ToList();

                                    // Remove subs that are not in the incoming DTO
                                    var subsToRemove = existingStoreAdd.Storeaddsubs
                                        .Where(x => x.storeAddSubId.HasValue &&
                                                  !updatedSubIds.Contains(x.storeAddSubId.Value))
                                        .ToList();

                                    foreach (var sub in subsToRemove)
                                    {
                                        existingStoreAdd.Storeaddsubs.Remove(sub);
                                        _context.Storeaddsubs.Remove(sub);
                                    }

                                    foreach (var subDto in storeAddDto.Storeaddsubs)
                                    {
                                        if (subDto.storeAddSubId.HasValue)
                                        {
                                            var existingSub = existingStoreAdd.Storeaddsubs
                                                .FirstOrDefault(x => x.storeAddSubId == subDto.storeAddSubId.Value);
                                            if (existingSub != null)
                                            {
                                                _mapper.Map(subDto, existingSub);
                                            }
                                        }
                                        else
                                        {
                                            var newSub = _mapper.Map<Storeaddsub>(subDto);
                                            newSub.StoreAddId = existingStoreAdd.StoreAddId.Value;
                                            existingStoreAdd.Storeaddsubs.Add(newSub);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var newStoreAdd = _mapper.Map<Storeadd>(storeAddDto);
                            newStoreAdd.InMatId = id;
                            existingMaterial.Storeadds.Add(newStoreAdd);

                            if (storeAddDto.Storeaddsubs != null)
                            {
                                foreach (var subDto in storeAddDto.Storeaddsubs)
                                {
                                    var newSub = _mapper.Map<Storeaddsub>(subDto);
                                    newStoreAdd.Storeaddsubs.Add(newSub);
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating raw inward material with ID {id}");
                return StatusCode(500, "An error occurred while updating the record.");
            }
        }
        // POST: api/RawInwardMaterials
        [HttpPost]
        public async Task<ActionResult<RawInwardMaterialReadOnlyDto>> PostRawInwardMaterial(RawInwardMaterialCreateOnlyDto materialDto)
        {
            if (_context.RawInwardMaterials == null)
            {
                _logger.LogWarning("Attempted to create a raw inward material but the entity set was null.");
                return Problem("Entity set 'Lg202324Context.RawInwardMaterials' is null.");
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Map the incoming DTO to the entity
                    var material = _mapper.Map<RawInwardMaterial>(materialDto);

                    // Add the material to the context
                    _context.RawInwardMaterials.Add(material);
                    await _context.SaveChangesAsync();

                    // Set Source and InMatId for Storeadds
                    if (material.Storeadds != null)
                    {
                        foreach (var storeAdd in material.Storeadds)
                        {
                            storeAdd.Source = 1; // Set source as 1 for RawInwardMaterial
                            storeAdd.InMatId = material.InMatId; // Set the foreign key

                            // Handle Storeaddsubs if they exist
                            if (storeAdd.Storeaddsubs != null)
                            {
                                foreach (var sub in storeAdd.Storeaddsubs)
                                {
                                    sub.StoreAddId = storeAdd.StoreAddId;
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Retrieve the created material along with its child entities
                    var createdMaterial = await _context.RawInwardMaterials
                        .Include(m => m.RawInwardMaterialSubs)
                        .Include(m => m.Storeadds)
                            .ThenInclude(m => m.Storeaddsubs)
                        .FirstOrDefaultAsync(m => m.InMatId == material.InMatId);

                    if (createdMaterial == null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("Failed to retrieve the created raw inward material.");
                        return NotFound("The newly created raw inward material could not be found.");
                    }

                    // Map the created material entity back to a read-only DTO
                    var createdMaterialDto = _mapper.Map<RawInwardMaterialReadOnlyDto>(createdMaterial);

                    await transaction.CommitAsync();
                    _logger.LogInformation($"Created a new raw inward material with ID {material.InMatId}.");

                    return CreatedAtAction(nameof(GetRawInwardMaterial),
                        new { id = material.InMatId },
                        createdMaterialDto);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a raw inward material.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }
        // DELETE: api/RawInwardMaterials/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRawInwardMaterial(int id)
        {
            try
            {
                var material = await _context.RawInwardMaterials
                    .Include(m => m.RawInwardMaterialSubs)
                     .Include(m => m.Storeadds)
                     .ThenInclude(m => m.Storeaddsubs)// Include child entities
                    .FirstOrDefaultAsync(m => m.InMatId == id);

                if (material == null)
                {
                    _logger.LogWarning($"Raw inward material with ID {id} was not found.");
                    return NotFound();
                }
                // Remove CustomerLocationDepartments
                foreach (var storeAddSub in material.Storeadds)
                {
                    _context.Storeaddsubs.RemoveRange(storeAddSub.Storeaddsubs);
                }
                _context.Storeadds.RemoveRange(material.Storeadds);

                _context.RawInwardMaterials.Remove(material);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted raw inward material with ID {id}.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting raw inward material with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        private bool RawInwardMaterialExists(int id)
        {
            return (_context.RawInwardMaterials?.Any(m => m.InMatId == id)).GetValueOrDefault();
        }



    }
}