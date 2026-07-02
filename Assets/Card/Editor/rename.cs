using UnityEngine;
using UnityEditor;

public class RenameBoardCells
{
    [MenuItem("Tools/Rename Board Cells (Index Based)")]
    static void RenameCells()
    {
        GameObject parent = Selection.activeGameObject;

        if (parent == null)
        {
            Debug.LogError("보드 부모 오브젝트를 선택하세요!");
            return;
        }

        int size = 19;
        int index = 0;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (index >= parent.transform.childCount) break;

                Transform child = parent.transform.GetChild(index);
                child.name = $"({x},{y})";

                index++;
            }
        }

        Debug.Log("이름 변경 완료 (인덱스 기준)");
    }
}