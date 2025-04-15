using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypeTextComponent : MonoBehaviour
{

    private float _defaultSpeed = 0.04f;

    private TextMeshProUGUI _label;
    private string _currentText = string.Empty;
    private string _finalText = string.Empty;

    private Coroutine _typeTextCoroutine;

    private static readonly string[] _uguiSymbols = { "b", "i" };
    private static readonly string[] _uguiCloseSymbols = { "b", "i", "size", "color" };
    private Action _onCompleteCallback;


    void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (_label == null)
        {
            _label = GetComponent<TextMeshProUGUI>();
        }
    }

    public void ClearText()
    {
        _label.text = string.Empty;
    }

    public void SetTypeText(string text, float speed = 0.04f, Action onComplete = null)
    {
        SetText(text, speed);
        SetOnComplete(onComplete);
    }

    /// <summary>
    /// 设置文本
    /// </summary>
    /// <param name="text"></param>
    /// <param name="speed"></param>
    public void SetText(string text, float speed = -1)
    {
        Init();

        _defaultSpeed = speed > 0 ? speed : _defaultSpeed;
        _finalText = text;
        _label.text = string.Empty;

        if (_typeTextCoroutine != null)
        {
            StopCoroutine(_typeTextCoroutine);
        }

        _typeTextCoroutine = StartCoroutine(TypeText(text));
    }

    public IEnumerator TypeText(string text)
    {
        _currentText = "";

        var len = text.Length;
        var speed = _defaultSpeed;
        var tagOpened = false;
        var tagType = "";
        for (var i = 0; i < len; i++)
        {
            // 

            var symbolDetected = false;
            for (var j = 0; j < _uguiSymbols.Length; j++)
            {
                var symbol = string.Format("<{0}>", _uguiSymbols[j]);
                if (text[i] == '<' && i + (1 + _uguiSymbols[j].Length) < len && text.Substring(i, 2 + _uguiSymbols[j].Length).Equals(symbol))
                {
                    _currentText += symbol;
                    i += (2 + _uguiSymbols[j].Length) - 1;
                    symbolDetected = true;
                    tagOpened = true;
                    tagType = _uguiSymbols[j];
                    break;
                }
            }

            if (text[i] == '<' && i + (1 + 15) < len && text.Substring(i, 2 + 6).Equals("<color=#") && text[i + 16] == '>')
            {
                _currentText += text.Substring(i, 2 + 6 + 8);
                i += (2 + 14) - 1;
                symbolDetected = true;
                tagOpened = true;
                tagType = "color";
            }

            if (text[i] == '<' && i + 5 < len && text.Substring(i, 6).Equals("<size="))
            {
                var parseSize = "";
                var size = (float)_label.fontSize;
                for (var j = i + 6; j < len; j++)
                {
                    if (text[j] == '>') break;
                    parseSize += text[j];
                }

                if (float.TryParse(parseSize, out size))
                {
                    _currentText += text.Substring(i, 7 + parseSize.Length);
                    i += (7 + parseSize.Length) - 1;
                    symbolDetected = true;
                    tagOpened = true;
                    tagType = "size";
                }
            }

            // exit symbol
            for (var j = 0; j < _uguiCloseSymbols.Length; j++)
            {
                var symbol = string.Format("</{0}>", _uguiCloseSymbols[j]);
                if (text[i] == '<' && i + (2 + _uguiCloseSymbols[j].Length) < len && text.Substring(i, 3 + _uguiCloseSymbols[j].Length).Equals(symbol))
                {
                    _currentText += symbol;
                    i += (3 + _uguiCloseSymbols[j].Length) - 1;
                    symbolDetected = true;
                    tagOpened = false;
                    break;
                }
            }

            if (symbolDetected) continue;

            _currentText += text[i];
            _label.text = _currentText + (tagOpened ? string.Format("</{0}>", tagType) : "");
            yield return new WaitForSeconds(speed);
        }

        _typeTextCoroutine = null;

        if (_onCompleteCallback != null)
            _onCompleteCallback();
    }

    /// <summary>
    /// 跳过打字
    /// </summary>
    public void SkipTypeText()
    {
        if (_typeTextCoroutine != null)
        {
            StopCoroutine(_typeTextCoroutine);
        }
        _typeTextCoroutine = null;

        _label.text = _finalText;

        if (_onCompleteCallback != null)
        {
            _onCompleteCallback();
        }
    }

    /// <summary>
    /// 是否可跳过
    /// </summary>
    public bool IsSkippable()
    {
        return _typeTextCoroutine != null;
    }

    /// <summary>
    /// 设置完成回调
    /// </summary>
    public void SetOnComplete(Action onComplete)
    {
        _onCompleteCallback = onComplete;
    }

}

public static class TypeTextComponentUtility
{

    public static void TypeText(this TextMeshProUGUI label, string text, float speed = 0.04f, Action onComplete = null)
    {
        if (!label.TryGetComponent<TypeTextComponent>(out var typeText))
        {
            typeText = label.gameObject.AddComponent<TypeTextComponent>();
        }

        typeText.SetText(text, speed);
        typeText.SetOnComplete(onComplete);
    }

    public static bool IsSkippable(this TextMeshProUGUI label)
    {
        if (!label.TryGetComponent<TypeTextComponent>(out var typeText))
        {
            typeText = label.gameObject.AddComponent<TypeTextComponent>();
        }

        return typeText.IsSkippable();
    }

    public static void SkipTypeText(this TextMeshProUGUI label)
    {
        if (!label.TryGetComponent<TypeTextComponent>(out var typeText))
        {
            typeText = label.gameObject.AddComponent<TypeTextComponent>();
        }

        typeText.SkipTypeText();
    }

}