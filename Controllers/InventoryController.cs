using inventory_service.Data;
using inventory_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryContext _context;
        private readonly ILogger<InventoryController> _logger;
 
        public InventoryController(InventoryContext context, ILogger<InventoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.InventoryItems.ToListAsync();
            return Ok(items);
        }

        [HttpGet("{artikleNum}")]
        public async Task<IActionResult> GetBySku(string artikleNum)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ArticleNumber == artikleNum);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("{artikleNum}")]
        public async Task<IActionResult> Add(string artikleNum, [FromBody] InventoryItem newItem)
        {
            newItem.ArticleNumber = artikleNum;
            
            _logger.LogWarning("🔴 POST /api/inventory aufgerufen - ArticleNumber: {ArticleNumber}, RequestId: {RequestId}", 
                newItem.ArticleNumber, HttpContext.TraceIdentifier);
            
            _context.InventoryItems.Add(newItem);
            await _context.SaveChangesAsync();
   
            _logger.LogInformation("✅ Item gespeichert - ID: {Id}, ArticleNumber: {ArticleNumber}", 
                newItem.Id, newItem.ArticleNumber);
        
            return CreatedAtAction(nameof(GetBySku), new { artikleNum = newItem.ArticleNumber }, newItem);
        }

        [HttpPut("{sku}")]
        public async Task<IActionResult> UpdateQuantity(string sku, [FromBody] int quantity)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ArticleNumber == sku);
            if (item == null) return NotFound();
            item.Quantity = quantity;
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpDelete("{sku}")]
        public async Task<IActionResult> Delete(string sku)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ArticleNumber == sku);
            if (item == null) return NotFound();
            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
