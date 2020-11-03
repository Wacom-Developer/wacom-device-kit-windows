# Readme

## Introduction
This demo implements a simple web drawing app showing how to use HTML5 Pointer Events. With HTML5 Pointer Events, you can create an app that is compatible on many popular browsers that detect pen ink, mouse, and touch data using a single unified API. HTML5 Pointer Events is supported on most, but not all popular browsers. For those browsers that do not support HTML Pointer Events, one solution is to use the older CSS pointer events API, which is also demonstrated in the app.

To run this application, a Wacom tablet driver must be installed and a supported Wacom tablet must be attached. All Wacom tablets supported by the Wacom driver are supported by this API. Get the driver that supports your device at: https://www.wacom.com/support/product-support/drivers.

When you load the sample code, ScribbleDemo.html, into a browser, you will get a drawing canvas, some simple controls, and a field to look at HTML5 Pointer Events data. The main focus of ScribbleDemo.html is, of course, to scribble with pen ink, touch or mouse data. Pen and touch-enabled Wacom tablets provide pen ink data and touch data that includes properties that are used to modify the drawing strokes.  

## Application Details
The HTML5 Pointer Events app supports the following:

* Drawing with pen, touch and mouse input
* Detection of whether the browser you're using supports HTML5 Pointer Events
* Using pen pressure to modify pen strokes
* Using pen tilt to modify pen strokes
* Setting pen color (primary for pen tip, secondary for pen button press)
* A debug monitor that shows input data
* Saving the drawing to a png file

The following illustrates the demo running on Chrome:  

![ScribbleDemo](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Scribble%20Demo%20Using%20Pointer%20Events%20Windows/Media/sc-rm-sdupe-demo.png)
|	#	|Description											|
|:-----:|:------------------------------------------------------|
|![1](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Scribble%20Demo%20Using%20Pointer%20Events%20Windows/Media/rm_1.png)|Indicates the browser supports HTML5 Pointer Events	|
|![2](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Scribble%20Demo%20Using%20Pointer%20Events%20Windows/Media/rm_2.png)	|Selectable Primary pen color							|
|![3](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Scribble%20Demo%20Using%20Pointer%20Events%20Windows/Media/rm_3.png)		|Selectable Secondary pen color by pen button selection	|
|![4](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Scribble%20Demo%20Using%20Pointer%20Events%20Windows/Media/rm_4.png)	|Pen stroke using tilt									|
|![5](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Scribble%20Demo%20Using%20Pointer%20Events%20Windows/Media/rm_5.png)	|Pen stroke using pressure only							|
|![6](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Scribble%20Demo%20Using%20Pointer%20Events%20Windows/Media/rm_6.png)		|Touch stroke											|
|![7](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Scribble%20Demo%20Using%20Pointer%20Events%20Windows/Media/rm_7.png)	|Mouse stroke											|
|![8](https://github.com/cbwinchild-devdocs/icbt-windows-sample-code-docs/blob/master/Scribble%20Demo%20Using%20Pointer%20Events%20Windows/Media/rm_8.png)		|Debug window showing pointer data						|


## See Also 
[Overview](https://developer-docs.wacom.com/wacom-device-api/docs/web-api-overview) - HTML5 Pointer Events overview  

[Basics](https://developer-docs.wacom.com/wacom-device-api/docs/web-api-basics) - Details on how to start writing HTML5 Pointer Events applications  

[Reference](https://developer-docs.wacom.com/wacom-device-api/docs/web-api-reference) - Information on the HTML5 Pointer Events API  

[FAQs](https://developer-docs.wacom.com/wacom-device-api/docs/web-api-faqs) - HTML5 Pointer Events programming tips  

## Where To Get Help
If you have questions about this demo or the Wintab API, please visit our support page: https://developer.wacom.com/developer-dashboard/support

## License
This sample code is licensed under the MIT License: https://choosealicense.com/licenses/mit/