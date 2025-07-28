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

## Документация API
Документация API для каждого микросервиса хранится в папке ../ApiDocumentation/

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

## Примеры запросов

### Auth Service

**Регистрация пользователя:**

POST api/auth/register
```cmd
curl -X 'POST' \
  'http://localhost:8080/api/auth/register' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "username": "User",
  "email": "user@example.com",
  "password": "password"
}'
```

**Вход (получение JWT токена):**

POST api/auth/login
```cmd
curl -X 'POST' \
  'http://localhost:8080/api/auth/login' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "user@example.com",
  "password": "password"
}'
```

**Получение информации о текущем пользователе:**

GET api/auth/me
```cmd
curl -X 'GET' \
  'http://localhost:8080/api/auth/me' \
  -H 'accept: text/plain' \
  -H 'Authorization: Bearer <token>'
```

### Task Service

**Создание задачи:**

POST api/tasks
```cmd
curl -X 'POST' \
  'http://localhost:8080/api/tasks' \
  -H 'accept: text/plain' \
  -H 'Authorization: Bearer <token>' \
  -H 'Content-Type: application/json' \
  -d '{
  "name": "Task",
  "description": "Some text",
  "deadline": "2025-07-31T00:00:00.182Z"
}'
```

**Получение списка задач с фильтрами:**

GET api/tasks
```cmd
curl -X 'GET' \
  'http://localhost:8080/api/tasks?Status=2&PerformerId=5&CreatedBy=2&Page=3&PageSize=10' \
  -H 'accept: text/plain' \
  -H 'Authorization: Bearer <token>'
```

**Получение задачи по ID:**

GET api/tasks/{id}
```cmd
curl -X 'GET' \
  'http://localhost:8080/api/tasks/1' \
  -H 'accept: text/plain' \
  -H 'Authorization: Bearer <token>'
```

**Обновление задачи:**

PUT api/tasks/{id}
```cmd
curl -X 'POST' \
  curl -X 'PUT' \
  'http://localhost:8080/api/tasks/1' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer <token>' \
  -H 'Content-Type: application/json' \
  -d '{
  "name": "string",
  "description": "string",
  "status": 3
}'
```

**Назначение исполнителя:**

PUT api/tasks/{id}/assign
```cmd
curl -X 'PUT' \
  'http://localhost:8080/api/tasks/1/assign' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer <token>' \
  -H 'Content-Type: application/json' \
  -d '{
  "performerId": 1
}'
```

**Мягкое удаление задачи:**

DELETE api/tasks/{id}
```cmd
curl -X 'DELETE' \
  'http://localhost:8080/api/tasks/1' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer <token>'
```

### Notification Service

**Создание уведомления (отправка по SignalR):**

POST api/notifications
```cmd
curl -X 'POST' \
  'http://localhost:8080/api/notifications' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer <token>' \
  -H 'Content-Type: application/json' \
  -d '{
  "userId": 1,
  "title": "Задача",
  "message": "Сообщение"
}'
```

**Получение списка уведомлений:**

GET api/notifications/{userId}
```cmd
curl -X 'GET' \
  'http://localhost:8080/api/notifications/1' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer <token>'
```

**Прочтение уведомления:**

PUT api/notifications/{id}/mark-as-read
```cmd
curl -X 'PUT' \
  'http://localhost:8080/api/notifications/2/mark-as-read' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer <token>'
```
