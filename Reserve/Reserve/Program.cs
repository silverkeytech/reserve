using EdgeDB;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Reserve.Core.Features.Queue;
using Reserve.Core.Features.Event;
using Reserve.Core.Features.MailService;
using Reserve.Endpoints;
using Reserve.Helpers;
using Reserve.Core.Features.Appointment;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
               _ => "This field is required.");
});
var connectionString = builder.Configuration.GetConnectionString("EdgeDB");
var connection = EdgeDBConnection.Parse(null, connectionString);
builder.Services.AddEdgeDB(connection, config =>
{
    config.SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy;
});
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IValidator<QueueEvent>, QueueEventValidator>();
builder.Services.AddScoped<IValidator<QueueTicket>, QueueTicketValidator>();
builder.Services.AddScoped<IValidator<CasualEventInput>, CasualEventInputValidator>();
builder.Services.AddScoped<IValidator<CasualTicketInput>, CasualTicketInputValidator>();
builder.Services.AddScoped<IValidator<AppointmentDetails>, AppointmentDetailsValidator>();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAntiforgery();
app.MapRazorPages();

app.MapGroup("/").MapEventsApi();
app.MapGroup("/").MapAppointmentsApi();
app.Run();
