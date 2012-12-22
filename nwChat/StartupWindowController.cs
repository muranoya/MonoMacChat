using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace nwChat
{
    public partial class StartupWindowController : MonoMac.AppKit.NSWindowController
    {
		#region Constructors
        // Called when created from unmanaged code
        public StartupWindowController(IntPtr handle) : base (handle)
        {
            Initialize();
        }
		
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public StartupWindowController(NSCoder coder) : base (coder)
        {
            Initialize();
        }
		
        // Call to load from the XIB/NIB file
        public StartupWindowController() : base ("StartupWindow")
        {
            Initialize();
        }
		
        // Shared initialization code
        void Initialize()
        {
        }
		#endregion
		
        partial void ClickCancelButton(NSObject sender)
        {
            System.Environment.Exit(0);
        }

        partial void ClickDoneButton(NSObject sender)
        {
            if (modeSelecta.SelectedSegment == 0)
            {
                ServerWindowController s = new ServerWindowController(portTextField.IntValue, handleNameTextField.StringValue);
                s.Window.Title = "Server (" + portTextField.StringValue + ")";
                s.LoadWindow();
            }
            else
            {
                ClientWindowController c = new ClientWindowController(handleNameTextField.StringValue, hostNameTextField.StringValue, portTextField.IntValue);
                c.Window.Title = handleNameTextField.StringValue;
                c.LoadWindow();
            }
            this.Close();
        }

        partial void ClickModeSelecta(NSObject sender)
        {
            hostNameTextField.Enabled = handleNameTextField.Enabled = modeSelecta.SelectedSegment == 1;
        }

        //strongly typed window accessor
        public new StartupWindow Window { get { return (StartupWindow)base.Window; } }
    }
}
