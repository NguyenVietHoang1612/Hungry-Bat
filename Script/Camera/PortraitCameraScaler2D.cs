using CandyProject;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PortraitCameraScaler2D : MonoBehaviour
{
    private BoardManager board;
    private Camera cam;
    [SerializeField] private Transform boardTile;

    public float baseAspect = 9f / 16f;
    [SerializeField] private float cameraOffset = -10f;
    [SerializeField] private float padding = 2f;

    IEnumerator Start()
    {
        yield return null; 
        board = FindFirstObjectByType<BoardManager>();
        cam = GetComponent<Camera>();

        RepositionCamera(board.Width - 1, board.Height - 1);
        MoveBoardTile(board.Width - 1, board.Height - 1);
    }

    void RepositionCamera(float x, float y)
    {
        Vector3 tempPosition = new Vector3(x / 2, y - 0.5f, cameraOffset);
        cam.transform.position = tempPosition;

        if (board.Width >= board.Height)
        {
            cam.orthographicSize = ((x / 2 + padding) / baseAspect) + 1.5f;
        }
        else 
        {
            cam.orthographicSize = (y / 2 + padding) + 1.5f;  
        }
    }

    void MoveBoardTile(float x, float y)
    {
        Vector2 tempPosition = new Vector2(x / 2, y / 2);
        boardTile.transform.position = tempPosition;
    }
}
