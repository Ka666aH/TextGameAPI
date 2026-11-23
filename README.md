# TextGameAPI

**Roguelike текстовая adventure-game в стиле RESTful API** — учебный pet-project.

[![CI](https://github.com/Ka666aH/TextGameAPI/actions/workflows/ci.yml/badge.svg)](https://github.com/Ka666aH/TextGameAPI/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/Ka666aH/TextGameAPI?logo=github)](https://github.com/Ka666aH/TextGameAPI/releases)
[![Docker Image Version](https://img.shields.io/docker/v/ka666ah/text-game-api?sort=semver&logo=docker&logoColor=fff)](https://hub.docker.com/r/ka666ah/text-game-api)
[![Docker Pulls](https://img.shields.io/docker/pulls/ka666ah/text-game-api?logo=docker&logoColor=fff)](https://hub.docker.com/r/ka666ah/text-game-api)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

![.NET 8](https://img.shields.io/badge/.NET_8-512BD4?logo=dotnet&logoColor=fff)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=fff)
![Redis](https://img.shields.io/badge/Redis-DC382D?logo=redis&logoColor=fff)
![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=fff)
![JWT](https://img.shields.io/badge/JWT-000000?logo=jsonwebtokens&logoColor=fff)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?logo=swagger&logoColor=000)

---

## Игровой процесс

Игрок путешествует по процедурно сгенерированному подземелью из комнат различных типов. В комнатах встречаются враги, сундуки с лутом, предметы экипировки и лечения, а также торговец.

Подземелье строится случайным образом: типы комнат выбираются по весам (взвешенный случайный выбор) с гарантированным завершением комнатой выхода.

Бой пошаговый: игрок атакует врага, враг атакует в ответ. Урон рассчитывается с учётом характеристик оружия, брони, случайных факторов и прочности экипировки. Есть несколько видов оружия и врагов со своими характеристиками и особенностями. Предметы имеют прочность и могут ломаться при использовании. Экипировку можно надевать и снимать.

В магазине можно покупать предметы и продавать найденное.

Цель игры — добраться до выхода.

---

## Технологический стек

| Технология | Назначение |
|-----------|------------|
| **.NET 8 + ASP.NET Core** | Основной фреймворк |
| **PostgreSQL + EF Core** | Хранение пользователей, токенов, сессий и сохранений |
| **Redis** | Кеширование текущих игровых сессий |
| **JWT (Access + Refresh tokens)** | Аутентификация с ротацией и отпечатком |
| **BCrypt** | Хеширование паролей и отпечатков токенов |
| **FluentValidation** | Валидация входных данных |
| **Swagger / OpenAPI** | Документация эндпоинтов |
| **Docker + Docker Compose** | Контейнеризация |
| **BackgroundService** | Фоновое автосохранение |
| **Rate Limiting** | Защита от брутфорса |

---

## Архитектура

Clean Architecture / DDD в 4 слоях. Зависимости направлены от внешних слоёв к внутренним:

```
Domain         — сущности, баланс, тексты, исключения
Application    — сервисы, оркестратор, фабрики, генераторы, guards, валидаторы
Infrastructure — EF Core, Redis, JWT, BCrypt, сериализация
Presentation   — контроллеры, middleware, атрибуты, DTO
```

Каждый следующий слой зависит от всех предыдущих.

Паттерны: **Repository**, **Unit of Work**, **Factory**, **Strategy** (взвешенный случайный выбор), **декларативные проверки предусловий** через DispatchProxy.

---

## Возможности

### Аутентификация
- Регистрация / логин / логаут
- JWT Access Token (15 мин) + Refresh Token (14 дней) с ротацией
- Fingerprint-привязка (User-Agent + IP)
- Обнаружение компрометации — при несовпадении отпечатка инвалидируется вся семья токенов
- Middleware бесшовно проверяет срок жизни Access Token и при необходимости обновляет
- Rate limiting на auth-эндпоинтах

### Игровое состояние и сохранения
- Сид генерации: задаётся игроком при создании сессии (`POST /sessions?seed=...`), иначе генерируется сам; хранится в сессии и виден в `GET /sessions`. Одинаковый сид → одинаковое подземелье (комнаты, содержимое, сундуки, мимик).
- Активная сессия живёт в **Redis** — каждый запрос читает и пишет состояние без обращения к БД
- 3 типа сохранений: Начальное (создаётся при старте сессии), Ручное, Автоматическое (фоновый сервис)
- При сохранении состояние сериализуется в JSON и записывается в **PostgreSQL**
- SHA-256 хеш для быстрого сравнения — автосохранение не перезаписывает БД, если состояние не изменилось
- При загрузке сессии состояние сначала ищется в Redis, при отсутствии — достаётся из БД и прогревается в кеш

### Безопасность
- Пароли и отпечатки хешируются BCrypt
- Ротация рефреш-токенов с детекцией семейства — каждый новый токен создаёт новое поколение, старые становятся недействительными. При использовании украденного токена с неверным отпечатком инвалидируется вся семья.
- Guard-атрибуты на оркестраторе проверяют: запущена ли игра, завершён ли поиск комнаты, не идёт ли бой, не пуст ли инвентарь, существует ли враг/сундук

---

## Docker

Образы публикуются в [`ka666ah/text-game-api`](https://hub.docker.com/r/ka666ah/text-game-api) (`linux/amd64`), а `docker-compose.yml` собирает образ из исходников.

### Быстрый старт

Переменные окружения необязательны: в `docker-compose.yml` уже есть рабочие значения по умолчанию.

```bash
docker compose up --build -d
```

- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`
- Health: `http://localhost:5000/health`

Остановить: `docker compose down`. Удалить данные вместе с томами: `docker compose down -v`.

### Настройка

Для своего деплоя задайте переменные из таблицы в окружении перед запуском:

| Переменная | По умолчанию | Назначение |
|---|---|---|
| `POSTGRES_DB` | `text_game_db` | Имя базы |
| `POSTGRES_USER` | `dev_user` | Пользователь БД |
| `POSTGRES_PASSWORD` | `dev_password` | Пароль БД |
| `JWT_SECRET` | placeholder | Секрет подписи JWT, **минимум 32 символа** |

Значения `POSTGRES_*` применяются только при первой инициализации тома. Если меняете их после первого запуска, пересоздайте том: `docker compose down -v`.

Время жизни токенов, TTL кеша, интервал автосохранения и лимиты рейтов — в `appsettings.json`.

### Разработка

Запустить API напрямую (нужны локальные Postgres/Redis, параметры — в `appsettings.Development.json`):

```bash
cd TextGameAPI
dotnet run
```

Для запуска тестов нужен **.NET SDK 10** (`dotnet test --project` использует Microsoft.Testing.Platform); само приложение собирается под `net8.0`.

> Для публичного деплоя обязательно задайте собственный `JWT_SECRET` и пароль БД. Значения по умолчанию предназначены только для локального запуска и демонстрации.

---

## API Endpoints

| Метод | Эндпоинт | Параметры | Назначение |
|-------|----------|-----------|------------|
| POST | `/users` | login, password | Регистрация (сразу вход, 201) |
| POST | `/auth/tokens` | login, password | Вход |
| GET | `/auth/me` | — | Кто залогинен: `userId`, `login`, активная сессия `sessionId`/`sessionName` (null — игры нет) |
| DELETE | `/auth/tokens` | — | Выход |
| POST | `/sessions` | name?, seed? | Новая игра (в ответе sessionId, sessionName, seed) |
| PUT | `/sessions/current` | id (в теле) | Сделать сессию активной |
| DELETE | `/sessions/{id}` | id | Удалить сессию |
| GET | `/sessions` | — | Список сессий (с сидами) |
| POST | `/saves` | name? | Создать сохранение |
| PUT | `/saves/current` | id (в теле) | Загрузить сохранение |
| DELETE | `/saves/{id}` | id | Удалить сохранение |
| GET | `/saves` | — | Список сохранений |
| GET | `/state` | — | Состояние игры |
| GET | `/state/map` | — | Карта подземелья |
| GET | `/state/coins` | — | Баланс монет |
| GET | `/state/keys` | — | Количество ключей |
| GET | `/state/inventory` | — | Инвентарь |
| GET | `/state/inventory/{id}` | id | Детали предмета |
| POST | `/state/inventory/{id}/sell` | id | Продать предмет |
| POST | `/state/inventory/{id}/use` | id | Использовать предмет |
| POST | `/state/inventory/{id}/equip` | id | Экипировать |
| GET | `/state/equipment` | — | Текущая экипировка |
| DELETE | `/state/equipment/weapon` | — | Снять оружие |
| DELETE | `/state/equipment/helm` | — | Снять шлем |
| DELETE | `/state/equipment/chestplate` | — | Снять нагрудник |
| GET | `/state/rooms/current` | — | Текущая комната |
| POST | `/state/rooms` | — | Следующая комната |
| PUT | `/state/rooms/current` | id (в теле) | Перейти в комнату |
| POST | `/state/rooms/current/enemy/attack` | — | Атаковать врага |
| GET | `/state/rooms/current/enemy` | — | Информация о враге |
| POST | `/state/rooms/current/items` | — | Обыскать комнату |
| POST | `/state/rooms/current/items/{id}/take` | id | Взять предмет |
| POST | `/state/rooms/current/items/takeall` | — | Взять всё |
| POST | `/state/rooms/current/items/{id}/buy` | id | Купить (в магазине) |
| POST | `/state/rooms/current/items/{chestId}/chest/hit` | chestId | Ударить сундук |
| POST | `/state/rooms/current/items/{chestId}/chest/open` | chestId | Открыть сундук |
| POST | `/state/rooms/current/items/{chestId}/chest/unlock` | chestId | Отпереть ключом |
| POST | `/state/rooms/current/items/{chestId}/chest/items` | chestId | Предметы в сундуке |
| POST | `/state/rooms/current/items/{chestId}/chest/items/{id}/take` | chestId, id | Взять из сундука |
| POST | `/state/rooms/current/items/{chestId}/chest/items/takeall` | chestId | Взять всё из сундука |
| GET | `/health` | — | Health check |

---

## Ключевые фичи реализации

- **Бесшовное взаимодействие** — пользователь не прерывается на повторные логины (токены обновляются автоматически) и не теряет прогресс после отсутствия (состояние автоматически сохраняется и восстанавливается)
- **Полиморфная иерархия GameObject** — Room, Item (с глубиной до 4 уровней: Item -> Equipment -> Weapon -> Sword), Enemy. Вся иерархия сериализуется через Newtonsoft.Json TypeNameHandling и восстанавливается с сохранением конкретного типа.
- **Динамические веса генерации** — вероятности появления каждого врага, предмета и комнаты зависят от номера комнаты через отдельные формулы, создавая плавное повышение сложности.
- **Локализация** — все строки (названия, описания, сообщения лога, ошибки), вынесены в `Domain/GameText`.

---

## CI/CD

- **CI** (`.github/workflows/ci.yml`) — на каждый push в `master` и pull request: сборка и полный прогон тестов (юнит + интеграционные) против Postgres 18 и Redis 7.
- **Release** (`.github/workflows/release.yml`) — по пушу тега `v*`: сначала тесты, затем сборка `linux/amd64`-образа, пуш в Docker Hub (`1.0.0`, `1.0`, `latest`) и создание GitHub Release.

---

## Лицензия

[MIT](LICENSE.txt) © 2026 Ka666aH
