using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}
app.UseStatusCodePages();

app.MapControllers();

app.Run();
