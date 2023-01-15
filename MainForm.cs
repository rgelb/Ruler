using Ruler.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Ruler
{
	public sealed class MainForm : Form
	{
		private ToolTip _toolTip = new ToolTip();

		private Point _offset;

		private Rectangle _mouseDownRect;

		private int _resizeBorderWidth = 5;

		private Point _mouseDownPoint;

		private MainForm.ResizeRegion _resizeRegion;

		private System.Windows.Forms.ContextMenu _menu = new System.Windows.Forms.ContextMenu();

		private MenuItem _verticalMenuItem;

		private MenuItem _toolTipMenuItem;

		private bool IsVertical
		{
			get
			{
				return this._verticalMenuItem.Checked;
			}
			set
			{
				this._verticalMenuItem.Checked = value;
			}
		}

		private bool ShowToolTip
		{
			get
			{
				return this._toolTipMenuItem.Checked;
			}
			set
			{
				this._toolTipMenuItem.Checked = value;
				if (value)
				{
					this.SetToolTip();
				}
			}
		}

		private bool _onAppearProcessed = false;

		public MainForm()
		{
			base.SetStyle(ControlStyles.ResizeRedraw, true);
			base.UpdateStyles();
			ResourceManager resourceManager = new ResourceManager(typeof(MainForm));
			base.Icon = (System.Drawing.Icon)resourceManager.GetObject("$this.Icon");
			this.SetUpMenu();
			this.Text = "Ruler";
			this.BackColor = Color.White;
			base.ClientSize = new System.Drawing.Size(400, 75);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.Opacity = 0.65;
			this.ContextMenu = this._menu;
			this.Font = new System.Drawing.Font("Tahoma", 10f);
			base.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
		}

		private MenuItem AddMenuItem(string text)
		{
			return this.AddMenuItem(text, Shortcut.None);
		}

		private MenuItem AddMenuItem(string text, Shortcut shortcut)
		{
			MenuItem menuItem = new MenuItem(text);
			menuItem.Click += new EventHandler(this.MenuHandler);
			menuItem.Shortcut = shortcut;
			this._menu.MenuItems.Add(menuItem);
			return menuItem;
		}

		private void ChangeOrientation()
		{
			this.IsVertical = !this.IsVertical;
			int width = base.Width;
			base.Width = base.Height;
			base.Height = width;
		}

		private void DrawRuler(Graphics g, int formWidth, int formHeight)
		{
			int num;
			g.DrawRectangle(Pens.Black, 0, 0, formWidth - 1, formHeight - 1);
			g.DrawString(string.Concat(formWidth, " pixels"), this.Font, Brushes.Black, 10f, (float)(formHeight / 2 - this.Font.Height / 2));
			for (int i = 0; i < formWidth; i++)
			{
				if (i % 2 == 0)
				{
					if (i % 100 != 0)
					{
						num = (i % 10 != 0 ? 5 : 10);
					}
					else
					{
						num = 15;
						this.DrawTickLabel(g, i.ToString(), i, formHeight, num);
					}
					MainForm.DrawTick(g, i, formHeight, num);
				}
			}
		}

		private static void DrawTick(Graphics g, int xPos, int formHeight, int tickHeight)
		{
			g.DrawLine(Pens.Black, xPos, 0, xPos, tickHeight);
			g.DrawLine(Pens.Black, xPos, formHeight, xPos, formHeight - tickHeight);
		}

		private void DrawTickLabel(Graphics g, string text, int xPos, int formHeight, int height)
		{
			g.DrawString(text, this.Font, Brushes.Black, (float)xPos, (float)height);
			g.DrawString(text, this.Font, Brushes.Black, (float)xPos, (float)(formHeight - height - this.Font.Height));
		}

		private MainForm.ResizeRegion GetResizeRegion(Point clientCursorPos)
		{
			if (clientCursorPos.Y <= this._resizeBorderWidth)
			{
				if (clientCursorPos.X <= this._resizeBorderWidth)
				{
					return MainForm.ResizeRegion.NW;
				}
				if (clientCursorPos.X >= base.Width - this._resizeBorderWidth)
				{
					return MainForm.ResizeRegion.NE;
				}
				return MainForm.ResizeRegion.N;
			}
			if (clientCursorPos.Y < base.Height - this._resizeBorderWidth)
			{
				if (clientCursorPos.X <= this._resizeBorderWidth)
				{
					return MainForm.ResizeRegion.W;
				}
				return MainForm.ResizeRegion.E;
			}
			if (clientCursorPos.X <= this._resizeBorderWidth)
			{
				return MainForm.ResizeRegion.SW;
			}
			if (clientCursorPos.X >= base.Width - this._resizeBorderWidth)
			{
				return MainForm.ResizeRegion.SE;
			}
			return MainForm.ResizeRegion.S;
		}

		private void HandleMoveResizeKeystroke(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Right)
			{
				if (!e.Control)
				{
					MainForm left = this;
					left.Left = left.Left + 5;
					return;
				}
				if (e.Shift)
				{
					MainForm width = this;
					width.Width = width.Width + 1;
					return;
				}
				MainForm mainForm = this;
				mainForm.Left = mainForm.Left + 1;
				return;
			}
			if (e.KeyCode == Keys.Left)
			{
				if (!e.Control)
				{
					MainForm left1 = this;
					left1.Left = left1.Left - 5;
					return;
				}
				if (e.Shift)
				{
					MainForm width1 = this;
					width1.Width = width1.Width - 1;
					return;
				}
				MainForm mainForm1 = this;
				mainForm1.Left = mainForm1.Left - 1;
				return;
			}
			if (e.KeyCode == Keys.Up)
			{
				if (!e.Control)
				{
					MainForm top = this;
					top.Top = top.Top - 5;
					return;
				}
				if (e.Shift)
				{
					MainForm height = this;
					height.Height = height.Height - 1;
					return;
				}
				MainForm top1 = this;
				top1.Top = top1.Top - 1;
				return;
			}
			if (e.KeyCode == Keys.Down)
			{
				if (e.Control)
				{
					if (e.Shift)
					{
						MainForm height1 = this;
						height1.Height = height1.Height + 1;
						return;
					}
					MainForm top2 = this;
					top2.Top = top2.Top + 1;
					return;
				}
				MainForm mainForm2 = this;
				mainForm2.Top = mainForm2.Top + 5;
			}
		}

		private void HandleResize()
		{
			switch (this._resizeRegion)
			{
				case MainForm.ResizeRegion.E:
				{
					int x = Control.MousePosition.X - this._mouseDownPoint.X;
					base.Width = this._mouseDownRect.Width + x;
					return;
				}
				case MainForm.ResizeRegion.SE:
				{
					int width = this._mouseDownRect.Width;
					Point mousePosition = Control.MousePosition;
					base.Width = width + mousePosition.X - this._mouseDownPoint.X;
					int height = this._mouseDownRect.Height;
					Point point = Control.MousePosition;
					base.Height = height + point.Y - this._mouseDownPoint.Y;
					return;
				}
				case MainForm.ResizeRegion.S:
				{
					int y = Control.MousePosition.Y - this._mouseDownPoint.Y;
					base.Height = this._mouseDownRect.Height + y;
					return;
				}
				default:
				{
					return;
				}
			}
		}

		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

		private void MenuHandler(object sender, EventArgs e)
		{
			MenuItem @checked = (MenuItem)sender;
			string text = @checked.Text;
			string str = text;
			if (text != null)
			{
				if (str == "Exit")
				{
					base.Close();
					return;
				}
				if (str == "Tool Tip")
				{
					this.ShowToolTip = !this.ShowToolTip;
					return;
				}
				if (str == "Vertical")
				{
					this.ChangeOrientation();
					return;
				}
				if (str == "Stay On Top")
				{
					@checked.Checked = !@checked.Checked;
					base.TopMost = @checked.Checked;
					return;
				}
				if (str == "About")
				{
					string str1 = string.Format("Ruler v{0}. Original code by Jeff Key.\nAmended by Robert Gelb.\nIcon by Kristen Magee @ www.kbecca.com", Application.ProductVersion);
					MessageBox.Show(str1, "About Ruler", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					return;
				}
			}
			MessageBox.Show("Unknown menu item.");
		}

		protected override void OnActivated(EventArgs e) {
			// we only want to execute this code once
            if (_onAppearProcessed) return; 			

            if (Settings.Default.Vertical) {
                this.ChangeOrientation();
            }

            if (Settings.Default.StayOnTop) {
                SetMenuCheckedStatus("Stay On Top", true);
                base.TopMost = true;
            }
			
            _onAppearProcessed = true;

            base.OnActivated(e);
		}

        protected override void OnFormClosing(FormClosingEventArgs e) {

			// save settings
			Properties.Settings.Default.StayOnTop = GetMenuCheckedStatus("Stay On Top");
            Properties.Settings.Default.Vertical = GetMenuCheckedStatus("Vertical");
			Properties.Settings.Default.Save();

			base.OnFormClosing(e);

			Debug.Write("OnFormClosing");
        }

		private bool GetMenuCheckedStatus(string menuText) {
			foreach (MenuItem item in _menu.MenuItems) {
				if (item.Text == menuText) { return item.Checked; }
			}

			return false;
		}

		private void SetMenuCheckedStatus(string menuText, bool value) {
            foreach (MenuItem item in _menu.MenuItems) {
                if (item.Text == menuText) { 
					item.Checked = value; 
					return;
				}
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Space:
				{
					this.ChangeOrientation();
					base.OnKeyDown(e);
					return;
				}
				case Keys.Prior:
				case Keys.Next:
				case Keys.End:
				case Keys.Home:
				{
					base.OnKeyDown(e);
					return;
				}
				case Keys.Left:
				case Keys.Up:
				case Keys.Right:
				case Keys.Down:
				{
					this.HandleMoveResizeKeystroke(e);
					base.OnKeyDown(e);
					return;
				}
				default:
				{
					base.OnKeyDown(e);
					return;
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			int x = Control.MousePosition.X - base.Location.X;
			int y = Control.MousePosition.Y;
			Point location = base.Location;
			this._offset = new Point(x, y - location.Y);
			this._mouseDownPoint = Control.MousePosition;
			this._mouseDownRect = base.ClientRectangle;
			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (this._resizeRegion != MainForm.ResizeRegion.None)
			{
				this.HandleResize();
				return;
			}
			Point client = base.PointToClient(Control.MousePosition);
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Inflate(-this._resizeBorderWidth, -this._resizeBorderWidth);
			if ((!base.ClientRectangle.Contains(client) ? true : clientRectangle.Contains(client)))
			{
				this.Cursor = Cursors.Default;
				if (e.Button == System.Windows.Forms.MouseButtons.Left)
				{
					int x = Control.MousePosition.X - this._offset.X;
					Point mousePosition = Control.MousePosition;
					base.Location = new Point(x, mousePosition.Y - this._offset.Y);
				}
			}
			else
			{
				MainForm.ResizeRegion resizeRegion = this.GetResizeRegion(client);
				this.SetResizeCursor(resizeRegion);
				if (e.Button == System.Windows.Forms.MouseButtons.Left)
				{
					this._resizeRegion = resizeRegion;
					this.HandleResize();
				}
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			this._resizeRegion = MainForm.ResizeRegion.None;
			base.OnMouseUp(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;
			int height = base.Height;
			int width = base.Width;
			if (this.IsVertical)
			{
				graphics.RotateTransform(90f);
				graphics.TranslateTransform(0f, (float)(-base.Width + 1));
				height = base.Width;
				width = base.Height;
			}
			this.DrawRuler(graphics, width, height);
			base.OnPaint(e);
		}

		protected override void OnResize(EventArgs e)
		{
			if (this.ShowToolTip)
			{
				this.SetToolTip();
			}
			base.OnResize(e);
		}

		private void OpacityMenuHandler(object sender, EventArgs e)
		{
			MenuItem menuItem = (MenuItem)sender;
			base.Opacity = double.Parse(menuItem.Text.Replace("%", "")) / 100;
		}

		private void SetResizeCursor(MainForm.ResizeRegion region)
		{
			switch (region)
			{
				case MainForm.ResizeRegion.N:
				case MainForm.ResizeRegion.S:
				{
					this.Cursor = Cursors.SizeNS;
					return;
				}
				case MainForm.ResizeRegion.NE:
				case MainForm.ResizeRegion.SW:
				{
					this.Cursor = Cursors.SizeNESW;
					return;
				}
				case MainForm.ResizeRegion.E:
				case MainForm.ResizeRegion.W:
				{
					this.Cursor = Cursors.SizeWE;
					return;
				}
				case MainForm.ResizeRegion.SE:
				case MainForm.ResizeRegion.NW:
				{
					this.Cursor = Cursors.SizeNWSE;
					return;
				}
				default:
				{
					this.Cursor = Cursors.SizeNESW;
					return;
				}
			}
		}

		private void SetToolTip()
		{
			this._toolTip.SetToolTip(this, string.Format("Width: {0} pixels\nHeight: {1} pixels", base.Width, base.Height));
		}

		private void SetUpMenu()
		{
			this.AddMenuItem("Stay On Top");
			this._verticalMenuItem = this.AddMenuItem("Vertical");
			this._toolTipMenuItem = this.AddMenuItem("Tool Tip");
			MenuItem menuItem = this.AddMenuItem("Opacity");
			this.AddMenuItem("-");
			this.AddMenuItem("About");
			this.AddMenuItem("-");
			this.AddMenuItem("Exit");
			for (int i = 10; i <= 100; i += 10)
			{
				MenuItem menuItem1 = new MenuItem(string.Concat(i, "%"));
				menuItem1.Click += new EventHandler(this.OpacityMenuHandler);
				menuItem.MenuItems.Add(menuItem1);
			}
		}

		private enum ResizeRegion
		{
			None,
			N,
			NE,
			E,
			SE,
			S,
			SW,
			W,
			NW
		}
	}
}