using UnityEngine;

public class KnappingIndicator : MonoBehaviour
{
    public GameObject cursorPrefab;
    public GameObject anglePrefab;
    public GameObject powerBarPrefab;

    private GameObject cursorInstance;
    private GameObject angleInstance;
    private GameObject powerBarInstance;
    private Material powerBarMaterial;

    void Start()
    {
        cursorInstance = Instantiate(cursorPrefab, transform);
        cursorInstance.SetActive(false);

        angleInstance = Instantiate(anglePrefab, transform);
        angleInstance.SetActive(false);

        powerBarInstance = Instantiate(powerBarPrefab, transform);
        powerBarInstance.SetActive(false);
        powerBarMaterial = powerBarInstance.GetComponent<MeshRenderer>().material;
    }

    public void ShowCursor(Vector3 worldPos, Vector3 normal)
    {
        cursorInstance.SetActive(true);
        cursorInstance.transform.position = worldPos + normal * 0.01f;
        cursorInstance.transform.rotation = Quaternion.LookRotation(-normal);
    }

    public void HideCursor()
    {
        cursorInstance.SetActive(false);
    }

    public void ShowAngle(Vector3 worldPos, Vector3 normal, float angleDegrees)
    {
        angleInstance.SetActive(true);
        angleInstance.transform.position = worldPos + normal * 0.05f;
        angleInstance.transform.rotation = Quaternion.LookRotation(-normal) * Quaternion.Euler(0, 0, angleDegrees);
    }

    public void HideAngle()
    {
        angleInstance.SetActive(false);
    }

    public void ShowPower(Vector3 worldPos, Vector3 normal, float power01)
    {
        powerBarInstance.SetActive(true);
        powerBarInstance.transform.position = worldPos + normal * 0.08f;
        powerBarInstance.transform.rotation = Quaternion.LookRotation(-normal);
        powerBarInstance.transform.localScale = new Vector3(0.3f * power01, 0.03f, 0.01f);

        Color color = Color.Lerp(Color.green, Color.red, power01);
        if (powerBarMaterial != null)
            powerBarMaterial.color = color;
    }

    public void HidePower()
    {
        powerBarInstance.SetActive(false);
    }
}