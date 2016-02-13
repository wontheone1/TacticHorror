using UnityEngine;
using System.Collections;
using FMOD;

public class Node : IHeapItem<Node> {
	
    
	public bool walkable, jumpStartable, jumpFinishable, jumpThroughable, blocked, covered, occupied;
	public Vector2 worldPosition;
	public int gridX;
	public int gridY;
	public int gCost;
	public int hCost;
	public Node parent;
	int heapIndex;

	public Node(bool _walkable, Vector2 _worldPos, int _gridX, int _gridY, bool _startable, bool finishable,
        bool throughable, bool _blocked, bool _covered, bool _occupied) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
	    jumpStartable = _startable;
	    jumpFinishable = finishable;
	    jumpThroughable = throughable;
	    blocked = _blocked;
	    covered = _covered;
	    occupied = _occupied;
        }

	public int FCost {
		get {
			return gCost + hCost;
		}
	}

	public int HeapIndex {
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
		}
	}

	public int CompareTo(Node nodeToCompare) {
		int compare = FCost.CompareTo(nodeToCompare.FCost);
		if (compare == 0) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}
