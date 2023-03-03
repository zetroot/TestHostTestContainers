using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestHostTestContainers.Database;

namespace TestHostTestContainers.Controllers;

[ApiController, Route("[controller]")]
public class DataController : ControllerBase
{
    private readonly DataContext _context;

    public DataController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Get([FromRoute]Guid id)
    {
        var data = await _context.Set<UserData>().Where(x => x.Id == id).Select(x => x.Data).FirstOrDefaultAsync();
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody]string data)
    {
        var item = new UserData { Id = Guid.NewGuid(), Data = data };
        _context.Set<UserData>().Add(item);
        await _context.SaveChangesAsync();
        return Ok(item.Id);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult> Update([FromRoute]Guid id, [FromBody]string data)
    {
        var item = await _context.Set<UserData>().FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
            return NotFound();
        item.Data = data;
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute]Guid id)
    {
        var item = await _context.Set<UserData>().FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
            return NotFound();
        _context.Set<UserData>().Remove(item);
        await _context.SaveChangesAsync();
        return Ok();
    }

}
