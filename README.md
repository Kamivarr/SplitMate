# SplitMate — System Zarządzania Wspólnymi Wydatkami

**Autor:** Kamil Śliwa  
**Indeks:** 177165  
**Technologie:** .NET 8 (Web API), React (TypeScript), PostgreSQL, Docker

---

## 🚀 Cel projektu
SplitMate to aplikacja webowa inspirowana rozwiązaniami typu "Splitwise", umożliwiająca grupie znajomych śledzenie wspólnych wydatków, automatyczne obliczanie bilansów oraz bezpieczne rozliczanie długów. 

Głównym założeniem architektonicznym była pełna konteneryzacja (Docker), dzięki czemu środowisko uruchomieniowe jest spójne i nie wymaga instalacji lokalnych zależności (poza Dockerem).

## ✨ Kluczowe funkcjonalności

* **Autentykacja i Bezpieczeństwo (JWT)**
  * Logowanie przy użyciu tokenów JWT (JSON Web Token).
  * Ochrona endpointów API (wymagany nagłówek `Authorization`).
  * Weryfikacja tożsamości po stronie backendu przy operacjach krytycznych.

* **Zarządzanie Grupami**
  * Tworzenie nowych grup wyjazdowych/domowych bezpośrednio z panelu użytkownika.
  * Dynamiczne przypisywanie użytkowników do grup.

* **Rejestracja Wydatków**
  * Dodawanie kosztów z określeniem płatnika (kto założył pieniądze).
  * Definiowanie uczestników wydatku (na kogo dzielony jest koszt).

* **Inteligentne Podsumowanie (Bilans)**
  * Automatyczny algorytm wyliczający saldo ("kto komu ile wisi") w czasie rzeczywistym.
  * Agregacja długów wewnątrz grupy.

* **Bezpieczne Spłaty (Settlements)**
  * Funkcja "Potwierdź odbiór" – system pozwala oznaczyć dług jako spłacony tylko wtedy, gdy akcję wykonuje **rzeczywisty odbiorca** pieniędzy. Zapobiega to sytuacji, w której dłużnik sam anuluje swoje zobowiązanie.

* **Auto-Seeding & Database Init**
  * Automatyczne tworzenie schematu bazy danych przy starcie kontenera (`EnsureCreated`).
  * Wstępne wypełnianie bazy danymi testowymi (użytkownicy, grupy).

---

## 🏗️ Architektura Systemu

Poniższy diagram przedstawia strukturę kontenerów Docker oraz przepływ danych w aplikacji:

```mermaid
graph TD
    User((Użytkownik))
    
    subgraph "Docker Compose Network"
        style Frontend fill:#e1f5fe,stroke:#01579b,stroke-width:2px
        style Backend fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px
        style Db fill:#fff3e0,stroke:#ef6c00,stroke-width:2px

        Frontend[Frontend<br/>React + Vite + TypeScript<br/>Port: 3000]
        Backend[Backend API<br/>ASP.NET Core 8.0<br/>Port: 5000]
        Db[(Baza Danych<br/>PostgreSQL 15<br/>Port: 5432)]
    end

    User -->|Przeglądarka| Frontend
    Frontend -->|REST API (JSON + JWT)| Backend
    Backend -->|Entity Framework Core| Db

```

---

## 🛠️ Wymagania Lokalne

Aby uruchomić projekt, potrzebujesz jedynie:

* **Docker Desktop** (Windows/Mac) lub **Docker Engine** (Linux)
* **Docker Compose**

> **Uwaga:** Nie musisz instalować lokalnie .NET SDK, Node.js ani serwera PostgreSQL. Wszystko jest zawarte w obrazach Docker.

---

## 🏁 Jak uruchomić (Szybki Start)

1. **Sklonuj repozytorium:**
```bash
git clone https://github.com/Kamivarr/SplitMate.git
cd SplitMate

```


2. **Uruchom aplikację:**
```bash
docker compose up --build

```


*Pierwsze uruchomienie może potrwać kilka minut, ponieważ Docker musi zbudować obrazy frontendu i backendu oraz pobrać zależności.*
3. **Gotowe! Aplikacja dostępna jest pod adresami:**
* **Dashboard (Frontend):** [http://localhost:3000](https://www.google.com/search?q=http://localhost:3000)
* **Dokumentacja API (Swagger):** [http://localhost:5000/swagger](https://www.google.com/search?q=http://localhost:5000/swagger)



---

## 🔑 Dane Testowe (Logowanie)

Aplikacja startuje z wstępnie skonfigurowanymi użytkownikami. Hasło dla wszystkich kont to: **`kamil123`**.

| Login | Rola w scenariuszu |
| --- | --- |
| **`kamil`** | Główny użytkownik testowy (często płatnik). |
| **`anna`** | Użytkownik, który może posiadać długi wobec Kamila. |
| **`arek`** | Dodatkowy członek grupy. |

---

## 💻 Przydatne komendy

**Zatrzymanie aplikacji:**

```bash
docker compose down

```

**Twardy reset (usunięcie bazy danych i rozpoczęcie od czysta):**
Użyj tej komendy, jeśli chcesz przywrócić stan początkowy (seed) bazy danych.

```bash
docker compose down -v
docker compose up --build

```

**Podgląd logów backendu:**
Przydatne do debugowania zapytań SQL lub błędów API.

```bash
docker compose logs -f backend

```

