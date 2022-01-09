using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameJam4
{
    public static class Globals
    {
        // Default size of buffer
        public static readonly Point DEFAULT_WINDOW_SIZE = new Point(800, 800);

        // Current size of buffer
        public static Point currentWindowSize = DEFAULT_WINDOW_SIZE;

        // How to apply render to buffer
        public static Rectangle renderDims = new Rectangle(Point.Zero, DEFAULT_WINDOW_SIZE);
    }

    public class Main : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _render;
        
        // Captured window size before fullscreen
        private Point previousWindowSize;

        KeyboardState currentKey = new KeyboardState(), prevKey;

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
         * TODO: Handling window maximizing. Currently the last buffer dimensions are stretched to fit the window, but doesn't update to fill or fit properly.
         */
        public void WindowChanged(object sender, EventArgs e)
        {
            // Update buffer bounds
            Globals.currentWindowSize.X = Window.ClientBounds.Width;
            Globals.currentWindowSize.Y = Window.ClientBounds.Height;

            // If width is greater, fill to height
            if (Globals.currentWindowSize.X > Globals.currentWindowSize.Y)
                Globals.renderDims = new Rectangle(Globals.currentWindowSize.X / 2 - Globals.currentWindowSize.Y / 2, 0, Globals.currentWindowSize.Y, Globals.currentWindowSize.Y);
            // If height is greater, fill to width
            else if (Globals.currentWindowSize.Y > Globals.currentWindowSize.X)
                Globals.renderDims = new Rectangle(0, Globals.currentWindowSize.Y / 2 - Globals.currentWindowSize.X / 2, Globals.currentWindowSize.X, Globals.currentWindowSize.X);
            // Buffer is square, fill to bounds
            else
                Globals.renderDims = new Rectangle(0, 0, Globals.currentWindowSize.X, Globals.currentWindowSize.Y);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            // Set buffer size to default
            _graphics.PreferredBackBufferWidth = Globals.DEFAULT_WINDOW_SIZE.X;
            _graphics.PreferredBackBufferHeight = Globals.DEFAULT_WINDOW_SIZE.Y;
            _graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _render = new RenderTarget2D(GraphicsDevice, Globals.currentWindowSize.X, Globals.currentWindowSize.Y);
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
                Console.WriteLine($"Current window size: {Window.ClientBounds}");
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
                Console.WriteLine($"Current window size: {Window.ClientBounds}");
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_render);
            GraphicsDevice.Clear(Color.Black);

            // TODO: Draw here

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin(); // If using sprites: SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp
            _spriteBatch.Draw((Texture2D)_render, Globals.renderDims, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
