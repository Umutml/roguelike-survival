using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveIndicatorText : AdvancedRichTextManager
{
    [SerializeField] private Image objectiveArea;
    [SerializeField] private bool autoClose;
    private List<Tuple<string,Color,float>> _textQueue = new();


    public async void ShowText(string status,Color backgroundColor, float showTime,bool isOld=false)
    {
        if(!isOld)
            _textQueue.Add(new Tuple<string,Color, float>(status,backgroundColor, showTime));
        if (objectiveArea.gameObject.activeSelf) return;
        objectiveArea.color = backgroundColor;
        objectiveArea.gameObject.SetActive(true);
        await ShowText(status);
        await Task.Delay(TimeSpan.FromSeconds(showTime),Cts.Token);
        _textQueue.RemoveAt(0);
        if (_textQueue.Count > 0)
        {
            CloseText();
            ShowText(_textQueue[0].Item1, _textQueue[0].Item2,_textQueue[0].Item3,true);
        }
        else
        {
            if (!autoClose) return;
            CloseText();
        }
    }

    public void CloseText()
    {
        objectiveArea.gameObject.SetActive(false);
    }
}
