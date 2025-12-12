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


Console.WriteLine("PROGRAM.CS LOADED!");

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
//app.MapGet("/cities/{city}/pubs", Pubs.GetPubs);

app.MapGet("/cities/{cityName}/pubs", Pubs.GetPubs);


// BEER CRUD
app.MapPost("/beers/create", Beers.Post); //admin
app.MapGet("/beers", Beers.GetAll);
app.MapGet("/beers/{id}", Beers.Get);
app.MapDelete("/beers/{id}", Beers.Delete); //admin


// Skapa hotel 
app.MapPost("/hotels/create", Hotels.CreateHotel); //Admin
app.MapGet("/cities/{city}/hotel", Hotels.GetHotel);


// Pub ölernas crud
app.MapPost("/pubs/{pubId}/addbeer", PubBeers.AddBeerToPub); //Admin
app.MapDelete("/pubs/{pubId}/beers/{beerId}", PubBeers.RemoveBeerFromPub); //Admin

//Pub Crud
app.MapPost("/pubs/create", Pubs.CreatePub); //Admin


// Statestik och informations hämtning

app.MapGet("/statistics/average-beer-price", BeerTravelStatistics.GetAverageBeerPricePerCity);
app.MapGet("/beers/cheapest", BeerTravelStatistics.GetTop10Cheapest);

app.MapGet("/pubs/{pubId}/beers", Pubs.GetBeersForPub); //Hämtar ut alla öl på specidic pub


// Hämtar Pubbar ur en specifik stad och hotelet.
app.MapGet("/cities/{city}/pubs", Pubs.GetPubs);

// ROOM ENDPOINTS

app.MapPost("/rooms/create", Rooms.CreateRoom);                        // Skapa rumstyp (admin)
app.MapGet("/hotels/{hotelId}/rooms", Rooms.GetRoomsByHotel);          // Hämta rumstyper för hotell
app.MapGet("/rooms/{id}", Rooms.GetRoom);                              // Hämta en rumstyp
app.MapGet("/hotels/{hotelId}/rooms/available",
    async (int hotelId, string checkIn, string checkOut, Config config) =>
    {
        if (!DateOnly.TryParse(checkIn, out var checkInDate) ||
            !DateOnly.TryParse(checkOut, out var checkOutDate))
            return Results.BadRequest("Ogiltigt datumformat. Använd YYYY-MM-DD");

        var rooms = await Rooms.GetAvailableRooms(hotelId, checkInDate, checkOutDate, config);
        return Results.Ok(rooms);
    });                                                                 // Hämta lediga rum för datum
app.MapDelete("/rooms/{id}", Rooms.DeleteRoom);                        // Ta bort rumstyp (admin)
app.MapPut("/rooms/{id}", Rooms.UpdateRoom);                           // Uppdatera rumstyp (admin)


// BOOKING ENDPOINTS
app.MapPost("/bookings", Bookings.CreateBooking);                      // Skapa bokning (inloggad)
app.MapGet("/bookings/my", Bookings.GetMyBookings);                    // Mina bokningar (inloggad)
app.MapGet("/bookings/{id}", Bookings.GetBooking);                     // Bokningsdetaljer
app.MapDelete("/bookings/{id}", Bookings.CancelBooking);               // Avboka
app.MapGet("/bookings", Bookings.GetAllBookings);                      // Alla bokningar (admin)


// BOOKING-ROOMS ENDPOINTS (koppling mellan bokningar och rum)
app.MapPost("/booking-rooms", BookingRooms.AddRoomToBooking);          // Lägg till rum i bokning
app.MapGet("/bookings/{bookingId}/rooms",
    async (int bookingId, Config config) =>
    Results.Ok(await BookingRooms.GetRoomsByBooking(bookingId, config)));  // Hämta rum i bokning
app.MapGet("/bookings/{bookingId}/summary",
    async (int bookingId, int nights, Config config) =>
    Results.Ok(await BookingRooms.GetBookingSummary(bookingId, nights, config)));  // Kostnadssammanfattning
app.MapPut("/booking-rooms/{id}",
    async (int id, int quantity, Config config, HttpContext ctx) =>
    await BookingRooms.UpdateRoomQuantity(id, quantity, config, ctx));  // Uppdatera antal rum
app.MapDelete("/booking-rooms/{id}", BookingRooms.RemoveRoomFromBooking);  // Ta bort rum från bokning


// SEED
app.MapDelete("/db/seed", DatabaseSeedEndpoints.ReseedDatabase);   // Rensa och fyll om


// special, reset db
app.MapDelete("/db", async (Config config, HttpContext ctx) =>
{
    var authCheck = await Auth.RequireAdmin(config, ctx);
    if (authCheck is not null)
        return authCheck;

    await Database.db_reset_to_default(config);
    return Results.Ok("Database reset");
});


app.Run();


