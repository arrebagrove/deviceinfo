using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using Acr.DeviceInfo.Internals;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.Telephony;


namespace Acr.DeviceInfo
{
    public class ConnectivityImpl : IConnectivity
    {
        readonly WifiManager wifiManager;
        readonly TelephonyManager telManager;
        readonly ConnectivityManager connectivityManager;


        public ConnectivityImpl()
        {
            this.wifiManager = (WifiManager)Application.Context.GetSystemService(Context.WifiService);
            this.telManager = (TelephonyManager)Application.Context.ApplicationContext.GetSystemService(Context.TelephonyService);
            this.connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
        }


        public NetworkReachability InternetReachability
        {
            get
            {
                var an = this.connectivityManager.ActiveNetworkInfo;
                if (an == null || !an.IsConnected)
                    return NetworkReachability.NotReachable;

                switch (an.Type)
                {

                    case ConnectivityType.Wimax:
                    case ConnectivityType.Wifi:
                        return NetworkReachability.Wifi;

                    case ConnectivityType.Mobile:
                        return NetworkReachability.Cellular;

                    default:
                        return NetworkReachability.Other;
                }
            }
        }


        public string CellularNetworkCarrier => this.telManager.NetworkOperatorName;


        public string IpAddress => Dns
            .GetHostEntry(Dns.GetHostName())
            .AddressList
            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)?
            .ToString();


        //<uses-permission android:name="android.permission.ACCESS_WIFI_STATE"></uses-permission>
        public string WifiSsid => this.wifiManager.ConnectionInfo?.SSID;

        public IObservable<NetworkReachability> WhenStatusChanged()
        {
            return AndroidObservables
                .WhenIntentReceived(ConnectivityManager.ConnectivityAction)
                .Select(intent => this.InternetReachability);
        }
    }
}