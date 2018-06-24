using System;
using System.Text;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.View;

namespace Cecs475.BoardGames.Chess.View {
	/// <summary>
	/// A chess game view for string-based console input and output.
	/// </summary>
	public class ChessConsoleView : IConsoleView {
		private static char[] LABELS = { '.', 'P', 'R', 'N', 'B', 'Q', 'K' };
		
		public string BoardToString(ChessBoard board) {
			StringBuilder str = new StringBuilder();

			for (int i = 0; i < ChessBoard.BoardSize; i++) {
				str.Append(8 - i);
				str.Append(" ");
				for (int j = 0; j < ChessBoard.BoardSize; j++) {
					var space = board.GetPieceAtPosition(new BoardPosition(i, j));
					if (space.PieceType == ChessPieceType.Empty)
						str.Append(". ");
					else if (space.Player == 1)
						str.Append($"{LABELS[(int)space.PieceType]} ");
					else
						str.Append($"{char.ToLower(LABELS[(int)space.PieceType])} ");
				}
				str.AppendLine();
			}
			str.AppendLine("  a b c d e f g h");
			return str.ToString();
		}

        private string GetPromotionString(ChessPieceType cpt)
        {
            switch (cpt)
            {
                case ChessPieceType.Queen:
                    return "Queen";
                case ChessPieceType.Bishop:
                    return "Bishop";
                case ChessPieceType.Knight:
                    return "Knight";
                case ChessPieceType.Rook:
                    return "Rook";
                default:
                    return "Empty";
            }
        }

		/// <summary>
		/// Converts the given ChessMove to a string representation in the form
		/// "(start, end)", where start and end are board positions in algebraic
		/// notation (e.g., "a5").
		/// 
		/// If this move is a pawn promotion move, the selected promotion piece 
		/// must also be in parentheses after the end position, as in 
		/// "(a7, a8, Queen)".
		/// </summary>
		public string MoveToString(ChessMove move) {
            string moveDetails;

            if(move.MoveType == ChessMoveType.PawnPromote)
            {
                moveDetails = "(" + PositionToString(move.StartPosition) + ", "
                + PositionToString(move.EndPosition) + ", " + GetPromotionString(move.PromotionPiece) + ")";
            }
            else
            {
                moveDetails = "(" + PositionToString(move.StartPosition) + ", "
               + PositionToString(move.EndPosition) + ")";
            }
            return moveDetails;
        }
    
		public string PlayerToString(int player) {
			return player == 1 ? "White" : "Black";
		}

        private ChessPieceType PromoteStringToPiece(string s)
        {
            switch (s.ToLower())
            {
                case "queen":
                    return ChessPieceType.Queen;
                case "bishop":
                    return ChessPieceType.Bishop;
                case "knight":
                    return ChessPieceType.Knight;
                case "rook":
                    return ChessPieceType.Rook;
                default:
                    return ChessPieceType.Empty;
            }
        }

		/// <summary>
		/// Converts a string representation of a move into a ChessMove object.
		/// Must work with any string representation created by MoveToString.
		/// </summary>
		public ChessMove ParseMove(string moveText) {
            string[] split = moveText.Trim(new char[] { '(', ')' }).Split(',');

            //for all except pawn promotion
            if(split.Length == 3)
                return new ChessMove(
                ParsePosition(split[0].Trim()), ParsePosition(split[1].Trim()),
                ChessMoveType.PawnPromote, PromoteStringToPiece(split[2].Trim()));
            else
                return new ChessMove(
                ParsePosition(split[0].Trim()), ParsePosition(split[1].Trim()),
                ChessMoveType.Normal);
		}

		public static BoardPosition ParsePosition(string pos) {
			return new BoardPosition(8 - (pos[1] - '0'), pos[0] - 'a');
		}

		public static string PositionToString(BoardPosition pos) {
			return $"{(char)(pos.Col + 'a')}{8 - pos.Row}";
		}

		#region Explicit interface implementations
		// Explicit method implementations. Do not modify these.
		string IConsoleView.BoardToString(IGameBoard board) {
			return BoardToString(board as ChessBoard);
		}

		string IConsoleView.MoveToString(IGameMove move) {
			return MoveToString(move as ChessMove);
		}

		IGameMove IConsoleView.ParseMove(string moveText) {
			return ParseMove(moveText);
		}
		#endregion
	}
}
