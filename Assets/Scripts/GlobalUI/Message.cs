using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 消息类型
/// </summary>
public enum MessageType
{
    Info,
    Warning,
    Error
}

/// <summary>
/// 消息
/// </summary>
public class Message : GlobalUIBase
{
    public Image bgImage;
    public TextMeshProUGUI contentText;

    public void SetMessage(string content, MessageType type = MessageType.Info)
    {
        contentText.text = content;

        Color color = type switch
        {
            MessageType.Info => new Color(0.2f, 0.2f, 0.2f, 0.9f),
            MessageType.Warning => new Color(0.9f, 0.6f, 0.1f, 0.9f),
            MessageType.Error => new Color(0.9f, 0.2f, 0.2f, 0.9f),
            _ => Color.black
        };

        contentText.color = color;
    }
}