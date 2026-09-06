using System.Text.Json.Serialization;
using TicTacToe.Api.Services;

// Create WebApplication with builde
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TicTacToe API V1");
        options.RoutePrefix = "docs"; // Swagger UI available at /docs
        options.DocumentTitle = "TicTacToe API Documentation";
        options.DisplayRequestDuration(); // Show request duration
    });
}

app.UseCors("Frontend");
app.MapControllers();
app.Run();
