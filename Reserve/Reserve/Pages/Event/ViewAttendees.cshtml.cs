using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reserve.Core.Features.Event;

namespace Reserve.Pages.Event;

public class ViewAttendeesModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Id { get; set; }
    public List<CasualTicketView?> Attendees { get; set; } = new();
    public CasualEventView? Event { get; set; }
    private readonly IEventRepository _eventRepository;

    public ViewAttendeesModel(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }
    public async Task<IActionResult> OnGet()
    {
        Event = await _eventRepository.GetByIdAsync(Id);
        Attendees = await _eventRepository.GetAttendeesAsync(Id);
        if(Event is null)
        {
            return RedirectToPage("EventError");
        }
        return Page();
    }
}
