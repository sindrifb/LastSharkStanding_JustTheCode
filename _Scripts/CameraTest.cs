using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
public class CameraTest : MonoBehaviour {
    public Text Text;
    List<Transform> GameObjects;
    int index = 0;
	// Use this for initialization
	void Start () {
        GameObjects = transform.GetComponentsInChildren<Transform>().ToList();
        //print(GameObjects.Count);
        GameObjects.RemoveAll(a => a.gameObject == gameObject);
        GameObjects.ForEach(a => a.gameObject.SetActive(false));
        GameObjects[0]?.gameObject.SetActive(true);
        Text.text = "CAMERA: " + (index + 1) + ", " + GameObjects[index].name;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            swap(false);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            swap(true);
        }
	}

    void swap(bool up)
    {
        if (up)
        {
            index++;
        }
        else
        {
            index--;
        }
        
        if (index >= GameObjects.Count)
        {
            index = 0;
        }
        else if (index < 0)
        {
            index = GameObjects.Count - 1;
        }

        //print(index);

        if (GameObjects != null && GameObjects.Count != 0)
        {
            GameObjects.ForEach(a => a.gameObject.SetActive(false));
            GameObjects[index].gameObject.SetActive(true);
            Text.text = "CAMERA: " + (index + 1) + ", " + GameObjects[index].name;
        }
    }
}
