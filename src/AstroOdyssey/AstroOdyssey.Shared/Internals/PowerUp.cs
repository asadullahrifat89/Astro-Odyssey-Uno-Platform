using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AstroOdyssey
{
    public class PowerUp : GameObject
    {
        #region Fields

        private readonly Image content = new Image() { Stretch = Stretch.Uniform };

        private readonly Random random = new Random();

        #endregion

        #region Ctor

        public PowerUp()
        {
            Tag = Constants.POWERUP;
            Height = Constants.DESTRUCTIBLE_OBJECT_SIZE;
            Width = Constants.DESTRUCTIBLE_OBJECT_SIZE;
            Child = content;

            YDirection = YDirection.DOWN;
            PowerUpType = PowerUpType.NONE;
        }

        #endregion

        #region Properties

        public PowerUpType PowerUpType { get; set; }

        #endregion

        #region Methods

        public void SetAttributes(double speed, double scale = 1)
        {
            Speed = speed;
            PowerUpType = (PowerUpType)random.Next(1, Enum.GetNames<PowerUpType>().Length);

            var uri = new Uri("ms-appx:///Assets/Images/icon_exclamationSmall.png", UriKind.RelativeOrAbsolute);
            Health = 10;

            content.Source = new BitmapImage(uri);

            Height = Constants.DESTRUCTIBLE_OBJECT_SIZE * scale;
            Width = Constants.DESTRUCTIBLE_OBJECT_SIZE * scale;

            HalfWidth = Width / 2;
        }

        #endregion
    }

    public enum PowerUpType
    {
        NONE,
        RAPID_SHOT_ROUNDS,
        DEAD_SHOT_ROUNDS,
        DOOM_SHOT_ROUNDS,
    }
}
