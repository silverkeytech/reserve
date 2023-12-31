﻿using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Reserve.Core.Features.Event;
using Reserve.Helpers;
namespace Reserve.Endpoints;

public static class EventEndpoints
{
    public static RouteGroupBuilder MapEventsApi(this RouteGroupBuilder group)
    {
        group.MapPost("close-reservation/{id}", async ([FromRoute] string id, HttpContext context, IEventRepository _eventRepository, IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
                await _eventRepository.CloseReservationAsync(GuidShortener.RestoreGuid(id).ToString());
                var html = @"<p class=""notification-text m-3"">Reservation closed!</p>";
                return Results.Content(html, "text/html");
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        });
        group.MapDelete("cancel-reservation/{id}", async([FromRoute] string id, HttpContext context, IEventRepository _eventRepository, IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
                CasualTicket? casualTicket = await _eventRepository.GetTicketByIdAsync(GuidShortener.RestoreGuid(id).ToString());
                ArgumentNullException.ThrowIfNull(casualTicket);
                ArgumentNullException.ThrowIfNull(casualTicket.CasualEvent);
                Guid guidTicketId = GuidShortener.RestoreGuid(id);
                await _eventRepository.CancelReservationAsync(guidTicketId, casualTicket.CasualEvent.Id);
                context.Response.Headers["HX-Redirect"] = "/event-cancellation";
                return Results.Ok();
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        });
        return group;
    }
}
