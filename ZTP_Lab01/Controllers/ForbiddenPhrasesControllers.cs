using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZTP_Lab01.Domains.Entities;
using ZTP_Lab01.Infarstructure.Repositories;

namespace ZTP_Lab01.Controllers;

public record CreateForbiddenPhraseDto(string Phrase);

[ApiController]
[Route("api/v1/forbidden-phrases")]
public class ForbiddenPhrasesController : ControllerBase
{
    private readonly IForbiddenPhraseRepository _repo;

    public ForbiddenPhrasesController(IForbiddenPhraseRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<List<ForbiddenPhrase>>> GetAll()
    {
        return Ok(await _repo.GetAllAsync());
    }

    [HttpPost]
    public async Task<ActionResult<ForbiddenPhrase>> Create([FromBody] CreateForbiddenPhraseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Phrase))
        {
            return BadRequest(new { errors = new { Phrase = new[] { "Phrase cannot be empty." } } });
        }

        var created = await _repo.AddAsync(new ForbiddenPhrase { Phrase = dto.Phrase.Trim() });
        return Created($"/api/v1/forbidden-phrases/{created.Id}", created);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var phrase = await _repo.GetByIdAsync(id);
        if (phrase is null) return NotFound();

        await _repo.DeleteAsync(phrase);
        return NoContent();
    }
}