using System;
using UnityEngine;

public class Swiper : MonoBehaviour
{
    [Header("Config")] 
    [SerializeField] private int maxPage;
    [SerializeField] private Vector3 pageStep;
    [SerializeField] private RectTransform imagesPagesRect;
    
    private int currentPage;
    private Vector3 targetPos;
    private bool swiperForLanguage = false;

    private void Awake()
    {
        currentPage = 1;
        targetPos = imagesPagesRect.localPosition;
    }

    public void Next()
    {
        if (currentPage < maxPage)
        {
            currentPage++;
            targetPos += pageStep;
            MovePage();
        }
        else
        {
            Previous();
        }
    }

    public void Previous()
    {
        if (currentPage > 1)
        {
            currentPage--;
            targetPos -= pageStep;
            MovePage();
        }
        else
        {
            Next();
        }
    }
    
    private void MovePage()
    {
        imagesPagesRect.localPosition = targetPos;
    }
}
