## Animated OMO. Select one from children and hide other

#### Version 2.5.0 (1 September 2020)
* ADD: selector of two types transformation moving: "through base state" and "through hidden state" 

#### Version 2.4.1 (1 September 2020)
* FIX: repeating transition animation when "transition throught base"
* FIX: transform strings matching to setup whet_animated settings
* FIX: moving to base if there isn't assigned the "transform hided"

#### Version 2.4.1 (30 August 2020)
* FIX: working with float point numbers, e.g. "scale=0.8"

#### Version 2.4.0 (30 June 2020)
* Add "manual show animation" to control easeness of showing animation

#### Version 2.3.0 (17 May 2020)
* Add "Root Container" to allow control another container (avoid running several scripts on a single container)
* FIX: animate params if noted only one axis, for example "rotX" ot "posY"

#### Version 2.2.0 (27 april 2020)
* Add "Transform base" to control base properties

#### Version 2.1.0 (26 april 2020)
* Add "middle transition" to manual precise control transition animation

#### Version 2.0.0 (26 april 2020)
* Rewrite the script to make code more flexible and clean

#### Version 1.4.1 (25 april 2020)
* Rename "Hide" button to "All to Hide!"
* Add "All to Base!" and "All to Selected!" buttons

#### Version 1.4.0 (24 april 2020)
* add "Hide" button to set all conatiners to __hide__ transformation imidiately

#### Version 1.3.1 (24 february 2020)
* BUGFIX: didn't consider correctly values below zero (with "-" sign)

#### Version 1.3 (17 february 2020)
* add "keep visible" like in original "Omo" to keep visible all items from 1 to current
* optimize animation to don't touch not nessesary parameters, but only parameters which was considered in "selected" and "hided"
* add possible value "-1" in order to hide all items

#### Version 1.2 (23 december 2019)
* add Advanced functions: 
* 1) child filter — to consider only childs which name corresponding the pattern
* 2) take director — play forward when sone element takes, and play backward when deselected all (switched to "base"), also it works when transition plays throught base
* it automatically plays animation directors called as child containers — forward to deselected, backward to selected — "00:00" time means base/selected state

#### Version 1.1 (1 december 2019)
* add "base", "next" and "prev" buttons
* add option "Transition throught base(0)" — it plays transition between two options in two parts: first it plays from previous selected item to the base, and second plays to new selected item

#### Version 1.0 (24 november 2019)
* creake selected and unselected style parsing
* working by selected number like build-in "Omo" plugin
