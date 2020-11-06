# Getting Started

## Test Environment
The Multi-Touch .NET (WacomMTDN) demo sample is provided as a C# application. To build this sample application, you will need the .NET 4 Framework.

To use the application, a Wacom tablet driver must be installed and a touch-enabled Wacom tablet must be attached. All touch-enabled Wacom tablets supported by the Wacom driver are supported by this API. Get the driver that supports your device at: https://www.wacom.com/support/product-support/drivers.


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
In order to run the sample application, it is necessary to install the Wacom Tablet Driver, which installs the necessary runtime components that support the 	Multi-Touch and WinTab APIs. The driver can be found at: https://www.wacom.com/support/product-support/drivers.

Once the driver has installed, and you have rebooted your system, check your tablet driver installation by doing the following:

1. Attach a supported touch-enabled Wacom tablet
2. Open the Wacom Tablet Properties application (from the Start Menu, go to Wacom Tablet > Wacom Tablet Properties) to confirm your tablet is recognized
3. Use the Pen and Touch to verify functionality
4. If all of the above checks out, proceed to the next section to build/run  the sample application.

## Build/run the sample application
To build the sample application:

1, Open the WacomMTDN.sln file in Visual Studio. The demo includes all SDK header files needed to build and communicate with the Multi-Touch API. Other SDK components necessary to run the demo are installed with the tablet driver.  

2. There are two projects in the solution:
	* WacomMTDN: This project builds the WacomMTDN.dll which is used to interface with native Wacom Multi-Touch library (installed with the Wacom Tablet Driver).
	* WacomMTDN_TestApp: This is a test application which uses WacomMTDN.dll  
	
1. This sample code project comes with the WinTabDN.dll, which brings in WinTab pen support into C# applications. You can read more about C# (.NET) support on the Wintab Basics page, in the [Programming framework, SDK, languages](https://developer-docs.wacom.com/wacom-device-api/docs/wintab-basics#prog-framewk-sdk-lang) section, and review the sample code which builds this dll in LINK [Windows .NET - Getting Started ](https://github.com/Wacom-Developer/wacom-device-kit-windows/blob/master/Wintab%20.Net/GETTING-STARTED.md).
1. From the top menu, select Build > Build Solution.
1. Be sure WacomMTDN_TestApp is set as the default start-up project.
1. Once built, start the solution from Visual Studio Local Windows Debugger.
1. As the app starts, there should be no warnings or errors. If you do see warnings/errors, be sure the driver is running with the attached, supported, tablet as described above, and be sure you have the necessary build and runtime components installed.
1. About the application:  

	* Finger data will show as circles of various colors.  
	
	* Pen strokes will appear in black with pressure.  
	
	* The "ESC" key will clear the window of all input.  
	
1. The options on the right allow you to control how touch is handled by the application and get information about attached devices. Information on what these different options do can be found on the Wacom Feel™  Multi-Touch Basics page, in the [Overview of a Multi-Touch Application section](https://developer-docs.wacom.com/wacom-device-api/docs/wfmt-basics#multi-touch-app-overview).  
	
![WacomMTDN Test Application](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Multi-Touch%20Windows%20C%23%20.NET/Media/sc-gs-MTC%23.NET-demo.png)

## See Also
[Basics](https://developer-docs.wacom.com/wacom-device-api/docs/wfmt-basics) - Details on how to configure and write Multi-Touch applications

[Reference](https://developer-docs.wacom.com/wacom-device-api/docs/wfmt-reference) - Complete Multi-Touch API details

[FAQs](https://developer-docs.wacom.com/wacom-device-api/docs/wfmt-faqs) - Multi-Touch programming tips


## Where to get help
If you have questions about the sample application or any of the setup process, please visit our Developer Support page at: https://developer.wacom.com/developer-dashboard/support.