using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameAI : MonoBehaviour
{
    private BoardController _board;

    private void Start()
    {
        _board = GetComponent<BoardController>();
    }

    private bool CanPlaceStone(Vector2 pos)
    {
        bool canPlace = _board.CanPlaceStone(_board.tiles[pos], TileType.White) == string.Empty;
        return canPlace;
    }

    public Vector2 MakeMove(Dictionary<Vector2, TileType> board)
    {
        List<Vector2> validMoves = new List<Vector2>();
        Vector2 center = new Vector2(_board.size / 2, _board.size / 2);

        foreach (var position in board.Keys)
        {
            TileType type = _board.tiles[position].type;
            if (CanPlaceStone(position))
            {
                validMoves.Add(position);
            }
            _board.tiles[position].type = type;
            _board.turnCapturedTiles?.Clear();
        }

        if (validMoves.Count == 0)
        {
            return Vector2.zero;
        }

        List<float> probabilities = new List<float>();
        foreach (var position in validMoves)
        {
            float distanceToCenter = Vector2.Distance(position, center);
            float probability = 1.0f / (distanceToCenter + 1);
            probabilities.Add(probability);
        }

        float totalProbability = 0;
        foreach (float probability in probabilities)
        {
            totalProbability += probability;
        }

        for (int i = 0; i < probabilities.Count; i++)
        {
            probabilities[i] /= totalProbability;
        }

        int selectedIndex = WeightedRandom(probabilities);
        return validMoves[selectedIndex];
    }

    private int WeightedRandom(List<float> probabilities)
    {
        float rand = Random.Range(0f, 1f);
        float cumulativeProbability = 0;
        for (int i = 0; i < probabilities.Count; i++)
        {
            cumulativeProbability += probabilities[i];
            if (rand <= cumulativeProbability)
            {
                return i;
            }
        }
        return probabilities.Count - 1;
    }

    public (float puntuacionBlack, float puntuacionWhite) CalcularPuntuacion(Dictionary<Vector2, TileType> tileMap)
    {
        float puntuacionBlack = 0f;
        float puntuacionWhite = 7.5f; // Komi

        foreach (var position in tileMap.Keys)
        {
            TileType type = tileMap[position];

            // Contar las fichas de Black y White
            if (type == TileType.Black)
            {
                puntuacionBlack++;
            }
            else if (type == TileType.White)
            {
                puntuacionWhite++;
            }
        }

        // Diccionario para rastrear territorios visitados
        Dictionary<Vector2, bool> visited = new Dictionary<Vector2, bool>();

        foreach (var position in tileMap.Keys)
        {
            if (!visited.ContainsKey(position) && tileMap[position] == TileType.Liberty)
            {
                // Iniciar búsqueda de territorio
                bool isBlackTerritory = true;
                bool isWhiteTerritory = true;

                Queue<Vector2> queue = new Queue<Vector2>();
                queue.Enqueue(position);

                HashSet<Vector2> currentTerritory = new HashSet<Vector2>();

                while (queue.Count > 0)
                {
                    Vector2 current = queue.Dequeue();
                    visited[current] = true;
                    currentTerritory.Add(current);

                    Vector2[] neighbors = new Vector2[]
                    {
                        new Vector2(current.x + 1, current.y),
                        new Vector2(current.x - 1, current.y),
                        new Vector2(current.x, current.y + 1),
                        new Vector2(current.x, current.y - 1)
                    };

                    foreach (var neighbor in neighbors)
                    {
                        if (!tileMap.ContainsKey(neighbor))
                        {
                            // Fuera de los límites del tablero
                            continue;
                        }

                        TileType neighborType = tileMap[neighbor];

                        if (neighborType == TileType.Black)
                        {
                            isWhiteTerritory = false;
                        }
                        else if (neighborType == TileType.White)
                        {
                            isBlackTerritory = false;
                        }
                        else if (neighborType == TileType.Liberty && !visited.ContainsKey(neighbor) && !queue.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                // Contar el territorio
                if (isBlackTerritory)
                {
                    puntuacionBlack += currentTerritory.Count;
                }
                else if (isWhiteTerritory)
                {
                    puntuacionWhite += currentTerritory.Count;
                }
            }
        }

        return (puntuacionBlack, puntuacionWhite);
    }

}
