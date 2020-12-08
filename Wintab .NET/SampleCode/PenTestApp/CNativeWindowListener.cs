using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WintabDN;

namespace FormTestApp
{
    // NativeWindow class to listen to operating system messages.
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    internal class CNativeWindowListener : NativeWindow
    {
        private TestForm parent;

        private const int WM_CREATE = 0x0001;
        private const int WM_ACTIVATE = 0x0006;


        public CNativeWindowListener(TestForm parent)
        {

            parent.HandleCreated += new EventHandler(this.OnHandleCreated);
            parent.HandleDestroyed += new EventHandler(this.OnHandleDestroyed);
            this.parent = parent;
        }

        // Listen for the control's window creation and then hook into it.
        internal void OnHandleCreated(object sender, EventArgs e)
        {
            // Window is now created, assign handle to NativeWindow.
            AssignHandle(((TestForm)sender).Handle);
        }
        internal void OnHandleDestroyed(object sender, EventArgs e)
        {
            // Window was destroyed, release hook.
            ReleaseHandle();
        }

        /// <summary>
        /// Listen for opertaing system messages.  Handle Wintab-specific messages.
        /// </summary>
        /// <param name="eventMsg">Windows event message.</param>
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message eventMsg)
        {
            // Listen for operating system messages
            //parent.testTextBox.AppendText("msg: " + eventMsg + "\n");

            switch (eventMsg.Msg)
            {
                case WM_ACTIVATE:
                    {
                        HCTX hCtx = parent.HLogContext;
  
                        if (hCtx > 0)
                        {
                            Int32 activateState = eventMsg.WParam.ToInt32() & 0xFFFF;

                            CWintabFuncs.WTEnable(parent.HLogContext, (activateState > 0));

                            if (activateState > 0)
                            {
                                CWintabFuncs.WTOverlap(hCtx, true);
                            }
                        }
                    }
                    break;

                case (int)WintabEventMessage.WT_PACKET:
                    {
                        // Notify the parent form that a data packet was received.                   
                        UInt32 pktSerialNum = (UInt32)eventMsg.WParam;
                        UInt32 hContext = (UInt32)eventMsg.LParam;

                        parent.HandleEvent_WT_PACKET(hContext, pktSerialNum);
                    }
                break;

                default:
                    base.WndProc(ref eventMsg);
                    break;
            }
        }
    }

}
