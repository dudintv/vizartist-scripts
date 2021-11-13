## Flex-positioning. Partly implemented of "flexbox" conception from html/css.

Known issue:

* It doen't support VizArtist 4.3 if you use it with one child container. I'll fix with the next version.

#### Version 1.3.1 (29 April 2021)
* fix: working with crazy shifted center points by X-axis
* broke: all Y-logic ! CAUTION !

#### Version 1.3 (28 April 2021)
* You can select any container as a root (not only this)

#### Version 1.2 (28 May 2020)
* Add animate transition with duration and several ease functions 

#### Version 1.1.1 (28 May 2020)
* FIX: there was incorrect calc item witdh in "first sub-container" mode

#### Version 1.1 (06 February 2020)
* add "shift gap" to plus or minus value to gap — useful when you see flickering zero-gap between child
* rename old "Shift gap" to "Multiply gap" because it's just add to gap a multiplier of empty space

#### Version 1.0 (04 February 2020)
* resolved any kind of transformation — now you can transform freely any container and Flex will continue to work properly
* add "Collapse if overflow" checkbox — when TRUE then actual gap downsize to zero if sum of elements + min_gaps has size bigger than gabarit, if FALSE then min_gap works always

#### Version 0.4 (16 January 2019)
* add "magnetic" parameter for aestetic correction when it changing count visible childs

#### Version 0.33 (15 January 2019)
* Translated in English
* fix aligning with first sub-container

#### Версия 0.32 (15 июня 2018)
* поправил перевод глобальных координат габарита в локальные
* и теперь полностью игнорится не только скрытый элемент, но и нулевой по габаритам

#### Версия 0.2 (25 января 2017)
* Добавил изменение основной оси — XY.
* Добавил выравнивание поперек основной оси.
* считывать габариты детей через общий размер и через размер первого подпотомка
* учитывает только видимые потомки
