using System;
using System.Collections.Generic;
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
    class PredictionController
    {
        private static PredictionController instance;
        public static PredictionController Instance()
        {
            if (instance == null)
            {
                instance = new PredictionController();
            }
            return instance;
        }

        private PredictionController()
        {
            timer = 1.0f;
            lastAck = 1;
            messageMultiplier = 1;
        }

        private static int SEQUENCEID = 1;

        int lastAck;
        float timer;

        int messageMultiplier;

        public int GetSequenceID()
        {
            return SEQUENCEID++;
        }

        public int MessageMultiplier()
        {
            return messageMultiplier;
        }

        public void Update(float deltaTime)
        {
            if(timer <= 0)
            {
                timer = 1;

                LatencyCheckMessage msg = new LatencyCheckMessage();

                msg.sequenceNum = GetSequenceID();
                msg.ackNum = lastAck;


                OutputQueue.AddToQueue(msg);


            }
            timer -= deltaTime;
        }

        public void Process(LatencyCheckMessage msg)
        {
            int clientLastAck = msg.ackNum;

            int packetsMissed = clientLastAck - lastAck - 1;

            float percentageLost = ((float)packetsMissed / (float)clientLastAck);
            percentageLost *= 100;
            
            if(percentageLost > 10)
            {
                messageMultiplier += 2;
            }
            else
            {
               // messageMultiplier -= 1;
               // if(messageMultiplier < 1)
               // {
               //     messageMultiplier = 1;
               // }
            }
            

            //Debug.WriteLine("Last Ack " + clientLastAck);
            //Debug.WriteLine("PacketsMissed " + packetsMissed);
           // Debug.WriteLine("Percentage Lost " + percentageLost);
            Debug.WriteLine("Message Multiplier " + messageMultiplier);
            lastAck = clientLastAck;
        }

    }
}
