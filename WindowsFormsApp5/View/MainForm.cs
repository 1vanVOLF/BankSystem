using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BankSystem.Controller;
using BankSystem.Model;
using BankSystem.Model.Entities;
using BankSystem.View.Controls;

namespace BankSystem.View
{
    public partial class MainForm : Form, IObserver
    {
        private BankSystemModel _model;
        private BankController _controller;

        // Элементы управления
        private ComboBox cmbBanks;
        private Button btnCreateBank;
        private Button btnViewData;
        private FlowLayoutPanel animationPanel;
        private ListBox lstLog;
        private SplitContainer mainSplitter;
        private SplitContainer bottomSplitter;
        private Panel leftPanel;

        private Timer autoTimer;
        private int autoTick = 0;

        public MainForm()
        {
            InitializeComponent();
            InitializeModel();
            StartDemo();
        }

        private void InitializeComponent()
        {
            this.Text = "🏦 Банковская система";
            this.Size = new Size(1400, 900);
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.WhiteSmoke;

            // Главный сплиттер (левая панель + правая часть)
            mainSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };

            // Левая панель с управлением
            leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.LightSteelBlue
            };

            var lblTitle = new Label
            {
                Text = "🏦 УПРАВЛЕНИЕ БАНКАМИ",
                Location = new Point(10, 10),
                Size = new Size(220, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            cmbBanks = new ComboBox
            {
                Location = new Point(10, 50),
                Width = 220,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };

            btnCreateBank = new Button
            {
                Text = "➕ Создать банк",
                Location = new Point(10, 95),
                Width = 220,
                Height = 40,
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnCreateBank.Click += BtnCreateBank_Click;

            btnViewData = new Button
            {
                Text = "📊 Просмотреть данные",
                Location = new Point(10, 145),
                Width = 220,
                Height = 40,
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnViewData.Click += BtnViewData_Click;

            leftPanel.Controls.AddRange(new Control[] { lblTitle, cmbBanks, btnCreateBank, btnViewData });

            // Нижний сплиттер (анимация + лог)
            bottomSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            // Панель анимации с прокруткой
            animationPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(5),
                BackColor = Color.White
            };

            // Лог событий
            lstLog = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                DrawMode = DrawMode.OwnerDrawFixed
            };
            lstLog.DrawItem += LstLog_DrawItem;

            bottomSplitter.Panel1.Controls.Add(animationPanel);
            bottomSplitter.Panel2.Controls.Add(lstLog);
            bottomSplitter.SplitterDistance = (int)(this.Height * 0.7);
            bottomSplitter.SplitterWidth = 5;

            mainSplitter.Panel1.Controls.Add(leftPanel);
            mainSplitter.Panel2.Controls.Add(bottomSplitter);
            mainSplitter.SplitterDistance = 260;
            mainSplitter.SplitterWidth = 5;

            this.Controls.Add(mainSplitter);
        }

        private void LstLog_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();
            var logItem = lstLog.Items[e.Index].ToString();

            // Разный цвет для разных типов сообщений
            if (logItem.Contains("ОШИБКА") || logItem.Contains("Error"))
                e.Graphics.DrawString(logItem, lstLog.Font, Brushes.Red, e.Bounds);
            else if (logItem.Contains("===="))
                e.Graphics.DrawString(logItem, new Font(lstLog.Font, FontStyle.Bold), Brushes.Yellow, e.Bounds);
            else if (logItem.Contains("кредит"))
                e.Graphics.DrawString(logItem, lstLog.Font, Brushes.Orange, e.Bounds);
            else
                e.Graphics.DrawString(logItem, lstLog.Font, Brushes.LightGreen, e.Bounds);

            e.DrawFocusRectangle();
        }

        private void InitializeModel()
        {
            _model = new BankSystemModel();
            _controller = new BankController(_model);
            _model.AddLog("═══════════════════════════════════════");
            _model.AddLog("🏦 БАНКОВСКАЯ СИСТЕМА ЗАПУЩЕНА");
            _model.AddLog("═══════════════════════════════════════");
        }

        private void StartDemo()
        {
            // Создаём 2 банка с демо-клиентами
            var bank1 = _controller.CreateBank("Альфа-Банк");
            var bank2 = _controller.CreateBank("Бета-Банк");

            bank1.Attach(this);
            bank2.Attach(this);

            // Физ лицо в первом банке
            var individual = _controller.CreateIndividual("Иван", "Петров", "Сергеевич",
                "1234567890", 100000, bank1);
            individual.Attach(this);

            // Юр лицо во втором банке
            var legalEntity = _controller.CreateLegalEntity("Рога и Копыта", "1234567890",
                "г. Москва, ул. Примерная, д. 1", 500000, bank2);
            legalEntity.Attach(this);

            UpdateBanksList();
            RebuildAnimationPanel();

            // Запускаем автоматические операции
            StartAutoOperations();

            _model.AddLog("✅ Демо-режим запущен. Автоматические операции каждые 7 секунд.");
        }

        private void RebuildAnimationPanel()
        {
            animationPanel.Controls.Clear();

            foreach (var bank in _model.GetBanks())
            {
                var rowControl = new BankRowControl(bank, _controller);
                rowControl.Width = animationPanel.Width - 25;
                rowControl.UpdateClientNames();
                animationPanel.Controls.Add(rowControl);
            }
        }

        private void UpdateBanksList()
        {
            cmbBanks.Items.Clear();
            foreach (var bank in _model.GetBanks())
            {
                cmbBanks.Items.Add(bank.Name);
            }
            if (cmbBanks.Items.Count > 0) cmbBanks.SelectedIndex = 0;
        }

        private void BtnCreateBank_Click(object sender, EventArgs e)
        {
            var dialog = new InputDialog("Введите название банка:", "Создание банка");
            if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                var bank = _controller.CreateBank(dialog.InputText);
                bank.Attach(this);
                UpdateBanksList();
                RebuildAnimationPanel();
                _model.AddLog($"✨ Создан новый банк: {bank.Name}");
            }
        }

        private void BtnViewData_Click(object sender, EventArgs e)
        {
            if (cmbBanks.SelectedItem != null)
            {
                var bankName = cmbBanks.SelectedItem.ToString();
                var bank = _model.GetBanks().FirstOrDefault(b => b.Name == bankName);
                if (bank != null)
                {
                    var detailsForm = new ClientDetailsForm(bank, _controller);
                    detailsForm.ShowDialog();
                    RebuildAnimationPanel();
                }
            }
        }

        private void StartAutoOperations()
        {
            autoTimer = new Timer { Interval = 7000 };
            autoTimer.Tick += (s, e) =>
            {
                autoTick++;

                if (autoTick == 1)
                {
                    _model.AddLog("");
                    _model.AddLog("═══════════════════════════════════════");
                    _model.AddLog("🔄 АВТООПЕРАЦИЯ: Начисление процентов");
                    _model.AddLog("═══════════════════════════════════════");

                    foreach (var bank in _model.GetBanks())
                    {
                        foreach (var client in bank.Clients)
                        {
                            var rowControl = FindBankRowControl(bank);

                            foreach (var account in client.Accounts)
                            {
                                if (account is DepositAccount deposit)
                                {
                                    deposit.ApplyInterest();
                                    _model.AddLog($"[{bank.Name}] {client.GetFullName()}: начислены % по депозиту");

                                    if (rowControl != null)
                                    {
                                        if (client is Individual)
                                            rowControl.AnimateIndividual(client.GetFullName(), "💰 Начисление %");
                                        else
                                            rowControl.AnimateCorporate(client.GetFullName(), "💰 Начисление %");
                                    }
                                }
                                else if (account is CreditAccount credit)
                                {
                                    credit.MakeMonthlyPayment();
                                    _model.AddLog($"[{bank.Name}] {client.GetFullName()}: ежемесячный платёж по кредиту");

                                    if (rowControl != null)
                                    {
                                        if (client is Individual)
                                            rowControl.AnimateIndividual(client.GetFullName(), "💳 Платёж по кредиту");
                                        else
                                            rowControl.AnimateCorporate(client.GetFullName(), "💳 Платёж по кредиту");
                                    }
                                }
                            }
                        }
                    }
                }
                else if (autoTick == 2)
                {
                    _model.AddLog("");
                    _model.AddLog("═══════════════════════════════════════");
                    _model.AddLog("🔄 АВТООПЕРАЦИЯ: Пополнение депозитов (+1%)");
                    _model.AddLog("═══════════════════════════════════════");

                    foreach (var bank in _model.GetBanks())
                    {
                        foreach (var client in bank.Clients)
                        {
                            var rowControl = FindBankRowControl(bank);
                            var amount = client.TotalDepositedLimit * 0.01m;

                            foreach (var account in client.Accounts)
                            {
                                if (account is DepositAccount deposit)
                                {
                                    _controller.DepositToAccount(deposit, amount, client);

                                    if (rowControl != null)
                                    {
                                        if (client is Individual)
                                            rowControl.AnimateIndividual(client.GetFullName(), $"📈 Пополнение +{amount:N0}₽");
                                        else
                                            rowControl.AnimateCorporate(client.GetFullName(), $"📈 Пополнение +{amount:N0}₽");
                                    }
                                }
                            }
                        }
                    }
                }
                else if (autoTick == 3)
                {
                    _model.AddLog("");
                    _model.AddLog("═══════════════════════════════════════");
                    _model.AddLog("🔄 АВТООПЕРАЦИЯ: Снятие с депозитов (-2%)");
                    _model.AddLog("═══════════════════════════════════════");

                    foreach (var bank in _model.GetBanks())
                    {
                        foreach (var client in bank.Clients)
                        {
                            var rowControl = FindBankRowControl(bank);
                            var amount = client.TotalDepositedLimit * 0.02m;

                            foreach (var account in client.Accounts)
                            {
                                if (account is DepositAccount deposit && deposit.Balance >= amount)
                                {
                                    _controller.WithdrawFromAccount(deposit, amount, client);

                                    if (rowControl != null)
                                    {
                                        if (client is Individual)
                                            rowControl.AnimateIndividual(client.GetFullName(), $"📉 Снятие -{amount:N0}₽");
                                        else
                                            rowControl.AnimateCorporate(client.GetFullName(), $"📉 Снятие -{amount:N0}₽");
                                    }
                                }
                            }
                        }
                    }
                    autoTick = 0;
                }
            };

            autoTimer.Start();
        }

        private BankRowControl FindBankRowControl(Bank bank)
        {
            foreach (BankRowControl control in animationPanel.Controls)
            {
                if (control.Bank == bank)
                    return control;
            }
            return null;
        }

        public void Update(string message)
        {
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action(() =>
                {
                    lstLog.Items.Insert(0, message);
                    if (lstLog.Items.Count > 500)
                        lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
                }));
            }
            else
            {
                lstLog.Items.Insert(0, message);
                if (lstLog.Items.Count > 500)
                    lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Адаптируем ширину контролов при изменении размера окна
            if (animationPanel != null)
            {
                foreach (BankRowControl control in animationPanel.Controls)
                {
                    control.Width = animationPanel.Width - 25;
                }
            }
        }
    }

    // Диалог для ввода текста
    public class InputDialog : Form
    {
        public string InputText { get; private set; }

        public InputDialog(string prompt, string title)
        {
            this.Text = title;
            this.Size = new Size(450, 160);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblPrompt = new Label
            {
                Text = prompt,
                Location = new Point(15, 20),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10)
            };

            var txtInput = new TextBox
            {
                Location = new Point(15, 50),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10)
            };

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(270, 90),
                Size = new Size(70, 30),
                DialogResult = DialogResult.OK,
                BackColor = Color.LightGreen
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(345, 90),
                Size = new Size(70, 30),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.LightCoral
            };

            btnOk.Click += (s, e) => { InputText = txtInput.Text; };

            this.Controls.AddRange(new Control[] { lblPrompt, txtInput, btnOk, btnCancel });
        }
    }
}