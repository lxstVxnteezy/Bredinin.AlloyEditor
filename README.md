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

## 🏗️ Архитектура

```
┌─────────────────────────────────────────────────────────────┐
│                         Клиенты                             │
│            (Web / Desktop / API Clients)                    │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      API Gateway (5030)                     │
│                Bredinin.AlloyEditor.Gateway                 │
└───────────┬───────────────────────────────┬────────────────┘
            │                               │
            ▼                               ▼
┌─────────────────────────┐     ┌────────────────────────────┐
│    Identity Service     │     │     Alloy Editor API       │
│         (5020)          │     │          (5019)            │
└───────────┬─────────────┘     └───────────────┬────────────┘
            │                                   │
            ▼                                   ▼
     ┌───────────────┐                   ┌───────────────┐
     │   IdentityDB  │                   │    AlloyDB    │
     │  PostgreSQL   │                   │   PostgreSQL  │
     │    (5433)     │                   │     (5432)    │
     └───────────────┘                   └───────────────┘
            │
            ▼
     ┌───────────────┐
     │     Redis     │
     │     (6379)    │
     └───────────────┘
```

---

## 🛠️ Стек технологий

### Backend

* **.NET 8 (ASP.NET Core, C# 12)**
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
# Отредактируйте .env (пароли, ключи)

# Запустить инфраструктуру
docker-compose up -d

# Запустить сервисы (каждый в отдельном терминале)
cd Bredinin.AlloyEditor.Identity.Service && dotnet run
cd Bredinin.AlloyEditor.WebApi && dotnet run
cd Bredinin.AlloyEditor.Gateway && dotnet run
```

---

## 🌐 Доступ к сервисам

| Сервис       | URL                           | Данные (dev)     |
| ------------ | ----------------------------- | ---------------- |
| API Gateway  | http://localhost:5030/swagger | -                |
| Identity API | http://localhost:5020/swagger | -                |
| Alloy API    | http://localhost:5019/swagger | -                |
| Grafana      | http://localhost:3000         | admin / admin    |
| Prometheus   | http://localhost:9090         | -                |
| Redis        | localhost:6379                | пароль из `.env` |

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


## 📄 Лицензия

MIT — свободно используй, дорабатывай, продавай.

---

## 👨‍💻 Автор

**Иван Брединин**
GitHub: https://github.com/lxstVxnteezy
Email: [bredinin1@gmail.com](mailto:bredinin1@gmail.com)
Telegram: @vntzyyy

---

> «Хорошая архитектура — это когда код не мешает тебе спать на работе»
> — Иван Брединин, .NET-архитектор
