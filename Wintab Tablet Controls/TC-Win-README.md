# Readme

## Introduction
This is a Win32 application that uses the Wintab Extended API to receive ExpressKey, Touch Ring, and Touch Strip data from one or more attached Wacom tablet(s). The application visualizes interactions with the ExpressKeys, Touch Ring, and/or Touch Strip.

This demo shows how an application can use Wintab to:

* Connect to Wintab-enabled Wacom tablets.
* Open an application-private Wintab context and set up handlers for ExpressKey, Touch Ring, and Touch Strip events.
* Receive message notification when tablet control data is available, as well as notification of other tablet events (such as when a pen comes into proximity, etc.).
* Receive usage and status data for each control.  

To run this application, a Wacom tablet driver must be installed and a supported Wacom tablet must be attached. All Wacom tablets supported by the Wacom driver are supported by this API. Get the driver that supports your device at: https://www.wacom.com/support/product-support/drivers.


## Application Details
The application uses an installed driver module, Wintab32.dll, to communicate with the tablet driver. If the driver is not installed, is not communicating, or there is no supported Wacom tablet attached or in the preferences, then the program will display an appropriate warning.

Here is a simplified overview of the major Wintab-supporting components:  

IMAGE  
![wintab-overview](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Wintab%20Tablet%20Controls/Media/sc-rm-wintab-overview.png)

You can download the sample code and view the inline comments to find out detailed information about the sample code itself.

## See Also
[Basics](https://developer-docs.wacom.com/wacom-device-api/docs/wintab-basics) - Details on how to configure and write Wintab applications.  

[Reference](https://developer-docs.wacom.com/wacom-device-api/docs/wintab-reference) - Complete API details 

[FAQs](https://developer-docs.wacom.com/wacom-device-api/docs/wintab-faqs) - Wintab programming tips  

## Where To Get Help
If you have questions about this demo or the Wintab API, please visit our support page: https://developer.wacom.com/developer-dashboard/support.

## License
This sample code is licensed under the MIT License: https://choosealicense.com/licenses/mit/.

