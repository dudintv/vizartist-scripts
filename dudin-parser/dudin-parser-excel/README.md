## Parser table data from DataPool "Reader"

#### Version 1.0 (06.12.2019)
* get params from "Reader" settings — SHM type, SHM variable name, rows & columns delimiters
* it's possible to output only a cell or whole table (with convenient alignment)
* you can output to console, to this.geometry.text, or to some plugin value (it support bool, int, double and string value)
* get ignore empty output — it happens when excel file is blocked (usually when the file is opened)

#How make DataPool "Reader" works?
* You have to install "Microsoft Access Database Engine" on all VizEngine machines to allow to open excel-files — https://www.microsoft.com/en-us/download/details.aspx?id=54920
* You have to save excel file as "Microsoft Excel 97/2000/XP" or older

####Setting up the plugin:
* Setup "File Name" and "Table/Sheet"
* Setup "Field is a:" to "Shared Memory"
* Setup "Fields Delimiter" and "Rows Delimiter" as you want. I recommend you to use "|" and "^". Choose it wisely, with corresponded your datas

__I recommend don't open file while you use it in Vizrt — if file is opened then "Reader" give you an empty string.__
