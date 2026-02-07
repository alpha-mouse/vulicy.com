# Vulicy

Карта што адлюстроўвае колькі ў нас яшчэ недарэчных каляніяльных назоваў вуліц, і прапановы па іх перайменаваньню.

## Патрабаваньні

- **.NET 10 SDK** - [Спампаваць тут](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js** (рэкамендуецца LTS версія, 18.x ці вышэй) - [Спампаваць тут](https://nodejs.org/)
- **PostgreSQL** з пашырэньнем **PostGIS** (версія 18 ці вышэй) - [Спампаваць тут](https://www.postgresql.org/download/)

## Наладка

### 1. Наладка базы даных

Пераканайцеся, што PostgreSQL запушчаны, і стварыце базу даных `vulicy`:

```bash
psql -U postgres -c "CREATE DATABASE vulicy;"
```

Пачатковае напаўненьне базы даных.. шчыра кажучы прасьцей узяць бэкап продаўскай базы, разгарнуць лякальна, ды ананімізаваць табліцу карыстальнікаў. 

### 2. Наладка бэкенда

Перайдзіце ў `Vulicy.Web` і наладзьце параметры лякальнай распрацоўкі:

1. Адкрыйце `Vulicy.Web/appsettings.Development.json`
2. Паўсюль дзе пустыя значэньні, будуць патрэбныя ключы.
Без сакрэтаў Discourse у прынцыпе можна жыць, калі не ўзаемадзейнічаць з форумам.
Без сакрэтаў AWS у прынцыпе таксама, але трэба будзе выкінуць AuditPersistenceHostedService.

### 3. Наладка фронтэнда

Перайдзіце ў каталог `Vulicy.UI` і ўсталюйце залежнасьці:

```bash
cd Vulicy.UI
npm install
```

## Запуск

### Запуск бэкенда (ASP.NET API)

```bash
cd Vulicy.Web
dotnet run
```

API будзе дасяжны па адрасе `http://localhost:5165` (або на порце, указаным у вашай канфігурацыі).

### Запуск фронтэнда (React + Vite)

У новым тэрмінале:

```bash
cd Vulicy.UI
npm run dev
```

Фронтэнд будзе дасяжны па адрасе `http://localhost:5173` (стандартны порт Vite).

## Працоўны працэс распрацоўкі

1. Запусціце PostgreSQL
2. Запусціце бэкенд API (`dotnet run` у `Vulicy.Web`)
3. Запусціце дэв-сервер фронтэнда (`npm run dev` у `Vulicy.UI`)
4. Адкрыйце `http://localhost:5173`

## Структура праекта

- **Vulicy.Web** - ASP.NET Core 10 Web API
- **Vulicy.UI** - React + Vite фронтэнд
- **Vulicy.DB** - База даных з Entity Framework Core
- **Vulicy.Domain** - Мадэлі дамэна
- **Vulicy.Services** - Сэрвісы
- **Vulicy.Tests** - Юніт тэсты (зусім крыху)

## Дадатковыя каманды

### Frontend

- `npm run build` - Зборка для прадукцыі
- `npm run lint` - Запуск ESLint
- `npm run preview` - Прадпрагляд прадукцыйнай зборкі лакальна

### Backend

- `dotnet build` - Зборка рашэння
- `dotnet test` - Запуск тэстаў


## Інфраструктура на продзе

### forum.vulicy.com
- Hetzner vpc
- Discourse на гэтай vpc падняты згодна з https://github.com/discourse/discourse/blob/main/docs/INSTALL-cloud.md
- cdn files on aws s3 Stockholm (eu-north-1) exposed via CloudFront forum-cdn.vulicy.com
- backups on aws s3
- google project for OAuth2 on vulicy.com@gmail.com
- mailgun for emails

### vulicy.com
- Hetzner vpc
- coolify on port 8000. Каб зь ім паўзаемадзейнічаць можна адкрыць порт на фаерволе на Hetzner, ці як раблю я, падключыцца да сэрвэра па ssh і замапіць порт каб ён быў дасяжны лякальна
- postgis db from postgis/postgis:18-3.6-alpine image
- db backed up to aws s3 Sydney (ap-southeast-2) bucket
- application as docker images at vulicy/vulicy-app
- audit aws DynamoDB tables in Stockholm (eu-north-1)
- растравы слой карты з maptiler
- Sentry для логінгу памылак