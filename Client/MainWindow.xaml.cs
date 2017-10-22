﻿using AniFile3.Contetns;
using AniFile3.DataStruct;
using CoreLib.DataStruct;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MessagePack;
using RichGrassHopper.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AniFile3
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private enum Category
        {
            Home,
            Subscription,
        }

        // This is Main datas for client, All of client's data is in this
        private Subscriptions _subscriptionStorage;
        private Dictionary<Category, Subscriptions.Node> _categories;
        private EpisodePage _episodePage;
        private HttpInterface _http;

        // 자주 쓰는 것
        private Subscriptions.Node SubscriptionNode
        {
            get => _categories[Category.Subscription];
            set => _categories[Category.Subscription] = value;
        }

        public MainWindow()
        {
            InitializeComponent();

            Console.SetOut(new LogWriter(_testLog));

            _http = new HttpInterface("http://localhost:2323");

            // If Local data file exists, it has to load
            _subscriptionStorage = new Subscriptions();

            // In file existing case
            //using (Stream file = new FileStream("datastorage.bin", FileMode.Open))
            //{
            //    _dataStorage = MessagePack.Deserialize<Subscriptions>(_subscriptionStorage);
            //}

            _categories = new Dictionary<Category, Subscriptions.Node>();            

            _MainTreeView.ItemsSource = _subscriptionStorage;

            _episodePage = new EpisodePage();

            // Home 셋팅
            _subscriptionStorage.Add(new Subscriptions.HomeNode(new HomePage())
            {
                Subject = "홈",
            });

            _categories[Category.Home] = _subscriptionStorage[0];
            
            // 최초 페이지 뷰잉
            _MainFrame.Navigate(_subscriptionStorage[0].CurrentPage);

            var searchPage = new SearchResultPage();
            searchPage.SubsriptionClicked += Subscription_Click;

            var node = new Subscriptions.Node(searchPage)
            {
                Subject = "구독중",
            };

            // '구독' 셋팅
            SubscriptionNode = node;

            node.Count = node.Children.Count;
            _subscriptionStorage.Add(node);

            NativeInterface.Initialize();
        }

        private async void Search()
        {
            var testRequest = Tuple.Create(_SerachText.Text, "720p");
            var response = await _http.Request<string, List<EpisodeInfo>>("/search", _SerachText.Text);
            if (response != null)
            {
                _testLog.Clear();
                foreach (var info in response)
                {
                    Console.WriteLine("{0}, {1}, {2}", info.Fullname, info.Resolution, info.Episode);
                }
            }
        }
        
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        //private async void Search_Click(object sender, RoutedEventArgs e)
        //{
        //    //_SerachText.Text

        //    //string magnetLink = @"magnet:?xt=urn:btih:B4D3A91D9CE527AA7C5B6EDACB969E5486B76EF2&dn= tvN  삼시세끼 바다목장편.E09.170929.720p-NEXT&tr=http://720pier.ru/tracker/announce.php?passkey=3t3mrdei39bxn7x9114tv9vy1reubs8g&tr=http://mgtracker.org:2710/announce&tr=http://tracker.kamigami.org:2710/announce&tr=http://tracker.mg64.net:6881/announce&tr=http://tracker2.wasabii.com.tw:6969/announce&tr=udp://9.rarbg.me:2720/announce&tr=udp://bt.xxx-tracker.com:2710/announce&tr=udp://opentrackr.org:1337/announce&tr=udp://www.eddie4.nl:6969/announce&tr=udp://zer0day.to:1337/announce&tr=udp://tracker1.wasabii.com.tw:6969/announce&tr=udp://185.50.198.188:1337/announce&tr=udp://tracker.leechers-paradise.org:6969&tr=udp://tracker2.indowebster.com:6969/announce&tr=udp://tracker.coppersurfer.tk:6969&tr=http://bt.ttk.artvid.ru:6969/announce&tr=http://bt.artvid.ru:6969/announce&tr=udp://thetracker.org./announce&tr=udp://tracker4.piratux.com:6969/announce&tr=udp://tracker.zer0day.to:1337/announce&tr=udp://62.212.85.66:2710/announce&tr=udp://eddie4.nl:6969/announce&tr=udp://public.popcorn-tracker.org:6969/announce&tr=udp://tracker.grepler.com:6969/announce&tr=http://tracker.dler.org:6969/announce&tr=http://tracker.tiny-vps.com:6969/announce&tr=http://tracker.filetracker.pl:8089/announce&tr=http://tracker.tvunderground.org.ru:3218/announce&tr=http://tracker.grepler.com:6969/announce&tr=http://tracker.kuroy.me:5944/announce&tr=http://210.244.71.26:6969/announce";

        //    string magnetLink = @"magnet:?xt=urn:btih:B4D3A91D9CE527AA7C5B6EDACB969E5486B76EF2";

        //    // TestCode - Mono Torrent
        //    EngineSettings settings = new EngineSettings();
        //    settings.AllowedEncryption = EncryptionTypes.All;
        //    settings.SavePath = System.IO.Path.Combine(Environment.CurrentDirectory, "download");

        //    var engine = new ClientEngine(settings);
        //    engine.ChangeListenEndpoint(new IPEndPoint(IPAddress.Any, 6969));

        //    TorrentSettings torrentSettings = new TorrentSettings(1,10);
        //    var torrentSavePath = System.IO.Path.Combine(settings.SavePath, "torrent");

        //    if (!Directory.Exists(settings.SavePath))
        //        Directory.CreateDirectory(settings.SavePath);

        //    if (!Directory.Exists(torrentSavePath))
        //        Directory.CreateDirectory(torrentSavePath);

        //    var magnet = new MagnetLink(magnetLink);
        //    var hash = InfoHash.FromMagnetLink(magnetLink);
        //    var hashHex = InfoHash.FromHex("B4D3A91D9CE527AA7C5B6EDACB969E5486B76EF2");

        //    var manager = new TorrentManager(magnet, settings.SavePath, torrentSettings, torrentSavePath);

        //    engine.Register(manager);

        //    // DH
        //    var DL = new DhtListener(new IPEndPoint(IPAddress.Any, 6969));
        //    var DH = new DhtEngine(DL);
        //    engine.RegisterDht(DH);

        //    //manager.Start();

        //    DH.Start();
        //    DL.Start();

        //    manager.Start();

        //    await Task.Run(()=>
        //    {  
        //        while (manager.State != TorrentState.Stopped
        //        && manager.State != TorrentState.Paused)
        //        {
        //            _testLog.Invoke(() =>
        //            {
        //                //Console.WriteLine(manager.Progress);
        //            });

        //            Thread.Sleep(1000);

        //            if(manager.Progress >= 100)
        //            {
        //                manager.Stop();
        //                break;
        //            }
        //        }
        //    });
        //}

        private void Subscription_Click(object sender)
        {
            var button = sender as Button;

            string subject = button.DataContext as string;
            
            var result = SubscriptionNode.Children.FirstOrDefault((element) => element.Subject == subject);
            if (result == null)
            {
                var node = new Subscriptions.EpisodeNode(_episodePage)
                {
                    Subject = subject
                };

                string magnet = "magnet:?xt=urn:btih:95F6D0F207888DDB67F89EDC0F47D39B945D2E95&dn=%5btvN%5d%20%ec%95%8c%eb%b0%94%ed%8a%b8%eb%a1%9c%ec%8a%a4.E04.171004.720p-NEXT.mp4&tr=udp%3a%2f%2fzer0day.to%3a1337%2fannounce&tr=udp%3a%2f%2ftracker1.wasabii.com.tw%3a6969%2fannounce&tr=http%3a%2f%2fmgtracker.org%3a6969%2fannounce&tr=udp%3a%2f%2ftracker.grepler.com%3a6969%2fannounce&tr=http%3a%2f%2ftracker.kamigami.org%3a2710%2fannounce&tr=udp%3a%2f%2f182.176.139.129%3a6969%2fannounce&tr=http%3a%2f%2ftracker.mg64.net%3a6881%2fannounce&tr=udp%3a%2f%2f185.50.198.188%3a1337%2fannounce&tr=udp%3a%2f%2f168.235.67.63%3a6969%2fannounce&tr=udp%3a%2f%2ftracker.leechers-paradise.org%3a6969&tr=udp%3a%2f%2fbt.xxx-tracker.com%3a2710%2fannounce&tr=http%3a%2f%2fexplodie.org%3a6969%2fannounce&tr=udp%3a%2f%2ftracker.coppersurfer.tk%3a6969%2fannounce&tr=udp%3a%2f%2ftracker.ilibr.org%3a80%2fannounce&tr=udp%3a%2f%2ftracker.coppersurfer.tk%3a6969&tr=http%3a%2f%2fbt.ttk.artvid.ru%3a6969%2fannounce&tr=http%3a%2f%2fbt.artvid.ru%3a6969%2fannounce&tr=http%3a%2f%2ftracker2.wasabii.com.tw%3a6969%2fannounce&tr=udp%3a%2f%2fthetracker.org.%2fannounce&tr=udp%3a%2f%2feddie4.nl%3a6969%2fannounce&tr=udp%3a%2f%2f62.212.85.66%3a2710%2fannounce&tr=udp%3a%2f%2ftracker.ilibr.org%3a6969%2fannounce&tr=udp%3a%2f%2ftracker.zer0day.to%3a1337%2fannounce";
                node.Episodes.Add(new ClientEpisodeInfo(new EpisodeInfo()
                {
                    Name = subject,
                    //Resolution = "1080p",
                    //Episode = 5,
                    Magnet = magnet
                }));
                //node.Episodes.Add(new EpisodeInfoClient(new EpisodeInfo(subject, "720p", 4, "마그넷주소")));

                node.Episodes[0].Start();

                SubscriptionNode.Children.Add(node);
            }
            else
            {
                Console.WriteLine("이미 같은 것을 구독중입니다");
            }
        }

        private void _MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var node = e.NewValue as Subscriptions.Node;
            node.Navigate(_MainFrame);
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync("확인","모든 데이터가 삭제됩니다\n정말로 삭제하시겠습니까?", MessageDialogStyle.AffirmativeAndNegative);
            if(result == MessageDialogResult.Affirmative)
            {
                var button = sender as Button;
                var node = button.DataContext as Subscriptions.EpisodeNode;

                if(SubscriptionNode.Children.Remove(node) == false)
                {
                    Console.WriteLine("잉 삭제 실패");
                }
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NativeInterface.Uninitialize();
        }

        private void _SerachText_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                MessageBox.Show(_SerachText.Text);
            }
        }
    }
}
