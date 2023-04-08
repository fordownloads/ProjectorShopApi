# Дипломный проект
Разработка  информационной системы «Продажа проекторов» на платформе .NET 6.0 с микросервисами и Android при использовании Visual Studio 2022.

**Репозиторий с микросервисами**

## Запуск проекта
1. Установите Docker
2. Создайте хранилища:
```sh
docker volume catalogvolume
docker volume authvolume
docker volume cachevolume
```
3. Через Visual Studio или через `docker-compose up` запустите проект

*Swagger находится на http://localhost:8080/swagger/index.html*

## Тестирование
1. Запуск баз данных:
```sh
docker run --name test-postgres -e POSTGRES_PASSWORD=test -e POSTGRES_DATABASE=test -e POSTGRES_USER=test -p 5532:5432 -d postgres
docker run --name test-redis -p 6380:6379 -d redis/redis-stack:latest
```
2. Visual Studio -> ПКМ по решению -> Выполнить тесты

## Деплой на удаленный сервер
```sh
docker-compose build
docker save apigateway:latest catalogservice:latest authservice:latest cartservice:latest | ssh -C <ssh-user>@<ssh-server> docker load
scp -pr docker-compose.yml <ssh-user@ssh-server>:~/.deploy/docker-compose.yml
ssh -C <ssh-user@ssh-server> sh -c "cd ~/.deploy;docker-compose up -d --force-recreate"
```
*<<ssh-user@ssh-server>> меняем на свое*

## Конфиг nginx
`server {...`

```
client_max_body_size 12M;

location /api/ {
	proxy_pass http://localhost:8080/api/;
}
```

`...}`