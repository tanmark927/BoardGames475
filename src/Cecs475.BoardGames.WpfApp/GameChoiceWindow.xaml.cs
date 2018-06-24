using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace Cecs475.BoardGames.WpfApp {
    /// <summary>
    /// Interaction logic for GameChoiceWindow.xaml
    /// </summary>
    public partial class GameChoiceWindow : Window {
        public GameChoiceWindow() {
            InitializeComponent();
            Type IWpfGameFactory = typeof(IWpfGameFactory);

            var files = Directory.GetFiles("games");
            foreach (var f in Directory.GetFiles("games")) {
                Console.WriteLine(f);

                // Strip " games\ " folder name
                string file = f.Substring(6);

                // strips .dll ending
                file = file.Substring(0, file.Length - 4);

                // Load from assembly for strong named
                Assembly.Load($"{file}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=68e71c13048d452a");

                //// if file ends with dll
                // if (file.Substring(file.Length - 3) == "dll") {
                
                // Load file
                //Assembly.LoadFrom(f);

                
            }

            List<object> GamesList = new List<object>();
            var boardTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => IWpfGameFactory.IsAssignableFrom(t) && t.IsClass);
            foreach (var val in boardTypes) {
                
                IWpfGameFactory v = (IWpfGameFactory)val.GetConstructor(Type.EmptyTypes).Invoke(null);
                GamesList.Add(v);
            }
            this.Resources["GameTypes"] = GamesList;
        }


        private void Button_Click(object sender, RoutedEventArgs e) {
            Button b = sender as Button;
            // Retrieve the game type bound to the button
            IWpfGameFactory gameType = b.DataContext as IWpfGameFactory;
            // Construct a GameWindow to play the game.
            var gameWindow = new GameWindow(gameType,
                mHumanBtn.IsChecked.Value ? NumberOfPlayers.Two : NumberOfPlayers.One)
            {
                Title = gameType.GameName
            };
            // When the GameWindow closes, we want to show this window again.
            gameWindow.Closed += GameWindow_Closed;

            // Show the GameWindow, hide the Choice window.
            gameWindow.Show();
            this.Hide();
        }

        private void GameWindow_Closed(object sender, EventArgs e) {
            this.Show();
        }
    }
}
