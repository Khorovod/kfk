using Consumer.Infrastructure;
using Consumer.Services;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions();
builder.Services.Configure<KafkaConfiguration>(builder.Configuration.GetSection(nameof(KafkaConfiguration)));
builder.Services.AddHostedService<ConsumerService>();

var app = builder.Build();

app.UseRouting();
app.Run();