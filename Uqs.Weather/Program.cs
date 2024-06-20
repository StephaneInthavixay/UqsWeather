using AdamTibi.OpenWeather;
using Uqs.Weather.Wrappers;

var builder = WebApplication.CreateBuilder(args);

/*
  Durée de vie des services : Singleton / Transient / Scoped
  
  Singleton : Un service singleton est créé une seule fois et partagé tout au long du cycle de vie de l'application. 
  Cela signfie qu'une seule instance du service est utilisée par tous les composants qui en ont besoin (ex : paramètres de configuration ou services de journalisation).
  
  Scoped : Un service scoped est crée une fois par requête (ou portée).
  Cela signifie que chaque requête client obtient une nouvelle instance du service, mais que, dans la même requête, tous les composants partagent la même instance.
  Cela garantit que les instances ne sont pas partagées entre les requêtes, ce qui peut aider à maintenir l'intégrité des données.
  Cela peut également aider à gérer efficacement les ressources en les libérant après chaque requête.
  
  Transient : Un service transient est créé chaque fois qu'il est demandé.
  Chaque composant qui dépend du service obtient une nouvelle instance.
    */

builder.Services.AddSingleton<IClient>(_ =>
{
    string apiKey = builder.Configuration["OpenWeather:Key"];
    HttpClient httpClient = new HttpClient();
    return new Client(apiKey, httpClient);
});
builder.Services.AddSingleton<INowWrapper>(_ => new NowWrapper());
builder.Services.AddTransient<IRandowWrapper>(_ => new RandomWrapper());

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();