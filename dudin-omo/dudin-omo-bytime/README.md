## "OMO" based on current time

Helps create "dark mode". You can define as many periods as you want.

_For example, you can define 3 period: "08:00", "15:00" and "22:00".
If current time is 7:30 it will be third period!_

#### Version 1.0 (05 May 2020)
* you can define begins of time periods as string in format "hh:mm"
* if current time is less than first begin — current period will be the last one
* you can change __"count_time_periods"__ variable in the beginning of the script to change count of periods