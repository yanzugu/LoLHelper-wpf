using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LoLHelper.Src.UserControllers
{
    /// <summary>
    /// CustomToggleButton.xaml 的互動邏輯
    /// </summary>
    public partial class SwitchToggleButton : UserControl, INotifyPropertyChanged
    {
        private bool isCircleColorHasValue = false;

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(SwitchToggleButton), new PropertyMetadata(0.7));



        public double CircleSize => Scale * 30;
        public double BorderHeight => Scale * 40;
        public double BorderWidth => Scale * 80;
        public double CornorRadius => Scale * 20;
        public double LabelFontSize => Scale * 15;
        public Thickness BorderPadding => new Thickness(Scale * 7, 0, Scale * 7, 0);

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set
            {
                CircleHorizontalAlignment = value ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                BorderBackground = value ? OnColor : OffColor;
                LabelColor = CalculateForegroundColor(BorderBackground);

                if (isCircleColorHasValue == false)
                {
                    CircleColor = CalculateForegroundColor(BorderBackground);
                }

                if (value)
                {
                    Checked?.Invoke();
                }
                else
                {
                    UnChecked?.Invoke();
                }

                SetValue(IsCheckedProperty, value);
            }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(SwitchToggleButton), new PropertyMetadata(false));



        public Brush OnColor
        {
            get { return (Brush)GetValue(OnColorProperty); }
            set { SetValue(OnColorProperty, value); }
        }

        public static readonly DependencyProperty OnColorProperty =
            DependencyProperty.Register("OnColor", typeof(Brush), typeof(SwitchToggleButton), new PropertyMetadata(Brushes.Green));



        public Brush OffColor
        {
            get { return (Brush)GetValue(OffColorProperty); }
            set { SetValue(OffColorProperty, value); }
        }

        public static readonly DependencyProperty OffColorProperty =
            DependencyProperty.Register("OffColor", typeof(Brush), typeof(SwitchToggleButton), new PropertyMetadata(Brushes.Red));



        public Brush CircleColor
        {
            get { return (Brush)GetValue(CircleColorProperty); }
            set { SetValue(CircleColorProperty, value); }
        }

        public static readonly DependencyProperty CircleColorProperty =
            DependencyProperty.Register("CircleColor", typeof(Brush), typeof(SwitchToggleButton), new PropertyMetadata(null));



        public bool ShowStatusLabel
        {
            get { return (bool)GetValue(ShowStatusLabelProperty); }
            set { SetValue(ShowStatusLabelProperty, value); }
        }

        public static readonly DependencyProperty ShowStatusLabelProperty =
            DependencyProperty.Register("ShowStatusLabel", typeof(bool), typeof(SwitchToggleButton), new PropertyMetadata(true));



        public Action Checked
        {
            get { return (Action)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Checked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedProperty =
            DependencyProperty.Register("Checked", typeof(Action), typeof(SwitchToggleButton), new PropertyMetadata(null));




        public Action UnChecked
        {
            get { return (Action)GetValue(UnCheckedProperty); }
            set { SetValue(UnCheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnCheckedProperty =
            DependencyProperty.Register("UnChecked", typeof(Action), typeof(SwitchToggleButton), new PropertyMetadata(null));





        private HorizontalAlignment _circleHorizontalAlignment;
        public HorizontalAlignment CircleHorizontalAlignment
        {
            get { return _circleHorizontalAlignment; }
            set
            {
                _circleHorizontalAlignment = value;
                OnPropertyChanged();
            }
        }

        public Brush _labelColor = Brushes.Red;
        public Brush LabelColor
        {
            get { return _labelColor; }
            set
            {
                _labelColor = value;
                OnPropertyChanged();
            }
        }

        public Brush _borderBackground = Brushes.Transparent;
        public Brush BorderBackground
        {
            get { return _borderBackground; }
            set
            {
                _borderBackground = value;
                OnPropertyChanged();
            }
        }


        public SwitchToggleButton()
        {
            InitializeComponent();

            DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CircleHorizontalAlignment = IsChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            BorderBackground = IsChecked ? OnColor : OffColor;
            LabelColor = CalculateForegroundColor(BorderBackground);

            if (CircleColor != null)
            {
                isCircleColorHasValue = true;
            }
            else
            {
                CircleColor = CalculateForegroundColor(BorderBackground);
            }
        }

        private void border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
        }

        private Brush CalculateForegroundColor(Brush background)
        {
            Color bgColor = ((SolidColorBrush)background).Color;

            double value = 0.2126 * bgColor.ScR + 0.7152 * bgColor.ScG + 0.0722 * bgColor.ScB;

            return value < 0.5 ? Brushes.AliceBlue : Brushes.Gray;
        }
    }
}
