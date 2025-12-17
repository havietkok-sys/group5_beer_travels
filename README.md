# Beer Travels 🍺

Hej! Det här är vårt skolprojekt i grupp 5.

Vi har byggt ett REST API för ett bokningssystem där man kan boka hotellrum och hitta pubar i olika svenska städer.

**OBS:** Detta är bara backend/API - det finns ingen frontend. Man testar API:et med Postman.

---

## Vad kan API:et göra?

- Skapa ett konto och logga in
- Söka efter hotell i 10 svenska städer
- Boka rum och se vad det kostar
- Se sina bokningar
- Kolla vilka pubar som finns och vilka öl de har

---

## Hur kör man projektet?

### 1. Ladda ner
```
[git clone https://github.com/havietkok-sys/group5_beer_travels.git]
```

### 2. Skapa databasen
Kör detta i MySQL:
```sql
CREATE DATABASE beertravels;
CREATE USER 'beertravels'@'127.0.0.1' IDENTIFIED BY 'beertravels';
GRANT ALL PRIVILEGES ON beertravels.* TO 'beertravels'@'127.0.0.1';
```

### 3. Starta servern
```
dotnet run
```

### 4. Fyll databasen med testdata
```
curl -X DELETE http://localhost:5000/db
curl -X POST http://localhost:5000/db/seed
```

---

## Testanvändare

| E-post | Lösenord | Typ |
|--------|----------|-----|
| admin@beertravels.se | admin123 | Admin |
| user@beertravels.se | user123 | Vanlig användare |

---

## Några endpoints

### Användare
| Metod | URL | Beskrivning |
|-------|-----|-------------|
| POST | /users | Registrera ny användare |
| POST | /login | Logga in |
| DELETE | /login | Logga ut |

### Hotell & Rum
| Metod | URL | Beskrivning |
|-------|-----|-------------|
| GET | /cities/Stockholm/hotel | Hämta hotell i Stockholm |
| GET | /hotels/1/rooms | Se alla rum på ett hotell |
| GET | /rooms/1/availability?checkIn=2025-06-15&checkOut=2025-06-20 | Kolla lediga rum |

### Bokningar
| Metod | URL | Beskrivning |
|-------|-----|-------------|
| GET | /bookings | Se mina bokningar |
| POST | /bookings | Skapa en bokning |
| DELETE | /bookings/1 | Avboka |

### Pubar & Öl
| Metod | URL | Beskrivning |
|-------|-----|-------------|
| GET | /beers | Se alla ölsorter |
| GET | /pubs/1/beers | Se öl på en specifik pub |
| POST | /beers/create | Skapa ny öl (admin) |

---

## Vilka filer gör vad?

| Fil | Beskrivning |
|-----|-------------|
| Program.cs | Här ligger alla routes |
| Database.cs | Skapar alla tabeller |
| Auth.cs | Kollar om man är inloggad |
| Users.cs | Registrering och användare |
| Hotels.cs | Hotell |
| Rooms.cs | Rum och tillgänglighet |
| Bookings.cs | Bokningar |
| Pubs.cs | Pubar |
| Beers.cs | Öl |

---

## Tekniker vi använt

- C# och .NET
- MySQL databas
- REST API

---

## Grupp 5

Detta är ett skolprojekt.
