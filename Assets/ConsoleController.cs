using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI consoleText;
    public Button clear;
    [SerializeField] private int maxLines = 21;
    private string currentText = "";
    public static ConsoleController In;

    // Черга для зберігання дій, які повинні бути виконані в основному потоці
    private readonly Queue<Action> mainThreadActions = new Queue<Action>();

    private void Awake() => In = this;

    private void Start()
    {
        clear.onClick.AddListener(() =>
        {
            Clear();
        });
    }

    private void Update()
    {
        // Виконуємо всі дії з черги в основному потоці
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                var action = mainThreadActions.Dequeue();
                action.Invoke();
            }
        }
    }

    // Використовується для безпечного виклику логування в основному потоці
    public static void Log(string message)
    {
        In.EnqueueAction(() => In.PrivateLog(message));
    }

    public static void LogError(string message)
    {
        In.EnqueueAction(() => In.PrivateLogError(message));
    }

    // Метод для додавання дії в чергу, яка буде виконана в основному потоці
    private void EnqueueAction(Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    private void PrivateLog(string message)
    {
        currentText += message + "\n";

        string[] lines = currentText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length > maxLines)
        {
            currentText = string.Join("\n", lines, lines.Length - maxLines, maxLines);
        }

        consoleText.text = currentText;
    }

    private void PrivateLogError(string message)
    {
        currentText += $"<color=red>{message}</color>\n";
        string[] lines = currentText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > maxLines)
        {
            currentText = string.Join("\n", lines, lines.Length - maxLines, maxLines);
        }
        consoleText.text = currentText;
    }

    public void Clear()
    {
        currentText = "";
        consoleText.text = currentText;
    }
}