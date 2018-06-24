using Cecs475.BoardGames.WpfView;
using System.Windows.Data;

namespace Cecs475.BoardGames.Chess.WpfViewNew
{
    public class ChessGameFactory : IWpfGameFactory
    {
        public string GameName { get { return "Chess"; } }

        public IValueConverter CreateBoardAdvantageConverter()
        {
            return new ChessAdvantageConverter();
        }

        public IValueConverter CreateCurrentPlayerConverter()
        {
            return new ChessCurrentPlayerConverter();
        }

        public IWpfGameView CreateGameView(NumberOfPlayers players)
        {
            var view = new ChessView();
            view.ViewModel.Players = players;
            return view;
        }
    }
}
