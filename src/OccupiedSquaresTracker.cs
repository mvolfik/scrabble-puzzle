namespace ScrabblePuzzleGenerator;

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

    public OccupiedSquaresTracker Clone()
    {
        return new OccupiedSquaresTracker((byte[,])grid.Clone());
    }

    /// <summary>
    /// Marks given square as blocked by given word (either directly occupied, or neighboring with it).
    /// 
    /// If the square is already blocked by another word, it is marked with the flag SquareNeighboringMultipleWords.
    /// </summary>
    void MarkSquare(int x, int y, byte marker)
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
    /// Checks if given tile is free or only blocked by given marker
    /// </summary>
    public bool IsOccupied(int x, int y, byte allowedMarker)
    {
        return grid[x, y] != FreeSquare && grid[x, y] != allowedMarker;
    }

    public bool IsOccupiedRelative(Direction direction, int directionCoord, int perpendicularCoord, byte allowedMarker)
    {
        return direction == Direction.Right
            ? IsOccupied(directionCoord, perpendicularCoord, allowedMarker)
            : IsOccupied(perpendicularCoord, directionCoord, allowedMarker);
    }
}
