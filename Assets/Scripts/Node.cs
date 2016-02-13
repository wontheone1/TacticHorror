using UnityEngine;
using System.Collections;
using FMOD;

public class Node : IHeapItem<Node> {
	
    
	public bool walkable;
    public NodeType nodeType;
	public Vector2 worldPosition;
	public int gridX;
	public int gridY;
	public int gCost;
	public int hCost;
	public Node parent;
	int heapIndex;
	
    public enum NodeType
    {
        walkable,
        blocked,
        covered,
        jumpCrossable,
        jumpDownable
    }

	public Node(bool _walkable, Vector2 _worldPos, int _gridX, int _gridY, NodeType _nodeType) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
	    nodeType = _nodeType;
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
