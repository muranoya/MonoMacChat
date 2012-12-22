using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace nwChat
{
    public class ChatController
    {
        private class Member
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public NetworkStream Connection { get; set; }

            public Member(string name, int id, NetworkStream ns)
            {
                this.ID = id;
                this.Name = name;
                this.Connection = ns;
            }
        }

        public struct People
        {
            private int id;
            public int ID { get { return this.id; } }
            private string name;
            public string Name { get { return this.name; } }

            public People(string name, int id)
            {
                this.name = name;
                this.id = id;
            }
        }

        private enum Protocol : byte
        {
            Unknown,
            ChatMSG,
            AssignID,
            DirectMSG,
            Introduce,
            MemberList,
            Leave,
            Test,
        }
        
        private static Encoding enc = Encoding.UTF8;
        private static readonly string CantUseServerFunc = "Can't use this function in client mode.";
        private static readonly string CantUseClientFunc = "Can't use this function in server mode.";

        private int serverID;
        private NetworkStream clientNs;

        private List<Member> members;

        private bool isServer;
        public bool IsServer { get { return isServer; } }

        private string host;
        public string Host { get { return this.host; } }

        private int port;
        public int Port { get { return this.port; } }

        private string myname;
        public string MyName { get { return this.myname; } }

        private int myid;
        public int MyID { get { return this.myid; } }

        private object syncobj = new object();

        public delegate void ReceiveAssignIDHandler(int newID);
        public event ReceiveAssignIDHandler ReceiveAssignID;

        public delegate void ReceiveChatMessageHandler(string name, string msg, int senderID);
        public event ReceiveChatMessageHandler ReceiveChatMessage;

        public delegate void ReceiveDirectMessageHandler(string name, string msg, int senderID);
        public event ReceiveDirectMessageHandler ReceiveDirectMessage;

        public delegate void ReceiveIntroduceHandler(string name, int senderID);
        public event ReceiveIntroduceHandler ReceiveIntroduce;

        public delegate void ReceiveMemberListHandler();
        public event ReceiveMemberListHandler ReceiveMemberList;

        public delegate void ReceiveLeaveHandler(int leaveID);
        public event ReceiveLeaveHandler ReceiveLeave;

        public delegate void ConnectionCloseHandler();
        public event ConnectionCloseHandler ConnectionClose;

        public IEnumerable<People> GetPeopleList()
        {
            return members.Select(m=>new People(m.Name, m.ID));
        }
        private int GetNextID()
        {
            lock (syncobj)
            {
                Random r = new Random(Environment.TickCount);
                bool mod;
                int ret = r.Next(0, int.MaxValue);
            
                if (members.Count == 0)
                    return ret;
            
                for (;; ret = r.Next(0, int.MaxValue))
                {
                    mod = false;
                    foreach (var n in members)
                    {
                        if (n.ID == ret)
                            break;
                        else
                            mod = true;
                    }
                    if (mod)
                        return ret;
                }
            }
        }
        private Member GetMember(int id)
        {
            lock (syncobj)
            {
                return members.FirstOrDefault(c => c.ID == id);
            }
        }
        private Member GetMember(NetworkStream ns)
        {
            lock (syncobj)
            {
                return members.FirstOrDefault(c => c.Connection == ns);
            }
        }

        public void Start()
        {
            if (IsServer)
                Task.Factory.StartNew(() => {
                    bool listenDone = true;
                    var listener = new TcpListener(IPAddress.Any, port);
                    try
                    {
                        listener.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        listenDone = false;
                    }

                    if (listenDone)
                        for (;;)
                        {
                            var client = listener.AcceptTcpClient();
                            var cl = new Member("", GetNextID(), client.GetStream());
                            members.Add(cl);
                            SendAssignID(cl.ID, cl.Connection);
                            SendMemberList(cl.ID);
                            Task.Factory.StartNew(()=>ReceiveData(cl.Connection));
                        }
                });
            else            
                Task.Factory.StartNew(() => {
                    NetworkStream ns = null;
                    try
                    {
                        TcpClient cl = new TcpClient();
                        cl.Connect(host, port);
                        ns = cl.GetStream();
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }

                    if (ns != null)
                        ReceiveData(clientNs = ns);
                });

            if (IsServer)
                Task.Factory.StartNew(()=>{
                    for (;;)
                    {
                        Task.Delay(15000).RunSynchronously();
                        if (members.Count > 0)
                            foreach (var n in members)
                                SendPacket(Protocol.Test, new byte[]{}, MyID, n.Connection);
                    }
                });
        }

        private void SendPacket(Protocol prt, byte[] bytes, int senderId, NetworkStream ns)
        {
            byte[] sendbytes = new byte[bytes.Length + 9];
            Array.Copy(BitConverter.GetBytes(bytes.Length), 0, sendbytes, 0, 4);
            sendbytes [4] = (byte)prt;
            Array.Copy(BitConverter.GetBytes(senderId), 0, sendbytes, 5, 4);
            Array.Copy(bytes, 0, sendbytes, 9, bytes.Length);
            
            try
            {
                ns.Write(sendbytes, 0, sendbytes.Length);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Close(ns);
                try
                {
                    ConnectionClose();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2.Message);
                    Console.WriteLine(ex2.StackTrace);
                }
            }
        }

        private void ReceiveData(NetworkStream ns)
        {
            try
            {
                for (;;)
                {
                    byte[] buf = new byte[9];
                    ns.Read(buf, 0, buf.Length);
                    
                    int datasize = BitConverter.ToInt32(buf, 0);
                    Protocol prt = (Protocol)buf [4];
                    int recid = BitConverter.ToInt32(buf, 5);

                    if (datasize == 0)
                        ActionRecvData(prt, recid, new byte[]{});
                    else
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int allrecsize;
                            buf = new byte[1024];
                            
                            for (allrecsize = 0; allrecsize != datasize;)
                            {
                                int readsize = (readsize = datasize - allrecsize) < buf.Length ? readsize : buf.Length;
                                int recsize = ns.Read(buf, 0, readsize);
                                allrecsize += recsize;
                                ms.Write(buf, 0, recsize);
                            }
                            
                            ActionRecvData(prt, recid, ms.ToArray());
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Close(ns);
                try
                {
                    ConnectionClose();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2.Message);
                    Console.WriteLine(ex2.StackTrace);
                }
            }
        }
        
        private void ActionRecvData(Protocol proc, int senderID, byte[] bytes)
        {
            string str;
            string[] spl;
            int recid;
            switch (proc)
            {
                case Protocol.ChatMSG:
                    if (bytes != null && bytes.Length > 0)
                    {
                        str = enc.GetString(bytes);
                        if (isServer)
                            ForwardingMSG(str, senderID);
                        try
                        {
                            ReceiveChatMessage(GetMember(senderID).Name, str, senderID);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    break;
                case Protocol.AssignID:
                    if (bytes != null && bytes.Length > 0 && !isServer)
                    {
                        myid = recid = Convert.ToInt32(enc.GetString(bytes));
                        serverID = senderID;
                        SendIntroduce(MyName, MyID, clientNs);
                        try
                        {
                            ReceiveAssignID(MyID);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    break;
                case Protocol.DirectMSG:
                    if (bytes != null && bytes.Length > 0)
                    {
                        str = enc.GetString(bytes);
                        spl = str.Split(new char[]{'\n'}, 2);
                        recid = Convert.ToInt32(spl[0]);
                        if (recid == MyID)
                        {
                            try
                            {
                                ReceiveDirectMessage(GetMember(senderID).Name, spl[1], senderID);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine(ex.StackTrace);
                            }
                        }
                        else if (isServer && recid != MyID)
                            SendDirectMSG(spl[1], recid, senderID);
                    }
                    break;
                case Protocol.Introduce:
                    if (bytes != null && bytes.Length > 0)
                    {
                        str = enc.GetString(bytes);
                        if (isServer)
                        {
                            ForwardingIntroduce(str, senderID);
                            GetMember(senderID).Name = str;
                        }
                        else
                        {
                            members.Add(new Member(str, senderID, null));
                        }

                        try
                        {
                            ReceiveIntroduce(str, senderID);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    break;
                case Protocol.MemberList:
                    if (bytes != null && bytes.Length > 0 && !isServer)
                    {
                        str = enc.GetString(bytes);
                        spl = str.Split(new char[]{'\n'});

                        members.Clear();
                        int len = spl.Length / 2;

                        for (int i = 0; i < len; i++)
                            members.Add(new Member(spl[2*i], Convert.ToInt32(spl[2*i+1]), null));

                        try
                        {
                            ReceiveMemberList();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    break;
                case Protocol.Leave:
                    var item = GetMember(senderID);
                    if (item != null)
                    {
                        Close(item.Connection);
                        members.Remove(item);
                        try
                        {
                            ReceiveLeave(senderID);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    break;
                case Protocol.Test:
                    break;
            }
        }
        
        private void SendMemberList(int toID)
        {
            if (!isServer)
                throw new Exception(CantUseServerFunc);

            var sb = new StringBuilder();
            foreach (var n in members.Where(c=>c.ID != toID))
                sb.Append(n.Name + "\n" + n.ID + "\n");
            SendPacket(Protocol.MemberList, enc.GetBytes(sb.ToString()), MyID, GetMember(toID).Connection);
        }

        private void SendIntroduce(string name, int senderID, NetworkStream ns)
        {
            SendPacket(Protocol.Introduce, enc.GetBytes(name), senderID, ns);
        }
        private void ForwardingIntroduce(string name, int senderID)
        {
            if (!isServer)
                throw new Exception(CantUseServerFunc);

            foreach (var n in members.Where(c=>c.ID != senderID))
                SendIntroduce(name, senderID, n.Connection);
        }

        public void SendDirectMSG(string msg, int to)
        {
            SendDirectMSG(msg, to, MyID);
        }
        private void SendDirectMSG(string msg, int toID, int senderID)
        {
            NetworkStream ns = isServer ? GetMember(toID).Connection : clientNs;
            SendPacket(Protocol.DirectMSG, enc.GetBytes(toID + "\n" + msg), senderID, ns);
        }

        public void SendMSG(string msg)
        {
            if (isServer)
                foreach (var n in members)
                    SendMSG(msg, this.MyID, n.Connection);
            else
                SendMSG(msg, this.MyID, this.clientNs);
        }
        private void SendMSG(string msg, int senderID, NetworkStream ns)
        {
            SendPacket(Protocol.ChatMSG, enc.GetBytes(msg), senderID, ns);
        }
        private void ForwardingMSG(string msg, int senderID)
        {
            foreach (var n in members.Where(c=>senderID != c.ID))
                SendMSG(msg, senderID, n.Connection);
        }

        private void SendAssignID(int clientID, NetworkStream ns)
        {
            if (!isServer)
                throw new Exception(CantUseServerFunc);
            SendPacket(Protocol.AssignID, enc.GetBytes(clientID.ToString()), MyID, ns);
        }

        private void SendLeave(int closeID)
        {
            if (!IsServer)
                throw new Exception(CantUseServerFunc);
            foreach (var n in members.Where(c=>closeID != c.ID))
                SendPacket(Protocol.Leave, new byte[]{}, closeID, n.Connection);
        }

        private void Close(NetworkStream ns)
        {
            try
            {
                if (ns != null)
                {
                    ns.Close();
                    ns.Dispose();
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            if (IsServer)
            {
                for (int i = 0; i < members.Count;)
                {
                    if (members [i].Connection == ns || members [i].Connection == null || !members [i].Connection.CanRead || !members [i].Connection.CanWrite)
                    {
                        SendLeave(members [i].ID);
                        members.RemoveAt(i);
                    } else
                        i++;
                }
            }
        }

        /// <summary>
        /// Create an instance in client mode.
        /// </summary>
        /// <param name="port">Port</param>
        /// <param name="name">Name</param>
        public ChatController(int port, string name)
        {
            this.members = new List<Member>();

            this.isServer = true;
            this.host = "";
            this.port = port;
            this.myname = name;
            this.myid = GetNextID();
        }
        /// <summary>
        /// Create an instance in server mode.
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        /// <param name="name">Name</param>
        public ChatController(string host, int port, string name)
        {
            this.members = new List<Member>();

            this.isServer = false;
            this.host = host;
            this.port = port;
            this.myname = name;
            this.myid = -1;
        }
    }
}
