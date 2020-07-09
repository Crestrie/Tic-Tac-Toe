using UnityEngine;
using UnityEngine.UI;

public class UiConnect : MonoBehaviour
{
    [SerializeField] private GameObject Shadow;
    [SerializeField] private GameObject Text;
    [SerializeField] private GameObject Search;
    
    private static Animator ConnectAnimator;
    private static Animator SearchAnimator;

    private Button Button;

    private void Awake()
    {
        ConnectAnimator = GetComponent<Animator>();
        SearchAnimator = Search.GetComponent<Animator>();
        Button = GetComponent<Button>();
    }

    public void Connect()
    {
        ConnectAnimator.Play("UiConnect-Push");
        FirebaseController.ConnectToLobby();
        Button.interactable = false;
    }

    public void HideShadow()
    {
        Shadow.SetActive(false);
    }

    public void HideText()
    {
        Text.SetActive(false);
        
        SearchAnimator.Play("UiConnect-Searching");
    }

    public void HideButton()
    {
        gameObject.SetActive(false);
    }

    public static void ScreenOut()
    {
        Debug.Log("WTF");
        
        ConnectAnimator.CrossFade("UiConnect-ScreenOut", 0.0f);
    }
}
