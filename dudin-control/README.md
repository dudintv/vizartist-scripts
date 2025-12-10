# Control Plugins Ids Renamer

It helps to rename control ids to make them unique for avoiding using ControlList.

E.g. if you have this structure:

```
object
└ script
└ root
  └ item1
    └ label (with ControlText)
    └ photo (with ControlImage)
  └ item2
    └ label (with ControlText)
    └ photo (with ControlImage)
```

then all control plugins obtain the following ids:

```
item1-label(TXT)
item1-photo(IMG)
item2-label(TXT)
item2-photo(IMG)
```

### version 1.4.0, 10.12.2025

* add prefix from the root container name

### version 1.3.0, 08.12.2025

* optionaly change corresponding descriptions
* skip plugins by regex in its id

### version 1.2.0, 07.12.2025

* support more control plugins (the most common)
* add checkbox "add type suffix"

### version 1.1.0

* add renaming ControlContainer plugins
* refactor: make easier the addding new control plugins types

### version 1.0.0

* can rename only TextControl ids to the combination Item container name + "-" + the Plugin container name
