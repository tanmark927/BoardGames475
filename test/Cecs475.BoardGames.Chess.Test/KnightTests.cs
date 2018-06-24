using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Cecs475.BoardGames.Chess.Model;
using Xunit;
using System.Linq;

namespace Cecs475.BoardGames.Chess.Test {
	public class KnightTests : ChessTest {
		//Checks the knight moves for black pieces
		[Fact]
		public void KnightMoves() {
			ChessBoard b = CreateBoardFromMoves(
				 "e2, e4");
			var moves = b.GetPossibleMoves();
			var expectedMoves = GetMovesAtPosition(moves, Pos("b8"));
			expectedMoves.Should().HaveCount(2, "The knight at location B8 should be able to move to A6 and C6")
				 .And.Contain(Move("b8,a6"))
				 .And.Contain(Move("b8,c6"));
		}

		[Fact] //w
		public void UndoKnightMoveTwice() {
			ChessBoard b = CreateBoardFromMoves(
				 "g1, f3",
				 "f7, f5",
				 "f3, d4"
			);

			b.GetPieceAtPosition(Pos("d4")).PieceType.Should().Be(ChessPieceType.Knight, "Knight moved to d4");
			b.UndoLastMove();
			b.GetPieceAtPosition(Pos("f3")).PieceType.Should().Be(ChessPieceType.Knight, "Knight returns back to f3");
			b.UndoLastMove();
			b.GetPieceAtPosition(Pos("f7")).PieceType.Should().Be(ChessPieceType.Pawn, "Pawn is at initial position");
			b.UndoLastMove();
			b.GetPieceAtPosition(Pos("g1")).PieceType.Should().Be(ChessPieceType.Knight, "Knight is at initial position");

		}

		/// <summary>
		/// Use white knight to validate that GetPossibleMoves correctly reports all possible moves
		/// </summary>
		[Fact]
		public void KnightValidatePossibleMoves() {
			ChessBoard b = new ChessBoard();

			var possMoves = b.GetPossibleMoves();
			b.CurrentPlayer.Should().Be(1, "white's turn");

			var twoMovesExpected = GetMovesAtPosition(possMoves, Pos("b1"));
			twoMovesExpected.Should().Contain(Move("b1, a3"))
				 .And.Contain(Move("b1, c3"))
				 .And.HaveCount(2, "white knight starting at B1 can't move to D2 because it is occupied");


			Apply(b, "d2, d4",
				 "h7, h6"); // move black pawn to change turns
			possMoves = b.GetPossibleMoves(); // recalculate possible moves
			b.CurrentPlayer.Should().Be(1, "white's turn");

			var threeMovesExpected = GetMovesAtPosition(possMoves, Pos("b1"));
			threeMovesExpected.Should().Contain(Move("b1, a3"))
				 .And.Contain(Move("b1, c3"))
				 .And.Contain(Move("b1, d2"))
				 .And.HaveCount(3, "white knight starting at B1 can now move to D2");

			Apply(b, "b1, c3", // move white knight 
				 "h6, h5", // move pieces out of the way
				 "d1, d3",
				 "h5, h4",
				 "e2, e3",
				 "h4, h3",
				 "a2, a3",
				 "g7, g6");
			possMoves = b.GetPossibleMoves(); // recalculate possible moves
			b.CurrentPlayer.Should().Be(1, "white's turn");

			var eightMovesExpected = GetMovesAtPosition(possMoves, Pos("c3"));
			var count = eightMovesExpected.Count();
			eightMovesExpected.Should().Contain(Move("c3, b1"))
				 .And.Contain(Move("c3, a2"))
				 .And.Contain(Move("c3, a4"))
				 .And.Contain(Move("c3, b5"))
				 .And.Contain(Move("c3, d5"))
				 .And.Contain(Move("c3, e4"))
				 .And.Contain(Move("c3, e2"))
				 .And.Contain(Move("c3, d1"))
				 .And.HaveCount(8, "white knight has uninhibited movement");
		}

		/// <summary>
		/// At the start, the knights should be able to move to two positions jumping over the pawns.
		/// Test : - Initial Starting Board state
		/// Player: - Black
		/// Piece: - Knight
		/// Position: - b8
		/// Desired Positions: - a6, c6
		/// </summary>
		[Fact]
		public void ValidStartingMoveForKnight() {
			ChessBoard board = new ChessBoard();

			//Move a white knight so that it is black's turn
			Apply(board, "b1,a3");
			var possibleMoves = board.GetPossibleMoves();
			var initialKnightMoves = GetMovesAtPosition(possibleMoves, Pos("b8"));

			initialKnightMoves.Should().HaveCount(2, "The knight should be able to move to two different positions" +
				 "in front of the pawns");

		}

		/// <summary>
		/// At most 1 test can be about the initial starting board state. Done
		/// </summary>
		[Fact]
		public void checkTurnOneMovesForKnights() {
			ChessBoard b = CreateBoardFromMoves();

			var possMoves = b.GetPossibleMoves();
			var twoMovesExpected = GetMovesAtPosition(possMoves, Pos("b1"));
			twoMovesExpected.Should().Contain(Move("b1, a3"))
				 .And.Contain(Move("b1, c3"))
				 .And.HaveCount(2, "a knight should be able to move into two different cells on each side for their first turn");

			possMoves = b.GetPossibleMoves();
			twoMovesExpected = GetMovesAtPosition(possMoves, Pos("g1"));
			twoMovesExpected.Should().Contain(Move("g1, f3"))
				 .And.Contain(Move("g1, h3"))
				 .And.HaveCount(2, "a knight should be able to move into two different cells on each side for their first turn");

			Apply(b, Move("b1, a3"));

			possMoves = b.GetPossibleMoves();
			twoMovesExpected = GetMovesAtPosition(possMoves, Pos("b8"));
			twoMovesExpected.Should().Contain(Move("b8, a6"))
				 .And.Contain(Move("b8, c6"))
				 .And.HaveCount(2, "a knight should be able to move into two different cells on each side for their first turn");

			possMoves = b.GetPossibleMoves();
			twoMovesExpected = GetMovesAtPosition(possMoves, Pos("g8"));
			twoMovesExpected.Should().Contain(Move("g8, f6"))
				 .And.Contain(Move("g8, h6"))
				 .And.HaveCount(2, "a knight should be able to move into two different cells on each side for their first turn");
		}

		/// <summary>
		/// Checks valid Knight moves from board created.
		/// </summary>
		[Fact]
		public void CheckKnightMoves() {
			ChessBoard b = CreateBoardFromMoves(
				 "b2, b4",
				 "f7, f6",
				 "g1, f3",
				 "b8, a6",
				 "f3, e5",
				 "c7, c6",
				 "d2, d3",
				 "g7, g6"
			);

			var possMoves = b.GetPossibleMoves();
			var Player1KnightMoves = GetMovesAtPosition(possMoves, Pos("e5"));
			Player1KnightMoves.Should().HaveCount(7, "Knight has seven valid moves, one move blocked by friendly pawn");
		}

		/// <summary>
		/// Testing undo of knight capture
		/// </summary>
		[Fact]
		public void UndoKnightCapture() {
			ChessBoard b = CreateBoardFromMoves(
				"c2, c4",
				"e7, e5",
				"c4, c5",
				"e5, e4",
				"c5, c6"
			);

			var possMoves = b.GetPossibleMoves();
			var kightCaptureExpected = GetMovesAtPosition(possMoves, Pos("b8"));
			kightCaptureExpected.Should().HaveCount(2, "kight can move two different ways from b8")
				.And.Contain(Move("b8, a6"))
				.And.Contain(Move(Pos("b8"), Pos("c6"), ChessMoveType.Normal));

			// Apply the knight capture passant
			Apply(b, Move(Pos("b8"), Pos("c6"), ChessMoveType.Normal));
			var knight = b.GetPieceAtPosition(Pos("c6"));
			knight.Player.Should().Be(2, "kight performed capture");
			knight.PieceType.Should().Be(ChessPieceType.Knight, "kight performed capture");
			b.CurrentAdvantage.Should().Be(Advantage(2, 1), "player 2 captured player 1's pawn with a knight");

			// Undo the move and check the board state
			b.UndoLastMove();
			b.CurrentAdvantage.Should().Be(Advantage(0, 0), "knight capture was undone");
			knight = b.GetPieceAtPosition(Pos("b8"));
			knight.Player.Should().Be(2, "knight capture was undone");
			var captured = b.GetPieceAtPosition(Pos("c6"));
			captured.Player.Should().Be(1, "knight capture was undone");
			var boardIntegrity = b.GetPieceAtPosition(Pos("e4"));
			boardIntegrity.Player.Should().Be(2, "player 2 has a piece at e4");
			boardIntegrity.PieceType.Should().Be(ChessPieceType.Pawn, "knight capture was undone");
		}

		[Fact]
		public void KnightStartingMoves() {
			ChessBoard b = new ChessBoard();

			var possMoves = b.GetPossibleMoves();

			// Checks possible moves for the white's left knight
			var leftWhiteKnight = GetMovesAtPosition(possMoves, Pos("b1"));
			leftWhiteKnight.Should().Contain(Move("b1, a3")).And.Contain(Move("b1, c3"))
			.And.HaveCount(2, "White's left knight should have two possible moves at the start");

			Apply(b, "a2, a3");

			// moves black pawn, blocking one of the possible moves of the left black knight
			Apply(b, "c7, c6");

			Apply(b, "a3, a4");
			possMoves = b.GetPossibleMoves();

			// Checks possible moves for the black's left knight
			var leftBlackKnight = GetMovesAtPosition(possMoves, Pos("b8"));
			leftBlackKnight.Should().Contain(Move("b8, a6")).
			And.HaveCount(1, "Black's left knight should have 1 possible moves after black pawn is moved");

			b.UndoLastMove();
			b.UndoLastMove();

			possMoves = b.GetPossibleMoves();

			// Checks possible moves for the black's left knight without a pawn at c6
			leftBlackKnight = GetMovesAtPosition(possMoves, Pos("b8"));
			leftBlackKnight.Should().Contain(Move("b8, a6")).And.Contain(Move("b8,c6")).
			And.HaveCount(2, "Left black knight should have 2 possible moves after the undo");
		}

        [Fact]
        public void KingPossibleStartAndPlay_543479() {
            ChessBoard board = new ChessBoard();

            board.GetPieceAtPosition(Pos(7, 4)).PieceType.Should().Be(ChessPieceType.King, "White's king at position (7,4)");
            board.GetPieceAtPosition(Pos(0, 4)).PieceType.Should().Be(ChessPieceType.King, "Black's king at position (0,4)");

            var whiteKingPossMoves = GetMovesAtPosition(board.GetPossibleMoves(), Pos(7, 4));
            var blackKingPossMoves = GetMovesAtPosition(board.GetPossibleMoves(), Pos(0, 4));

            // Game State Check of all False
            void StateCheck1() {
                board.IsFinished.Should().BeFalse();
                board.IsStalemate.Should().BeFalse();
                board.IsCheck.Should().BeFalse();
                board.IsCheckmate.Should().BeFalse();
            }

            // Check if the King's possible moves from the very start are nothing. They should be able to ignore their own pieces within possible moves

            whiteKingPossMoves.Should().BeEmpty();
            blackKingPossMoves.Should().BeEmpty();


            // Because they do not have possible moves, but it is the intiial start of the game, there shouldn't be any flags triggered
            StateCheck1();


            // Do a similar test above, but pawns that are not within the movement range of the king's directions
            // Check that the king's possible moves is still empty
            Apply(board, "b2,b3");
            StateCheck1();
            Apply(board, "h7,h5");
            StateCheck1();

            // King shouldn't be able to move and double check the position of the King
            whiteKingPossMoves.Should().BeEmpty();
            blackKingPossMoves.Should().BeEmpty();
            board.GetPieceAtPosition(Pos("e1")).PieceType.Should().Be(ChessPieceType.King, "King should be at position e1");
            board.GetPieceAtPosition(Pos("e8")).PieceType.Should().Be(ChessPieceType.King, "King should be at position e8");

            // Move a white pawn that's in front of the King that's within its range of movement
            Apply(board, "e2,e3");
            // Move a black pawn that's in front of the King that's within its range of movement
            Apply(board, "e7,e6");

            // Check white king's possible moves, there should be "something" and apply it
            var wkMove = GetMovesAtPosition(board.GetPossibleMoves(), Pos("e1"));
            wkMove.Should().NotBeNullOrEmpty();
            Apply(board, "e1,e2");

            // Check black king's possible moves, there should be "something" and apply it
            var bkMove = GetMovesAtPosition(board.GetPossibleMoves(), Pos("e8"));
            bkMove.Should().NotBeNullOrEmpty();
            Apply(board, "e8,e7");

            board.GetPieceAtPosition(Pos("e2")).PieceType.Should().Be(ChessPieceType.King, "King should be at position e2");
            board.GetPieceAtPosition(Pos("e7")).PieceType.Should().Be(ChessPieceType.King, "King should be at position e7");

            // Move pawns in front of queen up to allow the queens to move for both sides
            Apply(board, "d2,d4");
            Apply(board, "d7,d5");
            Apply(board, "d1,d2");
            Apply(board, "d8,d7");

            // Move white Queen to check White King
            Apply(board, "d2,b4");

            // Check what the Black King can do
            bkMove = GetMovesAtPosition(board.GetPossibleMoves(), Pos("e7"));
            board.IsCheck.Should().BeTrue();
            bkMove.Should().Contain(Move("e7,e8"), "The king can move back to its starting position")
                .And.Contain(Move("e7,d8"), "The king can move to a back row")
                .And.Contain(Move("e7,f6"), "The king can move forward diagonally");

            // Move black King back to start
            Apply(board, "e7,e8");
            // Move some white pawn to get to black's turn
            Apply(board, "h2,h4");
            // Move black Queen to check White King
            Apply(board, "d7,b5");

            // Check what the White King can do
            wkMove = GetMovesAtPosition(board.GetPossibleMoves(), Pos("e2"));
            board.IsCheck.Should().BeTrue();
            wkMove.Should().Contain(Move("e2,e1"), "The king can move back to its starting position")
                .And.Contain(Move("e2,d1"), "The king can move to a back row")
                .And.Contain(Move("e2,f3"), "The king can move forward diagonally");

            // Move white King forward
            Apply(board, "e2,f3");

            // Black Queen gets White Queen
            Apply(board, "b5,b4");

            Apply(board, "c2,c3");
            Apply(board, "b4,c3");
            Apply(board, "c1,d2");
            Apply(board, "c3,d2");
            Apply(board, "b1,d2");

            Apply(board, "e6,e5");
            Apply(board, "d4,e5");
            Apply(board, "d5,d4");
            Apply(board, "e3,d4");
            Apply(board, "f7,f5");

            // Apply an EnPassat Move
            Apply(board, "e5,f6");

            Apply(board, "g7,g5");
            Apply(board, "f6,f7");

            // Move King out of Pawn's check
            board.IsCheck.Should().BeTrue();
            bkMove = GetMovesAtPosition(board.GetPossibleMoves(), Pos("e8"));
            board.IsCheck.Should().BeTrue();
            bkMove.Should().Contain(Move("e8,d8"), "The king can move back to its starting position");
            Apply(board, "e8,d8");

            // Move Pawn to Promote to Queen
            Apply(board, "f7, g8, Queen");
            board.GetPieceAtPosition(Pos("g8")).PieceType.Should().Be(ChessPieceType.Queen, "Pawn should've promoted to a Queen");

            Apply(board, "c7,c5");
            Apply(board, "h4,g5");
            Apply(board, "h5,h4");
            Apply(board, "h1,h4");

            // Black bishop check's White King
            Apply(board, "c8,g4");

            // Check if Check flag is triggered and eliminate Black Bishop on White King
            board.IsCheck.Should().BeTrue();
            var protectWK = board.GetPossibleMoves();
            protectWK.Should().Contain(Move("h4,g4"), "Rook should be able to capture enemy bishop checking White King");
            Apply(board, "h4,g4");

            // Eliminate Black Rook with White Rook
            Apply(board, "h8,h7");
            Apply(board, "g4,h4");
            Apply(board, "c5,d4");
            Apply(board, "h4,h7");

            // Play a dummy move
            Apply(board, "b8,a6");
            // White Queen takes Black Bishop
            Apply(board, "g8,f8");
            board.IsCheckmate.Should().BeTrue();
        }
    }
}
