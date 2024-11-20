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

#### Item 1

Item `ESMD_1000` contains:

- id: ESMD_1000
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

This is mapped to a single item with 4 parts. The relevant item's metadata are:

- title: equal to the ID.
- flags: 2 = imported. All the imported items get marked with this flag, so you can later check them in the editor.

The parts are:

1. metadata: `id` = `CASTELLO_05-0001`.
2. support:
     - material: stone.
     - originalFn: "chiesa" (this is an error from the original source, correctly reported by the log: see above).
     - currentFn: religious.
     - objectType: wall.
     - hasField: true.
     - fieldSize: w=16 cm, h=15 cm.
     - counts: rows=2.
