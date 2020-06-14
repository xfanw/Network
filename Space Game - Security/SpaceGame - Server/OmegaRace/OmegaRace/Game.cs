using System;
using System.Diagnostics;
using Lidgren.Network;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;


namespace OmegaRace
{
    class NetworkGame : Azul.Game
    {
        float prevTime;

        //-----------------------------------------------------------------------------
        // Game::Initialize()
        //		Allows the engine to perform any initialization it needs to before 
        //      starting to run.  This is where it can query for any required services 
        //      and load any non-graphic related content. 
        //-----------------------------------------------------------------------------
        public override void Initialize()
        {
            // Game Window Device setup
            this.SetWindowName("Omega Race");
            this.SetWidthHeight(800, 500);
            this.SetClearColor(0.2f, 0.2f, 0.2f, 1.0f);


        }

        //-----------------------------------------------------------------------------
        // Game::LoadContent()
        //		Allows you to load all content needed for your engine,
        //	    such as objects, graphics, etc.
        //-----------------------------------------------------------------------------
        public override void LoadContent()
        {
            PhysicWorld.Instance();
            GameManager.Instance();
            ParticleSpawner.Instance();
            AudioManager.Instance();
            InputQueue.Instance();
            OutputQueue.Instance();
            MyServer.Instance();
            PredictionController.Instance();

            prevTime = GetTime();

            GameManager.Start();
        }

        //-----------------------------------------------------------------------------
        // Game::Update()
        //      Called once per frame, update data, tranformations, etc
        //      Use this function to control process order
        //      Input, AI, Physics, Animation, and Graphics
        //-----------------------------------------------------------------------------

       // static int number = 0;
        public override void Update()
        {
            float curTime = GetTime();
            float gameElapsedTime = curTime - prevTime;

            PhysicWorld.Update(gameElapsedTime);
            GameManager.Update(gameElapsedTime);
            
            InputManager.Update();
            InputTest.KeyboardTest();

            MyServer.Instance().Update();
            PredictionController.Instance().Update(gameElapsedTime);

            OutputQueue.Process();
            InputQueue.Process();



            GameManager.CleanUp();

            prevTime = curTime;

             
        }

        //-----------------------------------------------------------------------------
        // Game::Draw()
        //		This function is called once per frame
        //	    Use this for draw graphics to the screen.
        //      Only do rendering here
        //-----------------------------------------------------------------------------
        public override void Draw()
        {
            GameManager.Draw();
            
        }

        //-----------------------------------------------------------------------------
        // Game::UnLoadContent()
        //       unload content (resources loaded above)
        //       unload all content that was loaded before the Engine Loop started
        //-----------------------------------------------------------------------------
        public override void UnLoadContent()
        {
        }

    }
}

