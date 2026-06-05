# Shop — Frontend

React 18 + TypeScript + Vite aplikacija za Shop platformu.

## Zahtjevi

| Alat | Verzija |
|------|---------|
| [Node.js](https://nodejs.org/) | 20.13+ |
| [npm](https://www.npmjs.com/) | 10+ |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | 4.x+ (opciono, za production build) |

## Tehnologije

- **Vite 5** — build alat i dev server
- **React 18** + **TypeScript 5**
- **React Router v6** — rutiranje
- **TanStack Query v5** — server state management i API pozivi
- **Axios** — HTTP klijent
- **Tailwind CSS v3** — stilizacija
- **Vitest v2** + **Testing Library** — testovi

## Instalacija

```bash
cd frontend
npm install
```

## Pokretanje lokalno

Backend mora biti pokrenut na `http://localhost:5000` (pogledaj [backend README](../backend/README.md)).

```bash
npm run dev
```

Aplikacija je dostupna na: `http://localhost:3000`

Dev server automatski prosljeđuje `/api/*` zahtjeve na `http://localhost:5000`.

## Pokretanje testova

```bash
# Watch mode — pokreće se pri svakom snimanju fajla (za razvoj)
npm test

# Jednokratno pokretanje — kao na CI
npm run test:run

# Interaktivni UI u browseru
npm run test:ui
```

### Šta se testira

| Fajl | Šta pokriva |
|------|-------------|
| `src/hooks/useDebounce.test.ts` | Debounce logika — timing, cancel međuvrijednosti |
| `src/components/SearchBar.test.tsx` | Unos teksta, clear dugme |
| `src/components/Pagination.test.tsx` | Render uslovi, disabled stanje, klikovi, ellipsis |
| `src/pages/ProductsPage.test.tsx` | Prikaz proizvoda, debounced search, empty/error state, paginacija |

### Struktura testa

Testovi koriste [Vitest](https://vitest.dev/) + [Testing Library](https://testing-library.com/). API pozivi se mockuju sa `vi.mock()` — nije potreban pokrenut backend.

```bash
# Pokretanje samo jednog fajla
npm run test:run -- src/components/Pagination.test.tsx

# Pokretanje testova koji odgovaraju pattern-u
npm run test:run -- --reporter=verbose SearchBar
```

## Ostale komande

```bash
# Build za produkciju
npm run build

# Preview production builda lokalno
npm run preview

# TypeScript provjera bez build-a
npx tsc --noEmit

# Linting
npm run lint
```

## Pokretanje putem Dockera

Iz root foldera repozitorijuma (`shop/`), uz pokrenut backend:

```bash
docker compose -f docker-compose.yml -f docker-compose.frontend.yml up --build
```

Frontend je dostupan na: `http://localhost:3000`

## Struktura projekta

```
frontend/
├── public/              # Statički fajlovi
├── src/
│   ├── assets/          # Slike, fontovi
│   ├── components/      # Dijeljene UI komponente
│   ├── pages/           # Stranice (odgovaraju rutama)
│   ├── hooks/           # Custom React hookovi
│   ├── services/        # Axios API klijenti
│   ├── test/
│   │   └── setup.ts     # Vitest setup
│   ├── App.tsx
│   └── main.tsx
├── tailwind.config.js
├── vite.config.ts
└── tsconfig.app.json
```

## Environment varijable

Kreiraj `.env.local` fajl u `frontend/` folderu:

```env
VITE_API_URL=http://localhost:5000
```

> `.env.local` je u `.gitignore` i neće biti commit-ovan.
