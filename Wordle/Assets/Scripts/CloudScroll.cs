using UnityEngine;
using UnityEngine.UI;

public class CloudScroll : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float scrollSpeed = 100f;
    
    [Header("Configuración de Parallax")]
    [SerializeField] private GameObject cloudImage;
    [SerializeField] private int numberOfCopies = 3;
    
    private float imageWidth; // Ancho visual real (incluyendo escala)
    private RectTransform[] cloudCopies;
    private float screenLeftEdge;
    
    void Start()
    {
        RectTransform rectTransform = cloudImage.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("El GameObject no tiene un RectTransform");
            return;
        }
        
        // Capturar el ancho REAL considerando la escala
        float baseWidth = rectTransform.rect.width;
        float scaleX = rectTransform.localScale.x;
        imageWidth = baseWidth * scaleX;
        
        Debug.Log($"Ancho base: {baseWidth}, Escala X: {scaleX}, Ancho real: {imageWidth}");
        
        // Calcular el borde izquierdo de la pantalla
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            screenLeftEdge = -canvasRect.rect.width / 2f;
        }
        else
        {
            screenLeftEdge = -Screen.width / 2f;
        }
        
        CreateCloudCopies();
    }
    
    void CreateCloudCopies()
    {
        cloudCopies = new RectTransform[numberOfCopies];
        RectTransform originalRect = cloudImage.GetComponent<RectTransform>();
        cloudCopies[0] = originalRect;
        
        // Guardar configuración original
        Vector2 originalAnchorMin = originalRect.anchorMin;
        Vector2 originalAnchorMax = originalRect.anchorMax;
        Vector2 originalPivot = originalRect.pivot;
        Vector2 originalAnchoredPos = originalRect.anchoredPosition;
        Vector2 originalSizeDelta = originalRect.sizeDelta;
        Vector3 originalScale = originalRect.localScale;
        
        Debug.Log($"Configuración original - AnchoredPos: {originalAnchoredPos}, Pivot: {originalPivot}, Scale: {originalScale}");
        
        for (int i = 1; i < numberOfCopies; i++)
        {
            GameObject copy = Instantiate(cloudImage, transform);
            copy.name = $"Cloud_{i}";
            
            RectTransform copyRect = copy.GetComponent<RectTransform>();
            
            // Copiar TODA la configuración original
            copyRect.anchorMin = originalAnchorMin;
            copyRect.anchorMax = originalAnchorMax;
            copyRect.pivot = originalPivot;
            copyRect.sizeDelta = originalSizeDelta;
            copyRect.localScale = originalScale; // IMPORTANTE: mantener la escala
            
            // Posicionar usando anchoredPosition
            // La distancia debe ser imageWidth (que ya incluye la escala)
            Vector2 newPos = originalAnchoredPos;
            newPos.x = originalAnchoredPos.x + (imageWidth * i);
            copyRect.anchoredPosition = newPos;
            
            cloudCopies[i] = copyRect;
            
            Debug.Log($"Nube {i} - AnchoredPos: {newPos}");
        }
    }
    
    void Update()
    {
        foreach (RectTransform cloud in cloudCopies)
        {
            // Mover hacia la izquierda
            Vector2 pos = cloud.anchoredPosition;
            pos.x -= scrollSpeed * Time.deltaTime;
            cloud.anchoredPosition = pos;
            
            // Calcular borde derecho considerando el pivot
            // Con pivot 0.5 (centro): borde derecho = posición + (imageWidth / 2)
            float pivotOffsetX = (cloud.pivot.x - 0.5f) * imageWidth;
            float cloudRightEdge = cloud.anchoredPosition.x + (imageWidth / 2f) - pivotOffsetX;
            
            // Si el borde derecho sale completamente de la pantalla
            if (cloudRightEdge < screenLeftEdge)
            {
                // Encontrar la posición X más a la derecha
                float maxX = cloudCopies[0].anchoredPosition.x;
                foreach (RectTransform c in cloudCopies)
                {
                    if (c.anchoredPosition.x > maxX)
                    {
                        maxX = c.anchoredPosition.x;
                    }
                }
                
                // Reposicionar exactamente imageWidth unidades a la derecha
                Vector2 newPos = cloud.anchoredPosition;
                newPos.x = maxX + imageWidth;
                cloud.anchoredPosition = newPos;
                
                Debug.Log($"Nube {cloud.name} reposicionada en x: {newPos.x}");
            }
        }
    }
}