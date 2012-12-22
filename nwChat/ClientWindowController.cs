
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace nwChat
{
    public partial class ClientWindowController : MonoMac.AppKit.NSWindowController
    {
        ChatController cc;
        MyDataSource data;

        #region Constructors
	    // Called when created from unmanaged code
        public ClientWindowController(IntPtr handle) : base (handle)
        {
            Initialize();
        }
		
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public ClientWindowController(NSCoder coder) : base (coder)
        {
            Initialize();
        }
		
        // Call to load from the XIB/NIB file
        public ClientWindowController() : base ("ClientWindow")
        {
            Initialize();
        }
		
        // Shared initialization code
        void Initialize()
        {
        }

        public ClientWindowController(string name, string host, int port) : base("ClientWindow")
        {
            Initialize();

            cc = new ChatController(host, port, name);
        }
		
		#endregion
		
        private void ShowChatMSG(string name, string msg)
        {
            string str = name + "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + Environment.NewLine;

            if (mainTextField.StringValue.Length == 0)
                mainTextField.StringValue = str;
            else
                mainTextField.StringValue += str;
        }
        private void JoinMember(string name)
        {
            string str = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + name + " has joined the chat." + Environment.NewLine;
            if (mainTextField.StringValue.Length == 0)
                mainTextField.StringValue = str;
            else
                mainTextField.StringValue += str;
        }
        private void ShowDMSG(string toname, string fromname, string msg)
        {
            string str = "DM:from " + fromname + "/to " + toname + "[" + DateTime.Now.ToString("HH:mm:ss") + "]" + msg + Environment.NewLine;
            if (mainTextField.StringValue.Length == 0)
                mainTextField.StringValue = str;
            else
                mainTextField.StringValue += str;
        }

        partial void ClickShowMember(NSObject sender)
        {
            if (NSDrawerState.Open.Equals(memberDrawer.State))
            {
                memberDrawer.Close(this);
            }
            else
            {
                memberDrawer.Open(this);
            }
        }

        partial void ClickSendDM(NSObject sender)
        {
            if (memberView.SelectedRowCount > 0)
            {
                var item = data.members[memberView.SelectedRow];
              
                NSAlert alert = new NSAlert();
                NSTextField textbox = new NSTextField(new System.Drawing.RectangleF(0.0f, 0.0f, 250.0f, 24.0f));
                alert.AccessoryView = textbox;

                alert.AddButton("OK");
                alert.AddButton("Cancel");

                alert.AlertStyle = NSAlertStyle.Informational;
                alert.MessageText = "Send a direct message to " + item.Name + ".";

                alert.BeginSheetForResponse(this.Window, (ret)=>{
                    if (ret == (int)NSAlertButtonReturn.First)
                    {
                        var str = textbox.StringValue;
                        if (str.Length > 0)
                        {
                            cc.SendDirectMSG(str, item.ID);
                            ShowDMSG(item.Name, cc.MyName, str);
                        }
                    }
                });
            }
        }

        partial void ClickMemberView(NSObject sender)
        {
            directMessageMenu.Enabled = memberView.SelectedRowCount > 0;
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            data = new MyDataSource();
            memberView.DataSource = data;

            inputTextField.Delegate = new MyNSTextFieldDelegate((control) => {
                if (control != null && control.StringValue.Length > 0)
                {
                    cc.SendMSG(control.StringValue);
                    ShowChatMSG(cc.MyName, control.StringValue);
                    control.StringValue = "";
                }
            });

            cc.ReceiveAssignID += (newID) => this.InvokeOnMainThread(() => Window.Title = cc.MyName + " - " + newID);
            cc.ConnectionClose += () => memberDrawer.InvokeOnMainThread(() => {
                StartupWindowController s = new StartupWindowController();
                s.Window.MakeKeyAndOrderFront(this);
                this.Close();
            });
            cc.ReceiveIntroduce += (name, senderID) => mainTextField.InvokeOnMainThread(() => {
                JoinMember(name);
                data.members.Add(new ConnectionMember(name, senderID));
                memberView.ReloadData();
            });
            cc.ReceiveChatMessage += (name, msg, senderID) => mainTextField.InvokeOnMainThread(() => ShowChatMSG(name, msg));
            cc.ReceiveDirectMessage += (name, msg, senderID) => mainTextField.InvokeOnMainThread(()=>{
                ShowDMSG(cc.MyName, name, msg);
            });
            cc.ReceiveMemberList += () => memberDrawer.InvokeOnMainThread(() => {
                var e = cc.GetPeopleList();
                foreach (var n in e)
                    data.members.Add(new ConnectionMember(n.Name, n.ID));
                memberView.ReloadData();
            });
            cc.ReceiveLeave += (leaveID) => memberDrawer.InvokeOnMainThread(() => {
                var item = data.members.FirstOrDefault(c=>c.ID == leaveID);
                if (item != null)
                {
                    data.members.Remove(item);
                    memberView.ReloadData();
                }
            });

            cc.Start();
        }

        //strongly typed window accessor
        public new ClientWindow Window
        {
            get
            {
                return (ClientWindow)base.Window;
            }
        }
    }
}
