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

### Log

When running the dump, the log contains these error types (in many instances):

- `[ERR] Invalid segmento_progetto value at region 17.0-19.0=col-segmento_progetto: ""`: this means that the column named "segmento_progetto" has an invalid (here empty) value. In fact, this is a required field while the source is missing a value here. We have 20 instances of this error.
- `[ERR] Unknown value for col-funzione_originaria: "chiesa" at region 29.0-31.0=col-funzione_originaria`: an unknown value implies that we are expecting a value from a known set of values (a thesaurus). In fact, here we are expecting one of the values from the functions thesaurus, named `epi-support-functions`, like "public" or "religious". Instead, we get a value ("church") from another set (corresponding to the thesaurus of structure types, named `epi-support-types`). Clearly, this is a human error where the operator has placed the value in the wrong column. We have 3 instances of this error.

So, here you should edit the source Excel file, and fix the errors:

- add the missing values in column "segmento progetto";
- move "chiesa" from the wrong to the right column.

If you then re-run the validation, you should no longer see any errors. So, this is an incremental process: validate, fix, and repeat until everything is correct.

### Report

The Markdown report provides details about data extraction and mapping for each item imported. Each row in the source Excel file (except of course the top legend row) is an item. In the report, items are numbered starting from 1.

#### Item 1

Item `CASTELLO_05-0001` contains:

- id: CASTELLO_05-0001 ✔️
- area: Castello_5
- sestiere: Castello
- denominazione: Chiesa di San Martino
- funzione originaria: Chiesa
- funzione attuale: religiosa
- tipologia struttura: Chiesa
- interno/esterno: esterno
- supporto: muro
- materiale: litico ✔️
- terminus post: 1781
- cronologia: 1781
- testo: si
- numeri: si
- misure: 16X15 ✔️
- numero righe: 2 ✔️
- alfabeto: latino
- lingua: ITA
- tipologia grafica: maiuscolo

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
