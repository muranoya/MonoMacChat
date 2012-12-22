
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace nwChat
{
    public partial class StartupWindow : MonoMac.AppKit.NSWindow
    {
		#region Constructors
		
        // Called when created from unmanaged code
        public StartupWindow(IntPtr handle) : base (handle)
        {
            Initialize();
        }
		
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public StartupWindow(NSCoder coder) : base (coder)
        {
            Initialize();
        }
		
        // Shared initialization code
        void Initialize()
        {
        }
		
		#endregion
    }
}

