using Chat.Server.Controllers;
using Chat.Server.Data;

using Chat.Server.GraphQL;
using Chat.Server.Services;
using Chat.Server.Message;
using Chat.Server.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;

using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Read Azure AD B2C API settings from environment variables
builder.Configuration.AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();

builder.Services.AddTransient<ExceptionFilter>();
builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>();

builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGraphQLServer()
    .AddAuthorization()
    .RegisterDbContext<ApplicationDbContext>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddType<UserType>()
    .AddTypeExtension<SpaceMessageAttachmentExtension>()
    .AddTypeExtension<DirectMessageAttachmentExtension>()
    .AddQueryType<Query>()
    .AddMutationConventions()
    .AddMutationType<Mutation>()
    .AddErrorFilter<GraphQLErrorFilter>();

builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
       new[] { "application/octet-stream" });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    Func<MessageReceivedContext, Task> existingOnMessageReceivedHandler = options.Events.OnMessageReceived;
    options.Events.OnMessageReceived = async context =>
    {
        await existingOnMessageReceivedHandler(context);
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) &&
            (path.StartsWithSegments("/signalr")))
        {
            context.Token = accessToken;
        }
    };

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseResponseCompression();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.MapGraphQL().RequireAuthorization();
app.MapHub<ChatHub>("/signalr");

app.Run();