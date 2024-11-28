using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] RectTransform progressImage;
    public void SetProgress(double progress)
    {
        progressImage.localScale = new Vector3((float)progress, 1f, 1f);
    }
}
