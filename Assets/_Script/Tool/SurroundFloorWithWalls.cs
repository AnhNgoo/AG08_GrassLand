using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class SurroundFloorWithWalls : MonoBehaviour
{
    public Tilemap floorTilemap; // Tilemap chứa "floor"
    public Tilemap wallTilemap;  // Tilemap chứa "wall"
    public TileBase wallTile;    // Sử dụng TileBase để tương thích cả Tile và AnimatedTile
    public int borderSize = 2;   // Độ dày của vùng bao quanh (tính bằng số ô)

    // Danh sách lưu trữ các vị trí wall do script tạo
    private List<Vector3Int> generatedWallPositions = new List<Vector3Int>();

    [ContextMenu("Surround Floor With Walls")]
    public void GenerateWallBorder()
    {
        // Xóa danh sách cũ để tránh trùng lặp nếu chạy lại
        generatedWallPositions.Clear();

        // Lấy bounds của floorTilemap
        BoundsInt floorBounds = floorTilemap.cellBounds;

        // Tạo bounds lớn hơn bao quanh floorTilemap
        int minX = floorBounds.xMin - borderSize;
        int maxX = floorBounds.xMax + borderSize;
        int minY = floorBounds.yMin - borderSize;
        int maxY = floorBounds.yMax + borderSize;

        // Duyệt qua toàn bộ khu vực hình chữ nhật lớn hơn
        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);

                // Nếu vị trí này không có tile trong floorTilemap
                // và cũng không có tile trong wallTilemap
                if (floorTilemap.GetTile(position) == null && wallTilemap.GetTile(position) == null)
                {
                    // Đặt tile "wall" vào wallTilemap
                    wallTilemap.SetTile(position, wallTile);
                    // Làm mới tile để kích hoạt animation
                    wallTilemap.RefreshTile(position);
                    // Lưu vị trí này vào danh sách
                    generatedWallPositions.Add(position);
                }
            }
        }
    }

    [ContextMenu("Clear Wall Border")]
    public void ClearWallBorder()
    {
        // Chỉ xóa các wall trong danh sách generatedWallPositions
        foreach (Vector3Int position in generatedWallPositions)
        {
            // Xóa tile "wall" tại vị trí này
            wallTilemap.SetTile(position, null);
        }

        // Xóa danh sách sau khi xóa wall để tránh tích lũy
        generatedWallPositions.Clear();
    }

    void OnDrawGizmos()
    {
        if (GameObject.FindWithTag("Walls"))
        {
            wallTilemap = GameObject.FindWithTag("Walls").GetComponent<Tilemap>();
        }
        if (GameObject.FindWithTag("Floor"))
        {
            floorTilemap = GameObject.FindWithTag("Floor").GetComponent<Tilemap>();
        }
    }
}
