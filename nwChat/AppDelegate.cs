using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace nwChat
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        StartupWindowController startupWindowController;

        public AppDelegate()
        {
        }

        public override void FinishedLaunching(NSObject notification)
        {
            startupWindowController = new StartupWindowController();
            startupWindowController.Window.MakeKeyAndOrderFront(this);
        }
    }
}

