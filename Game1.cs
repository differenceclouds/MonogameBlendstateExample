using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BlendstateExample {
    public class Game1 : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        RenderTarget2D composite;

        BlendState currentBlend;
        BlendState blendMask;
        BlendState blendMaskInverse;

        Texture2D background;
        Texture2D info;
        Texture2D sprite1;
        Texture2D sprite2;
        Texture2D sprite3;
        Texture2D mask1;
        Texture2D mask2;
        Texture2D pixel;

        SpriteFont font;

        Rectangle window;

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent() {
            window = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            background = Content.Load<Texture2D>("background");
            info = Content.Load<Texture2D>("info");

            sprite1 = Content.Load<Texture2D>("sprite1");
            sprite2 = Content.Load<Texture2D>("sprite2");
            sprite3 = Content.Load<Texture2D>("durer_solid");

            //black and white image with no alpha data
            mask1 = AlphaFromColor(Content.Load<Texture2D>("clouds"), false);
            mask2 = AlphaFromColor(Content.Load<Texture2D>("dot"), false);

            composite = new RenderTarget2D(GraphicsDevice, window.Width, window.Height);

            blendMask = new BlendState {
                //Zero mask texture so it doesn't render
                ColorSourceBlend = Blend.Zero,
                AlphaSourceBlend = Blend.Zero,

                //Multiply color and alpha by mask alpha
                ColorDestinationBlend = Blend.SourceAlpha,
                AlphaDestinationBlend = Blend.SourceAlpha,

                //ColorBlendFunction and AlphaBlendFunction don't matter here as mask is zeroed. default is BlendFunction.Add.
            };
            blendMaskInverse = new BlendState {
                ColorSourceBlend = Blend.Zero,
                AlphaSourceBlend = Blend.Zero,

                ColorDestinationBlend = Blend.InverseSourceAlpha,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
            };

            currentBlend = blendMask;
        }


        /// <summary>
		/// Set alpha channel from texture color (red channel). blackout sets colors to black (makes no difference in this example). 
		/// </summary>
        static Texture2D AlphaFromColor(Texture2D input, bool blackout) {
            var output = input;
            var textureData = new Color[output.Width * output.Height];
            output.GetData(textureData);
            for (int i = 0; i < textureData.Length; i++) {
                var alpha = textureData[i].R;
                if (blackout)
                    textureData[i] = new Color((byte)0, (byte)0, (byte)0, alpha);
                else
                    textureData[i].A = alpha;
            }
            output.SetData(textureData);
            return output;
        }


        bool spacePressed = false;
        bool animating = true;
        bool IKeyPressed = false;
        bool inverted = false;

        protected override void Update(GameTime gameTime) {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            bool prevSpace = spacePressed;
            spacePressed = Keyboard.GetState().IsKeyDown(Keys.Space);

            if (spacePressed && spacePressed != prevSpace)
                animating = !animating;

            bool prevI = IKeyPressed;
            IKeyPressed = Keyboard.GetState().IsKeyDown(Keys.I);

            if (IKeyPressed && IKeyPressed != prevI) {
                inverted = !inverted;
                //mask = inverted ? maskTextureInverse : maskTexture;
                currentBlend = inverted ? blendMaskInverse : blendMask;
            }
        }

        int frame = 0;
        protected override void Draw(GameTime gameTime) {
            if (animating) frame++;

            Rectangle rightHalf = new Rectangle(composite.Width / 2, 0, composite.Width / 2, composite.Height);
            //composite sprites and mask to rendertarget...
            {
                GraphicsDevice.SetRenderTarget(composite);
                GraphicsDevice.Clear(Color.Transparent); //important for default BlendState.Alphablend

                spriteBatch.Begin();
                spriteBatch.Draw(sprite1, new Vector2(0,0), Color.White);
                spriteBatch.Draw(sprite2, new Vector2(285, 150), Color.White);
                spriteBatch.Draw(sprite3, new Vector2(50, 200), Color.White);

                //draw white rectangle to show mask on right half of screen
                spriteBatch.Draw(pixel, rightHalf, Color.White);
                spriteBatch.End();


                
                //draw mask. PointWrap to tile mask textures
                spriteBatch.Begin(blendState: currentBlend, samplerState: SamplerState.PointWrap);
                spriteBatch.Draw(mask1, composite.Bounds, new Rectangle(frame, frame, composite.Width, composite.Height), Color.White);
				spriteBatch.Draw(mask2, composite.Bounds, new Rectangle(frame/4, -frame, composite.Width, composite.Height), Color.White);
                spriteBatch.End();
            }

            //draw background and rendertarget...
            {
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.CornflowerBlue);

                spriteBatch.Begin(samplerState: SamplerState.PointWrap);

                //draw background
                spriteBatch.Draw(background, Vector2.Zero, Color.White);

                //draw black background to show mask on right half of screen
                spriteBatch.Draw(pixel, rightHalf, Color.Black);

                //draw sprite + mask composite
                spriteBatch.Draw(composite, Vector2.Zero, Color.White);

                spriteBatch.Draw(info, new Vector2(window.Width - info.Width, window.Height - info.Height), Color.White);

                spriteBatch.End();
            }
        }


    }
}