# اجرای RabbitMQ با Docker

## دریافت Image

```bash
docker pull rabbitmq:4-management
```

## ساخت و اجرای Container

```bash
docker run -d --name rabbit-server -p 5672:5672 -p 15672:15672 rabbitmq:4-management
```

### توضیح Portها

```bash
-p 5672:5672
```

پورت `5672` برای ارتباط برنامه‌ها با RabbitMQ استفاده می‌شود.

ساختار کلی:

```text
-p HOST_PORT:CONTAINER_PORT
```

یعنی:

```text
5672 روی سیستم → 5672 داخل Container
```

---

```bash
-p 15672:15672
```

پورت `15672` برای **RabbitMQ Management UI** استفاده می‌شود.

یعنی می‌توانیم پنل مدیریت RabbitMQ را از طریق آدرس زیر باز کنیم:

```text
http://localhost:15672
```

ساختار کلی:

```text
15672 روی سیستم → 15672 داخل Container
```
