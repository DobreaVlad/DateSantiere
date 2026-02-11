# Docker Development cu Hot-Reload

## 🚀 Pornire rapidă

```bash
# Construiește și pornește containerul
docker-compose -f docker-compose.dev.yml up --build

# Sau în background
docker-compose -f docker-compose.dev.yml up -d --build
```

## ✨ Caracteristici

- **Hot-reload automat** - Modifici codul → vezi instant în browser
- **Volume mounts** - Codul local e montat în container
- **dotnet watch** - Recompilare automată la modificări

## 📝 Comenzi utile

```bash
# Start
docker-compose -f docker-compose.dev.yml up

# Stop
docker-compose -f docker-compose.dev.yml down

# Rebuild complet
docker-compose -f docker-compose.dev.yml up --build --force-recreate

# Vezi logs
docker-compose -f docker-compose.dev.yml logs -f

# Intră în container
docker-compose -f docker-compose.dev.yml exec web bash
```

## 🌐 Acces

- Aplicație: http://localhost:5000

## 🔥 Testare Hot-Reload

1. Pornește containerul
2. Deschide http://localhost:5000
3. Modifică orice fișier .cs sau .cshtml
4. Salvează
5. Refresh browser - vezi modificările instant!

## ⚠️ Note

- Nu uita să oprești aplicația locală (dotnet run) înainte
- Baza de date SQLite e persistentă (e montată din local)
- Modificările în Views (.cshtml) apar instant
- Modificările în Controllers/Models necesită ~2-3 secunde
