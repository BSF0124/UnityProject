using UnityEngine;
using DG.Tweening;
using TMPro;

public class SubCharacter : MonoBehaviour
{
    public TextMeshPro distanceTextPrefab;
    private TextMeshPro distanceText;
    private Vector3 targetPosition;
    public GameObject[] maps;
    private GameObject[,] tilemap;

    [HideInInspector]
    public int distance_x, distance_y; // 남은 거리
    public float sum_X, sum_Y; // 캐릭터 이동 거리
    public float scale;
    private bool goalCheck = false;
    private char[,] map; // 맵의 구조
    // S:시작점, G:도착점, L:사다리, W:벽, E:길
    // 짝수:비활성화 길, 홀수:비활성화 사다리
    private int column, row; // 맵의 가로, 세로의 크기
    private int current_x, current_y; // 현재 위치
    private int goalCount = 0;
    private int stageIndex;
    private int[,] deactivatedBoard; // 비활성화 맵 정보
    private int deactivatedCount = 0;
    private int deactivatedTotal = 0; // 총 비활성화 개수
    private float duration = 0.1f;

    void Start()
    {
        string[] str = transform.parent.parent.transform.name.Split();
        stageIndex = int.Parse(str[1])-1;
        // 변수들을 불러옴
        column = DataManager.instance.stageList.stage[stageIndex].map_Width;
        row = DataManager.instance.stageList.stage[stageIndex].map_Height;
        current_x = DataManager.instance.stageList.stage[stageIndex].map_X;
        current_y = DataManager.instance.stageList.stage[stageIndex].map_Y;

        // 맵의 크기 설정
        map = new char[column, row];

        // 목표 좌표 설정
        targetPosition = transform.localPosition;

        for(int i=0; i<column*row; i++)
        {
            // 맵의 구성을 불러옴
            map[i%column,i/column] = DataManager.instance.stageList.stage[stageIndex].map_Elements[i];

            // 비활성화 길, 사다리 갯수 확인
            if(map[i%column,i/column] < 65)
            {
                deactivatedTotal++;
            }

            // 도착지가 1개인 경우
            if(goalCount == 0 && (map[i%column, i/column] == 'G' || map[i%column, i/column] == 'B'))
            {
                goalCount++;
                distance_x = i%column;
                distance_y = -(i/column);
            }

            // 도착지가 2개 이상인 경우
            else if(map[i%column, i/column] == 'G' || map[i%column, i/column] == 'B')
            {
                goalCount++;
            }
        }

        deactivatedBoard = new int[deactivatedTotal,3];
        int n = 0;
        for(int i=0; i<column*row; i++)
        {
            // 비활성화 길, 사다리 정보를 저장
            if(map[i%column,i/column] < 65)
            {
                deactivatedBoard[n,0] = map[i%column,i/column];
                deactivatedBoard[n,1] = i%column;
                deactivatedBoard[n,2] = i/column;
                n++;
            }
        }

        // 도착지까지 남은 거리를 나타내는 텍스트 설정
        distanceText = Instantiate(distanceTextPrefab,gameObject.transform.parent);
        if(goalCount > 1)
        {
            goalCheck = true;
        }

        // 남은 거리 텍스트가 화면 밖으로 나가는 경우 서브 캐릭터의 아래에 배치
        if(targetPosition.y + 1 >= 8)
        {
            distanceText.transform.localPosition = targetPosition + Vector3.down;
        }
        // 화면 밖으로 나가지 않으면 서브 캐릭터 위에 배치
        else
        {
            distanceText.transform.localPosition = targetPosition + Vector3.up;
        }
        distance_x += current_x;
        distance_y += current_y;
        DistanceTextUpdate();

        // 맵 생성
        tilemap = new GameObject[column,row];
        for(int i=0; i<column; i++)
        {
            for(int j=0; j<row; j++)
            {
                MapPlacement(i,j);
            }
        }

        SortArray(deactivatedBoard);
    }

    void Update()
    {
        if(map[current_x,current_y] == 'G' || map[current_x,current_y] == 'B')
        {
            GameManager.instance.isSubClear = true;
        }
        else
        {
            GameManager.instance.isSubClear = false;
        }
    }

    public void Move_Left()
    {
        if((current_x == 0) || (map[current_x-1,current_y] == 'W') || (map[current_x-1,current_y] < 65))
        {
            Shake();
        }
        else
        {
            current_x--;
            targetPosition -= new Vector3(sum_X, 0, 0);
            if(targetPosition.y + 1 >= 8)
            {
                transform.DOLocalMove(targetPosition, 0.25f);
                distanceText.transform.DOLocalMove(targetPosition + Vector3.down, 0.25f);
            }
            else
            {
                transform.DOLocalMove(targetPosition, 0.25f);
                distanceText.transform.DOLocalMove(targetPosition + Vector3.up, duration);
            }
            distance_x++;
            DistanceTextUpdate();
            
        }

    }
    public void Move_Right()
    {
        if((current_x == column-1) || (map[current_x+1,current_y] == 'W') || (map[current_x+1,current_y] < 65))
        {
            Shake();
        }
        else
        {
            current_x++;
            targetPosition += new Vector3(sum_X, 0, 0);
            if(targetPosition.y + 1 >= 8)
            {
                transform.DOLocalMove(targetPosition, 0.25f);
                distanceText.transform.DOLocalMove(targetPosition + Vector3.down, 0.25f);
            }
            else
            {
                transform.DOLocalMove(targetPosition, 0.25f);
                distanceText.transform.DOLocalMove(targetPosition + Vector3.up, duration);
            }
            distance_x--;
            DistanceTextUpdate();
        }
    }

    public void Move_Up()
    {
        if((current_y == 0) || ((map[current_x,current_y] != 'L') && (map[current_x,current_y] != 'B') && (map[current_x,current_y] != 'S')) || (map[current_x,current_y-1] == 'W') || (map[current_x,current_y-1] < 65))
        {
            Shake();
        }
        else
        {
            
            current_y--;
            targetPosition += new Vector3(0, sum_Y, 0);

            if(targetPosition.y + 1 >= 8)
            {
                transform.DOLocalMove(targetPosition, 0.25f);
                distanceText.transform.DOLocalMove(targetPosition + Vector3.down, 0.25f);
            }
            else
            {
                transform.DOLocalMove(targetPosition, 0.25f);
                distanceText.transform.DOLocalMove(targetPosition + Vector3.up, 0.25f);
            }
            distance_y--;
            DistanceTextUpdate();
        }
    }
    public void Move_Down()
    {

        if((current_y == row-1) || ((map[current_x,current_y] != 'L') && (map[current_x,current_y] != 'B')) || (map[current_x,current_y+1] == 'W') || (map[current_x,current_y+1] < 65))
        {
            Shake();
        }
        else
        {
            current_y++;
            targetPosition -= new Vector3(0, sum_Y, 0);
            if(targetPosition.y + 1 >= 8)
            {
                transform.DOLocalMove(targetPosition, 0.25f);
                distanceText.transform.DOLocalMove(targetPosition + Vector3.down, 0.25f);
            }
            else
            {
                transform.DOLocalMove(targetPosition, 0.25f);
                distanceText.transform.DOLocalMove(targetPosition + Vector3.up, duration);
            }
            distance_y++;
            DistanceTextUpdate();
        }
    }

    public void BoardActivated()
    {
        if(deactivatedCount == deactivatedTotal)
        {    
            return;
        }

        int x = deactivatedBoard[deactivatedCount,1];
        int y = deactivatedBoard[deactivatedCount,2];

        if(deactivatedBoard[deactivatedCount,0] % 2 == 0)
        {
            map[x,y] = 'E';
        }
        else
        {
            map[x,y] = 'L';
        }
        AudioManager.instance.PlaySfx(AudioManager.Sfx.PlayerTool);
        Transform temp = tilemap[x,y].transform;
        Destroy(tilemap[x,y]);
        TypeCheck(x,y);
        tilemap[x,y].transform.localScale = new Vector3(scale,scale,scale);
        tilemap[x,y].transform.parent = transform.parent;
        tilemap[x,y].transform.position = temp.position;
        deactivatedCount++;
    }

    void Shake()
    {
        transform.DOShakePosition(duration, 0.05f, 25, 90);
    }
    
    void DistanceTextUpdate()
    {
        if(goalCheck)
        {
            distanceText.text = "?,?";
        }
        else if(distance_x == 0 && distance_y == 0)
        {
            distanceText.text = "Goal!";
        }

        else
        {
            distanceText.text = $"[{distance_x},{distance_y}]";
        }
    }

    void MapPlacement(int x, int y)
    {

        if(map[x,y] != 'W')
        {
            if(map[x,y] < 65)
            {
                tilemap[x,y] = Instantiate(maps[4]);
                tilemap[x,y].transform.localScale = new Vector3(scale,scale,scale);
                tilemap[x,y].transform.parent = transform.parent;
                tilemap[x,y].transform.localPosition = new Vector3(transform.localPosition.x + ((x - DataManager.instance.stageList.stage[stageIndex].map_X) * sum_X),
                transform.localPosition.y -((y-DataManager.instance.stageList.stage[stageIndex].map_Y+1) * sum_Y), -0.01f);
            }
            else
            {
                TypeCheck(x,y);
                tilemap[x,y].transform.localScale = new Vector3(scale,scale,scale);
                tilemap[x,y].transform.parent = transform.parent;
                tilemap[x,y].transform.localPosition = new Vector3(transform.localPosition.x + ((x - DataManager.instance.stageList.stage[stageIndex].map_X) * sum_X),
                transform.localPosition.y -((y-DataManager.instance.stageList.stage[stageIndex].map_Y+1) * sum_Y), -0.01f);
            }
        }
    }
    void TypeCheck(int x, int y)
    {
        if(x != column-1 && (x == 0 || map[x-1,y] == 'W') && map[x+1,y] != 'W')
            tilemap[x,y] = Instantiate(maps[0]);
        else if(x != 0 && (x == column-1 || map[x+1,y] == 'W') && map[x-1,y] != 'W')
            tilemap[x,y] = Instantiate(maps[1]);
        else if((x == 0 && map[x+1,y] == 'W') || (x == column-1 && map[x-1,y] == 'W') || (map[x-1,y] == 'W' && map[x+1,y] == 'W'))
            tilemap[x,y] = Instantiate(maps[2]);
        else
            tilemap[x,y] = Instantiate(maps[3]);
    }

    void SortArray(int[,] array)
    {
        int n = array.GetLength(0);

        for(int i=0; i<n; i++)
        {
            for(int j=0; j<n-i-1; j++)
            {
                if(array[j,0] > array[j+1,0])
                {
                    for(int k=0; k<3; k++)
                    {
                        int temp = array[j,k];
                        array[j,k] = array[j+1,k];
                        array[j+1,k] = temp;
                    }
                }
            }
        }
    }
}