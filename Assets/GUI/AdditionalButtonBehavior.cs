using UnityEngine;
using UnityEngine.UI;

public class AdditionalButtonBehavior : MonoBehaviour {

    private string text;

	// Use this for initialization
	void Start () {
        text = gameObject.GetComponentInChildren<Text>().text;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        RectTransform button = gameObject.GetComponent<RectTransform>();
        Vector3[] c = new Vector3[4];
        button.GetWorldCorners(c);
        Rect rect = new Rect(c[0].x, c[0].y, c[2].x - c[1].x, c[1].y - c[0].y);
        if(rect.Contains(Input.mousePosition))
        {
            gameObject.GetComponentInChildren<Text>().text = "";
        }
        else
        {
            gameObject.GetComponentInChildren<Text>().text = text;
        }
    }
}
