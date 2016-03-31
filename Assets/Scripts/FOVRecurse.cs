using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The method GetVisibleCells() is called to calculate the cells
/// visible to the player by examing each octant sequantially. 
/// The generic list VisiblePoints contains the cells visible to the player.
/// </summary>
public class FOVRecurse : MonoBehaviour
{
    private Grid _grid;
    private GameController gameController;

    public bool[,] map { get; private set; }

    /// <summary>
    /// Radius of the player's circle of vision
    /// </summary>
    public int VisualRange { get; set; }

    /// <summary>
    /// List of points visible to the player
    /// </summary>
    public List<Node> VisiblePoints { get; private set; }  // Cells the player can see

    private List<Unit> Units;

    /// <summary>
    /// The octants which a player can see
    /// </summary>

    //  Octant data
    //
    //    \ 1 | 2 /
    //   8 \  |  / 3
    //   -----+-----
    //   7 /  |  \ 4
    //    / 6 | 5 \
    //
    //  1 = NNW, 2 =NNE, 3=ENE, 4=ESE, 5=SSE, 6=SSW, 7=WSW, 8 = WNW

    /// <summary>
    /// The octants which a player can see
    /// </summary>

    // List<int> VisibleOctants = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
    List<int> VisibleOctantsWhenFacingLeft = new List<int>() { 7, 8 };
    List<int> VisibleOctantsWhenFacingRight = new List<int>() { 3, 4 };
    //List<int> VisibleOctantsWhenFacingUp = new List<int>() { 1, 2 };
    //List<int> VisibleOctantsWhenFacingDown = new List<int>() { 5, 6 };

    // ReSharper disable once UnusedMember.Local
    void Awake()
    {
        _grid = GetComponent<Grid>();
        map = new bool[(int)_grid.GridSizeX, (int)_grid.GridSizeY];
        VisualRange = 8;
        gameController = GetComponent<GameController>();
    }

    #region map point code

    /// <summary>
    /// Check if the provided coordinate is within the bounds of the mapp array
    /// </summary>
    /// <param name="pX"></param>
    /// <param name="pY"></param>
    /// <returns></returns>
    private bool Point_Valid(int pX, int pY)
    {
        return pX >= 0 & pX < map.GetLength(0)
                & pY >= 0 & pY < map.GetLength(1);
    }

    /// <summary>
    /// Called from the Statemachine script
    /// </summary>
    public void SetActiveUnits()
    {
        Units = gameController.ActiveUnits;
    }

    /// <summary>
    /// Get the value of the point at the specified location
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_y"></param>
    /// <returns>Cell value</returns>
    public bool Point_Get(int _x, int _y)
    {
        return map[_x, _y];
    }

    /// <summary>
    /// Set the map point to the specified value
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_y"></param>
    /// <param name="_val"></param>
    public void Point_Set(int _x, int _y, bool _val)
    {
        if (Point_Valid(_x, _y))
            map[_x, _y] = _val;
    }

    #endregion

    #region FOV algorithm

    //  Octant data
    //
    //    \ 1 | 2 /
    //   8 \  |  / 3
    //   -----+-----
    //   7 /  |  \ 4
    //    / 6 | 5 \
    //
    //  1 = NNW, 2 =NNE, 3=ENE, 4=ESE, 5=SSE, 6=SSW, 7=WSW, 8 = WNW

    /// <summary>
    /// Start here: go through all the octants which surround the player to
    /// determine which open cells are visible
    /// </summary>
    public void GetVisibleCells()
    {
        VisiblePoints = new List<Node>();
        foreach (Unit u in Units)
        {
            VisiblePoints.Add(u.GetCurrentNode());
            Node currentNode = u.GetCurrentNode();
            if ((int)u.GetLocalScaleX() == 1)
            {
                foreach (int o in VisibleOctantsWhenFacingRight)
                    ScanOctant(1, o, 1.0, 0.0, currentNode);
                int[] coord = _grid.GetNodeCoord(currentNode);
                VisiblePoints.Add(_grid.Nodes[coord[0], coord[1]+1]);
            }
            else
            {
                foreach (int o in VisibleOctantsWhenFacingLeft)
                    ScanOctant(1, o, 1.0, 0.0, currentNode);
                int[] coord = _grid.GetNodeCoord(currentNode);
                VisiblePoints.Add(_grid.Nodes[coord[0], coord[1]+1]);
            }
        }
    }

    /// <summary>
    /// Examine the provided octant and calculate the visible cells within it.
    /// </summary>
    /// <param name="pDepth">Depth of the scan</param>
    /// <param name="pOctant">Octant being examined</param>
    /// <param name="pStartSlope">Start slope of the octant</param>
    /// <param name="pEndSlope">End slope of the octance</param>
    protected void ScanOctant(int pDepth, int pOctant, double pStartSlope, double pEndSlope, Node unitPos)
    {

        int visrange2 = VisualRange * VisualRange;
        int x = 0;
        int y = 0;

        switch (pOctant)
        {

            case 1: //NNW
                y = unitPos.GridY - pDepth;
                if (y < 0) return;

                x = unitPos.GridX - Convert.ToInt32((pStartSlope * Convert.ToDouble(pDepth)));
                if (x < 0) x = 0;

                while (GetSlope(x, y, unitPos.GridX, unitPos.GridY, false) >= pEndSlope)
                {
                    if (GetVisDistance(x, y, unitPos.GridX, unitPos.GridY) <= visrange2)
                    {
                        if (map[x, y]) //current cell blocked
                        {
                            if (x - 1 >= 0 && !map[x - 1, y]) //prior cell within range AND open...
                                                              //...incremenet the depth, adjust the endslope and recurse
                                ScanOctant(pDepth + 1, pOctant, pStartSlope,
                                    GetSlope(x - 0.5, y + 0.5, unitPos.GridX, unitPos.GridY, false), unitPos);
                        }
                        else
                        {

                            if (x - 1 >= 0 && map[x - 1, y]) //prior cell within range AND open...
                                                             //..adjust the startslope
                                pStartSlope = GetSlope(x - 0.5, y - 0.5, unitPos.GridX, unitPos.GridY, false);

                            VisiblePoints.Add(_grid.Nodes[x, y]);
                        }
                    }
                    x++;
                }
                x--;
                break;

            case 2: //NNE

                y = unitPos.GridY - pDepth;
                if (y < 0) return;

                x = unitPos.GridX + Convert.ToInt32((pStartSlope * Convert.ToDouble(pDepth)));
                if (x >= map.GetLength(0)) x = map.GetLength(0) - 1;

                while (GetSlope(x, y, unitPos.GridX, unitPos.GridY, false) <= pEndSlope)
                {
                    if (GetVisDistance(x, y, unitPos.GridX, unitPos.GridY) <= visrange2)
                    {
                        if (map[x, y])
                        {
                            if (x + 1 < map.GetLength(0) && !map[x + 1, y])
                                ScanOctant(pDepth + 1, pOctant, pStartSlope, GetSlope(x + 0.5, y + 0.5, unitPos.GridX, unitPos.GridY, false), unitPos);
                        }
                        else
                        {
                            if (x + 1 < map.GetLength(0) && map[x + 1, y])
                                pStartSlope = -GetSlope(x + 0.5, y - 0.5, unitPos.GridX, unitPos.GridY, false);

                            VisiblePoints.Add(_grid.Nodes[x, y]);
                        }
                    }
                    x--;
                }
                x++;
                break;

            case 3: //ENE

                x = unitPos.GridX + pDepth;
                if (x >= map.GetLength(0)) return;

                y = unitPos.GridY - Convert.ToInt32((pStartSlope * Convert.ToDouble(pDepth)));
                if (y < 0) y = 0;

                while (GetSlope(x, y, unitPos.GridX, unitPos.GridY, true) <= pEndSlope)
                {

                    if (GetVisDistance(x, y, unitPos.GridX, unitPos.GridY) <= visrange2)
                    {

                        if (map[x, y])
                        {
                            if (y - 1 >= 0 && !map[x, y - 1])
                                ScanOctant(pDepth + 1, pOctant, pStartSlope, GetSlope(x - 0.5, y - 0.5, unitPos.GridX, unitPos.GridY, true), unitPos);
                        }
                        else
                        {
                            if (y - 1 >= 0 && map[x, y - 1])
                                pStartSlope = -GetSlope(x + 0.5, y - 0.5, unitPos.GridX, unitPos.GridY, true);

                            VisiblePoints.Add(_grid.Nodes[x, y]);
                        }
                    }
                    y++;
                }
                y--;
                break;

            case 4: //ESE

                x = unitPos.GridX + pDepth;
                if (x >= map.GetLength(0)) return;

                y = unitPos.GridY + Convert.ToInt32((pStartSlope * Convert.ToDouble(pDepth)));
                if (y >= map.GetLength(1)) y = map.GetLength(1) - 1;

                while (GetSlope(x, y, unitPos.GridX, unitPos.GridY, false) >= pEndSlope)
                {

                    if (GetVisDistance(x, y, unitPos.GridX, unitPos.GridY) <= visrange2)
                    {

                        if (map[x, y])
                        {
                            if (y + 1 < map.GetLength(1) && !map[x, y + 1])
                                ScanOctant(pDepth + 1, pOctant, pStartSlope, GetSlope(x - 0.5, y + 0.5, unitPos.GridX, unitPos.GridY, true), unitPos);
                        }
                        else
                        {
                            if (y + 1 < map.GetLength(1) && map[x, y + 1])
                                pStartSlope = GetSlope(x + 0.5, y + 0.5, unitPos.GridX, unitPos.GridY, true);

                            VisiblePoints.Add(_grid.Nodes[x, y]);
                        }
                    }
                    y--;
                }
                y++;
                break;

            case 5: //SSE

                y = unitPos.GridY + pDepth;
                if (y >= map.GetLength(1)) return;

                x = unitPos.GridX + Convert.ToInt32((pStartSlope * Convert.ToDouble(pDepth)));
                if (x >= map.GetLength(0)) x = map.GetLength(0) - 1;

                while (GetSlope(x, y, unitPos.GridX, unitPos.GridY, false) >= pEndSlope)
                {
                    if (GetVisDistance(x, y, unitPos.GridX, unitPos.GridY) <= visrange2)
                    {

                        if (map[x, y])
                        {
                            if (x + 1 < map.GetLength(1) && !map[x + 1, y])
                                ScanOctant(pDepth + 1, pOctant, pStartSlope, GetSlope(x + 0.5, y - 0.5, unitPos.GridX, unitPos.GridY, false), unitPos);
                        }
                        else
                        {
                            if (x + 1 < map.GetLength(1)
                                    && map[x + 1, y])
                                pStartSlope = GetSlope(x + 0.5, y + 0.5, unitPos.GridX, unitPos.GridY, false);

                            VisiblePoints.Add(_grid.Nodes[x, y]);
                        }
                    }
                    x--;
                }
                x++;
                break;

            case 6: //SSW

                y = unitPos.GridY + pDepth;
                if (y >= map.GetLength(1)) return;

                x = unitPos.GridX - Convert.ToInt32((pStartSlope * Convert.ToDouble(pDepth)));
                if (x < 0) x = 0;

                while (GetSlope(x, y, unitPos.GridX, unitPos.GridY, false) <= pEndSlope)
                {
                    if (GetVisDistance(x, y, unitPos.GridX, unitPos.GridY) <= visrange2)
                    {

                        if (map[x, y])
                        {
                            if (x - 1 >= 0 && !map[x - 1, y])
                                ScanOctant(pDepth + 1, pOctant, pStartSlope, GetSlope(x - 0.5, y - 0.5, unitPos.GridX, unitPos.GridY, false), unitPos);
                        }
                        else
                        {
                            if (x - 1 >= 0
                                    && map[x - 1, y])
                                pStartSlope = -GetSlope(x - 0.5, y + 0.5, unitPos.GridX, unitPos.GridY, false);

                            VisiblePoints.Add(_grid.Nodes[x, y]);
                        }
                    }
                    x++;
                }
                x--;
                break;

            case 7: //WSW

                x = unitPos.GridX - pDepth;
                if (x < 0) return;

                y = unitPos.GridY + Convert.ToInt32((pStartSlope * Convert.ToDouble(pDepth)));
                if (y >= map.GetLength(1)) y = map.GetLength(1) - 1;

                while (GetSlope(x, y, unitPos.GridX, unitPos.GridY, true) <= pEndSlope)
                {

                    if (GetVisDistance(x, y, unitPos.GridX, unitPos.GridY) <= visrange2)
                    {

                        if (map[x, y])
                        {
                            if (y + 1 < map.GetLength(1) && !map[x, y + 1])
                                ScanOctant(pDepth + 1, pOctant, pStartSlope, GetSlope(x + 0.5, y + 0.5, unitPos.GridX, unitPos.GridY, true), unitPos);
                        }
                        else
                        {
                            if (y + 1 < map.GetLength(1) && map[x, y + 1])
                                pStartSlope = -GetSlope(x - 0.5, y + 0.5, unitPos.GridX, unitPos.GridY, true);

                            VisiblePoints.Add(_grid.Nodes[x, y]);
                        }
                    }
                    y--;
                }
                y++;
                break;

            case 8: //WNW

                x = unitPos.GridX - pDepth;
                if (x < 0) return;

                y = unitPos.GridY - Convert.ToInt32((pStartSlope * Convert.ToDouble(pDepth)));
                if (y < 0) y = 0;

                while (GetSlope(x, y, unitPos.GridX, unitPos.GridY, true) >= pEndSlope)
                {

                    if (GetVisDistance(x, y, unitPos.GridX, unitPos.GridY) <= visrange2)
                    {

                        if (map[x, y])
                        {
                            if (y - 1 >= 0 && !map[x, y - 1])
                                ScanOctant(pDepth + 1, pOctant, pStartSlope, GetSlope(x + 0.5, y - 0.5, unitPos.GridX, unitPos.GridY, true), unitPos);

                        }
                        else
                        {
                            if (y - 1 >= 0 && map[x, y - 1])
                                pStartSlope = GetSlope(x - 0.5, y - 0.5, unitPos.GridX, unitPos.GridY, true);

                            VisiblePoints.Add(_grid.Nodes[x, y]);
                        }
                    }
                    y++;
                }
                y--;
                break;
        }


        if (x < 0)
            x = 0;
        else if (x >= map.GetLength(0))
            x = map.GetLength(0) - 1;

        if (y < 0)
            y = 0;
        else if (y >= map.GetLength(1))
            y = map.GetLength(1) - 1;

        if (pDepth < VisualRange & !map[x, y])
            ScanOctant(pDepth + 1, pOctant, pStartSlope, pEndSlope, unitPos);

    }

    /// <summary>
    /// Get the gradient of the slope formed by the two points
    /// </summary>
    /// <param name="pX1"></param>
    /// <param name="pY1"></param>
    /// <param name="pX2"></param>
    /// <param name="pY2"></param>
    /// <param name="pInvert">Invert slope</param>
    /// <returns></returns>
    private double GetSlope(double pX1, double pY1, double pX2, double pY2, bool pInvert)
    {
        if (pInvert)
            return (pY1 - pY2) / (pX1 - pX2);
        else
            return (pX1 - pX2) / (pY1 - pY2);
    }


    /// <summary>
    /// Calculate the distance between the two points
    /// </summary>
    /// <param name="pX1"></param>
    /// <param name="pY1"></param>
    /// <param name="pX2"></param>
    /// <param name="pY2"></param>
    /// <returns>Distance</returns>
    private int GetVisDistance(int pX1, int pY1, int pX2, int pY2)
    {
        return ((pX1 - pX2) * (pX1 - pX2)) + ((pY1 - pY2) * (pY1 - pY2));
    }

    #endregion


    //event raised when a player has successfully moved
    public delegate void moveDelegate();
    public event moveDelegate playerMoved;
}
