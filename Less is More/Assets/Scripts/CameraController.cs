using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private struct FocusArea
    {
        public Vector2 center;
        public Vector2 velocity;
        private float left, right, top, bottom;

        public FocusArea(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x / 2;
            right = targetBounds.center.x + size.x / 2;
            top = targetBounds.center.y + size.y / 2;
            bottom = targetBounds.center.y - size.y / 2;

            velocity = Vector2.zero;
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
        }

        public void UpdateFocusArea(Bounds targetBounds)
        {
            float shiftX = 0;
            if (targetBounds.min.x < left)
            {
                shiftX = targetBounds.min.x - left;
            }
            else if(targetBounds.max.x > right)
            {
                shiftX = targetBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;

            float shiftY = 0;
            if (targetBounds.min.y < bottom)
            {
                shiftY = targetBounds.min.y - bottom;
            }
            else if (targetBounds.max.y > top)
            {
                shiftY = targetBounds.max.y - top;
            }
            bottom += shiftY;
            top += shiftY;

            velocity = new Vector2(shiftX, shiftY);
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
        }
    }

    [SerializeField] private Vector2 focusAreaSize;
    [SerializeField] private float smoothTime;

    private GameController gameController;
    private PlayerController playerController;
    private Collider2D target;
    private FocusArea focusArea;
    private float smoothVelocityX;
    private float smoothVelocityY;
    private bool isInitialized;

    private void Start()
    {

    }

    public void Init(GameController gameController, PlayerController playerController)
    {
        transform.position = playerController.transform.position;

        this.gameController = gameController;
        this.playerController = playerController;
        target = playerController.GetComponent<Collider2D>();
        focusArea = new FocusArea(target.bounds, focusAreaSize);

        isInitialized = true;
    }

    private void LateUpdate()
    {
        if (!isInitialized)
            return;

        focusArea.UpdateFocusArea(target.bounds);

        Vector2 focusPosition = focusArea.center;

        focusPosition.x = Mathf.SmoothDamp(transform.position.x, focusPosition.x, ref smoothVelocityX, smoothTime);
        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, smoothTime);
        transform.position = (Vector3)focusPosition + Vector3.forward * -10;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(focusArea.center, focusAreaSize);
    }
}
