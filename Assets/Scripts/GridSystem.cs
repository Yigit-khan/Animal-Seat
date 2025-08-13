using System.Collections.Generic;
using UnityEngine;

public class GridSystem
{
    // Dictionary, bir 2D diziden daha esnektir.
    // Otobüs koridoru gibi boþluklu grid'lere izin verir.
    private Dictionary<Vector2Int, SeatController> grid = new Dictionary<Vector2Int, SeatController>();

    private Vector3 gridOrigin;
    private Vector2 cellSize;

    // Kurucu Metot (Constructor)
    public GridSystem(Transform seatParent, Transform originTransform, Vector2 sizeOfCell)
    {
        gridOrigin = originTransform.position;
        cellSize = sizeOfCell;
        InitializeGrid(seatParent);
    }

    private void InitializeGrid(Transform seatParent)
    {
        grid.Clear();
        foreach (Transform seatTransform in seatParent)
        {
            if (seatTransform.TryGetComponent<SeatController>(out SeatController seat))
            {
                Vector2Int gridPos = WorldToGridPosition(seat.transform.position);
                seat.SetGridPosition(gridPos);

                // Ayný pozisyona baþka bir koltuk eklenmediðinden emin ol
                if (!grid.ContainsKey(gridPos))
                {
                    grid.Add(gridPos, seat);
                }
                else
                {
                    Debug.LogWarning($"Grid Pozisyonu Çakýþmasý: {gridPos} konumunda zaten bir koltuk var!", seat.gameObject);
                }
            }
        }
        Debug.Log($"Grid Sistemi Baþlatýldý. Toplam {grid.Count} koltuk eklendi.");
    }

    // Bir dünya pozisyonunu (Vector3) grid koordinatýna (Vector2Int) çevirir.
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 relativePos = worldPosition - gridOrigin;
        // Otobüsümüz X ve Z eksenlerinde kurulu olduðu için Y'yi kullanmýyoruz.
        int x = Mathf.RoundToInt(relativePos.x / cellSize.x);
        int y = Mathf.RoundToInt(relativePos.z / cellSize.y); // Dünya Z'si bizim grid Y'mizdir.
        return new Vector2Int(x, y);
    }

    // Verilen bir grid pozisyonundaki koltuðu döndürür.
    public SeatController GetSeatAt(Vector2Int gridPosition)
    {
        grid.TryGetValue(gridPosition, out SeatController seat);
        return seat;
    }

    // Bir koltuðun etrafýndaki komþularýný belirli bir menzilde bulur.
    // OYUNUNUZUN EN ÖNEMLÝ FONKSÝYONU BUDUR!
    public List<SeatController> GetNeighbors(SeatController centerSeat, int range)
    {
        List<SeatController> neighbors = new List<SeatController>();
        Vector2Int centerPos = centerSeat.GridPosition;

        // Merkez etrafýndaki bir kare alaný tara
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                // Merkezin kendisini atla
                if (x == 0 && y == 0)
                {
                    continue;
                }

                // Sadece Manhattan mesafesi menzile uyanlarý al
                // Bu, kare yerine baklava dilimi þeklinde komþu bulur (daha doðru)
                if (Mathf.Abs(x) + Mathf.Abs(y) > range)
                {
                    continue;
                }

                Vector2Int neighborPos = centerPos + new Vector2Int(x, y);

                // Bu pozisyonda bir koltuk var mý diye kontrol et
                if (grid.TryGetValue(neighborPos, out SeatController neighborSeat))
                {
                    neighbors.Add(neighborSeat);
                }
            }
        }
        return neighbors;
    }

    public Dictionary<Vector2Int, SeatController> GetFullGrid()
    {
        return grid;
    }

    public List<SeatController> GetAllEmptySeats()
    {
        List<SeatController> emptySeats = new List<SeatController>();
        foreach (var seatController in grid)
        {
            if (!seatController.Value.isOccupied)
            {
                emptySeats.Add(seatController.Value);
            }
        }
        return emptySeats;
    }
}