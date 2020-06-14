using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OmegaRace
{
    // Lets where to send this
    public enum SEND_TYPE
    {
        LOCAL = 0,
        NETWORKED
    }

    public enum DATAMESSAGE_TYPE
    {
        PLAYER_INPUT = 0,
        PLAYER_UPDATE,
        MISSILE_CREATE,
        MISSILE_UPDATE,
        MISSILE_REMOVE,
        LATENCY_CHECK
    }

    public enum PLAYER_ID
    {
        P1,
        P2
    }


    [Serializable]
    public abstract class DataMessage
    {
        public SEND_TYPE sendType;
        public DATAMESSAGE_TYPE dataType;

        public DataMessage()
        {
            dataType = DATAMESSAGE_TYPE.PLAYER_INPUT;
        }

        public DataMessage(DATAMESSAGE_TYPE dt, SEND_TYPE t)
        {
            dataType = dt;
            sendType = t;
        }

        public virtual void Serialize(ref BinaryWriter writer)
        {
            writer.Write((int)dataType);
        }
    }

    [Serializable]
    public class LatencyCheckMessage : DataMessage
    {
        public int sequenceNum;
        public int ackNum;

        public LatencyCheckMessage()
        {
            dataType = DATAMESSAGE_TYPE.LATENCY_CHECK;
            sendType = SEND_TYPE.NETWORKED;
        }

        public override void Serialize(ref BinaryWriter writer)
        {
            writer.Write(this.sequenceNum);
            writer.Write(this.ackNum);
        }

        public static LatencyCheckMessage Deserialize(ref BinaryReader reader)
        {
            LatencyCheckMessage output = new LatencyCheckMessage();
            output.sequenceNum = reader.ReadInt32();
            output.ackNum = reader.ReadInt32();

            return output;
        }
    }


    public class PlayerInputMessage : DataMessage
    {
        public PLAYER_ID ID;
        public int horzInput;
        public int vertInput;

        public PlayerInputMessage()
        {
            dataType = DATAMESSAGE_TYPE.PLAYER_INPUT;
        }

        public PlayerInputMessage(SEND_TYPE t, PLAYER_ID id, int hInput, int vInput)
            : base(DATAMESSAGE_TYPE.PLAYER_INPUT, t)
        {
            horzInput = hInput;
            vertInput = vInput;
            ID = id;
        }

        public override void Serialize(ref BinaryWriter writer)
        {
            writer.Write((int)this.ID);
            writer.Write(horzInput);
            writer.Write(vertInput);
        }

        public static PlayerInputMessage Deserialize(ref BinaryReader reader)
        {
            PlayerInputMessage output = new PlayerInputMessage();
            output.ID = (PLAYER_ID)reader.ReadInt32();
            output.horzInput = reader.ReadInt32();
            output.vertInput = reader.ReadInt32();

            return output;
        }
    }

    public class PlayerUpdateMessage : DataMessage
    {
        public PLAYER_ID ID;
        public float x;
        public float y;
        public float pAngle;

        public PlayerUpdateMessage()
        {
            dataType = DATAMESSAGE_TYPE.PLAYER_UPDATE;
        }

        public PlayerUpdateMessage(SEND_TYPE t, PLAYER_ID id, float xPos, float yPos, float angle)
            : base(DATAMESSAGE_TYPE.PLAYER_UPDATE, t)
        {
            x = xPos;
            y = yPos;
            pAngle = angle;
            ID = id;
        }

        public override void Serialize(ref BinaryWriter writer)
        {
            writer.Write((int)this.ID);
            writer.Write(x);
            writer.Write(y);
            writer.Write(pAngle);
        }

        public static PlayerUpdateMessage Deserialize(ref BinaryReader reader)
        {
            PlayerUpdateMessage output = new PlayerUpdateMessage();
            output.ID = (PLAYER_ID)reader.ReadInt32();
            output.x = reader.ReadSingle();
            output.y = reader.ReadSingle();
            output.pAngle = reader.ReadSingle();
            return output;
        }
    }

    public class CreateMissileMessage : DataMessage
    {
        public PLAYER_ID ID;
        public int missileNetworkID;

        public CreateMissileMessage()
        {
            dataType = DATAMESSAGE_TYPE.MISSILE_CREATE;
        }

        public CreateMissileMessage(SEND_TYPE t, PLAYER_ID id, int uniqueMissileID)
            : base(DATAMESSAGE_TYPE.MISSILE_CREATE, t)
        {
            ID = id;
            missileNetworkID = uniqueMissileID;
        }

        public override void Serialize(ref BinaryWriter writer)
        {
            writer.Write((int)this.ID);
            writer.Write(missileNetworkID);
        }

        public static CreateMissileMessage Deserialize(ref BinaryReader reader)
        {
            CreateMissileMessage output = new CreateMissileMessage();
            output.ID = (PLAYER_ID)reader.ReadInt32();
            output.missileNetworkID = reader.ReadInt32();
            return output;
        }
    }
    public class MissileUpdateMessage : DataMessage
    {
        public PLAYER_ID ID;
        public int missileNetworkID;
        public float x;
        public float y;
        public int lastID;

        public MissileUpdateMessage()
        {
            dataType = DATAMESSAGE_TYPE.MISSILE_UPDATE;
        }

        public MissileUpdateMessage(SEND_TYPE t, PLAYER_ID id, int networkID, float xPos, float yPos, int last)
            : base(DATAMESSAGE_TYPE.MISSILE_UPDATE, t)
        {
            ID = id;
            missileNetworkID = networkID;
            x = xPos;
            y = yPos;
            lastID = last;
        }

        public override void Serialize(ref BinaryWriter writer)
        {
            writer.Write((int)this.ID);
            writer.Write(missileNetworkID);
            writer.Write(x);
            writer.Write(y);
            writer.Write(lastID);
        }

        public static MissileUpdateMessage Deserialize(ref BinaryReader reader)
        {
            MissileUpdateMessage output = new MissileUpdateMessage();
            output.ID = (PLAYER_ID)reader.ReadInt32();
            output.missileNetworkID = reader.ReadInt32();
            output.x = reader.ReadSingle();
            output.y = reader.ReadSingle();
            output.lastID = reader.ReadInt32();
            return output;
        }
    }

public class RemoveMissileMessage : DataMessage
    {
        public PLAYER_ID ID;
        public int missileNetworkID;

        public RemoveMissileMessage()
        {
            dataType = DATAMESSAGE_TYPE.MISSILE_REMOVE;
        }

        public RemoveMissileMessage(SEND_TYPE t, PLAYER_ID id, int uniqueMissileID)
            : base(DATAMESSAGE_TYPE.MISSILE_REMOVE, t)
        {
            ID = id;
            missileNetworkID = uniqueMissileID;
        }

        public override void Serialize(ref BinaryWriter writer)
        {
            writer.Write((int)this.ID);
            writer.Write(missileNetworkID);
        }

        public static RemoveMissileMessage Deserialize(ref BinaryReader reader)
        {
            RemoveMissileMessage output = new RemoveMissileMessage();
            output.ID = (PLAYER_ID)reader.ReadInt32();
            output.missileNetworkID = reader.ReadInt32();
            return output;
        }
    }
}
