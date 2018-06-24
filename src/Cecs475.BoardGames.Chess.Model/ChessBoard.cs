using System;
using System.Collections.Generic;
using System.Text;
using Cecs475.BoardGames.Model;
using System.Linq;

namespace Cecs475.BoardGames.Chess.Model {
	/// <summary>
	/// Represents the board state of a game of chess. Tracks which squares of the 8x8 board are occupied
	/// by which player's pieces.
	/// </summary>
	public class ChessBoard : IGameBoard {
		#region Member fields.
		// The history of moves applied to the board.
		private List<ChessMove> mMoveHistory = new List<ChessMove>();
        private List<List<CaptureSet>> mCaptureSets = new List<List<CaptureSet>>();
        private int mCurrentPlayer = 1;
        private int mAdvantageValue = 0;
        public const int BoardSize = 8;

        //Bit representation of board for all pieces and players
        private ulong mWhitePawn    = 0b00000000_00000000_00000000_00000000_00000000_00000000_11111111_00000000;
        private ulong mWhiteRook    = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_10000001;
        private ulong mWhiteKnight  = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_01000010;
        private ulong mWhiteBishop  = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00100100;
        private ulong mWhiteQueen   = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00010000;
        private ulong mWhiteKing    = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00001000;

        private ulong mBlackPawn    = 0b00000000_11111111_00000000_00000000_00000000_00000000_00000000_00000000;
        private ulong mBlackRook    = 0b10000001_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
        private ulong mBlackKnight  = 0b01000010_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
        private ulong mBlackBishop  = 0b00100100_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
        private ulong mBlackQueen   = 0b00010000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
        private ulong mBlackKing    = 0b00001000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;

        //private static sbyte[,] mPositionWeights = new sbyte[BoardSize, BoardSize]
        //{
        //    {0,0,0,0,0,0,0,0},
        //    {0,0,0,0,0,0,0,0},
        //    {0,0,0,0,0,0,0,0},
        //    {0,0,0,0,0,0,0,0},
        //    {0,0,0,0,0,0,0,0},
        //    {0,0,0,0,0,0,0,0},
        //    {0,0,0,0,0,0,0,0},
        //    {0,0,0,0,0,0,0,0}
        //};

        private struct CaptureSet
        { 
            public BoardPosition Position { get; set; }
            public ChessPiece CapturePiece { get; set; }
            public CaptureSet(ChessPiece cp, BoardPosition bp) { CapturePiece = cp; Position = bp; }
        }

        #endregion

        #region Properties

        public bool IsFinished
        {
            get {
                var gpm = GetPossibleMoves();
                return !gpm.Any() || IsDraw;
            }
        }

        public int CurrentPlayer => mCurrentPlayer == 1 ? 1 : 2;

        public GameAdvantage CurrentAdvantage { get; private set; }
        public int PassCount { get; private set; }
        public IReadOnlyList<ChessMove> MoveHistory => mMoveHistory;

        private bool check;

        // Check
        // if king is under attack, but still has moves to go with
        // check is true
        // checkmate is false (for now)
        // stalemate is false

        // if checkmate is already true, check must be false.
        public bool IsCheck {
            get 
            {
                IEnumerable<BoardPosition> kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);
                BoardPosition bp = new BoardPosition();

                if (kingPos != null && kingPos.FirstOrDefault() != null) {
                    bp = kingPos.FirstOrDefault();
                }
                bool test = PositionIsThreatened(bp, -mCurrentPlayer);

                //cannot call StalemateTest because it will lead to infinite loop
                //int checkCount = AltStaleTest(bp);
                int checkCount = StalemateTest(bp);
                int othermoves = GetPossibleMoves().Where(x => x.StartPosition != bp).Count();


                // test for attacked positions, checkCount for any square that gets us out of check, !checkmate for checkmate not triggered yet
                // removing !checkmate swaps FourMoves test case (returns empty set) with BlackCheckMate (check and checkmate are simultaneously true)
                if (test && (checkCount != 0 || othermoves != 0)) {
                    check = true;
                    checkmate = false;
                    stalemate = false;
                }
                // reset flags (?)
                else {
                    check = false;

                }

                
                return check;
            }
            set { check = value; }
        }

        private bool checkmate;
        
        public bool IsCheckmate {
            get {
                IEnumerable<BoardPosition> kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);
                BoardPosition bp = new BoardPosition();

                if (kingPos != null && kingPos.FirstOrDefault() != null) {
                    bp = kingPos.FirstOrDefault();
                }

                bool test = PositionIsThreatened(bp, -mCurrentPlayer);
                int checkCount = StalemateTest(bp);
                int othermoves = GetPossibleMoves().Where(x => x.StartPosition != bp).Count();


                if (test && checkCount == 0 && othermoves == 0) {
                    check = false;
                    checkmate = true;
                    stalemate = false;
                }
                else {          
                    checkmate = false;    
                }
                

                return checkmate;
            }
            set { checkmate = value; }
        }

        private bool stalemate;
        public bool IsStalemate {
            get {
                IEnumerable<BoardPosition> kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);
                BoardPosition bp = new BoardPosition();

                if (kingPos != null && kingPos.FirstOrDefault() != null)
                    bp = kingPos.FirstOrDefault();

                // check initial board state for king movement
                if ((mWhitePawn == 0b00000000_00000000_00000000_00000000_00000000_00000000_11111111_00000000 &&
                    mWhiteRook == 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_10000001 &&
                    mWhiteKnight == 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_01000010 &&
                    mWhiteBishop == 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00100100 &&
                    mWhiteQueen == 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00010000 &&
                    mWhiteKing == 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00001000) ||
                    (mBlackPawn == 0b00000000_11111111_00000000_00000000_00000000_00000000_00000000_00000000 &&
                    mBlackRook == 0b10000001_00000000_00000000_00000000_00000000_00000000_00000000_00000000 &&
                    mBlackKnight == 0b01000010_00000000_00000000_00000000_00000000_00000000_00000000_00000000 &&
                    mBlackBishop == 0b00100100_00000000_00000000_00000000_00000000_00000000_00000000_00000000 &&
                    mBlackQueen == 0b00010000_00000000_00000000_00000000_00000000_00000000_00000000_00000000 &&
                    mBlackKing == 0b00001000_00000000_00000000_00000000_00000000_00000000_00000000_00000000)) {
                    stalemate = false;
                    //Console.WriteLine("stalemate 1: " + stalemate);
                    //Console.WriteLine("check 1: " + check);
                    //Console.WriteLine("checkmate 1: " + checkmate);
                    return stalemate;
                }

                // Check if the Queen and King has moved from the starting position
                if ((mWhiteQueen == 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00010000 &&
                    mWhiteKing == 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00001000) ||
                    (mBlackQueen == 0b00010000_00000000_00000000_00000000_00000000_00000000_00000000_00000000 &&
                    mBlackKing == 0b00001000_00000000_00000000_00000000_00000000_00000000_00000000_00000000)) {
                    // check if the pawns in front of the king's directions has moved or not.
                    if ((mWhitePawn != 0b11111111_11111111_11111111_11111111_11111111_11111111_11100011_11111111) ||
                        (mBlackPawn != 0b11111111_11100011_11111111_11111111_11111111_11111111_11111111_11111111)) {
                        stalemate = false;
                        //Console.WriteLine("stalemate 2: " + stalemate);
                        //Console.WriteLine("check 2: " + check);
                        //Console.WriteLine("checkmate 2: " + checkmate);
                        return stalemate;
                    }
                }


                bool test = PositionIsThreatened(bp, -mCurrentPlayer);
                int checkCount = StalemateTest(bp);
                if (checkCount == 0 && !test) {
                    check = false;
                    checkmate = false;
                    stalemate = true;
                    //Console.WriteLine("stalemate test hit");
                    //Console.WriteLine("check 3: " + check);
                    //Console.WriteLine("checkmate 3: " + checkmate);
                }
                else {
                    stalemate = false;
                    //Console.WriteLine("stalemate test not hit");
                }
                return stalemate;
            }
            private set {
                stalemate = value;
            }
        }
        //only kings are on board
        public bool IsDraw => mWhiteKing   != 0 & mBlackKing   != 0 & mWhiteQueen  == 0 & mBlackQueen  == 0
                            & mWhiteBishop == 0 & mBlackBishop == 0 & mWhiteKnight == 0 & mBlackKnight == 0
                            & mWhiteRook   == 0 & mBlackRook   == 0 & mWhitePawn   == 0 & mBlackPawn   == 0
                           | DrawCounter == 100;		
		
        /// <summary>
		/// Tracks the current draw counter, which goes up by 1 for each non-capturing, non-pawn move,
        /// and resets to 0 for other moves. If the counter reaches 100 (50 full turns), the game is a draw.
		/// </summary>
		public int DrawCounter { get; private set; }
        public List<int> setDraw = new List<int>();

        #endregion

        #region Public methods.
        public static IEnumerable<BoardDirection> KnightMoves { get; }
            = new BoardDirection[] {
                    new BoardDirection(-2, -1), new BoardDirection(-2, 1),
                    new BoardDirection(-1, -2), new BoardDirection(-1, 2),
                    new BoardDirection(1, -2), new BoardDirection(1, 2),
                    new BoardDirection(2, -1), new BoardDirection(2, 1)
            };

        //private IEnumerable<ChessMove> mMoves;
        public IEnumerable<ChessMove> GetPossibleMoves() {
            //if (mMoves != null)
            //    return mMoves;

            var moves = new List<ChessMove>();
            foreach (BoardPosition bp in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
            {
                if (PositionIsEmpty(bp))
                    continue;

                //check for knight possible moves
                if(GetPieceAtPosition(bp).PieceType == ChessPieceType.Knight)
                {
                    foreach (BoardDirection dirKnight in ChessBoard.KnightMoves)
                    {
                        BoardPosition newPos = bp;
                        newPos = newPos.Translate(dirKnight);

                        if(PositionInBounds(newPos) && GetPlayerAtPosition(bp) == CurrentPlayer)
                        {
                            ChessMove cm = new ChessMove(bp, newPos, ChessMoveType.Normal,ChessPieceType.Empty);
                            AddOneMove(newPos,cm,moves);
                        }
                    }
                }
                else if(GetPieceAtPosition(bp).PieceType == ChessPieceType.Rook)
                {
                    //move horizontally or vertically
                    foreach(BoardDirection dir in BoardDirection.CardinalDirections.
                        Where(x => Math.Abs(x.RowDelta) == 1 ^ Math.Abs(x.ColDelta) == 1))
                    {
                        BoardPosition newPos = bp;
                        if(PositionInBounds(newPos))
                            AttemptAddMoves(bp, newPos, dir, moves);
                    }
                }
                else if(GetPieceAtPosition(bp).PieceType == ChessPieceType.Bishop)
                {
                    //move diagonally
                    foreach (BoardDirection dir in BoardDirection.CardinalDirections.
                        Where(x => Math.Abs(x.RowDelta) == 1 && Math.Abs(x.ColDelta) == 1))
                    {
                        BoardPosition newPos = bp;
                        if (PositionInBounds(newPos))
                            AttemptAddMoves(bp, newPos, dir, moves);
                    }
                }
                else if(GetPieceAtPosition(bp).PieceType == ChessPieceType.Queen)
                {
                    //move in all directions
                    foreach (BoardDirection dir in BoardDirection.CardinalDirections)
                    {
                        BoardPosition newPos = bp;
                        if (PositionInBounds(newPos))
                            AttemptAddMoves(bp, newPos, dir, moves);
                    }
                }
                else if(GetPieceAtPosition(bp).PieceType == ChessPieceType.King)
                {
                    BoardPosition newPosOne = bp;

                    IEnumerable<BoardPosition> kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);
                    BoardPosition bpk = new BoardPosition();

                    if (kingPos != null && kingPos.FirstOrDefault() != null)
                        bpk = kingPos.FirstOrDefault();

                    if (!PositionIsThreatened(bpk, -mCurrentPlayer) && (CurrentPlayer == 1 && newPosOne.Row == 7 || CurrentPlayer == 2 && newPosOne.Row == 0))
                        AttemptCastling(newPosOne, moves);
          
                    foreach (BoardDirection dir in BoardDirection.CardinalDirections)
                    {
                        BoardPosition newPos = bp;
                        newPos = newPos.Translate(dir);
                        if (PositionInBounds(newPos))
                        {
                            ChessMove cm = new ChessMove(bp, newPos, ChessMoveType.Normal, ChessPieceType.Empty);

                            if ((PositionIsEmpty(newPos) || PositionIsEnemy(newPos, CurrentPlayer))
                              && GetPlayerAtPosition(bp) == CurrentPlayer)
                            {
                                ApplyMove(cm);
                                mCurrentPlayer = -mCurrentPlayer;
                                 kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);
                                if (kingPos != null && kingPos.FirstOrDefault() != null)
                                    bpk = kingPos.FirstOrDefault();
                                if (!PositionIsThreatened(bpk, -mCurrentPlayer))
                                    moves.Add(cm);
                                UndoLastMove();
                                mCurrentPlayer = -mCurrentPlayer;
                            }
                        }
                    }
                }
                else if(GetPieceAtPosition(bp).PieceType == ChessPieceType.Pawn)
                {
                    //for capture moves, diagonal
                    foreach (BoardDirection oppDir in BoardDirection.CardinalDirections
                        .Where(x => Math.Abs(x.RowDelta) == 1 && Math.Abs(x.ColDelta) == 1))
                    {
                        BoardPosition newPos = bp;
                        if (oppDir.RowDelta == -1 && CurrentPlayer == 1 ||
                            oppDir.RowDelta == 1 && CurrentPlayer == 2)
                        {
                            newPos = newPos.Translate(oppDir);
                            ChessPiece oppPiece = GetPieceAtPosition(newPos);

                            if (PositionIsEnemy(newPos,CurrentPlayer) && !PositionIsEmpty(newPos)
                                && GetPlayerAtPosition(bp) == CurrentPlayer && PositionInBounds(newPos))
                            {
                                ChessMove cm;
                                if(newPos.Row == 0 && CurrentPlayer == 1 || newPos.Row == 7 && CurrentPlayer == 2)
                                {
                                    ChessMoveType cmt = ChessMoveType.PawnPromote;
                                    PawnPromotionMoves(bp, newPos, cmt, moves);
                                }
                                else
                                {
                                    cm = new ChessMove(bp, newPos, ChessMoveType.Normal,ChessPieceType.Empty);
                                    AddOneMove(newPos, cm, moves);
                                }
                            }
                        }
                    }

                    //for standard moves, vertical
                    foreach (BoardDirection dir in BoardDirection.CardinalDirections.
                        Where(x => x.ColDelta == 0))
                    {
                        BoardPosition newPos = bp;
                        if (PositionInBounds(newPos) && (GetPlayerAtPosition(bp) == 1 && dir.RowDelta == -1 ||
                            GetPlayerAtPosition(bp) == 2 && dir.RowDelta == 1))
                            AttemptPawnMove(bp, newPos, dir, moves);
                    }
                }
            }
            //mMoves = moves;
            return moves;
        }

		public void ApplyMove(ChessMove m) {
            if (m == null) {
                throw new ArgumentNullException(nameof(m));
            }

            List<CaptureSet> currentCaptures = new List<CaptureSet>();
            m.Player = CurrentPlayer;

            if (m.IsPass) {
                PassCount++;
            }
            else
            {
                PassCount = 0;
                ChessPiece cpCaptured = GetPieceAtPosition(m.EndPosition);
                ChessPiece cpMoved = GetPieceAtPosition(m.StartPosition);
                setDraw.Add(DrawCounter);

                if (cpMoved.PieceType != ChessPieceType.Pawn && cpCaptured.PieceType == ChessPieceType.Empty)
                    DrawCounter++;
                else
                    DrawCounter = 0;

                if (m.MoveType == ChessMoveType.EnPassant)
                {
                    ChessPiece pawnToTake;
                    if(CurrentPlayer == 1)
                    {
                        pawnToTake = GetPieceAtPosition(m.EndPosition.Translate(1, 0));
                        SetPieceAtPosition(m.EndPosition.Translate(1, 0), ChessPiece.Empty);
                        SetPlayerAtPosition(m.EndPosition.Translate(1, 0), 0);
                    }
                    else
                    {
                        pawnToTake = GetPieceAtPosition(m.EndPosition.Translate(-1, 0));
                        SetPieceAtPosition(m.EndPosition.Translate(-1, 0), ChessPiece.Empty);
                        SetPlayerAtPosition(m.EndPosition.Translate(-1, 0), 0);
                    }

                    if (pawnToTake.PieceType == ChessPieceType.Pawn && pawnToTake.Player != CurrentPlayer)
                    {
                        if(CurrentPlayer == 1)
                            currentCaptures.Add(new CaptureSet(pawnToTake, m.EndPosition.Translate(1,0)));
                        else
                            currentCaptures.Add(new CaptureSet(pawnToTake, m.EndPosition.Translate(-1,0)));

                        if (pawnToTake.Player == 2)
                            ChangeAdvantage(1);
                        else
                            ChangeAdvantage(-1);
                    }
                }

                // Otherwise update the board at the move's position with the current player and piece.
                if (m.MoveType == ChessMoveType.PawnPromote)
                {
                    if (m.Player == 1)
                        ChangeAdvantage(-1);
                    else
                        ChangeAdvantage(1);
                    SetPlayerAtPosition(m.EndPosition, CurrentPlayer);
                    SetPieceAtPosition(m.EndPosition, new ChessPiece(m.PromotionPiece, CurrentPlayer));

                    switch (m.PromotionPiece)
                    {
                        case ChessPieceType.Rook:
                            if (CurrentPlayer == 1)
                                ChangeAdvantage(5);
                            else
                                ChangeAdvantage(-5);
                            break;
                        case ChessPieceType.Knight:
                            if (CurrentPlayer == 1)
                                ChangeAdvantage(3);
                            else
                                ChangeAdvantage(-3);
                            break;
                        case ChessPieceType.Bishop:
                            if (CurrentPlayer == 1)
                                ChangeAdvantage(3);
                            else
                                ChangeAdvantage(-3);
                            break;
                        case ChessPieceType.Queen:
                            if (CurrentPlayer == 1)
                                ChangeAdvantage(9);
                            else
                                ChangeAdvantage(-9);
                            break;
                    }
                }
                else if(m.MoveType == ChessMoveType.CastleKingSide || m.MoveType == ChessMoveType.CastleQueenSide)
                {
                    IEnumerable<BoardPosition> oldRookSpot = GetPositionsOfPiece(ChessPieceType.Rook, CurrentPlayer);
                    if (m.MoveType == ChessMoveType.CastleQueenSide)
                    {
                        BoardPosition newRookSpot = new BoardPosition(m.EndPosition.Row, m.EndPosition.Col + 1);

                        SetPieceAtPosition(newRookSpot, new ChessPiece(ChessPieceType.Rook, CurrentPlayer));
                        //SetPlayerAtPosition(newRookSpot, CurrentPlayer);

                        SetPieceAtPosition(oldRookSpot.First(), ChessPiece.Empty);
                        SetPlayerAtPosition(oldRookSpot.First(), 0);
                    }
                    else if (m.MoveType == ChessMoveType.CastleKingSide)
                    {
                        BoardPosition newRookSpot = new BoardPosition(m.EndPosition.Row, m.EndPosition.Col - 1);

                        SetPieceAtPosition(newRookSpot, new ChessPiece(ChessPieceType.Rook, CurrentPlayer));
                        //SetPlayerAtPosition(newRookSpot, CurrentPlayer);

                        SetPieceAtPosition(oldRookSpot.Last(), ChessPiece.Empty);
                        SetPlayerAtPosition(oldRookSpot.Last(), 0);
                    }
                    SetPlayerAtPosition(m.EndPosition, cpMoved.Player);
                    SetPieceAtPosition(m.EndPosition, cpMoved);
                }
                else
                {
                    SetPlayerAtPosition(m.EndPosition, cpMoved.Player);
                    SetPieceAtPosition(m.EndPosition, cpMoved);
                }

                SetPlayerAtPosition(m.StartPosition, 0);
                SetPieceAtPosition(m.StartPosition, ChessPiece.Empty);

                //check if there is actually a piece to capture
                if (cpCaptured.PieceType != ChessPieceType.Empty && cpCaptured.Player != 0)
                {
                    currentCaptures.Add(new CaptureSet(cpCaptured, m.EndPosition));

                    switch (cpCaptured.PieceType)
                    {
                        case ChessPieceType.Pawn:
                            if (cpCaptured.Player == 2)
                                ChangeAdvantage(1);
                            else
                                ChangeAdvantage(-1);
                            break;
                        case ChessPieceType.Rook:
                            if (cpCaptured.Player == 2)
                                ChangeAdvantage(5);
                            else
                                ChangeAdvantage(-5);
                            break;
                        case ChessPieceType.Knight:
                            if (cpCaptured.Player == 2)
                                ChangeAdvantage(3);
                            else
                                ChangeAdvantage(-3);
                            break;
                        case ChessPieceType.Bishop:
                            if (cpCaptured.Player == 2)
                                ChangeAdvantage(3);
                            else
                                ChangeAdvantage(-3);
                            break;
                        case ChessPieceType.Queen:
                            if (cpCaptured.Player == 2)
                                ChangeAdvantage(9);
                            else
                                ChangeAdvantage(-9);
                            break;
                    }
                }
            }

            // Update the rest of the board state.
            SetAdvantage();
            mCurrentPlayer = -mCurrentPlayer;
            mMoveHistory.Add(m);
            mCaptureSets.Add(currentCaptures);
            //mMoves = null;
        }

		public void UndoLastMove() {
            ChessMove m = mMoveHistory.Last();

            if (!m.IsPass) {
                BoardPosition capturePos;
                if (m.MoveType == ChessMoveType.EnPassant && m.Player == 1)
                    capturePos = new BoardPosition(m.EndPosition.Row + 1, m.EndPosition.Col);
                else if (m.MoveType == ChessMoveType.EnPassant && m.Player == 2)
                    capturePos = new BoardPosition(m.EndPosition.Row - 1, m.EndPosition.Col);
                else
                    capturePos = m.EndPosition;


                IEnumerable<CaptureSet> cs = mCaptureSets.Last().Where(x => x.Position == capturePos);

                if (cs.Count() != 0)
                {
                    ChessPiece cp = GetPieceAtPosition(m.EndPosition);
                    DrawCounter = setDraw.Last();

                    foreach (var captureSet in cs)
                    {
                        if (m.MoveType == ChessMoveType.PawnPromote)
                        {
                            if (m.Player == 1)
                                ChangeAdvantage(1);
                            else
                                ChangeAdvantage(-1);

                            SetPlayerAtPosition(m.StartPosition, cp.Player);
                            SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Pawn, cp.Player));

                            switch (m.PromotionPiece)
                            {
                                case ChessPieceType.Rook:
                                    if (m.Player == 2)
                                        ChangeAdvantage(5);
                                    else
                                        ChangeAdvantage(-5);
                                    break;
                                case ChessPieceType.Knight:
                                    if (m.Player == 2)
                                        ChangeAdvantage(3);
                                    else
                                        ChangeAdvantage(-3);
                                    break;
                                case ChessPieceType.Bishop:
                                    if (m.Player == 2)
                                        ChangeAdvantage(3);
                                    else
                                        ChangeAdvantage(-3);
                                    break;
                                case ChessPieceType.Queen:
                                    if (m.Player == 2)
                                        ChangeAdvantage(9);
                                    else
                                        ChangeAdvantage(-9);
                                    break;
                            }
                        }
                        else
                        {
                            SetPlayerAtPosition(m.StartPosition, cp.Player);
                            SetPieceAtPosition(m.StartPosition, cp);
                        }

                        if(m.MoveType == ChessMoveType.EnPassant)
                        {
                            SetPlayerAtPosition(m.EndPosition, 0);
                            SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Empty,0));
                        }

                        SetPlayerAtPosition(captureSet.Position, captureSet.CapturePiece.Player);
                        SetPieceAtPosition(captureSet.Position, captureSet.CapturePiece);

                        switch (captureSet.CapturePiece.PieceType)
                        {
                            //if captured piece was from P1, P1 will gain points
                            //else P1 will gain points (aka P1 will lose points)
                            case ChessPieceType.Pawn:
                                if (captureSet.CapturePiece.Player == 1)
                                    ChangeAdvantage(1);
                                else
                                    ChangeAdvantage(-1);
                                break;
                            case ChessPieceType.Rook:
                                if (captureSet.CapturePiece.Player == 1)
                                    ChangeAdvantage(5);
                                else
                                    ChangeAdvantage(-5);
                                break;
                            case ChessPieceType.Knight:
                                if (captureSet.CapturePiece.Player == 1)
                                    ChangeAdvantage(3);
                                else
                                    ChangeAdvantage(-3);
                                break;
                            case ChessPieceType.Bishop:
                                if (captureSet.CapturePiece.Player == 1)
                                    ChangeAdvantage(3);
                                else
                                    ChangeAdvantage(-3);
                                break;
                            case ChessPieceType.Queen:
                                if (captureSet.CapturePiece.Player == 1)
                                    ChangeAdvantage(9);
                                else
                                    ChangeAdvantage(-9);
                                break;
                        }
                    }
                }
                else
                {
                    ChessPiece cp = GetPieceAtPosition(m.EndPosition);
                    DrawCounter = setDraw.Last();

                    if (m.MoveType == ChessMoveType.PawnPromote)
                    {
                        if (m.Player == 1)
                            ChangeAdvantage(1);
                        else
                            ChangeAdvantage(-1);

                        SetPlayerAtPosition(m.StartPosition, cp.Player);
                        SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Pawn, cp.Player));

                        switch (m.PromotionPiece)
                        {
                            case ChessPieceType.Rook:
                                if (m.Player == 2)
                                    ChangeAdvantage(5);
                                else
                                    ChangeAdvantage(-5);
                                break;
                            case ChessPieceType.Knight:
                                if (m.Player == 2)
                                    ChangeAdvantage(3);
                                else
                                    ChangeAdvantage(-3);
                                break;
                            case ChessPieceType.Bishop:
                                if (m.Player == 2)
                                    ChangeAdvantage(3);
                                else
                                    ChangeAdvantage(-3);
                                break;
                            case ChessPieceType.Queen:
                                if (m.Player == 2)
                                    ChangeAdvantage(9);
                                else
                                    ChangeAdvantage(-9);
                                break;
                        }
                    }
                    else if(m.MoveType == ChessMoveType.CastleQueenSide)
                    {
                        SetPlayerAtPosition(m.StartPosition, cp.Player);
                        SetPieceAtPosition(m.StartPosition, cp);

                        ChessPiece rook = new ChessPiece(ChessPieceType.Rook, cp.Player);
                        BoardPosition oldPos = new BoardPosition();
                        if (cp.Player == 1)
                            oldPos = new BoardPosition(7, 0);
                        else
                            oldPos = new BoardPosition(0, 0);

                        BoardPosition currentPos = new BoardPosition(m.EndPosition.Row, m.EndPosition.Col + 1);
                        SetPlayerAtPosition(currentPos, 0);
                        SetPieceAtPosition(currentPos, ChessPiece.Empty);

                        SetPlayerAtPosition(oldPos, rook.Player);
                        SetPieceAtPosition(oldPos, rook);
                    }
                    else if(m.MoveType == ChessMoveType.CastleKingSide)
                    {
                        SetPlayerAtPosition(m.StartPosition, cp.Player);
                        SetPieceAtPosition(m.StartPosition, cp);

                        ChessPiece rook = new ChessPiece(ChessPieceType.Rook, cp.Player);
                        BoardPosition oldPos = new BoardPosition();
                        if (cp.Player == 1)
                            oldPos = new BoardPosition(7, 7);
                        else
                            oldPos = new BoardPosition(0, 7);

                        BoardPosition currentPos = new BoardPosition(m.EndPosition.Row, m.EndPosition.Col - 1);
                        SetPlayerAtPosition(currentPos, 0);
                        SetPieceAtPosition(currentPos, ChessPiece.Empty);

                        SetPlayerAtPosition(oldPos, rook.Player);
                        SetPieceAtPosition(oldPos, rook);
                    }
                    else if(m.MoveType == ChessMoveType.Normal)
                    {
                        SetPlayerAtPosition(m.StartPosition, cp.Player);
                        SetPieceAtPosition(m.StartPosition, cp);
                    }

                    SetPlayerAtPosition(m.EndPosition, 0);
                    SetPieceAtPosition(m.EndPosition, ChessPiece.Empty);
                }
            }
            else {
                PassCount--;
            }
            // Reset the remaining game state.
            SetAdvantage();
            mCurrentPlayer = -mCurrentPlayer;
            mMoveHistory.RemoveAt(mMoveHistory.Count - 1);
            mCaptureSets.RemoveAt(mCaptureSets.Count - 1);
            setDraw.RemoveAt(setDraw.Count - 1);
            //mMoves = null;
        }

        /// <summary>
        /// Returns whatever chess piece is occupying the given position.
        /// </summary>
        public ChessPiece GetPieceAtPosition(BoardPosition position) {
            int index = GetBitIndexForPosition(position);
            ulong mask = 1UL << index;

            //compare placement of desired black piece to placement of existing black pieces
            //if bit board is equal to the bit board ORed with mask, then piece exists there
            if (mBlackPawn.CompareTo(mBlackPawn | mask) == 0)
                return new ChessPiece(ChessPieceType.Pawn, 2);

            else if (mBlackRook.CompareTo(mBlackRook | mask) == 0)
                return new ChessPiece(ChessPieceType.Rook, 2);

            else if (mBlackKnight.CompareTo(mBlackKnight | mask) == 0)
                return new ChessPiece(ChessPieceType.Knight, 2);

            else if (mBlackBishop.CompareTo(mBlackBishop | mask) == 0)
                return new ChessPiece(ChessPieceType.Bishop, 2);

            else if (mBlackQueen.CompareTo(mBlackQueen | mask) == 0)
                return new ChessPiece(ChessPieceType.Queen, 2);

            else if (mBlackKing.CompareTo(mBlackKing | mask) == 0)
                return new ChessPiece(ChessPieceType.King, 2);

            //compare placement of desired white piece to placement of existing white pieces
            //if bit board is equal to the bit board ORed with position, then piece exists there
            else if (mWhitePawn.CompareTo(mWhitePawn | mask) == 0)
                return new ChessPiece(ChessPieceType.Pawn, 1);

            else if (mWhiteRook.CompareTo(mWhiteRook | mask) == 0)
                return new ChessPiece(ChessPieceType.Rook, 1);

            else if (mWhiteKnight.CompareTo(mWhiteKnight | mask) == 0)
                return new ChessPiece(ChessPieceType.Knight, 1);

            else if (mWhiteBishop.CompareTo(mWhiteBishop | mask) == 0)
                return new ChessPiece(ChessPieceType.Bishop, 1);

            else if (mWhiteQueen.CompareTo(mWhiteQueen | mask) == 0)
                return new ChessPiece(ChessPieceType.Queen, 1);

            else if (mWhiteKing.CompareTo(mWhiteKing | mask) == 0)
                return new ChessPiece(ChessPieceType.King, 1);
            else
                return new ChessPiece(ChessPieceType.Empty, 0);
        }

		/// <summary>
		/// Returns whatever player is occupying the given position.
		/// </summary>
		public int GetPlayerAtPosition(BoardPosition pos) {
            ChessPiece cp = GetPieceAtPosition(pos);

            if (cp.Player == 1)
                return 1;
            else if (cp.Player == 2)
                return 2;
            else
                return 0;
        }

        /// <summary>
        /// Returns true if the given position on the board is empty.
        /// </summary>
        /// <remarks>returns false if the position is not in bounds</remarks>
        public bool PositionIsEmpty(BoardPosition pos) => GetPlayerAtPosition(pos) == 0;

        /// <summary>
        /// Returns true if the given position contains a piece that is the enemy of the given player.
        /// </summary>
        /// <remarks>returns false if the position is not in bounds</remarks>
        public bool PositionIsEnemy(BoardPosition pos, int player) => GetPlayerAtPosition(pos) + player == 3;

        /// <summary>
        /// Returns true if the given position is in the bounds of the board.
        /// </summary>
        public static bool PositionInBounds(BoardPosition pos) => pos.Row >= 0 && pos.Row <= 7 && pos.Col >= 0 && pos.Col <= 7;

		/// <summary>
		/// Returns all board positions where the given piece can be found.
		/// </summary>
		public IEnumerable<BoardPosition> GetPositionsOfPiece(ChessPieceType piece, int player) {
            var list = new List<BoardPosition>();

            if (player == -1)
                player = 2;
            switch(player)
            {
                case 1:
                    switch (piece)
                    {
                        case ChessPieceType.Pawn:
                            AddPiecePositions(list, mWhitePawn);
                            break;
                        case ChessPieceType.Rook:
                            AddPiecePositions(list, mWhiteRook);
                            break;
                        case ChessPieceType.Knight:
                            AddPiecePositions(list, mWhiteKnight);
                            break;
                        case ChessPieceType.Bishop:
                            AddPiecePositions(list, mWhiteBishop);
                            break;
                        case ChessPieceType.Queen:
                            AddPiecePositions(list, mWhiteQueen);
                            break;
                        case ChessPieceType.King:
                            AddPiecePositions(list, mWhiteKing);
                            break;
                    }
                    break;
                case 2:
                    switch (piece)
                    {
                        case ChessPieceType.Pawn:
                            AddPiecePositions(list, mBlackPawn);
                            break;
                        case ChessPieceType.Rook:
                            AddPiecePositions(list, mBlackRook);
                            break;
                        case ChessPieceType.Knight:
                            AddPiecePositions(list, mBlackKnight);
                            break;
                        case ChessPieceType.Bishop:
                            AddPiecePositions(list, mBlackBishop);
                            break;
                        case ChessPieceType.Queen:
                            AddPiecePositions(list, mBlackQueen);
                            break;
                        case ChessPieceType.King:
                            AddPiecePositions(list, mBlackKing);
                            break;
                    }
                    break;
                default:
                    break;
            }
            return list;
        }

		/// <summary>
		/// Returns true if the given player's pieces are attacking the given position.
		/// </summary>
		public bool PositionIsThreatened(BoardPosition position, int byPlayer) {
            if (byPlayer == -1)
                byPlayer = 2;

            ChessPiece cp = GetPieceAtPosition(position);
            ISet<BoardPosition> bps = GetAttackedPositions(byPlayer);            
            return (bps.Contains(position)) ? true : false;
		}

		/// <summary>
		/// Returns a set of all BoardPositions that are attacked by the given player.
		/// </summary>
		public ISet<BoardPosition> GetAttackedPositions(int byPlayer) {
            if (byPlayer == -1)
                byPlayer = 2;
            var positions = new HashSet<BoardPosition>();

            foreach (BoardPosition bp in BoardPosition.GetRectangularPositions(BoardSize, BoardSize)) {

                //checks if a non-opponent piece is at a position
                if (PositionIsEmpty(bp)) {
                    continue;
                }

                ChessPiece opposingPiece = GetPieceAtPosition(bp);
                if(opposingPiece.Player == byPlayer)
                {
                    if (opposingPiece.PieceType == ChessPieceType.Knight)
                    {
                        // Iterate through all 8 knight move directions to find any knight pieces
                        foreach (BoardDirection dirKnight in ChessBoard.KnightMoves)
                        {
                            BoardPosition newPos = bp;
                            newPos = newPos.Translate(dirKnight);

                            if (PositionIsEnemy(newPos,byPlayer) && !PositionIsEmpty(newPos)
                                && GetPlayerAtPosition(bp) == byPlayer && PositionInBounds(newPos))
                                positions.Add(newPos);
                        }
                    }
                    else if(opposingPiece.PieceType == ChessPieceType.Pawn)
                    {
                        foreach(BoardDirection dir in BoardDirection.CardinalDirections.Where(x => Math.Abs(x.RowDelta) == 1 && Math.Abs(x.ColDelta) == 1))
                        {
                            BoardPosition newPos = bp;
                            newPos = newPos.Translate(dir);

                            if (PositionIsEnemy(newPos, byPlayer) && !PositionIsEmpty(newPos)
                                && GetPlayerAtPosition(bp) == byPlayer && PositionInBounds(newPos))
                                positions.Add(newPos);
                        }
                    }
                    else if (opposingPiece.PieceType == ChessPieceType.Rook)
                    {
                        foreach (BoardDirection dir in BoardDirection.CardinalDirections.Where(x => Math.Abs(x.RowDelta) == 1 ^ Math.Abs(x.ColDelta) == 1))
                        {
                            BoardPosition newPos = bp;

                            do
                            {
                                newPos = newPos.Translate(dir);
                                if (GetPlayerAtPosition(newPos) == byPlayer)
                                    break;
                            } while (!PositionIsEnemy(newPos, byPlayer) && PositionInBounds(newPos));


                            if (PositionIsEnemy(newPos, byPlayer) && !PositionIsEmpty(newPos)
                                && GetPlayerAtPosition(bp) == byPlayer && PositionInBounds(newPos))
                                positions.Add(newPos);
                        }
                    }
                    else if (opposingPiece.PieceType == ChessPieceType.Bishop)
                    {
                        foreach (BoardDirection dir in BoardDirection.CardinalDirections.Where(x => Math.Abs(x.RowDelta) == 1 & Math.Abs(x.ColDelta) == 1))
                        {
                            BoardPosition newPos = bp;

                            do
                            {
                                newPos = newPos.Translate(dir);
                                if (GetPlayerAtPosition(newPos) == byPlayer)
                                    break;
                            } while (!PositionIsEnemy(newPos, byPlayer) && PositionInBounds(newPos));

                            if (PositionIsEnemy(newPos, byPlayer) && !PositionIsEmpty(newPos)
                                && GetPlayerAtPosition(bp) == byPlayer && PositionInBounds(newPos))
                                positions.Add(newPos);
                        }
                    }
                    else if (opposingPiece.PieceType == ChessPieceType.Queen)
                    {
                        foreach (BoardDirection dir in BoardDirection.CardinalDirections)
                        {
                            BoardPosition newPos = bp;

                            do
                            {
                                newPos = newPos.Translate(dir);
                                if (GetPlayerAtPosition(newPos) == byPlayer)
                                    break;
                            } while (!PositionIsEnemy(newPos, byPlayer) && PositionInBounds(newPos));

                            if (PositionIsEnemy(newPos, byPlayer) && !PositionIsEmpty(newPos)
                                && GetPlayerAtPosition(bp) == byPlayer && PositionInBounds(newPos))
                                positions.Add(newPos);
                        }
                    }
                    else if (opposingPiece.PieceType == ChessPieceType.King)
                    {
                        foreach (BoardDirection dir in BoardDirection.CardinalDirections)
                        {
                            BoardPosition newPos = bp;
                            newPos = newPos.Translate(dir);

                            if (PositionIsEnemy(newPos, byPlayer) && !PositionIsEmpty(newPos)
                                && GetPlayerAtPosition(bp) == byPlayer && PositionInBounds(newPos))
                                positions.Add(newPos);
                        }
                    }
                }
            }

            return positions;
        }
        #endregion

        #region Private methods.

        private bool SetCheck(BoardPosition bp)
        {
            bool test = PositionIsThreatened(bp, -mCurrentPlayer);
            if (test)
                return true;
            else
                return false;
        }

        private int AltStaleTest(BoardPosition bp)
        {
            int stalemateCount = 0;

            foreach (BoardDirection dir in BoardDirection.CardinalDirections)
            {
                BoardPosition newPos = bp;
                newPos = newPos.Translate(dir);
                ChessPiece cp = GetPieceAtPosition(newPos);

                if (PositionInBounds(newPos) && (PositionIsEmpty(newPos) || PositionIsEnemy(newPos, CurrentPlayer)))
                {
                    ChessMove cm = new ChessMove(bp, newPos, ChessMoveType.Normal, ChessPieceType.Empty);
                    PartApply(cm);
                    bool sc = SetCheck(bp);

                    if (!sc)
                        stalemateCount++;

                    PartUndo();
                }

            }
            return stalemateCount;
        }

        private void AttemptCastling(BoardPosition bp, List<ChessMove> moves)
        {
            IEnumerable<BoardPosition> cprooks = GetPositionsOfPiece(ChessPieceType.Rook, CurrentPlayer).Where(x => x.Row == bp.Row);
            bool isFree = false;
            foreach (BoardPosition rookPos in cprooks)
            {
                if (bp.Col > rookPos.Col)
                {
                    for (int i = bp.Col - 1; i > rookPos.Col; i--)
                    {
                        BoardPosition findBlank = new BoardPosition(bp.Row, i);
                        ChessPiece cp = GetPieceAtPosition(findBlank);
                        isFree = (cp.PieceType == ChessPieceType.Empty) ? true : false;
                        if (!isFree)
                            break;
                    }
                }
                else
                {
                    for (int i = bp.Col + 1; i < rookPos.Col; i++)
                    {
                        BoardPosition findBlank = new BoardPosition(bp.Row, i);
                        ChessPiece cp = GetPieceAtPosition(findBlank);
                        isFree = (cp.PieceType == ChessPieceType.Empty) ? true : false;
                        if (!isFree)
                            break;
                    }
                }

                //check if king and rook have not moved (not part of move history)
                IEnumerable<ChessMove> kingHis = MoveHistory.Where(move => move.EndPosition == bp);
                IEnumerable<ChessMove> rookHis = MoveHistory.Where(move => move.EndPosition == rookPos);

                IEnumerable<BoardPosition> kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);
                BoardPosition bpk = new BoardPosition();

                if (kingPos != null && kingPos.FirstOrDefault() != null)
                    bpk = kingPos.FirstOrDefault();

                if (!PositionIsThreatened(bpk, -mCurrentPlayer) && isFree && kingHis.Count() == 0 && rookHis.Count() == 0)
                {
                    if (bp.Col > rookPos.Col)
                    {
                        BoardPosition newPos = bp;
                        newPos = new BoardPosition(bp.Row, bp.Col - 2);
                        BoardPosition newPosTemp = new BoardPosition(bp.Row, bp.Col - 1);
                        ChessMove cmOne;
                        if (newPos.Col >= 0 && newPos.Col <= 3 && GetPieceAtPosition(newPos).PieceType == ChessPieceType.Empty)
                        {
                            bool advance = false;
                            cmOne = new ChessMove(bp, newPosTemp, ChessMoveType.Normal, ChessPieceType.Empty);
                            ApplyMove(cmOne);
                            mCurrentPlayer = -mCurrentPlayer;

                            kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);

                            if (kingPos != null && kingPos.FirstOrDefault() != null)
                                bpk = kingPos.FirstOrDefault();

                            if (!PositionIsThreatened(bpk, -mCurrentPlayer))
                                advance = true;
                            UndoLastMove();
                            mCurrentPlayer = -mCurrentPlayer;


                            if (advance)
                            {
                                cmOne = new ChessMove(bp, newPos, ChessMoveType.CastleQueenSide, ChessPieceType.Empty);
                                ApplyMove(cmOne);
                                mCurrentPlayer = -mCurrentPlayer;

                                kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);

                                if (kingPos != null && kingPos.FirstOrDefault() != null)
                                    bpk = kingPos.FirstOrDefault();

                                if (!PositionIsThreatened(bpk, -mCurrentPlayer))
                                    moves.Add(cmOne);
                                UndoLastMove();
                                mCurrentPlayer = -mCurrentPlayer;
                            }
                        }
                    }
                    else
                    {
                        BoardPosition newPos = bp;
                        newPos = new BoardPosition(bp.Row, bp.Col + 2);
                        BoardPosition newPosTemp = new BoardPosition(bp.Row, bp.Col + 1);
                        ChessMove cmOne;
                        if (newPos.Col >= 4 && newPos.Col <= 7 && GetPieceAtPosition(newPos).PieceType == ChessPieceType.Empty)
                        {
                            bool advance = false;
                            cmOne = new ChessMove(bp, newPosTemp, ChessMoveType.Normal, ChessPieceType.Empty);
                            ApplyMove(cmOne);
                            mCurrentPlayer = -mCurrentPlayer;

                            kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);

                            if (kingPos != null && kingPos.FirstOrDefault() != null)
                                bpk = kingPos.FirstOrDefault();

                            if (!PositionIsThreatened(bpk, -mCurrentPlayer))
                                advance = true;
                            UndoLastMove();
                            mCurrentPlayer = -mCurrentPlayer;

                            if (advance)
                            {
                                cmOne = new ChessMove(bp, newPos, ChessMoveType.CastleKingSide, ChessPieceType.Empty);
                                ApplyMove(cmOne);
                                mCurrentPlayer = -mCurrentPlayer;

                                kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);

                                if (kingPos != null && kingPos.FirstOrDefault() != null)
                                    bpk = kingPos.FirstOrDefault();

                                if (!PositionIsThreatened(bpk, -mCurrentPlayer))
                                    moves.Add(cmOne);
                                UndoLastMove();
                                mCurrentPlayer = -mCurrentPlayer;
                            }
                        }
                    }
                }
            }
        }

        private void AddOneMove(BoardPosition newPos, ChessMove cm, List<ChessMove> moves)
        {
            if (PositionIsEmpty(newPos) || PositionIsEnemy(newPos, CurrentPlayer))
            {
                ApplyMove(cm);
                mCurrentPlayer = -mCurrentPlayer;

                IEnumerable<BoardPosition> kingPos = GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer);
                BoardPosition bpk = new BoardPosition();

                if (kingPos != null && kingPos.FirstOrDefault() != null)
                    bpk = kingPos.FirstOrDefault();
                if (!PositionIsThreatened(bpk, -mCurrentPlayer))
                    moves.Add(cm);
                UndoLastMove();
                mCurrentPlayer = -mCurrentPlayer;
            }
        }

        private void EnPassantMove(ChessMoveType cmt, BoardPosition bp,
            BoardPosition bp_opp, List<ChessMove> moves)
        {
            BoardPosition potential_pos = bp_opp;
            if (GetPlayerAtPosition(potential_pos) == 2)
                //black to be captured
                potential_pos = bp_opp.Translate(new BoardDirection(-1, 0));
            else
                // white to be captured
                potential_pos = bp_opp.Translate(new BoardDirection(1, 0));

            if (PositionInBounds(potential_pos) && !PositionIsEmpty(bp_opp) && GetPlayerAtPosition(bp) == CurrentPlayer)
            {
                cmt = ChessMoveType.EnPassant;
                ChessMove cm = new ChessMove(bp, potential_pos, cmt, ChessPieceType.Empty);
                AddOneMove(potential_pos, cm, moves);
            }
        }

        private void PawnPromotionMoves(BoardPosition bp, BoardPosition newPos,
            ChessMoveType cmt, List<ChessMove> moves)
        {
            List<ChessMove> ppm = new List<ChessMove>();
            ChessMove cmOne = new ChessMove(bp, newPos, cmt, ChessPieceType.Rook);
            ChessMove cmTwo = new ChessMove(bp, newPos, cmt, ChessPieceType.Knight);
            ChessMove cmThree = new ChessMove(bp, newPos, cmt, ChessPieceType.Bishop);
            ChessMove cmFour = new ChessMove(bp, newPos, cmt, ChessPieceType.Queen);
            ppm.Add(cmOne);
            ppm.Add(cmTwo);
            ppm.Add(cmThree);
            ppm.Add(cmFour);

            foreach (ChessMove pp in ppm)
                AddOneMove(newPos, pp, moves);
        }

        private void AttemptPawnMove(BoardPosition bp,
            BoardPosition newPos, BoardDirection dir, List<ChessMove> moves)
        {
            ChessMoveType cmt = ChessMoveType.Normal;
            int step = 1;
            int increment = 0;
            if (bp.Row == 6 && CurrentPlayer == 1
              || bp.Row == 1 && CurrentPlayer == 2)
                step = 2;

            if (GetPlayerAtPosition(bp) == 1 && bp.Row == 3  // about to capture adjacent black pawn
              || GetPlayerAtPosition(bp) == 2 && bp.Row == 4) // about to capture adjacent white pawn
            {
                BoardPosition bpcheck = new BoardPosition(bp.Row, bp.Col - 1);
                BoardPosition bpcheck2 = new BoardPosition(bp.Row, bp.Col + 1);
                ChessMove m = new ChessMove(new BoardPosition(), new BoardPosition(), ChessMoveType.Normal, ChessPieceType.Empty);
                if (mMoveHistory != null & mMoveHistory.LastOrDefault() != null)
                    m = mMoveHistory.LastOrDefault();

                if (GetPieceAtPosition(bpcheck).PieceType == ChessPieceType.Pawn
                && GetPlayerAtPosition(bpcheck) != CurrentPlayer && m.EndPosition == bpcheck)
                    EnPassantMove(cmt, bp, bpcheck, moves);
                else if (GetPieceAtPosition(bpcheck2).PieceType == ChessPieceType.Pawn
                && GetPlayerAtPosition(bpcheck2) != CurrentPlayer && m.EndPosition == bpcheck2)
                    EnPassantMove(cmt, bp, bpcheck2, moves);
            }

            while (increment < step)
            {
                newPos = newPos.Translate(dir);
                if (!PositionIsEmpty(newPos))
                    break;

                //pawn promotion rule
                if (GetPlayerAtPosition(bp) == CurrentPlayer && PositionIsEmpty(newPos)
                   && (newPos.Row == 0 && CurrentPlayer == 1 || newPos.Row == 7 && CurrentPlayer == 2))
                {
                    cmt = ChessMoveType.PawnPromote;
                    PawnPromotionMoves(bp, newPos, cmt, moves);
                }
                else if (GetPlayerAtPosition(bp) == CurrentPlayer && PositionIsEmpty(newPos))
                {
                    ChessMove cm = new ChessMove(bp, newPos, cmt, ChessPieceType.Empty);
                    AddOneMove(newPos, cm, moves);
                }
                increment++;
            }

        }

        private void AttemptAddMoves(BoardPosition bp, BoardPosition newPos, BoardDirection dir, List<ChessMove> moves)
        {
            do
            {
                newPos = newPos.Translate(dir);

                if (GetPlayerAtPosition(bp) == CurrentPlayer && PositionInBounds(newPos))
                {
                    ChessMove cm = new ChessMove(bp, newPos, ChessMoveType.Normal, ChessPieceType.Empty);
                    AddOneMove(newPos, cm, moves);
                }
                if (GetPlayerAtPosition(newPos) == CurrentPlayer)
                    break;
            } while (!PositionIsEnemy(newPos, CurrentPlayer) && PositionInBounds(newPos));
        }

        private int StalemateTest(BoardPosition bp)
        {
            IEnumerable<ChessMove> kingMove = GetPossibleMoves().Where(x => x.StartPosition == bp);
            return kingMove.Count();
        }

        private void PartUndo()
        {
            ChessMove m = mMoveHistory.Last();
            BoardPosition capturePos = m.EndPosition;
            IEnumerable<CaptureSet> cs = mCaptureSets.Last().Where(x => x.Position == capturePos);

            if (cs.Count() != 0)
            {
                ChessPiece cp = GetPieceAtPosition(m.EndPosition);

                foreach (var captureSet in cs)
                {
                    SetPlayerAtPosition(m.StartPosition, cp.Player);
                    SetPieceAtPosition(m.StartPosition, cp);

                    SetPlayerAtPosition(captureSet.Position, captureSet.CapturePiece.Player);
                    SetPieceAtPosition(captureSet.Position, captureSet.CapturePiece);
                }
            }
            else
            {
                ChessPiece cp = GetPieceAtPosition(m.EndPosition);

                SetPlayerAtPosition(m.StartPosition, cp.Player);
                SetPieceAtPosition(m.StartPosition, cp);

                SetPlayerAtPosition(m.EndPosition, 0);
                SetPieceAtPosition(m.EndPosition, ChessPiece.Empty);
            }

            SetAdvantage();
            mMoveHistory.RemoveAt(mMoveHistory.Count - 1);
            mCaptureSets.RemoveAt(mCaptureSets.Count - 1);
        }

        private void PartApply(ChessMove m)
        {
            if (m == null)
            {
                throw new ArgumentNullException(nameof(m));
            }

            List<CaptureSet> currentCaptures = new List<CaptureSet>();
            m.Player = CurrentPlayer;

            PassCount = 0;
            ChessPiece cpCaptured = GetPieceAtPosition(m.EndPosition);
            ChessPiece cpMoved = GetPieceAtPosition(m.StartPosition);

            SetPlayerAtPosition(m.EndPosition, cpMoved.Player);
            SetPieceAtPosition(m.EndPosition, cpMoved);

            SetPlayerAtPosition(m.StartPosition, 0);
            SetPieceAtPosition(m.StartPosition, ChessPiece.Empty);

            if (cpCaptured.PieceType != ChessPieceType.Empty && cpCaptured.Player != 0)
                currentCaptures.Add(new CaptureSet(cpCaptured, m.EndPosition));

            SetAdvantage();
            mMoveHistory.Add(m);
            mCaptureSets.Add(currentCaptures);
        }

        /// <summary>
        /// Adds positions of a given piece to a list
        /// </summary>
        private void AddPiecePositions(List<BoardPosition> list, ulong bitboard)
        {
            foreach (var p in BoardPosition.GetRectangularPositions(8, 8))
            {
                int index = GetBitIndexForPosition(p);
                ulong mask = 1UL << index;
                if(bitboard.CompareTo(bitboard | mask) == 0)
                    list.Add(p);
            }
        }

        /// <summary>
        /// Erases all white bitboards for easier piece setting
        /// </summary>
        private void EraseWhiteBitBoards(ulong mask)
        {
            mWhitePawn &= ~mask;
            mWhiteRook &= ~mask;
            mWhiteKnight &= ~mask;
            mWhiteBishop &= ~mask;
            mWhiteQueen &= ~mask;
            mWhiteKing &= ~mask;
        }

        /// <summary>
        /// Erases all black bitboards for easier piece setting
        /// </summary>
        private void EraseBlackBitBoards(ulong mask)
        {
            mBlackPawn &= ~mask;
            mBlackRook &= ~mask;
            mBlackKnight &= ~mask;
            mBlackBishop &= ~mask;
            mBlackQueen &= ~mask;
            mBlackKing &= ~mask;
        }

        /// <summary>
        /// Erases all bitboards for easier piece setting
        /// </summary>
        private void EraseBitBoards(ulong mask)
        {
            mBlackPawn &= ~mask;
            mBlackRook &= ~mask;
            mBlackKnight &= ~mask;
            mBlackBishop &= ~mask;
            mBlackQueen &= ~mask;
            mBlackKing &= ~mask;
            mWhitePawn &= ~mask;
            mWhiteRook &= ~mask;
            mWhiteKnight &= ~mask;
            mWhiteBishop &= ~mask;
            mWhiteQueen &= ~mask;
            mWhiteKing &= ~mask;
        }


		/// <summary>
		/// Mutates the board state so that the given piece is at the given position.
		/// </summary>
		private void SetPieceAtPosition(BoardPosition position, ChessPiece piece) {
            int index = GetBitIndexForPosition(position);
            ulong mask = 1UL << index;

            // To set a particular piece at a given position, we must bitwise OR the mask
            // into the player's bitboard, and then remove that mask from the other player's
            // bitboard.
            switch (piece.PieceType)
            {
                case ChessPieceType.Pawn:
                    if(piece.Player == 1)
                    {
                        EraseBitBoards(mask);
                        mWhitePawn |= mask;
                    }
                    else
                    {
                        EraseBitBoards(mask);
                        mBlackPawn |= mask;
                    }
                    break;
                case ChessPieceType.Rook:
                    if (piece.Player == 1)
                    {
                        EraseBitBoards(mask);
                        mWhiteRook |= mask;
                    }
                    else
                    {
                        EraseBitBoards(mask);
                        mBlackRook |= mask;
                    }
                    break;
                case ChessPieceType.Knight:
                    if (piece.Player == 1)
                    {
                        EraseBitBoards(mask);
                        mWhiteKnight |= mask;
                    }
                    else
                    {
                        EraseBitBoards(mask);
                        mBlackKnight |= mask;
                    }
                    break;
                case ChessPieceType.Bishop:
                    if (piece.Player == 1)
                    {
                        EraseBitBoards(mask);
                        mWhiteBishop |= mask;
                    }
                    else
                    {
                        EraseBitBoards(mask);
                        mBlackBishop |= mask;
                    }
                    break;
                case ChessPieceType.Queen:
                    if (piece.Player == 1)
                    {
                        EraseBitBoards(mask);
                        mWhiteQueen |= mask;
                    }
                    else
                    {
                        EraseBitBoards(mask);
                        mBlackQueen |= mask;
                    }
                    break;
                case ChessPieceType.King:
                    if (piece.Player == 1)
                    {
                        EraseBitBoards(mask);
                        mWhiteKing |= mask;
                    }
                    else
                    {
                        EraseBitBoards(mask);
                        mBlackKing |= mask;
                    }
                    break;
                case ChessPieceType.Empty:
                    EraseBitBoards(mask);
                    break;
            }


        }

        private void SetAdvantage(){
            CurrentAdvantage = new GameAdvantage(mAdvantageValue > 0 ? 1 : mAdvantageValue < 0 ? 2 : 0,
                Math.Abs(mAdvantageValue));
        }

        /// <summary>
		/// Returns the bit index corresponding to the given BoardPosition, with the LSB being index 0
		/// and the MSB being index 63.
		/// </summary>
		private static int GetBitIndexForPosition(BoardPosition boardPosition) =>
            63 - (boardPosition.Row * 8 + boardPosition.Col);

        private void AddWhiteBitBoards(ulong mask)
        {
            mWhitePawn |= mask;
            mWhiteRook |= mask;
            mWhiteKnight |= mask;
            mWhiteBishop |= mask;
            mWhiteQueen |= mask;
            mWhiteKing |= mask;
        }

        private void AddBlackBitBoards(ulong mask)
        {
            mBlackPawn |= mask;
            mBlackRook |= mask;
            mBlackKnight |= mask;
            mBlackBishop |= mask;
            mBlackQueen |= mask;
            mBlackKing |= mask;
        }

        private void ChangeAdvantage(int value)
        {
            mAdvantageValue += value;
        }

        private void AdvantagePiece(Tuple<BoardPosition, ChessPiece> pos)
        {
            switch (pos.Item2.PieceType)
            {
                case ChessPieceType.Pawn:
                    if (pos.Item2.Player == 1)
                        ChangeAdvantage(1);
                    else
                        ChangeAdvantage(-1);
                    break;
                case ChessPieceType.Rook:
                    if (pos.Item2.Player == 1)
                        ChangeAdvantage(5);
                    else
                        ChangeAdvantage(-5);
                    break;
                case ChessPieceType.Knight:
                    if (pos.Item2.Player == 1)
                        ChangeAdvantage(3);
                    else
                        ChangeAdvantage(-3);
                    break;
                case ChessPieceType.Bishop:
                    if (pos.Item2.Player == 1)
                        ChangeAdvantage(3);
                    else
                        ChangeAdvantage(-3);
                    break;
                case ChessPieceType.Queen:
                    if (pos.Item2.Player == 1)
                        ChangeAdvantage(9);
                    else
                        ChangeAdvantage(-9);
                    break;
                case ChessPieceType.King:
                    if (pos.Item2.Player == 1)
                        ChangeAdvantage(50);
                    else
                        ChangeAdvantage(-50);
                    break;
                default: //for ChessPieceType.Empty
                    ChangeAdvantage(0);
                    break;
            }
        }

        private void SetPlayerAtPosition(BoardPosition position, int player)
        {
            // Construct a bitmask for the bit position corresponding to the BoardPosition.
            int index = GetBitIndexForPosition(position);
            ulong mask = 1UL << index;

            // To set a particular player at a given position, we must bitwise OR the mask
            // into the player's bitboard, and then remove that mask from the other player's
            // bitboard. 
            if (player == 1)
            {
                AddWhiteBitBoards(mask);
                EraseBlackBitBoards(mask);
            }
            else if (player == 2)
            {
                AddBlackBitBoards(mask);
                EraseWhiteBitBoards(mask);
            }
            else
            {
                EraseBitBoards(mask);
            }
        }
        #endregion

        #region Explicit IGameBoard implementations.
        IEnumerable<IGameMove> IGameBoard.GetPossibleMoves() {
			return GetPossibleMoves();
		}
		void IGameBoard.ApplyMove(IGameMove m) {
			ApplyMove(m as ChessMove);
		}
		IReadOnlyList<IGameMove> IGameBoard.MoveHistory => mMoveHistory;

        private long weight;
        public long BoardWeight
        {
            get {
                weight = 0;
                weight += (long)(CurrentAdvantage.Advantage);

                foreach (BoardPosition bp in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
                {
                    if (GetPieceAtPosition(bp).PieceType == ChessPieceType.Pawn)
                    {
                        int originalRow = (GetPlayerAtPosition(bp) == 1) ? 6 : 1;
                        int rowDelta = bp.Row - originalRow;
                        weight -= rowDelta;
                    }


                    if (PositionIsThreatened(bp, CurrentPlayer))
                    {
                        int valueChange = 0;
                        if (GetPieceAtPosition(bp).PieceType == ChessPieceType.Knight || GetPieceAtPosition(bp).PieceType == ChessPieceType.Bishop)
                            valueChange = 1;
                        else if (GetPieceAtPosition(bp).PieceType == ChessPieceType.Rook)
                            valueChange = 2;
                        else if (GetPieceAtPosition(bp).PieceType == ChessPieceType.Queen)
                            valueChange = 5;
                        else if (GetPieceAtPosition(bp).PieceType == ChessPieceType.King)
                            valueChange = 4;

                        if (CurrentPlayer == 1)
                            weight += valueChange;
                        else
                            weight -= valueChange;
                    }
                }

                foreach (BoardPosition bp in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
                {
                    if (PositionIsEmpty(bp))
                    {
                        continue;
                    }

                    int valueChange = 0;
                    ChessPiece friendlyPiece = GetPieceAtPosition(bp);
                    if (friendlyPiece.Player == CurrentPlayer)
                    {
                        if (friendlyPiece.PieceType == ChessPieceType.Knight)
                        {
                            // Iterate through all 8 knight move directions to find any knight pieces
                            foreach (BoardDirection dirKnight in ChessBoard.KnightMoves)
                            {
                                BoardPosition newPos = bp;
                                newPos = newPos.Translate(dirKnight);

                                if (!PositionIsEnemy(newPos, CurrentPlayer) && !PositionIsEmpty(newPos)
                                    && GetPlayerAtPosition(bp) == CurrentPlayer && PositionInBounds(newPos)
                                    && (GetPieceAtPosition(newPos).PieceType == ChessPieceType.Knight || GetPieceAtPosition(newPos).PieceType == ChessPieceType.Bishop))
                                    valueChange = 1;
                            }
                        }
                        else if (friendlyPiece.PieceType == ChessPieceType.Pawn)
                        {
                            foreach (BoardDirection dir in BoardDirection.CardinalDirections.Where(x => Math.Abs(x.RowDelta) == 1 && Math.Abs(x.ColDelta) == 1))
                            {
                                BoardPosition newPos = bp;
                                newPos = newPos.Translate(dir);

                                if (!PositionIsEnemy(newPos, CurrentPlayer) && !PositionIsEmpty(newPos)
                                    && GetPlayerAtPosition(bp) == CurrentPlayer && PositionInBounds(newPos)
                                    && (GetPieceAtPosition(newPos).PieceType == ChessPieceType.Knight || GetPieceAtPosition(newPos).PieceType == ChessPieceType.Bishop))
                                    valueChange = 1;
                            }
                        }
                        else if (friendlyPiece.PieceType == ChessPieceType.Rook)
                        {
                            foreach (BoardDirection dir in BoardDirection.CardinalDirections.Where(x => Math.Abs(x.RowDelta) == 1 ^ Math.Abs(x.ColDelta) == 1))
                            {
                                BoardPosition newPos = bp;

                                do
                                {
                                    newPos = newPos.Translate(dir);
                                    if (GetPlayerAtPosition(newPos) == CurrentPlayer)
                                        break;
                                } while (!PositionIsEnemy(newPos, CurrentPlayer) && PositionInBounds(newPos));


                                if (!PositionIsEnemy(newPos, CurrentPlayer) && !PositionIsEmpty(newPos)
                                    && GetPlayerAtPosition(bp) == CurrentPlayer && PositionInBounds(newPos)
                                    && (GetPieceAtPosition(newPos).PieceType == ChessPieceType.Knight || GetPieceAtPosition(newPos).PieceType == ChessPieceType.Bishop))
                                    valueChange = 1;
                            }
                        }
                        else if (friendlyPiece.PieceType == ChessPieceType.Bishop)
                        {
                            foreach (BoardDirection dir in BoardDirection.CardinalDirections.Where(x => Math.Abs(x.RowDelta) == 1 & Math.Abs(x.ColDelta) == 1))
                            {
                                BoardPosition newPos = bp;

                                do
                                {
                                    newPos = newPos.Translate(dir);
                                    if (GetPlayerAtPosition(newPos) == CurrentPlayer)
                                        break;
                                } while (!PositionIsEnemy(newPos, CurrentPlayer) && PositionInBounds(newPos));

                                if (!PositionIsEnemy(newPos, CurrentPlayer) && !PositionIsEmpty(newPos)
                                    && GetPlayerAtPosition(bp) == CurrentPlayer && PositionInBounds(newPos)
                                    && (GetPieceAtPosition(newPos).PieceType == ChessPieceType.Knight || GetPieceAtPosition(newPos).PieceType == ChessPieceType.Bishop))
                                    valueChange = 1;
                            }
                        }
                        else if (friendlyPiece.PieceType == ChessPieceType.Queen)
                        {
                            foreach (BoardDirection dir in BoardDirection.CardinalDirections)
                            {
                                BoardPosition newPos = bp;

                                do
                                {
                                    newPos = newPos.Translate(dir);
                                    if (GetPlayerAtPosition(newPos) == CurrentPlayer)
                                        break;
                                } while (!PositionIsEnemy(newPos, CurrentPlayer) && PositionInBounds(newPos));

                                if (!PositionIsEnemy(newPos, CurrentPlayer) && !PositionIsEmpty(newPos)
                                    && GetPlayerAtPosition(bp) == CurrentPlayer && PositionInBounds(newPos)
                                    && (GetPieceAtPosition(newPos).PieceType == ChessPieceType.Knight || GetPieceAtPosition(newPos).PieceType == ChessPieceType.Bishop))
                                    valueChange = 1;
                            }
                        }
                        else if (friendlyPiece.PieceType == ChessPieceType.King)
                        {
                            foreach (BoardDirection dir in BoardDirection.CardinalDirections)
                            {
                                BoardPosition newPos = bp;
                                newPos = newPos.Translate(dir);

                                if (!PositionIsEnemy(newPos, CurrentPlayer) && !PositionIsEmpty(newPos)
                                    && GetPlayerAtPosition(bp) == CurrentPlayer && PositionInBounds(newPos)
                                    && (GetPieceAtPosition(newPos).PieceType == ChessPieceType.Knight || GetPieceAtPosition(newPos).PieceType == ChessPieceType.Bishop))
                                    valueChange = 1;
                            }
                        }
                    }
                    if (CurrentPlayer == 1)
                        weight += valueChange;
                    else
                        weight -= valueChange;
                }
                return weight;
            }
            private set { weight = value; }
        }
		#endregion

		public ChessBoard() {
            CurrentAdvantage = new GameAdvantage(0,0);
        }

		public ChessBoard(IEnumerable<Tuple<BoardPosition, ChessPiece>> startingPositions)
			: this() {
			var king1 = startingPositions.Where(t => t.Item2.Player == 1 && t.Item2.PieceType == ChessPieceType.King);
			var king2 = startingPositions.Where(t => t.Item2.Player == 2 && t.Item2.PieceType == ChessPieceType.King);
			if (king1.Count() != 1 || king2.Count() != 1) {
				throw new ArgumentException("A chess board must have a single king for each player");
			}

			foreach (var position in BoardPosition.GetRectangularPositions(8, 8)) {
				SetPieceAtPosition(position, ChessPiece.Empty);
			}

			foreach (var pos in startingPositions) {
                SetPieceAtPosition(pos.Item1, pos.Item2);
                AdvantagePiece(pos);
			}
            SetAdvantage();
        }
    }
}
