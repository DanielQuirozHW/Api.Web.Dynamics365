using Api.Web.Dynamics365;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);

startup.configureServices(builder.Services);

var app = builder.Build();

startup.Configure(app, app.Environment);

app.Run();
