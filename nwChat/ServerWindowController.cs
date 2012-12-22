
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace nwChat
{
    public partial class ServerWindowController : MonoMac.AppKit.NSWindowController
    {
        ChatController cc;
        MyDataSource tabledata;

		#region Constructors
		// Called when created from unmanaged code
        public ServerWindowController(IntPtr handle) : base (handle)
        {
            Initialize();
        }
		
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public ServerWindowController(NSCoder coder) : base (coder)
        {
            Initialize();
        }
		
        // Call to load from the XIB/NIB file
        public ServerWindowController() : base ("ServerWindow")
        {
            Initialize();
        }
		
        // Shared initialization code
        void Initialize()
        {
        }
		
        public ServerWindowController(int port, string name) : base("ServerWindow")
        {
            Initialize();

            cc = new ChatController(port, name);
        }
     	#endregion

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            tabledata = new MyDataSource();
            connectionView.DataSource = tabledata;

            cc.ReceiveIntroduce += (name, senderID) => {
                tabledata.members.Add(new ConnectionMember(name, senderID));
                connectionView.InvokeOnMainThread(()=>connectionView.ReloadData());
            };
            cc.ConnectionClose += () => connectionView.InvokeOnMainThread(()=>{
                tabledata.members.Clear();
                var e = cc.GetPeopleList();
                foreach (var n in e)
                    tabledata.members.Add(new ConnectionMember(n.Name, n.ID));
                connectionView.ReloadData();
            });
            cc.Start();
        }
		
        //strongly typed window accessor
        public new ServerWindow Window { get { return (ServerWindow)base.Window; } }
    }
}
