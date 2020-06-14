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
    public class Missile : GameObject
    {
        int ownerID;
        float MaxForce;

        PLAYER_ID pOwner;

        public Missile(Azul.Rect destRect, int ownerIDNum, PLAYER_ID owner, Vec2 direction, Azul.Color color)
            : base(GAMEOBJECT_TYPE.MISSILE,new Azul.Rect(0, 0, 24, 6), destRect, GameObject.missileTexture, color)
        {
            PhysicBody_Data data = new PhysicBody_Data();

            data.position = new Vec2(destRect.x, destRect.y);
            data.size = new Vec2(destRect.width, destRect.height);
            //data.radius = 25f;
            data.isSensor = true;
            data.angle = 0;
            data.shape_type = PHYSICBODY_SHAPE_TYPE.DYNAMIC_BOX;

            ownerID = ownerIDNum;
            pOwner = owner;

            MaxForce = 1000f;

            CreatePhysicBody(data);

            LaunchAt(direction);
        }


        public int GetOwnerID()
        {
            return ownerID;
        }

        public PLAYER_ID GetOwner()
        {
            return pOwner;
        }

        void LaunchAt(Vec2 direction)
        {
            direction.Normalize();
            Vec2 right = new Vec2(1, 0);

            float angle = Vec2.Dot(direction, right);
            angle = (float)System.Math.Acos(Vec2.Dot(direction, right));
            angle *= PhysicWorld.MATH_180_PI;

            Vec2 sub = direction - right;
            if (sub.Y < 0)
            {
                angle = 360 - angle;
            }
            pBody.SetAngle(angle);

            pBody.ApplyForce(direction * MaxForce);
        }

        public override void Accept(GameObject obj)
        {
            obj.VisitMissile(this);
        }

        public void OnHit()
        {
            RemoveMissileMessage msg = new RemoveMissileMessage(SEND_TYPE.NETWORKED, this.GetOwner(), this.GetNetworkID());
            OutputQueue.AddToQueue(msg);
            GameManager.Instance().lastRemovedMissile = this.GetNetworkID();

            AudioManager.PlaySoundEvent(AUDIO_EVENT.MISSILE_HIT);
            ParticleSpawner.SpawnParticleEvent(PARTICLE_EVENT.EXPLOSION, this);
            GameManager.MissileDestroyed(this);
            GameManager.DestroyObject(this);


        }

        public override void VisitFence(Fence f)
        {
            OnHit();
            f.OnHit();
        }

        public override void VisitFencePost(FencePost fp)
        {
            OnHit();
        }

        public override void VisitMissile(Missile m)
        {
        }
        public override void VisitShip(Ship s)
        {
            if (s.getID() != ownerID)
            {
                OnHit();
                s.OnHit();
            }
        }
    }
}
