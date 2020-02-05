## Flex-positioning. Partly implemented of "flexbox" conception from html/css.

#### Version 1.0 (04 february 2020)
* resolved any kind of transformation — now you can transform freely any container and Flex will continue to work properly
* add "Collapse if overflow" checkbox — when TRUE then actual gap downsize to zero if sum of elements + min_gaps has size bigger than gabarit, if FALSE then min_gap works always

#### Version 0.4 (16 january 2019)
* add "magnetic" parameter for aestetic correction when it changing count visible childs

#### Version 0.33 (15 january 2019)
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
