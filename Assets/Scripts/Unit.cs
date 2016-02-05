using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    public Vector2 target;
    float speed = 5f;
    Vector2[] path;
    int targetIndex;
	bool succesful = false;

	//delete units path, used before switching units and switching turn, function is called from Grid-script
	public void deletePath(){
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
	public void startMoving(){
		if (succesful) {
			StopCoroutine ("FollowPath");
			succesful = false;
			StartCoroutine ("FollowPath");

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
