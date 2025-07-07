using KzBarry.Utils.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKzBarryServices(builder.Configuration);
builder.Services.AddKzBarrySwagger();
builder.Services.AddKzBarryAuthentication(builder.Configuration);

var app = builder.Build();

app.UseKzBarrySwagger();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
