using UnityEditor;

[CustomEditor(typeof(Board))]
public class BoardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var board = (Board)target;
        if (board.BoardState == null)
            return;
        board.GetBoardState();
        var boardString = board.GetBoardStateString();
        EditorGUILayout.TextArea(boardString);
        if (EditorApplication.isPlaying)
        {
            Repaint();
        }
    }
}
