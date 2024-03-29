# Getting Started 

## Test Environment
The Wintab demo samples have been provided as Win32 C++ applications. To build this sample application, you will need Windows 7 and above with Visual Studio 2017 or above.

To test the application, a Wacom tablet driver must be installed and a supported Wacom tablet must be attached. All Wacom tablets supported by the Wacom driver are supported by this API. Get the tablet driver that supports your device at: https://www.wacom.com/support/product-support/drivers.
```
Wintab SDK License
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
In order to run a sample application, it is necessary to install a Wacom tablet driver, which installs the necessary runtime components that support Wintab. The driver can be found at: https://www.wacom.com/support/product-support/drivers.

Once the driver has installed, and you have rebooted your system, check your tablet driver installation by doing the following:

1. Attach a supported Wacom tablet.
1. Open the Wacom Tablet Properties application (from the Windows Start Menu, go to Wacom Tablet > Wacom Tablet Properties) to confirm your tablet is recognized.
1. Use tablet ExpressKeys, Touch Ring, and/or Touch Strips to verify functionality.
1. If all of the above checks out, proceed to the next section to build/run the sample application.


## Build/run the sample application

1. Open the TabletControlsSample.sln file in Visual Studio. The demo includes all SDK header files needed to build with. Other SDK components necessary to run the demo are installed with the tablet driver.
2. From the top menu, select Build > Build Solution.
3. Once built, start the solution from Visual Studio Local Windows Debugger.
4. As the app starts, there should be no warnings. If you do see warnings, be sure the driver is running with the attached, supported, tablet as described above.
5. Controls for all tablets currently and previously attached will appear. To only show attached tablets, reset settings from the Wacom Desktop Center Backup settings section.  


![Tablet controls](./Media/sc-gs-tabletcontrols.png)

## See Also
[Wintab - Basics](https://developer-docs.wacom.com/intuos-cintiq-business-tablets/docs/wintab-basics) - How to configure and write Wintab applications  

[Wintab - Reference](https://developer-docs.wacom.com/intuos-cintiq-business-tablets/docs/wintab-reference) - Complete API details 

[Wintab - FAQs](https://developer-support.wacom.com/hc/en-us/articles/12844524637975-Wintab) - Wintab programming tips  

## Where to get help
If you have questions about the sample application or any of the setup processes, please visit our Developer Support page at: https://developer.wacom.com/developer-dashboard/support.

