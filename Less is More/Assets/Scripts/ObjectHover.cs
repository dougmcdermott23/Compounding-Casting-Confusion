using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHover : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float animationHeight;
    [SerializeField] private float animationSpeed;
    [SerializeField] private float animationRowDifference;

    private Vector3 startPosition;
    private float animTimeModifier;

    private void Awake()
    {
        startPosition = transform.position;
    }

    public void Init(int row)
    {
        this.animTimeModifier = row * animationRowDifference;
    }

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, animationCurve.Evaluate((Time.time * animationSpeed % animationCurve.length + animTimeModifier)) * animationHeight + startPosition.y, transform.position.z);
    }
}
