using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cecs475.BoardGames.Chess.WpfViewNew
{
    class ChessSquarePlayerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Model.ChessPiece cs = (Model.ChessPiece)value;
                if (cs.PieceType != Model.ChessPieceType.Empty && cs.Player != 0)
                {
                    switch (cs.Player)
                    {
                        case 1:
                            switch (cs.PieceType)
                            {
                                case Model.ChessPieceType.Pawn:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/WhitePawn.png", UriKind.Relative));
                                case Model.ChessPieceType.Rook:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/WhiteRook.png", UriKind.Relative));
                                case Model.ChessPieceType.Knight:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/WhiteKnight.png", UriKind.Relative));
                                case Model.ChessPieceType.Bishop:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/WhiteBishop.png", UriKind.Relative));
                                case Model.ChessPieceType.Queen:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/WhiteQueen.png", UriKind.Relative));
                                case Model.ChessPieceType.King:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/WhiteKing.png", UriKind.Relative));
                                default:
                                    return null;
                            }
                        case 2:
                            switch (cs.PieceType)
                            {
                                case Model.ChessPieceType.Pawn:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/BlackPawn.png", UriKind.Relative));
                                case Model.ChessPieceType.Rook:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/BlackRook.png", UriKind.Relative));
                                case Model.ChessPieceType.Knight:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/BlackKnight.png", UriKind.Relative));
                                case Model.ChessPieceType.Bishop:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/BlackBishop.png", UriKind.Relative));
                                case Model.ChessPieceType.Queen:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/BlackQueen.png", UriKind.Relative));
                                case Model.ChessPieceType.King:
                                    return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfViewNew;component/Resources/BlackKing.png", UriKind.Relative));
                                default:
                                    return null;
                            }
                        default:
                            return null;
                    }
                }
                else
                   return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
