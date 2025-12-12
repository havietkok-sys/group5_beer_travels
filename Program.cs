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

app.Run();
app.Run();


// Login
app.MapGet("/login", Login.Get);
app.MapPost("/login", Login.Post);
app.MapDelete("/login", Login.Delete);



//Users
app.MapGet("/users", Users.GetAll);
app.MapPost("/users", Users.Post);
app.MapGet("/users/{id}", Users.Get);
app.MapDelete("/users/{id}", Users.Delete);  //admin


// Skapa cities
app.MapPost("/cities/create", Cities.CreateCity);    //admin
app.MapDelete("/cities/{id}", Cities.DeleteCity);   //admin

// Hämtar Pubbar ur en specifik stad och hotelet.
app.MapGet("/cities/{city}/pubs", Pubs.GetPubs);

// BEER CRUD
app.MapPost("/beers/create", Beers.Post); //admin
app.MapGet("/beers", Beers.GetAll);
app.MapGet("/beers/{id}", Beers.Get);
app.MapDelete("/beers/{id}", Beers.Delete); //admin
app.MapGet("/beers/cheapest", BeerTravelStatistics.GetTop10Cheapest);

// Skapa hotel 
app.MapPost("/hotels/create", Hotels.CreateHotel); //Admin
app.MapGet("/cities/{city}/hotel", Hotels.GetHotel);


// Pub ölernas crud
app.MapPost("/pubs/{pubId}/addbeer", PubBeers.AddBeerToPub); //Admin
app.MapDelete("/pubs/{pubId}/beers/{beerId}", PubBeers.RemoveBeerFromPub); //Admin

app.MapGet("/pubs/{pubId}/beers", Pubs.GetBeersForPub); //Hämtar ut alla öl på specidic pub



// special, reset db
app.MapDelete("/db", Database.db_reset_to_default); //admin




//Pub Crud
app.MapPost("/pubs/create", Pubs.CreatePub); //Admin


app.MapPost("/beer_flavors/create", beer_flavors.CreateFlavor);
app.MapGet("/beer_flavors", beer_flavors.GetAllFlavors);
app.MapDelete("/beer_flavors/{id}", beer_flavors.DeleteFlavor);
app.MapPost("/beers/{beerId}/add_flavor/{flavorId}", beer_flavors.AddflavorTobeer);
app.MapDelete("/beers/{beerId}/remove_flavor/{flavorId}", beer_flavors.Removeflavorfrombeer);




// Crud endpoints för databasen
//DatabaseEndpoints.Map(app);

