using System.Collections;
using System.IO.Ports;
using UnityEngine;

public class GetSandwitch : MonoBehaviour
{
    [SerializeField] private string portName = "COM3"; // измените на свой порт
    [SerializeField] private int baudRate = 9600;

    private SerialPort serialPort;

    private void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            serialPort.ReadTimeout = 10;
            Debug.Log("Порт открыт.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Не удалось открыть порт: " + e.Message);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnWinGame += SetSandwich;
    }

    private void OnDisable()
    {
        GameEvents.OnWinGame -= SetSandwich;
    }

    public void SetSandwich()
    {
        if (serialPort == null || !serialPort.IsOpen)
        {
            Debug.LogWarning("Последовательный порт не открыт.");
            return;
        }

        string command = "SANDWICH";
        string fullCommand = command + "\n";

        try
        {
            serialPort.Write(fullCommand);
            Debug.Log("Отправлено: " + fullCommand.Trim());
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка отправки: " + e.Message);
        }
    }

    [ContextMenu("Get Sandwitch")]
    private void DebugGet()
    {
        StartCoroutine(DebugGetCor());
    }

    private IEnumerator DebugGetCor()
    {
        while (true)
        {
            SetSandwich();
            yield return new WaitForSeconds(20f);
        }
    }
}
