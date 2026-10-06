# Max Size in a line

Dynamically scales a group of containers, uniformly reducing their size if their total local width exceeds a defined limit. 

### Version 1.0 (6 Oktober 2026)

* Dynamic Drop Zones: The script provides configurable container slots. To change the number of available containers, modify the QUANTITY_OF_CONTAINERS constant at the top of the script.
* Target Size Modes: The maximum width threshold can be set manually in pixels or derived dynamically from a reference container.
* The interface automatically hides inactive settings based on the selected radio button mode using the SendGuiParameterShow procedure.  
* Proportional Scaling (2D): When the "Proportional Scale y (2D)" checkbox is enabled, the Y-axis is scaled proportionally to the X-axis compression ratio.
If disabled, only the X-axis is squashed.Default Scale Retention:
If the total width remains below the target limit, the script actively maintains the user-defined base values (Default x-scale and Default y-scale).
Interactive Initialization: The "Initialize text scale" action, registered via the RegisterPushButton procedure, allows operators to instantly reset the scale of all assigned containers to their default parameters.   
