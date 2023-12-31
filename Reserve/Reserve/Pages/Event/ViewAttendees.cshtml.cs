using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reserve.Core.Features.Event;
using Reserve.Helpers;
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
        string restoredId = GuidShortener.RestoreGuid(Id).ToString();
        CasualEvent? eventDetails = await _eventRepository.GetByIdAsync(restoredId);
        List<CasualTicket> attendees = await _eventRepository.GetAttendeesAsync(restoredId);
        if(eventDetails is null)
        {
            return RedirectToPage("EventError");
        }
        Event = new CasualEventView
        {
            Id = eventDetails.Id,
            Title = eventDetails.Title,
            OrganizerName = eventDetails.OrganizerName,
            OrganizerEmail = eventDetails.OrganizerEmail,
            Location = eventDetails.Location,
            Description = eventDetails.Description,
            StartDate = eventDetails.StartDate,
            EndDate = eventDetails.EndDate,
            ImageUrl = eventDetails.ImageUrl,
            Opened = eventDetails.Opened,
            MaximumCapacity = eventDetails.MaximumCapacity,
            CurrentCapacity = eventDetails.CurrentCapacity,
        };
        foreach (var attendee in attendees)
        {
            Attendees.Add(new CasualTicketView
            {
                Id = attendee.Id,
                ReserverName = attendee.ReserverName,
                ReserverEmail = attendee.ReserverEmail,
                ReserverPhoneNumber = attendee.ReserverPhoneNumber,
                CasualEvent = attendee.CasualEvent,
            });
        }
        return Page();
    }
}
