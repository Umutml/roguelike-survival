using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class AdvancedRichTextManager : MonoBehaviour
{
    public AdvancedRichTextContainer[] advancedRichTextContainers;
    public TMP_Text text;
    protected CancellationTokenSource Cts;

    private void OnEnable()
    {
        Cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        Cts?.Cancel();
        Cts?.Dispose();
    }

    private void RefreshText()
    {
        text.text = "";
    }

    protected async Task ShowText(string messageKey,int typingSpeed=0)
    {
        RefreshText();
        var message = advancedRichTextContainers.FirstOrDefault(x => x.textKey == messageKey)?.advancedRichTexts.ToString();
        if (message == null)
        {
            text.text = messageKey;
            return;
        }
        if (typingSpeed == 0)
        {
            text.text = message;
            return;
        }
        var currentText = "";
        var insideTag = false;
        text.text = "";
        for (var i = 0; i < message.Length; i++)
        {
            var c = message[i];
            if (c == '<') insideTag = true;
            if (!insideTag) currentText += c;
            else currentText += c;
            if (c == '>') insideTag = false;
            text.text = currentText;
            if (!insideTag)
            {
                await Task.Delay(typingSpeed,Cts.Token);
            }
        }
    }
}
