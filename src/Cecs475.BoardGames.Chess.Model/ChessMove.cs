﻿using System;
using Cecs475.BoardGames.Model;

namespace Cecs475.BoardGames.Chess.Model {
	/// <summary>
	/// Represents a single move to be applied to a chess board.
	/// </summary>
	public class ChessMove : IGameMove, IEquatable<ChessMove> {
		// You can add additional fields, properties, and methods as you find
		// them necessary, but you cannot MODIFY any of the existing implementations.

		/// <summary>
		/// The starting position of the move.
		/// </summary>
		public BoardPosition StartPosition { get; }

		/// <summary>
		/// The ending position of the move.
		/// </summary>
		public BoardPosition EndPosition { get; }

        public ChessPieceType PromotionPiece { get; }

		/// <summary>
		/// The type of move being applied.
		/// </summary>
		public ChessMoveType MoveType { get; set; }

		// You must set this property when applying a move.
		public int Player { get; set; }

		/// <summary>
		/// Constructs a ChessMove that moves a piece from one position to another
		/// </summary>
		/// <param name="start">the starting position of the piece to move</param>
		/// <param name="end">the position where the piece will end up</param>
		/// <param name="moveType">the type of move represented</param>
        /// <param name="pieceType">the piece to replace pawn during promotion</param>

        public ChessMove(BoardPosition start, BoardPosition end, ChessMoveType moveType = ChessMoveType.Normal, ChessPieceType pieceType = ChessPieceType.Empty)
        {
            StartPosition = start;
            EndPosition = end;
            MoveType = moveType;
            PromotionPiece = pieceType;
        }

        public virtual bool Equals(ChessMove other)
        {
            if (MoveType != ChessMoveType.PawnPromote)
                return other != null && StartPosition.Equals(other.StartPosition)
                && EndPosition.Equals(other.EndPosition);
            else
                return other != null && StartPosition.Equals(other.StartPosition)
                && EndPosition.Equals(other.EndPosition) && PromotionPiece.Equals(other.PromotionPiece);
        }

		// Equality methods.
		bool IEquatable<IGameMove>.Equals(IGameMove other) {
			ChessMove m = other as ChessMove;
			return this.Equals(m);
		}

		public override bool Equals(object other) {
			return Equals(other as ChessMove);
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = StartPosition.GetHashCode();
				hashCode = (hashCode * 397) ^ EndPosition.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)MoveType;
				return hashCode;
			}
		}

        /// <summary>
        /// True if the move represents a "pass".
        /// </summary>
        public bool IsPass =>
            EndPosition.Row == -1 && EndPosition.Col == -1;

        public override string ToString() {
			return $"{StartPosition} to {EndPosition}";
		}
	}
}
