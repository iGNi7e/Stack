using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheStack : MonoBehaviour {

    private const float BOUNDS_SIZE = 3.5f; //размер платформ
    private const float STACK_MOVING_SPEED = 5.0f; //скорость платформ для камеры
    private const float ERROR_MARGIN = 0.1f;

    private GameObject[] theStack; //массив всех платформ
    private Vector2 stackBounds = new Vector2(BOUNDS_SIZE,BOUNDS_SIZE);

    private int stackIndex; //
    private int scoreCount = 0; //счет
    private int combo = 0;

    private float tileTransition = 0.0f;
    private float tileSpeed = 2.5f; //скорость платформ
    private float secondaryPosition; //следующая позиция

    private bool isMovingOnX = true; //направление

    private Vector3 desiredPosition;
    private Vector3 lastTilePosition;

	void Start () {
        theStack = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            theStack[i] = transform.GetChild(i).gameObject;
        }

        stackIndex = transform.childCount - 1;
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

        //Move  TheStack
        transform.position = Vector3.Lerp(transform.position,desiredPosition,STACK_MOVING_SPEED * Time.deltaTime);
	}

    private void MoveTile()
    {
        tileTransition += Time.deltaTime * tileSpeed;
        if(isMovingOnX)
            theStack[stackIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * BOUNDS_SIZE,scoreCount,secondaryPosition);
        else
            theStack[stackIndex].transform.localPosition = new Vector3(secondaryPosition,scoreCount,Mathf.Sin(tileTransition) * BOUNDS_SIZE);
    }

    private void SpawnTile()
    {
        lastTilePosition = theStack[stackIndex].transform.localPosition;
        stackIndex--;
        if(stackIndex < 0)
        {
            stackIndex = transform.childCount - 1;
        }

        desiredPosition = (Vector3.down) * scoreCount;
        theStack[stackIndex].transform.localPosition = new Vector3(0,scoreCount,0);

    }

    private bool PlaceTile()
    {
        Transform t = theStack[stackIndex].transform;

        if (isMovingOnX)
        {
            float deltaX = lastTilePosition.x - t.position.x;
            if(Mathf.Abs(deltaX) > ERROR_MARGIN)
            {
                //cut the tile
                combo = 0;
                stackBounds.x -= Mathf.Abs(deltaX);
                if (stackBounds.x < 0)
                    return false;

                float middle = lastTilePosition.x + t.localPosition.x / 2;
                t.localScale = new Vector3(stackBounds.x,1,stackBounds.y);
            }
        }

        secondaryPosition = (isMovingOnX) ? t.localPosition.x : t.localPosition.z;
        isMovingOnX = !isMovingOnX;
        return true;
    }

    private void EndGame()
    {

    }
}
