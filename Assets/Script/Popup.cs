using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField]
    private string popupName = "BallPopup";
    private bool spawnPopup = false;
    private UIPopup popup;

    // Start is called before the first frame update
    void Start()
    {
        popup = UIPopup.GetPopup(popupName);
    }

    // Update is called once per frame
    void Update()
    {
        popup.transform.position = Camera.main.WorldToScreenPoint(gameObject.transform.position);

    }

    public void ShowPopup()
    {
        if (popup == null)
        {
            return;
        }
        
        if (!popup.IsShowing)
        {
            spawnPopup = true;
            popup.transform.position = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            popup.Show();
        }
    }

    public void HidePopup()
    {
        if (popup == null)
        {
            return;
        }

        spawnPopup = false;
        popup.Hide();
    }

    private void OnTriggerEnter(Collider collision)
    {
        ShowPopup();
        Debug.Log("Hit");
    }



    private void OnTriggerExit(Collider collision)
    {
        HidePopup();
        Debug.Log("Hide");
    }

}
