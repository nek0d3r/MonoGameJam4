using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameJam4
{
    public static class Globals
    {
        // Default size of window
        public static readonly Point DEFAULT_WINDOW_SIZE = new Point(640, 320);

        // Current size of window
        public static Point currentWindowSize = DEFAULT_WINDOW_SIZE;

        // Pixel depth
        public static readonly int PIXEL_DEPTH = 16;

        // Buffer tile dimensions
        public static readonly Point BUFFER_TILE_DIMS = new Point(40, 20);

        // Buffer size
        public static readonly Point BUFFER_SIZE = new Point(BUFFER_TILE_DIMS.X * PIXEL_DEPTH, BUFFER_TILE_DIMS.Y * PIXEL_DEPTH);

        // How to apply buffer to render target
        public static Rectangle renderDims;

        // Default water width from the bottom
        public static readonly int NEUTRAL_WATER_LEVEL = 2;
    }

    public class Main : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _render;
        
        // Captured window size before fullscreen
        private Point previousWindowSize;

        private Texture2D pixel;
        private Texture2D mockup;

        KeyboardState currentKey = new KeyboardState(), prevKey;

        // Tilemaps of background, foreground, and objects
        List<List<Point>> bg, fg, obj;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.HardwareModeSwitch = false;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Allow user to resize window, add event handler
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += WindowChanged;
        }

        /***
         * Thrown on resizing the window.
         * Attempts to fill the buffer while maintaining aspect ratio.
         */
        public void WindowChanged(object sender, EventArgs e)
        {
            // Update buffer bounds
            Globals.currentWindowSize.X = Window.ClientBounds.Width;
            Globals.currentWindowSize.Y = Window.ClientBounds.Height;
            
            var windowAspectRatio = (float)Globals.currentWindowSize.X / Globals.currentWindowSize.Y;
            var bufferAspectRatio = (float)Globals.BUFFER_SIZE.X / Globals.BUFFER_SIZE.Y;

            Point origin, dimensions;

            // If buffer aspect ratio is higher than window aspect ratio, fill to width
            if (bufferAspectRatio > windowAspectRatio)
            {
                dimensions.X = Globals.currentWindowSize.X;
                dimensions.Y = (int)(Globals.currentWindowSize.Y / ((float)bufferAspectRatio / windowAspectRatio));
                origin.X = 0;
                origin.Y = Globals.currentWindowSize.Y / 2 - dimensions.Y / 2;
            }
            // If window aspect ratio is higher than buffer aspect ratio, fill to height
            else if (windowAspectRatio > bufferAspectRatio)
            {
                dimensions.X = (int)(Globals.currentWindowSize.X / ((float)windowAspectRatio / bufferAspectRatio));
                dimensions.Y = Globals.currentWindowSize.Y;
                origin.X = Globals.currentWindowSize.X / 2 - dimensions.X / 2;
                origin.Y = 0;
            }
            // Window aspect ratio matches buffer, fill to bounds
            else
            {
                dimensions.X = Globals.currentWindowSize.X;
                dimensions.Y = Globals.currentWindowSize.Y;
                origin.X = 0;
                origin.Y = 0;
            }

            Globals.renderDims = new Rectangle(origin, dimensions);
        }

        protected override void Initialize()
        {
            bg = new List<List<Point>>();

            for(var i = 0; i < Globals.BUFFER_TILE_DIMS.Y; i++)
            {
                var bgRow = new List<Point>();
                for(var j = 0; j < Globals.BUFFER_TILE_DIMS.X; j++)
                {
                    bgRow.Add(new Point(0,0));
                }
                bg.Add(bgRow);
            }

            base.Initialize();

            // Set buffer size to default
            _graphics.PreferredBackBufferWidth = Globals.DEFAULT_WINDOW_SIZE.X;
            _graphics.PreferredBackBufferHeight = Globals.DEFAULT_WINDOW_SIZE.Y;
            _graphics.ApplyChanges();

            WindowChanged(null, null);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _render = new RenderTarget2D(GraphicsDevice, Globals.BUFFER_SIZE.X, Globals.BUFFER_SIZE.Y);

            pixel = Content.Load<Texture2D>("pixel");
            mockup = Content.Load<Texture2D>("mockup");
        }

        protected override void Update(GameTime gameTime)
        {
            // Set previous key state and update current state
            prevKey = currentKey;
            currentKey = Keyboard.GetState();

            // Default exit on escape or back on Xbox controller
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Window is not supporting Alt + Enter despite Window.AllowAltF4, so this handles shortcut fullscreen toggle
            if ((currentKey.IsKeyDown(Keys.LeftAlt) || currentKey.IsKeyDown(Keys.RightAlt)) && currentKey.IsKeyDown(Keys.Enter) && !prevKey.IsKeyDown(Keys.Enter))
            {
                // If not currently fullscreen, handle window changes before fullscreen
                if (!_graphics.IsFullScreen)
                {
                    // Capture current window size
                    previousWindowSize = Globals.currentWindowSize;
                    // Set graphics buffer to native resolution and apply
                    _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    _graphics.ApplyChanges();
                }
                _graphics.ToggleFullScreen();
                // If not currently fullscreen, handle window changes after exiting fullscreen
                if (!_graphics.IsFullScreen)
                {
                    // Set graphics buffer to previously captured window size and apply
                    _graphics.PreferredBackBufferWidth = previousWindowSize.X;
                    _graphics.PreferredBackBufferHeight = previousWindowSize.Y;
                    _graphics.ApplyChanges();
                }
                // Trigger window change
                WindowChanged(true, null);
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Render drawing to sprite buffer
            GraphicsDevice.SetRenderTarget(_render);
            GraphicsDevice.Clear(Color.Black);

            // Drawing begins here
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            for(var i = 0; i < Globals.BUFFER_TILE_DIMS.Y; i++)
            {
                for(var j = 0; j < Globals.BUFFER_TILE_DIMS.X; j++)
                {
                    _spriteBatch.Draw(
                        mockup,
                        new Rectangle(j * Globals.PIXEL_DEPTH, i * Globals.PIXEL_DEPTH, Globals.PIXEL_DEPTH, Globals.PIXEL_DEPTH),
                        new Rectangle(Globals.PIXEL_DEPTH * bg[i][j].X, Globals.PIXEL_DEPTH * bg[i][j].X, Globals.PIXEL_DEPTH, Globals.PIXEL_DEPTH),
                        Color.White
                    );
                }
            }

            _spriteBatch.End();

            // Set render target to device back buffer and clear
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            // Draw sprite buffer to back buffer
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            _spriteBatch.Draw((Texture2D)_render, Globals.renderDims, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
