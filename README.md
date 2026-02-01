# DateSantiere - Platforma de Căutare Șantiere

## Descriere
DateSantiere este o platformă modernă pentru căutarea și gestionarea informațiilor despre șantiere de construcții din România. Oferă acces la peste 109,000 de șantiere active cu informații detaliate despre proiecte, beneficiari, localizare și valori estimate.

## Tehnologii
- **Backend**: ASP.NET Core 8.0 MVC
- **Frontend**: Bootstrap 5, Razor Pages
- **Database**: SQL Server / Azure SQL
- **Autentificare**: ASP.NET Core Identity
- **Plăți**: Stripe

## Funcționalități

### Publice
- ✅ Căutare avansată șantiere (după județ, categorie, status)
- ✅ Vizualizare detalii complete șantier
- ✅ Filtrare și sortare rezultate
- ✅ Newsletter pentru șantiere noi
- ✅ Formular de contact

### Pentru Utilizatori Autentificați
- 📋 Salvare șantiere favorite
- 🔍 Salvare căutări personalizate
- 📊 Dashboard personalizat
- 📧 Notificări email pentru șantiere noi
- 💾 Export date (Excel, PDF)

### Admin Panel
- ➕ Adăugare/Editare/Ștergere șantiere
- 👥 Gestionare utilizatori
- 📈 Statistici și rapoarte
- 💳 Gestionare abonamente
- 📬 Răspuns la mesaje contact

## Instalare și Configurare

### Cerințe
- .NET 8.0 SDK sau superior
- SQL Server 2019+ sau Azure SQL Database
- Visual Studio 2022 sau VS Code

### Pași de instalare

1. **Clonează repository-ul**
```bash
git clone https://github.com/yourusername/DateSantiere.git
cd DateSantiere
```

2. **Configurează connection string**
Editează `appsettings.json` și setează connection string-ul pentru baza de date:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=DateSantiere;Trusted_Connection=True;"
}
```

3. **Rulează migrările**
```bash
cd DateSantiere.Web
dotnet ef database update
```

4. **Configurează Stripe (opțional)**
Editează `appsettings.json` cu cheile tale Stripe:
```json
"Stripe": {
  "PublishableKey": "pk_test_YOUR_KEY",
  "SecretKey": "sk_test_YOUR_KEY"
}
```

5. **Rulează aplicația**
```bash
dotnet run
```

Aplicația va fi disponibilă la: `https://localhost:5001`

## Structura Proiectului

```
DateSantiere/
├── DateSantiere.Models/      # Modele de date
├── DateSantiere.Data/         # DbContext și repositories
└── DateSantiere.Web/          # Aplicația web MVC
    ├── Controllers/           # Controllers
    ├── Views/                 # Razor views
    ├── wwwroot/              # Fișiere statice
    └── Areas/                # Admin area
```

## Cont Admin Implicit

După prima rulare, vei avea un cont admin creat automat:
- **Email**: admin@datesantiere.ro
- **Parolă**: Admin@123456

**IMPORTANT**: Schimbă parola imediat după prima autentificare!

## Deployment

### Azure App Service
1. Creează App Service și SQL Database în Azure
2. Configurează connection string-ul în Azure Portal
3. Deploy folosind:
   - Visual Studio (right-click > Publish)
   - Azure CLI
   - GitHub Actions

### IIS
1. Publică aplicația: `dotnet publish -c Release`
2. Copiază fișierele din `bin/Release/net8.0/publish/` pe server
3. Creează site în IIS
4. Configurează Application Pool pentru .NET Core

## Contribuții
Contribuțiile sunt binevenite! Te rugăm să:
1. Fork repository-ul
2. Creează un branch pentru feature-ul tău
3. Commit schimbările
4. Push pe branch
5. Creează un Pull Request

## Licență
Acest proiect este proprietatea Callinvest SRL.

## Contact
- **Website**: https://www.datesantiere.ro
- **Email**: office@datesantiere.ro
- **Telefon**: 0766.183.434

## TO-DO

- [ ] Implementare sistem de notificări push
- [ ] Integrare hartă interactivă (Google Maps/Leaflet)
- [ ] Export Excel/PDF
- [ ] API REST pentru integrări
- [ ] Aplicație mobilă (Flutter/React Native)
- [ ] Import masiv date din Excel
- [ ] Dashboard analytics avansat
- [ ] Sistem de raportare și statistici
