# SocialNetworkAnalyzer

## WebApi
Solution: SocialNetworkAnalyzer.sln

### Struktura projektu
Solution je rozdělený do **4 částí**:
#### App
- Část doménocé logiky a WebAPI

#### Core
- Primitivní třídy, které jsou sdílené napříč sln

#### Data
- Implementace persistence

#### Tests
- Unit a integrační testy

### Abstractions projekty
Obsahují nugets, které využívají projekty, které referencují tento projekt - cílem je sjednocení nugets v projektech v sln, dále jsou určeny pro base interfaces.

## Část App
### WebApi
- Minimal API, endpointy jsou registrovány přes IEnpoint implementace - každý endpoint je separovaný ve vlastní třídě
- Design:
  - CQRS pattern - ačkoli se zde nejedná o "ukázkovou" implementaci CQRS (není zde dostatečná izolace Command/Query cesty, command není klasická "roura" kam jej zahodím), je tento design využitý pro vytváření datasetů (command) a dotazování datasetů se statistikami (Query)
  - Event driven - pro interní messaging se využívá MediatR, ukázka zpracovává asynchronně statistiky importovaných dat na základě eventu vytvoření datasetu
  - Validace vstupů pomocí FluentValidations
 
### App
- Implementace commandů, queries a event handlerů
- Implementace ASP.NET exception handlerů

## Část Core
- Guards: validační guardy - optimalizace cyklomatické komplexity
- Utils: drobnosti pro debug

## Část Data
Persistence je rozdělena na část abstrakce (SocialNetworkAnalyzer.Data.Repositories) a implementace (SocialNetworkAnalyzer.Data.EntityFramework), což umožňuje kompletně odstínit implementaci (Entity framework) z doménové logiky. Při spuštění aplikace se vytvoří databáze (update není podporovaný), následně práce s databází probíhá v transakcích.

## Testy
- Integrační testy probíhají nad spuštěnou DB, pro každý TestFixture se vytváří nové schema
- Mockování nebylo moc potřeba, jen výjimečně je použito FakeItEasy
- Pokrytí testy je > 90%

# Spuštění API
- pro debug je potřeba spustit postgres DB - v rootu repozitáře je docker-compose.yml s postgres databází `docker compose up -d`
- pro spuštění api je potřeba v adresáři *\src\SocialNetworkAnalyzer.App.WebApi* spustit příkaz `dotnet run -e Development` - Development environment je nutný pro spuštění swagger UI
- API je následně dostupná na URL [http://localhost:5000/swagger](http://localhost:5000/swagger)

# Provoz
Import dat:
- POST endpoint na URL [http://localhost:5000/datasets](http://localhost:5000/datasets)
- je nutný uploadovat content **text/***

Získání statistik:
- GET endpoint na URL [http://localhost:5000/datasets](http://localhost:5000/datasets)
- data jdou stránkovaná
- statistiky se aktualizují asynchronně, výstup obsahuje **state** a **stateString** indikující stav výpočtu statistik

Statistiky:
- nodesCount - počet uživatelů v datasetu
- relationshipsCount - počet vazeb (počítají se i vazby z druhého směru 1 -> 2 a 2 -> 1 i když jedna z nich fyzicky neexituje v DB)
- avgRelationsCount - průměrný počet vazeb uživatele
- avgGroupRelationsCount - průměrná velikost skupiny uživatelů, kteří mají mezi sebou vazbu
- error - v případě chyby je zde kompletní exception
