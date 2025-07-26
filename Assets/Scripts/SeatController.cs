using UnityEngine;

public class SeatController : MonoBehaviour
{
    // Artýk public gridX/gridY yerine Vector2Int kullanacaðýz.
    // Bu, grid koordinatlarýný tek bir deðiþkende tutar.
    public Vector2Int GridPosition { get; private set; }

    public bool isOccupied { get; private set; } = false;
    public AnimalController occupiedBy { get; private set; } = null;
    public bool isWet { get; set; } = false;

    // GridSystem bu metodu kullanarak koltuðun pozisyonunu ona bildirecek.
    public void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
    }

    public void Occupy(AnimalController animal)
    {
        isOccupied = true;
        occupiedBy = animal;
    }

    public void Vacate()
    {
        isOccupied = false;
        occupiedBy = null;
    }
}