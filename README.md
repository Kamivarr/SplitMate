# SplitMate — Lokalny setup

Autor: Kamil Śliwa, indeks 177165

## Cel
Prosty backend ASP.NET Core Web API + EF Core (PostgreSQL) do zarządzania wspólnymi wydatkami.
Na start: modele (User, Group, Expense, ExpenseShare), DbContext, migracje i seed przykładowych danych, Docker + docker-compose.

## Wymagania lokalne
- Docker & Docker Compose
- .NET SDK 8.0 (opcjonalne, jeśli chcesz uruchamiać/rozwijać lokalnie bez Dockera)
- (opcjonalnie) dotnet-ef global tool dla migracji lokalnych

## Jak uruchomić wszystko lokalnie (najprościej)
1. Sklonuj repo (lub skopiuj pliki).
2. Uruchom:
   ```bash
   docker-compose up --build
   dotnet ef database update

