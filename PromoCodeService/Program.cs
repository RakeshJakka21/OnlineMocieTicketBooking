
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/promos/validate", (string code) =>
{
    var normalized = code.Trim().ToUpperInvariant();
    return normalized switch
    {
        "WELCOME10" => Results.Ok(new { code, percent = 10, valid = true }),
        "FESTIVE20" => Results.Ok(new { code, percent = 20, valid = true }),
        _ => Results.Ok(new { code, percent = 0, valid = false })
    };
});

app.Run();
