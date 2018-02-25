﻿using Scriping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace AniFile3.MagnetLising
{
    // 이름 짓기 어렵네
    public class MagnetListUp
    {
        private FirstSite _scriper;
        private List<SyndicationItem> _feeds;
        private WeakReference<HttpInterface> _http;

        public MagnetListUp(HttpInterface httpInterface)
        {
            _http = new WeakReference<HttpInterface>(httpInterface);

            // 스크랩퍼 초기화
            _scriper = new Scriping.FirstSite(new System.Windows.Forms.WebBrowser());
            _scriper.InitializeCompleted += async () =>
            {
                // 여기서 검색등 자료를 찾고, 그 자료를 서버로 보낸다.

                // Test
                //var result = await _scriper.SearchBox("아는 형님");

                //foreach(var item in result)
                //{
                //    Console.WriteLine("{0}:{1}", item.Name, item.Episode);
                //}
            };

            _scriper.Initialize();

            UpdateRSSList();
        }

        // 서버로부터 RSS 리스트를 얻어와 업뎃한다.
        private async void UpdateRSSList()
        {
            HttpInterface http = null;
            if (_http.TryGetTarget(out http))
            {
                var rsslist = await http.RequestWithTimeout<List<string>>("/update_rsslist");

                if(rsslist?.Count > 0)
                {
                    Preference.Instance.RSSList = rsslist;
                }
            }
        }
        
        private async Task<string> RequestXMLString(string uri)
        {
            Console.WriteLine("[RSS:{0}] Reading XML...", uri);

            string content = null;

            try
            {
                WebRequest request = WebRequest.Create(uri);
                request.Credentials = CredentialCache.DefaultCredentials;

                using (WebResponse response = await request.GetResponseAsync())
                {
                    Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    content = reader.ReadToEnd();

                    reader.Close();
                    dataStream.Close();

                    Console.WriteLine("[RSS:{0}] Done, Contents Lenght({1})", uri, content.Length);
                }
            }
            catch (WebException e)
            {
                Console.WriteLine("[RSS ERROR:{0}] {1}", uri, e.ToString());
            }

            return content;
        }

        // feeds에 모든 RSS 피드를 채워 넣음
        public async void UpdateRSS()
        {
            _feeds = new List<SyndicationItem>();

            // 리스트를 클로닝 해서 처리한다, 다른 스레드에서 리스트를 업뎃 할수 있기에
            var rssList = new List<string>(Preference.Instance.RSSList);
            foreach (var uri in rssList)
            {
                var xmlstring = await RequestXMLString(uri);

                if (string.IsNullOrEmpty(xmlstring) == false)
                {
                    Regex regex = new Regex(@"&(?![a-z]{2,5};)");
                    xmlstring = regex.Replace(xmlstring, "&amp;");

                    using (XmlReader reader = XmlReader.Create(new StringReader(xmlstring)))
                    {
                        SyndicationFeed feed = SyndicationFeed.Load(reader);
                        reader.Close();
                        foreach (SyndicationItem item in feed.Items)
                        {
                            _feeds.Add(item);
                            //Console.WriteLine("{0}, {1}", item.Title.Text, item.Links[0].Uri);
                        }
                    }
                }
            }
        }
    }
}