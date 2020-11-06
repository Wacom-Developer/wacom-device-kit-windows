# Readme

## Introduction
This is a Win32 application that uses the Wintab API to receive pen data from multiple attached Wacom tablets and allows the user to do free-form sketching on a canvas, showing lines with varying pen pressure.

This demo shows how an application can use Wintab to:
* Connect to Wintab-enabled Wacom tablets.
* Open an application-private Wintab context, which can be customized as needed to map the tablet to the display configuration and specify what pen data is to be delivered to the app.
* Receive message notification when pen data is available, as well as notification of other tablet events (such as when tablets are attached and detached, when a pen comes into proximity, etc.).
* Receive pen data as raw tablet counts or system pixels as needed for the application.
* Have full access to pen properties such as location, pressure, orientation, tilt, etc.

To run this application, a Wacom tablet driver must be installed and a supported Wacom tablet must be attached. All Wacom tablets supported by the Wacom driver are supported by this API. Get the driver that supports your device at: https://www.wacom.com/support/product-support/drivers.


## Application Details
The application uses an installed driver module, Wintab32.dll, to communicate with the tablet driver. If the driver is not installed, is not communicating, or there is no supported Wacom tablet attached, then the program will display an appropriate warning.

The following image illustrates a simplified overview of the major Wintab-supporting components:  

![scribbledemo-overview](https://github.com/Wacom-Developer/wacom-device-kit-windows/blob/master/Wintab%20ScribbleDemo/Media/sc-rm-sd-suppcom-overview.png)

You can download the sample code and view the inline comments to find out detailed information about the sample code itself.

## See Also  
[Basics](https://developer-docs.wacom.com/wacom-device-api/docs/wintab-basics) - Details on how to configure and write Wintab applications.  

[Reference](https://developer-docs.wacom.com/wacom-device-api/docs/wintab-reference) - Complete API details 

[FAQs](https://developer-docs.wacom.com/wacom-device-api/docs/wintab-faqs) - Wintab programming tips  


## Where To Get Help
If you have questions about this demo or the Wintab API, please visit our support page: https://developer.wacom.com/developer-dashboard/support.

## License
This sample code is licensed under the MIT License: https://choosealicense.com/licenses/mit/.