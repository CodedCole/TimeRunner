using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _searchRadius = 12;
    [SerializeField] [Range(0.01f, 1.0f)] private float _searchRatio = 0.5f;
    [SerializeField] private Transform _eyes;
    [SerializeField] [Range(0, 360)] private float _searchFOV = 120;
    [SerializeField] [Min(0.01f)]private float _searchDelay = 0.1f;

    [SerializeField] private LayerMask _targetableLayers;
    [SerializeField] private LayerMask _blockLineOfSightLayers;

    [Header("Debug")]
    [SerializeField] [Min(1)] private int _searchResolution = 1;

    private TopDownMovement _movement;
    private GunController _gunController;

    private List<Transform> _targets = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        _movement = GetComponent<TopDownMovement>();
        _gunController = GetComponent<GunController>();

        StartCoroutine(SearchForTargets(_searchDelay));
    }

    // Update is called once per frame
    void Update()
    {
        _gunController.AimAtPos((Vector2)transform.position + new Vector2(Mathf.Cos(Time.time), Mathf.Sin(Time.time)));
        if(_targets.Count != 0)
        {
            _gunController.Fire();
        }
    }

    IEnumerator SearchForTargets(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);

            _targets.Clear();
            Collider2D[] possibleTargets = Physics2D.OverlapCircleAll(transform.position, _searchRadius, _targetableLayers);
            foreach (var pt in possibleTargets)
            {
                //not us
                if (pt.transform == transform)
                    continue;

                //within the search ratio
                Vector2 toTarget = pt.transform.position - transform.position;
                float scaledY = toTarget.y / _searchRatio;
                if (new Vector2(toTarget.x, scaledY).sqrMagnitude > _searchRadius * _searchRadius)
                    continue;

                //within the search fov
                Vector3 fovMax = _eyes.right * Mathf.Cos(_searchFOV * 0.5f * Mathf.Deg2Rad) + _eyes.up * Mathf.Sin(_searchFOV * 0.5f * Mathf.Deg2Rad);
                fovMax.y *= _searchRatio;
                fovMax.Normalize();
                Vector3 fovMin = _eyes.right * Mathf.Cos(_searchFOV * 0.5f * Mathf.Deg2Rad) - _eyes.up * Mathf.Sin(_searchFOV * 0.5f * Mathf.Deg2Rad);
                fovMin.y *= _searchRatio;
                fovMin.Normalize();
                if (Vector2.SignedAngle(fovMax.normalized, toTarget.normalized) > 0 || Vector2.SignedAngle(fovMin.normalized, toTarget.normalized) < 0)
                    continue;

                //unobstructed
                RaycastHit2D hit = Physics2D.Raycast(_eyes.position, toTarget.normalized, toTarget.magnitude, _blockLineOfSightLayers);
                if (hit)
                    continue;

                //add to targets
                _targets.Add(pt.transform);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3[] circumfrence = new Vector3[4 * _searchResolution];
        float anglePerSegment = (Mathf.PI * 2) / circumfrence.Length;
        for (int i = 0; i < circumfrence.Length; i++)
        {
            circumfrence[i] = transform.position + (new Vector3(Mathf.Cos(i * anglePerSegment), Mathf.Sin(i * anglePerSegment) * _searchRatio) * _searchRadius);
        }
        Gizmos.DrawLineStrip(circumfrence, true);

        Vector3 fovMax = _eyes.right * Mathf.Cos(_searchFOV * 0.5f * Mathf.Deg2Rad) + _eyes.up * Mathf.Sin(_searchFOV * 0.5f * Mathf.Deg2Rad);
        fovMax.Normalize();
        fovMax *= _searchRadius;
        fovMax.y *= _searchRatio;
        Vector3 fovMin = _eyes.right * Mathf.Cos(_searchFOV * 0.5f * Mathf.Deg2Rad) - _eyes.up * Mathf.Sin(_searchFOV * 0.5f * Mathf.Deg2Rad);
        fovMin.Normalize();
        fovMin *= _searchRadius;
        fovMin.y *= _searchRatio;
        Gizmos.DrawLine(_eyes.position, _eyes.position + fovMin);
        Gizmos.DrawLine(_eyes.position, _eyes.position + fovMax);

        Gizmos.color = Color.red;
        foreach (var t in _targets)
        {
            Gizmos.DrawLine(transform.position, t.position);
        }
    }
}
