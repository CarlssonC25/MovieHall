# MovieHall

Eine Bibliothek für Filme und Serien (Anime), mit der man Inhalte nach dem Anschauen kategorisieren, bewerten, suchen und mit Notizen versehen kann.  
Die minimale Auflösung des Projekts beträgt **1920 × 1080 (Full HD) oder höher**.

---

## Installation

### Voraussetzungen
- Visual Studio 2022 oder neuer  
- Docker Desktop (optional, wenn Docker verwendet wird)  
- Git  

---

### Projekt in Visual Studio starten
1. Visual Studio 2022 öffnen  
2. **„Repository klonen“** auswählen  
3. Im Feld **Repositoryspeicherort** diesen Link eintragen:  
   `https://github.com/CarlssonC25/MovieHall`
4. Auf **Klonen** klicken  

---

### Projekt über Docker starten
1. Docker Desktop starten  
2. Die Konsole/Terminal öffnen  
3. Folgenden Befehl eingeben:  
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Str0ng!Passw0rd123" -p 1433:1433 --name MovieHallDB -d mcr.microsoft.com/mssql/server:2022-latest
4. Den Container **MovieHallDB** starten  

---

### Datenbanktabellen erstellen
1. In Visual Studio 2022 die **Paket-Manager-Konsole** öffnen  
(Extras ? NuGet-Paket-Manager ? Paket-Manager-Konsole)  
2. Folgenden Befehl ausführen:  
Update-Database

---

### Projekt starten
1. In Visual Studio 2022 auf **Play (HTTPS)** klicken, um das Projekt auszuführen  

---

### Test-Datenbank importieren
1. Auf das **Zahnrad** oben rechts in der Menüleiste klicken  
2. In den Einstellungen nach **„Datenbank“** suchen  
3. Auf **„Datei wählen“** klicken  
4. Die ZIP-Datei auswählen, die im geklonten Projekt unter  
`MovieHall ? Data`  
liegt  
5. Auf **„Öffnen“** klicken  

---

## Features
- Editierbare Home-Bilder  
- Filme speichern und als Favorit markieren  
- Serien/Anime speichern und von 1 bis 10 bewerten  
- Suche nach Filmen oder Serien/Anime (je nach aktueller Seite)  
- Eigene Suchlinks erstellen  
- Genres hinzufügen und bearbeiten  
- Notizen zu Filmen und Serien/Anime erstellen, um sie später schneller zu finden  
- Datenbank exportieren und importieren  
- Export einer TXT-Kaufliste für Filme oder Serien/Anime, die man noch nicht besitzt  
- Mini-Infos, z. B. wie viele Filme/Serien gespeichert sind oder wie viele Suchergebnisse geladen wurden  
- Unter jedem Film/Serie werden angezeigt:  
- verfügbare Staffeln  
- durchschnittliche Episoden (nur bei Serien/Anime)  
- wie viele Staffeln im Besitz sind  
- Statusanzeige in **Rot, Weiß oder Grün**  
 (kein Besitz / teilweise Besitz / alles im Besitz)  
- Kleine Suchfunktionen direkt bei Filmen und Serien/Anime  

---

## Verwendete Technologien
- ASP.NET Core  
- .NET 8.0  
- Entity Framework Core  
- Bootstrap  
- HTML  
- CSS (SCSS)  
- jQuery  
- MVC (Model-View-Controller)