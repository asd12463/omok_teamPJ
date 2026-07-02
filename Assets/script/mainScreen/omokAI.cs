using UnityEngine;

public class omokAI : MonoBehaviour
{
    private int boardSize = 19;
    private omokMain mainScript;

    void Awake()
    {
        mainScript = GetComponent<omokMain>();
    }

    // 최적의 한 수를 찾아 좌표(x, y)를 반환하는 메인 함수
    public Vector2Int GetBestMove(int[,] board)
    {
        int bestScore = -1;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (board[x, y] == 0) // 빈칸일 때만 점수 계산
                {
                    int score = CalculateScore(x, y, board);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new Vector2Int(x, y);
                    }
                }
            }
        }
        return bestMove;
    }

    private int CalculateScore(int x, int y, int[,] board)
    {
        int totalScore = 0;
        // AI(2번)의 공격 점수 (가중치 1.5배)
        totalScore += GetLineScore(x, y, 2, board) * 15 / 10;
        // 플레이어(1번)의 수비 점수
        totalScore += GetLineScore(x, y, 1, board);
        return totalScore;
    }

    private int GetLineScore(int x, int y, int player, int[,] board)
    {
        int maxLineScore = 0;
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            int count = 1;
            count += CountStones(x, y, dx[i], dy[i], player, board);
            count += CountStones(x, y, -dx[i], -dy[i], player, board);

            // 점수 테이블: 돌이 많아질수록 점수가 폭발적으로 증가
            if (count >= 5) maxLineScore += 100000;
            else if (count == 4) maxLineScore += 10000;
            else if (count == 3) maxLineScore += 1000;
            else if (count == 2) maxLineScore += 100;
        }
        return maxLineScore;
    }

    private int CountStones(int x, int y, int dx, int dy, int player, int[,] board)
    {
        int count = 0;
        int nx = x + dx;
        int ny = y + dy;
        while (nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize && board[nx, ny] == player)
        {
            count++;
            nx += dx;
            ny += dy;
        }
        return count;
    }
}