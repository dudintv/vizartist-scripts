## JSON Parser

Parsing of valid JSON string and getting any value oe array of values by path.

#### Version 0.1 (23 Oktober 2020)
* Validate JSON string with very weak rules
* Build the tree of semantic objects
* Return single value by path
* Support access by array index
* Support ".count()" function to calculate childs under speciefic path
* Return array of values with "wild" square brackets, for example "scenario\[1\].steps\[\].countries\[\].name" will return array of array of countries names
* Small set of test asserts