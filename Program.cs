global using MySql.Data.MySqlClient;
using server;

var builder = WebApplication.CreateBuilder(args);

Config config = new("server=127.0.0.1;uid=beertravels;pwd=beertravels;database=beertravels");
builder.Services.AddSingleton<Config>(config);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
app.UseSession();


// Login
app.MapGet("/login", Login.Get);
app.MapPost("/login", Login.Post);
app.MapDelete("/login", Login.Delete);



//Users
app.MapGet("/users", Users.GetAll);
app.MapPost("/users", Users.Post);
app.MapGet("/users/{id}", Users.Get);
app.MapDelete("/users/{id}", Users.Delete);






// Hämtar Pubbar år en specifik stad och hotelet.
app.MapGet("/cities/{city}/pubs", Cities.GetPubs);
app.MapGet("/cities/{city}/hotel", Cities.GetHotel);




// special, reset db
app.MapDelete("/db", Database.db_reset_to_default);
app.Run();


//Pub Crud
app.MapPost("/pubs/create", Pubs.CreatePub);




// Crud endpoints för databasen
//DatabaseEndpoints.Map(app);

