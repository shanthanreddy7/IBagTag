using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Quick.Xamarin.BLE.Abstractions;
using Xamarin.Forms;
namespace IbagTag
{
    public partial class BluetoothService : ContentPage
    {

        public static AdapterConnectStatus BleStatus;
        List<IGattCharacteristic> AllCharacteristics = new List<IGattCharacteristic>();
        IGattCharacteristic SelectCharacteristic = null;
        ObservableCollection<CharacteristicsList> CharacteristicsList = new ObservableCollection<CharacteristicsList>();

        public bool IsLED;

        private string ledString = "41e10003-a387-4964-a1fa-19a7e312cf4e";
        public static string blink_green_one_input = "040401040104010401";
        public static string blink_green_all_input = "020101020203030404";
        public static string reset_off_red = "000000000000000000";


        public BluetoothService()
        {
            InitializeComponent();
            ScanDevicesList.ble.AdapterStatusChange += Ble_AdapterStatusChange;
        }

        private void Ble_AdapterStatusChange(object sender, AdapterConnectStatus e)
        {
            Device.BeginInvokeOnMainThread(async () => {
                ScanDevicesList.BleStatus = e;
                if (ScanDevicesList.BleStatus == AdapterConnectStatus.Connected)
                {
                    msg_txt.Text = "Success";
                    await Task.Delay(3000);
                    msg_layout.IsVisible = false;
                    ReadCharacteristics();
                }
                if (ScanDevicesList.BleStatus == AdapterConnectStatus.None)
                {
                    await Navigation.PopToRootAsync(true);
                }
            });
        }
        void ReadCharacteristics()
        {
            ScanDevicesList.ConnectDevice.CharacteristicsDiscovered(cha =>
            {
                Device.BeginInvokeOnMainThread(() => {
                    AllCharacteristics.Add(cha);
                    CharacteristicsList.Add(new CharacteristicsList(cha.Uuid, cha.CanRead(), cha.CanWrite(), cha.CanNotify()));
                    ActivateLEDMode();
                });
            });
        }

        // activate the LED Mode 
        void ActivateLEDMode()
        {
            foreach (var c in AllCharacteristics)
            {
                //validate if eligible to the LEDS //
                if (c.Uuid.StartsWith("41e10003"))
                {
                    IsLED = true;
                    button_layout.IsVisible = true;
                    SelectCharacteristic = c;
                    break;
                }
            }
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ScanDevicesList.ble.AdapterStatusChange -= Ble_AdapterStatusChange;
            if (ScanDevicesList.ConnectDevice != null) ScanDevicesList.ConnectDevice.DisconnectDevice();
        }

        // to read the characteritics...
        private void read_Clicked(object sender, EventArgs e)
        {
            if (SelectCharacteristic != null)
            {
                SelectCharacteristic.ReadCallBack();
            }
        }

        // to blin to green one side...
        private void blink_green_one(object sender, EventArgs e)
        {
            var blue_byteArray = StringToByteArray(blink_green_one_input);

            if (SelectCharacteristic != null)
            {
                SelectCharacteristic.Write(blue_byteArray);
            }
        }

        // to blink to green all...
        private void blink_green_all(object sender, EventArgs e)
        {
            var green_byteArray = StringToByteArray(blink_green_all_input);

            if (SelectCharacteristic != null)
            {
                SelectCharacteristic.Write(green_byteArray);
            }
        }

        // to set it back its original state...
        private void reset(object sender, EventArgs e)
        {
            var reset_byte_response = StringToByteArray(reset_off_red);

            if (SelectCharacteristic != null)
            {
                SelectCharacteristic.Write(reset_byte_response);
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            try
            {
                return Enumerable.Range(0, hex.Length)
                                 .Where(x => x % 2 == 0)
                                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                 .ToArray();
            }
            catch { return null; }
        }
    }
}

