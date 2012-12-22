// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace nwChat
{
	[Register ("ServerWindowController")]
	partial class ServerWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTableView connectionView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (connectionView != null) {
				connectionView.Dispose ();
				connectionView = null;
			}
		}
	}

	[Register ("ServerWindow")]
	partial class ServerWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
