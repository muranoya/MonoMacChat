using System;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace nwChat
{
    public class MyNSTextFieldDelegate : NSTextFieldDelegate
    {
        Action<NSControl> onEnterPressed;
        
        public override bool DoCommandBySelector(NSControl control, NSTextView textView, MonoMac.ObjCRuntime.Selector commandSelector)
        {
            if (control != null && "insertNewline:".Equals(commandSelector.Name))
            {
                onEnterPressed(control);
                return true;
            }
            return false;
        }
        
        public MyNSTextFieldDelegate(Action<NSControl> act)
        {
            onEnterPressed = act;
        }
    }
}

