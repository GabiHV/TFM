using UnityEngine;

public class PlannedPathVisualizer : MonoBehaviour
{
    public GameObject tfRootGO;
    public GameObject plannedPathGO;
    public GameObject plannedMarkerPrefab;

    private App.Utilities.Collections.Queue<GameObject> reusableMarkers;
    private static readonly int maxPath = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() =>
        InstantiateMarkerPool();
        
    private void InstantiateMarkerPool()
    {
        reusableMarkers = new(maxPath);
        for(int i = 0; i < maxPath; i++)
        {
            GameObject go = Instantiate(plannedMarkerPrefab, plannedPathGO.transform);
            go.SetActive(false);
            reusableMarkers.Enqueue(go);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
