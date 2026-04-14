using UnityEngine;
using TMPro;
using System.Collections;

public class ModalMessage : MonoBehaviour
{

    public TMP_Text messageText;
    private Result result;

    private static ModalMessage _instance;
    
    public enum Result {None, Yes, No};

    public static void ShowDialog(string message)
    {
        if(_instance == null) return;

        _instance.SetMessage(message);
        _instance.ShowButtons(false);
        _instance.ShowWindow();
    }

    public static IEnumerator ShowDialogWithConfirmation(string message)
    {
        if(_instance ==  null) yield return Result.None;
        _instance.result = Result.None; // Refreshing instance response to none.

        _instance.SetMessage(message);
        _instance.ShowButtons(true);
        _instance.ShowWindow();

        yield return new WaitWhile(() => _instance.result == Result.None);

        _instance.DismissWindow();
    }

    public static Result GetResult() =>
        _instance == null ? Result.None : _instance.result;

    void Awake()
    {
        _instance = this;
        gameObject.SetActive(false);
    }

    private void SetMessage(string message) =>
        this.messageText.text = message;

    private void ShowWindow() =>
        this.gameObject.SetActive(true);
    
    public void DismissWindow() =>
        this.gameObject.SetActive(false);
    
    private void ShowButtons(bool flag)
    {
        GameObject buttonsPanel = this.transform.Find("ModalPanel/ButtonsPanel").gameObject;
        buttonsPanel.SetActive(flag);
    }
    
    public void Confirm() =>
        result = Result.Yes;
    
    public void Cancel() =>
        result = Result.No;
}
