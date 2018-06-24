using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.ComputerOpponent {
    internal struct MinimaxBestMove {
        public long Weight { get; set; }
        public IGameMove Move { get; set; }
    }

    public class MinimaxAi : IGameAi {
        private int mMaxDepth;
        public MinimaxAi(int maxDepth) {
            mMaxDepth = maxDepth;
        }

        public IGameMove FindBestMove(IGameBoard b) {
            return FindBestMove(b, mMaxDepth,
                b.CurrentPlayer == 1,
                true ? int.MinValue : int.MaxValue,
                true ? int.MaxValue : int.MinValue
                ).Move;
        }

        //private static MinimaxBestMove FindBestMove(IGameBoard b, int alpha, int beta, int depthLeft, bool maximize) {



        //private static Tuple<long, IGameMove> FindBestMove(IGameBoard board, int depth, bool isMaximizing, int alpha, int beta)

        // alpha = MAXIMUM Lower bound of possible solutions
        // beta = MINIMUM upper bound of possible solutions
        //private static MinimaxBestMove FindBestMove(IGameBoard board, int depth, bool isMaximizing, int alpha, int beta)
        //{
        //    if (depth == 0 | board.IsFinished)
        //        return new MinimaxBestMove {
        //            Weight = (int)board.BoardWeight,
        //            Move = null
        //        };



        //    long bestWeight = (isMaximizing) ? Int64.MinValue : Int64.MaxValue;
        //    IGameMove bestMove = null;
        //    foreach (var possibleMove in board.GetPossibleMoves()) {
        //        board.ApplyMove(possibleMove);
        //        var nextBestMove = FindBestMove(board, depth - 1, !isMaximizing, alpha, beta);
        //        board.UndoLastMove();

        //        // If maximizing the AI's own advantage
        //        if (isMaximizing) {
        //            // if found the nextMove's weight to be higher than the last found (beta)
        //            // replace it with that.
        //            if (nextBestMove.Weight >= beta) {
        //                bestWeight = nextBestMove.Weight;
        //                bestMove = possibleMove;

        //            }
        //            // if the weight is less than alpha, keep searching
        //            if (nextBestMove.Weight <= alpha) continue;

        //            bestMove = possibleMove;
        //            alpha = nextBestMove.Weight;
        //        }
        //        else { // Minimizing the enemy "human"'s advantage
        //            // if human player's weight for that square is less than or equal to
        //            // "alpha" or a calculated/pulled weight or the lowest advantage possible
        //            if (nextBestMove.Weight <= alpha) {
        //                // return that value.
        //                bestWeight = nextBestMove.Weight;
        //                bestMove = possibleMove;
        //            }
        //            // else keep going down the tree
        //            if (nextBestMove.Weight >= beta) continue;

        //            bestMove = possibleMove;
        //            beta = nextBestMove.Weight;
        //        }
        //    }
        //    // when done, this is the result.
        //    return new MinimaxBestMove {
        //        Weight = (int)bestWeight,
        //        Move = bestMove
        //        };

        //}

        // -- Old AI, non alpha/beta -- 
        //    private static MinimaxBestMove FindBestMove(IGameBoard board, int depth, bool isMaximizing, int alpha, int beta) {
        //        if (depth == 0 || board.IsFinished)
        //            return new MinimaxBestMove {
        //                Weight = board.BoardWeight,
        //                Move = null
        //            };



        //        long bestWeight = (isMaximizing) ? Int64.MinValue : Int64.MaxValue;
        //        IGameMove bestMove = null;
        //        foreach (IGameMove possMove in board.GetPossibleMoves()) {
        //            board.ApplyMove(possMove);
        //            MinimaxBestMove w = FindBestMove(board, depth - 1, !isMaximizing, alpha, beta);

        //            board.UndoLastMove();
        //            // if maximized and weight is greater than best weight 
        //            // or not maximized and weight is less than best weight
        //            if (isMaximizing && w.Weight > bestWeight || !isMaximizing && w.Weight < bestWeight) {
        //                bestWeight = w.Weight;
        //                bestMove = possMove;
        //            }



        //        }
        //        return new MinimaxBestMove {
        //            Weight = bestWeight,
        //            Move = bestMove
        //        };

        //    }
        private static MinimaxBestMove FindBestMove(IGameBoard board, int depth, bool isMaximizing, long alpha, long beta) {
            if (depth == 0 || board.IsFinished)
                return new MinimaxBestMove {
                    Weight = board.BoardWeight,
                    Move = null
                };


            long bestWeight = (isMaximizing) ? Int64.MinValue : Int64.MaxValue;
            IGameMove bestMove = null;

            foreach (var move in board.GetPossibleMoves()) {
                board.ApplyMove(move);
                var w = FindBestMove(board, depth - 1, !isMaximizing, alpha, beta);
                board.UndoLastMove();
                if (isMaximizing && w.Weight > alpha) {
                    alpha = w.Weight;
                    bestMove = move;
                }
                else if (!isMaximizing && w.Weight < beta) {
                    beta = w.Weight;
                    bestMove = move;
                }
                if (alpha >= beta) {
                    long opponentsWeight = (isMaximizing) ? beta : alpha;
                    return new MinimaxBestMove() {
                        Move = move,
                        Weight = opponentsWeight
                    };
                }

            }

            long myWeight = (isMaximizing) ? alpha : beta;
            return new MinimaxBestMove() {
                Move = bestMove,
                Weight = myWeight
            };
        }        
        
    }
}
