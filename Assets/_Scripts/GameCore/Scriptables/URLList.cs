using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


[CreateAssetMenu(fileName = "URLList", menuName = "ScriptableObjects/URLList", order = 0)]
public class URLList : ScriptableObject
{
    [SerializeField] private List<URL> urls = new ();
    
    public string GetURL(URLType type) => urls.FirstOrDefault(url => url.Type.Equals(type)).Url;
}



[Serializable]
public struct URL
{
    [SerializeField] private URLType type;
    [SerializeField] private string url;
    
    
    public URLType Type => type;
    public string Url => url;
}


public enum URLType
{
    Discord,
    PrivacyPolicy,
    Attribution,
}