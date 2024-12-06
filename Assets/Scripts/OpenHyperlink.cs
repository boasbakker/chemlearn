using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class OpenHyperlink : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text _mTextMeshPro;
    [SerializeField] UitlegScript uitlegScript;
    void Start()
    {
        _mTextMeshPro = GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_mTextMeshPro, Input.mousePosition, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = _mTextMeshPro.textInfo.linkInfo[linkIndex];
            var regex = new Regex(Regex.Escape("0000EE"));
            _mTextMeshPro.text = regex.Replace(_mTextMeshPro.text, "551A8B", 1);
            if (linkInfo.GetLinkID() == "sluit")
            {
                uitlegScript.Sluit();
            }
            else
                Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}