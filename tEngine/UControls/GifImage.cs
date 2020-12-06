using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace tEngine.UControls {
    public class GifImage : Image {
        private Int32Animation mAnimation;
        private GifBitmapDecoder mGifDecoder;
        private bool mIsInitialized;

        /// <summary>
        /// Defines whether the animation starts on it's own
        /// </summary>
        public bool AutoStart {
            get { return (bool) GetValue( AutoStartProperty ); }
            set { SetValue( AutoStartProperty, value ); }
        }

        public int FrameIndex {
            get { return (int) GetValue( FrameIndexProperty ); }
            set { SetValue( FrameIndexProperty, value ); }
        }

        public string GifSource {
            get { return (string) GetValue( GifSourceProperty ); }
            set { SetValue( GifSourceProperty, value ); }
        }

        /// <summary>
        /// Starts the animation
        /// </summary>
        public void StartAnimation() {
            if( !mIsInitialized )
                this.Initialize();

            BeginAnimation( FrameIndexProperty, mAnimation );
        }

        /// <summary>
        /// Stops the animation
        /// </summary>
        public void StopAnimation() {
            BeginAnimation( FrameIndexProperty, null );
        }

        static GifImage() {
            VisibilityProperty.OverrideMetadata( typeof( GifImage ),
                new FrameworkPropertyMetadata( VisibilityPropertyChanged ) );
        }

        private static void AutoStartPropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e ) {
            if( (bool) e.NewValue )
                (sender as GifImage).StartAnimation();
        }

        private static void ChangingFrameIndex( DependencyObject obj, DependencyPropertyChangedEventArgs ev ) {
            var gifImage = obj as GifImage;
            gifImage.Source = gifImage.mGifDecoder.Frames[(int) ev.NewValue];
        }

        private static void GifSourcePropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e ) {
            (sender as GifImage).Initialize();
        }

        private void Initialize() {
            //todo ругается в дизайнере, не критично
            mGifDecoder = new GifBitmapDecoder( new Uri( @"pack://application:,,," + this.GifSource ),
                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default );
            mAnimation = new Int32Animation( 0, mGifDecoder.Frames.Count - 1,
                new Duration( new TimeSpan( 0, 0, 0, mGifDecoder.Frames.Count/10,
                    (int) ((mGifDecoder.Frames.Count/10.0 - mGifDecoder.Frames.Count/10)*1000) ) ) );
            mAnimation.RepeatBehavior = RepeatBehavior.Forever;
            this.Source = mGifDecoder.Frames[0];

            mIsInitialized = true;
        }

        private static void VisibilityPropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e ) {
            if( (Visibility) e.NewValue == Visibility.Visible ) {
                ((GifImage) sender).StartAnimation();
            } else {
                ((GifImage) sender).StopAnimation();
            }
        }

        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register( "FrameIndex", typeof( int ), typeof( GifImage ),
                new UIPropertyMetadata( 0, new PropertyChangedCallback( ChangingFrameIndex ) ) );

        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register( "AutoStart", typeof( bool ), typeof( GifImage ),
                new UIPropertyMetadata( false, AutoStartPropertyChanged ) );

        public static readonly DependencyProperty GifSourceProperty =
            DependencyProperty.Register( "GifSource", typeof( string ), typeof( GifImage ),
                new UIPropertyMetadata( string.Empty, GifSourcePropertyChanged ) );
    }
}