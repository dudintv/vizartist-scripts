## Parser math sentences

#### Version 1.0 (30.04.2020)
* accept "+", "-", "*", "/" actions
* supports parentheses with infinite nesting
* supports functions, for this version ready only "random" and "wiggle"
* supports variables, you can create StringMap with array of variables and load it into the function
* name variable can contain only non-numeric characters, for example "rand" is ok, but "rand2" calls an error
* any error returns "0" as a result of calculation
* test button checks many situations in one click!