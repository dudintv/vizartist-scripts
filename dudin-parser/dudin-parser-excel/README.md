## Parser table data from DataPool "Reader"

#### Version 1.7.0 (08.09.2020)
* add: new type "percent" to provide correct appearing of percent values (just multiply by 100.0)

#### Version 1.6.1 (08.09.2020)
* fix: printing error when data is empty

#### Version 1.6.0 (02.09.2020)
* add: number param for "number" type, you can type "=x,y:number(2)" in order to get float number with two numbers after point
* add: active searching the actual last valuable cell in each column separatelly

#### Version 1.5.1 (30.08.2020)
* fix: change color parsing (from ";" to " " delimeter)

#### Version 1.5.0 (30.08.2020)
* new feature: you can arbitrary name your container, e.g. "Month=3,2"
* new feature: you can index row from the end of the table, e.g. "LastValue=-1,2" — it gives you the last value in second column
* fix: out of array indexes

#### Version 1.4.1 (30.08.2020)
* add output to ":color" type
* repair table formatting to console

#### Version 1.4 (26.08.2020)
* add collecting data by column into SHM variables (useful for VisualDataTools geom plugins)

#### Version 1.3 (05.08.2019)
* add parsing type of target children to control Omo plugin. Example of target name: "=4,4:omo" (with "Omo" type) and "=4,4" (with default "text" type)

#### Version 1.2 (07.12.2019)
* add searching row/column by text in certain column/row — it's usefull when, for example, you need find certain row with the certain text in the first column

#### Version 1.1 (06.12.2019)
* add "Output to" childs texts — it works by name of child containers — you need to call interactive containers by template "=X,Y", where X and Y can be a number or any name of auto-counter. If it's a name then it will be auto incremented within all childs of this container. Also you can change the started index for auto-counting
* add debug option "Print numbers of rows and columns" in order to provide convinient output table

#### Version 1.0 (06.12.2019)
* get params from "Reader" settings — SHM type, SHM variable name, rows & columns delimiters
* it's possible to output only a cell or whole table (with convenient alignment)
* it can output to console, to this.geometry.text, or to some plugin value (it support bool, int, double and string value)
* it can ignore the empty output — it happens when excel file is blocked (usually when the file is opened) — that way you can avoid erasing result previous successful loading

---

## How make DataPool "Reader" works?
* You have to install "Microsoft Access Database Engine" on all VizEngine machines in order to allow Vizrt to open excel-files — https://www.microsoft.com/en-us/download/details.aspx?id=54920
* You have to save excel file as "Microsoft Excel 97/2000/XP" or older

#### Setting up the plugin:
* Setup "File Name" and "Table/Sheet"
* Setup "Field is a:" to "Shared Memory"
* Setup "Fields Delimiter" and "Rows Delimiter" as you want. I recommend you to use "|" and "^". Choose it wisely, with corresponded your datas

__I recommend don't open file while you use it in Vizrt — if file is opened then "Reader" give you an empty string.__
