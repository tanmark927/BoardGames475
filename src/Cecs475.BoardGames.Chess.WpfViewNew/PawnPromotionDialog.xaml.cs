using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Cecs475.BoardGames.WpfView;

namespace Cecs475.BoardGames.Chess.WpfViewNew {
    /// <summary>
    /// Interaction logic for PawnPromotionDialog.xaml
    /// </summary>
    public partial class PawnPromotionDialog {

        private readonly ChessViewModel cvm;
        private ChessSquare startPos;
        private ChessSquare endPos;

        public PawnPromotionDialog(ChessViewModel Model, ChessSquare start, ChessSquare end) {
            InitializeComponent();
            DataContext = this;
            cvm = Model;
            startPos = start;
            endPos = end;
            if (cvm.CurrentPlayer == 1) {
                Rook.Content = FindResource("WR");
                Bishop.Content = FindResource("WB");
                Knight.Content = FindResource("WK");
                Queen.Content = FindResource("WQ");
            }
            else {
                Rook.Content = FindResource("BR");
                Bishop.Content = FindResource("BB");
                Knight.Content = FindResource("BK");
                Queen.Content = FindResource("BQ");
            }
        }

        public Model.ChessPieceType PieceType;
        
        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            var button = sender as Button;
          
            if (button == null)
                return;
           
            switch (button.Name)
            {
                case "Rook":
                    PieceType = Model.ChessPieceType.Rook;
                    break;
                case "Knight":
                    PieceType = Model.ChessPieceType.Knight;
                    break;
                case "Bishop":
                    PieceType = Model.ChessPieceType.Bishop;
                    break;
                case "Queen":
                    PieceType = Model.ChessPieceType.Queen;
                    break;
            }
            await cvm.ApplyMove(startPos.Position, endPos.Position, PieceType);
            Close();
        }
    }
}