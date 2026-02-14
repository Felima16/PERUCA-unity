using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides the ability to reset specified objects if they fall below a certain position - designated by this transform's height.
/// </summary>
public class HairReset : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Which objects to reset if falling out of range.")]
    List<GameObject> m_ObjectsToReset = new List<GameObject>();

    [SerializeField]
    [Tooltip("The object root used to compute local positions relative to. Objects will respawn relative to their position in this transform's hierarchy.")]
    Transform m_ObjectRoot = null;

    readonly List<Pose> m_OriginalPositions = new List<Pose>();

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void Start()
    {
        foreach (var currentTransform in m_ObjectsToReset)
        {
            if (currentTransform != null)
            {
                var position = currentTransform.transform.position;

                if (m_ObjectRoot != null)
                    position = m_ObjectRoot.InverseTransformPoint(currentTransform.transform.position);

                m_OriginalPositions.Add(new Pose(position, currentTransform.transform.rotation));
            }
            else
            {
                m_OriginalPositions.Add(new Pose());
            }
        }
    }

    public void Reset() {
        for (var transformIndex = 0; transformIndex < m_ObjectsToReset.Count; transformIndex++)
        {
            var currentGameObject = m_ObjectsToReset[transformIndex];
            currentGameObject.SetActive(true);
            var currentTransform = m_ObjectsToReset[transformIndex].transform;

            if (currentTransform == null)
                continue;
            
            var originalWorldPosition = m_OriginalPositions[transformIndex].position;
            if (m_ObjectRoot != null)
                originalWorldPosition = m_ObjectRoot.TransformPoint(originalWorldPosition);

            currentTransform.SetPositionAndRotation(originalWorldPosition, m_OriginalPositions[transformIndex].rotation);
        }
    }
}

