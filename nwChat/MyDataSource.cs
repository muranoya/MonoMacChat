using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Collections.Generic;

namespace nwChat
{
    public class ConnectionMember : NSObject
    {
        public string Name { get; set; }
        public int ID { get; set; }
        
        public ConnectionMember(string name, int id)
        {
            this.Name = name;
            this.ID = id;
        }
    }

    public class MyDataSource : NSTableViewDataSource
    {
        public List<ConnectionMember> members { get; set; }

        [Export ("numberOfRowsInTableView:")]
        public int numberOfRowsInTableView(NSTableView view)
        {
            return members.Count;
        }

        [Export ("tableView:objectValueForTableColumn:row:")]
        public NSObject objectValueForTableColumn(NSTableView view, NSTableColumn col, int row)
        {
            if (members.Count > row && row >= 0)
            {
                var m = members[row];
                var ident = col.Identifier.ToString();
                if (ident == "colName")
                    return (NSString)m.Name;
                else if (ident == "colID")
                    return (NSString)m.ID.ToString();
            }
            return (NSString)"invalid value";
        }

        public MyDataSource()
        {
            this.members = new List<ConnectionMember>();
        }
    }
}

