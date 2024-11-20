# Sample File

This example refers to the import procedure for the sample [vela.xlsx](../vela.xlsx) document in this repository.

## Excel Dump

This dump is rarely used in production, but it can be invaluable for diagnostic purposes. Essentially, it generates an Excel file listing all the records with their detected regions, as they are read from the source Excel file.

1. create a working folder for your data files and place into it (a) the Excel files you want to validate and (b) the [JSON profile for Excel output](../vela-tool/Assets/Dump-xlsx.json).
2. for each source file, update its input and output file names in the above JSON profile (under `entryReader/options/inputFile` and `entrySetExporters/options/outputDirectory`), and run the validation with `./vela-tool import <PATH_TO_YOUR_JSON_PROFILE>`.

## Markdown Dump

1. create a working folder for your data files and place into it (a) the Excel files you want to validate and (b) the [JSON profile for Markdown output](../vela-tool/Assets/Dump-md.json).
2. for each source file, update its input and output file names in the above JSON profile (under `entryReader/options/inputFile` and `entrySetExporters/options/outputDirectory`), and run the validation with `./vela-tool import <PATH_TO_YOUR_JSON_PROFILE>`.

This dump creates a detailed Markdown report with all the data extracted and remapped from the source Excel document into the Cadmus-based target model.

Additionally, a log file is created (in the same folder of the CLI tool) including the list of all the issues found during import. You should carefully check this log to ensure that no errors are present.

### Report

The Markdown report provides details about data extraction and mapping for each item imported. Each row in the source Excel file (except of course the top legend row) is an item. In the report, items are numbered starting from 1.

#### Items 1-4

All these items share the same data, except for their ID.

- id: from ESMD_1000 to ESMD_1002
- stato: in lavorazione
- segmento progetto: VeLA Monastica
- provincia: Venezia
- citta': Venezia
- centri/localita': Cannareggio
- localizzazione: Fondamenta Daniele Canal
- denominazione struttura: Chiesa Santa Maria dei Servi
- funzione originaria: religiosa
- tipologia originaria: edificio di culto
- funzione attuale: privata
- tipologia attuale: struttura ricettiva
- interno/esterno: esterno

This is mapped to a single item with 3 parts. The relevant item's metadata are:

- title: equal to the ID column value.
- flags (hex 0083):
  - 0x0001: in lavorazione.
  - 0x0002: imported. All the imported items get marked with this flag, so you can later check them in the editor.
  - 0x0080: monastica.

The parts are:

1. metadata: `id` = `ESMD_1000`.
2. district location: Venezia, Venezia, Cannareggio, Fondamenta Daniele Canal, Chiesa Santa Maria dei Servi.
3. support:
     - material: -.
     - originalFn: religious.
     - currentFn: private.
     - originalType: worship-building.
     - currentType: accommodation.

#### Item 5

Item ESMD_1003 is equal to the previous ones, except for the addition of an author (Frambusto Giulia). This is correctly found in metadata.

#### Item 6

- id: ESMD_1004
- stato: in lavorazione
- autore: Masiero Francesco, Frambusto Giulia
- segmento progetto: VeLA Monastica
- provincia: Venezia
- citta': Venezia
- centri/localita': Cannareggio
- localizzazione: Fondamenta Daniele Canal
- denominazione struttura: Chiesa Santa Maria dei Servi
- funzione originaria: religiosa
- tipologia originaria: edificio di culto
- funzione attuale: privata
- tipologia attuale: struttura ricettiva
- interno/esterno: esterno
- supporto: Lapide (graffito su)
- testo: si
- lettere non interpretabili: si
- lingua: LAT
- funzione non definibile: si
- cifra: romana
- cronologia: età medioevale
- secolo: xv secolo
- materia: materiale litico
- misure supporto: 38x43
- stato di conservazione: mutilo
- damnatio: si
- campo: aperto
- solco: V??
- incisione: si
- scalpello: si
- impaginazione del testo: piena pagina??
- numero righe: 4
- presenza di preparazione: si
- scrittura: maiuscola
- tipologia grafica: gotica
- data prima ricognizione: 23/05/2024
- data ultima ricognizione: 23/05/2024
- edizione: `(1) <<...>> V <<...>> IIII (2) <<...>>s magistri (3) <<...>>p condaci<<...>> (4) <<...>> suox eredum`

This is mapped to a single item with 9 parts. The relevant item's metadata are:

- title: equal to the ID column value.
- flags (hex 0083):
  - 0x0001: in lavorazione.
  - 0x0002: imported. All the imported items get marked with this flag, so you can later check them in the editor.
  - 0x0080: monastica.

The parts are:

1. metadata: `id` = `ESMD_1000`, `author`=`Masiero Francesco`, `author`=`Frambusto Giulia`, `era`=`età medioevale`.
2. district location: Venezia, Venezia, Cannareggio, Fondamenta Daniele Canal, Chiesa Santa Maria dei Servi.
3. support:
      - material: stone
      - originalFn: religious
      - currentFn: private
      - originalType: worship-building
      - objectType: tombstone
      - support size: 38x43 cm.
      - counts: rows=4
      - features: preparation
      - hasDamnatio: true
