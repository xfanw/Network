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
    public class Ship : GameObject
    {
        float maxSpeed;
        float maxForce;
        float rotateSpeed;
        Vec2 heading;

        Azul.Color shipColor;

        int missileCount;

        Vec2 respawnPos;
        bool respawning;

        Vec2 prevPos;
        Vec2 curPos;

        //int prevTime;
        //int curTime;

        bool newDataReceived = false;
        Vec2 curVelocity;

        public Ship(Azul.Rect screenRect, Azul.Color color)
            : base(GAMEOBJECT_TYPE.SHIP, new Azul.Rect(0, 0, 32, 32), screenRect, GameObject.shipTexture, color)
        {
            //PhysicBody_Data data = new PhysicBody_Data();

            //data.position = new Vec2(screenRect.x, screenRect.y);
            //data.size = new Vec2(screenRect.width, screenRect.height);

            //data.angle = 0;
            //data.shape_type = PHYSICBODY_SHAPE_TYPE.SHIP_MANIFOLD;
            //CreatePhysicBody(data);

            //maxSpeed is pixels / sec
            maxSpeed = 150.0f;

            maxForce = 20f;
            rotateSpeed = 5.0f;
            // heading = new Vec2((float)System.Math.Cos(pBody.GetAngleDegs()), (float)System.Math.Sin(pBody.GetAngleDegs()));
            prevPos = new Vec2(screenRect.x, screenRect.y);
            curPos = prevPos;
 

            //prevTime = GetTime();
            //curTime = prevTime;

            curVelocity = new Vec2(0.0f, 0.0f);


            missileCount = 3;
            shipColor = color;

            respawnPos = new Vec2(screenRect.x, screenRect.y);
        }

        // This should use Azul Time library
        public int GetTime()
        {
            int min = DateTime.Now.Minute;
            int second = DateTime.Now.Second;
            int millisecond = DateTime.Now.Millisecond;
            return (min* 60 + second) * 1000 + millisecond;

        }
        public override void Update()
        {
            if (!newDataReceived) { 
                // Before the update
                prevPos = new Vec2(pWorldRect.x, pWorldRect.y);


                //Debug.WriteLine("In Update");
                //Debug.WriteLine("prevWorld: {0}, {1}", pWorldRect.x, pWorldRect.y);
                pWorldRect.x += curVelocity.X ;
                pWorldRect.y += curVelocity.Y ;

                // After the update

                curPos = new Vec2(pWorldRect.x, pWorldRect.y);

                curVelocity = curPos - prevPos;

                // prevPos = new Vec2(pWorldRect.x, pWorldRect.y);
                //prevTime = curTime;

                //Debug.WriteLine("In Update");
                //Debug.WriteLine("prevWorld: {0}, {1}", prevPos.X, prevPos.Y);
                // Debug.WriteLine("CurrWorld: {0}, {1}", pWorldRect.x, pWorldRect.y);
                ////Debug.WriteLine("Time Passed: {0}, Delta: {1}, {2}", timepassed, curVelocity.X * timepassed, curVelocity.Y * timepassed);
                //Debug.WriteLine("Calced World: {0}, {1}", pWorldRect.x, pWorldRect.y);
                //heading = pSprite.angle;
                
            }
            base.Update();
            //LimitSpeed();
            UpdateDirection();

            HandleRespawn();
            newDataReceived = false;

        }
        public override void UpdateSprite(float x, float y, float angle)
        {
            base.UpdateSprite( x,  y,  angle);
            newDataReceived = true;
        }
        //public override void UpdateSprite(float x, float y, float angle)
        //{
        //    //curTime = GetTime();
        //    //int timeInterval = curTime - prevTime;


        //    // Before the update
        //    // prevPos = new Vec2(pWorldRect.x, pWorldRect.y);    

        //    //currMsgTime = Azul.Game.GetTime();
        //    pWorldRect.x = x;
        //    pWorldRect.y = y;


        //    //curVelocity.X /= timeInterval;
        //    //curVelocity.Y /= timeInterval;
        //    //Debug.WriteLine("\nIn Spite Update"); 
        //    //Debug.WriteLine("prevWorld: {0}, {1}", prevPos.X, prevPos.Y);
        //    //Debug.WriteLine("CurrWorld: {0}, {1}", curPos.X, curPos.Y);
        //    //Debug.WriteLine("Velocity: {0}, {1}", curVelocity.X, curVelocity.Y);
        //    ////Debug.WriteLine("Time Passed: {0}, vel: {1}, {2}", timeInterval, curVelocity.X, curVelocity.Y);

        //    //prevTime = curTime;
        //    pSprite.angle = angle;
        //}


        public override void Draw()
        {
            base.Draw();
        }

        public Azul.Color getColor()
        {
            return shipColor;
        }

        public void Move(int vertInput)
        {
            if (vertInput < 0)
            {
                vertInput = 0;
            }
            pBody.ApplyForce(heading * vertInput * maxForce);
        }

        public void Rotate(int horzInput)
        {
            pBody.SetAngularVelocity(0);
            pBody.SetAngle(pBody.GetAngleDegs() + (horzInput * -rotateSpeed));
        }

        public void LimitSpeed()
        {
            Vec2 shipVel = pBody.GetBox2DVelocity();
            float magnitude = shipVel.Length();

            if (magnitude > maxSpeed)
            {
                shipVel.Normalize();
                shipVel *= maxSpeed;
                pBody.SetBox2DVelocity(shipVel);
            }
        }

        public bool UseMissile()
        {
            bool output = false;

            if (missileCount > 0)
            {
                missileCount--;
                output = true;
            }
            return output;
        }

        public int MissileCount()
        {
            return missileCount;
        }

        public void GiveMissile()
        {
            if (missileCount < 3)
            {
                missileCount++;
            }
        }

        public void Respawn(Vec2 v)
        {
            respawning = true;
            respawnPos = v;
        }

        private void HandleRespawn()
        {
            if (respawning == true)
            {
                pBody.SetBox2DPosition(respawnPos);
                respawning = false;
            }
        }

        void UpdateDirection()
        {
            // heading = new Vec2((float)System.Math.Cos(pBody.GetAngleRads()), (float)System.Math.Sin(pBody.GetAngleRads()));
            heading = new Vec2((float)System.Math.Cos(pSprite.angle), (float)System.Math.Sin(pSprite.angle));
        }

        public Vec2 GetHeading()
        {
            return heading;
        }

        public void OnHit()
        {
            GameManager.PlayerKilled(this);
        }

        public override void Accept(GameObject obj)
        {
            obj.VisitShip(this);
        }

        public override void VisitMissile(Missile m)
        {
            if (m.GetOwnerID() != getID())
            {
                m.OnHit();
                OnHit();
            }
        }

        public override void VisitFence(Fence f)
        {
            f.OnHit();
        }
    }
}
