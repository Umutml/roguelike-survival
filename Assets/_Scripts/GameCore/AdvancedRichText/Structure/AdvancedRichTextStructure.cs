using System;
using System.Linq;
using UnityEngine;
[Serializable]
public class AdvancedRichTextContainer
{
    public string textKey;
    public AdvancedRichText advancedRichTexts;
    public override string ToString()
    {
        return advancedRichTexts.ToString();
    }
}
[Serializable]
public class AdvancedRichText
{
    public RichTextObject[] richTextObjects;
    public override string ToString()
    {
        return richTextObjects.Aggregate("", (current, richTextObject) => current + richTextObject.GetText());
    }
}
[Serializable]
public class RichTextObject
{
    public string text;
    public bool newLine;
    public Color textColor = new(1,1,1,1);
    public TextType textType;
    public LineType lineType;
    public string GetText()
    {
        var newStringText = "";
        if (newLine)
            newStringText += "\n";
        newStringText += $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>";
        // newStringText += $"<size={textSize}>";
        switch (lineType)
        {
            case LineType.Normal:
                break;
            case LineType.Underline:
                newStringText += "<u>";
                break;
            case LineType.Strikethrough:
                newStringText += "<s>";
                break;
        }
        switch (textType)
        {
            case TextType.Normal:
                newStringText += text;
                break;
            case TextType.Bold:
                newStringText += $"<b>{text}</b>";
                break;
            case TextType.Italic:
                newStringText += $"<i>{text}</i>";
                break;
            case TextType.BoldItalic:
                newStringText += $"<b><i>{text}</i></b>";
                break;
            case TextType.Emote:
                newStringText += $"<sprite name=\"{text}\"/>";
                break;
        }
        switch (lineType)
        {
            case LineType.Underline:
                newStringText += "</u>";
                break;
            case LineType.Strikethrough:
                newStringText += "</s>";
                break;
        }
        // newStringText += "</size>";
        newStringText += "</color>";
        return newStringText;
    }
}
public enum TextType
{
    Normal,
    Bold,
    Italic,
    BoldItalic,
    Emote,
}
public enum LineType
{
    Normal,
    Underline,
    Strikethrough,
}