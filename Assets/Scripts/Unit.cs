using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    float speed = 5f; // speed for animation
    Vector2[] path;
    int targetIndex;
    bool succesful = false;
    Vector2 originalClickPos;
    public int MAX_ACTION_POINT; //movement point + other action
    public int MAX_HP; // health point
    public int MAX_AP; // attack point
    public int MAX_MP; // mana
    public int actionPoint; //movement point + other action
    public int hp; // health point
    public int ap; // attack point
    public int mp; // mana

    //delete units path, used before switching units and switching turn, function is called from Grid-script
    public void deletePath()
    {
        path = null;
    }

    public void RequestPath(Vector2 target)
    {
        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
    }

    public void OnPathFound(Vector2[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            //mark path succesful
            succesful = true;
            path = newPath;
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

        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length > 0)
        {
            Vector3 currentWaypoint = path[0];
            while (true)
            {
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
