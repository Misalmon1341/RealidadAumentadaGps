using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
public class GPSManager : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    private Vector3 startPosition;
    private Vector3 moveDirection;
    private Vector2 startGPSPosition;
    private Vector2 deltaGPSPosition;

    public Slider sliderScale;
    public Transform playerTransform;
    private IEnumerator Start()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitForSeconds(1f);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Debug.LogError("El usuario no concedió permiso de ubicación.");
            yield break;
        }
#endif
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("La ubicación/GPS está desactivada en el dispositivo.");
            yield break;
        }
        // desiredAccuracyInMeters, updateDistanceInMeters
        Input.location.Start(1f, 1f);
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing &&
               maxWait > 0)
        {
            yield return new WaitForSeconds(1f);
            maxWait--;
        }
        if (maxWait <= 0)
        {
            Debug.LogError("Timeout inicializando el GPS.");
            yield break;
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("No se pudo determinar la ubicación.");
            yield break;
        }
        Debug.Log("GPS iniciado correctamente.");
        LocationInfo location = Input.location.lastData;
        
        startPosition = playerTransform.position;
        startGPSPosition = new Vector2(location.latitude, location.longitude);
        deltaGPSPosition = Vector2.zero;
        moveDirection = Vector3.zero;
    }
    private void Update()
    {
        if (Input.location.status != LocationServiceStatus.Running)
            return;
        LocationInfo location = Input.location.lastData;

        textDisplay.text = location.latitude + " " + location.longitude;
        
        deltaGPSPosition.x = location.latitude - startGPSPosition.x;
        deltaGPSPosition.y = location.longitude - startGPSPosition.y;
        
        moveDirection.x = deltaGPSPosition.x;
        moveDirection.z = deltaGPSPosition.y;
        
        playerTransform.position = startPosition + (moveDirection * sliderScale.value);
    }
    private void OnDestroy()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            Input.location.Stop();
        }
    }
}

