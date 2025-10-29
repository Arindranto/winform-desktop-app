using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinformDesktopApp
{
    public partial class CircularProgressBar : UserControl
    {
        private int _value = 0;
        private Color _barColor = Color.Blue;
        private int _penWidth = 10;
        private bool _isIndeterminate = false;
        private float _rotationAngle = 0f; // Stores the current position of the arc

        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set { _isIndeterminate = value; this.Invalidate(); }
        }

        // Add a Timer control to manage the animation frame rate
        private System.Windows.Forms.Timer animationTimer;

        public int Value
        {
            get { return _value; }
            set
            {
                _value = Math.Max(0, Math.Min(100, value)); // Clamp value between 0 and 100
                this.Invalidate(); // Force the control to redraw
            }
        }

        public Color BarColor
        {
            get { return _barColor; }
            set { _barColor = value; this.Invalidate(); }
        }

        public int PenWidth
        {
            get { return _penWidth; }
            set { _penWidth = value; this.Invalidate(); }
        }
        public CircularProgressBar()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // Reduces flickering
                                        // Setup the animation timer
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 15; // Fast interval for smooth animation (milliseconds)
            animationTimer.Tick += new EventHandler(AnimationTimer_Tick);

            // Start the timer by default, or only when IsIndeterminate is set to true
            // It's often easier to manage the timer externally, but for simplicity, we'll start it here.
            animationTimer.Start();
        }
        // CircularProgressBar.cs

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (_isIndeterminate)
            {
                // Increment the angle by a small amount each tick
                _rotationAngle = (_rotationAngle + 10) % 360;

                // Force the control to call OnPaint() and redraw the arc at the new angle
                this.Invalidate();
            }
        }

        // Helper method to manage the timer start/stop based on the state
        public void StartIndeterminate()
        {
            this.IsIndeterminate = true;
            if (!animationTimer.Enabled)
            {
                animationTimer.Start();
            }
        }

        public void StopIndeterminate()
        {
            this.IsIndeterminate = false;
            // You may stop the timer here, or just let it run if you switch between states often
            // animationTimer.Stop(); 
        }

        // CircularProgressBar.cs

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int diameter = Math.Min(this.Width, this.Height) - _penWidth;
            Rectangle rect = new Rectangle(_penWidth / 2, _penWidth / 2, diameter, diameter);

            // 1. Draw the background track (full circle outline)
            using (Pen backgroundPen = new Pen(Color.LightGray, _penWidth))
            {
                e.Graphics.DrawEllipse(backgroundPen, rect);
            }

            if (_isIndeterminate)
            {
                // Indeterminate State: Draw a short, rotating arc segment
                float startAngle = _rotationAngle;
                float sweepAngle = 45f; // The length of the visible arc segment

                using (Pen progressPen = new Pen(_barColor, _penWidth))
                {
                    // The arc segment starts at the rotation angle and sweeps 45 degrees
                    // We start the rotation angle at 270 (top)
                    e.Graphics.DrawArc(progressPen, rect, 270 + startAngle, sweepAngle);
                }
            }
            else // Determinate State (Previous Logic)
            {
                // Determinate State: Draw a fixed arc based on the Value property
                float sweepAngle = (float)_value * 3.6f;
                using (Pen progressPen = new Pen(_barColor, _penWidth))
                {
                    e.Graphics.DrawArc(progressPen, rect, 270, sweepAngle);
                }

                // Draw the value percentage text (optional)
                // ... (Text drawing logic here)
            }

            base.OnPaint(e);
        }
    }
}
