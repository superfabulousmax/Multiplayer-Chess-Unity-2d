using UnityEngine;

public class QueenCheckRule : ICheckRule
{
    ICheckRule rookCheckRule;
    ICheckRule bishopCheckRule;

    public QueenCheckRule(ICheckRule rookCheckRule, ICheckRule bishopCheckRule)
    {
        this.rookCheckRule = rookCheckRule;
        this.bishopCheckRule = bishopCheckRule;
    }

    public bool PossibleCheck(IBoard board, int[,] boardState, IChessPiece queen, Vector3Int position, out IChessPiece king)
    {
        if(rookCheckRule.PossibleCheck(board, boardState, queen, position, out king))
        {
            return true;
        }
        return bishopCheckRule.PossibleCheck(board, boardState,queen, position, out king);
    }
}
