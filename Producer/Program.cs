using Producer.Infrastructure;
using Producer.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions();
builder.Services.Configure<KafkaConfiguration>(builder.Configuration.GetSection(nameof(KafkaConfiguration)));
builder.Services.AddHostedService<ProducerService>();

var app = builder.Build();

app.UseRouting();
app.Run();