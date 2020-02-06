using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextSetter : MonoBehaviour
{
    private TextMeshProUGUI proText;
    public IntVariable Variable;
    public string prefix;
    public string postfix;

    void Start()
    {
        proText = GetComponent<TextMeshProUGUI>();
    }
    
    private void Update()
    {
        if (proText != null && Variable != null)
        {
            proText.text = prefix +  Variable.Value.ToString() + postfix;
        }
        else
        {
            Debug.LogError("Missing text from UI");
        }
    }
}
