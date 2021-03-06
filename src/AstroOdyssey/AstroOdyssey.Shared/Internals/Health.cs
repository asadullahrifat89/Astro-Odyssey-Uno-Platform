using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AstroOdyssey
{
    public class Health : GameObject
    {
        #region Fields
        
        private readonly Image content = new Image() { Stretch = Stretch.Uniform };

        #endregion

        #region Ctor

        public Health()
        {
            Tag = Constants.HEALTH;
            Height = Constants.DESTRUCTIBLE_OBJECT_SIZE;
            Width = Constants.DESTRUCTIBLE_OBJECT_SIZE;
            Child = content;
            YDirection = YDirection.DOWN;
        } 

        #endregion

        #region Methods

        public void SetAttributes(double speed, double scale = 1)
        {
            Speed = speed;

            var uri = new Uri("ms-appx:///Assets/Images/icon_plusSmall.png", UriKind.RelativeOrAbsolute);
            Health = 10;

            content.Source = new BitmapImage(uri);

            Height = Constants.DESTRUCTIBLE_OBJECT_SIZE * scale;
            Width = Constants.DESTRUCTIBLE_OBJECT_SIZE * scale;

            HalfWidth = Width / 2;
        } 

        #endregion
    }
}
