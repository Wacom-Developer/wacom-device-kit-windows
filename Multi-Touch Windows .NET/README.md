# Readme

## Introduction
This is a C#/.NET application that uses the Multi-Touch and Wintab APIs (via a C# wrapper) to receive Touch and Pen data from one or more attached Wacom tablet(s). The application visualizes those interactions.

This demo shows how an application can use the Multi-Touch API to:

* Connect to touch-enabled Wacom tablets
* Open application-private connections to the APIs
* Receive message notification when a user interacts via Touch or Pen with a tablet, as well as notification of other tablet events (such as when a pen comes into proximity, etc.)
* Receive capability data for each attached tablet

To run this application, a Wacom tablet driver must be installed and a touch-enabled device must be attached. Get the tablet driver that supports your device at: https://www.wacom.com/support/product-support/drivers.

## Application Details
The application uses a managed C# wrapper library (WacomMTDN.dll) to communicate with the installed touch driver module, WacomMT.dll. The WacomMTDN source code is included as a project within the sample's solution. If the driver is not installed or is not communicating, the program will display an appropriate warning. Additionally, WintabDN.dll is included to add support for Wintab pen input. To learn more about, and view the source code for WintabDN.dll, see the Wintab .NET sample code [GETTING STARTED page](/Wintab%20.NET/GETTING-STARTED.md).

The following image illustrates a simplified overview of the major  components:

![simplified overview image of the major components](./Media/sc-rm-MTCDN-appOverview.png)


You can download the sample code and view the inline comments to find out detailed information about the sample code itself.

## See Also
[Basics](https://developer-docs.wacom.com/intuos-cintiq-business-tablets/docs/wfmt-basics) - Details on how to configure and write Multi-Touch applications

[Reference](https://developer-docs.wacom.com/intuos-cintiq-business-tablets/docs/wfmt-reference) - Complete Multi-Touch API details

[FAQs](https://developer-support.wacom.com/hc/en-us/articles/12844682604567-Wacom-Feel-Multi-Touch) - Multi-Touch programming tips

## License
This sample code is licensed under the MIT License: https://choosealicense.com/licenses/mit/.