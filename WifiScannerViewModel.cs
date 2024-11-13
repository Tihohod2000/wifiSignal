using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Npgsql;

namespace wifiSignal
{
    public class WifiScannerViewModel : INotifyPropertyChanged
    {
        private string _wifiNetworksText;
        private ObservableCollection<WifiNetwork> _wifiNetworks;

        public string WifiNetworksText
        {
            get => _wifiNetworksText;
            set
            {
                _wifiNetworksText = value;
                OnPropertyChanged(nameof(WifiNetworksText));
            }
        }

        public ObservableCollection<WifiNetwork> WifiNetworks
        {
            get => _wifiNetworks;
            set
            {
                _wifiNetworks = value;
                OnPropertyChanged(nameof(WifiNetworks));
            }
        }

        public ICommand ScanWifiCommand { get; }
        public ICommand SaveWifiCommand { get; }

        public WifiScannerViewModel()
        {
            WifiNetworks = new ObservableCollection<WifiNetwork>();
            ScanWifiCommand = new RelayCommand(ScanWifiNetworks);
            SaveWifiCommand = new RelayCommand(SaveWifiNetworks);
        }

        private void ScanWifiNetworks()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "wlan show networks mode=bssid",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.GetEncoding("CP866")
                };

                using (Process process = Process.Start(startInfo))
                {
                    StringBuilder output = new StringBuilder();
                    ObservableCollection<WifiNetwork> networks = new ObservableCollection<WifiNetwork>();

                    using (var reader = process.StandardOutput)
                    {
                        string line;
                        string ssid = null;
                        string signalStrength = null;

                        while ((line = reader.ReadLine()) != null)
                        {
                            output.AppendLine(line);

                            // Ищем SSID сети
                            if (line.StartsWith("SSID"))
                            {
                                ssid = line.Substring(7).Trim(); // Получаем SSID из строки
                            }



                            if (line.TrimStart().StartsWith("Сигнал") || line.TrimStart().StartsWith("Signal"))
                            {
                                signalStrength = line.Substring(line.IndexOf(":") + 1).Trim().TrimEnd('%');

                                // Добавляем сеть в коллекцию, если оба значения (SSID и сигнал) найдены
                                if (!string.IsNullOrEmpty(ssid))
                                {
                                    networks.Add(new WifiNetwork(ssid, signalStrength));
                                    ssid = null;  // Сбрасываем SSID
                                    signalStrength = null;  // Сбрасываем уровень сигнала
                                }
                            }
                        }
                    }

                    WifiNetworksText = output.ToString(); // Отображаем весь вывод в TextBox

                    /*WifiNetworksText = networks.ToString();*/


                    // Обновляем коллекцию для DataGrid
                    WifiNetworks.Clear(); // Очищаем предыдущие данные перед добавлением новых
                    int Signal = 0;


                    foreach (var network in networks)
                    {
                        WifiNetworks.Add(network);

                        if (Signal < Convert.ToInt32(network.SignalStrength))
                        {
                            WifiNetworksText = $"SSID: {network.SSID}          Сигнал: {Convert.ToInt32(network.SignalStrength)}";
                            Signal = Convert.ToInt32(network.SignalStrength);
                        }

                    }

                    // Отладочные сообщения
                    Console.WriteLine($"Количество сетей: {WifiNetworks.Count}");
                }
            }
            catch (Exception ex)
            {
                WifiNetworksText = $"Ошибка: {ex.Message}";
            }
        }

        private void SaveWifiNetworks()
        {

            string connectionString = "Host=localhost;Username=postgres;Password=12345;Database=wifiSignal";

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    var config = new DbConfig
                    {
                      /*  Host = "localhost",
                        Username = "your_username",
                        Password = "your_password",
                        Database = "your_database"*/
                    };

                    var repository = new WifiSignalRepository(config);

                    // Создание таблицы, если её нет
                    repository.EnsureTableExists();


                    foreach (var wifi in WifiNetworks)
                    {
                        // Вставка данных
                        repository.InsertWifiSignal(wifi.SSID, Convert.ToDouble(wifi.SignalStrength));

                        Console.WriteLine("Данные успешно добавлены.");

                    }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при вставке данных: {ex.Message}");
            }
        }
    

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
