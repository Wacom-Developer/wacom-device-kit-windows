# Readme

## Introduction  

This is a C++ application that uses the Multi-Touch and Wintab APIs to receive Touch and Pen data from one or more attached Wacom tablet(s). The application visualizes those interactions.

This demo shows how an application can use the Multi-Touch API to:

* Connect to touch-enabled Wacom tablets.  
* Open application-private connections to the APIs.
* Receive message notification when a user interacts via Touch or Pen with a tablet, as well as notification of other tablet events (such as when a pen comes into proximity, etc.).
* Receive capability data for each attached tablet.

To run this application, a Wacom tablet driver must be installed and a touch-enabled device must be attached. Get the tablet driver that supports your device at: https://www.wacom.com/support/product-support/drivers.
 	

## Application Details  

The application uses installed driver modules, WacomMT.dll and Wintab32.dll, to communicate with the tablet driver. If the driver is not installed or is not communicating, the program will display an appropriate warning.

Here is a simplified overview of the major components:  
![Overview of major components](https://github.com/Wacom-Developer/wacom-device-kit-windows/blob/master/Multi-Touch%20Windows%20C%2B%2B/Media/sc-rm-mtc-appOverview.png)


You can download the sample code and view the inline comments to find out detailed information about the sample code itself.


## See Also  
[Basics](https://developer-docs.wacom.com/wacom-device-api/docs/wfmt-basics) - Details on how to configure and write Multi-Touch applications

[Reference](https://developer-docs.wacom.com/wacom-device-api/docs/wfmt-reference) - Complete Multi-Touch API details

[FAQs](https://developer-docs.wacom.com/wacom-device-api/docs/wfmt-faqs) - Multi-Touch programming tips


Where To Get Help
If you have questions about this demo, the Multi-Touch API, or the Wintab API, please visit our support page: https://developer.wacom.com/developer-dashboard/support.

 

License
This sample code is licensed under the MIT License: https://choosealicense.com/licenses/mit/.