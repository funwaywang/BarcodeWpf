using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
using Zen.Barcode;

namespace BarcodeWpf
{
    public class BarcodeBlock : Control
    {
        ImageSource BarcodeImage;
        string ErrorMessage;

        public static readonly DependencyProperty CodeProperty = DependencyProperty.Register("Code", typeof(string), typeof(BarcodeBlock));
        public static readonly DependencyProperty SymbologyProperty = DependencyProperty.Register("Symbology", typeof(BarcodeSymbology), typeof(BarcodeBlock),
            new PropertyMetadata(BarcodeSymbology.Code128));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(int), typeof(BarcodeBlock),
            new PropertyMetadata(3));
        public static readonly DependencyProperty ErrorForegroundProperty = DependencyProperty.Register("ErrorForeground", typeof(Brush), typeof(BarcodeBlock),
            new PropertyMetadata(Brushes.Red));
        
        public string Code
        {
            get { return (string)GetValue(CodeProperty); }
            set { SetValue(CodeProperty, value); }
        }

        public BarcodeSymbology Symbology
        {
            get { return (BarcodeSymbology)GetValue(SymbologyProperty); }
            set { SetValue(SymbologyProperty, value); }
        }

        public int Scale
        {
            get { return (int)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public Brush ErrorForeground
        {
            get { return (Brush)GetValue(ErrorForegroundProperty); }
            set { SetValue(ErrorForegroundProperty, value); }
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            
            if(!string.IsNullOrEmpty(Code) && BarcodeImage != null)
            {
                var contentWidth = Math.Min(BarcodeImage.Width, ActualWidth);
                var contentHeight = Math.Min(BarcodeImage.Height, ActualHeight);
                if(Symbology == BarcodeSymbology.CodeQr)
                {
                    contentWidth = contentHeight = Math.Min(contentWidth, contentHeight);
                }

                var rect = new Rect(0, 0, ActualWidth, ActualHeight);
                rect.X += (rect.Width - contentWidth) / 2;
                rect.Y += (rect.Height - contentHeight) / 2;
                rect.Width = contentWidth;
                rect.Height = contentHeight;
                drawingContext.DrawImage(BarcodeImage, rect);
            }

            if(!string.IsNullOrEmpty(ErrorMessage))
            {
                var formatedText = new FormattedText(ErrorMessage, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Brushes.Red);
                drawingContext.DrawText(formatedText, new Point((ActualWidth - formatedText.Width) / 2, (ActualHeight - formatedText.Height) / 2));
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if(e.Property == BackgroundProperty
                || e.Property == ForegroundProperty
                || e.Property == CodeProperty
                || e.Property == SymbologyProperty
                || e.Property == ScaleProperty)
            {
                RefreshImage();
            }
        }
        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            RefreshImage();
        }

        void RefreshImage()
        {
            if (Symbology == BarcodeSymbology.Unknown || string.IsNullOrEmpty(Code) || ActualHeight <= 0)
            {
                ErrorMessage = null;
                BarcodeImage = null;
            }
            else
            {
                var bc = BarcodeDrawFactory.GetSymbology(Symbology);
                bc.Background = Background;
                bc.Foreground = Foreground;

                try
                {
                    ErrorMessage = null;
                    BarcodeImage = bc.DrawImage(Code, Math.Max(1, (int)ActualHeight), Math.Max(1, Scale));
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    BarcodeImage = null;
                }
            }
            InvalidateVisual();
        }
    }
}
