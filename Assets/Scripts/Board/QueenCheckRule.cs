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

    public bool PossibleCheck(Board board, ChessPiece queen, Vector3Int position, out ChessPiece king)
    {
        if(rookCheckRule.PossibleCheck(board, queen, position, out king))
        {
            return true;
        }
        return bishopCheckRule.PossibleCheck(board, queen, position, out king);
    }
}
