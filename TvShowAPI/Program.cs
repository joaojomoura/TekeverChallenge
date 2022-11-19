using FluentValidation;
using TvShowAPI.Data;
using TvShowAPI.Endpoints.Internal;

var builder = WebApplication.CreateBuilder(args);

//Service registration starts here

//Deal with CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AnyOrigin", x => x.AllowAnyOrigin());
});

// builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
//     .AddScheme<ApiKeyAuthSchemeOptions, ApiKeuyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
// builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x => {
    x.EnableAnnotations();
});
builder.Services.AddSingleton<IDbConnectionFactory>(_ => 
    new SqliteConnectionFactory(
        builder.Configuration.GetValue<string>("Database:ConnectionString")));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddEnpoints<Program>(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

//Service registration stops here

var app = builder.Build();

//Middleware registration starts here

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
// app.UseAuthorization();
app.UseEndpoints<Program>();

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

//Middleware registration stops here

app.Run();
