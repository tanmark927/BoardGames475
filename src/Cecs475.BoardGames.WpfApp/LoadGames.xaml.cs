using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cecs475.BoardGames.WpfApp
{
    /// <summary>
    /// Interaction logic for LoadGames.xaml
    /// </summary>
    public partial class LoadGames : Window
    {
        public LoadGames()
        {
            InitializeComponent();
        }

        private async void mLoaded(object sender, RoutedEventArgs e)
        {
            var client = new RestClient("https://cecs475-boardamges.herokuapp.com/");
            var request = new RestRequest("api/games", Method.GET);
            var response = await client.ExecuteTaskAsync(request);

            string r = response.Content;
            r = r.Substring(1);
            r = r.Substring(0, r.Length - 1);
            var games = JObject.Parse(r);
            JToken results = games.Last.First;
            List<string> strList = new List<string>();
            List<JToken> things = new List<JToken>() { results.First, results.Last };
            foreach (JToken t in things) {
                JToken token = t.First;
                do {
                    strList.Add(token.First.ToObject<string>());
                    token = token.Next;
                }
                while (token != null);
            }
            for (int i = 0; i < strList.Count(); i++) {
                string val = strList.ElementAt(i);
                if (val.Contains("https")) {
                    var web = new WebClient();
                    await web.DownloadFileTaskAsync(val, "games/" + strList.ElementAt(i - 1));
                }
            }
            
            
            GameChoiceWindow cgw = new GameChoiceWindow();
            cgw.Show();
            this.Close();
        }
    }
}
