using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BankSystem.Controller;
using BankSystem.Model.Entities;

namespace BankSystem.View
{
    public partial class ClientDetailsForm : Form, IObserver
    {
        private Bank _bank;
        private BankController _controller;

        // Элементы управления
        private DataGridView dgvClients;
        private TabControl tabControl;
        private TabPage tabInfo;
        private TabPage tabActions;
        private Panel panelClientInfo;
        private Label lblClientInfo;
        private DataGridView dgvAccounts;
        private ComboBox cmbAccounts;
        private NumericUpDown nudAmount;
        private Button btnDeposit;
        private Button btnWithdraw;
        private Button btnOpenDeposit;
        private Button btnTakeCredit;
        private Button btnRepayCredit;
        private Label lblAmount;

        public ClientDetailsForm(Bank bank, BankController controller)
        {
            _bank = bank;
            _controller = controller;
            _bank.Attach(this);

            InitializeComponent();
            LoadClients();
        }

        private void InitializeComponent()
        {
            this.Text = $"Клиенты банка: {_bank.Name}";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 500);

            // ===== DataGridView для списка клиентов =====
            dgvClients = new DataGridView
            {
                Dock = DockStyle.Left,
                Width = 350,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White
            };
            dgvClients.SelectionChanged += DgvClients_SelectionChanged;

            // ===== TabControl =====
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // === Вкладка "Информация" ===
            tabInfo = new TabPage("Информация");

            panelClientInfo = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            lblClientInfo = new Label
            {
                Dock = DockStyle.Top,
                Height = 120,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                BackColor = Color.LightYellow,
                Padding = new Padding(5),
                Text = "Выберите клиента из списка слева"
            };

            dgvAccounts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            panelClientInfo.Controls.Add(dgvAccounts);
            panelClientInfo.Controls.Add(lblClientInfo);
            tabInfo.Controls.Add(panelClientInfo);

            // === Вкладка "Действия" ===
            tabActions = new TabPage("Действия");

            var actionsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Выпадающий список счетов
            var lblSelectAccount = new Label
            {
                Text = "Выберите счёт:",
                Location = new Point(20, 20),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            cmbAccounts = new ComboBox
            {
                Location = new Point(20, 50),
                Size = new Size(250, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };

            // Поле для суммы
            lblAmount = new Label
            {
                Text = "Сумма:",
                Location = new Point(20, 100),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            nudAmount = new NumericUpDown
            {
                Location = new Point(20, 130),
                Size = new Size(250, 30),
                Minimum = 1,
                Maximum = 1000000,
                DecimalPlaces = 2,
                ThousandsSeparator = true,
                Font = new Font("Segoe UI", 10)
            };

            // Кнопки операций
            btnDeposit = new Button
            {
                Text = "💰 Внести",
                Location = new Point(20, 180),
                Size = new Size(250, 40),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnDeposit.Click += BtnDeposit_Click;

            btnWithdraw = new Button
            {
                Text = "💸 Снять",
                Location = new Point(20, 230),
                Size = new Size(250, 40),
                BackColor = Color.LightCoral,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnWithdraw.Click += BtnWithdraw_Click;

            btnOpenDeposit = new Button
            {
                Text = "🏦 Открыть депозит",
                Location = new Point(20, 290),
                Size = new Size(250, 40),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnOpenDeposit.Click += BtnOpenDeposit_Click;

            btnTakeCredit = new Button
            {
                Text = "📋 Взять кредит",
                Location = new Point(20, 340),
                Size = new Size(250, 40),
                BackColor = Color.LightYellow,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnTakeCredit.Click += BtnTakeCredit_Click;

            btnRepayCredit = new Button
            {
                Text = "✅ Погасить кредит",
                Location = new Point(20, 390),
                Size = new Size(250, 40),
                BackColor = Color.LightSteelBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnRepayCredit.Click += BtnRepayCredit_Click;

            actionsPanel.Controls.AddRange(new Control[]
            {
                lblSelectAccount, cmbAccounts, lblAmount, nudAmount,
                btnDeposit, btnWithdraw, btnOpenDeposit, btnTakeCredit, btnRepayCredit
            });

            tabActions.Controls.Add(actionsPanel);

            tabControl.TabPages.Add(tabInfo);
            tabControl.TabPages.Add(tabActions);

            // ===== Добавляем всё на форму =====
            this.Controls.Add(tabControl);
            this.Controls.Add(dgvClients);
        }

        private void LoadClients()
        {
            dgvClients.Columns.Clear();
            dgvClients.Columns.Add("Name", "ФИО / Название");
            dgvClients.Columns.Add("Type", "Тип");
            dgvClients.Columns.Add("Created", "Дата создания");
            dgvClients.Columns.Add("AccountsCount", "Кол-во счетов");
            dgvClients.Columns.Add("Deposited", "Внесено");
            dgvClients.Columns.Add("Limit", "Лимит");

            dgvClients.Columns["Name"].Width = 120;
            dgvClients.Columns["Type"].Width = 80;
            dgvClients.Columns["Created"].Width = 80;
            dgvClients.Columns["AccountsCount"].Width = 60;
            dgvClients.Columns["Deposited"].Width = 80;
            dgvClients.Columns["Limit"].Width = 80;

            foreach (var client in _bank.Clients)
            {
                dgvClients.Rows.Add(
                    client.GetFullName(),
                    client.GetTypeName(),
                    client.CreatedDate.ToShortDateString(),
                    client.Accounts.Count,
                    client.TotalDeposited,
                    client.TotalDepositedLimit
                );
            }
        }

        private void DgvClients_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count > 0)
            {
                var clientName = dgvClients.SelectedRows[0].Cells[0].Value.ToString();
                var client = _bank.Clients.FirstOrDefault(c => c.GetFullName() == clientName);
                if (client != null)
                {
                    UpdateClientInfo(client);
                }
            }
        }

        private void UpdateClientInfo(Client client)
        {
            // Обновляем информационную панель
            lblClientInfo.Text = $"📌 {client.GetFullName()}\n" +
                $"📋 Тип: {client.GetTypeName()}\n" +
                $"📅 Дата создания: {client.CreatedDate:dd.MM.yyyy HH:mm}\n" +
                $"💰 Лимит внесения: {client.TotalDepositedLimit:N2} руб.\n" +
                $"💵 Внесено всего: {client.TotalDeposited:N2} руб.\n" +
                $"📊 Остаток лимита: {(client.TotalDepositedLimit - client.TotalDeposited):N2} руб.";

            // Обновляем таблицу счетов
            dgvAccounts.Columns.Clear();
            dgvAccounts.Columns.Add("Type", "Тип счёта");
            dgvAccounts.Columns.Add("Id", "ID счёта");
            dgvAccounts.Columns.Add("Balance", "Баланс");

            dgvAccounts.Columns["Type"].Width = 120;
            dgvAccounts.Columns["Id"].Width = 200;
            dgvAccounts.Columns["Balance"].Width = 150;

            cmbAccounts.Items.Clear();

            foreach (var account in client.Accounts)
            {
                string accountType = account.AccountType;
                if (account is CreditAccount credit)
                {
                    accountType = $"Кредитный (долг: {credit.RemainingDebt:N2})";
                }

                dgvAccounts.Rows.Add(accountType, account.Id.ToString().Substring(0, 8), $"{account.Balance:N2} руб.");
                cmbAccounts.Items.Add($"{account.AccountType} - {account.Balance:N2} руб.");
            }

            if (cmbAccounts.Items.Count > 0)
                cmbAccounts.SelectedIndex = 0;
        }

        private Client GetSelectedClient()
        {
            if (dgvClients.SelectedRows.Count == 0) return null;
            var clientName = dgvClients.SelectedRows[0].Cells[0].Value.ToString();
            return _bank.Clients.FirstOrDefault(c => c.GetFullName() == clientName);
        }

        private Account GetSelectedAccount()
        {
            var client = GetSelectedClient();
            if (client == null || cmbAccounts.SelectedIndex < 0) return null;
            if (cmbAccounts.SelectedIndex < client.Accounts.Count)
                return client.Accounts[cmbAccounts.SelectedIndex];
            return null;
        }

        private void BtnDeposit_Click(object sender, EventArgs e)
        {
            var client = GetSelectedClient();
            var account = GetSelectedAccount();
            if (client != null && account != null)
            {
                _controller.DepositToAccount(account, nudAmount.Value, client);
                RefreshData();
            }
        }

        private void BtnWithdraw_Click(object sender, EventArgs e)
        {
            var client = GetSelectedClient();
            var account = GetSelectedAccount();
            if (client != null && account != null)
            {
                _controller.WithdrawFromAccount(account, nudAmount.Value, client);
                RefreshData();
            }
        }

        private void BtnOpenDeposit_Click(object sender, EventArgs e)
        {
            var client = GetSelectedClient();
            if (client != null)
            {
                var deposit = new DepositAccount(0);
                client.AddAccount(deposit);
                _controller.DepositToAccount(deposit, nudAmount.Value, client);
                RefreshData();
            }
        }

        private void BtnTakeCredit_Click(object sender, EventArgs e)
        {
            var client = GetSelectedClient();
            if (client != null)
            {
                _controller.TakeCredit(client, nudAmount.Value, _bank);
                RefreshData();
            }
        }

        private void BtnRepayCredit_Click(object sender, EventArgs e)
        {
            var client = GetSelectedClient();
            var account = GetSelectedAccount();
            if (client != null && account is CreditAccount credit)
            {
                _controller.RepayCredit(credit, nudAmount.Value, client);
                RefreshData();
            }
            else if (account != null && !(account is CreditAccount))
            {
                MessageBox.Show("Выбранный счёт не является кредитным!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshData()
        {
            LoadClients();
            if (dgvClients.SelectedRows.Count > 0)
            {
                DgvClients_SelectionChanged(null, null);
            }
            Refresh();
        }

        public void Update(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => RefreshData()));
            }
            else
            {
                RefreshData();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _bank.Detach(this);
            base.OnFormClosing(e);
        }
    }
}