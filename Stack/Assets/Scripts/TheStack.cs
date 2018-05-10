using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheStack : MonoBehaviour {

    public Color32[] gameColors = new Color32[4];
    public Material stackMat;

    private const float BOUNDS_SIZE = 3.5f; //размер платформ
    private const float STACK_MOVING_SPEED = 5.0f; //скорость платформ для камеры
    private const float ERROR_MARGIN = 0.1f; //допустимый промах
    private const float STACK_BOUNDS_GAIN = 0.25f; //увеличение платформы при комбо
    private const int COMBO_START_GAIN = 2; //количество комбо

    private GameObject[] theStack; //массив всех платформ
    private Vector2 stackBounds = new Vector2(BOUNDS_SIZE,BOUNDS_SIZE); //размер платформы которая будет создана сверху

    private int stackIndex; // 
    private int scoreCount = 0; //счет
    private int combo = 0;

    private float tileTransition = 0.0f;
    private float tileSpeed = 2.5f; //скорость платформ
    private float secondaryPosition; //следующая позиция

    private bool isMovingOnX = true; //направление
    private bool gameOver = false;

    private Vector3 desiredPosition;
    private Vector3 lastTilePosition;

	void Start () {
        theStack = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            theStack[i] = transform.GetChild(i).gameObject;
            ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
        }

        stackIndex = transform.childCount - 1;
	}

    void CreateRubble(Vector3 pos,Vector3 scale) //создание отрезанной части платформы
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>();

        go.GetComponent<MeshRenderer>().material = stackMat;
        ColorMesh(go.GetComponent<MeshFilter>().mesh);
    }
	
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceTile())
            {
                SpawnTile();
                scoreCount++;
            }
            else
            {
                EndGame();
            }
        }

        MoveTile();

        //Smooth move TheStack(all objects)
        transform.position = Vector3.Lerp(transform.position,desiredPosition,STACK_MOVING_SPEED * Time.deltaTime);
	}

    private void MoveTile() //движение в стороны кждого блока
    {
        if (gameOver)
            return;

        tileTransition += Time.deltaTime * tileSpeed;
        if(isMovingOnX)
            theStack[stackIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * BOUNDS_SIZE,scoreCount,secondaryPosition);
        else
            theStack[stackIndex].transform.localPosition = new Vector3(secondaryPosition,scoreCount,Mathf.Sin(tileTransition) * BOUNDS_SIZE);
    }

    private void SpawnTile() //перемещение нижней платформы наверх
    {
        lastTilePosition = theStack[stackIndex].transform.localPosition;

        stackIndex--;
        if(stackIndex < 0)
        {
            stackIndex = transform.childCount - 1;
        }

        desiredPosition = (Vector3.down) * scoreCount;
        theStack[stackIndex].transform.localPosition = new Vector3(0,scoreCount,0);
        theStack[stackIndex].transform.localScale = new Vector3(stackBounds.x,1,stackBounds.y);

        ColorMesh(theStack[stackIndex].GetComponent<MeshFilter>().mesh);
    }

    private bool PlaceTile()
    {
        Transform t = theStack[stackIndex].transform;

        if (isMovingOnX) //движение по оси Х
        {
            float deltaX = lastTilePosition.x - t.position.x; //размер оставшегося куска от платформы
            if(Mathf.Abs(deltaX) > ERROR_MARGIN)
            {
                //cut the tile
                combo = 0;
                stackBounds.x -= Mathf.Abs(deltaX);
                if (stackBounds.x < 0)
                    return false;

                float middle = lastTilePosition.x + t.localPosition.x / 2;
                t.localScale = new Vector3(stackBounds.x,1,stackBounds.y);
                CreateRubble(
                    new Vector3((t.position.x > 0) 
                    ? t.position.x + (t.localScale.x / 2) 
                    : t.position.x - (t.localScale.x / 2)
                    ,t.position.y
                    ,t.position.z),
                    new Vector3(Mathf.Abs(deltaX),1,t.localScale.z)
                    );
                t.localPosition = new Vector3(middle - (lastTilePosition.x / 2),scoreCount,lastTilePosition.z);
            }
            else
            {
                if (combo > COMBO_START_GAIN)
                {
                    stackBounds.x += STACK_BOUNDS_GAIN;

                    if (stackBounds.x > BOUNDS_SIZE)
                        stackBounds.x = BOUNDS_SIZE;

                    float middle = lastTilePosition.x + t.localPosition.x / 2;
                    t.localScale = new Vector3(stackBounds.x,1,stackBounds.y);
                    t.localPosition = new Vector3(middle - (lastTilePosition.x / 2),scoreCount,lastTilePosition.z);
                }

                combo++;
                t.localPosition = new Vector3(lastTilePosition.x,scoreCount,lastTilePosition.z);
            }
        }
        else //движение по оси Z
        {
            float deltaZ = lastTilePosition.z - t.position.z;
            if (Mathf.Abs(deltaZ) > ERROR_MARGIN)
            {
                //cut the tile
                combo = 0;
                stackBounds.y -= Mathf.Abs(deltaZ);
                if (stackBounds.y < 0)
                    return false;

                float middle = lastTilePosition.z + t.localPosition.z / 2;
                t.localScale = new Vector3(stackBounds.x,1,stackBounds.y);
                CreateRubble(
                    new Vector3(t.position.x
                    ,t.position.y
                    ,(t.position.z > 0)
                    ? t.position.z + (t.localScale.z / 2)
                    : t.position.z - (t.localScale.z / 2)),
                    new Vector3(t.localScale.x,1,Mathf.Abs(deltaZ))
                    );
                t.localPosition = new Vector3(lastTilePosition.x,scoreCount, middle - (lastTilePosition.z / 2));
            }
            else
            {
                if (combo > COMBO_START_GAIN)
                {
                    stackBounds.y += STACK_BOUNDS_GAIN;

                    if (stackBounds.y > BOUNDS_SIZE)
                        stackBounds.y = BOUNDS_SIZE;

                    float middle = lastTilePosition.z + t.localPosition.z / 2;
                    t.localScale = new Vector3(stackBounds.x,1,stackBounds.y);
                    t.localPosition = new Vector3(lastTilePosition.x,scoreCount,middle - (lastTilePosition.z / 2));
                }

                combo++;
                t.localPosition = new Vector3(lastTilePosition.x,scoreCount,lastTilePosition.z);
            }
        }

        secondaryPosition = (isMovingOnX) ? t.localPosition.x : t.localPosition.z;//следующая позиция платформы
        isMovingOnX = !isMovingOnX; //смена направления
        return true;
    }

    private void ColorMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];
        float f = Mathf.Sin(scoreCount * 0.25f);

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = Lerp4(gameColors[0],gameColors[1],gameColors[2],gameColors[3],f);
        }
        mesh.colors32 = colors;
    }

    private Color32 Lerp4(Color32 a,Color32 b,Color32 c,Color32 d, float t)
    {
        if (t < 0.33f)
            return Color.Lerp(a,b,t / 0.33f);
        else if (t < 0.66f)
            return Color.Lerp(b,c,(t - 0.33f) / 0.33f);
        else
            return Color.Lerp(c,d,(t - 0.66f) / 0.66f);
    }

    private void EndGame() //Конец игры
    {
        Debug.Log("Loss");
        gameOver = true;
        theStack[stackIndex].AddComponent<Rigidbody>();
    }
}
