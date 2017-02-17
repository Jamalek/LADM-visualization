using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parcel3DOnClick : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        { // if left button pressed...
            //Debug.Log("Click");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log("Mouse Down Hit the following object: " + hit.collider.name);
                /*Debug.Log(Camera.FindSceneObjectsOfType<System.Type.
                    Component.FindObjectsOfTypeAll(UnityEngine.UI.
                    GameObject.Find("Cube"));//SetActive(true);
                *///GameObject.Find("Cube").SetActive(false);
                foreach (GameObject bf in GameObject.FindObjectsOfType<GameObject>())
                {
                    if (bf.name == hit.collider.name)
                    {
                        //Debug.Log(bf.GetComponent<MeshRenderer>().material.name);
                        if (bf.GetComponent<MeshRenderer>().material.name == "Parcel3DMaterial (Instance)")
                        {
                            bf.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Parcel3DSelectedMaterial");
                        } else bf.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Parcel3DMaterial");
                    }
                }
            }
        }
    }
}
