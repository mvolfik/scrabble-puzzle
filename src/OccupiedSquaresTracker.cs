namespace ScrabblePuzzleGenerator;

/// <summary>
/// Utility class for keeping track of which squares on a grid can be used and which not.
/// 
/// Grid tiles can be in 3 states:
/// - free
/// - occupied by marker X (byte in range 0-253 inclusive)
/// - completely blocked (occupied by 2+ markers)
/// 
/// Create an instance of this class with specific size, then use MarkSquares() to mark squares under and neigboring a word.
/// 
/// Then use IsOccupied() to check if a square is free or not. You can also use the helper method IsOccupiedRelative(),
/// which takes word direction, and the axes of the coordinates are relative to the word.
/// </summary>
class OccupiedSquaresTracker
{
    /// <summary>
    /// Marker for free square
    /// 
    /// For occupied squares, PuzzleGenerator stores the index of the word that is blocking the square. Therefore we can't use low
    /// value here, however, we can safely assume that the ciphertext won't be longer than 253 words,
    /// (they wouldn't fit on the grid anyways), so byte.MaxValue is safe.
    /// </summary>
    const byte FreeSquare = byte.MaxValue;
    /// <summary>
    /// Square unusable under any circumstances
    /// </summary>
    const byte SquareNeighboringMultipleWords = byte.MaxValue - 1;


    readonly byte[,] grid;
    readonly int size;

    public OccupiedSquaresTracker(int size)
    {
        grid = new byte[size, size];
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                grid[x, y] = FreeSquare;
    }

    private OccupiedSquaresTracker(byte[,] grid)
    {
        this.grid = grid;
        size = grid.GetLength(0);
    }

    /// <summary>
    /// Deep copy of the grid. Relatively cheap - only a copy of byte[,], which is usually optimized to memcpy.
    /// </summary>
    public OccupiedSquaresTracker Clone()
    {
        return new OccupiedSquaresTracker((byte[,])grid.Clone());
    }

    /// <summary>
    /// Marks given square as blocked by given marker.
    /// 
    /// If the square is already blocked by another marker, it is directly marked as completely unusable.
    /// </summary>
    public void MarkSquare(int x, int y, byte marker)
    {
        if (marker == FreeSquare || marker == SquareNeighboringMultipleWords)
            throw new System.ArgumentException("Invalid marker value", nameof(marker));

        if (grid[x, y] == FreeSquare)
            grid[x, y] = marker;
        else
            grid[x, y] = SquareNeighboringMultipleWords;
    }

    /// <summary>
    /// Marks squares under and around give word as occupied, with given wordIndex
    /// </summary>
    public void MarkSquares(WordPositionDefinition pos, byte marker)
    {
        if (pos.direction == Direction.Right)
        {
            for (int x = pos.startX; x < pos.startX + pos.length; x++)
            {
                MarkSquare(x, pos.startY, marker);
                if (pos.startY > 0)
                    MarkSquare(x, pos.startY - 1, marker);
                if (pos.startY < size - 1)
                    MarkSquare(x, pos.startY + 1, marker);
            }
            if (pos.startX > 0)
                MarkSquare(pos.startX - 1, pos.startY, marker);
            if (pos.startX + pos.length < size)
                MarkSquare(pos.startX + pos.length, pos.startY, marker);

        }
        else
        {
            for (int y = pos.startY; y < pos.startY + pos.length; y++)
            {
                MarkSquare(pos.startX, y, marker);
                if (pos.startX > 0)
                    MarkSquare(pos.startX - 1, y, marker);
                if (pos.startX < size - 1)
                    MarkSquare(pos.startX + 1, y, marker);
            }
            if (pos.startY > 0)
                MarkSquare(pos.startX, pos.startY - 1, marker);
            if (pos.startY + pos.length < size)
                MarkSquare(pos.startX, pos.startY + pos.length, marker);
        }
    }

    /// <summary>
    /// Checks if given tile is free or only blocked by given marker.
    /// </summary>
    public bool IsOccupied(int x, int y, byte allowedMarker)
    {
        return grid[x, y] != FreeSquare && grid[x, y] != allowedMarker;
    }

    /// <summary>
    /// If the direction is to the right, the directionCoord is X, and perpendicular coord is Y. When the direction is Down, the axes are flipped.
    /// 
    /// Otherwise same as IsOccupied()
    /// </summary>
    public bool IsOccupiedRelative(Direction direction, int directionCoord, int perpendicularCoord, byte allowedMarker)
    {
        return direction == Direction.Right
            ? IsOccupied(directionCoord, perpendicularCoord, allowedMarker)
            : IsOccupied(perpendicularCoord, directionCoord, allowedMarker);
    }
}
