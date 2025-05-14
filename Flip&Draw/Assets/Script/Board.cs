using Lacobus.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Board : MonoBehaviour
{
    // Fields

    [SerializeField] private Vector2Int _gridDimension;
    [SerializeField] private Vector2 _cellDimension;
    [SerializeField] private GameObject _whiteCoinPrefab;
    [SerializeField] private GameObject _blackCoinPrefab;
    [SerializeField] private GameObject _blackMarkerPrefab;
    [SerializeField] private GameObject _whiteMarkerPrefab;
    [Range(0.001f, 0.2f)]
    [SerializeField] private float _coinRollSpeed;

    private Grid<BoardData> _grid;

    private Transform _t;
    private Camera _camera;

    private CoinFace _latestFace;
    private Vector2Int _latestPoint;

    private List<Vector2Int> _cachedBlackPoints = null;
    private List<Vector2Int> _cachedWhitePoints = null;

    private GameObject _markerPlaceholder;

    private bool _canPlay = true;
    private int _coinsPlaced = 0;


    // Properties

    private Vector3 gridOrigin => _t.position - new Vector3((_gridDimension.x * _cellDimension.x) / 2, (_gridDimension.y * _cellDimension.y) / 2);


    // Public methods

    public bool PlaceCoinOnBoard(CoinFace face)
    {
        var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);

        if (_grid.ConvertToXY(mousePos, out Vector2Int index) && _grid.GetCellData(mousePos).isOccupied == false)
        {
            if (face == CoinFace.black)
            {
                if (_cachedBlackPoints.Contains(index) == false)
                    return false;
            }
            else
            {
                if (_cachedWhitePoints.Contains(index) == false)
                    return false;
            }

            var coin = makeCoin(face, _grid.GetCellCenter(index));

            _grid.GetCellData(index).isOccupied = true;
            _grid.GetCellData(index).coin = coin;

            _latestPoint = index;
            _latestFace = face;

            StartCoroutine(updateCoinCaptures());

            clearEligibleMarkers();

            return true;
        }
        else
            return false;
    }

    public Vector2Int GetCoinCount()
    {
        int blacks = 0, whites = 0;

        for (int i = 0; i < _grid.GridDimension.x; ++i)
        {
            for (int j = 0; j < _grid.GridDimension.y; ++j)
            {
                if (_grid.GetCellData(i, j).isOccupied)
                {
                    if (_grid.GetCellData(i, j).coin.GetFace() == CoinFace.black)
                        ++blacks;
                    else
                        ++whites;
                }
            }
        }

        return new Vector2Int(blacks, whites);
    }

    public bool UpdateEligiblePositions(CoinFace face)
    {
        switch (face)
        {
            case CoinFace.black:
                if (_cachedBlackPoints == null)
                {
                    if (_cachedWhitePoints != null)
                        _cachedWhitePoints = null;

                    _cachedBlackPoints = getAllEligiblePosition(CoinFace.black);

                    if (_cachedBlackPoints.Count == 0)
                        return false;

                    // Draw new ones
                    drawNewEligibleMarkers(_cachedBlackPoints, CoinFace.black);
                }
                break;
            case CoinFace.white:
                if (_cachedWhitePoints == null)
                {
                    if (_cachedBlackPoints != null)
                        _cachedBlackPoints = null;

                    _cachedWhitePoints = getAllEligiblePosition(CoinFace.white);

                    if (_cachedWhitePoints.Count == 0)
                        return false;

                    // Draw new ones
                    drawNewEligibleMarkers(_cachedWhitePoints, CoinFace.white);
                }
                break;
        }

        return true;
    }

    public bool CanPlay() => _canPlay;

    public bool IsFull()
    {
        if (_coinsPlaced == 64)
        {
            _canPlay = false;
            return true;
        }
        else return false;
    }


    // Private methods

    private Coin makeCoin(CoinFace face, Vector3 worldPosition)
    {
        ++_coinsPlaced;

        switch (face)
        {
            case CoinFace.black:
                return Instantiate(_blackCoinPrefab, worldPosition, Quaternion.identity, _t).GetComponent<Coin>();
            case CoinFace.white:
                return Instantiate(_whiteCoinPrefab, worldPosition, Quaternion.identity, _t).GetComponent<Coin>();
            default:
                return null;
        }
    }

    private void makeMark(Vector3 worldPosition, CoinFace face)
    {
        switch (face)
        {
            case CoinFace.black:
                Instantiate(_blackMarkerPrefab, worldPosition, Quaternion.identity, _markerPlaceholder.transform);
                break;
            case CoinFace.white:
                Instantiate(_whiteMarkerPrefab, worldPosition, Quaternion.identity, _markerPlaceholder.transform);
                break;
        }
    }

    private void initBoard()
    {
        int xCenter = _grid.GridDimension.x / 2;
        int yCenter = _grid.GridDimension.y / 2;

        var coin_1 = makeCoin(CoinFace.black, _grid.GetCellCenter(xCenter, yCenter));
        var coin_2 = makeCoin(CoinFace.black, _grid.GetCellCenter(xCenter - 1, yCenter - 1));
        var coin_3 = makeCoin(CoinFace.white, _grid.GetCellCenter(xCenter - 1, yCenter));
        var coin_4 = makeCoin(CoinFace.white, _grid.GetCellCenter(xCenter, yCenter - 1));

        _grid.GetCellData(xCenter, yCenter).isOccupied = true;
        _grid.GetCellData(xCenter, yCenter).coin = coin_1;

        _grid.GetCellData(xCenter - 1, yCenter - 1).isOccupied = true;
        _grid.GetCellData(xCenter - 1, yCenter - 1).coin = coin_2;

        _grid.GetCellData(xCenter - 1, yCenter).isOccupied = true;
        _grid.GetCellData(xCenter - 1, yCenter).coin = coin_3;

        _grid.GetCellData(xCenter, yCenter - 1).isOccupied = true;
        _grid.GetCellData(xCenter, yCenter - 1).coin = coin_4;

        _canPlay = true;
    }

    private Dictionary<int, List<Vector2Int>> getHorizontalCoinsToBeCaptured()
    {
        bool shouldFlipCoin;
        List<Vector2Int> coinsArray = null;
        Dictionary<int, List<Vector2Int>> coinsToBeFlipped = new Dictionary<int, List<Vector2Int>>();


        // Right
        shouldFlipCoin = false;
        coinsArray = new List<Vector2Int>();
        for (int x = _latestPoint.x + 1; x < _grid.GridDimension.x; ++x)
        {
            if (_grid.GetCellData(x, _latestPoint.y).isOccupied == false)
                break;
            if (_grid.GetCellData(x, _latestPoint.y).coin.GetFace() != _latestFace)
            {
                shouldFlipCoin = true;
                continue;
            }
            else
            {
                if (shouldFlipCoin == false)
                    break;

                for (int i = _latestPoint.x + 1; i < x; ++i)
                    coinsArray.Add(new Vector2Int(i, _latestPoint.y));
                break;
            }
        }
        coinsToBeFlipped.Add(0, coinsArray);

        // Left
        shouldFlipCoin = false;
        coinsArray = new List<Vector2Int>();
        for (int x = _latestPoint.x - 1; x >= 0; --x)
        {
            if (_grid.GetCellData(x, _latestPoint.y).isOccupied == false)
                break;
            if (_grid.GetCellData(x, _latestPoint.y).coin.GetFace() != _latestFace)
            {
                shouldFlipCoin = true;
                continue;
            }
            else
            {
                if (shouldFlipCoin == false)
                    break;

                for (int i = _latestPoint.x - 1; i > x; --i)
                    coinsArray.Add(new Vector2Int(i, _latestPoint.y));
                break;
            }
        }
        coinsToBeFlipped.Add(1, coinsArray);

        return coinsToBeFlipped;
    }

    private Dictionary<int, List<Vector2Int>> getVerticalCoinsToBeCaptured()
    {
        bool shouldFlipCoin;
        List<Vector2Int> coinsArray = null;
        Dictionary<int, List<Vector2Int>> coinsToBeFlipped = new Dictionary<int, List<Vector2Int>>();

        // Up
        shouldFlipCoin = false;
        coinsArray = new List<Vector2Int>();
        for (int y = _latestPoint.y + 1; y < _grid.GridDimension.y; ++y)
        {
            if (_grid.GetCellData(_latestPoint.x, y).isOccupied == false)
                break;
            if (_grid.GetCellData(_latestPoint.x, y).coin.GetFace() != _latestFace)
            {
                shouldFlipCoin = true;
                continue;
            }
            else
            {
                if (shouldFlipCoin == false)
                    break;

                for (int i = _latestPoint.y + 1; i < y; ++i)
                    coinsArray.Add(new Vector2Int(_latestPoint.x, i));
                break;
            }
        }
        coinsToBeFlipped.Add(0, coinsArray);

        // Down
        shouldFlipCoin = false;
        coinsArray = new List<Vector2Int>();
        for (int y = _latestPoint.y - 1; y >= 0; --y)
        {
            if (_grid.GetCellData(_latestPoint.x, y).isOccupied == false)
                break;
            if (_grid.GetCellData(_latestPoint.x, y).coin.GetFace() != _latestFace)
            {
                shouldFlipCoin = true;
                continue;
            }
            else
            {
                if (shouldFlipCoin == false)
                    break;

                for (int i = _latestPoint.y - 1; i > y; --i)
                    coinsArray.Add(new Vector2Int(_latestPoint.x, i));
                break;
            }
        }
        coinsToBeFlipped.Add(1, coinsArray);

        return coinsToBeFlipped;
    }

    private Dictionary<int, List<Vector2Int>> getDiagonalCoinsToBeCaptured()
    {
        bool shouldFlipCoin;
        List<Vector2Int> coinsArray = null;
        Dictionary<int, List<Vector2Int>> coinsToBeFlipped = new Dictionary<int, List<Vector2Int>>();

        // Up right
        shouldFlipCoin = false;
        coinsArray = new List<Vector2Int>();
        for (int x = _latestPoint.x + 1, y = _latestPoint.y + 1; x < _grid.GridDimension.x && y < _grid.GridDimension.y; ++x, ++y)
        {
            if (_grid.GetCellData(x, y).isOccupied == false)
                break;
            if (_grid.GetCellData(x, y).coin.GetFace() != _latestFace)
            {
                shouldFlipCoin = true;
                continue;
            }
            else
            {
                if (shouldFlipCoin == false)
                    break;

                for (int i = _latestPoint.x + 1, j = _latestPoint.y + 1; i < x && j < y; ++i, ++j)
                    coinsArray.Add(new Vector2Int(i, j));
                break;
            }
        }
        coinsToBeFlipped.Add(0, coinsArray);

        // Up left
        shouldFlipCoin = false;
        coinsArray = new List<Vector2Int>();
        for (int x = _latestPoint.x - 1, y = _latestPoint.y + 1; x >= 0 && y < _grid.GridDimension.y; --x, ++y)
        {
            if (_grid.GetCellData(x, y).isOccupied == false)
                break;
            if (_grid.GetCellData(x, y).coin.GetFace() != _latestFace)
            {
                shouldFlipCoin = true;
                continue;
            }
            else
            {
                if (shouldFlipCoin == false)
                    break;

                for (int i = _latestPoint.x - 1, j = _latestPoint.y + 1; i > x; --i, ++j)
                    coinsArray.Add(new Vector2Int(i, j));
                break;
            }
        }
        coinsToBeFlipped.Add(1, coinsArray);

        // Down left
        shouldFlipCoin = false;
        coinsArray = new List<Vector2Int>();
        for (int x = _latestPoint.x - 1, y = _latestPoint.y - 1; x >= 0 && y >= 0; --x, --y)
        {
            if (_grid.GetCellData(x, y).isOccupied == false)
                break;
            if (_grid.GetCellData(x, y).coin.GetFace() != _latestFace)
            {
                shouldFlipCoin = true;
                continue;
            }
            else
            {
                if (shouldFlipCoin == false)
                    break;

                for (int i = _latestPoint.x - 1, j = _latestPoint.y - 1; i > x && j > y; --i, --j)
                    coinsArray.Add(new Vector2Int(i, j));
                break;
            }
        }
        coinsToBeFlipped.Add(2, coinsArray);

        // Down right
        shouldFlipCoin = false;
        coinsArray = new List<Vector2Int>();
        for (int x = _latestPoint.x + 1, y = _latestPoint.y - 1; x < _grid.GridDimension.x && y >= 0; ++x, --y)
        {
            if (_grid.GetCellData(x, y).isOccupied == false)
                break;
            if (_grid.GetCellData(x, y).coin.GetFace() != _latestFace)
            {
                shouldFlipCoin = true;
                continue;
            }
            else
            {
                if (shouldFlipCoin == false)
                    break;

                for (int i = _latestPoint.x + 1, j = _latestPoint.y - 1; i < x && j > y; ++i, --j)
                    coinsArray.Add(new Vector2Int(i, j));
                break;
            }
        }
        coinsToBeFlipped.Add(3, coinsArray);

        return coinsToBeFlipped;
    }

    private IEnumerator updateCoinCaptures()
    {
        _canPlay = false;

        var hor = getHorizontalCoinsToBeCaptured();
        var ver = getVerticalCoinsToBeCaptured();
        var dia = getDiagonalCoinsToBeCaptured();

        var r = hor[0];
        var l = hor[1];
        var u = ver[0];
        var d = ver[1];
        var ur = dia[0];
        var ul = dia[1];
        var dl = dia[2];
        var dr = dia[3];

        for (int i = 0; i < 8; ++i)
        {
            // Horizontal
            if (i < r.Count)
                _grid.GetCellData(r[i]).coin.FlipFace();
            if (i < l.Count)
                _grid.GetCellData(l[i]).coin.FlipFace();

            // Vertical
            if (i < u.Count)
                _grid.GetCellData(u[i]).coin.FlipFace();
            if (i < d.Count)
                _grid.GetCellData(d[i]).coin.FlipFace();

            // Diagonal
            if (i < ur.Count)
                _grid.GetCellData(ur[i]).coin.FlipFace();
            if (i < ul.Count)
                _grid.GetCellData(ul[i]).coin.FlipFace();
            if (i < dl.Count)
                _grid.GetCellData(dl[i]).coin.FlipFace();
            if (i < dr.Count)
                _grid.GetCellData(dr[i]).coin.FlipFace();

            yield return new WaitForSeconds(_coinRollSpeed);
        }

        _canPlay = true;
    }

    private List<Vector2Int> getAllEligiblePosition(CoinFace face)
    {
        List<Vector2Int> points = new List<Vector2Int>();
        bool shouldFlip = false;

        for (int x = 0; x < _grid.GridDimension.x; ++x)
        {
            for (int y = 0; y < _grid.GridDimension.y; ++y)
            {
                if (_grid.GetCellData(x, y).isOccupied == true && _grid.GetCellData(x, y).coin.GetFace() == face)
                {
                    Vector2Int targetPoint = new Vector2Int(x, y);

                    // Horizontals
                    // Right
                    shouldFlip = false;
                    for (int i = targetPoint.x + 1; i < _grid.GridDimension.x; ++i)
                    {
                        if (_grid.GetCellData(i, targetPoint.y).isOccupied)
                        {
                            if (_grid.GetCellData(i, targetPoint.y).coin.GetFace() != face)
                                shouldFlip = true;
                            else
                                break;
                        }
                        else
                        {
                            if (shouldFlip)
                            {
                                points.Add(new Vector2Int(i, targetPoint.y));
                                break;
                            }
                            else
                                break;
                        }
                    }

                    // Left
                    shouldFlip = false;
                    for (int i = targetPoint.x - 1; i >= 0; --i)
                    {
                        if (_grid.GetCellData(i, targetPoint.y).isOccupied)
                        {
                            if (_grid.GetCellData(i, targetPoint.y).coin.GetFace() != face)
                                shouldFlip = true;
                            else
                                break;
                        }
                        else
                        {
                            if (shouldFlip)
                            {
                                points.Add(new Vector2Int(i, targetPoint.y));
                                break;
                            }
                            else
                                break;
                        }
                    }


                    // Verticals
                    // Up
                    shouldFlip = false;
                    for (int i = targetPoint.y + 1; i < _grid.GridDimension.y; ++i)
                    {
                        if (_grid.GetCellData(targetPoint.x, i).isOccupied)
                        {
                            if (_grid.GetCellData(targetPoint.x, i).coin.GetFace() != face)
                                shouldFlip = true;
                            else
                                break;
                        }
                        else
                        {
                            if (shouldFlip)
                            {
                                points.Add(new Vector2Int(targetPoint.x, i));
                                break;
                            }
                            else
                                break;
                        }
                    }

                    // Down
                    shouldFlip = false;
                    for (int i = targetPoint.y - 1; i >= 0; --i)
                    {
                        if (_grid.GetCellData(targetPoint.x, i).isOccupied)
                        {
                            if (_grid.GetCellData(targetPoint.x, i).coin.GetFace() != face)
                                shouldFlip = true;
                            else
                                break;
                        }
                        else
                        {
                            if (shouldFlip)
                            {
                                points.Add(new Vector2Int(targetPoint.x, i));
                                break;
                            }
                            else
                                break;
                        }
                    }


                    // Diagonals
                    // Up right
                    shouldFlip = false;
                    for (int i = targetPoint.x + 1, j = targetPoint.y + 1; i < _grid.GridDimension.x && j < _grid.GridDimension.y; ++i, ++j)
                    {
                        if (_grid.GetCellData(i, j).isOccupied)
                        {
                            if (_grid.GetCellData(i, j).coin.GetFace() != face)
                                shouldFlip = true;
                            else
                                break;
                        }
                        else
                        {
                            if (shouldFlip)
                            {
                                points.Add(new Vector2Int(i, j));
                                break;
                            }
                            else
                                break;
                        }
                    }

                    // Up left
                    shouldFlip = false;
                    for (int i = targetPoint.x - 1, j = targetPoint.y + 1; i >= 0 && j < _grid.GridDimension.y; --i, ++j)
                    {
                        if (_grid.GetCellData(i, j).isOccupied)
                        {
                            if (_grid.GetCellData(i, j).coin.GetFace() != face)
                                shouldFlip = true;
                            else
                                break;
                        }
                        else
                        {
                            if (shouldFlip)
                            {
                                points.Add(new Vector2Int(i, j));
                                break;
                            }
                            else
                                break;
                        }
                    }

                    // Down left
                    shouldFlip = false;
                    for (int i = targetPoint.x - 1, j = targetPoint.y - 1; i >= 0 && j >= 0; --i, --j)
                    {
                        if (_grid.GetCellData(i, j).isOccupied)
                        {
                            if (_grid.GetCellData(i, j).coin.GetFace() != face)
                                shouldFlip = true;
                            else
                                break;
                        }
                        else
                        {
                            if (shouldFlip)
                            {
                                points.Add(new Vector2Int(i, j));
                                break;
                            }
                            else
                                break;
                        }
                    }

                    // Down right
                    shouldFlip = false;
                    for (int i = targetPoint.x + 1, j = targetPoint.y - 1; i < _grid.GridDimension.x && j >= 0; ++i, --j)
                    {
                        if (_grid.GetCellData(i, j).isOccupied)
                        {
                            if (_grid.GetCellData(i, j).coin.GetFace() != face)
                                shouldFlip = true;
                            else
                                break;
                        }
                        else
                        {
                            if (shouldFlip)
                            {
                                points.Add(new Vector2Int(i, j));
                                break;
                            }
                            else
                                break;
                        }
                    }
                }
            }
        }

        return points;
    }

    private void clearEligibleMarkers()
    {
        destroyPlaceholderChildren();
    }

    private void drawNewEligibleMarkers(List<Vector2Int> eligiblePoints, CoinFace face)
    {
        foreach (var p in eligiblePoints)
            makeMark(_grid.GetCellCenter(p), face);
    }

    private void destroyPlaceholderChildren()
    {
        var t = _markerPlaceholder.transform;
        while (t.childCount > 0)
            DestroyImmediate(t.GetChild(0).gameObject);
    }


    // Lifecycle methods

    private void Awake()
    {
        _canPlay = false;
        _t = transform;
        _camera = Camera.main;
        _markerPlaceholder = new GameObject("-Marker Placeholder-");

        _grid = new Grid<BoardData>(gridOrigin, _gridDimension, _cellDimension);
        _grid.PrepareGrid();
        initBoard();
    }
}

public class BoardData
{
    public bool isOccupied = false;
    public Coin coin;
}