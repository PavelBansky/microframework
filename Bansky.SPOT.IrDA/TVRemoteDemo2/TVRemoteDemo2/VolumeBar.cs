using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;

namespace TVRemoteDemo2
{
    public class VolumeBar : UIElement
    {
        public VolumeBar()
        {
            _blockPen = new Pen(Colors.Black);
            _fillBrush = new SolidColorBrush(ColorUtility.ColorFromRGB(0, 127, 70));            
        }

        public override void OnRender(DrawingContext dc)
        {            
            // Draw the bars
            for(int i=0; i < Value; i++)
            {
                dc.DrawRectangle(_fillBrush, _blockPen, (i*10)+10, 5, 5, 40);
            }

            base.OnRender(dc);
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            desiredHeight = Height;
            desiredWidth = Width;
        }

        public new int Height
        {
            get { return 50; }
        }

        public new int Width
        {
            get { return SystemMetrics.ScreenWidth; }
        }
        
        Pen _blockPen;
        Brush _fillBrush;        

        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (_value < 0) _value = 0;
                if (_value > 20) _value = 20;
                this.Invalidate();
            }
        }
    }
}
