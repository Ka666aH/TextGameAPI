# TextGameAPI

**Roguelike текстовая adventure-game в стиле RESTful API** — учебный pet-project.

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

```yaml
services:
  textgame:   # ASP.NET Core API (порт 8080)
  postgres:   # PostgreSQL 18
  redis:      # Redis (latest)
```

```bash
docker compose up --build
```

Переменные окружения (`.env`): `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `JWT_SECRET`.

Настройки времени жизни токенов, TTL кеша, интервала автосохранения и лимитов рейтов — в `appsettings.json`.

Swagger UI: `http://localhost:5000/swagger`

---

## API Endpoints

| Метод | Эндпоинт | Параметры | Назначение |
|-------|----------|-----------|------------|
| POST | `/auth/register` | login, password | Регистрация |
| POST | `/auth/login` | login, password | Вход |
| POST | `/auth/logout` | — | Выход |
| POST | `/sessions` | name? | Новая сессия |
| POST | `/sessions/{id}` | id | Загрузить сессию |
| DELETE | `/sessions/{id}` | id | Удалить сессию |
| GET | `/sessions` | — | Список сессий |
| POST | `/saves` | name? | Создать сохранение |
| POST | `/saves/{id}` | id | Загрузить сохранение |
| DELETE | `/saves/{id}` | id | Удалить сохранение |
| GET | `/saves` | — | Список сохранений |
| GET | `/state/info` | — | Состояние игры |
| GET | `/state/map` | — | Карта подземелья |
| GET | `/state/coins` | — | Баланс монет |
| GET | `/state/keys` | — | Количество ключей |
| GET | `/state/inventory` | — | Инвентарь |
| GET | `/state/inventory/{id}` | id | Детали предмета |
| POST | `/state/inventory/{id}/sell` | id | Продать предмет |
| POST | `/state/inventory/{id}/use` | id | Использовать предмет |
| POST | `/state/inventory/{id}/equip` | id | Экипировать |
| GET | `/state/equipment` | — | Текущая экипировка |
| POST | `/state/equipment/weapon/unequip` | — | Снять оружие |
| POST | `/state/equipment/helm/unequip` | — | Снять шлем |
| POST | `/state/equipment/chestplate/unequip` | — | Снять нагрудник |
| GET | `/state/rooms/current` | — | Текущая комната |
| POST | `/state/rooms/next` | — | Следующая комната |
| POST | `/state/rooms/{id}` | id | Перейти в комнату |
| POST | `/state/rooms/current/enemy/attack` | — | Атаковать врага |
| GET | `/state/rooms/current/enemy` | — | Информация о враге |
| GET | `/state/rooms/current/items` | — | Предметы в комнате |
| POST | `/state/rooms/current/items/{id}/take` | id | Взять предмет |
| POST | `/state/rooms/current/items/takeall` | — | Взять всё |
| POST | `/state/rooms/current/items/{id}/buy` | id | Купить (в магазине) |
| POST | `/state/rooms/current/chest/open` | — | Открыть сундук |
| POST | `/state/rooms/current/chest/unlock` | — | Отпереть ключом |
| POST | `/state/rooms/current/chest/hit` | — | Ударить сундук |
| GET | `/state/rooms/current/chest/items` | — | Предметы в сундуке |
| POST | `/state/rooms/current/chest/items/{id}/take` | id | Взять из сундука |
| POST | `/state/rooms/current/chest/items/takeall` | — | Взять всё из сундука |
| GET | `/health` | — | Health check |

---

## Ключевые фичи реализации

- **Бесшовное взаимодействие** — пользователь не прерывается на повторные логины (токены обновляются автоматически) и не теряет прогресс после отсутствия (состояние автоматически сохраняется и восстанавливается)
- **Полиморфная иерархия GameObject** — Room, Item (с глубиной до 4 уровней: Item -> Equipment -> Weapon -> Sword), Enemy. Вся иерархия сериализуется через Newtonsoft.Json TypeNameHandling и восстанавливается с сохранением конкретного типа.
- **Динамические веса генерации** — вероятности появления каждого врага, предмета и комнаты зависят от номера комнаты через отдельные формулы, создавая плавное повышение сложности.
- **Локализация** — все строки (названия, описания, сообщения лога, ошибки), вынесены в `Domain/GameText`.
