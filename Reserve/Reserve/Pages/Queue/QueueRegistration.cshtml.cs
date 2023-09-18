using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reserve.Core.Features.Queue;
using System.ComponentModel.DataAnnotations;

namespace Reserve.Pages.Queue;

[BindProperties]
public class QueueRegistrationModel : PageModel
{
    private readonly IQueueRepository _queueRepository;
    private readonly IValidator<QueueTicket> _validator;
    [Required]
    public QueueTicket NewQueueTicket { get; set; }
    public Guid QueueEventId { get; set; }
    public QueueEventView QueueEvent { get; set; }
    public int NextQueueNumber { get; set; }

    public QueueRegistrationModel(IQueueRepository queueRepository, IValidator<QueueTicket> validator)
    {
        _queueRepository = queueRepository;
        _validator = validator;
        NewQueueTicket = new QueueTicket();
        QueueEvent = new QueueEventView();
    }

    public async Task OnGet(Guid id)
    {
        QueueEventId = id;
        QueueEvent CurrentQueue = await _queueRepository.GetQueueEventByID(id.ToString());
        QueueEvent = new QueueEventView()
        {
            Id = CurrentQueue.Id,
            Title = CurrentQueue.Title,
            OrganizerEmail = CurrentQueue.OrganizerEmail,
            Description = CurrentQueue.Description,
            CurrentNumberServed = CurrentQueue.CurrentNumberServed,
            TicketCounter = CurrentQueue.TicketCounter,
            LastReset = CurrentQueue.LastReset
        };
        NextQueueNumber = await _queueRepository.GetNextQueueNumber(id.ToString());
    }

    public async Task<IActionResult> OnPost()
    {
        NewQueueTicket.QueueEvent = await _queueRepository.GetQueueEventByID(QueueEventId.ToString());
        var validationResult = await _validator.ValidateAsync(NewQueueTicket);

        if (!validationResult.IsValid)
        {
            return Page();
        }
        NewQueueTicket.QueueNumber = await _queueRepository.GetNextQueueNumber(QueueEventId.ToString());
        await _queueRepository.IncrementTicketCounter(QueueEventId.ToString());
        NewQueueTicket = await _queueRepository.RegisterCustomer(NewQueueTicket);
        return RedirectToPage("QueueTicket", new { QueueTicketId = NewQueueTicket.Id, QueueId = QueueEventId });
    }

}
