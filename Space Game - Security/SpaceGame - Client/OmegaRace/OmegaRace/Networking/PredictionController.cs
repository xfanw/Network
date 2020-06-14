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

        private static int SEQUENCEID = 1;
        int lastAck;

        private PredictionController()
        {
            lastAck = 1;
        }

        public int GetSequenceID()
        {
            return SEQUENCEID++;
        }

        public void Update(float deltaTime)
        {
            
            
        }

        public void Process(LatencyCheckMessage msg)
        {
            lastAck = msg.sequenceNum;

            LatencyCheckMessage outMsg = new LatencyCheckMessage();
            outMsg.ackNum = lastAck;
            outMsg.sequenceNum = GetSequenceID();

            OutputQueue.AddToQueue(outMsg);

            Debug.WriteLine("Last Ack " + lastAck);
        }

    }
}