using System;
using System.Drawing;
using System.Windows.Forms;
using BankSystem.Controller;
using BankSystem.Model.Entities;

namespace BankSystem.View.Controls
{
    public class BankRowControl : Panel
    {
        private Bank _bank;
        private BankController _controller;

        private PictureBox _bankPic;
        private PictureBox _individualPic;
        private PictureBox _corporatePic;
        private Label _bankName;
        private Label _individualName;
        private Label _corporateName;
        private Label _individualAction;
        private Label _corporateAction;

        private Point _individualOriginalPos;
        private Point _corporateOriginalPos;
        private Point _bankPos;

        private bool _individualMovingToBank = false;
        private bool _individualMovingBack = false;
        private bool _corporateMovingToBank = false;
        private bool _corporateMovingBack = false;

        private Timer _animationTimer;

        public Bank Bank => _bank;

        public BankRowControl(Bank bank, BankController controller)
        {
            _bank = bank;
            _controller = controller;
            this.Size = new Size(800, 200);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.White;
            this.Margin = new Padding(5);

            CreateControls();
            StartAnimationTimer();
        }

        private void CreateControls()
        {
            // ===== БАНК (левая часть) =====
            _bankPic = new PictureBox
            {
                Size = new Size(100, 100),
                Location = new Point(30, 40),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.LightGray
            };

            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "bank.png");
                if (System.IO.File.Exists(path))
                    _bankPic.Image = Image.FromFile(path);
                else
                    _bankPic.BackColor = Color.LightBlue;
            }
            catch { _bankPic.BackColor = Color.LightBlue; }

            _bankName = new Label
            {
                Text = _bank.Name,
                Location = new Point(30, 145),
                Size = new Size(100, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightSteelBlue
            };

            // ===== КЛИЕНТЫ (правая часть) =====
            _individualPic = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(this.Width - 120, 20),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.LightGray,
                Cursor = Cursors.Hand
            };

            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "individual.png");
                if (System.IO.File.Exists(path))
                    _individualPic.Image = Image.FromFile(path);
                else
                    _individualPic.BackColor = Color.LightGreen;
            }
            catch { _individualPic.BackColor = Color.LightGreen; }

            _corporatePic = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(this.Width - 120, 110),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.LightGray,
                Cursor = Cursors.Hand
            };

            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "corporate.png");
                if (System.IO.File.Exists(path))
                    _corporatePic.Image = Image.FromFile(path);
                else
                    _corporatePic.BackColor = Color.LightCoral;
            }
            catch { _corporatePic.BackColor = Color.LightCoral; }

            _individualOriginalPos = _individualPic.Location;
            _corporateOriginalPos = _corporatePic.Location;
            _bankPos = new Point(140, 45);

            _individualName = new Label
            {
                Location = new Point(this.Width - 120, 100),
                Size = new Size(100, 40),
                Font = new Font("Arial", 7),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightYellow
            };

            _corporateName = new Label
            {
                Location = new Point(this.Width - 120, 190),
                Size = new Size(100, 40),
                Font = new Font("Arial", 7),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightYellow
            };

            _individualAction = new Label
            {
                Location = new Point(this.Width - 120, 0),
                Size = new Size(100, 20),
                Font = new Font("Arial", 7, FontStyle.Bold),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            _corporateAction = new Label
            {
                Location = new Point(this.Width - 120, 90),
                Size = new Size(100, 20),
                Font = new Font("Arial", 7, FontStyle.Bold),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            this.Controls.AddRange(new Control[] {
                _bankPic, _bankName,
                _individualPic, _corporatePic,
                _individualName, _corporateName,
                _individualAction, _corporateAction
            });
        }

        public void AnimateIndividual(string clientName, string action)
        {
            if (_individualMovingToBank || _individualMovingBack) return;
            _individualName.Text = clientName;
            _individualAction.Text = action;
            _individualMovingToBank = true;
        }

        public void AnimateCorporate(string clientName, string action)
        {
            if (_corporateMovingToBank || _corporateMovingBack) return;
            _corporateName.Text = clientName;
            _corporateAction.Text = action;
            _corporateMovingToBank = true;
        }

        public void UpdateClientNames()
        {
            foreach (var client in _bank.Clients)
            {
                if (client is Individual && string.IsNullOrEmpty(_individualName.Text))
                    _individualName.Text = client.GetFullName();
                else if (client is LegalEntity && string.IsNullOrEmpty(_corporateName.Text))
                    _corporateName.Text = client.GetFullName();
            }
        }

        private void StartAnimationTimer()
        {
            _animationTimer = new Timer { Interval = 16 };
            _animationTimer.Tick += (s, e) => Animate();
            _animationTimer.Start();
        }

        private void Animate()
        {
            // Анимация физ лица
            if (_individualMovingToBank)
            {
                int newX = _individualPic.Location.X - 12;
                if (newX <= _bankPos.X)
                {
                    _individualPic.Location = _bankPos;
                    _individualMovingToBank = false;
                    System.Threading.Tasks.Task.Delay(800).ContinueWith(_ => _individualMovingBack = true);
                }
                else
                {
                    _individualPic.Location = new Point(newX, _individualPic.Location.Y);
                }
            }
            else if (_individualMovingBack)
            {
                int newX = _individualPic.Location.X + 12;
                if (newX >= _individualOriginalPos.X)
                {
                    _individualPic.Location = _individualOriginalPos;
                    _individualMovingBack = false;
                    _individualAction.Text = "";
                }
                else
                {
                    _individualPic.Location = new Point(newX, _individualPic.Location.Y);
                }
            }

            // Анимация юр лица
            if (_corporateMovingToBank)
            {
                int newX = _corporatePic.Location.X - 12;
                if (newX <= _bankPos.X)
                {
                    _corporatePic.Location = _bankPos;
                    _corporateMovingToBank = false;
                    System.Threading.Tasks.Task.Delay(800).ContinueWith(_ => _corporateMovingBack = true);
                }
                else
                {
                    _corporatePic.Location = new Point(newX, _corporatePic.Location.Y);
                }
            }
            else if (_corporateMovingBack)
            {
                int newX = _corporatePic.Location.X + 12;
                if (newX >= _corporateOriginalPos.X)
                {
                    _corporatePic.Location = _corporateOriginalPos;
                    _corporateMovingBack = false;
                    _corporateAction.Text = "";
                }
                else
                {
                    _corporatePic.Location = new Point(newX, _corporatePic.Location.Y);
                }
            }
        }
    }
}