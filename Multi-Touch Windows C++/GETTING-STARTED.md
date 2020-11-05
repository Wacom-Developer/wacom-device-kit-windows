# Getting Started 

## Test Environment  

The Multi-Touch (WacomMT_Scribble) demo sample is provided as a C++ application. To build this sample application, you will need Windows 7 and above with Visual Studio 2017 or above.

To test the application, a Wacom tablet driver must be installed and a touch-enabled Wacom tablet must be attached. All touch-enabled Wacom tablets supported by the Wacom driver are supported by this API. Get the driver that supports your device at: https://www.wacom.com/support/product-support/drivers.


## Multi-Touch API and Wintab SDK License  
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

## Install the Wacom tablet driver and verify tablet operation  

In order to run a sample application, it is necessary to install a Wacom tablet driver, which installs the necessary runtime components that support Wintab and the Multi-Touch API. The driver can be found at: https://www.wacom.com/support/product-support/drivers

Once the driver has installed, and you have rebooted your system, check your tablet driver installation by doing the following:

1. Attach a supported, touch-enabled Wacom tablet.
2. Open the Wacom Tablet Properties application (from the Start Menu, go to Wacom Tablet > Wacom Tablet Properties) to confirm your tablet is recognized.
3. Use the Pen and Touch to verify functionality.
4. If all of the above checks out, proceed to the next section to build/run  the sample application.


## Build/run the sample application  

To build the sample application:

1. Open the WacomMT_Scribble.sln file in Visual Studio. The demo includes all SDK header files needed to build with. Other SDK components necessary to run the demo are installed with the tablet driver.
2. From the top menu, select Build > Build Solution.
3. Once built, start the solution from Visual Studio Local Windows Debugger.
4. As the app starts, there should be no warnings or errors. If you do see warnings/errors, be sure the driver is running with the attached, supported tablet.
5. About the application:  
	a. Finger data will show as circles of various colors with size information.  
	b. Pen strokes will appear in blue with pressure.  
	c. The "ESC" key will clear the windows of all input.  
	d. The "Options" menu item allows you to control how touch is handled by the application and get information about attached devices.  
	
![Wacom Multi-Touch Scribble Pen Demo](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Multi-Touch%20Windows%20C%2B%2B/Media/sc-gs-mtc-scribblePen.png)

## See Also  
[Basics](https://developer-docs.wacom.com/wacom-device-api/docs/wfmt-basics) - Details on how to configure and write Multi-Touch applications

[Reference](https://developer-docs.wacom.com/wacom-device-api/docs/wfmt-reference) - Complete Multi-Touch API details

[FAQs](https://developer-docs.wacom.com/wacom-device-api/docs/wfmt-faqs) - Multi-Touch programming tips



## Where to get help  

If you have questions about the sample application or any of the setup process, please visit our Developer Support page at: https://developer.wacom.com/developer-dashboard/support.