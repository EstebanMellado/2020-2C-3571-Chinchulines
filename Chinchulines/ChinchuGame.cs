using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Chinchulines.Graphics;
using Chinchulines.Enemigo;
using System.Collections.Generic;
using Chinchulines.Entities;
using Microsoft.Xna.Framework.Media;
using Chinchulines.Cameras;

namespace Chinchulines
{
    /// <summary>
    ///     Esta es la clase principal  del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class ChinchuGame : Game
    {
        public const string ContentFolderModels = "Models/";
        public const string ContentFolderEffect = "Effects/";
        public const string ContentFolderMusic = "Music/";
        // public const string ContentFolderSounds = "Sounds/";
        // public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";
        public const string ModelMK1 = "Models/Spaceships/SpaceShip-MK1";
        public const string ModelMK3 = "Models/Spaceships/SpaceShip-MK3";
        public const string TextureMK1 = "Textures/Spaceships/MK1/MK1-Texture";
        public const string TextureMK3 = "Textures/Spaceships/MK3/MK3-Albedo";
        public const string CrossHairTexture = "Textures/Crosshair/crosshair";

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public ChinchuGame(bool godmode)
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Descomentar para que el juego sea pantalla completa.
            // Graphics.IsFullScreen = true;
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;

            gmode = godmode;
        }

        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }
        private Model SpaceShipModelMK1 { get; set; }
        private Model SpaceShipModelMK3 { get; set; }
        private Model VenusModel { get; set; }

        private Model TGCcito { get; set; }

        private Texture2D CrossHair;
        private Texture2D HealthBar;

        private Texture2D Victory;
        private Texture2D GameOver;
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }

        private float venusRotation { get; set; }
        BoundingSphere venusSphere;
        private Vector3[] checkpoints = { new Vector3(42f, 1f, -8f),
                                           new Vector3(42f, 1f, -18f),
                                           new Vector3(4f, 1f, -18f),
                                           new Vector3(4f, 1f, -23f),
                                           new Vector3(42f, 1f, -23f),
                                           new Vector3(42f, 1f, -27f),
                                           new Vector3(4f, 1f, -27f),
                                           new Vector3(4f, 1f, -35f),
                                           new Vector3(42f, 1f, -35f),
                                           new Vector3(42f, 1f, -43f), };

        private enemyManager EM;

        private Song background;

        Skybox skybox;

        private Vector3 centerPosition;
        private Trench _trench;

        private Vector3 _lightDirection = new Vector3(3f, 40f, 5f);

        private Vector3 _spaceshipPosition = new Vector3(8f, 7f, -3f);
        private Quaternion _spaceshipRotation = Quaternion.Identity;
        BoundingSphere shipSphere;
        private float movementSpeed;
        private float speedUp;
        private int barrelSide = 0;
        private bool turnBack = false;
        float clock = 0f;

        private Vector3 _cameraPosition;
        private Vector3 _cameraDirection;

        private Random ran = new Random();

        private LaserManager _laserManager;

        public GameState State { get; private set; }

        private SpriteFont _spriteFont;

        private Effect BloomEffect { get; set; }
        private Effect BlurEffect { get; set; }
        private BasicEffect SpaceShipEffect;
        private BasicEffect VenusEffect;
        private BasicEffect TGCcitoEffect;

        private RenderTarget2D MainSceneRenderTarget;
        private RenderTarget2D FirstPassBloomRenderTarget;
        private RenderTarget2D SecondPassBloomRenderTarget;

        private const int PassCount = 2;

        bool gmode;

        private FullScreenQuad FullScreenQuad;

        TimeSpan _timeSpan = TimeSpan.FromMinutes(5);
        int _actualCheckpoint = 0;
        int _health = 100;
        private float _gameSpeed = 1.0f;
        private Vector3 finalBossPosition;

        private Effect ShadowMapEffect { get; set; }
        private RenderTarget2D ShadowMapRenderTarget;
        private const int ShadowmapSize = 2048;

        private readonly float LightCameraFarPlaneDistance = 3000f;

        private readonly float LightCameraNearPlaneDistance = 5f;

        private Vector3 LightPosition = new Vector3(8f, 7f, -3f);
        private float Timer;
        private TargetCamera TargetLightCamera { get; set; }



        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: todo procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // World = Matrix.CreateTranslation(new Vector3(0, 0, 0));
            View = Matrix.CreateLookAt(new Vector3(0, 0, 20), Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 1000f);


            //World = Matrix.Identity;
            //View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
            //Projection =
            //    Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 500);

            centerPosition = new Vector3(0, 0, 0);

            Graphics.PreferredBackBufferWidth = 1024;
            Graphics.PreferredBackBufferHeight = 768;
            Graphics.ApplyChanges();

            EM = new enemyManager();
            for (int i = 0; i < 10; i++) EM.CrearEnemigo();
            EM.CrearEnemigoVigilante(_spaceshipPosition);

            _trench = new Trench();
            _laserManager = new LaserManager();

            finalBossPosition = new Vector3(4f, 1f, -43f);

            State = GameState.Playing;

            TargetLightCamera = new TargetCamera(1f, LightPosition, Vector3.Zero);
            TargetLightCamera.BuildProjection(1f, LightCameraNearPlaneDistance, LightCameraFarPlaneDistance,
                MathHelper.PiOver2);

            base.Initialize();
        }
        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el
        ///     procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            SpaceShipModelMK1 = Content.Load<Model>(ModelMK1); // Se puede cambiar por MK2 y MK3

            SpaceShipEffect = (BasicEffect)SpaceShipModelMK1.Meshes[0].Effects[0];
            SpaceShipEffect.TextureEnabled = true;
            SpaceShipEffect.Texture = Content.Load<Texture2D>(TextureMK1);

            VenusModel = Content.Load<Model>(ContentFolderModels + "Venus/Venus");

            EM.LoadContent(Content, Graphics);

            background = Content.Load<Song>(ContentFolderMusic + "Rising Tide (faster)");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(background);

            CrossHair = Content.Load<Texture2D>(CrossHairTexture);

            Victory = Content.Load<Texture2D>(ContentFolderTextures + "/GameStates/Victory");
            GameOver = Content.Load<Texture2D>(ContentFolderTextures + "/GameStates/GameOver");

            SpaceShipModelMK3 = Content.Load<Model>(ModelMK3);

            var spaceShipEffect3 = (BasicEffect)SpaceShipModelMK3.Meshes[0].Effects[0];
            spaceShipEffect3.TextureEnabled = true;
            spaceShipEffect3.Texture = Content.Load<Texture2D>(TextureMK3); // Se puede cambiar por MK2 y MK3

            VenusEffect = (BasicEffect)VenusModel.Meshes[0].Effects[0];
            VenusEffect.TextureEnabled = true;
            VenusEffect.Texture = Content.Load<Texture2D>(ContentFolderTextures + "Venus/Venus-Texture");

            TGCcito = Content.Load<Model>(ContentFolderModels + "tgc-logo/tgc-logo");
            TGCcitoEffect = (BasicEffect)TGCcito.Meshes[0].Effects[0];
            TGCcitoEffect.DiffuseColor = Color.DarkBlue.ToVector3();
            TGCcitoEffect.EnableDefaultLighting();

            HealthBar = Content.Load<Texture2D>(ContentFolderTextures + "HealthBar/health-bar");

            skybox = new Skybox("Skyboxes/SunInSpace", Content);
            _trench.LoadContent(ContentFolderTextures + "Trench/TrenchTexture", ContentFolderEffect + "Trench", Content, Graphics);
            _laserManager.LoadContent(ContentFolderTextures + "Lasers/doble-laser-verde", ContentFolderEffect + "Trench", Content, Graphics);

            SetUpCamera();

            _spriteFont = Content.Load<SpriteFont>("Fonts/Font");

            BloomEffect = Content.Load<Effect>(ContentFolderEffect + "Bloom");
            BlurEffect = Content.Load<Effect>(ContentFolderEffect + "GaussianBlur");
            BlurEffect.Parameters["screenSize"]
                .SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

            MainSceneRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            FirstPassBloomRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            SecondPassBloomRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.DiscardContents);

            // Create a full screen quad to post-process
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);


            // Create a shadow map. It stores depth from the light position
            ShadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, ShadowmapSize, ShadowmapSize, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
            ShadowMapEffect = Content.Load<Effect>(ContentFolderEffect + "ShadowMap");

            base.LoadContent();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.UnloadContent();
                Exit();
            }

            if (State == GameState.Playing)
            {
                UpdateLightPosition((float)gameTime.ElapsedGameTime.TotalSeconds);
                TargetLightCamera.Position = LightPosition;
                TargetLightCamera.BuildView();

                _timeSpan -= gameTime.ElapsedGameTime;
                if (_timeSpan < TimeSpan.Zero || _health < 0)
                {
                    State = GameState.GameOver;
                }


                UpdateCamera();
                MoveSpaceship(gameTime);
                InputController(gameTime);

                movementSpeed = gameTime.ElapsedGameTime.Milliseconds / 500.0f * _gameSpeed;
                MoveForward(ref _spaceshipPosition, _spaceshipRotation, movementSpeed);

                EM.Update(gameTime, centerPosition);

                CollisionType laserCollision = EM.UpdateEnemigoVigilante(_spaceshipPosition, gameTime, movementSpeed, _health);
                if (laserCollision == CollisionType.Laser && !gmode)
                {
                    _health -= 2;
                }

                venusRotation += .005f;

                _laserManager.UpdateLaserAndCheckCollision(movementSpeed, _spaceshipPosition, _health);

                shipSphere = new BoundingSphere(_spaceshipPosition, 0.09f);
                CollisionType collisionType = CheckCollision(shipSphere);
                if (collisionType != CollisionType.None)
                {
                    if (collisionType == CollisionType.Trench)
                    {
                        _spaceshipPosition = new Vector3(8, 7, -3);
                        _spaceshipRotation = Quaternion.Identity;
                        if (!gmode) _health -= 10;
                    }

                }

                if (_actualCheckpoint < 10)
                {
                    venusSphere = new BoundingSphere(checkpoints[_actualCheckpoint], 0.5f);
                    if (shipSphere.Intersects(venusSphere))
                    {
                        _actualCheckpoint++;
                    }
                }

                if (_actualCheckpoint >= 10)
                {
                    BoundingSphere TgcBound = new BoundingSphere(finalBossPosition, 0.09f);
                    if (shipSphere.Intersects(TgcBound))
                    {
                        State = GameState.Win;
                    }
                }
            }

            base.Update(gameTime);
        }

        private void InputController(GameTime gameTime)
        {
            KeyboardState keystate = Keyboard.GetState();
            if (keystate.IsKeyDown(Keys.Space))
            {
                _laserManager.ShootLaser(gameTime, _spaceshipPosition, _spaceshipRotation);
            }
        }

        private void SetUpCamera()
        {
            View = Matrix.CreateLookAt(new Vector3(20, 13, -5), new Vector3(8, 0, -7), new Vector3(0, 1, 0));
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.2f, 500.0f);
        }

        private void MoveSpaceship(GameTime gameTime)
        {
            float leftRightRotation = 0;
            float upDownRotation = 0;

            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            turningSpeed *= 1.6f * _gameSpeed;

            KeyboardState keys = Keyboard.GetState();

            if (keys.IsKeyDown(Keys.D)) leftRightRotation += turningSpeed;
            if (keys.IsKeyDown(Keys.A)) leftRightRotation -= turningSpeed;
            if (keys.IsKeyDown(Keys.S)) upDownRotation += turningSpeed;
            if (keys.IsKeyDown(Keys.W)) upDownRotation -= turningSpeed;

            if (keys.IsKeyDown(Keys.LeftShift)) if (speedUp < 0.10f) speedUp += 0.01f;
            if (keys.IsKeyDown(Keys.LeftControl)) if (speedUp > 0) speedUp -= 0.01f;

            if (keys.IsKeyDown(Keys.E)) barrelSide = -1;
            if (keys.IsKeyDown(Keys.Q)) barrelSide = 1;
            if (keys.IsKeyDown(Keys.X)) turnBack = true;

            if (turnBack)
            {
                clock++;
                upDownRotation += turningSpeed;
                if (clock > 117f)
                {
                    clock = 0;
                    turnBack = !turnBack;
                    int turn = ran.Next(-1, 1);
                    while (turn == 0) turn = ran.Next(-1, 1);
                    barrelSide = 2 * turn;
                }
            }

            if (barrelSide != 0)
            {
                if (Math.Abs(barrelSide) == 1) BarrelRoll(59f, ref barrelSide);
                else
                {
                    if (Math.Abs(barrelSide) == 2)
                    {
                        BarrelRoll((59f / 2), ref barrelSide);
                    }
                }
            }



            _spaceshipRotation *= Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), leftRightRotation)
                    * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRotation);
        }

        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotationQuat);
            position += addVector * (speed + speedUp);
        }

        private void BarrelRoll(float time, ref int side)
        {
            clock++;

            if (Math.Sign(side) == -1) _spaceshipRotation *= Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), MathHelper.PiOver2 / 15);
            else if (Math.Sign(side) == 1) _spaceshipRotation *= Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), MathHelper.PiOver2 / 15);

            if (clock > time)
            {
                clock = 0;
                side = 0;
                _spaceshipRotation *= Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), 0);
            }
        }

        private void UpdateCamera()
        {

            Vector3 cameraPosition = new Vector3(0, 0.1f, 0.6f);
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateFromQuaternion(_spaceshipRotation));
            cameraPosition += _spaceshipPosition;
            Vector3 cameraUpDirection = new Vector3(0, 1, 0);
            cameraUpDirection = Vector3.Transform(cameraUpDirection, Matrix.CreateFromQuaternion(_spaceshipRotation));

            View = Matrix.CreateLookAt(cameraPosition, _spaceshipPosition, cameraUpDirection);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.2f, 500.0f);

            _cameraPosition = cameraPosition;
            _cameraDirection = cameraUpDirection;
        }

        private void UpdateLightPosition(float elapsedTime)
        {
            LightPosition = new Vector3(MathF.Cos(Timer) * 10f, 5f, MathF.Sin(Timer) * 10f);
            Timer += elapsedTime * 0.5f;
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);



            if (State == GameState.Playing)
            {
                #region Pass 1

                // Use the default blend and depth configuration
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.BlendState = BlendState.Opaque;

                // Set the main render target, here we'll draw the base scene
                GraphicsDevice.SetRenderTarget(MainSceneRenderTarget);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

                DrawCheckpoints();
                DrawSkybox();

                // Assign the basic effect and draw
                foreach (var modelMesh in SpaceShipModelMK1.Meshes)
                    foreach (var part in modelMesh.MeshParts)
                        part.Effect = SpaceShipEffect;
                SpaceShipModelMK1.Draw(
                                        Matrix.CreateScale(0.005f) *
                                        Matrix.CreateFromQuaternion(_spaceshipRotation) *
                                        Matrix.CreateTranslation(_spaceshipPosition)
                        , View, Projection);

                EM.Draw(View, Projection);
                EM.DrawEnemigoVigilante(View, Projection, _spaceshipPosition, _spaceshipRotation, _cameraPosition, _cameraDirection, Graphics);

                _trench.Draw(View, Projection, _lightDirection, Graphics);

                _laserManager.DrawLasers(View, Projection, _cameraPosition, _cameraDirection, Graphics);

                DrawHUD();
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                #endregion

                DrawShadows(SpaceShipModelMK1, Matrix.CreateScale(0.005f) *
                                        Matrix.CreateFromQuaternion(_spaceshipRotation) *
                                        Matrix.CreateTranslation(_spaceshipPosition), SpaceShipEffect);

                #region Pass 2

                // Set the render target as our bloomRenderTarget, we are drawing the bloom color into this texture
                GraphicsDevice.SetRenderTarget(FirstPassBloomRenderTarget);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

                BloomEffect.CurrentTechnique = BloomEffect.Techniques["BloomPass"];
                BloomEffect.Parameters["baseTexture"].SetValue(SpaceShipEffect.Texture);

                // We get the base transform for each mesh
                var modelMeshesBaseTransforms = new Matrix[SpaceShipModelMK1.Bones.Count];
                SpaceShipModelMK1.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
                foreach (var modelMesh in SpaceShipModelMK1.Meshes)
                {
                    foreach (var part in modelMesh.MeshParts)
                        part.Effect = BloomEffect;

                    // We set the main matrices for each mesh to draw
                    var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index];

                    // WorldViewProjection is used to transform from model space to clip space
                    BloomEffect.Parameters["WorldViewProjection"].SetValue(worldMatrix * Matrix.CreateScale(0.005f) *
                                        Matrix.CreateFromQuaternion(_spaceshipRotation) *
                                        Matrix.CreateTranslation(_spaceshipPosition) * View * Projection);

                    // Once we set these matrices we draw
                    modelMesh.Draw();
                }

                #endregion

                #region Multipass Bloom

                BlurEffect.CurrentTechnique = BlurEffect.Techniques["Blur"];

                var bloomTexture = FirstPassBloomRenderTarget;
                var finalBloomRenderTarget = SecondPassBloomRenderTarget;

                for (var index = 0; index < PassCount; index++)
                {
                    GraphicsDevice.SetRenderTarget(finalBloomRenderTarget);
                    GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

                    BlurEffect.Parameters["baseTexture"].SetValue(bloomTexture);
                    FullScreenQuad.Draw(BlurEffect);

                    if (index != PassCount - 1)
                    {
                        var auxiliar = bloomTexture;
                        bloomTexture = finalBloomRenderTarget;
                        finalBloomRenderTarget = auxiliar;
                    }
                }

                #endregion

                #region Final Pass

                GraphicsDevice.DepthStencilState = DepthStencilState.None;

                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);

                BloomEffect.CurrentTechnique = BloomEffect.Techniques["Integrate"];
                BloomEffect.Parameters["baseTexture"].SetValue(MainSceneRenderTarget);
                BloomEffect.Parameters["bloomTexture"].SetValue(finalBloomRenderTarget);
                FullScreenQuad.Draw(BloomEffect);

                #endregion

            }
            else
            if (State == GameState.Win)
            {
                SpriteBatch.Begin(samplerState: GraphicsDevice.SamplerStates[0],
                    rasterizerState: GraphicsDevice.RasterizerState);
                SpriteBatch.Draw(Victory,
                    new Rectangle(200, 100, 600, 600),
                    Color.White);
                SpriteBatch.End();
            }
            else
                if (State == GameState.GameOver)
            {
                SpriteBatch.Begin(samplerState: GraphicsDevice.SamplerStates[0],
                    rasterizerState: GraphicsDevice.RasterizerState);
                SpriteBatch.Draw(GameOver,
                    new Rectangle(200, 100, 600, 600), Color.White);
                SpriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void DrawShadows(Model model, Matrix world, BasicEffect basicEffect)
        {
            #region Pass 1

            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Set the render target as our shadow map, we are drawing the depth into this texture
            //GraphicsDevice.SetRenderTarget(ShadowMapRenderTarget);
            //GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            ShadowMapEffect.CurrentTechnique = ShadowMapEffect.Techniques["DepthPass"];

            // We get the base transform for each mesh
            var modelMeshesBaseTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            foreach (var modelMesh in model.Meshes)
            {
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = ShadowMapEffect;

                // We set the main matrices for each mesh to draw
                var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index];

                // WorldViewProjection is used to transform from model space to clip space
                ShadowMapEffect.Parameters["WorldViewProjection"]
                        .SetValue(worldMatrix * world * TargetLightCamera.View * TargetLightCamera.Projection);

                // Once we set these matrices we draw
                modelMesh.Draw();
            }

            #endregion

            #region Pass 2

            // Set the render target as null, we are drawing on the screen!
            //GraphicsDevice.SetRenderTarget(null);
            //GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            ShadowMapEffect.CurrentTechnique = ShadowMapEffect.Techniques["DrawShadowedPCF"];
            ShadowMapEffect.Parameters["baseTexture"].SetValue(basicEffect.Texture);
            ShadowMapEffect.Parameters["shadowMap"].SetValue(ShadowMapRenderTarget);
            ShadowMapEffect.Parameters["lightPosition"].SetValue(LightPosition);
            ShadowMapEffect.Parameters["shadowMapSize"].SetValue(Vector2.One * ShadowmapSize);
            ShadowMapEffect.Parameters["LightViewProjection"].SetValue(TargetLightCamera.View * TargetLightCamera.Projection);
            foreach (var modelMesh in model.Meshes)
            {
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = ShadowMapEffect;

                // We set the main matrices for each mesh to draw
                var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index];

                // WorldViewProjection is used to transform from model space to clip space
                ShadowMapEffect.Parameters["WorldViewProjection"].SetValue(worldMatrix * world * View * Projection);
                ShadowMapEffect.Parameters["World"].SetValue(worldMatrix);
                ShadowMapEffect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix)));

                // Once we set these matrices we draw
                modelMesh.Draw();
            }

            #endregion

        }

        private void DrawCheckpoints()
        {
            if (_actualCheckpoint < 10)
                DrawShadows(VenusModel, Matrix.CreateScale(.01f) *
                Matrix.CreateRotationY(venusRotation) *
                Matrix.CreateTranslation(checkpoints[_actualCheckpoint]), VenusEffect);

            if (_actualCheckpoint >= 10) TGCcito.Draw(
                  Matrix.CreateScale(.005f) *
                  Matrix.CreateRotationY(venusRotation) *
                  Matrix.CreateTranslation(finalBossPosition), View, Projection);

        }

        private void DrawSkybox()
        {
            RasterizerState originalRasterizerState = Graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Graphics.GraphicsDevice.RasterizerState = rasterizerState;

            skybox.Draw(View, Projection, centerPosition);

            Graphics.GraphicsDevice.RasterizerState = originalRasterizerState;
        }
        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }

        // TODO: Agregar variables para el tiempo restante, checkpoints y vida
        private void DrawHUD()
        {
            SpriteBatch.Begin(samplerState: GraphicsDevice.SamplerStates[0], rasterizerState: GraphicsDevice.RasterizerState);
            SpriteBatch.DrawString(_spriteFont, $"TIEMPO RESTANTE:\n    {_timeSpan.Minutes}:{_timeSpan.Seconds}",
                new Vector2(50, 50), Color.IndianRed);
            SpriteBatch.DrawString(_spriteFont, $"CHECKPOINT:\n    {_actualCheckpoint}/10",
                new Vector2(GraphicsDevice.Viewport.Width / 3 + 75, 50), Color.IndianRed);
            SpriteBatch.DrawString(_spriteFont, $"VIDA:",
                new Vector2(GraphicsDevice.Viewport.Width / 4 * 3 + 75, 50), Color.IndianRed);
            SpriteBatch.Draw(HealthBar, new Rectangle(GraphicsDevice.Viewport.Width / 4 * 3 + 75, 70, _health, 20), Color.White);

            SpriteBatch.Draw(CrossHair,
                new Vector2((Window.ClientBounds.Width / 2) - (CrossHair.Width / 2),
                (Window.ClientBounds.Height / 2) - (CrossHair.Height / 2) - 150),
                Color.White);

            SpriteBatch.End();

        }

        private CollisionType CheckCollision(BoundingSphere sphere)
        {
            for (int i = 0; i < _trench.TrenchBoundingBoxes.Length; i++)
            {
                if (_trench.TrenchBoundingBoxes[i].Contains(sphere) != ContainmentType.Disjoint)
                {
                    return CollisionType.Trench;
                }
            }

            if (_trench.CompleteTrenchBox.Contains(sphere) != ContainmentType.Contains)
            {
                return CollisionType.Trench;
            }

            return CollisionType.None;
        }
    }
}
