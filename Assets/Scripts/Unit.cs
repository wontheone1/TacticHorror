using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    float speed = 5f; // speed for animation
    Vector2[] path;
    int targetIndex;
    bool unitMoving = false;
    bool succesful = false;
    Vector2 originalClickPos;
    protected int MAX_ACTION_POINT; //movement point + other action
    protected int MAX_HP; // health point
    protected int MAX_AP; // attack point
    protected int MAX_MP; // mana
    protected int actionPoint; //movement point + other action
    protected int hp; // health point
    protected int ap; // attack point
    protected int mp; // mana
    private int movementCostToDestination;

    //delete units path, used before switching units and switching turn, function is called from Grid-script
    public void deletePath()
    {
        if(!unitMoving)
            path = null;
    }

    public void RequestPath(Vector2 target)
    {
        if(isMovementPossibe())
            PathRequestManager.RequestPath(transform.position, target, actionPoint, OnPathFound);
    }

    public bool isMovementPossibe()
    {
        return actionPoint > 10;
    }

    public void OnPathFound(Vector2[] newPath, bool pathSuccessful, int movementCost)
    {
        if (pathSuccessful)
        {
            //mark path succesful
            succesful = true;
            path = newPath;
            movementCostToDestination = movementCost;
        }
    }

    //movement script to move unit when "move button" is clicked, succesful boolean tests for succesful path before moving
    public void startMoving()
    {
        if (succesful)
        {
            StopCoroutine("FollowPath");
            succesful = false;
            StartCoroutine("FollowPath");
            actionPoint -= movementCostToDestination;
        }
    }

    public void replenishActionPoint()
    {
        actionPoint = MAX_ACTION_POINT;
    }

    IEnumerator FollowPath()
    {
        if (path.Length > 0)
        {
            
            Vector3 currentWaypoint = path[0];
            while (true)
            {
                unitMoving = true;
                if (transform.position == currentWaypoint)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        /// When finished moving, clear up 
                        targetIndex = 0;
                        path = new Vector2[0];
                        yield break;
                    }
                    currentWaypoint = path[targetIndex];
                }

                transform.position = Vector2.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
                unitMoving = false;
                yield return null;
            }
            
        }
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], new Vector3(0.25f, 0.25f, 0.25f));

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}
