using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.WpfView;
using Cecs475.BoardGames.ComputerOpponent;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.Chess.WpfViewNew
{
    public class ChessSquare: INotifyPropertyChanged
    {
        private int mPlayer;
        public int Player
        {
            get { return mPlayer; }
            set
            {
                if (value != mPlayer)
                {
                    mPlayer = value;
                    OnPropertyChanged(nameof(Player));
                }
            }
        }

        private ChessPiece mPiece;
        public ChessPiece Piece
        {
            get { return mPiece; }
            set {
                if (value.Player != mPiece.Player)
                {
                    mPiece = value;
                    OnPropertyChanged(nameof(Piece));
                }
            }
        }


        public BoardPosition Position { get; set; }

        private bool mIsCheck;
        public bool IsCheck {
            get { return mIsCheck; }
            set {
                if (value != mIsCheck) {
                    mIsCheck = value;
                    OnPropertyChanged(nameof(IsCheck));
                }
            }
        }

        private bool mIsHighlighted;
        public bool IsHighlighted
        {
            get { return mIsHighlighted; }
            set
            {
                if (value != mIsHighlighted)
                {
                    mIsHighlighted = value;
                    OnPropertyChanged(nameof(IsHighlighted));
                }
            }
        }

        private bool mIsSelected;
        public bool IsSelected
        {
            get { return mIsSelected; }
            set
            {
                if (value != mIsSelected)
                {
                    mIsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class ChessViewModel : INotifyPropertyChanged, IGameViewModel
    {
        // IGameViewModel properties
        public GameAdvantage BoardAdvantage => mBoard.CurrentAdvantage;
        public long Weight => mBoard.BoardWeight;
        public int CurrentPlayer { get { return mBoard.CurrentPlayer; } }
        public bool CanUndo => mBoard.MoveHistory.Any() && UndoEnabler;
        public bool Check { get { return mBoard.IsCheck; } }

        public event EventHandler GameFinished;
        private const int MAX_AI_DEPTH = 4;
        private IGameAi mGameAi = new MinimaxAi(MAX_AI_DEPTH);
        public NumberOfPlayers Players { get; set; }

        private bool enabler;
        public bool UndoEnabler
        {
            get { return enabler; }
            set {
                enabler = value;
                OnPropertyChanged(nameof(CanUndo));
            }
        }

        private ChessBoard mBoard;
        private ObservableCollection<ChessSquare> mSquares;
        public HashSet<BoardPosition> StartMoves { get; private set; }
        public HashSet<BoardPosition> EndMoves { get; set; }
        public IEnumerable<ChessMove> PossMoves { get; private set; }
        public ObservableCollection<ChessSquare> Squares { get { return mSquares; } }

        /// <summary>
        /// Set board, square objects, and possible moves
        /// </summary>
        public ChessViewModel()
        {
            // Pawn Promotion Board Testing
            List<Tuple<BoardPosition, ChessPiece>> PawnPromoTest = new List<Tuple<BoardPosition, ChessPiece>>();
            PawnPromoTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(7, 4), new ChessPiece(ChessPieceType.King, 1)));
            PawnPromoTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(0, 4), new ChessPiece(ChessPieceType.King, 2)));
            PawnPromoTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Rook, 1)));
            PawnPromoTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Rook, 2)));
            PawnPromoTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(7, 7), new ChessPiece(ChessPieceType.Rook, 1)));
            PawnPromoTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(0, 7), new ChessPiece(ChessPieceType.Rook, 2)));
            PawnPromoTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(5, 6), new ChessPiece(ChessPieceType.Bishop, 2)));

            // King Check Test - Kings Only, can test Stalemate
            List<Tuple<BoardPosition, ChessPiece>> KingOnlyCheckTest = new List<Tuple<BoardPosition, ChessPiece>>();
            KingOnlyCheckTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(7, 4), new ChessPiece(ChessPieceType.King, 1)));
            KingOnlyCheckTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(0, 4), new ChessPiece(ChessPieceType.King, 2)));

            // King Check Test - Kings + Other Pieces
            List<Tuple<BoardPosition, ChessPiece>> CheckTest = new List<Tuple<BoardPosition, ChessPiece>>();
            CheckTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(7, 4), new ChessPiece(ChessPieceType.King, 1)));
            CheckTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(0, 4), new ChessPiece(ChessPieceType.King, 2)));
            CheckTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Rook, 1)));
            CheckTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Rook, 2)));

            List<Tuple<BoardPosition, ChessPiece>> VisTest = new List<Tuple<BoardPosition, ChessPiece>>();
            VisTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(3, 4), new ChessPiece(ChessPieceType.King, 1)));
            VisTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(1, 4), new ChessPiece(ChessPieceType.King, 2)));
            VisTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(3, 3), new ChessPiece(ChessPieceType.Rook, 1)));
            VisTest.Add(new Tuple<BoardPosition, ChessPiece>(new BoardPosition(1, 3), new ChessPiece(ChessPieceType.Rook, 2)));

            //mBoard = new ChessBoard(VisTest);
            //mBoard = new ChessBoard(PawnPromoTest);
            //mBoard = new ChessBoard(KingOnlyCheckTest);
            //mBoard = new ChessBoard(CheckTest);
             mBoard = new ChessBoard();

            mSquares = new ObservableCollection<ChessSquare>(
                BoardPosition.GetRectangularPositions(8, 8)
                .Select(pos => new ChessSquare()
                {
                    Position = pos,
                    Player = mBoard.GetPlayerAtPosition(pos),
                    Piece = mBoard.GetPieceAtPosition(pos)
                })
            );

            PossMoves = mBoard.GetPossibleMoves();
            EndMoves = new HashSet<BoardPosition>();
            StartMoves = new HashSet<BoardPosition>(
                from ChessMove m in mBoard.GetPossibleMoves()
                select m.StartPosition
            );
        }

        /// <summary>
        /// Applies a move for the current player at the given position.
        /// </summary>
        public async Task ApplyMove(BoardPosition start, BoardPosition end, ChessPieceType cpt)
        {
            foreach (var move in mBoard.GetPossibleMoves())
            {
                if (move.StartPosition.Equals(start) & move.EndPosition.Equals(end))
                {
                    if(move.PromotionPiece != ChessPieceType.Empty & move.PromotionPiece == cpt | move.PromotionPiece == ChessPieceType.Empty)
                    {
                        mBoard.ApplyMove(move);
                        break;
                    }
                }
            }

            if (Players == NumberOfPlayers.One && !mBoard.IsFinished)
            {
                var bestMove = await Task.Run(() => mGameAi.FindBestMove(mBoard));
                if (bestMove != null)
                {
                    mBoard.ApplyMove(bestMove as ChessMove);
                }
            }

            RebindState();

            if (mBoard.IsFinished)
                GameFinished?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Rebind possible moves and update all squares
        /// </summary>
        private void RebindState()
        {
            PossMoves = mBoard.GetPossibleMoves();
            EndMoves = new HashSet<BoardPosition>();
            StartMoves = new HashSet<BoardPosition>(
                from ChessMove m in mBoard.GetPossibleMoves()
                select m.StartPosition
            );

            var newSquares = BoardPosition.GetRectangularPositions(8, 8);
            int i = 0;
            foreach (var pos in newSquares)
            {
                ChessSquare cs = mSquares[i];
                cs.Player = mBoard.GetPlayerAtPosition(pos);
                cs.Piece = mBoard.GetPieceAtPosition(pos);
                cs.IsCheck = (Check && cs.Piece.PieceType == ChessPieceType.King && cs.Player == CurrentPlayer) ? true : false;
                i++;
            }
            
            OnPropertyChanged(nameof(BoardAdvantage));
            OnPropertyChanged(nameof(CurrentPlayer));
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(Check));
        }

        /// <summary>
        /// Undo last move if there is a move to undo
        /// </summary>
        public void UndoMove()
        {
            if (CanUndo) {
                mBoard.UndoLastMove();
                if (Players == NumberOfPlayers.One && CanUndo) {
                    mBoard.UndoLastMove();
                }
                RebindState();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
