using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Quick.Xamarin.BLE;
using Quick.Xamarin.BLE.Abstractions;
using Xamarin.Forms;
namespace IbagTag
{
    public partial class ScanDevicesList : ContentPage
    {
        public static AdapterConnectStatus BleStatus;
        public static IBle ble;
        public static IDev ConnectDevice = null;
        ObservableCollection<BleList> blelist = new ObservableCollection<BleList>();
        public static List<IDev> ScanDevices = new List<IDev>();

        public ScanDevicesList()
        {
            InitializeComponent();


            ble = CrossBle.Createble();
            //when search devices
            ble.OnScanDevicesIn += Ble_OnScanDevicesIn;

            BleStatus = ble.AdapterConnectStatus;
            listView.ItemsSource = blelist;
        }

        private void Ble_OnScanDevicesIn(object sender, IDev e)
        {
            Device.BeginInvokeOnMainThread(() => {

                try
                {

                    if (e.Name != null)
                    {
                        var n = ScanDevices.Find(x => x.Uuid == e.Uuid);
                        if (n == null)
                        {
                            ScanDevices.Add(e);
                            blelist.Add(new BleList(e.Name, e.Uuid));
                        }

                    }
                }
                catch { }

            });
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var n = (BleList)e.Item;
            foreach (var dev in ScanDevices)
            {
                if (n.Uuid == dev.Uuid)
                {
                    var check = await DisplayAlert("", "Connecting to  [" + dev.Name + "]", "ok", "cancel");

                    if (check)
                    {
                        ConnectDevice = dev;
                        ConnectDevice.ConnectToDevice();
                        await Navigation.PushAsync(new BluetoothService(), false);

                    }
                }
            }
        }

        protected override void OnAppearing()
        {

            ScanDevices.Clear();
            blelist.Clear();

            ble.StartScanningForDevices();
            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            ble.StopScanningForDevices();
            base.OnDisappearing();
        }
        private void ListView_Refreshing(object sender, EventArgs e)
        {
            ScanDevices.Clear();
            blelist.Clear();
            ScanDevices = new List<IDev>();
            blelist = new ObservableCollection<BleList>();
            listView.EndRefresh();
        }
    }
}

