using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G_PasswordField : MonoBehaviour {
    public string passwordToEdit = "Password1";
    void OnGUI()
    {
        passwordToEdit = GUI.PasswordField(transform.GetComponent<RectTransform>().rect, passwordToEdit, "*"[0], 25);
    }
    void Start () {}
	void Update () {}
}
