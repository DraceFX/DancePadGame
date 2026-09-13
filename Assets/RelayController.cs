using System.IO.Ports;
using UnityEngine;

public class RelayController : MonoBehaviour
{
    [SerializeField] private string portName = "COM3";
    [SerializeField] private int baudRate = 9600;
    [SerializeField] private float reconnectInterval = 1f; // интервал попыток (сек)

    private SerialPort serialPort;
    private float reconnectTimer = 0f;
    private bool wasConnected = false;

    void Start()
    {
        TryConnect();
    }

    private void Update()
    {
        // Если порт не открыт – пытаемся подключиться раз в reconnectInterval секунд
        if (serialPort == null || !serialPort.IsOpen)
        {
            reconnectTimer += Time.deltaTime;
            if (reconnectTimer >= reconnectInterval)
            {
                reconnectTimer = 0f;
                TryConnect();
            }
            return;
        }

        // (здесь остальная логика чтения данных, если порт открыт)
    }

    private void TryConnect()
    {
        ClosePort(); // закрываем старый порт, если остался

        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 10;
            serialPort.Open();

            if (!wasConnected)
            {
                Debug.Log("Порт " + portName + " открыт.");
                wasConnected = true;
            }
            else
            {
                Debug.Log("Порт " + portName + " переподключён.");
            }
        }
        catch (System.Exception e)
        {
            if (wasConnected)
            {
                Debug.LogWarning("Порт " + portName + " недоступен: " + e.Message);
                wasConnected = false;
            }
            serialPort = null;
        }
    }

    private void ClosePort()
    {
        if (serialPort != null)
        {
            try
            {
                if (serialPort.IsOpen)
                    serialPort.Close();
                serialPort.Dispose();
            }
            catch { }
            serialPort = null;
        }
    }

    void OnDestroy()
    {
        ClosePort();
    }

    /*
     * w  - 1
     * a - 3
     * s - 2
     * d - 4
     * q - 5
     * e - 6
     * z - 7
     * c - 8
     */

    // Метод для отправки команды включения/выключения реле
    public void SetRelay(int relayIndex, bool state)
    {
        if (relayIndex < 0 || relayIndex > 7)
        {
            Debug.LogWarning("Индекс реле должен быть от 0 до 7.");
            return;
        }

        if (serialPort == null || !serialPort.IsOpen)
        {
            Debug.LogWarning("Последовательный порт не открыт.");
            return;
        }

        string command = state ? "On" : "Of"; // используем "Of" как сокращение для "Off"
        string fullCommand = command + relayIndex + "\n"; // добавляем перевод строки

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
}