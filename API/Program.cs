using FluentValidation;
using FluentValidation.AspNetCore;
using IceBreakerApp.API.Middleware;
using Microsoft.OpenApi.Models;
using IceBreakerApp.Application.Services;
using IceBreakerApp.Infrastructure.Repositories;
using IceBreakerApp.Infrastructure.Configuration;
using IceBreakerApp.Domain.IRepositories;
using IceBreakerApp.Domain.Interfaces;
using IceBreakerApp.Application.Interfaces;
using IceBreakerApp.Application.IServices;
using IceBreakerApp.Domain.Interfaces.IServices;


var builder = WebApplication.CreateBuilder(args);

// Add builder.Services to the container
builder.Services.AddControllers().AddNewtonsoftJson(); // Для поддержки JsonPatchDocument
builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ice Breaker API",
        Version = "v1",
        Description = "API для системы вопросов и ответов",
        Contact = new OpenApiContact
        {
            Name = "Ice Breaker App",
            Email = "support@icebreak.com"
        }
    });
    c.EnableAnnotations();
// XML Comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// CORS конфигурация
builder.Services.AddCors(options => options.AddPolicy("AllowAll", 
    policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
    
builder.Services.AddFluentValidationAutoValidation();

// Регистрация всех валидаторов
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateQuestionValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateQuestionAnswerValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTopicValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateQuestionValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateTopicValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserValidator>();

// Настройка конфигурации хранилища
builder.Services.Configure<StorageSettings>(options =>
{
    options.StoragePath = Path.Combine(builder.Environment.ContentRootPath, "Storage");
    options.WriteIndented = true;
    options.PropertyNamingPolicy = "CamelCase";
});

// Настройка AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Регистрация репозиториев
builder.Services.AddScoped<IUserRepository,UserRepository>();

builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

builder.Services.AddScoped<ITopicRepository,TopicRepository>();

builder.Services.AddScoped<IQuestionAnswerRepository,QuestionAnswerRepository>();

builder.Services.AddScoped<IQuestionLikeRepository,QuestionLikeRepository>();

// Регистрация сервисов
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IQuestionAnswerService, QuestionAnswerService>();
builder.Services.AddScoped<IQuestionLikeService, QuestionLikeService>();

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ice Breaker API v1");
        c.RoutePrefix = string.Empty; // Swagger UI на корневом URL
    });
}
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
// Health Check Endpoint
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
})).WithTags("Health");
app.Run();