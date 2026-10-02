# مرحله اول: طراحی معماری و ساخت زیرساخت

## ۱. مشخصات پروژه

| بخش              | تکنولوژی                 |
| ---------------- | ------------------------ |
| Frontend         | ASP.NET Core Razor Pages |
| Backend          | .NET 9                   |
| Architecture     | Clean Architecture       |
| Message Broker   | RabbitMQ 4               |
| Messaging Client | RabbitMQ.Client          |
| Database         | SQL Server               |
| ORM              | EF Core                  |
| Logging          | Serilog                  |
| Real-time UI     | SignalR (در مراحل بعد)   |
| Infrastructure   | Docker Compose           |

## ۲. رفتار سیستم

فرض کن کاربر از طریق Razor Pages یک Notification ایجاد می‌کند.

مثلاً:

JSON

```
{
  "recipient": "ali@example.com",
  "message": "Welcome to our application",
  "type": "Email"
}
```

جریان پردازش:

Razor Pages

دریافت درخواست کاربر

Application Layer

اعتبارسنجی و اجرای Use Case

SQL Server + Outbox

ثبت Notification و رویداد Outbox

RabbitMQ

Exchange → Queue → Consumer

Email

Worker

SMS

Worker

Push

Worker

در نسخه اول، Workerها به‌جای ارسال واقعی، عملیات را در Console ثبت می‌کنند.

نکته معماری: Outbox را از ابتدا در طراحی در نظر می‌گیریم، اما ابتدا ارتباط مستقیم Producer و Consumer را یاد می‌گیریم. بعد Outbox را اضافه می‌کنیم تا مشکل Dual Write را عملاً حل کنیم.

## ۳. ساختار Solution

ساختار هدف ما این است:

```
NotificationSystem/
│
├── src/
│   │
│   ├── NotificationSystem.Web/
│   │   ├── Pages/
│   │   ├── wwwroot/
│   │   └── Program.cs
│   │
│   ├── NotificationSystem.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── Events/
│   │
│   ├── NotificationSystem.Application/
│   │   ├── Abstractions/
│   │   ├── Features/
│   │   └── DTOs/
│   │
│   ├── NotificationSystem.Infrastructure/
│   │   ├── Messaging/
│   │   ├── Persistence/
│   │   └── DependencyInjection.cs
│   │
│   └── NotificationSystem.Workers/
│       ├── Email/
│       ├── Sms/
│       └── Push/
│
├── tests/
│   ├── UnitTests/
│   └── IntegrationTests/
│
├── docker-compose.yml
└── NotificationSystem.sln
```

فعلاً همه Workerها را در یک پروژه Worker نگه می‌داریم و در مراحل بعد، با مشخص شدن نیازهای مقیاس‌پذیری، درباره جداسازی سرویس‌ها تصمیم می‌گیریم.


## ۴. ساخت Solution و پروژه‌ها

اولین قدم عملی را شروع کنیم.

در ترمینال اجرا کن:

Bash

```
mkdir NotificationSystem
cd NotificationSystem

dotnet new sln -n NotificationSystem

mkdir src
mkdir tests

cd src

dotnet new webapp -n NotificationSystem.Web
dotnet new classlib -n NotificationSystem.Domain
dotnet new classlib -n NotificationSystem.Application
dotnet new classlib -n NotificationSystem.Infrastructure
dotnet new worker -n NotificationSystem.Workers

cd ..
```

حالا پروژه‌ها را به Solution اضافه می‌کنیم:

Bash

```
dotnet sln add src/NotificationSystem.Web
dotnet sln add src/NotificationSystem.Domain
dotnet sln add src/NotificationSystem.Application
dotnet sln add src/NotificationSystem.Infrastructure
dotnet sln add src/NotificationSystem.Workers
```

### تنظیم Project Referenceها

وابستگی‌ها را به این شکل تعریف می‌کنیم:

Bash

```
dotnet add src/NotificationSystem.Application reference src/NotificationSystem.Domain

dotnet add src/NotificationSystem.Infrastructure reference src/NotificationSystem.Application

dotnet add src/NotificationSystem.Infrastructure reference src/NotificationSystem.Domain

dotnet add src/NotificationSystem.Web reference src/NotificationSystem.Application

dotnet add src/NotificationSystem.Web reference src/NotificationSystem.Infrastructure

dotnet add src/NotificationSystem.Workers reference src/NotificationSystem.Application

dotnet add src/NotificationSystem.Workers reference src/NotificationSystem.Infrastructure
```

نکته: Domain هیچ وابستگی به لایه‌های دیگر ندارد. این اصل را در تمام مراحل حفظ می‌کنیم.

## ۵. نصب پکیج‌ها

برای RabbitMQ:

Bash

```
dotnet add src/NotificationSystem.Infrastructure package RabbitMQ.Client

dotnet add src/NotificationSystem.Workers package RabbitMQ.Client
```

برای SQL Server و EF Core:

Bash

```
dotnet add src/NotificationSystem.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer

dotnet add src/NotificationSystem.Infrastructure package Microsoft.EntityFrameworkCore.Design
```

برای Logging:

Bash

```
dotnet add src/NotificationSystem.Web package Serilog.AspNetCore

dotnet add src/NotificationSystem.Workers package Serilog.Extensions.Hosting
```

برای EF Core بهتر است نسخه پکیج‌ها با نسخه SDK و Runtime مورد استفاده هماهنگ باشد.

## ۶. اجرای RabbitMQ با Docker

در ریشه Solution فایل `docker-compose.yml` بساز:

YAML

```
services:

  rabbitmq:
    image: rabbitmq:4-management
    container_name: notification-rabbitmq

    hostname: notification-rabbitmq

    ports:
      - "5672:5672"
      - "15672:15672"

    environment:
      RABBITMQ_DEFAULT_USER: notification_admin
      RABBITMQ_DEFAULT_PASS: StrongPassword123!

    volumes:
      - rabbitmq_data:/var/lib/rabbitmq

    restart: unless-stopped

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: notification-sqlserver

    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "YourStrong!Passw0rd"
      MSSQL_PID: Developer

    ports:
      - "1433:1433"

    volumes:
      - sqlserver_data:/var/opt/mssql

    restart: unless-stopped

volumes:
  rabbitmq_data:
  sqlserver_data:
```

این رمزها صرفاً برای محیط توسعه محلی هستند؛ در محیط واقعی باید از Secret Management استفاده کنیم.

اجرا:

Bash

```
docker compose up -d
```

و بررسی:

Bash

```
docker compose ps
```

پنل RabbitMQ:

![](https://www.google.com/s2/favicons?domain=http://localhost:15672\&sz=32)

[RabbitMQ Management](http://localhost:15672)

اطلاعات ورود:

```
Username: notification_admin
Password: StrongPassword123!
```

در این پنل می‌توانی Exchangeها، Queueها، Bindingها، Connections، Channels و پیام‌ها را مشاهده کنی.


## ۷. یک نکته مهم درباره نسخه .NET

یک نکته را قبل از ادامه در نظر بگیر: طبق سیاست رسمی مایکروسافت، پشتیبانی .NET 9 در تاریخ ۱۰ نوامبر ۲۰۲۶ به پایان می‌رسد. .NET 10 نسخه LTS است و تا نوامبر ۲۰۲۸ پشتیبانی می‌شود.

![](https://www.google.com/s2/favicons?domain=https://dotnet.microsoft.com\&sz=32)

.NET

+1

پیشنهاد مهندسی من این است که اگر محدودیت خاصی نداری، همین پروژه را با .NET 10 بسازیم. مفاهیم RabbitMQ تغییری نمی‌کنند و پروژه از ابتدا روی نسخه پشتیبانی‌شده قرار می‌گیرد. البته اگر اصرار به .NET 9 داری، با همان ادامه می‌دهیم.

## ۸. اولین تست پروژه

بعد از اجرای Docker، این دستورات را اجرا کن:

Bash

```
dotnet --version

docker compose ps

docker logs notification-rabbitmq

dotnet build NotificationSystem.sln
```

انتظار داریم:

* SDK نسخه موردنظر نصب باشد.

* RabbitMQ در وضعیت Running باشد.

* SQL Server آماده اتصال باشد.

* Solution بدون خطا Build شود.

## ۹. اولین مفهومی که قرار است عمیق یاد بگیریم

قبل از نوشتن Producer می‌خواهم این چهار مفهوم را کاملاً درک کنیم:

| مفهوم      | کاربرد                    |
| ---------- | ------------------------- |
| Connection | ارتباط TCP با RabbitMQ    |
| Channel    | کانال AMQP روی Connection |
| Exchange   | دریافت و مسیریابی پیام    |
| Queue      | نگهداری پیام تا زمان مصرف |

یک نکته کلیدی که بعداً در معماری Production اهمیت زیادی پیدا می‌کند:

هر Request نباید یک Connection جدید به RabbitMQ ایجاد کند.

مدیریت طول عمر Connection و Channel، بازیابی اتصال و مدیریت هم‌زمانی را به‌صورت اصولی طراحی خواهیم کرد.

فعلاً این مرحله را اجرا کن و نتیجه را بفرست.

به‌خصوص خروجی `dotnet build` و `docker compose ps` را می‌خواهم ببینم.

بعد از تأیید این مرحله، می‌رویم سراغ طراحی اولین Messaging Infrastructure و ساخت Producer واقعی؛ با توضیح کامل Connection، Channel و Queue، نه صرفاً کپی کردن کد.
