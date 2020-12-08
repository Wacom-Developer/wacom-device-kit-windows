///////////////////////////////////////////////////////////////////////////////
//
//	PURPOSE
//		Wintab Extensions render code for WintabDN
//
//	COPYRIGHT
//		Copyright (c) 2019-2020 Wacom Co., Ltd.
//
//		The text and information contained in this file may be freely used,
//		copied, or distributed without compensation or licensing restrictions.
//
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace FormExtTestApp
{
	public class TabletGraphicList : ArrayList
	{

	}

	/// <summary>
	/// Sets up parameters necessary to draw a specific control.
	/// </summary>
	public delegate void DrawControlsSetupFunction(UInt32 tabletIndex_I, int controlIndex_I, int functionIndex_I, bool available_I, int locationIndex_I, int min_I, int max_I);

	public struct TabletGraphic
	{
		public int tabletID;

		public ExpressKeyGraphic[] expKeys;
		public LinearControlGraphic[] touchRings;
		public LinearControlGraphic[] touchStrips;
	}

	public struct ExpressKeyGraphic
	{
		public bool available;
		public bool down;
		public int location;
	}

	public struct LinearControlGraphic
	{
		public LinearControlMode[] modes;
		public bool available;
		public bool down;
		public int position;
		public int min;
		public int max;
		public int location;
	}

	public struct LinearControlMode
	{
		public bool active;
	}

	/// <summary>
	/// Maintains state for extension controls that are rendered.
	/// </summary>
	class ExtensionControlState
	{
		private Dictionary<UInt32, TabletGraphic> mTablets = new Dictionary<UInt32, TabletGraphic>();

		public Dictionary<UInt32, TabletGraphic> Tablets
		{
			get { return mTablets; }
		}

		public ExtensionControlState() { }

		/// <summary>
		/// Adds a new Express Key for specified tablet if not already created; else it's a NO-OP.
		/// </summary>
		/// <param name="tabletIndex_I">index of tablet where control to be added</param>
		/// <param name="controlIndex_I">index of control to add</param>
		public void AddExpressKey(UInt32 tabletIndex_I, int controlIndex_I)
		{
			if (!mTablets.ContainsKey(tabletIndex_I))
			{
				mTablets[tabletIndex_I] = new TabletGraphic();
			}

			TabletGraphic tabGraphic = mTablets[tabletIndex_I];

			if (tabGraphic.expKeys == null)
			{
				tabGraphic.expKeys = new ExpressKeyGraphic[1];
				mTablets[tabletIndex_I] = tabGraphic;
			}

			if (controlIndex_I >= mTablets[tabletIndex_I].expKeys.Length)
			{
				Array.Resize(ref tabGraphic.expKeys, tabGraphic.expKeys.Length + 1);
				mTablets[tabletIndex_I] = tabGraphic;
			}
		}

		/// <summary>
		/// Adds a new Touch Ring for specified tablet if not already created; else it's a NO-OP.
		/// </summary>
		/// <param name="tabletIndex_I">index of tablet where control to be added</param>
		/// <param name="controlIndex_I">index of control to add</param>
		/// <param name="modeIndex_I">index of the mode</param>
		public void AddTouchRing(UInt32 tabletIndex_I, int controlIndex_I, int modeIndex_I)
		{
			if (!mTablets.ContainsKey(tabletIndex_I))
			{
				mTablets[tabletIndex_I] = new TabletGraphic();
			}

			TabletGraphic tabGraphic = mTablets[tabletIndex_I];

			if (tabGraphic.touchRings == null)
			{
				tabGraphic.touchRings = new LinearControlGraphic[1];
				mTablets[tabletIndex_I] = tabGraphic;
			}

			if (controlIndex_I >= mTablets[tabletIndex_I].touchRings.Length)
			{
				Array.Resize(ref tabGraphic.touchRings, mTablets[tabletIndex_I].touchRings.Length + 1);
				mTablets[tabletIndex_I] = tabGraphic;
			}

			if (mTablets[tabletIndex_I].touchRings[controlIndex_I].modes == null)
			{
				mTablets[tabletIndex_I].touchRings[controlIndex_I].modes = new LinearControlMode[1];
			}

			if (modeIndex_I >= (int)mTablets[tabletIndex_I].touchRings[controlIndex_I].modes.Length)
			{
				Array.Resize(ref tabGraphic.touchRings[controlIndex_I].modes, tabGraphic.touchRings[controlIndex_I].modes.Length + 1);
				mTablets[tabletIndex_I] = tabGraphic;
			}
		}

		/// <summary>
		/// Adds a new Touch Strip for specified tablet if not already created; else it's a NO-OP.
		/// </summary>
		/// <param name="tabletIndex_I">index of tablet where control to be added</param>
		/// <param name="controlIndex_I">index of control to add</param>
		/// <param name="modeIndex_I">index of the mode</param>
		public void AddTouchStrip(UInt32 tabletIndex_I, int controlIndex_I, int modeIndex_I)
		{
			if (!mTablets.ContainsKey(tabletIndex_I))
			{
				mTablets[tabletIndex_I] = new TabletGraphic();
			}

			TabletGraphic tabGraphic = mTablets[tabletIndex_I];

			if (tabGraphic.touchStrips == null)
			{
				tabGraphic.touchStrips = new LinearControlGraphic[1];
				mTablets[tabletIndex_I] = tabGraphic;
			}

			if (controlIndex_I >= mTablets[tabletIndex_I].touchStrips.Length)
			{
				Array.Resize(ref tabGraphic.touchStrips, mTablets[tabletIndex_I].touchStrips.Length + 1);
				mTablets[tabletIndex_I] = tabGraphic;
			}

			if (mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes == null)
			{
				mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes = new LinearControlMode[1];
			}

			if (modeIndex_I >= (int)mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes.Length)
			{
				Array.Resize(ref tabGraphic.touchStrips[controlIndex_I].modes, tabGraphic.touchStrips[controlIndex_I].modes.Length + 1);
				mTablets[tabletIndex_I] = tabGraphic;
			}
		}

		/// <summary>
		/// Set up ExpressKey structures to match on-tablet buttons.
		/// </summary>
		/// <param name="tabletIndex_I"></param>
		/// <param name="controlIndex_I"></param>
		/// <param name="functionIndex_I"></param>
		/// <param name="available_I"></param>
		/// <param name="locationIndex_I"></param>
		/// <param name="min_I"></param>
		/// <param name="max_I"></param>
		public void SetupExpressKey(UInt32 tabletIndex_I, int controlIndex_I, int functionIndex_I, bool available_I, int locationIndex_I, int min_I, int max_I)
		{
			DumpSetup("EK", tabletIndex_I, controlIndex_I, functionIndex_I, available_I, locationIndex_I, min_I, max_I);

			AddExpressKey(tabletIndex_I, controlIndex_I);

			mTablets[tabletIndex_I].expKeys[controlIndex_I].available = available_I;
			mTablets[tabletIndex_I].expKeys[controlIndex_I].down = false;
			mTablets[tabletIndex_I].expKeys[controlIndex_I].location = locationIndex_I;
		}

		/// <summary>
		/// Set up TouchRing structures to match on-tablet touch rings.
		/// </summary>
		/// <param name="tabletIndex_I"></param>
		/// <param name="controlIndex_I"></param>
		/// <param name="functionIndex_I"></param>
		/// <param name="available_I"></param>
		/// <param name="locationIndex_I"></param>
		/// <param name="min_I"></param>
		/// <param name="max_I"></param>
		/// 
		public void SetupTouchRing(UInt32 tabletIndex_I, int controlIndex_I, int modeIndex_I, bool available_I, int locationIndex_I, int min_I, int max_I)
		{
			DumpSetup("TR", tabletIndex_I, controlIndex_I, modeIndex_I, available_I, locationIndex_I, min_I, max_I);

			AddTouchRing(tabletIndex_I, controlIndex_I, modeIndex_I);

			mTablets[tabletIndex_I].touchRings[controlIndex_I].available = available_I;
			mTablets[tabletIndex_I].touchRings[controlIndex_I].min = min_I;
			mTablets[tabletIndex_I].touchRings[controlIndex_I].max = max_I - 1;
			mTablets[tabletIndex_I].touchRings[controlIndex_I].location = locationIndex_I;
			mTablets[tabletIndex_I].touchRings[controlIndex_I].down = false;
			mTablets[tabletIndex_I].touchRings[controlIndex_I].position = 0;
			mTablets[tabletIndex_I].touchRings[controlIndex_I].modes[modeIndex_I].active = false;
		}

		/// <summary>
		/// Set up TouchStrip structures to match on-tablet touch strips.
		/// </summary>
		/// <param name="tabletIndex_I"></param>
		/// <param name="controlIndex_I"></param>
		/// <param name="functionIndex_I"></param>
		/// <param name="available_I"></param>
		/// <param name="locationIndex_I"></param>
		/// <param name="min_I"></param>
		/// <param name="max_I"></param>
		public void SetupTouchStrip(UInt32 tabletIndex_I, int controlIndex_I, int modeIndex_I, bool available_I, int locationIndex_I, int min_I, int max_I)
		{
			DumpSetup("TS", tabletIndex_I, controlIndex_I, modeIndex_I, available_I, locationIndex_I, min_I, max_I);

			AddTouchStrip(tabletIndex_I, controlIndex_I, modeIndex_I);

			mTablets[tabletIndex_I].touchStrips[controlIndex_I].available = available_I;
			mTablets[tabletIndex_I].touchStrips[controlIndex_I].min = min_I;
			mTablets[tabletIndex_I].touchStrips[controlIndex_I].max = max_I;
			mTablets[tabletIndex_I].touchStrips[controlIndex_I].location = locationIndex_I;
			mTablets[tabletIndex_I].touchStrips[controlIndex_I].down = false;
			mTablets[tabletIndex_I].touchStrips[controlIndex_I].position = 0;
			mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes[modeIndex_I].active = false;
		}

		/// <summary>
		/// Updates one ExpressKey state (up or down).
		/// Should be called when app receives a WT_PACKETEXT message.
		/// </summary>
		/// <param name="tabletIndex_I"></param>
		/// <param name="controlIndex_I"></param>
		/// <param name="locationIndex_I"></param>
		/// <param name="state_I"></param>
		public void UpdateExpressKey(UInt32 tabletIndex_I, int controlIndex_I, int locationIndex_I, uint state_I)
		{
			DumpUpdate("EK", tabletIndex_I, controlIndex_I, locationIndex_I, (int)state_I);

			try
			{
				// Should always be ExpressKeys.
				if (mTablets[tabletIndex_I].expKeys == null)
				{
					throw new Exception("Oops - expKeys not created for this tablet");
				}

				if (mTablets.ContainsKey(tabletIndex_I))
				{
					if (controlIndex_I < mTablets[tabletIndex_I].expKeys.Length)
					{
						mTablets[tabletIndex_I].expKeys[controlIndex_I].down = (state_I != 0);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/// <summary>
		/// Updates a touchring finger position. 
		/// Should be called when app receives a WT_PACKETEXT message.
		/// </summary>
		/// <param name="tabletIndex_I"></param>
		/// <param name="controlIndex_I"></param>
		/// <param name="modeIndex_I"></param>
		/// <param name="position_I"></param>
		public void UpdateTouchRing(UInt32 tabletIndex_I, int controlIndex_I, int modeIndex_I, uint position_I)
		{
			DumpUpdate("TR", tabletIndex_I, controlIndex_I, modeIndex_I, (int)position_I);

			try
			{
				if (mTablets.ContainsKey(tabletIndex_I))
				{
					// This tablet may not have touch rings.
					if (mTablets[tabletIndex_I].touchRings != null &&
						controlIndex_I < (int)mTablets[tabletIndex_I].touchRings.Length)
					{
						mTablets[tabletIndex_I].touchRings[controlIndex_I].down = (position_I != 0);
						mTablets[tabletIndex_I].touchRings[controlIndex_I].position = (int)(position_I - 1);

						for (int index = 0; index < (int)mTablets[tabletIndex_I].touchRings[controlIndex_I].modes.Length; index++)
						{
							mTablets[tabletIndex_I].touchRings[controlIndex_I].modes[index].active = (index == modeIndex_I);
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/// <summary>
		/// Updates a touchstrip finger position. 
		/// Should be called when app receives a WT_PACKETEXT message.
		/// </summary>
		/// <param name="tabletIndex_I"></param>
		/// <param name="controlIndex_I"></param>
		/// <param name="modeIndex_I"></param>
		/// <param name="position_I"></param>
		public void UpdateTouchStrip(UInt32 tabletIndex_I, int controlIndex_I, int modeIndex_I, uint position_I)
		{
			DumpUpdate("TR", tabletIndex_I, controlIndex_I, modeIndex_I, (int)position_I);

			try
			{
				if (mTablets.ContainsKey(tabletIndex_I))
				{
					// This tablet may not have touch rings.
					if (mTablets[tabletIndex_I].touchStrips != null &&
						controlIndex_I < (int)mTablets[tabletIndex_I].touchStrips.Length)
					{
						mTablets[tabletIndex_I].touchStrips[controlIndex_I].down = (position_I != 0);
						mTablets[tabletIndex_I].touchStrips[controlIndex_I].position = (int)(position_I - 1);

						for (int index = 0; index < (int)mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes.Length; index++)
						{
							mTablets[tabletIndex_I].touchStrips[controlIndex_I].modes[index].active = (index == modeIndex_I);
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/// <summary>
		/// Setup extension data dumped to debug output.
		/// </summary>
		/// <param name="ctrlType_I"></param>
		/// <param name="tabletIndex_I"></param>
		/// <param name="controlIndex_I"></param>
		/// <param name="functionIndex_I"></param>
		/// <param name="available_I"></param>
		/// <param name="locationIndex_I"></param>
		/// <param name="min_I"></param>
		/// <param name="max_I"></param>
		private void DumpSetup(String ctrlType_I, UInt32 tabletIndex_I, int controlIndex_I, int functionIndex_I, bool available_I, int locationIndex_I, int min_I, int max_I)
		{
			Debug.WriteLine(ctrlType_I +
				": tab:" + tabletIndex_I +
				"; ctrl:" + controlIndex_I +
				"; func:" + functionIndex_I +
				"; avail:" + available_I.ToString() +
				"; loc:" + locationIndex_I +
				"; min:" + min_I +
				"; max:" + max_I);
		}

		/// <summary>
		/// Update extension data dumped to debug output.
		/// </summary>
		/// <param name="ctrlType_I"></param>
		/// <param name="tabletIndex_I"></param>
		/// <param name="controlIndex_I"></param>
		/// <param name="locationIndex_I"></param>
		/// <param name="state_I"></param>
		private void DumpUpdate(String ctrlType_I, UInt32 tabletIndex_I, int controlIndex_I, int locationIndex_I, int state_I)
		{
			Debug.WriteLine(ctrlType_I +
				": tab:" + tabletIndex_I +
				"; ctrl:" + controlIndex_I +
				"; loc:" + locationIndex_I +
				"; state:" + state_I);
		}
	}
}
