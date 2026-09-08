using System.IO.Ports;
using UnityEngine;

public class RelayController : MonoBehaviour
{
    [SerializeField] private string portName = "COM3"; // укажите свой порт
    [SerializeField] private int baudRate = 9600;

    private SerialPort serialPort;

    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            serialPort.ReadTimeout = 100;
            Debug.Log("Порт открыт.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Не удалось открыть порт: " + e.Message);
        }
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

    void OnDestroy()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            serialPort.Dispose();
            Debug.Log("Порт закрыт.");
        }
    }

    // Пример использования: включаем реле 0, выключаем реле 3
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    SetRelay(0, true); // включить реле 0
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    SetRelay(0, false); // выключить реле 0
        //}
        // и так далее...
    }
}