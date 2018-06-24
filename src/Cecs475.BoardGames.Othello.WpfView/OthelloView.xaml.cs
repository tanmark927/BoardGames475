using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Cecs475.BoardGames.Othello.WpfView {
	/// <summary>
	/// Interaction logic for OthelloView.xaml
	/// </summary>
	public partial class OthelloView : UserControl, IWpfGameView {
		public OthelloView() {
			InitializeComponent();
		}

		private void Border_MouseEnter(object sender, MouseEventArgs e) {
            if (!IsEnabled) return;
            Border b = sender as Border;
			var square = b.DataContext as OthelloSquare;
			var vm = FindResource("vm") as OthelloViewModel;
			if (vm.PossibleMoves.Contains(square.Position)) {
				square.IsHighlighted = true;
			}
		}

		private void Border_MouseLeave(object sender, MouseEventArgs e) {
            if (!IsEnabled) return;
            Border b = sender as Border;
			var square = b.DataContext as OthelloSquare;
			square.IsHighlighted = false;
		}

		public OthelloViewModel OthelloViewModel => FindResource("vm") as OthelloViewModel; 

		public Control ViewControl => this;

		public IGameViewModel ViewModel => OthelloViewModel;

		private async void Border_MouseUp(object sender, MouseButtonEventArgs e) {
            if (!IsEnabled) return;
            Border b = sender as Border;
			var square = b.DataContext as OthelloSquare;
			var vm = FindResource("vm") as OthelloViewModel;
			if (vm.PossibleMoves.Contains(square.Position)) {
                IsEnabled = false;
                vm.UndoEnabler = false;
                await vm.ApplyMove(square.Position);
                IsEnabled = true;
                vm.UndoEnabler = true;
				square.IsHighlighted = false;
			}
		}
	}
}
