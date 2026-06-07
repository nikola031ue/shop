# Shop - Backend

ASP.NET Core 10 Web API sa CQRS/MediatR arhitekturom i PostgreSQL bazom.

## Zahtjevi

| Alat | Verzija |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | 4.x+ |
| [Docker Compose](https://docs.docker.com/compose/) | v2+ |

## Struktura projekta

```
backend/
├── Shop.Api/            # ASP.NET Core Web API - entry point, kontroleri, middleware
├── Shop.Application/    # CQRS handleri (MediatR), servisi, interfejsi
├── Shop.Domain/         # Domain entiteti, value objekti, domain eventi
├── Shop.Infrastructure/ # EF Core DbContext, PostgreSQL repozitorijumi
├── Shop.Tests.Unit/     # Unit testovi
└── Shop.Tests.Integration/  # Integracioni testovi (Testcontainers)
```

## Pokretanje lokalno (bez Dockera)

### 1. Pokretanje PostgreSQL baze

```bash
docker run -d \
  --name shop-db \
  -e POSTGRES_DB=shopdb \
  -e POSTGRES_USER=shop \
  -e POSTGRES_PASSWORD=shop123 \
  -p 5432:5432 \
  postgres:16
```

### 2. Pokretanje API-ja

```bash
cd Shop.Api
dotnet run
```

API je dostupan na: `http://localhost:5000`

Swagger UI: `http://localhost:5000/swagger`

## Pokretanje putem Dockera

Iz root foldera repozitorijuma (`shop/`):

```bash
docker compose up --build
```

Ovo pokreće:
- `api` - backend na portu `5000`
- `db` - PostgreSQL na portu `5432`

Za zaustavljanje:

```bash
docker compose down
```

Za zaustavljanje i brisanje podataka iz baze:

```bash
docker compose down -v
```

## Migracije baze podataka

### Automatska primjena

Migracije se **automatski primjenjuju pri svakom pokretanju aplikacije** - nema potrebe da ih ručno pokrećeš za lokalni razvoj ili deployment. Seed podaci (admin korisnik i test proizvodi) se dodaju samo ako baza još uvijek prazna.

### Kada kreirati novu migraciju

Novu migraciju treba kreirati kada se **promijeni model domene** - dodavanje entiteta, kolone, indeksa ili promjena ograničenja:

```bash
# Instalacija dotnet-ef alata (jednom, globalno)
dotnet tool install --global dotnet-ef

# Kreiranje nove migracije (izvršiti iz backend/ foldera)
dotnet ef migrations add <NazivMigracije> \
  --project Shop.Infrastructure \
  --startup-project Shop.Api \
  --output-dir Persistence/Migrations
```

Primjeri naziva migracija: `AddProductCategory`, `AddOrderShippingAddress`, `RenameStockToQuantity`.

Migracije se čuvaju u `Shop.Infrastructure/Persistence/Migrations/` i trebaju biti commitovane u repozitorijum.

### Ručna primjena (opcionalno)

Ako iz nekog razloga treba ručno primijeniti migracije na bazu (npr. debugging):

```bash
dotnet ef database update \
  --project Shop.Infrastructure \
  --startup-project Shop.Api
```

### Rollback migracije

```bash
# Vraćanje na određenu migraciju (po imenu)
dotnet ef database update <NazivPrethodneMigracije> \
  --project Shop.Infrastructure \
  --startup-project Shop.Api

# Brisanje posljednje neprimijenjene migracije (samo ako nije u bazi)
dotnet ef migrations remove \
  --project Shop.Infrastructure \
  --startup-project Shop.Api
```

### Connection string (lokalni razvoj)

```
Host=localhost;Port=5432;Database=shopdb;Username=shop;Password=shop123
```

## Pokretanje testova

```bash
# Svi testovi
dotnet test

# Samo unit testovi
dotnet test Shop.Tests.Unit

# Samo integracioni testovi (zahtijeva Docker za Testcontainers)
dotnet test Shop.Tests.Integration
```

## Environment varijable

| Varijabla | Opis | Default |
|-----------|------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | - |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` | `Production` |
