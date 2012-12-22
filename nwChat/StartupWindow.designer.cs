// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace nwChat
{
	[Register ("StartupWindowController")]
	partial class StartupWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField handleNameTextField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField hostNameTextField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField portTextField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSegmentedControl modeSelecta { get; set; }

		[Action ("ClickCancelButton:")]
		partial void ClickCancelButton (MonoMac.Foundation.NSObject sender);

		[Action ("ClickDoneButton:")]
		partial void ClickDoneButton (MonoMac.Foundation.NSObject sender);

		[Action ("ClickModeSelecta:")]
		partial void ClickModeSelecta (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (handleNameTextField != null) {
				handleNameTextField.Dispose ();
				handleNameTextField = null;
			}

			if (hostNameTextField != null) {
				hostNameTextField.Dispose ();
				hostNameTextField = null;
			}

			if (portTextField != null) {
				portTextField.Dispose ();
				portTextField = null;
			}

			if (modeSelecta != null) {
				modeSelecta.Dispose ();
				modeSelecta = null;
			}
		}
	}

	[Register ("StartupWindow")]
	partial class StartupWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
