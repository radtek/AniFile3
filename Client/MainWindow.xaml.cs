﻿using AniFile3.DataStruct;
using CoreLib.MessagePackets;
using MahApps.Metro.Controls;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Client.Encryption;
using MonoTorrent.Common;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using RichGrassHopper.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AniFile3
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Subscriptions _subscriptions;
        private List<string> _tempResponse;

        public MainWindow()
        {
            InitializeComponent();

            _tempResponse = new List<string>();

            //Console.SetOut(new LogWriter(_testLog));

            _subscriptions = new Subscriptions();

            _MainTreeView.ItemsSource = _subscriptions;
            _TempResultList.ItemsSource = _tempResponse;

            //---- Test Data
            var node = new Subscriptions.Node()
            {
                Subject = "구독중"
            };
            node.Children.Add(new Subscriptions.Node()
            {
                Subject = "무한도전"
            });

            node.Count = node.Children.Count;

            _subscriptions.Add(node);

            _tempResponse.Add("11111");
            _tempResponse.Add("22222");
            _tempResponse.Add("333333");
            _tempResponse.Add("4444");
            _tempResponse.Add("무한도전");
            _tempResponse.Add("5555");

            // TestCode - Serialize and Deserialize
            var aaa = MessagePack.Serialize(_tempResponse);
            var bbb = MessagePack.Deserialize<List<string>>(aaa);
        }
        
        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            //_SerachText.Text

            //string magnetLink = @"magnet:?xt=urn:btih:B4D3A91D9CE527AA7C5B6EDACB969E5486B76EF2&dn= tvN  삼시세끼 바다목장편.E09.170929.720p-NEXT&tr=http://720pier.ru/tracker/announce.php?passkey=3t3mrdei39bxn7x9114tv9vy1reubs8g&tr=http://mgtracker.org:2710/announce&tr=http://tracker.kamigami.org:2710/announce&tr=http://tracker.mg64.net:6881/announce&tr=http://tracker2.wasabii.com.tw:6969/announce&tr=udp://9.rarbg.me:2720/announce&tr=udp://bt.xxx-tracker.com:2710/announce&tr=udp://opentrackr.org:1337/announce&tr=udp://www.eddie4.nl:6969/announce&tr=udp://zer0day.to:1337/announce&tr=udp://tracker1.wasabii.com.tw:6969/announce&tr=udp://185.50.198.188:1337/announce&tr=udp://tracker.leechers-paradise.org:6969&tr=udp://tracker2.indowebster.com:6969/announce&tr=udp://tracker.coppersurfer.tk:6969&tr=http://bt.ttk.artvid.ru:6969/announce&tr=http://bt.artvid.ru:6969/announce&tr=udp://thetracker.org./announce&tr=udp://tracker4.piratux.com:6969/announce&tr=udp://tracker.zer0day.to:1337/announce&tr=udp://62.212.85.66:2710/announce&tr=udp://eddie4.nl:6969/announce&tr=udp://public.popcorn-tracker.org:6969/announce&tr=udp://tracker.grepler.com:6969/announce&tr=http://tracker.dler.org:6969/announce&tr=http://tracker.tiny-vps.com:6969/announce&tr=http://tracker.filetracker.pl:8089/announce&tr=http://tracker.tvunderground.org.ru:3218/announce&tr=http://tracker.grepler.com:6969/announce&tr=http://tracker.kuroy.me:5944/announce&tr=http://210.244.71.26:6969/announce";

            string magnetLink = @"magnet:?xt=urn:btih:B4D3A91D9CE527AA7C5B6EDACB969E5486B76EF2";

            // TestCode - Mono Torrent
            EngineSettings settings = new EngineSettings();
            settings.AllowedEncryption = EncryptionTypes.All;
            settings.SavePath = System.IO.Path.Combine(Environment.CurrentDirectory, "download");

            var engine = new ClientEngine(settings);
            engine.ChangeListenEndpoint(new IPEndPoint(IPAddress.Any, 6969));

            TorrentSettings torrentSettings = new TorrentSettings(1,10);
            var torrentSavePath = System.IO.Path.Combine(settings.SavePath, "torrent");

            if (!Directory.Exists(settings.SavePath))
                Directory.CreateDirectory(settings.SavePath);

            if (!Directory.Exists(torrentSavePath))
                Directory.CreateDirectory(torrentSavePath);
            
            var magnet = new MagnetLink(magnetLink);
            var hash = InfoHash.FromMagnetLink(magnetLink);
            var hashHex = InfoHash.FromHex("B4D3A91D9CE527AA7C5B6EDACB969E5486B76EF2");

            var manager = new TorrentManager(magnet, settings.SavePath, torrentSettings, torrentSavePath);

            engine.Register(manager);

            // DH
            var DL = new DhtListener(new IPEndPoint(IPAddress.Any, 6969));
            var DH = new DhtEngine(DL);
            engine.RegisterDht(DH);

            //manager.Start();

            DH.Start();
            DL.Start();

            DL.MessageReceived += DL_MessageReceived;

            manager.Start();

            await Task.Run(()=>
            {  
                while (manager.State != TorrentState.Stopped
                && manager.State != TorrentState.Paused)
                {
                    _testLog.Invoke(() =>
                    {
                        Console.WriteLine(manager.Progress);
                    });
                    
                    Thread.Sleep(1000);

                    if(manager.Progress >= 100)
                    {
                        manager.Stop();
                        break;
                    }
                }
            });
        }

        private void DL_MessageReceived(byte[] buffer, IPEndPoint endpoint)
        {
            using (FileStream file = new FileStream("test.torrent", FileMode.Create))
            {
                file.Write(buffer, 0, buffer.Length);
            }
        }

        private void Subscription_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            string subject = button.DataContext as string;
            
            var result = _subscriptions.FirstOrDefault((element) => element.Subject == subject);
            if(result == null)
            {
                var node = new Subscriptions.Node()
                {
                    Subject = subject
                };
                _subscriptions.Add(node);
            }
            else
            {
                Console.WriteLine("이미 같은 것을 구독중입니다");
            }
        }
    }
}