using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextInputHandler : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject clientObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSubmit()
    {
        string textInput = inputField.text;
        ClientObject2 clientObject2 = clientObject.GetComponent<ClientObject2>();
        clientObject2.ConnectToServer(textInput);
    }
}
