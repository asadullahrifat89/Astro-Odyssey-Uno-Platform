﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using static AstroOdyssey.Constants;
using System.Diagnostics;
using System.Threading.Tasks;
using Uno.Foundation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AstroOdyssey
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        #region Fields

        private string baseUrl;

        private int fpsCounter = 0;
        private int frameStatUpdateCounter;

        private double windowWidth, windowHeight;

        //private object backgroundAudio = null;

        //private object enemyDestructionAudio = null;
        //private object meteorDestructionAudio = null;

        //private object playerHealthLossAudio = null;
        //private object playerHealthGainAudio = null;

        //private object levelUpAudio = null;

        //private object powerUpAudio = null;
        //private object powerDownAudio = null;

        //private object laserImpactAudio = null;
        //private object laserAudio = null;
        //private object laserPoweredUpModeAudio = null;

        private int powerUpCounter = 1500;
        private int powerUpTriggerCounter;
        private int laserCounter;
        private int enemyCounter;
        private int meteorCounter;
        private int enemySpawnCounter;
        private int healthCounter;
        private int starCounter;
        private int playerDamagedOpacityCounter;
        private int showInGameTextCounter;

        private readonly Random random = new Random();

        private readonly Stack<GameObject> enemyStack = new Stack<GameObject>();
        private readonly Stack<GameObject> meteorStack = new Stack<GameObject>();
        private readonly Stack<GameObject> healthStack = new Stack<GameObject>();
        private readonly Stack<GameObject> powerUpStack = new Stack<GameObject>();
        private readonly Stack<GameObject> starStack = new Stack<GameObject>();

        private bool moveLeft = false, moveRight = false;

        private System.Timers.Timer frameGenerationTimer;

        #endregion

        #region Ctor

        public GamePage()
        {
            InitializeComponent();

            Loaded += GamePage_Loaded;
            Unloaded += GamePage_Unloaded;

            SetWindowSize();
        }

        #endregion

        #region Properties

        private int FpsCount { get; set; } = 0;

        private float LastFrameTime { get; set; } = 0;

        private long FrameStartTime { get; set; }

        private bool PowerUpTriggered { get; set; }

        private double LaserSpeed { get; set; } = 18;

        private double EnemySpeed { get; set; } = 3;

        private double MeteorSpeed { get; set; } = 2;

        private double HealthSpeed { get; set; } = 2;

        private double PowerUpSpeed { get; set; } = 2;

        private double StarSpeed { get; set; } = 0.1d;

        private double PlayerSpeed { get; set; } = 15;

        private int FrameStatUpdateLimit { get; set; } = 5;

        private int LaserSpawnLimit { get; set; } = 18;

        private int PowerUpTriggerLimit { get; set; } = 500;

        private int EnemySpawnLimit { get; set; } = 35;

        private int MeteorSpawnLimit { get; set; } = 40;

        private int HealthSpawnLimit { get; set; } = 1000;

        private int PowerUpSpawnLimit { get; set; } = 1500;

        private int StarSpawnLimit { get; set; } = 100;

        private int PlayerDamagedOpacityLimit { get; set; } = 100;

        private int ShowInGameTextLimit { get; set; } = 100;

        private Player Player { get; set; }

        private GameLevel GameLevel { get; set; }

        private double Score { get; set; } = 0;

        private double PointerX { get; set; }

        private int FrameDuration { get; set; } = 13;

        private bool IsGameRunning { get; set; }

        #endregion

        #region Methods

        #region Game Methods

        /// <summary>
        /// Starts the game. Spawns the player and starts game and laser loops.
        /// </summary>
        private void StartGame()
        {
            PlaySound(SoundType.BACKGROUND_MUSIC);
            SpawnPlayer();
            SpawnStar();
            IsGameRunning = true;
            RunGame();
        }

        /// <summary>
        /// Runs game. Updates stats, gets player bounds, spawns enemies and meteors, moves the player, updates the frame, scales difficulty, checks player health, calculates fps and frame time.
        /// </summary>
        private void RunGame()
        {
            var watch = Stopwatch.StartNew();

            //while (IsGameRunning)
            //{
            //    UpdateGameStats();

            //    SpawnEnemy();

            //    SpawnMeteor();

            //    SpawnHealth();

            //    SpawnPowerUp();

            //    SpawnStar();

            //    SpawnLaser(PowerUpTriggered);

            //    MovePlayer();

            //    UpdateGameView();

            //    UpdateStarView();

            //    ShiftGameLevel();

            //    HideInGameText();

            //    TriggerPowerDown();

            //    PlayerOpacity();

            //    CheckPlayerDeath();

            //    //KeyboardFocus();

            //    CalculateFps();

            //    SetFrameAnalytics();

            //    FrameStartTime = watch.ElapsedMilliseconds;

            //    await ElapseFrameDuration();
            //}

            frameGenerationTimer = new System.Timers.Timer(FrameDuration);

            frameGenerationTimer.Elapsed += (s, e) =>
            {
                UpdateGameStats();

                SpawnEnemy();

                SpawnMeteor();

                SpawnHealth();

                SpawnPowerUp();

                SpawnStar();

                SpawnLaser(PowerUpTriggered);

                MovePlayer();

                UpdateGameView();

                UpdateStarView();

                ShiftGameLevel();

                HideInGameText();

                TriggerPowerDown();

                PlayerOpacity();

                CheckPlayerDeath();

                //KeyboardFocus();

                CalculateFps();

                SetFrameAnalytics();

                FrameStartTime = watch.ElapsedMilliseconds;
            };

            frameGenerationTimer.Start();
        }

        /// <summary>
        /// Stops the game.
        /// </summary>
        private void StopGame()
        {
            StopBackgroundMusic();
            IsGameRunning = false;

            frameGenerationTimer?.Stop();
            frameGenerationTimer?.Dispose();
        }

        /// <summary>
        /// Brings focus on keyboard so that keyboard events work.
        /// </summary>
        private void KeyboardFocus()
        {
            GameView.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Updates the game score, player health.
        /// </summary>
        private void UpdateGameStats()
        {
            ScoreText.Text = "Score: " + Score;
            HealthText.Text = Player.GetHealthPoints();
        }

        /// <summary>
        /// Shows the level up text in game view.
        /// </summary>
        private void ShowLevelUp()
        {
            ShowInGameText(GameLevel.ToString().ToUpper().Replace("_", " "));
            PlaySound(SoundType.LEVEL_UP);
        }

        /// <summary>
        /// Shows the in game text in game view.
        /// </summary>
        private void ShowInGameText(string text)
        {
            InGameText.Text = text;
            InGameText.Visibility = Visibility.Visible;
            showInGameTextCounter = ShowInGameTextLimit;
        }

        /// <summary>
        /// Hides the in game text after keeping it visible.
        /// </summary>
        private void HideInGameText()
        {
            if (InGameText.Visibility == Visibility.Visible)
            {
                showInGameTextCounter -= 1;

                if (showInGameTextCounter <= 0)
                {
                    InGameText.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Sets the window and canvas size on startup.
        /// </summary>
        private void SetWindowSize()
        {
            windowWidth = Window.Current.Bounds.Width;
            windowHeight = Window.Current.Bounds.Height;

            PointerX = windowWidth / 2;

            SetViewSizes();
        }

        /// <summary>
        /// Sets the game and star view sizes according to current window size.
        /// </summary>
        private void SetViewSizes()
        {
            GameView.SetSize(windowHeight, windowWidth);
            StarView.SetSize(windowHeight, windowWidth);
        }

        /// <summary>
        /// Updates a frame of game view. Advances game objects in the frame.
        /// </summary>
        private void UpdateGameView()
        {
            var gameObjects = GameView.GetGameObjects<GameObject>().Where(x => x is not AstroOdyssey.Player);

            foreach (var gameObject in gameObjects)
            {
                UpdateGameViewObjects(gameObject);
            }

            ClearGameView();
        }

        /// <summary>
        /// Updates game objects in game view. Moves the objects. Detects collision causes and applies effects.
        /// </summary>
        /// <param name="gameObject"></param>
        private void UpdateGameViewObjects(GameObject gameObject)
        {
            if (gameObject.MarkedForFadedRemoval)
            {
                gameObject.Fade();

                if (gameObject.HasFadedAway)
                {
                    GameView.AddDestroyableGameObject(gameObject);
                    return;
                }
            }

            var tag = gameObject.Tag;

            switch (tag)
            {
                case Constants.ENEMY:
                    {
                        var enemy = gameObject as Enemy;

                        // move enemy down
                        enemy.MoveY();
                        enemy.MoveX();

                        // if the object is marked for lazy destruction then no need to perform collisions
                        if (enemy.MarkedForFadedRemoval)
                            return;

                        // if enemy or meteor object has gone beyond game view
                        if (RemoveGameObject(enemy))
                            return;

                        // check if enemy collides with player
                        if (PlayerCollision(enemy))
                            return;

                        // perform laser collisions
                        LaserCollision(enemy);
                    }
                    break;
                case Constants.METEOR:
                    {
                        var meteor = gameObject as Meteor;

                        // move meteor down
                        meteor.Rotate();
                        meteor.MoveY();

                        // if the object is marked for lazy destruction then no need to perform collisions
                        if (meteor.MarkedForFadedRemoval)
                            return;

                        // if enemy or meteor object has gone beyond game view
                        if (RemoveGameObject(meteor))
                            return;

                        // check if meteor collides with player
                        if (PlayerCollision(meteor))
                            return;

                        // perform laser collisions
                        LaserCollision(meteor);
                    }
                    break;
                case Constants.LASER:
                    {
                        var laser = gameObject as Laser;

                        // move laser up                
                        laser.MoveY();

                        // remove laser if outside game canvas
                        if (laser.GetY() < 10)
                            GameView.AddDestroyableGameObject(laser);
                    }
                    break;
                case Constants.HEALTH:
                    {
                        var health = gameObject as Health;

                        // move Health down
                        health.MoveY();

                        // if health object has gone below game view
                        if (health.GetY() > GameView.Height)
                            GameView.AddDestroyableGameObject(health);

                        if (Player.GetRect().Intersects(health.GetRect()))
                        {
                            GameView.AddDestroyableGameObject(health);
                            PlayerHealthGain(health);
                        }
                    }
                    break;
                case Constants.POWERUP:
                    {
                        var powerUp = gameObject as PowerUp;

                        // move PowerUp down
                        powerUp.MoveY();

                        // if PowerUp object has gone below game view
                        if (powerUp.GetY() > GameView.Height)
                            GameView.AddDestroyableGameObject(powerUp);

                        if (Player.GetRect().Intersects(powerUp.GetRect()))
                        {
                            GameView.AddDestroyableGameObject(powerUp);
                            TriggerPowerUp();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Clears destroyable objects from game view.
        /// </summary>
        private void ClearGameView()
        {
            foreach (var destroyable in GameView.GetDestroyableGameObjects())
            {
                if (destroyable is Enemy enemy)
                    enemyStack.Push(enemy);

                if (destroyable is Meteor meteor)
                    meteorStack.Push(meteor);

                if (destroyable is Health health)
                    healthStack.Push(health);

                GameView.RemoveGameObject(destroyable);
            }

            GameView.ClearDestroyableGameObjects();
        }

        /// <summary>
        /// Removes a game object from game view. 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        private bool RemoveGameObject(GameObject gameObject)
        {
            if (gameObject.GetY() > GameView.Height || gameObject.GetX() > GameView.Width || gameObject.GetX() + gameObject.Width < 10)
            {
                GameView.AddDestroyableGameObject(gameObject);

                return true;
            }

            return false;
        }

        #endregion

        #region Frame Methods      

        /// <summary>
        /// Sets analytics of fps, frame time and objects currently in view.
        /// </summary>
        private void SetFrameAnalytics()
        {
            frameStatUpdateCounter -= 1;

            if (frameStatUpdateCounter < 0)
            {
                FPSText.Text = "FPS: " + FpsCount;
#if DEBUG
                FrameDurationText.Text = "Frame duration: " + FrameDuration + "ms";
                ObjectsCountText.Text = "Objects count: " + GameView.Children.Count();
#endif

                frameStatUpdateCounter = FrameStatUpdateLimit;
            }
        }

        /// <summary>
        /// Elapses the frame duration.
        /// </summary>
        /// <returns></returns>
        private async Task ElapseFrameDuration()
        {
            await Task.Delay(FrameDuration);
        }

        /// <summary>
        /// Calculates frames per second.
        /// </summary>
        /// <param name="frameStartTime"></param>
        private void CalculateFps()
        {
            // calculate FPS
            if (LastFrameTime + 1000 < FrameStartTime)
            {
                FpsCount = fpsCounter;
                fpsCounter = 0;
                LastFrameTime = FrameStartTime;
            }

            fpsCounter++;
        }

        #endregion        

        #region Score Methods

        /// <summary>
        /// Increase player score if an enemy was destroyed.
        /// </summary>
        private void PlayerScoreByEnemyDestruction()
        {
            Score += 2;
        }

        /// <summary>
        /// Increase player score if a meteor was destroyed.
        /// </summary>
        private void PlayerScoreByMeteorDestruction()
        {
            Score++;
        }

        #endregion

        #region Difficulty Methods

        /// <summary>
        /// Shifts the game level according to score; 
        /// </summary>
        private void ShiftGameLevel()
        {
            var lastGameLevel = GameLevel;

            if (Score > 0)
                GameLevel = GameLevel.Level_1;
            if (Score > 25)
                GameLevel = GameLevel.Level_2;
            if (Score > 50)
                GameLevel = GameLevel.Level_3;
            if (Score > 100)
                GameLevel = GameLevel.Level_4;
            if (Score > 200)
                GameLevel = GameLevel.Level_5;
            if (Score > 400)
                GameLevel = GameLevel.Level_6;
            if (Score > 800)
                GameLevel = GameLevel.Level_7;
            if (Score > 1600)
                GameLevel = GameLevel.Level_8;

            // when difficulty changes show level up
            if (lastGameLevel != GameLevel)
            {
                ShowLevelUp();
                AdjustGameEnvironmentToGameLevel();
            }
        }

        /// <summary>
        /// Adjusts the game environment according to game level.
        /// </summary>
        private void AdjustGameEnvironmentToGameLevel()
        {
            switch (GameLevel)
            {
                case GameLevel.Level_1:
                    break;
                default:
                    {
                        EnemySpawnLimit -= 3;
                        EnemySpeed += 2;

                        MeteorSpawnLimit -= 3;
                        MeteorSpeed += 2;

                        LaserSpawnLimit -= 1;

                        HealthSpeed += 2;
                        PowerUpSpeed += 2;

                        StarSpeed += 0.1d;
                    }
                    break;
            }
        }

        #endregion

        #region Player Methods

        /// <summary>
        /// Spawns the player.
        /// </summary>
        private void SpawnPlayer()
        {
            Player = new Player();
            Player.SetAttributes(PlayerSpeed);
            Player.AddToGameEnvironment(top: windowHeight - Player.Height - 20, left: PointerX, gameEnvironment: GameView);
        }

        /// <summary>
        /// Sets the y axis position of the player on game canvas.
        /// </summary>
        private void SetPlayerY()
        {
            Player.SetY(windowHeight - Player.Height - 20);
        }

        /// <summary>
        /// Sets the x axis position of the player on game canvas.
        /// </summary>
        /// <param name="x"></param>
        private void SetPlayerX(double x)
        {
            Player.SetX(x);
        }

        /// <summary>
        /// Moves the player to last pointer pressed position by x axis.
        /// </summary>
        private void MovePlayer()
        {
            var playerX = Player.GetX();
            var playerWidthHalf = Player.Width / 2;

            if (moveLeft && playerX > 0)
                PointerX -= Player.Speed;

            if (moveRight && playerX + Player.Width < windowWidth)
                PointerX += Player.Speed;

            // move right
            if (PointerX - playerWidthHalf > playerX + Player.Speed)
            {
                if (playerX + playerWidthHalf < windowWidth)
                {
                    SetPlayerX(playerX + Player.Speed);
                }
            }

            // move left
            if (PointerX - playerWidthHalf < playerX - Player.Speed)
            {
                SetPlayerX(playerX - Player.Speed);
            }
        }

        /// <summary>
        /// Check if player is dead.
        /// </summary>
        private void CheckPlayerDeath()
        {
            // game over
            if (Player.HasNoHealth)
            {
                HealthText.Text = "Game Over";
                StopGame();

                //var contentDialogue = new MessageDialogueWindow(title: "GAME OVER", message: "Would you like to play again?", result: (result) =>
                //{
                //    if (result)
                //        App.NavigateToPage("/GamePage");
                //    else
                //        App.NavigateToPage("/GameStartPage");
                //});
                //contentDialogue.Show();
            }
        }

        /// <summary>
        /// Makes the player loose health.
        /// </summary>
        private void PlayerHealthLoss()
        {
            Player.LooseHealth();

            PlaySound(SoundType.HEALTH_LOSS);

            Player.Opacity = 0.4d;

            playerDamagedOpacityCounter = PlayerDamagedOpacityLimit;
        }

        /// <summary>
        /// Sets the player opacity.
        /// </summary>
        private void PlayerOpacity()
        {
            playerDamagedOpacityCounter -= 1;

            if (playerDamagedOpacityCounter <= 0)
            {
                Player.Opacity = 1;
            }
        }

        /// <summary>
        /// Makes the player gain health.
        /// </summary>
        /// <param name="health"></param>
        private void PlayerHealthGain(Health health)
        {
            Player.GainHealth(health.Health);

            PlaySound(SoundType.HEALTH_GAIN);
        }

        /// <summary>
        /// Checks and performs player collision.
        /// </summary>
        /// <param name="gameObject"></param>
        /// 
        /// <returns></returns>
        private bool PlayerCollision(GameObject gameObject)
        {
            if (Player.GetRect().Intersects(gameObject.GetRect()))
            {
                GameView.AddDestroyableGameObject(gameObject);
                PlayerHealthLoss();

                return true;
            }

            return false;
        }

        #endregion

        #region Laser Methods        

        /// <summary>
        /// Spawns a laser.
        /// </summary>
        private void SpawnLaser(bool isPoweredUp)
        {
            // each frame progress decreases this counter
            laserCounter -= 1;

            if (laserCounter <= 0)
            {
                // any object falls within player range
                if (GameView.GetGameObjects<GameObject>().Where(x => x.IsDestructible).Any(x => Player.AnyNearbyObjectsOnTheRight(gameObject: x) || Player.AnyNearbyObjectsOnTheLeft(gameObject: x)))
                {
                    double laserHeight = 0, laserWidth = 0;

                    switch (GameLevel)
                    {
                        case GameLevel.Level_1:
                            { laserHeight = 25; laserWidth = 5; }
                            break;
                        case GameLevel.Level_2:
                            { laserHeight = 30; laserWidth = 10; }
                            break;
                        case GameLevel.Level_3:
                            { laserHeight = 35; laserWidth = 15; }
                            break;
                        case GameLevel.Level_4:
                            { laserHeight = 40; laserWidth = 20; }
                            break;
                        case GameLevel.Level_5:
                            { laserHeight = 45; laserWidth = 25; }
                            break;
                        case GameLevel.Level_6:
                            { laserHeight = 50; laserWidth = 30; }
                            break;
                        case GameLevel.Level_7:
                            { laserHeight = 55; laserWidth = 35; }
                            break;
                        case GameLevel.Level_8:
                            { laserHeight = 60; laserWidth = 40; }
                            break;
                        default:
                            break;
                    }

                    GenerateLaser(laserHeight: laserHeight, laserWidth: laserWidth, isPoweredUp: isPoweredUp);
                }

                laserCounter = LaserSpawnLimit;
            }
        }

        /// <summary>
        /// Generates a laser.
        /// </summary>
        /// <param name="laserHeight"></param>
        /// <param name="laserWidth"></param>
        private void GenerateLaser(double laserHeight, double laserWidth, bool isPoweredUp)
        {
            var newLaser = new Laser();

            newLaser.SetAttributes(speed: LaserSpeed, height: laserHeight, width: laserWidth, isPoweredUp: isPoweredUp);

            newLaser.AddToGameEnvironment(top: Player.GetY() - 20, left: Player.GetX() + Player.Width / 2 - newLaser.Width / 2, gameEnvironment: GameView);

            if (newLaser.IsPoweredUp)
                PlaySound(SoundType.LASER_FIRE_POWERED_UP);
            else
                PlaySound(SoundType.LASER_FIRE);
        }

        /// <summary>
        /// Checks and performs laser collision.
        /// </summary>
        /// <param name="gameObject"></param>
        /// 
        private void LaserCollision(GameObject gameObject)
        {
            var lasers = GameView.GetGameObjects<Laser>().Where(laser => laser.GetRect().Intersects(gameObject.GetRect()));

            if (lasers is not null && lasers.Any())
            {
                foreach (var laser in lasers)
                {
                    GameView.AddDestroyableGameObject(laser);

                    // if laser is powered up then execute over kill
                    if (laser.IsPoweredUp)
                        gameObject.LooseHealth(gameObject.HealthSlot * 2);
                    else
                        gameObject.LooseHealth();

                    // move the enemy backwards a bit on laser hit
                    gameObject.MoveY(gameObject.Speed * 4 / 2, YDirection.UP);

                    PlaySound(SoundType.LASER_HIT);

                    if (gameObject.HasNoHealth)
                    {
                        switch (gameObject.Tag)
                        {
                            case Constants.ENEMY:
                                {
                                    DestroyEnemy(gameObject as Enemy);
                                }
                                break;
                            case Constants.METEOR:
                                {
                                    DestroyMeteor(gameObject as Meteor);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Enemy Methods

        /// <summary>
        /// Spawns an enemy.
        /// </summary>
        private void SpawnEnemy()
        {
            // each frame progress decreases this counter
            enemyCounter -= 1;

            // when counter reaches zero, create an enemy
            if (enemyCounter < 0)
            {
                GenerateEnemy();

                enemySpawnCounter++;

                enemyCounter = EnemySpawnLimit;
            }
        }

        /// <summary>
        /// Generates a random enemy.
        /// </summary>
        private void GenerateEnemy()
        {
            var newEnemy = enemyStack.Any() ? enemyStack.Pop() as Enemy : new Enemy();

            newEnemy.SetAttributes(EnemySpeed + random.Next(0, 4));

            var left = random.Next(10, (int)windowWidth - 100);
            var top = 0 - newEnemy.Height;

            // when not noob anymore enemy moves sideways
            if ((int)GameLevel > 0 && enemySpawnCounter >= 10)
            {
                newEnemy.XDirection = (XDirection)random.Next(1, 3);
                enemySpawnCounter = 0;
            }

            newEnemy.AddToGameEnvironment(top: top, left: left, gameEnvironment: GameView);
        }

        /// <summary>
        /// Destroys an enemy. Removes from game environment, increases player score, plays sound effect.
        /// </summary>
        /// <param name="meteor"></param>
        private void DestroyEnemy(Enemy enemy)
        {
            enemy.MarkedForFadedRemoval = true;

            PlayerScoreByEnemyDestruction();

            PlaySound(SoundType.ENEMY_DESTRUCTION);
        }

        #endregion

        #region Meteor Methods

        /// <summary>
        /// Spawns a meteor.
        /// </summary>
        private void SpawnMeteor()
        {
            if ((int)GameLevel > 0)
            {
                // each frame progress decreases this counter
                meteorCounter -= 1;

                // when counter reaches zero, create a meteor
                if (meteorCounter < 0)
                {
                    GenerateMeteor();
                    meteorCounter = MeteorSpawnLimit;
                }
            }
        }

        /// <summary>
        /// Generates a random meteor.
        /// </summary>
        private void GenerateMeteor()
        {
            var newMeteor = meteorStack.Any() ? meteorStack.Pop() as Meteor : new Meteor();

            newMeteor.SetAttributes(MeteorSpeed + random.NextDouble());
            newMeteor.AddToGameEnvironment(top: 0 - newMeteor.Height, left: random.Next(10, (int)windowWidth - 100), gameEnvironment: GameView);
        }

        /// <summary>
        /// Destroys a meteor. Removes from game environment, increases player score, plays sound effect.
        /// </summary>
        /// <param name="meteor"></param>
        private void DestroyMeteor(Meteor meteor)
        {
            meteor.MarkedForFadedRemoval = true;

            PlayerScoreByMeteorDestruction();

            PlaySound(SoundType.METEOR_DESTRUCTION);
        }

        #endregion

        #region Health Methods

        /// <summary>
        /// Spawns a Health.
        /// </summary>
        private void SpawnHealth()
        {
            if (Player.Health <= 60)
            {
                // each frame progress decreases this counter
                healthCounter -= 1;

                // when counter reaches zero, create a Health
                if (healthCounter < 0)
                {
                    GenerateHealth();
                    healthCounter = HealthSpawnLimit;
                }
            }
        }

        /// <summary>
        /// Generates a random Health.
        /// </summary>
        private void GenerateHealth()
        {
            var newHealth = healthStack.Any() ? healthStack.Pop() as Health : new Health();

            newHealth.SetAttributes(HealthSpeed + random.NextDouble());
            newHealth.AddToGameEnvironment(top: 0 - newHealth.Height, left: random.Next(10, (int)windowWidth - 100), gameEnvironment: GameView);
        }

        #endregion

        #region PowerUp Methods

        /// <summary>
        /// Spawns a PowerUp.
        /// </summary>
        private void SpawnPowerUp()
        {
            // each frame progress decreases this counter
            powerUpCounter -= 1;

            // when counter reaches zero, create a PowerUp
            if (powerUpCounter < 0)
            {
                GeneratePowerUp();
                powerUpCounter = PowerUpSpawnLimit;
            }

        }

        /// <summary>
        /// Generates a random PowerUp.
        /// </summary>
        private void GeneratePowerUp()
        {
            var newPowerUp = powerUpStack.Any() ? powerUpStack.Pop() as PowerUp : new PowerUp();

            newPowerUp.SetAttributes(PowerUpSpeed + random.NextDouble());
            newPowerUp.AddToGameEnvironment(top: 0 - newPowerUp.Height, left: random.Next(10, (int)windowWidth - 100), gameEnvironment: GameView);
        }

        /// <summary>
        /// Triggers the powered up state.
        /// </summary>
        private void TriggerPowerUp()
        {
            PowerUpTriggered = true;
            powerUpTriggerCounter = PowerUpTriggerLimit;

            LaserSpawnLimit -= 1;
            ShowInGameText("POWER UP!");

            PlaySound(SoundType.POWER_UP);
            Player.TriggerPowerUp();
        }

        /// <summary>
        /// Triggers the powered up state down.
        /// </summary>
        private void TriggerPowerDown()
        {
            if (PowerUpTriggered)
            {
                powerUpTriggerCounter -= 1;

                var powerGauge = (double)(powerUpTriggerCounter / 100) + 1;

                Player.SetPowerGauge(powerGauge);

                if (powerUpTriggerCounter <= 0)
                {
                    PowerUpTriggered = false;
                    LaserSpawnLimit += 1;

                    PlaySound(SoundType.POWER_DOWN);
                    Player.TriggerPowerDown();
                }
            }
        }

        #endregion

        #region Star Methods

        /// <summary>
        /// Spawns random stars in the star view.
        /// </summary>
        private void SpawnStar()
        {
            // each frame progress decreases this counter
            starCounter -= 1;

            // when counter reaches zero, create an star
            if (starCounter < 0)
            {
                GenerateStar();
                starCounter = StarSpawnLimit;
            }
        }

        /// <summary>
        /// Generates a random star.
        /// </summary>
        private void GenerateStar()
        {
            var newStar = starStack.Any() ? starStack.Pop() as Star : new Star();

            newStar.SetAttributes(StarSpeed);
            newStar.AddToGameEnvironment(top: 0 - newStar.Height, left: random.Next(10, (int)windowWidth - 10), gameEnvironment: StarView);
        }

        /// <summary>
        /// Updates stars on the star canvas.
        /// </summary>
        private void UpdateStarView()
        {
            var starObjects = StarView.GetGameObjects<GameObject>();

            foreach (var star in starObjects)
            {
                UpdateStarViewObject(star);
            }

            ClearStarView();
        }

        /// <summary>
        /// Updates the star objects. Moves the stars.
        /// </summary>
        /// <param name="star"></param>
        private void UpdateStarViewObject(GameObject star)
        {
            // move star down
            star.MoveY(StarSpeed);

            if (star.GetY() > windowHeight)
                StarView.AddDestroyableGameObject(star);
        }

        /// <summary>
        /// Clears destroyable stars from the star view.
        /// </summary>
        private void ClearStarView()
        {
            foreach (var star in StarView.GetDestroyableGameObjects())
            {
                StarView.RemoveGameObject(star);
                starStack.Push(star);
            }

            StarView.ClearDestroyableGameObjects();
        }

        #endregion

        #region Canvas Events

        private void GameCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(GameView);

            PointerX = currentPoint.Position.X;
        }

        #endregion

        #region Focus Events

        private void FocusBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Left)
            {
                moveLeft = true;
            }

            if (e.Key == Windows.System.VirtualKey.Right)
            {
                moveRight = true;
            }
        }

        private void FocusBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Left)
            {
                moveLeft = false;
            }

            if (e.Key == Windows.System.VirtualKey.Right)
            {
                moveRight = false;
            }
        }

        #endregion

        #region Window Events

        /// <summary>
        /// When the window is loaded, we add the event Current_SizeChanged.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            var indexUrl = Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.href;");
            var appPackage = Environment.GetEnvironmentVariable("UNO_BOOTSTRAP_APP_BASE");

            this.SizeChanged += GamePage_SizeChanged;
            baseUrl = $"{indexUrl}{appPackage}";

            Console.WriteLine(baseUrl);

            StartGame();
        }

        /// <summary>
        /// When the window size is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GamePage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            windowWidth = Window.Current.Bounds.Width;
            windowHeight = Window.Current.Bounds.Height;

            Console.WriteLine($"{windowWidth} x {windowHeight}");

            SetViewSizes();
            SetPlayerY();
        }

        /// <summary>
        /// When the window is unloaded, we remove the event Current_SizeChanged.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GamePage_Unloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= GamePage_SizeChanged;
            StopGame();
        }

        #endregion

        #region Audio Methods

        /// <summary>
        /// Plays the laser sound efect.
        /// </summary>
        private void PlaySound(SoundType soundType)
        {
            switch (soundType)
            {
                case SoundType.BACKGROUND_MUSIC:
                    {
                        var musicTrack = random.Next(1, 12);

                        string host = null;

                        switch (musicTrack)
                        {
                            case 1: { host = $"{baseUrl}/Assets/Sounds/slow-trap-18565.mp3"; } break;
                            case 2: { host = $"{baseUrl}/Assets/Sounds/space-chillout-14194.mp3"; } break;
                            case 3: { host = $"{baseUrl}/Assets/Sounds/cinematic-space-drone-10623.mp3"; } break;
                            case 4: { host = $"{baseUrl}/Assets/Sounds/slow-thoughtful-sad-piano-114586.mp3"; } break;
                            case 5: { host = $"{baseUrl}/Assets/Sounds/space-age-10714.mp3"; } break;
                            case 6: { host = $"{baseUrl}/Assets/Sounds/drone-space-main-9706.mp3"; } break;
                            case 7: { host = $"{baseUrl}/Assets/Sounds/cyberpunk-2099-10701.mp3"; } break;
                            case 8: { host = $"{baseUrl}/Assets/Sounds/insurrection-10941.mp3"; } break;
                            case 9: { host = $"{baseUrl}/Assets/Sounds/space-trip-114102.mp3"; } break;
                            case 10: { host = $"{baseUrl}/Assets/Sounds/dark-matter-10710.mp3"; } break;
                            case 11: { host = $"{baseUrl}/Assets/Sounds/music-807dfe09ce23793891674eb022b38c1b.mp3"; } break;
                            default:
                                break;
                        }

                        this.ExecuteJavascript($"playSound('{host}',0.4);");
                    }
                    break;
                case SoundType.LASER_FIRE:
                    {
                        var host = $"{baseUrl}/Assets/Sounds/shoot02wav-14562.mp3";
                        this.ExecuteJavascript($"playSound('{host}',0.4);");
                    }
                    break;
                case SoundType.LASER_HIT:
                    {
                        var host = $"{baseUrl}/Assets/Sounds/explosion-sfx-43814.mp3";
                        this.ExecuteJavascript($"playSound('{host}',0.6);");
                    }
                    break;
                case SoundType.LASER_FIRE_POWERED_UP:
                    {
                        var host = $"{baseUrl}/Assets/Sounds/plasmablaster-37114.mp3";
                        this.ExecuteJavascript($"playSound('{host}', 1);");
                    }
                    break;
                case SoundType.POWER_UP:
                    {
                        var host = $"{baseUrl}/Assets/Sounds/spellcast-46164.mp3";
                        this.ExecuteJavascript($"playSound('{host}', 1);");
                    }
                    break;
                case SoundType.POWER_DOWN:
                    {
                        var host = $"{baseUrl}/Assets/Sounds/power-down-7103.mp3";
                        this.ExecuteJavascript($"playSound('{host}', 1);");
                    }
                    break;
                case SoundType.ENEMY_DESTRUCTION:
                case SoundType.METEOR_DESTRUCTION:
                    {
                        var host = $"{baseUrl}/Assets/Sounds/explosion-36210.mp3";
                        this.ExecuteJavascript($"playSound('{host}', 0.8);");
                    }
                    break;
                case SoundType.LEVEL_UP:
                    {
                        var host = $"{baseUrl}/Assets/Sounds/8-bit-powerup-6768.mp3";
                        this.ExecuteJavascript($"playSound('{host}', 1.0);");
                    }
                    break;
                case SoundType.HEALTH_GAIN:
                    {
                        var host = $"{baseUrl}/Assets/Sounds/scale-e6-14577.mp3";
                        this.ExecuteJavascript($"playSound('{host}', 1.0);");
                    }
                    break;
                case SoundType.HEALTH_LOSS:
                    {
                        var host = $"{baseUrl}/Assets/Sounds/explosion-39897.mp3";
                        this.ExecuteJavascript($"playSound('{host}', 1.0);");
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Stops the background music.
        /// </summary>
        private void StopBackgroundMusic()
        {
            //if (backgroundAudio is not null)
            //{
            //    AudioService.PauseAudio(backgroundAudio);
            //}
        }

        #endregion

        #endregion
    }
}
