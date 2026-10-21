# Max Size: Background Scaler

### Version: 1.15.0

A dynamic VizScript plugin designed to automatically scale and position a background container based on the exact transformed bounds of a target source.

Core Features

* Global Progress: Smoothly animate the final calculated size from 0% to 1000+%.
* Smart Source Pathing: Target containers dynamically using relationships (FIRST, PREV), direct assignment, or relative/absolute path traversal.
* Size Limits & Padding: Set independent X/Y/Z padding, minimum size constraints, and custom "Off" size transitions.
* Geometry vs. Scaling: Choose to manipulate standard Scaling properties or direct Geometry width/height parameters.
* Inertia & Smart Pause: Built-in physical inertia with customizable directional frame-delays to prevent jitter during live updates.
* Auto-Positioning: Automatically align the background to the source object using configurable anchors and pixel shifts.
