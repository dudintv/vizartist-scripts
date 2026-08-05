## Parliament Colorize

This script controls two builtin plugins: `Parlament` and `Colorize`.

It accepts the text input in format:

```
1 | 10
2 | 17
3 | 33
```

where one line is one party. "|" is separator for the data columns (so you can have more columns than needed, the rest are just ignored).

One colummn suppose to have the color index from a container with multiple children, where each child brings a material sample. THe very first child is the default color. 
Another columns suppose to have the amount of "seats" for the corresponding party.

#### Version 1.0.0 (9 December 2023)
* get total amount of seats from the Palament plugin
* get table-like text input from either container text or SHM variable
* you can specify columns with the color-index property and with value
* you can specify the root container with containers with colors
* signal in the console when there is no SHM variable, or the input container is not specified
* signal in the console when the sum of seats is more than total number of seats
