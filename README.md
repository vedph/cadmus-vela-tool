# Cadmus VeLA CLI Tool

This is a command-line tool providing administrative functions for [Cadmus VeLA](https://github.com/vedph/cadmus-vela). Currently, it is designed to import data from Excel files into the VeLA database using a [Proteus](https://myrmex.github.io/overview/proteus/)-based pipeline.

To import Excel files, run the `import` command, passing the path to the JSON pipeline configuration file, e.g.:

```ps1
import c:\users\dfusi\desktop\vela.json
```

Preset profiles can be found under the `Assets` folder of the CLI app.

## History

- 2024-01-25: updated packages.
- 2024-01-19: updated packages and profiles.
- 2024-01-18:
  - updated packages.
  - added column name filtering to asset profiles, as our source has column names with whitespaces and various casing.
