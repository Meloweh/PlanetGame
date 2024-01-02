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
- Formel für elastischen Stoß einsetzen und anpassen bis es optisch ansprechend wirkt
![6](https://github.com/Meloweh/PlanetGame/assets/49780209/96b2fa57-b10f-4930-806b-c847feaf9c3a)
### Ergebnis
https://github.com/Meloweh/PlanetGame/assets/49780209/d1b0d073-9772-4fb3-920b-994205b1c96d
### Herausforderungen
Ich habe mich dazu entschieden den Kollisionstest aus Zeitgründen in einer Schleife innerhalb des Kernels zu testen. Es geht sicherlich besser, aber mir fehlt die Zeit genauer darauf einzugehen.

## V4
### Meilensteine
- Planeten spawnen
- Gravitation aller Planeten beachten
- Partikel auf Planet kleben lassen
### Ansatz
- Spawnkernel einführen
![7](https://github.com/Meloweh/PlanetGame/assets/49780209/5e11d62b-1c5b-4db4-b38b-cccf3fb31d3f)
- Gravitation aller Planeten beachten
![8](https://github.com/Meloweh/PlanetGame/assets/49780209/8153c2a0-c6d8-4f7f-8668-bb445e62873a)
- Partikel auf Planet kleben lassen
![9](https://github.com/Meloweh/PlanetGame/assets/49780209/6dc8cf80-407b-4873-943e-3e36cbd987e4)
### Ergebnis
https://github.com/Meloweh/PlanetGame/assets/49780209/39da92d5-9211-4f77-a17c-143b9954ee31
### Herausforderungen
Partikel verschwanden mit naiven Ansätzen des Kontakttests durch Überlappung bzw. Besetzung derselben Position. Mit dem Bresenham Schnitt konnte ich das Problem umgehen.

## V5
### Meilensteine
- Partikel um den Planeten “dickflüssig” machen
### Ansatz
- Für alle Partikel pro Planet, ist ein Nachbarpixel frei und liegt dieser näher zum Planetzentrum, erlaube einen "Sprung" um n (belassen auf n=2) Pixel.
### Ergebnis
https://github.com/Meloweh/PlanetGame/assets/49780209/95d43266-9a70-4725-9b25-74e05e37348c
### Herausforderungen
- Diverse Ansätze bin ich zu kompliziert angegangen. Der oben genannte Ansatz scheint trivial, ist allerdings ein "brute-force" Ansatz.
- Hier ein paar Bugs und Fehlschläge:
https://github.com/Meloweh/PlanetGame/assets/49780209/edb166b7-82c7-4270-a2f5-054dc08b07ea

https://github.com/Meloweh/PlanetGame/assets/49780209/55329615-a9df-478b-b7f1-4aa524760f1e

https://github.com/Meloweh/PlanetGame/assets/49780209/43cf5c34-9daa-45a7-b30c-06be6baa091b

https://github.com/Meloweh/PlanetGame/assets/49780209/fff5d1a8-5439-4381-bccd-6e3ec9ddc448

https://github.com/Meloweh/PlanetGame/assets/49780209/5bae7be1-15b5-4351-ba30-6264b683ffe2

https://github.com/Meloweh/PlanetGame/assets/49780209/9da37169-894b-4ecb-b575-bb85eef11d5c

https://github.com/Meloweh/PlanetGame/assets/49780209/94e6ab43-0f2d-4ccb-bc56-4c02eb0a9a3c

## V6
### Meilensteine
- Berge/Täler ermöglichen
- Höhenlimit für Berge
### Ansatz
- “Druck”-Kette einführen
- Starter-Viskosität erlauben
![image](https://github.com/Meloweh/PlanetGame/assets/49780209/2225e651-6852-4640-8995-bae7d4adf0f5)
### Herausforderungen
- Problem:
“Druck” wird nicht gleichmäßig um den Planeten herum berechnet
- Mögliche Lösungsideen:
“Tunnelbildung”, Kraterkanten, etc. durch Bresenham+Negativdruck verhindern
- Hinweis: Der Ansatz wurde aus Zeittechnischen Gründen nicht mehr fortgesetzt.
### Dokumentierte Ansätze
https://github.com/Meloweh/PlanetGame/assets/49780209/c5947266-b2ac-4d44-8108-036900b6db91


https://github.com/Meloweh/PlanetGame/assets/49780209/b02ceba6-2268-4d73-8a1d-822b4b2f1b38


https://github.com/Meloweh/PlanetGame/assets/49780209/85ca485b-87bf-4b6a-9847-2064be194bdd


https://github.com/Meloweh/PlanetGame/assets/49780209/84dd38ba-1c58-4bac-a401-c7529faf0e59


https://github.com/Meloweh/PlanetGame/assets/49780209/b56536b8-5a85-446c-8dff-238098cf956a

## V7
### Meilensteine
- Auftrag: “Eye Candy” umsetzen:
  - Part 1: Partikel leuchten lassen
### Ansatz
Pixel im Radius von 1 "leere" Pixel anfärben und "fade" Animation einführen, um Schleiereffekt zu erzeugen.
### Ergebnis
![image51](https://github.com/Meloweh/PlanetGame/assets/49780209/f0d29d55-364f-4ec0-8c7a-e860e21608ae)

- Umgesetzt:
  - Feuer-Effekt eingeführt und wird heller/dunkler, je nach Geschwindigkeit
  - Berg und Kraterbildung durch Zähleransatz gelöst:
  - Ein Partikel auf dem Planeten angekommen darf sich 100 Steps bewegen. Dieser Wert kann z.B. bei Events, wie Einschläge einfach resettet werden, um die Animation wiederzubeleben.
  - Position update fix: Attribute wird jetzt mit float2 anstelle von int2 geupdated
  - Gravitation wurde erhöht, damit keine kantige Flugbahn entsteht
  - Auflösung auf HD reduziert

https://github.com/Meloweh/PlanetGame/assets/49780209/2167a6f1-7420-4698-8a6b-f051947e0c0c

### Herausforderungen
Memory Leak machte die Pixel "glitchy" und crashte meinen Computer. Vermutlich durch eine Race-Condition, wenn ich auf gleiche Attribute von Pixel in einem parallelisiertem Umfeld in einer Schleife zugreife.

## V7.2
### Meilensteine
- Flamme durch relative Arealbeleuchtung um Partikel
### Ansatz
In einer Schleife, "leere" Pixel in Abhängigkeit von der Distanz zum fokussierten Pixel, von Geschwindigkeit und Masse unterschiedlich blass/durchsichtig leuchten lassen.
### Ergebnis
- Hard Lock Memory Leak behoben

![image](https://github.com/Meloweh/PlanetGame/assets/49780209/1cbee663-efbf-48a3-8820-c49f0e1b796b)

https://github.com/Meloweh/PlanetGame/assets/49780209/07c8f8eb-f998-40ca-bedd-f90bacd5e7a1

### Herausforderungen
Die Skalierung des Glow-Effekts in Abhängigkeit von der Masse des Pixels führte ebenfalls zu einem Memory Leak. Diesen konnte ich zwar durch das Variieren vom Aufbau meiner Operationen beheben, habe dadurch aber nicht nachvollziehen können warum das Problem an erster Linie entstanden ist.

## V8
### Meilensteine
- Auftrag: “Eye Candy” umsetzen:
  - Part 2: Einschlag simulieren
### Ansatz
### Ergebnis
### Herausforderungen
