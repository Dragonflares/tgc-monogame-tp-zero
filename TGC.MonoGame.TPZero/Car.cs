using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP
{
    class Car
    {
        public Vector3 Position { get; set; }
        public float speed { get; set; }
        private float maxspeed { get; set; }
        private float maxacceleration { get; set; }
        public Model modelo { get; set; }
        public Vector3 orientacion { get; set; }
        public Vector3 orientacionSobreOla { get; set; }
        public float anguloDeGiro { get; set; }
        public float giroBase { get; set; }

        private Boolean pressedAccelerator { get; set; }
        private Boolean HandBrake { get; set; }
        private Boolean pressedReverse { get; set; }

        private TGCGame _game;
        public string ModelName;
        private Matrix[] boneTransforms;
        public Car(Vector3 initialPosition, Vector3 currentOrientation, float MaxSpeed, TGCGame game)
        {
            speed = 0;
            Position = initialPosition;
            orientacion = currentOrientation;
            maxspeed = MaxSpeed;
            maxacceleration = 0.005f;
            anguloDeGiro = 0f;
            giroBase = 0.003f;
            pressedAccelerator = false;
            HandBrake = false;
            pressedReverse = false;
            _game = game;
        }
        public void LoadContent()
        {
            modelo = _game.Content.Load<Model>(TGCGame.ContentFolder3D + ModelName);

            var basicShader = _game.Content.Load<Effect>(TGCGame.ContentFolderEffects + "BasicShader");
            var effect = modelo.Meshes.FirstOrDefault().Effects.FirstOrDefault() as BasicEffect;
            var texture = effect.Texture;

            // Set the Texture to the Effect
            // 
            basicShader.Parameters["ModelTexture"].SetValue(texture);

            // Assign the mesh effect
            // A model contains a collection of meshes
            foreach (var mesh in modelo.Meshes)
            {
                // A mesh contains a collection of parts
                foreach (var meshPart in mesh.MeshParts)
                    // Assign the loaded effect to each part
                    meshPart.Effect = basicShader;
            }
        }

        public void Update(GameTime gameTime)
        {
            ProcessKeyboard(_game.ElapsedTime);
            UpdateMovementSpeed(_game.ElapsedTime);
            Move();
        }

        public void Move()
        {
            var newOrientacion = new Vector3((float)Math.Sin(anguloDeGiro), 0, (float)Math.Cos(anguloDeGiro));
            orientacion = newOrientacion;

            //TODO improve wave speed modification
            var extraSpeed = 10;
            if (speed <= float.Epsilon) extraSpeed = 0; //Asi no se lo lleva el agua cuando esta parado
            var speedMod = speed + extraSpeed * -Vector3.Dot(orientacionSobreOla, Vector3.Up);

            var newPosition = new Vector3(Position.X - speed * orientacion.X, Position.Y, Position.Z + speed * orientacion.Z);

            Position = newPosition;
        }
        public void Draw()
        {
            var playerCarWorld = _game.CarWorld * Matrix.CreateTranslation(Position);
            for (int i = 0; i < modelo.Meshes.Count; i++)
            {
                var mesh = modelo.Meshes[i];
                for (int j = 0; j < mesh.MeshParts.Count; j++)
                {
                    var part = mesh.MeshParts[j];
                    var effect = part.Effect;
                    effect.Parameters["View"].SetValue(_game.FollowCamera.View);
                    effect.Parameters["Projection"].SetValue(_game.FollowCamera.Projection);
                }
                mesh.Draw();
            }
        }

        private void UpdateMovementSpeed(float gameTime)
        {
            float acceleration;
            if (HandBrake) acceleration = maxacceleration;
            else acceleration = maxacceleration * 8;
            float GearMaxSpeed = (maxspeed);
            if (speed > GearMaxSpeed)
            {
                if (speed - acceleration < GearMaxSpeed)
                {
                    speed = GearMaxSpeed;
                }
                else
                {
                    speed -= acceleration;
                }
            }
            else if (speed < GearMaxSpeed)
            {
                if (speed + acceleration > GearMaxSpeed)
                {
                    speed = GearMaxSpeed;
                }
                else
                {
                    speed += acceleration;
                }
            }
        }
        private void ProcessKeyboard(float elapsedTime)
        {
            var keyboardState = Keyboard.GetState();


            if (keyboardState.IsKeyDown(Keys.A))
            {
                if (speed == 0) { }
                else
                {
                    if (anguloDeGiro + giroBase >= MathF.PI * 2)
                    {
                        anguloDeGiro = anguloDeGiro + giroBase - MathF.PI * 2;
                    }
                    else
                    {
                        anguloDeGiro -= giroBase;
                    }
                }
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                if (speed == 0) { }
                else
                {
                    if (anguloDeGiro + giroBase < 0)
                    {
                        anguloDeGiro = anguloDeGiro - giroBase + MathF.PI * 2;
                    }
                    else
                    {
                        anguloDeGiro += giroBase;
                    }
                }
            }

            if (this.pressedAccelerator == false && keyboardState.IsKeyDown(Keys.W))
            {
                Console.WriteLine("b");
                pressedAccelerator = true;
                if (HandBrake) HandBrake = false;
            }
            if (this.pressedAccelerator == true && keyboardState.IsKeyUp(Keys.W))
            {
                Console.WriteLine("a");
                pressedAccelerator = false;
            }

            if (this.pressedReverse == false && keyboardState.IsKeyDown(Keys.S))
            {
                pressedReverse = true;
                if (HandBrake) HandBrake = false;
            }
            if (this.pressedReverse == true && keyboardState.IsKeyUp(Keys.S))
            {
                pressedReverse = false;
            }

            if (HandBrake == false && keyboardState.IsKeyDown(Keys.Space))
            {
                HandBrake = true;
            }
        }
    }
}
