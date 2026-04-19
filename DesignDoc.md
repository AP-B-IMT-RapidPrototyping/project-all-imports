# PRANK BIRD Design Document

---

## Gameplay Mechanics

### Summary

**Naam:** Prank Bird

**Concept Pitch:**
Jij bent een vogel in een stad. Jouw doel is de levens van voetgangers ontregelen door te poepen op hun spullen, eten te stelen en weg te vliegen voordat ze je kunnen stoppen.

**Genre:** Simulation / Comedy Sandbox

**Doelpubliek:** Casual gamers en comedyliefhebbers, 12+, fans van Untitled Goose Game en Goat Simulator.

---

### Game Flow Samenvatting

1. De speler begint in een **vogelnest** bovenop een gebouw.
2. Vanuit het nest observeert de speler de stad beneden en leest de **takenlijst** (bv. "Poep op een witte auto", "Steel de taart van mevrouw Janssen").
3. De speler vliegt de stad in, voert taken uit via list en interactie-mechanics, en ontsnapt terug naar het nest.
4. Elke voltooide taak geeft punten. Sommige taken zijn optioneel, sommige verplicht om het level te voltooien.
5. Na alle verplichte taken is het level gewonnen.

---

### Basic Look & Feel

- Cartoon gestyleerde 3D-stad, niet realistisch.
- Third person chase camera
- UI: minimalistisch, HUD toont takenlijst, alertniveau van NPC's en een stamina-balk voor vliegen.

---

### Gameplay

**Doel van het spel:**
Voltooi alle verplichte pranks op de takenlijst binnen het tijdslimiet zonder gepakt te worden.

**Hoe win je:**
Alle verplichte taken voltooien en terugkeren naar het nest.

**Hoe verlies je (falen):**

- Je wordt gevangen door een NPC.
- Het tijdslimiet loopt af vóór je alle verplichte taken hebt voltooid.
- Je vliegt in een elektriciteitsdraad of obstakel en valt bewusteloos — verloren draagobject valt op de grond.

---

### Mechanics

#### Controls

| Knop                  | Actie                                             |
| --------------------- | ------------------------------------------------- |
| **Muis**              | Camera rondkijken                                 |
| **Spatie (tikken)**   | Kleine sprong                                     |
| **Spatie (inhouden)** | Vleugels klappen — 3D vliegen activeren           |
| **WASD**              | Bewegen (grond) / Richting sturen (lucht)         |
| **F**                 | Poep-projectiel afschieten — vereist mikken in 3D |
| **E**                 | Object oppakken of loslaten                       |
| **Shift**             | Versnellen                                        |

#### Vliegen

De vogel heeft een **stamina-balk** voor het vliegen. Krachtig vliegen put de balk uit; glijvliegen of zweven herstelt ze langzaam. Als de stamina op is, zakt de vogel naar beneden en moet landen om te herstellen. Het dragen van zware objecten (zoals een taart) verdubbelt het staminaverbruik.

#### Poep-Projectiel (F)

De speler zweeft boven een doelwit. Een kleine pijl verschijnt op het scherm die de valtrajectorie toont. De speler moet het moment correct inschatten en F indrukken. Raak je een NPC, dan schrikt die en loopt naar het gespatte punt, waardoor ze tijdelijk worden afgeleid. Raak je een geparkeerde auto, dan gaat het alarm af.

#### Snavel-Interactie (E)

Wanneer de vogel dicht genoeg bij een interacteerbaar object zweeft (gemarkeerd met een subtiele glinstering), kan de speler E indrukken om het object vast te pakken. Zware objecten beïnvloeden de vliegdynamiek (zie Vliegen). Objecten kunnen worden losgelaten of doelbewust gegooid door E snel dubbel te tikken.

#### Observatiefase

Vóór elke level kan de speler vanuit het nest **naar beneden turen** (camera omlaag richten) om NPC-patronen te bestuderen. NPC's lopen vaste routes maar variëren licht elk cycle. Door patronen te herkennen, kan de speler beslissen welke taak het eerst aanpakken.

#### NPC Alertheid (Zichtveld)

NPC's hebben een conisch zichtveld. Als de vogel te lang boven een NPC cirkelt, vult de NPC's alertmeter zich. Bij volledige alertheid begint de NPC om player te vangen.

#### Infiltratie (Inbraak Mechanic)

Open ramen of dakramen zijn ingang-punten voor gebouwen. De vogel vliegt door een raam naar binnen. Binnenin bevinden zich meerdere dozen, lades of planken die interacteerbare objecten kunnen bevatten.

**Zoekfase (antwoord op risikovraag):**
Binnen de kamer start een **tijdslimiet** (bv. 20 seconden). Dozen/lades zijn gesloten. De speler kan E gebruiken om een doos te openen — elke opening kost ~3 seconden. De speler heeft dus een beperkt aantal pogingen vóór de tijd om is. Correcte dozen (die het gewenste object bevatten) zijn willekeurig bepaald bij level-start.

---

## Verhaal / Immersive Design

### Verhaal & Setting

**Setting:** Een Europese stadsbuurt met smalle straten

**Achtergrondverhaal:**
Je bent gewoon een vogel die zich te veel verveelt, dus heb je besloten jezelf uit te dagen door overlast te veroorzaken.

**Personages:**

- **De Vogel (speler):** Slim. Communiceert via kraakgeluiden.
- **De Postbode:** Loopt vaste routes.

---

### Game Art / Audio

**Visuele Stijl:**

- Low-poly 3D (referentie: Untitled Goose Game).

**Asset List:**

- 1 vogel-character model + animaties (vliegen, landen, oppakken, wandelen)
- 3–5 NPCs (variaties op dezelfde base mesh)
- 1 straat/pleinomgeving (modulaire blokken)
- 2–3 gebouw-interieurs (kamer-ruimtes)
- Props: taart, brood, auto, waslijn met was, dozen, bloempot

**Audio:**

- Achtergrondmuziek: Jazz
- NPC geluiden: Roepen
- Vogelgeluiden: Gekras, vleugels

---

### User Interface

| Element               | Beschrijving                                               |
| --------------------- | ---------------------------------------------------------- |
| **Takenlijst**        | Linksboven, geeft taken weer met checkboxes                |
| **Alertmeter NPC's**  | Kleine iconen boven NPC-hoofden                            |
| **Stamina-balk**      | Rechtsonder                                                |
| **Tijdslimiet**       | Middelste boven, countdown timer                           |
| **Mikpijl (F)**       | Verschijnt alleen tijdens het mikken, toont valtrajectorie |
| **Interactie-prompt** | "E" verschijnt als interacteerbare objecten binnen bereik  |

---

## Pagina 3: Level Ideas

### Level Progressie

**Level 1 — De Buurt (Tutorial)**
Rustige woonstraat. Weinig NPC's, geen hoge alertheid. Doel: poep op een auto, steel een brood terug. Leert basiscontrols, vliegen en F-mechanic.

**Level 2 — Het Marktplein**
Drukker plein met meer NPC's. Taken: steel 3 stuks fruit, poep op de parasol. Introduceert alertmeter.

**Level 3 — De Buurt met inbraken**
Afgeschermd villagebied. Hoge muren, bewakers bij ingangen. Infiltratiemechanic geïntroduceerd: raam vinden, huizen doorzoeken binnen tijdslimiet, taart stelen en ontsnappen.

**Level 4 — Het Marktplein met inbraken**

---

## Pagina 4: Tech

### Artificial Intelligence

**NPC Gedrag:**

- NPC's volgen een vaste waypoints.
- Elke NPC heeft een zichtveld-cone.
- Bij detectie van de vogel vult de **alertmeter** zich. Drie niveaus:
  - Neutraal — normaal gedrag
  - Achterdochtig — NPC stopt en kijkt omhoog, alertmeter laadt langzamer
  - Alarm — NPC probeert vogel te vangen

**Reacties op Events:**

- Poep-inslag: NPC rent naar de plek, kijkt om zich heen (3–5 sec afleiding).
- Autoalarm: Eigenaar rent naar de auto, omstanders stoppen en kijken.
- Gestolen object: NPC loopt naar de lege plek, begint te zoeken.

---

### Technical

| Uitdaging        | Aanpak                                                                                        |
| ---------------- | --------------------------------------------------------------------------------------------- |
| 3D vliegfysica   | Vereenvoudig tot vrije camerabewegingen in 3D-software met een constante voorwaartse beweging |
| Poep-trajectorie | Moeten nog bekijken                                                                           |

---

## Pagina 5–6: Schema

### Backlog (Feature List, geprioriteerd)

| Prioriteit      | Feature                                       | Belang              |
| --------------- | --------------------------------------------- | ------------------- |
| 🔴 Must Have    | Vliegmechanic (stamina, richting, duikvlucht) | Core gameplay       |
| 🔴 Must Have    | Poep-projectiel met miktrajectorie            | Core prank mechanic |
| 🔴 Must Have    | Snavel-interactie (oppakken/loslaten)         | Core prank mechanic |
| 🔴 Must Have    | NPC zichtveld + alertmeter                    | Core challenge      |
| 🔴 Must Have    | Takenlijst + win/verlies condities            | Game flow           |
| 🟠 Should Have  | Infiltratie-mechanic (kamer + dozen)          | Risico-mechanic     |
| 🟠 Should Have  | NPC achtervolging (bezem)                     | Fail-state feedback |
| 🟠 Should Have  | Level 1 volledig speelbaar                    | Prototype milestone |
| 🟡 Nice to Have | Level 2 en 3                                  | Scope uitbreiding   |
| 🟡 Nice to Have | Chaos-kettingreacties                         | Extra fun           |
| 🟡 Nice to Have | Nacht-level (level 5)                         | Eindlevel           |
| ⚪ Wens         | Scoresysteem + leaderboard                    | Replayability       |
| ⚪ Wens         | Vogel-customisatie (hoedjes)                  | Cosmetics           |

---

### Milestones & Productietijdlijn (6 weken)

| Week       | Milestone                         | Doel                                                     |
| ---------- | --------------------------------- | -------------------------------------------------------- |
| **Week 1** | Prototype vliegen                 | Vogel beweegt correct in 3D, camera volgt, stamina werkt |
| **Week 2** | Core mechanics                    | Poep-projectiel + snavel-interactie werkend              |
| **Week 3** | NPC systeem                       | NPC routes, zichtveld, alertmeter werkend                |
| **Week 4** | **Level 1 volledig speelbaar** ✅ | Takenlijst, win/verlies, basis art en audio              |
| **Week 5** | Infiltratie-mechanic + Level 2    | Kamer-zoek-mechanic, tweede level                        |
| **Week 6** | Polish + Level 3                  | Bug fixing, UI verbetering, examenpresentatie            |

> ⚠️ _Week 10 (examen): volledig uitgewerkt spel met minimaal 3 speelbare levels._

---

---

# 📄 Paper Prototype Rapport

## Onderzochte Mechanic: De Infiltratie (Inbraak + Zoekfase)

### Probleemstelling

De inbraak-mechanic was de grootste **risicovolle mechanic** van het ontwerp. De kernvraag was:

> _Hoe zoekt de vogel objecten in een kamer? Hoe maken we dit spannend en afhankelijk van player skill, zonder het puur willekeurig te maken?_

---

### Hoe Werkt het Paper Prototype

**Materialen:**

- 6 gesloten enveloppen (= dozen in de kamer), elk genummerd 1–6
- In 1 of 2 enveloppen zit een kaartje met "TAART 🎂" (het doelwit)
- In de rest zit een kaartje met "LEEG" of "VERKEERD OBJECT"
- Stopwatch voor tijdslimiet
- 3 "kans-tokens" per speler (= aantal dozen dat geopend mag worden)

**Visuele hints (gesimuleerd):**

- Enveloppen hebben kleine symbolen geschreven op de buitenkant (kleur-stip, streep, kruis) — de "hints" die een oplettende speler kan interpreteren
- Vóór het prototype krijgen spelers 10 seconden om de hints te bestuderen (= observatiefase)

**Spelverloop paper prototype:**

1. Facilitator plaatst enveloppen neer, één of twee bevatten het doelwit.
2. Speler heeft 10 seconden observatietijd (hints bekijken).
3. Stopwatch start: 20 seconden.
4. Speler kiest een envelop, opent die (kost 3 seconden gesimuleerde tijd — facilitator telt af).
5. Als het doelwit gevonden is: succes! Zo niet: keuze maken — volgende proberen of opgeven.
6. Na 3 geopende enveloppen of als de tijd op is: ronde voorbij.

---

### Foto's Paper Prototype

> _(Voeg hier foto's toe van de opgestelde enveloppen, de tokens en de speeltafel tijdens de playtest.)_

**Foto 1:** Overzicht van de 6 enveloppen met symbolen op tafel.
**Foto 2:** Speler bestudeert hints tijdens observatiefase.
**Foto 3:** Speler opent een envelop — facilitator houdt stopwatch bij.
**Foto 4:** Reactie van speler bij het vinden van de "TAART" kaart.

---

### Spelerservaringen

**Speler A:**
Gebruikte de hints actief en vond de taart bij de 2e envelop. Ervaarde de tijdsdruk als spannend maar haalbaar. Opmerking: _"Ik voelde echt dat ik een beslissing moest maken — snel nog een doos proberen of stoppen?"_

**Speler B:**
Negeerde de hints en koos willekeurig. Vond de taart niet binnen de tijd. Reactie: _"Ik had beter moeten kijken, maar de tijdsdruk maakte me nerveus."_ Dit bevestigt dat player skill een rol speelt.

**Speler C:**
Vond de hints te vaag bij de eerste ronde. Na uitleg van de hint-logica lukte het de tweede keer beter. Feedback: _"De hints moeten duidelijker zijn in de echte game, of er moet een tutorialtip zijn."_

**Algemeen observatie:**

- Met 3 kansen bij 6 dozen (1–2 correct) voelde de mechanic uitdagend maar eerlijk.
- Met slechts 2 kansen werd het te frustrerend voor nieuwe spelers.
- Met 4+ kansen verdween de spanning volledig.

---

### Conclusies

**Wat werkt:**

- De combinatie van tijdslimiet + beperkte kansen + visuele hints creëert een **echte spanningsboog**: spelers moeten snel observeren en dan beslissen.
- Player skill is aantoonbaar relevant: spelers die hints lezen, slagen vaker.
- De mechanic is intuitief uit te leggen en snel te leren.

**Wat moet aangepast:**

- Het aantal kansen moet goed gebalanceerd worden: **3 kansen bij 6 dozen** is het sweet spot.
- De hints moeten consistenter en leesbaarder zijn — in de digitale versie kunnen subtiele animaties of geluiden hints geven (bv. een doos die licht beweegt, een geur-effect boven de juiste doos).
- Een kortere tijdslimiet (15 sec) verhoogt de spanning verder zonder unfair te worden.

**Implicaties voor digitale implementatie:**

- De "dozen zoeken" mechanic wordt in Unity geïmplementeerd als een **tijdslimiet-zone**: de speler betreedt de kamer en een countdown start.
- Dozen hebben subtiele visuele hints (lichtglinstering, beweging).
- Het aantal openbare dozen per kamer wordt ingesteld per level (moeilijkheidsgraad).
- Een te zware vogel (met al een object) kan geen tweede object oppakken — stimuleert planning vooraf.

---

_Document versie 1.0 — Team Prank Bird_
