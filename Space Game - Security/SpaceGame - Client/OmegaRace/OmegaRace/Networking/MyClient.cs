using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace OmegaRace
{
    struct NetworkInfo
    {
        public string IPADDRESS;
        public int port;
    }

    class MyClient
    {
        private static MyClient instance;
        public static MyClient Instance()
        {
            if (instance == null)
            {
                instance = new MyClient();
            }
            return instance;
        }

        NetClient client;
        NetworkInfo myClientInfo;

        NetworkInfo connectedServerInfo;

        bool isConnected;

        private MyClient()
        {
            Setup();
        }

        public void Setup()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("Connected Test");
            config.AcceptIncomingConnections = true;
            config.MaximumConnections = 100;
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.Port = 14250;
            

            client = new NetClient(config);
            client.Start();

            isConnected = false;
        }

        public void Update()
        {
            //AttemptConnection();
            if(client.ConnectionStatus != NetConnectionStatus.Connected)
            {
                AttemptConnection();
            }

            ReadData();


           // Debug.WriteLine(client.ConnectionStatus.ToString());

        }


        public bool AttemptConnection()
        {
            IPEndPoint ep = NetUtility.Resolve("127.0.0.1", 14240);
            bool output = false;


            if(client.GetConnection(ep) == null)
            {
                client.DiscoverKnownPeer(ep);

                connectedServerInfo.IPADDRESS = ep.ToString();
                connectedServerInfo.port = ep.Port;

                myClientInfo.IPADDRESS = client.Configuration.BroadcastAddress.ToString();
                myClientInfo.port = client.Port;

                output = true;
            }
            

            return output;
        }

        public void SendData(DataMessage msg)
        {

            NetOutgoingMessage om = client.CreateMessage();

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            om.Write((int)msg.dataType);

            msg.Serialize(ref writer);

            om.Write(stream.ToArray());

            // Add security layer
            NetEncryption algo = new NetXtea(client, "MytestKey");
            om.Encrypt(algo);

            client.SendMessage(om, NetDeliveryMethod.Unreliable);

            client.FlushSendQueue();
        }


        void ReadData()
        {
            

            NetIncomingMessage im;

            
            while ((im = client.ReadMessage()) != null)
            {

                // Add security layer
                if (im.MessageType == NetIncomingMessageType.DebugMessage || 
                    im.MessageType == NetIncomingMessageType.WarningMessage || 
                    im.MessageType == NetIncomingMessageType.DiscoveryResponse ||
                    im.MessageType == NetIncomingMessageType.VerboseDebugMessage ||
                    im.MessageType == NetIncomingMessageType.ErrorMessage ||
                    im.MessageType == NetIncomingMessageType.StatusChanged
                    )
                {
                    // Debug or worning message has no encryption
                }
                else
                {

                    NetEncryption algo = new NetXtea(client, "MytestKey");
                    im.Decrypt(algo);
                }
               // Debug.WriteLine("{0}", im.MessageType);
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                        string debug1 = im.ReadString();
                        Debug.WriteLine(debug1);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                        string debug2 = im.ReadString();
                        Debug.WriteLine(debug2);
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        string warning = im.ReadString();
                        Debug.WriteLine(warning);
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        Debug.WriteLine(im.ReadString() + Environment.NewLine);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                        string reason = im.ReadString();
                        Debug.WriteLine(status.ToString() + ": " + reason);

                        break;
                    case NetIncomingMessageType.Data:
                        byte[] msg = im.ReadBytes(im.LengthBytes);

                        BinaryReader reader = new BinaryReader(new MemoryStream(msg));

                        DATAMESSAGE_TYPE type = (DATAMESSAGE_TYPE)reader.ReadInt32();

                        DataMessage dataMsg;

                        switch (type)
                        {
                            case DATAMESSAGE_TYPE.PLAYER_INPUT:
                                dataMsg = PlayerInputMessage.Deserialize(ref reader);
                                InputQueue.AddToQueue(dataMsg);
                                break;
                            case DATAMESSAGE_TYPE.PLAYER_UPDATE:
                                dataMsg = PlayerUpdateMessage.Deserialize(ref reader);
                                InputQueue.AddToQueue(dataMsg);
                                break;
                            case DATAMESSAGE_TYPE.MISSILE_CREATE:
                                dataMsg = CreateMissileMessage.Deserialize(ref reader);
                                InputQueue.AddToQueue(dataMsg);
                                break;
                            case DATAMESSAGE_TYPE.MISSILE_UPDATE:
                                dataMsg = MissileUpdateMessage.Deserialize(ref reader);
                                InputQueue.AddToQueue(dataMsg);
                                break;
                            case DATAMESSAGE_TYPE.MISSILE_REMOVE:
                                dataMsg = RemoveMissileMessage.Deserialize(ref reader);
                                InputQueue.AddToQueue(dataMsg);
                                break;
                            case DATAMESSAGE_TYPE.LATENCY_CHECK:
                                dataMsg = LatencyCheckMessage.Deserialize(ref reader);
                                InputQueue.AddToQueue(dataMsg);
                                break;
                        }



                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        Debug.WriteLine("Found server at " + im.SenderEndPoint + " name: " + im.ReadString());
                        client.Connect(im.SenderEndPoint);
                        break;

                    case NetIncomingMessageType.UnconnectedData:
                        Debug.WriteLine("Received from " + im.SenderEndPoint + ": " + im.ReadString() + Environment.NewLine);
                        break;
                }
                client.Recycle(im);
            }

        }

    }
}