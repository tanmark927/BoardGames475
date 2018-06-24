using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Cecs475.BoardGames.Chess.WpfViewNew
{
    class ChessSquareBackgroundConverter : IMultiValueConverter
    {
        private static SolidColorBrush HOVER_BRUSH = Brushes.LightGreen;
        private static SolidColorBrush BLACK_BRUSH = Brushes.Gray;
        private static SolidColorBrush WHITE_BRUSH = Brushes.White;
        private static SolidColorBrush SELECTED_BRUSH = Brushes.Red;
        private static SolidColorBrush CHECK_BRUSH = Brushes.Yellow;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // This converter will receive two properties: the Position of the square, and whether it
            // is being hovered.
            BoardPosition pos = (BoardPosition)values[0];
            bool isHovered = (bool)values[1];
            int playerSquare = (int)values[2];
            Model.ChessPiece cp = (Model.ChessPiece)values[3];
            bool isSelected = (bool)values[4];
            bool isCheck = (bool)values[5];

            // Mouse Hovering over a Piece
            if (isHovered)
                return HOVER_BRUSH;
            // Current Piece that has been selected to interact with
            if (isCheck)
                return CHECK_BRUSH;
            if (isSelected)
                //if selected, change to red background
                return SELECTED_BRUSH;
            if (pos.Row % 2 == 0 & pos.Col % 2 == 0 | pos.Row % 2 == 1 & pos.Col % 2 == 1)
                return WHITE_BRUSH;
            else
                return BLACK_BRUSH;

           

            //if current player king in check, change square to CHECK_BRUSH
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
