# Planetenspiel in einer Partikelsimulation

## Projektbeschreibung
- 2D Space Pixel Planetenspiel
- Welt aus Simulierten Pixel wie in Noita
- Physik wie planetare Gravitation
- Interaktion mit Körper (z.B. Kollisionen)

## V1
### Rahmenbedingungen
- Performance > Umstand
- Aufgabe 1: “Zeigen, dass wir Pixel anzeigen, bewegen und miteinander interagieren lassen können”
- Ansatz: Verwende Unitiy’s “Compute Shader”
### Meilensteine
- Pixel anzeigen
- Pixel bewegen
- Implementierbarkeit von Logik beweisen (für Pixel Interaktionen)

### Ansatz
- Umsetzung einer "RenderTexture", also einer 2D Pixelfläche, die wir mit dem Compute Shader ansprechen, um Pixel zu verschieben/bewegen.
![3](https://github.com/Meloweh/PlanetGame/assets/49780209/b6fb7cc7-b16a-45c4-91ab-c86521d80d4f)
![4](https://github.com/Meloweh/PlanetGame/assets/49780209/e0c6bada-f56f-43ef-bff0-0737195b2216)
![5](https://github.com/Meloweh/PlanetGame/assets/49780209/0917783d-eee6-41da-9c73-cfaea87b6489)

### Ergebnis
https://github.com/Meloweh/PlanetGame/assets/49780209/beb1ea19-65f4-4d7a-ba61-04941d022061

### Herausforderungen
- Parallelisierte Architektur ist ungewohnt und die Dokumentation hilft beim Verständnis nur beschränkt
  - Es gibt keine Hilfestellung bei Race Conditions während ich versucht habe mich durch kleine Tests einzuarbeiten

## V2
### Meilensteine
- Gravitation in Richtung eines Punktes im Raum
### Ansatz
- Formel für gravitational Attraction einsetzen
- (Zur Vereinfachung erstmal nur für Partikelkörper, nicht für Planetenkörper)
![image8](https://github.com/Meloweh/PlanetGame/assets/49780209/10f4e19f-5a3f-46cd-a6dd-740ad9919c7d)
### Ergebnis
https://github.com/Meloweh/PlanetGame/assets/49780209/c8b0752b-4a63-40e4-8b98-371d76514cbf
### Herausforderungen
- Kantige Kurven:
  - Die Flugbahn ist teilweise quadratisch
  - Möglicherweise durch Rundungsfehler

## V3
### Meilensteine
- Partikelkollisionen
### Ansatz
### Ergebnis
### Herausforderungen

## V4
### Meilensteine
### Ansatz
### Ergebnis
### Herausforderungen

## V5
### Meilensteine
### Ansatz
### Ergebnis
### Herausforderungen

## V6
### Meilensteine
### Ansatz
### Ergebnis
### Herausforderungen

## V7
### Meilensteine
### Ansatz
### Ergebnis
### Herausforderungen

## V8
### Meilensteine
### Ansatz
### Ergebnis
### Herausforderungen
