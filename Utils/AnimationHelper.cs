using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace SceneTodo.Utils
{
    /// <summary>
    /// Animation helper utilities
    /// </summary>
    public static class AnimationHelper
    {
        /// <summary>
        /// Default animation duration
        /// </summary>
        public static TimeSpan DefaultDuration => TimeSpan.FromMilliseconds(300);

        /// <summary>
        /// Fast animation duration
        /// </summary>
        public static TimeSpan FastDuration => TimeSpan.FromMilliseconds(150);

        /// <summary>
        /// Slow animation duration
        /// </summary>
        public static TimeSpan SlowDuration => TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Default easing function
        /// </summary>
        public static IEasingFunction DefaultEasing => new CubicEase { EasingMode = EasingMode.EaseOut };

        /// <summary>
        /// Fade in animation
        /// </summary>
        public static void FadeIn(UIElement element, TimeSpan? duration = null, EventHandler? completed = null)
        {
            if (element == null) return;

            var animation = new DoubleAnimation
            {
                From = element.Opacity,
                To = 1.0,
                Duration = duration ?? DefaultDuration,
                EasingFunction = DefaultEasing
            };

            if (completed != null)
                animation.Completed += completed;

            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        /// <summary>
        /// Fade out animation
        /// </summary>
        public static void FadeOut(UIElement element, TimeSpan? duration = null, EventHandler? completed = null)
        {
            if (element == null) return;

            var animation = new DoubleAnimation
            {
                From = element.Opacity,
                To = 0.0,
                Duration = duration ?? DefaultDuration,
                EasingFunction = DefaultEasing
            };

            if (completed != null)
                animation.Completed += completed;

            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        /// <summary>
        /// Slide in from left animation
        /// </summary>
        public static void SlideInFromLeft(FrameworkElement element, TimeSpan? duration = null, EventHandler? completed = null)
        {
            if (element == null) return;

            var animation = new ThicknessAnimation
            {
                From = new Thickness(-element.ActualWidth, 0, element.ActualWidth, 0),
                To = new Thickness(0),
                Duration = duration ?? DefaultDuration,
                EasingFunction = DefaultEasing
            };

            if (completed != null)
                animation.Completed += completed;

            element.BeginAnimation(FrameworkElement.MarginProperty, animation);
        }

        /// <summary>
        /// Slide in from right animation
        /// </summary>
        public static void SlideInFromRight(FrameworkElement element, TimeSpan? duration = null, EventHandler? completed = null)
        {
            if (element == null) return;

            var animation = new ThicknessAnimation
            {
                From = new Thickness(element.ActualWidth, 0, -element.ActualWidth, 0),
                To = new Thickness(0),
                Duration = duration ?? DefaultDuration,
                EasingFunction = DefaultEasing
            };

            if (completed != null)
                animation.Completed += completed;

            element.BeginAnimation(FrameworkElement.MarginProperty, animation);
        }

        /// <summary>
        /// Scale animation (zoom in/out)
        /// </summary>
        public static void Scale(FrameworkElement element, double fromScale, double toScale, TimeSpan? duration = null, EventHandler? completed = null)
        {
            if (element == null) return;

            element.RenderTransformOrigin = new Point(0.5, 0.5);
            var scaleTransform = new System.Windows.Media.ScaleTransform(fromScale, fromScale);
            element.RenderTransform = scaleTransform;

            var animationX = new DoubleAnimation
            {
                From = fromScale,
                To = toScale,
                Duration = duration ?? DefaultDuration,
                EasingFunction = DefaultEasing
            };

            var animationY = new DoubleAnimation
            {
                From = fromScale,
                To = toScale,
                Duration = duration ?? DefaultDuration,
                EasingFunction = DefaultEasing
            };

            if (completed != null)
                animationX.Completed += completed;

            scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, animationX);
            scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, animationY);
        }

        /// <summary>
        /// Bounce animation
        /// </summary>
        public static void Bounce(FrameworkElement element, double amplitude = 10, TimeSpan? duration = null, EventHandler? completed = null)
        {
            if (element == null) return;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = -amplitude,
                Duration = duration ?? FastDuration,
                AutoReverse = true,
                EasingFunction = new BounceEase { Bounces = 2, EasingMode = EasingMode.EaseOut }
            };

            if (completed != null)
                animation.Completed += completed;

            var transform = new System.Windows.Media.TranslateTransform();
            element.RenderTransform = transform;
            transform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, animation);
        }

        /// <summary>
        /// Shake animation (for errors)
        /// </summary>
        public static void Shake(FrameworkElement element, double amplitude = 5, TimeSpan? duration = null, EventHandler? completed = null)
        {
            if (element == null) return;

            var animation = new DoubleAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(-amplitude, KeyTime.FromPercent(0.2)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(amplitude, KeyTime.FromPercent(0.4)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(-amplitude, KeyTime.FromPercent(0.6)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(amplitude, KeyTime.FromPercent(0.8)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(1)));
            animation.Duration = duration ?? FastDuration;

            if (completed != null)
                animation.Completed += completed;

            var transform = new System.Windows.Media.TranslateTransform();
            element.RenderTransform = transform;
            transform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, animation);
        }

        /// <summary>
        /// Highlight animation (flash background)
        /// </summary>
        public static void Highlight(FrameworkElement element, System.Windows.Media.Color highlightColor, TimeSpan? duration = null, EventHandler? completed = null)
        {
            if (element == null) return;

            var originalBrush = element.GetValue(System.Windows.Controls.Control.BackgroundProperty) as System.Windows.Media.SolidColorBrush;
            var originalColor = originalBrush?.Color ?? System.Windows.Media.Colors.Transparent;

            var colorAnimation = new ColorAnimation
            {
                From = highlightColor,
                To = originalColor,
                Duration = duration ?? DefaultDuration,
                EasingFunction = DefaultEasing
            };

            if (completed != null)
                colorAnimation.Completed += completed;

            var brush = new System.Windows.Media.SolidColorBrush(originalColor);
            element.SetValue(System.Windows.Controls.Control.BackgroundProperty, brush);
            brush.BeginAnimation(System.Windows.Media.SolidColorBrush.ColorProperty, colorAnimation);
        }
    }
}
