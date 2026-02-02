using Events.DATA.DTOs;
using Events.DATA.DTOs.Book;
using Events.DATA.DTOs.Payment;
using Events.Entities;
using Events.Entities.Book;
using Events.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class BookController : BaseController
{
    
    private readonly IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }
    
    
    [Authorize]
    [HttpPost("{eventId}")]  
    public async Task<IActionResult> CreateBook(Guid eventId , [FromBody] ObjectForm bookObjects) => Ok(await _bookService.CreateBook(eventId , Id , bookObjects));
    
    
    // GetBooksAsync
    [HttpGet]
    public async Task<IActionResult> GetBooksAsync([FromQuery]BookFilter filter) => Ok(await _bookService.GetBooksAsync(Role, Id, filter) , filter.PageNumber);
    
    // book by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookAsync(Guid id) => Ok(await _bookService.GetBookAsync(id));
    
    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] PayBillRequest billResponse) => Ok(await _bookService.Pay(billResponse));
  
    
}