# 🏭 Bredinin.AlloyEditor

**Промышленная платформа для управления данными о металлических сплавах**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-336791?logo=postgresql)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7.4-DC382D?logo=redis)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-24.0-2496ED?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## 📋 О проекте

**AlloyEditor** — микросервисная платформа для работы со справочником металлических сплавов.

Система позволяет:

* Управлять сплавами и их химическим составом
* Работать со справочником химических элементов
* Управлять системами сплавов (Alloy Systems)
* Настраивать термообработку (Heat Treatment)
* Использовать JWT + Refresh токены
* Мониторить систему через Prometheus + Grafana

---

## ✨ Features

- Микросервисная архитектура
- API Gateway + NGINX Load Balancer
- JWT + Refresh Token аутентификация
- Redis caching
- CQRS через MediatR
- PostgreSQL + FluentMigrator
- Централизованное логирование (Serilog)
- Метрики и трейсинг (OpenTelemetry)
- Мониторинг через Prometheus + Grafana
- Docker инфраструктура

---

## 🏗️ Архитектура

```
┌─────────────────────────────────────────────────────────────┐
│                         Клиенты                             │
│                (Web / Desktop / API Clients)                │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                        NGINX (80)                           │
│  Reverse Proxy + Load Balancer + Rate Limiting              │
└───────────────┬─────────────────────────────────────────────┘
                │
                ▼
     ┌──────────────────────────────────────┐
     │         API Gateway Cluster          │
     │                                      │
     │   Gateway #1 (5030)                  │
     │   Gateway #2 (5031)                  │
     │   Gateway #3 (5032)                  │
     │                                      │
     │     Load balancing: least_conn       │
     └───────────────┬───────────────┬──────┘
                     │               │
                     ▼               ▼
        ┌─────────────────┐   ┌─────────────────┐
        │ IdentityService │   │  AlloyEditorAPI │
        │      (5020)     │   │      (5019)     │
        └─────────┬───────┘   └────────┬────────┘
                  │                    │
                  ▼                    ▼
          ┌──────────────┐      ┌──────────────┐
          │ Identity DB  │      │   Alloy DB   │
          │ PostgreSQL   │      │ PostgreSQL   │
          │    (5433)    │      │    (5432)    │
          └──────────────┘      └──────────────┘
                  │
                  ▼
             ┌─────────┐
             │  Redis  │
             │  6379   │
             └─────────┘
```

---

# ⚖️ NGINX Load Balancing и Rate Limiting

В системе используется **NGINX как единая точка входа**.

Он выполняет следующие функции:

* Reverse Proxy
* Load Balancing между API Gateway
* Rate Limiting
* Централизованное логирование
* Forwarding заголовков

### Балансировка нагрузки

Трафик распределяется между **3 экземплярами API Gateway**.

Используется алгоритм:

```
least_conn
```

Алгоритм **Least Connections** направляет новый входящий запрос на тот сервер,
у которого **наименьшее количество активных соединений** в данный момент.

В отличие от стандартного **round-robin**, этот подход позволяет:

* равномернее распределять нагрузку
* избегать перегрузки отдельных инстансов
* учитывать длительные запросы

Это особенно важно для API, где время обработки запроса может отличаться.

### Пример работы

| Gateway | Активные соединения |
|--------|---------------------|
| Gateway #1 | 12 |
| Gateway #2 | 5 |
| Gateway #3 | 8 |

Новый запрос будет отправлен в **Gateway #2**, потому что у него **наименьшее число активных соединений**.

Такой подход обеспечивает **более эффективное использование ресурсов и лучшую масштабируемость системы**.

## 🛠️ Стек технологий

### Backend

* **.NET 8 (ASP.NET Core, C# 12)**
* **NGINX**
* **Entity Framework Core** — ORM
* **FluentMigrator** — миграции БД
* **MediatR** — CQRS
* **FluentValidation** — валидация
* **Refit** — HTTP-клиенты
* **StackExchange.Redis** — кэширование
* **Serilog** — логирование
* **OpenTelemetry** — метрики и трейсинг

### Desktop

* **WPF**
* **Prism (MVVM)**
* **Telerik UI**

### Инфраструктура

* **Docker + Docker Compose**
* **PostgreSQL 15** (2 экземпляра)
* **Redis 7**
* **Prometheus**
* **Grafana**
* **OpenTelemetry Collector**

---

## 🔐 Аутентификация и безопасность

* JWT access tokens (15 минут)
* Refresh tokens в Redis (одноразовые, с ротацией)
* BCrypt — хеширование паролей
* Ролевая модель (User / Admin)
* Rate Limiting на уровне Gateway

### Пример ротации refresh токена

```csharp
public async Task<AuthResponse> RefreshAsync(string refreshToken, User user)
{
    var key = RefreshTokenPrefix + refreshToken;
    var entryJson = await cache.GetStringAsync(key);

    if (entryJson == null)
        throw new UnauthorizedAccessException("Invalid refresh token");

    await cache.RemoveAsync(key);
    return await GenerateTokensAsync(user);
}
```

---

## 📡 Пример API

### Создание сплава

POST `/api/alloys`

```json
{
  "name": "Сталь 45",
  "description": "Конструкционная углеродистая сталь",
  "alloySystemId": "fe-c-system-id",
  "chemicalCompositions": [
    {
      "chemicalElementId": "carbon-id",
      "minValue": 0.42,
      "maxValue": 0.5
    },
    {
      "chemicalElementId": "silicon-id",
      "minValue": 0.17,
      "maxValue": 0.37
    }
  ],
  "heatTreatments": [
    {
      "heatTreatmentTypeId": "quenching-id",
      "temperatureMin": 800,
      "temperatureMax": 820,
      "holdingTimeMin": 30,
      "holdingTimeMax": 60,
      "coolingMedium": "Масло",
      "isDefault": true
    }
  ],
  "defaultMechanicalProperties": [
    {
      "propertyTypeId": "hardness-hb-id",
      "valueMin": 170,
      "valueMax": 210,
      "condition": "исходное состояние",
      "source": "ГОСТ 1050-2013"
    },
    {
      "propertyTypeId": "tensile-strength-id",
      "valueExact": 600,
      "condition": "исходное состояние"
    }
  ]
}
```

### Ответ

201
```json
{
  "id": 3fa85f64-5717-4562-b3fc-2c963f66afa6, 
}
```

---

## 📊 Мониторинг

Стек мониторинга:

* OpenTelemetry — сбор метрик
* Prometheus — хранение метрик
* Grafana — визуализация

В Grafana доступны:

* .NET метрики (GC, ThreadPool, HTTP)
* Бизнес-метрики (сплавы, пользователи)
* Redis статистика
* PostgreSQL метрики

---

## ⚙️ Переменные окружения

| Переменная | Описание |
|------------|----------|
| POSTGRES_PASSWORD | пароль PostgreSQL |
| REDIS_PASSWORD | пароль Redis |
| JWT_SECRET | секретный ключ JWT |
| JWT_ISSUER | издатель токена |
| JWT_AUDIENCE | аудитория токена |

---

## 🚀 Быстрый старт

### Требования

* Docker Desktop
* .NET 8 SDK
* Git

### Установка

```bash
# Клонировать репозиторий
git clone https://github.com/lxstVxnteezy/Bredinin.AlloyEditor.git
cd Bredinin.AlloyEditor

# Настроить переменные окружения
cp .env.example .env

# Запустить инфраструктуру
docker-compose up -d

# Запустить сервисы
cd Bredinin.AlloyEditor.Identity.Service && dotnet run
cd Bredinin.AlloyEditor.WebApi && dotnet run
cd Bredinin.AlloyEditor.Gateway && dotnet run
```

---

## 🌐 Доступ к сервисам

| Сервис       | URL                           | Данные (dev) |
|--------------|-------------------------------|---------------|
| API Gateway  | http://localhost:5030/swagger | - |
| Identity API | http://localhost:5020/swagger | - |
| Alloy API    | http://localhost:5019/swagger | - |
| Grafana      | http://localhost:3000 | admin / admin |
| Prometheus   | http://localhost:9090 | - |
| Redis        | localhost:6379 | пароль из `.env` |

---

## 📁 Структура проекта

```
Bredinin.AlloyEditor/
├── Bredinin.AlloyEditor.Common/
├── Bredinin.AlloyEditor.Core/
├── Bredinin.AlloyEditor.Contracts/
├── Bredinin.AlloyEditor.Domain/
├── Bredinin.AlloyEditor.DAL/
├── Bredinin.AlloyEditor.Handlers/
├── Bredinin.AlloyEditor.WebApi/
├── Bredinin.AlloyEditor.Identity.Service/
├── Bredinin.AlloyEditor.Gateway/
├── Bredinin.AlloyEditor.Desktop/
├── docker-compose.yml
├── .env
├── prometheus.yml
├── otel-collector-config.yaml
└── redis.conf
```

---

## 🐳 Docker сервисы

| Контейнер | Назначение |
|-----------|------------|
| nginx | Reverse proxy |
| postgres-alloy | БД сплавов |
| postgres-identity | БД пользователей |
| redis | кэш и refresh tokens |
| prometheus | хранение метрик |
| grafana | визуализация |
| otel-collector | сбор telemetry |

---

## 🧪 Тестирование

Проект использует:

* **xUnit**
* **FluentAssertions**
* **Testcontainers**

---

## 🗺️ Roadmap

- [x] JWT Authentication
- [x] Redis Refresh Tokens
- [x] NGINX Load Balancing
- [x] Prometheus + Grafana
- [ ] Kubernetes deployment
- [ ] CI/CD pipeline
- [ ] Alloy simulation module
- [ ] ML prediction for alloy properties

---

## 🤝 Contributing

Pull requests приветствуются.

1. Fork репозиторий
2. Создать feature branch
3. Сделать commit
4. Открыть Pull Request

---

## 📄 Лицензия

MIT — свободно используй, дорабатывай, продавай.

---

## 👨‍💻 Автор

**Иван Брединин**

GitHub: https://github.com/lxstVxnteezy  
Email: bredinin1@gmail.com  
Telegram: @vntzyyy

---

> «Хорошая архитектура — это когда код не мешает тебе спать на работе»  
> — Иван Брединин, .NET-архитектор
