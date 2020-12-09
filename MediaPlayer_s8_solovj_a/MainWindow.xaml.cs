using System;

using System.Windows;

using System.Windows.Controls;

using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
namespace AudioVideo
{
    public partial class MediaPlayer_s8_solovj_a : Window
    {
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private bool fullscreenMode = false;
        private bool reverseTime = false; // lblProgressStatus karogs
        private TimeSpan timeSpan = TimeSpan.FromSeconds(5);

        public MediaPlayer_s8_solovj_a()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliderProgress.Minimum = 0;
                sliderProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;

                sliderProgress.Value = mePlayer.Position.TotalSeconds;

            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg;*.mp4)|*.mp3;*.mpg;*.mpeg;*.mp4|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                mePlayer.Source = new Uri(openFileDialog.FileName);
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Play();
            mediaPlayerIsPlaying = true;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Pause();
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }
        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Stop();
            mediaPlayerIsPlaying = false;
        }

        private void sliderProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliderProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.05 : -0.05;
        }

        private void lblProgressStatus_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            reverseTime = !reverseTime;
            timeUpdate();
        }


        private void timeUpdate()
        {
            if (!reverseTime)
                lblProgressStatus.Text = TimeSpan.FromSeconds(sliderProgress.Value).ToString(@"hh\:mm\:ss");

            else
                lblProgressStatus.Text = "-"
                    + TimeSpan.FromSeconds(mePlayer.NaturalDuration.TimeSpan.TotalSeconds
                    - mePlayer.Position.TotalSeconds).ToString(@"hh\:mm\:ss");
        }


        private void lblProgressStatus_MouseLeave(object sender, MouseEventArgs e)
        {
            // tad kad pele vairs nav virsuu lblProgressStatus teksta blokam
        }

        #region ScaleValue Depdency Property
        public static readonly DependencyProperty ScaleValueProperty
            = DependencyProperty.Register("ScaleValue",
                typeof(double),
                typeof(MediaPlayer_s8_solovj_a),
                new UIPropertyMetadata(1.0,
                    new PropertyChangedCallback(OnScaleValueChanged),
                    new CoerceValueCallback(OnCoerceScaleValue)));

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            MediaPlayer_s8_solovj_a mainWindow = o as MediaPlayer_s8_solovj_a;

            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);

            else return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MediaPlayer_s8_solovj_a mainWindow = o as MediaPlayer_s8_solovj_a;

            if (mainWindow != null)
                mainWindow.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual double OnCoerceScaleValue(double value)
        {
            if (double.IsNaN(value))
                return 1.0f;

            value = Math.Max(0.1, value);
            return value;
        }

        protected virtual void OnScaleValueChanged(double oldValue, double newValue)
        {

        }

        public double ScaleValue
        {
            get
            {
                return (double)GetValue(ScaleValueProperty);
            }

            set
            {
                SetValue(ScaleValueProperty, value);
            }
        }

        private void MainGrid_SizeChanged(object sender, EventArgs e)
        {
            CalculateScale();
        }

        private void CalculateScale()
        {
            double yScale = ActualHeight / 400f; // galvena loga izmers
            double xScale = ActualWidth / 600f;
            double value = Math.Min(xScale, yScale);
            ScaleValue = (double)OnCoerceScaleValue(myMainWindow, value);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mePlayer.StretchDirection = StretchDirection.UpOnly;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged += Window_SizeChanged;
        }
        #endregion

        private void Backwards_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerIsPlaying)
            {
                mePlayer.Position -= timeSpan;
            }
        }

        private void Forwards_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerIsPlaying)
            {
                mePlayer.Position += timeSpan;
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerIsPlaying)
            {
                mePlayer.Stop();
                mePlayer.Play();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (!fullscreenMode)
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
                fullscreenMode = true;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                fullscreenMode = false;
            }
        }

        private void sliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mePlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
            timeUpdate();
        }

        private void mePlayer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mediaPlayerIsPlaying)
            {
                mePlayer.Pause();
                mediaPlayerIsPlaying = false;
            }
            else
            {
                mePlayer.Play();
                mediaPlayerIsPlaying = true;
            }
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (mediaPlayerIsPlaying)
                {
                    mePlayer.Pause();
                    mediaPlayerIsPlaying = false;
                }
                else
                {
                    mePlayer.Play();
                    mediaPlayerIsPlaying = true;
                }
            }

            if (e.Key == Key.Escape)
            {
                if (fullscreenMode)
                {
                    this.WindowState = WindowState.Normal;
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    fullscreenMode = false;
                }
            }

        }
    }

}