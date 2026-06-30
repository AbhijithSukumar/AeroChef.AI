using AeroChef.AI.Agents; // Ensure your Factory is here
using AeroChef.AI.Agents.Agents;
using Microsoft.Extensions.AI;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.

// Add services to the container
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AeroChef-API"))
            .AddAspNetCoreInstrumentation() // Auto-instrument HTTP requests
            .AddHttpClientInstrumentation() // Auto-instrument outgoing calls (to Gemini)
            .AddSource("AeroChef.AI.Agents") // This connects your Agent library spans
            .AddOtlpExporter(options =>
            {
                // LangSmith OTLP Endpoint
                options.Endpoint = new Uri("https://api.smith.langchain.com/otel/v1/traces");
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
                // Set required authentication headers
                options.Headers = "x-api-key=" + builder.Configuration["LangSmith:ApiKey"] +
                                  ",AerochefAgent=" + builder.Configuration["LANGSMITH_PROJECT:ProjectID"];
            });
    });

// 2. Register the IChatClient using the bridge library directly
// This replaces the manual IGoogleGenAI registration
builder.Services.AddScoped<IChatClient>(sp =>
{
    var apiKey = builder.Configuration["Gemini:ApiKey"] ?? throw new Exception("Gemini key not found");
    return new Google.GenAI.Client(apiKey: apiKey).AsIChatClient("gemini-3.1-flash-lite");
});

builder.Services.AddScoped<AgentFactory>();
builder.Services.AddScoped<ChefBot>();
builder.Services.AddScoped<SafetyBot>();
builder.Services.AddScoped<OpsLead>();



// 3. Register the Factory
builder.Services.AddScoped<AgentFactory>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

var app = builder.Build();

app.UseCors("AllowAll");

// Enable Swagger UI middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AeroChef API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
