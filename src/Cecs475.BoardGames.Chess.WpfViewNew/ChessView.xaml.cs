using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cecs475.BoardGames.WpfView;


namespace Cecs475.BoardGames.Chess.WpfViewNew
{
    /// <summary>
    /// Interaction logic for ChessView.xaml
    /// </summary>
    public partial class ChessView : UserControl, IWpfGameView
    {
        public ChessView()
        {
            InitializeComponent();
        }

        private void Border_MouseClick(object sender, MouseEventArgs e)
        {
            if (!IsEnabled) return;
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("vm") as ChessViewModel;
            square.IsSelected = (square.Player == vm.CurrentPlayer & vm.StartMoves.Contains(square.Position)) ? true : false;

            // save start position if there is none, null represents beginning of turn
            if (CurrentSelection == null && square.IsSelected)
                CurrentSelection = square;
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsEnabled) return;
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("vm") as ChessViewModel;

            // Select start position
            if (CurrentSelection == null & vm.StartMoves.Contains(square.Position))
                square.IsHighlighted = true;

            // Select possible end position
            if (CurrentSelection != null)
            {
                foreach (Model.ChessMove cm in vm.PossMoves)
                {
                    if (cm.EndPosition.Equals(square.Position) & cm.StartPosition.Equals(CurrentSelection.Position))
                        vm.EndMoves.Add(square.Position);
                }

                if (vm.EndMoves.Contains(square.Position))
                    square.IsHighlighted = true;
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsEnabled) return;
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            square.IsHighlighted = false;
        }

        private async void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("vm") as ChessViewModel;

            if (CurrentSelection == null) return;

            //for selecting start position
            if (square.Position == CurrentSelection.Position) 
            {
                foreach (Model.ChessMove cm in vm.PossMoves.Where(x => x.StartPosition == CurrentSelection.Position))
                {
                    if (cm.EndPosition.Equals(square.Position) & cm.StartPosition.Equals(CurrentSelection.Position))
                        vm.EndMoves.Add(square.Position);
                }
                square.IsHighlighted = false;
            }
            //for selecting end position
            else {
                if (vm.StartMoves.Contains(CurrentSelection.Position) & vm.EndMoves.Contains(square.Position))
                {
                    Model.ChessMove pm = vm.PossMoves.Where(x => x.StartPosition == CurrentSelection.Position & x.EndPosition == square.Position).FirstOrDefault();

                    if (pm.MoveType == Model.ChessMoveType.PawnPromote)
                    {
                        var pawnWindow = new PawnPromotionDialog(vm, CurrentSelection, square);
                        pawnWindow.Show();
                    }
                    else
                    {
                        IsEnabled = false;
                        vm.UndoEnabler = false;
                        await vm.ApplyMove(CurrentSelection.Position, square.Position, Model.ChessPieceType.Empty);
                        IsEnabled = true;
                        vm.UndoEnabler = true;
                    }
                    CurrentSelection.IsSelected = false;
                    square.IsSelected = false;
                    CurrentSelection = null;
                    vm.EndMoves = new HashSet<BoardGames.Model.BoardPosition>();
                }
                else if (vm.StartMoves.Contains(CurrentSelection.Position) & !vm.EndMoves.Contains(square.Position))
                {
                    CurrentSelection.IsSelected = false;
                    square.IsSelected = false;
                    CurrentSelection = null;
                    vm.EndMoves = new HashSet<BoardGames.Model.BoardPosition>();
                }
            }
        }

        public ChessSquare CurrentSelection;
        public ChessViewModel ChessViewModel => FindResource("vm") as ChessViewModel;
        public Control ViewControl => this;
        public IGameViewModel ViewModel => ChessViewModel;
    }
}
