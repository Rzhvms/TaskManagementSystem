# Task Management System

## Описание
Распределенная система для управления задачами с возможностью получения уведомлений в реальном времени.

**Технологический стек:**
- C#, .NET 8
- PostgreSQL
- SignalR (realtime-уведомления)
- Микросервисная архитектура (3+ сервиса)
- Межсервисное взаимодействие по HTTP

**Дополнительно:**
- InMemory (кеширование)
- JWT (аутентификация)
- Unit-тесты (xUnit)
- Health Checks на каждый сервис
- Оптимизация запросов к БД (индексы)

## Запуск и развертывание

### Требования:
- Установленный [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Инструкция по запуску:

1. Клонируйте репозиторий:
```cmd
git clone git@github.com:Rzhvms/TaskManagementSystem.git
cd TaskManagementSystem
```

2. Запустите все сервисы с помощью Docker Compose:
```cmd
docker-compose up --build -d
```

3. После успешного запуска сервисы будут доступны:

- API Gateway: http://localhost:8080
- Task Service: http://localhost:5001/api/tasks или http://localhost:8080/api/tasks (через API Gateway)
- Notification Service: http://localhost:5002/api/notifications или http://localhost:8080/api/notifications (через API Gateway)
- Auth Service: http://localhost:5003/api/auth или http://localhost:8080/api/auth (через API Gateway)

4. Для остановки сервисов выполните:
```cmd
docker-compose down
```
