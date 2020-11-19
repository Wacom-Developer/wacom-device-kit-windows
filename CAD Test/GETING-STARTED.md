# Getting Started

## Test Environment
The Cadtest demo sample has been provided as a C++ application. To build this sample application, you will need the Windows 8.1 SDK and the v141 platform toolset (Visual Studio 2017).

To run the application, make sure the Wacom Tablet Driver is installed and you have a supported Wacom tablet. Drivers and information on supported tablets can be found here: https://www.wacom.com/support/product-support/drivers.

## Wintab SDK License:
```
Copyright (c) 2020, Wacom Technology Corporation
   
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
   
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
```

## Install the Wacom Tablet Driver and Verify Tablet Operation
In order to run a sample application, it is necessary to install a Wacom tablet driver, which installs the necessary runtime components that support Wintab. The driver can be found at: https://www.wacom.com/support/product-support/drivers.

Once the driver has installed, and you have rebooted your system, check your tablet driver installation by doing the following:

1. Attach a supported Wacom tablet.
1. Open the Wacom Tablet Properties application (from the Start Menu, go to Wacom Tablet > Wacom Tablet Properties) to confirm your tablet is recognized.
1. Use the pen to verify functionality.
1. If all of the above checks out, proceed to the next section to build/run the sample application.

## Build/Run the Sample Application
To build the sample application:

1. Open the CadTest.sln file in Visual Studio. The demo includes all the SDK header files needed for building. Other SDK components necessary to run the demo are installed with the tablet driver.
1. Select CPU type Win32.
1. Press Build.
1. Once built, start the solution from the Visual Studio Local Windows Debugger.
1. As the app starts, there should be no warnings.  If you do see warnings, recheck to see whether the driver is running with the attached tablet as described above.
1. Depending on the tablet type, your monitor configuration, and where you started Visual Studio, you may need to move the app window to an appropriate display. For example, if using a Cintiq Pro, you would need to move the app window to that tablet’s display.
1. Once on the appropriate display, hovering your pen in the application window should show a vertical and a horizontal line which meet where your pen tip is.
1. In order to use the ruler, do the following:
	1. While the application is running, click Demo > Ruler Demo. This opens a new dialog as well as opens a second digitizer context for each connected tablet.
	1. Because this opens new digitizer contexts, you must place the pen into proximity of the tablet you wish to measure surface distance on before starting the measurement procedure.
	1. Use your mouse to click on the opened dialog. This begins a Wintab polling loop which will be used to take the first and last points of contact from when you place the pen tip onto the tablet surface.
	1. Press the pen to the surface, move it to a desired location, and lift the pen off the surface.
	1. The opened dialog will now give the distance between the starting and ending locations (regardless of path traveled), in inches and centimeters.


## See Also
[Wintab - Basics](https://developer-docs.wacom.com/wacom-device-api/docs/wintab-basics) – How to configure and write Wintab applications

[Wintab - Reference](https://developer-docs.wacom.com/wacom-device-api/docs/wintab-reference) - Complete API details

[Wintab - FAQs](https://developer-docs.wacom.com/wacom-device-api/docs/wintab-faqs) - Wintab programming tips


## Where to get help
If you have questions about the sample application or any of the setup process, please visit our support page at: https://developer.wacom.com/developer-dashboard/support

