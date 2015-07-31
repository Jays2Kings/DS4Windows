using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScpServer
{
    [System.ComponentModel.DesignerCategory("")]
    public class AdvancedComboBox : ComboBox
    {
        public Label Label { get; set; }

        public AdvancedComboBox()
        {
            base.Visible = false;
            Label = new Label();
            Label.Tag = this;
            Label.ForeColor = Color.Blue;
            Label.TextAlign = ContentAlignment.MiddleCenter;
            Label.BackColor = Color.Transparent;
            Label.MouseDown += Label_MouseDown;
        }

        public Color Color
        {
            get
            {
                return Label.ForeColor;
            }
            set 
            {
                Label.ForeColor = value;
            }
        }

        public new bool Visible 
        {
            get 
            {
                return Label.Visible;
            }
            set 
            {
                Label.Visible = value;
            }
        }
        
        private void Label_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                DroppedDown = true;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent != null)
            {
                Label.Location = this.Location;
                Label.Size = this.Size;
                Label.Dock = this.Dock;
                Label.Anchor = this.Anchor;
                Label.Enabled = this.Enabled;
                Label.Visible = this.Visible;
                Label.RightToLeft = this.RightToLeft;
                Label.Font = this.Font;
                Label.Text = this.Text
                    .Replace("Right Click", "Right-Click")
                    .Replace(" Button", string.Empty)
                    .Replace("Left ", string.Empty)
                    .Replace("Right ", string.Empty);
                Label.TabStop = this.TabStop;
                Label.TabIndex = this.TabIndex;
            }
            Label.Parent = this.Parent;
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);

            Label.Text = this.Text
                .Replace("Right Click", "Right-Click")
                .Replace(" Button", string.Empty)
                .Replace("Left ", string.Empty)
                .Replace("Right ", string.Empty);
        }

    }
}
