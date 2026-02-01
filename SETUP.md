# DateSantiere - Setup și Deployment

## 🎉 Proiectul este pregătit!

### ✅ Ce am realizat:

1. **Aplicație ASP.NET Core 8.0 completă**
   - 3 proiecte (Models, Data, Web)
   - Entity Framework Core cu SQLite
   - ASP.NET Identity pentru autentificare
   - Structură MVC completă

2. **Funcționalități implementate:**
   - Pagină principală cu statistici
   - Căutare șantiere cu filtre
   - Detalii complete pentru fiecare șantier
   - Newsletter subscription
   - Formular de contact
   - Design responsive Bootstrap 5

3. **Baza de date:**
   - SQLite (pentru development local ușor)
   - Toate tabelele create
   - Seed data cu admin user

## 🚀 Cum să rulezi local:

```powershell
cd c:\Projects\dotnet\DateSantiere.Web
dotnet run
```

Apoi deschide browserul la: **http://localhost:5000**

## 📤 Upload pe GitHub:

### Opțiunea 1: GitHub CLI (recomandat)

```powershell
# Instalează GitHub CLI dacă nu ai: https://cli.github.com/
gh auth login
gh repo create DateSantiere --public --source=. --remote=origin
git push -u origin master
```

### Opțiunea 2: Manual

1. Mergi pe https://github.com/new
2. Creează un repository nou numit "DateSantiere"
3. **NU** bifa "Initialize with README"
4. După crearea repository-ului, rulează:

```powershell
cd c:\Projects\dotnet
git remote add origin https://github.com/USERNAME/DateSantiere.git
git branch -M main
git push -u origin main
```

Înlocuiește `USERNAME` cu username-ul tău GitHub.

## 📝 Cont Admin Implicit:

- **Email:** admin@datesantiere.ro
- **Password:** Admin@123456

**⚠️ IMPORTANT:** Schimbă parola după prima autentificare!

## 🔧 Configurare pentru producție:

### 1. Schimbă baza de date de la SQLite la SQL Server:

În [appsettings.json](DateSantiere.Web/appsettings.json):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=DateSantiere;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```

În [Program.cs](DateSantiere.Web/Program.cs), schimbă:
```csharp
options.UseSqlite(connectionString, ...)
```
cu:
```csharp
options.UseSqlServer(connectionString, ...)
```

### 2. Configurează variabilele de mediu:

- **ConnectionStrings__DefaultConnection**
- **Stripe__SecretKey**
- **Stripe__PublishableKey**
- **Email__SmtpHost**, **Email__SmtpUser**, etc.

## 📊 Următorii pași:

1. **Import date** - Importă cele 109,582 șantiere existente
2. **Admin panel** - Interfață pentru gestionare
3. **Stripe** - Sistemul de plăți
4. **Email service** - Newsletter și notificări
5. **Hartă interactivă** - Google Maps
6. **Export** - Excel/PDF pentru rapoarte

## 🌐 Deployment:

### Azure App Service:
1. Creează App Service în Azure Portal
2. Configurează connection string
3. Deploy folosind Visual Studio sau Azure CLI

### IIS (Windows Server):
1. Instalează .NET 8 Hosting Bundle
2. Publică: `dotnet publish -c Release`
3. Configurează site în IIS

### Docker:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY published/ /app
WORKDIR /app
ENTRYPOINT ["dotnet", "DateSantiere.Web.dll"]
```

## 📞 Contact:

Pentru întrebări: office@datesantiere.ro
