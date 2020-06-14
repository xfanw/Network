using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Box2DX.Dynamics;
using Box2DX.Collision;
using Box2DX.Common;

namespace OmegaRace
{
    public enum GAME_STATE
    {
        PLAY
    }

    public class GameManager 
    {
        private static GameManager instance = null;
        public static GameManager Instance()
        {
            if (instance == null)
            {
                instance = new GameManager();
            }
            return instance;
        }

        List<GameObject> destroyList;
        List<GameObject> gameObjList;

        List<Missile> missileList;

        public Ship player1;
        public Ship player2;

        public int p2Score;
        public int p1Score;

        GameManager_UI gamManUI;

        // Each networked gameobject is given their own ID Nubmer
        static int NETWORKIDNUM;
        public int lastRemovedMissile = -1;
        private GameManager()
        {
            destroyList = new List<GameObject>();
            gameObjList = new List<GameObject>();
            missileList = new List<Missile>();

            gamManUI = new GameManager_UI();
        }

        public static void Start()
        {
            LoadLevel_Helper.LoadLevel();
        }


        public static void Update(float gameTime)
        {
            GameManager inst = Instance();
            
            inst.pUpdate();
        }

        public static void Draw()
        {
            GameManager inst = Instance();
            
            inst.pDraw();
        }

        

        private void pUpdate()
        {
            int p1_H = InputManager.GetAxis(INPUTAXIS.HORIZONTAL_P1);
            int p1_V = InputManager.GetAxis(INPUTAXIS.VERTICAL_P1);
            

            PlayerInputMessage msg1 = new PlayerInputMessage(SEND_TYPE.LOCAL, PLAYER_ID.P1, p1_H, p1_V);
            OutputQueue.AddToQueue(msg1);
            

            //int p2_H = InputManager.GetAxis(INPUTAXIS.HORIZONTAL_P2);
            //int p2_V = InputManager.GetAxis(INPUTAXIS.VERTICAL_P2);

            

            if (InputManager.GetButtonDown(INPUTBUTTON.P1_FIRE))
            {
                // GameManager.FireMissile(player1);

                int networkID = NETWORKIDNUM++;

                CreateMissileMessage createMissileMsg = new
                    CreateMissileMessage(SEND_TYPE.LOCAL, PLAYER_ID.P1, networkID);
                OutputQueue.AddToQueue(createMissileMsg);
                CreateMissileMessage createMissileMsg1 = new
                    CreateMissileMessage(SEND_TYPE.NETWORKED, PLAYER_ID.P1, networkID);
                OutputQueue.AddToQueue(createMissileMsg1);
            }
            

            if (InputManager.GetButtonDown(INPUTBUTTON.P2_FIRE))
            {
               // GameManager.FireMissile(player2, PLAYER_ID.P2);
            }

            for (int i = gameObjList.Count - 1; i >= 0; i--)
            {
                gameObjList[i].Update();
            }

            float p1X = player1.GetWorldPosition().X;
            float p1Y = player1.GetWorldPosition().Y;

            PlayerUpdateMessage pUpdateMsg = new PlayerUpdateMessage(
                SEND_TYPE.NETWORKED, PLAYER_ID.P1,
                p1X, p1Y, player1.GetSpriteAngle());
            OutputQueue.AddToQueue(pUpdateMsg);

            float p2X = player2.GetWorldPosition().X;
            float p2Y = player2.GetWorldPosition().Y;

            PlayerUpdateMessage pUpdateMsg2 = new PlayerUpdateMessage(
                SEND_TYPE.NETWORKED, PLAYER_ID.P2,
                p2X, p2Y, player2.GetSpriteAngle());
            OutputQueue.AddToQueue(pUpdateMsg2);

            for (int i = 0; i < missileList.Count; i++)
            {
                Missile m = missileList[i];

                MissileUpdateMessage missileUpdateMsg = new MissileUpdateMessage(SEND_TYPE.NETWORKED,
                    m.GetOwner(), m.GetNetworkID(),
                    m.GetWorldPosition().X, m.GetWorldPosition().Y, lastRemovedMissile);

                OutputQueue.AddToQueue(missileUpdateMsg);

            }



            gamManUI.Update();
        }
        
        

        private void pDraw()
        {
            player1.Draw();
            player2.Draw();

            for (int i = 0; i < gameObjList.Count; i++)
            {
                gameObjList[i].Draw();
            }

            gamManUI.Draw();
        }
        

        public static void RecieveMessage(DataMessage msg)
        {
            switch(msg.dataType)
            {
                case DATAMESSAGE_TYPE.PLAYER_INPUT:
                    Instance().ProcessInputMessage(msg as PlayerInputMessage);
                    break;
                case DATAMESSAGE_TYPE.PLAYER_UPDATE:
                    // Process here
                    break;
                case DATAMESSAGE_TYPE.MISSILE_CREATE:
                    Instance().CreateMissileMessage(msg as CreateMissileMessage);
                    break;
                case DATAMESSAGE_TYPE.MISSILE_UPDATE:
                    //Instance().ProcessUpdateMissileMessage(msg as MissileUpdateMessage);
                    break;
                case DATAMESSAGE_TYPE.MISSILE_REMOVE:
                    //Instance().RemoveMissileMessage(msg as CreateMissileMessage);
                    break;
                default:
                    break;
            }
        }

        private void ProcessInputMessage(PlayerInputMessage msg)
        {
           if (msg.ID == PLAYER_ID.P1)
           {
               int p1_H = msg.horzInput;
               int p1_V = msg.vertInput;

               Instance().player1.Rotate(p1_H);
               Instance().player1.Move(p1_V);
           }
           else
           {
               int p2_H = msg.horzInput;
               int p2_V = msg.vertInput;

               Instance().player2.Rotate(p2_H);
               Instance().player2.Move(p2_V);
           }
        }

        void CreateMissileMessage(CreateMissileMessage msg)
        {
            if(msg.ID == PLAYER_ID.P1)
            {
                GameManager.FireMissile(player1, PLAYER_ID.P1, msg.missileNetworkID);
            }
            else if(msg.ID == PLAYER_ID.P2)
            {
                int id = NETWORKIDNUM++;
                CreateMissileMessage createMissileMsg = new CreateMissileMessage(SEND_TYPE.NETWORKED, PLAYER_ID.P2, id);
                OutputQueue.AddToQueue(createMissileMsg);
                GameManager.FireMissile(player2, PLAYER_ID.P2, id);
            }
        }


        public static void PlayerKilled(Ship s)
        {
            Instance().pPlayerKilled(s);
        }
        

        void pPlayerKilled(Ship shipKilled)
        {

            // Player 1 is Killed
            if(player1.getID() == shipKilled.getID())
            {
                p2Score++;

                player1.Respawn(new Vec2(400, 100));
                player2.Respawn(new Vec2(400, 400));
            }
            // Player 2 is Killed
            else if (player2.getID() == shipKilled.getID())
            {
                p1Score++;
                player1.Respawn(new Vec2(400, 100));
                player2.Respawn(new Vec2(400, 400));
                  
            }
        }

        public static void MissileDestroyed(Missile m)
        {
            GameManager inst = Instance();

            if (m.GetOwnerID() == inst.player1.getID())
            {
                inst.player1.GiveMissile();
            }
            else if (m.GetOwnerID() == inst.player2.getID())
            {
                inst.player2.GiveMissile();
            }
        }

        public static void FireMissile(Ship ship, PLAYER_ID playerID, int networkID)
        {
            if (ship.UseMissile())
            {
                ship.Update();
                Vec2 pos = ship.GetWorldPosition();
                Vec2 direction = ship.GetHeading();
                Missile m = new Missile(new Azul.Rect(pos.X, pos.Y, 20, 5), ship.getID(), playerID, direction, ship.getColor());
                m.SetNetworkID(networkID);
                Instance().gameObjList.Add(m);
                AudioManager.PlaySoundEvent(AUDIO_EVENT.MISSILE_FIRE);

                // For Networking
                Instance().missileList.Add(m);
            }
        }

        

        public static void AddGameObject(GameObject obj)
        {
            Instance().gameObjList.Add(obj);
        }

        public static void CleanUp()
        {
            foreach (GameObject obj in Instance().destroyList)
            {
                obj.Destroy();
            }

            Instance().destroyList.Clear();
        }
        
        public void DestroyAll()
        {
            foreach(GameObject obj in gameObjList)
            {
                destroyList.Add(obj);
            }
            gameObjList.Clear();
        }
            
        public static void DestroyObject(GameObject obj)
        {
            obj.setAlive(false);
            Instance().gameObjList.Remove(obj);
            Instance().destroyList.Add(obj);

            if(obj.type == GAMEOBJECT_TYPE.MISSILE)
            {
                Missile missile = obj as Missile;
                Instance().missileList.Remove(missile);
            }
        }
        
        
    }
}
