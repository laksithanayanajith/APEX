using ApexGPT.Bot.Bots;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- BOT FRAMEWORK REGISTRATION START ---

// 1. Create the Bot Adapter (The "Phone Line")
// We use CloudAdapter which is the modern standard
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, CloudAdapter>();

// 2. Create the Bot (The "Person on the Phone")
builder.Services.AddTransient<IBot, EchoBot>();
// Register the Ticket Database (Singleton = One shared database for the whole app)
builder.Services.AddSingleton<ApexGPT.Bot.Services.TicketService>();

// --- BOT FRAMEWORK REGISTRATION END ---

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();