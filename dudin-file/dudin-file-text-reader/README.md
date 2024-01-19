## Text file reader

This script recognizes data from text formatted as a table. By default, it splits text into rows by "new line". And, split to cells by "|" (vertical pipe).

Also, you can have "Front Matter" section in the beginning in the file. The script separates "Front Matter" from the data by one line with "---" (three or more hyphens).

Each line of "Front Matter" separates by ":", for example: "name: value".

Full file example:

data: 20 January 2024
source: internet
--------------
1|Orange is orange|news
2|Plum is purple|news
3|Kiwi is green|weather

# Versions

#### Version 1.4 (9 December 2023)
* add internal console for debug information
* add splitting of the file path to make easier to control only the file name independently from the root path

#### Version 1.3.1 (9 December 2023)
* fix reading the fine within initialization (set a very small interval fro the very first try)

#### Version 1.3 (03 December 2023)
* add setting to ignore the first line (it can be a "table header")
* support "Front Matter" splitting by "---" line
* the "Front Matter" can be output in many ways: this container, other container (or containers by name), to SHM variable (or variables by name)
* each "Front Matter" line should has `name` and `value` separated by ":", for example: "title: election data"

#### Version 1.2 (10 August 2023)
* update UI: make target more explicit

#### Version 1.1 (13 September 2022)
* added an output mode — to specific container with text geometry (with default as "this")
* now you can disable removing empty lines

#### Version 1.0 (8 February 2022)
Inputs:
* full filepath
* interval (sec) of reading
* global output variable name
* button: read file now

Basic functional:
* reads text file and put the content to a global variable
* reads by interval in seconds
* ignore empty lines
