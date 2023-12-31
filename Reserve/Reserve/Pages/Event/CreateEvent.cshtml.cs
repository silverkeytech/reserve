using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reserve.Core.Features.Event;
using Reserve.Core.Features.MailService;
using System;
using System.Globalization;
using static Reserve.Helpers.ImageHelper;
using static Reserve.Core.Features.MailService.MailFormats;
using Reserve.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Reserve.Pages.Event;
[BindProperties]
public class CreateEventModel : PageModel
{
    public CasualEventInput? NewEvent { get; set; }
    private readonly IEventRepository _eventRepository;
    private readonly IValidator<CasualEventInput> _validator;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IEmailService _emailService;
    public CreateEventModel(IEventRepository eventRepository, IWebHostEnvironment webHostEnvironment, IValidator<CasualEventInput> validator, IEmailService emailService)
    {
        _eventRepository = eventRepository;
        _webHostEnvironment = webHostEnvironment;
        _validator = validator;
        _emailService = emailService;
    }
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if(NewEvent is null)
        {
            return RedirectToPage("EventError");
        }
        IFormFile? imageFile = Request.Form.Files.FirstOrDefault();
        ValidationResult result = await _validator.ValidateAsync(NewEvent);
        if(imageFile is not null)
        {
            NewEvent.ImageUrl = @"\images\event\" + imageFile.FileName;
        }
        else
        {
            NewEvent.ImageUrl = @"\images\generic.jpeg";
        }
        if (result.IsValid)
        {
            if (imageFile is not null)
            {
                NewEvent.ImageUrl = SaveImage(imageFile, _webHostEnvironment);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, NewEvent.ImageUrl);
                string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/event"));
                string fullPath = Path.Combine(path, Path.GetFileName(NewEvent.ImageUrl));
                if (imageFile.Length >= 3 * 1024 * 1024)
                {
                    using (Image image = Image.Load(imageFile.OpenReadStream()))
                    {
                        int width = image.Width / 2;
                        int height = image.Height / 2;
                        image.Mutate(x => x.Resize(width, height));
                        image.Save(fullPath);
                    }
                }
            }
            CasualEvent? casualEvent = new CasualEvent
            {
                Title = NewEvent.Title,
                OrganizerName = NewEvent.OrganizerName,
                OrganizerEmail = NewEvent.OrganizerEmail,
                MaximumCapacity = NewEvent.MaximumCapacity,
                CurrentCapacity = NewEvent.CurrentCapacity,
                Location = NewEvent.Location,
                StartDate = NewEvent.StartDate,
                EndDate = NewEvent.EndDate,
                Opened = NewEvent.Opened,
                Description = NewEvent.Description,
                ImageUrl = NewEvent.ImageUrl
            };
            casualEvent = await _eventRepository.CreateAsync(casualEvent);
            if (casualEvent is not null) {
                MailRequest mailRequest = new MailRequest
                {
                    ToEmail = NewEvent.OrganizerEmail,
                    Subject = "Event Created",
                    Body = EventCreationNotification(casualEvent.Id.ToString())
                };
                //await _emailService.SendEmailAsync(mailRequest);
                return RedirectToPage("CreationNotification", new { id = GuidShortener.ShortenGuid(casualEvent.Id) });
            }
            else
            {
                return RedirectToPage("EventError");
            }
        }
        else
        {
            result.AddToModelState(this.ModelState, "NewEvent");
        }
        return Page();
    }
}
