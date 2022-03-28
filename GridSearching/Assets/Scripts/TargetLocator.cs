
// Authors: WeAreQiiwi International (andreasa@qiiwi.com, jack@qiiwi.com)
// Created: 12/05/2020
// Copyright (c) 2020 WeAreQiiwi International

using UnityEngine;

public static class TargetLocator
{
	public static int Cost {get; private set;}
	
    /// <summary>
    /// Enum representing a direction in the grid
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
        OnTarget
    }

    private static Vector2Int targetGridPosition;
    private static readonly Direction[,] directionMap = {{Direction.UpLeft, Direction.Up, Direction.UpRight},
                                               { Direction.Left, Direction.OnTarget, Direction.Right },
                                               { Direction.DownLeft, Direction.Down, Direction.DownRight} };

    /// <summary>
    /// Generates a new target position based on the specified grid size
    /// </summary>
    /// <param name="gridSize">The size of the grid</param>
    /// <returns></returns>
    public static Vector2Int GenerateTargetGridPosition(Vector2Int gridSize)
    {
        int x = Random.Range(1, gridSize.x);
        int y = Random.Range(1, gridSize.y);

        targetGridPosition = new Vector2Int(x, y);
		Cost = 0;
        return targetGridPosition;
    }

    /// <summary>
    /// Returns the direction of the target relative to a position in a grid
    /// </summary>
    /// <param name="gridPosition">The position in the grid to which a direction to the target is going to be returned</param>
    /// <returns></returns>
    public static Direction GetDirectionToTarget(Vector2Int gridPosition)
    {
        int diffX = Mathf.Clamp(targetGridPosition.x - gridPosition.x, -1, 1) + 1;
        int diffY = Mathf.Clamp(targetGridPosition.y - gridPosition.y, -1, 1) + 1;
		
		Cost++;

        return directionMap[diffX, diffY];
    }
}
