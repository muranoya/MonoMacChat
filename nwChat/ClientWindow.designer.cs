// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace nwChat
{
	[Register ("ClientWindowController")]
	partial class ClientWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField mainTextField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField inputTextField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView memberView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSDrawer memberDrawer { get; set; }

		[Outlet]
		MonoMac.AppKit.NSMenuItem directMessageMenu { get; set; }

		[Action ("ClickShowMember:")]
		partial void ClickShowMember (MonoMac.Foundation.NSObject sender);

		[Action ("ClickSendDM:")]
		partial void ClickSendDM (MonoMac.Foundation.NSObject sender);

		[Action ("ClickMemberView:")]
		partial void ClickMemberView (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (mainTextField != null) {
				mainTextField.Dispose ();
				mainTextField = null;
			}

			if (inputTextField != null) {
				inputTextField.Dispose ();
				inputTextField = null;
			}

			if (memberView != null) {
				memberView.Dispose ();
				memberView = null;
			}

			if (memberDrawer != null) {
				memberDrawer.Dispose ();
				memberDrawer = null;
			}

			if (directMessageMenu != null) {
				directMessageMenu.Dispose ();
				directMessageMenu = null;
			}
		}
	}

	[Register ("ClientWindow")]
	partial class ClientWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
