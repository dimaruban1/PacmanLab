using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    internal class FinalScreen : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        SpriteFont mainFont;
        
        string message;


        public FinalScreen(string message)
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            this.message = message;
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().GetPressedKeyCount() > 0) 
            { 
                Exit(); 
            }
            base.Update(gameTime);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            mainFont = Content.Load<SpriteFont>("alice");
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            _spriteBatch.DrawString(mainFont, message,
                new Vector2((GraphicsDevice.Viewport.Width - mainFont.MeasureString(message).X) / 2,
                            (GraphicsDevice.Viewport.Height - mainFont.MeasureString(message).Y) / 2),
                Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
