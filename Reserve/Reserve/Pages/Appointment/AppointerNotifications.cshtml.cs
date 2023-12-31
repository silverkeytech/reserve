using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reserve.Core.Features.Appointment;

namespace Reserve.Pages.Appointment;

public class AppointerNotificationsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }
    private readonly IAppointmentRepository _appointmentRepository;
    public List<AppointerNotifications> AppointerNotifications { get; set; }
    public List<AppointmentReschedule> RescheduleRequests { get; set; } = new();
    public AppointerNotificationsModel(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }
    public async Task<IActionResult> OnGet()
    {
        if (string.IsNullOrEmpty(Id))
        {
            return RedirectToPage("AppointmentError");
        }
        AppointerNotifications = await _appointmentRepository.GetAppointmentNotificationsForCalendarAsync(Id);
        if (AppointerNotifications is null)
        {
            return RedirectToPage("AppointmentError");
        }
        RescheduleRequests = await _appointmentRepository.GetAllRequestsForCalendar(Id);
        return Page();
    }
}
