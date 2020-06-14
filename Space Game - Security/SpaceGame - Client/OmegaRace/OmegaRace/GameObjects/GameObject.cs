﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Dynamics;
using Box2DX.Collision;
using Box2DX.Common;

namespace OmegaRace
{
    public enum GAMEOBJECT_TYPE
    {
        NULL,
        SHIP,
        MISSILE,
        FENCEPOST,
        FENCE,
        MINE,
        PARTICLE
    }

    public class GameObject : Visitor
    {
        // I'm lazy so I predefine all my textures into static variables
        public static Azul.Texture shipTexture = new Azul.Texture("PlayerShip.tga");
        public static Azul.Texture missileTexture = new Azul.Texture("Missile.tga");
        public static Azul.Texture mineTexture = new Azul.Texture("Mine.tga");
        public static Azul.Texture mineTexture2 = new Azul.Texture("Mine2.tga");
        public static Azul.Texture fenceTexture = new Azul.Texture("FenceTall1.tga");
        public static Azul.Texture fencePostTexture = new Azul.Texture("FencePost.tga");
        
        // Each object has a type
        public GAMEOBJECT_TYPE type;

        // Reference for the Sprite
        protected Azul.Sprite pSprite;

        // Reference for the WorldRect (i.e. Position and Size)
        protected Azul.Rect pWorldRect;

        // Refernce for the Color (tints the sprite)
        protected Azul.Color color;

        // World angle of the object (Degrees)
        protected float angle;

        // Reference to Physics Body
        protected PhysicBody pBody;
        
        // Every GameObject has a unique ID number.
        static int IDNUM;
        int id;

        int networkID;

        // 
        bool alive;
        

        public GameObject(GAMEOBJECT_TYPE _type, Azul.Rect textureRect, Azul.Rect worldRect, Azul.Texture text, Azul.Color c)
        {
            type = _type;
            color = c;
            pSprite = new Azul.Sprite(text, textureRect, worldRect, color);
            pWorldRect = worldRect;
            id = IDNUM++;
            alive = true;
        }

        public int getID()
        {
            return id;
        }

        public void SetNetworkID(int id)
        {
            networkID = id;
        }
        public int GetNetworkID()
        {
            return networkID;
        }


        public Azul.Rect getWorldRect()
        {
            return pWorldRect;
        }

        public virtual void Update()
        {
            //if (pBody != null)
           // {
            //    pushPhysics();
           // }

            pSprite.x = pWorldRect.x;
            pSprite.y = pWorldRect.y;
            pSprite.Update();
        }

        public virtual void UpdateSprite(float x, float y, float angle)
        {
            pWorldRect.x = x;
            pWorldRect.y = y;

            pSprite.angle = angle;
        }
        
        public bool isAlive()
        {
            return alive;
        }
        public void setAlive(bool b)
        {
            alive = b;
        }

        public void CreatePhysicBody(PhysicBody_Data _data)
        {
            pBody = new PhysicBody(_data, this);
        }

        public virtual void Draw()
        {
            pSprite.Render();
        }

        public Vec2 GetWorldPosition()
        {
            return new Vec2(pSprite.x, pSprite.y);
        }

        public void SetWorldVelocity(Vec2 worldVelocity)
        {
            pBody.SetWorldVelocity(worldVelocity);
        }

        public Vec2 GetWorldVelocity()
        {
            return pBody.GetWorldVelocity();
        }

        public Vec2 GetBox2DVelocity()
        {
            return pBody.GetBox2DVelocity();
        }

        public float GetAngle_Deg()
        {
            return pBody.GetAngleDegs();
        }
        public float GetAngle_Rad()
        {
            return pBody.GetAngleRads();
        }


        void pushPhysics()
        {
            Vec2 bodyPos = pBody.GetWorldPosition();
            pSprite.angle = pBody.GetAngleRads();

            pWorldRect.x = bodyPos.X;
            pWorldRect.y = bodyPos.Y;
        }

        public virtual void Destroy()
        {
            pSprite = null;

            if (pBody != null)
            {
                Body b = pBody.GetBody();
                if (b != null)
                {
                    PhysicWorld.GetWorld().DestroyBody(pBody.GetBody());
                }
            }
            pBody = null;

        }

    }
}
