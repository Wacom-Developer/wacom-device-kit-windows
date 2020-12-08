///////////////////////////////////////////////////////////////////////////////
//
//	DESCRIPTION
//		Implementation for tablet-related functions.
//
//	COPYRIGHT
//		Copyright (c) 2014-2020 Wacom Co., Ltd.
//		All rights reserved.
//
///////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "Tablet.h"
#include "Drawing.h"
#include "Utils.h"
#include <sstream>
#include <map>

////////////////////////////////////////////////////////////////////////////////
// Module-global variables

static HCTX ghCtx = NULL;
static DWORD gNumTablets = 0;
static DWORD gNumTabletsThatHaveBeenAttached = 0;

DWORD gNumCursorsPerTablet = 0;
std::map<int, bool> gAttachMap;

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Convert the wintab extension value to the index for this version of wintab.
//	Parameters:
//		tagToMatch_I - The extension tag value
//		index_O - The index of the requested tag
//	Return:
//		bool - true if tag value found
//	Notes:
//		Not all versions of wintab support all extensions.  Extensions are defined by
//		value and referenced by index.
//
bool FindWTExtension(UINT tagToMatch_I,
							UINT &index_O)
{
	UINT index = 0xFFFFFFFF;

	// Iterate through Wintab extension indices
	UINT thisTag = 0;
	for (UINT i = 0; gpWTInfoA(WTI_EXTENSIONS+i, EXT_TAG, &thisTag); ++i)
	{
		// looking for the specified tag
		if (thisTag == tagToMatch_I)
		{
			// note the index of the found tag
			index = i;
			break;
		}
	}

	// if found report the index
	if (index != 0xFFFFFFFF)
	{
		index_O = index;
		return true;
	}

	return false;
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Get a property value from an extention.
//	Parameters:
//		ext_I - The index of the extension
//		tablet_I - The index of the tablet
//		control_I - The index of the control on the tablet
//		function_I - The index of the function on the control
//		property_I - The value of the property requested
//	Return:
//		<T> - the value of the propery correctly typecast; or a self initialize
//				value to type T
//	Notes:
//		This must be called with the correct data type for the property value.
//
template <typename T>
T CtrlPropertyGet(UINT ext_I,
						BYTE tablet_I,
						BYTE control_I,
						BYTE function_I,
						WORD property_I)
{
	T result = T();

	// allocate a buffer
	std::unique_ptr<BYTE[]> buffer(new BYTE[sizeof(EXTPROPERTY) + sizeof(T)]);

	// cast the buffer to the extension property data structure
	EXTPROPERTY *prop = (EXTPROPERTY*)buffer.get();

	// fill in the data
	prop->version = 0;
	prop->tabletIndex = tablet_I;
	prop->controlIndex = control_I;
	prop->functionIndex = function_I;
	prop->propertyID = property_I;
	prop->reserved = 0;
	prop->dataSize = sizeof(T);

	// send the command to Wintab
	const bool gotit = (FALSE != gpWTExtGet(ghCtx, ext_I, prop));

	// if successful
	if (gotit)
	{
		// store the data requested
		result = *((T*)(&prop->data[0]));
	}
	else
	{
		// otherwise report the failure
		ShowError("Failed to get a property.");
	}

	return result;
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Set a value for a property of an extention.
//	Parameters:
//		ext_I - The index of the extension
//		tablet_I - The index of the tablet
//		control_I - The index of the control on the tablet
//		function_I - The index of the function on the control
//		property_I - The property value to set
//		value_I - The value to set the property to
//	Return:
//		bool - true if the value correctly set
//	Notes:
//		This must be called with the correct data type for the property value.
//
template <typename T>
bool CtrlPropertySet(UINT ext_I,
							BYTE tablet_I,
							BYTE control_I,
							BYTE function_I,
							WORD property_I,
							T value_I)
{
	// allocate a buffer
	std::unique_ptr<BYTE[]> buffer(new BYTE[sizeof(EXTPROPERTY) + sizeof(T)]);

	// cast the buffer to the extension property data structure
	EXTPROPERTY *prop = (EXTPROPERTY*)buffer.get();

	// fill in the data
	prop->version = 0;
	prop->tabletIndex = tablet_I;
	prop->controlIndex = control_I;
	prop->functionIndex = function_I;
	prop->propertyID = property_I;
	prop->reserved = 0;
	prop->dataSize = sizeof(T);
	*((T*)(&prop->data[0])) = value_I;

	// send the command to Wintab and record result
	return (FALSE != gpWTExtSet(ghCtx, ext_I, prop));
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Specialization for a STL string.
//	Notes:
//		See CtrlPropertySet
//
template <>
bool CtrlPropertySet(UINT ext_I,
							BYTE tablet_I,
							BYTE control_I,
							BYTE function_I,
							WORD property_I,
							std::string value_I)
{
	// allocate a buffer
	std::unique_ptr<BYTE[]> buffer(new BYTE[sizeof(EXTPROPERTY) + value_I.length() + 1]);

	// cast the buffer to the extension property data structure
	EXTPROPERTY *prop = (EXTPROPERTY*)buffer.get();

	// fill in the data
	prop->version = 0;
	prop->tabletIndex = tablet_I;
	prop->controlIndex = control_I;
	prop->functionIndex = function_I;
	prop->propertyID = property_I;
	prop->reserved = 0;
	prop->dataSize = static_cast<DWORD>(value_I.length() + 1);
	strcpy((char*)(&prop->data[0]), value_I.c_str());

	// send the command to Wintab and record result
	return (FALSE != gpWTExtSet(ghCtx, ext_I, prop));
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Specialization for a STL BYTE vector.
//	Notes:
//		See CtrlPropertySet
//
template <>
bool CtrlPropertySet(UINT ext_I,
							BYTE tablet_I,
							BYTE control_I,
							BYTE function_I,
							WORD property_I,
							std::vector<BYTE> value_I)
{
	// allocate a buffer
	std::unique_ptr<BYTE[]> buffer(new BYTE[sizeof(EXTPROPERTY) + value_I.size()]);

	// cast the buffer to the extension property data structure
	EXTPROPERTY *prop = (EXTPROPERTY*)buffer.get();

	// fill in the data
	prop->version = 0;
	prop->tabletIndex = tablet_I;
	prop->controlIndex = control_I;
	prop->functionIndex = function_I;
	prop->propertyID = property_I;
	prop->reserved = 0;
	prop->dataSize = static_cast<DWORD>(value_I.size());

	for (size_t i = 0; i < value_I.size(); ++i)
	{
		prop->data[i] = value_I[i];
	}

	// send the command to Wintab and record result
	return (FALSE != gpWTExtSet(ghCtx, ext_I, prop));
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Set the icon for a specified control.
//	Parameters:
//		ext_I - The index of the extension
//		tablet_I - The index of the tablet
//		control_I - The index of the control on the tablet
//		function_I - The index of the function on the control
//		filename_I - Filename of the image to load
//	Return:
//		bool - true if the value correctly set
//
bool SetIcon(UINT ext_I,
				 UINT tablet_I,
				 UINT control_I,
				 UINT function_I,
				 std::string filename_I)
{
	// open a file stream
	std::ifstream imageFile(filename_I.c_str(), std::ios::in | std::ios::binary);

	// create a BYTE vector to hold
	std::vector<BYTE> imageData;

	// fill the vector with the data from the stream
	while (!imageFile.eof())
	{
		imageData.push_back(imageFile.get());
	}

	// send the vector as property "TABLET_PROPERTY_OVERRIDE_ICON"
	return CtrlPropertySet(ext_I, tablet_I, control_I, function_I,
		TABLET_PROPERTY_OVERRIDE_ICON, imageData);
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Sets all properties for a given tablet/control/function.
//	Parameters:
//		tablet_I - The index of the tablet
//		ext_I - The index of the extension
//		control_I - The index of the control on the tablet
//		function_I - The index of the function on the control
//		fp_I - Drawing function to render the control in this app.
//	Return:
//		none
//	Notes:
//		This is a definite testing function.  Normally you would only take over what
//		you need.  This code takes over everything.
//
void SetupPropertiesForFunc(UINT tablet_I,
									 UINT ext_I,
									 UINT control_I,
									 UINT function_I,
									 Drawing::SetupControlPtr fp_I)
{
	// ask if control is available for override?
	const BOOL avail = CtrlPropertyGet<BOOL>(ext_I, tablet_I, control_I, function_I, TABLET_PROPERTY_AVAILABLE);

	// If so, override the control
	if (avail)
	{
		CtrlPropertySet(ext_I, tablet_I, control_I, function_I,
			TABLET_PROPERTY_OVERRIDE, static_cast<BOOL>(TRUE));

		// Give control a custom name.
		// Be aware that these names will show up in Intuos4 OLEDs,
		// so they should be short.
		std::stringstream name;

		switch(ext_I)
		{
			case WTX_EXPKEYS2:
			{
				name << "EK: " << control_I;
				break;
			}
			case WTX_TOUCHRING:
			{
				name << "TR: " << function_I;
				break;
			}
			case WTX_TOUCHSTRIP:
			{
				name << "TS: " << function_I;
				break;
			}
			default:
			{
				name << "";	// unknown control
				break;
			}
		}

		CtrlPropertySet(ext_I, tablet_I, control_I, function_I,
			TABLET_PROPERTY_OVERRIDE_NAME, name.str());
	}

	// Get the location of the control
	const UINT32 location = CtrlPropertyGet<UINT32>(ext_I, tablet_I, control_I,
		function_I, TABLET_PROPERTY_LOCATION);

	// Get the range of values
	const UINT32 min = CtrlPropertyGet<UINT32>(ext_I, tablet_I, control_I, function_I, TABLET_PROPERTY_MIN);
	const UINT32 max = CtrlPropertyGet<UINT32>(ext_I, tablet_I, control_I, function_I, TABLET_PROPERTY_MAX);

	// WARNING - these icons will overwrite displayed key names (eg: "EK: 0, EK: 1, etc.")

	// Set the display properties
	// first check the format of the display.
	const UINT32 iconFmt = CtrlPropertyGet<UINT32>(ext_I, tablet_I, control_I, function_I,
		TABLET_PROPERTY_ICON_FORMAT);
	// TABLET_ICON_FMT_NONE is returned if the control does not have a display
	if (iconFmt != TABLET_ICON_FMT_NONE)
	{
		// Get the width of the display icon
		const UINT32 iconW = CtrlPropertyGet<UINT32>(ext_I, tablet_I, control_I, function_I,
			TABLET_PROPERTY_ICON_WIDTH);
		// Get the height of the display icon
		const UINT32 iconH = CtrlPropertyGet<UINT32>(ext_I, tablet_I, control_I, function_I,
			TABLET_PROPERTY_ICON_HEIGHT);

		// set the icon to "sample.png"
		SetIcon(ext_I, tablet_I, control_I, function_I, "sample.png");
	}

	// Send all the values to the drawing code to display in the client area.
	fp_I(tablet_I, control_I, function_I, avail, location, min, max);
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Iterate through all functions on this control.
//	Parameters:
//		tablet_I - The index of the tablet
//		ext_I - The index of the extension
//		control_I - The index of the control on the tablet
//		fp_I - Drawing function to render the control in this app.
//	Return:
//		none
//	Notes:
//		This is a definite testing function.  Normally you would only take over what
//		you need.  This code takes over everything.
//
void SetupFuncsForControl(UINT tablet_I,
								  UINT ext_I,
								  UINT control_I,
								  Drawing::SetupControlPtr fp_I)
{
	// Get number of functions.
	// Note the function element is ignored for this property.
	const UINT32 numFuncs = CtrlPropertyGet<UINT32>(ext_I, tablet_I, control_I, 0,
		TABLET_PROPERTY_FUNCCOUNT);
	for (UINT i=0; i<numFuncs; i++)
	{
		SetupPropertiesForFunc(tablet_I, ext_I, control_I, i, fp_I);
	}
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Iterate through all controls on this tablet/extension.
//	Parameters:
//		tablet_I - The index of the tablet
//		ext_I - The index of the extension
//		fp_I - Drawing function to render the control in this app.
//	Return:
//		none
//	Notes:
//		This is a definite testing function.  Normally you would only take over what
//		you need.  This code takes over everything.
//
void SetupControlsForExtension(UINT tablet_I,
										 UINT ext_I,
										 Drawing::SetupControlPtr fp_I)
{
	// Get number of controls of this type
	// Note the control and function elements are ignored for this property.
	const UINT32 numCtrls = CtrlPropertyGet<UINT32>(ext_I, tablet_I, 0, 0,
		TABLET_PROPERTY_CONTROLCOUNT);

	for (UINT32 idx = 0; idx < numCtrls; ++idx)
	{
		// You only need to setup controls you wish to override
		SetupFuncsForControl(tablet_I, ext_I, idx, fp_I);
	}
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Iterate through all extensions on this tablet.
//	Parameters:
//		tablet_I - The index of the tablet
//	Return:
//		none
//	Notes:
//		This is a definite testing function.  Normally you would only take over what
//		you need.  This code takes over everything.
//
void SetupControlsForTablet(UINT tablet_I)
{
	// Express Keys
	SetupControlsForExtension(tablet_I, WTX_EXPKEYS2,   Drawing::SetupKey);

	// Touch Rings
	SetupControlsForExtension(tablet_I, WTX_TOUCHRING,  Drawing::SetupRing);

	// Touch Strips
	SetupControlsForExtension(tablet_I, WTX_TOUCHSTRIP, Drawing::SetupStrip);
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Iterate through all controls/functions on this tablet/extension.
//	Parameters:
//		tablet_I - The index of the tablet
//		ext_I - The index of the extension
//	Return:
//		none
//	Notes:
//		This is a definite testing function.  Normally you would only take over what
//		you need.  This code takes over everything.
//
void RemoveOverridesForExtension(UINT tablet_I,
											UINT ext_I)
{
	// Get number of controls of this type
	// Note the control and function elements are ignored for this property.
	const UINT32 numCtrls = CtrlPropertyGet<UINT32>(ext_I, tablet_I, 0, 0,
		TABLET_PROPERTY_CONTROLCOUNT);
	for (UINT control=0; control<numCtrls; control++)
	{
		// Get number of functions.
		// Note the function element is ignored for this property.
		const UINT32 numFuncs = CtrlPropertyGet<UINT32>(ext_I, tablet_I, control, 0,
			TABLET_PROPERTY_FUNCCOUNT);
		for (UINT function = 0; function < numFuncs; ++function)
		{
			CtrlPropertySet(ext_I, tablet_I, control, function,
				TABLET_PROPERTY_OVERRIDE, static_cast<BOOL>(FALSE));
		}
	}
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Iterate through all extensions on this tablet.
//	Parameters:
//		tablet_I - The index of the tablet
//	Return:
//		none
//
void RemoveOverridesForTablet(UINT tablet_I)
{
	// Express Keys
	RemoveOverridesForExtension(tablet_I, WTX_EXPKEYS2);

	// Touch Rings
	RemoveOverridesForExtension(tablet_I, WTX_TOUCHRING);

	// Touch Strips
	RemoveOverridesForExtension(tablet_I, WTX_TOUCHSTRIP);
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Create a wintab context that will get the extension data.
//	Parameters:
//		hWnd_I - The handle to the application window that will receive messages.
//	Return:
//		bool - true if context was created
//
bool Tablet::Init(HWND hWnd_I)
{
	if (!LoadWintab())
	{
		ShowError("Wintab not available or not all Wintab functions available.");
		return FALSE;
	}

	// check if WinTab available
	if (!gpWTInfoA(0, 0, NULL))
	{
		ShowError("WinTab Services Not Available.");
		return FALSE;
	}

	// Verify that the extensions we're targeting are available
	WTPKT lTouchRing_Mask = 0;
	UINT extIndex_TouchRing = 0;

	// get the extension index for the touch ring
	if (FindWTExtension(WTX_TOUCHRING, extIndex_TouchRing))
	{
		// get the extension mask for the touch ring
		gpWTInfoA(WTI_EXTENSIONS + extIndex_TouchRing, EXT_MASK, &lTouchRing_Mask);
	}
	else
	{
		ShowError("TouchRing extension not found.");
	}

	WTPKT lTouchStrip_Mask = 0;
	UINT extIndex_TouchStrip = 0;

	// get the extension index for the touch strip
	if (FindWTExtension(WTX_TOUCHSTRIP, extIndex_TouchStrip))
	{
		// get the extension mask for the touch strip
		gpWTInfoA(WTI_EXTENSIONS + extIndex_TouchStrip, EXT_MASK, &lTouchStrip_Mask);
	}
	else
	{
		ShowError("TouchStrip Extension not found.");
	}

	WTPKT lExpKeys_Mask = 0;
	UINT extIndex_ExpKeys = 0;

	// get the extension index for the express keys
	if (FindWTExtension(WTX_EXPKEYS2, extIndex_ExpKeys))
	{
		// get the extension mask for the express keys
		gpWTInfoA(WTI_EXTENSIONS + extIndex_ExpKeys, EXT_MASK, &lExpKeys_Mask);
	}
	else
	{
		ShowError("ExpKeys Extension not found.");
	}

	LOGCONTEXT lcContext = {0};

	// ask for the default system context
	if (!gpWTInfoA(WTI_DEFSYSCTX, 0, &lcContext))
	{
		ShowError("Couldn't retrieve default context information.");
		return false;
	}

	// set the packet data to include extension data
	lcContext.lcPktData = (PK_X | PK_Y | PK_BUTTONS | PK_CURSOR) | lTouchRing_Mask | lTouchStrip_Mask | lExpKeys_Mask;
	lcContext.lcPktMode = PACKETMODE;

	// have the context send messages to the window
	lcContext.lcOptions |= CXO_MESSAGES;

	// Open the Wintab context
	ghCtx = gpWTOpenA(hWnd_I, (LPLOGCONTEXTA)&lcContext, TRUE);
	if (!ghCtx)
	{
		ShowError("Couldn't open a context.");
		return false;
	}

	// This counts the number of tablets that the driver detects as
	// having been attached since the last time the driver settings
	// were cleared.  Wintab only supports 16 attached tablets.
	// When WTInfo returns zero, we know that number.
	for (int idx = 0; idx < 16; ++idx)
	{
		LOGCONTEXT lcTemp = { 0 };
		const int numBytes = gpWTInfoA(WTI_DDCTXS + idx, 0, &lcTemp);
		if (numBytes)
		{
			SetupControlsForTablet(idx);
		}
		else
		{
			break;
		}
	}
	return true;
}

////////////////////////////////////////////////////////////////////////////////
//	Purpose:
//		Remove the extension overides and close the context.
//	Parameters:
//		none
//	Return:
//		none
//
void Tablet::Cleanup(void)
{
	// Remove overrides for each tablet
	for (UINT8 i = 0; i < gNumTablets; ++i)
	{
		RemoveOverridesForTablet(i);
	}
	gNumTablets = 0;

	// close the context
	if (ghCtx)
	{
		gpWTClose(ghCtx);
		ghCtx = NULL;
	}

	UnloadWintab();
}
