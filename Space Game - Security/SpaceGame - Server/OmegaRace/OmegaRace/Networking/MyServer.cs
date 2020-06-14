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

    class MyServer
    {
        private static MyServer instance;
        public static MyServer Instance()
        {
            if(instance == null)
            {
                instance = new MyServer();
            }
            return instance;
        }

        NetServer server;
        NetworkInfo networkInfo;

        private MyServer()
        {
            Setup();
        }

        public void Setup()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("Connected Test");
            config.AcceptIncomingConnections = true;
            config.MaximumConnections = 100;
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.Port = 14240;

            config.SimulatedLoss = 0.05f;
             config.PingInterval = 0.05f;
             config.ConnectionTimeout = 5.0f;

            server = new NetServer(config);
            server.Start();

            networkInfo = new NetworkInfo();
            networkInfo.IPADDRESS = server.Configuration.BroadcastAddress.ToString();
            networkInfo.port = 14240;
        }

        public void Update()
        {

            ReadInData();
        }


        public void SendData(DataMessage msg)
        {
            if (server.ConnectionsCount > 0)
            {
                foreach (NetConnection con in server.Connections)
                {
                    int messageMultiplier = PredictionController.Instance().MessageMultiplier();

                    for (int i = 0; i < messageMultiplier; i++)
                    {
                        NetOutgoingMessage om = server.CreateMessage();

                        MemoryStream stream = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(stream);

                        om.Write((int)msg.dataType);
                        
                        msg.Serialize(ref writer);

                        om.Write(stream.ToArray());

                        // Add security layer
                        NetEncryption algo = new NetXtea(server, "MytestKey");
                        om.Encrypt(algo);

                        server.SendMessage(om, server.Connections, NetDeliveryMethod.Unreliable, 0);
                    }
                    server.FlushSendQueue();

                    
                }
            }
            else
            {
              //  Debug.WriteLine("No Connections to server");
            }

        }

        void ReadInData()
        {
            

            NetIncomingMessage im;
            while ((im = server.ReadMessage()) != null)
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

                    NetEncryption algo = new NetXtea(server, "MytestKey");
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
                            case DATAMESSAGE_TYPE.LATENCY_CHECK:
                                dataMsg = LatencyCheckMessage.Deserialize(ref reader);
                                InputQueue.AddToQueue(dataMsg);
                                break;
                        }

                        break;
                    case NetIncomingMessageType.DiscoveryRequest:
                        // Create a response
                        NetOutgoingMessage om = server.CreateMessage();
                        om.Write("Connecting to DOG server");
                        server.SendDiscoveryResponse(om, im.SenderEndPoint);
                        break;

                    case NetIncomingMessageType.UnconnectedData:
                        Debug.WriteLine("Received from " + im.SenderEndPoint + ": " + im.ReadString() + Environment.NewLine);
                        break;
                }
                server.Recycle(im);
            }

        }


    }
}
