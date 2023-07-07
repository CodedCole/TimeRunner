using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContextMovement : MonoBehaviour
{
    [SerializeField][Min(0.01f)] private float _updateDelay = 0.1f;
    [SerializeField][Min(4)] private int _directionCount = 8;
    [SerializeField][Min(0)] private float _favorCurrentDirection = 0.5f;

    [Header("Debug")]
    [SerializeField] private float _directionInfluenceScale = 0.1f;
    [SerializeField] private float _directionOffset = 0.2f;

    private TopDownMovement _movement;

    [HideInInspector]
    public Vector3[] _interestPoints;
    private Vector3[] _directions;
    private float[] _directionWeights;
    private float _angleBetweenDirection;

    private int previousBestDirection;

    // Start is called before the first frame update
    void Start()
    {
        _movement = GetComponent<TopDownMovement>();

        _directions = new Vector3[_directionCount];
        _directionWeights = new float[_directionCount];
        _angleBetweenDirection = (Mathf.PI * 2) / _directionCount;
        for (int i = 0; i < _directionCount; i++)
        {
            _directions[i] = new Vector3(Mathf.Cos(i * _angleBetweenDirection), Mathf.Sin(i * _angleBetweenDirection)).normalized;
        }

        StartCoroutine(UpdateMovement(_updateDelay));
    }

    IEnumerator UpdateMovement(float delay)
    {
        while(true)
        {
            yield return new WaitForSeconds(delay);

            //soft reset preferring the direction already travelling
            for (int i = 0; i < _directionCount; i++)
            {
                _directionWeights[i] = (Vector3.Dot(_directions[i], _directions[previousBestDirection]) + 1) * 0.5f * _favorCurrentDirection;
            }

            //find influence
            foreach (var point in _interestPoints)
            {
                Vector3 toPoint = point - transform.position;
                toPoint.z = 0;
                for (int i = 0; i < _directionCount; i++)
                {
                    float dot = (Vector3.Dot(toPoint.normalized, _directions[i]) + 1) * 0.5f;
                    _directionWeights[i] = _directionWeights[i] + dot * (point.z / Mathf.Max(0.01f, toPoint.magnitude));
                }
            }

            //find best direction
            int bestDirection = 0;
            for (int i = 1; i < _directionCount; i++)
            {
                if (_directionWeights[i] > _directionWeights[bestDirection])
                    bestDirection = i;
            }

            _movement.UpdateMove(_directions[bestDirection]);
            previousBestDirection = bestDirection;
        }
    }

    public void SetInterestPoints(Vector3[] points)
    {
        _interestPoints = points;
    }

    private void OnDrawGizmosSelected()
    {
        if (_directions != null)
        {
            for (int i = 0; i < _directions.Length; i++)
            {
                Gizmos.color = _directionWeights[i] < 0 ? Color.red : Color.green;
                Gizmos.DrawLine(transform.position + (_directions[i] * _directionOffset), transform.position + (_directions[i] * (_directionOffset + (Mathf.Abs(_directionWeights[i]) * _directionInfluenceScale))));
            }
        }
    }
}
