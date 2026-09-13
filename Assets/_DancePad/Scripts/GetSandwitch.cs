using System.Collections;
using System.IO.Ports;
using UnityEngine;

public class GetSandwitch : MonoBehaviour
{
    [SerializeField] private string portName = "COM3";
    [SerializeField] private int baudRate = 9600;
    [SerializeField] private float reconnectInterval = 1f; // интервал попыток (сек)

    private SerialPort serialPort;
    private float reconnectTimer = 0f;
    private bool wasConnected = false;

    private void Start()
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

    private void OnEnable()
    {
        GameEvents.OnWinGame += SetSandwich;
    }

    private void OnDisable()
    {
        GameEvents.OnWinGame -= SetSandwich;
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

    public void SetSandwich()
    {
        if (serialPort == null || !serialPort.IsOpen)
        {
            Debug.LogWarning("���������������� ���� �� ������.");
            return;
        }

        string command = "SANDWICH";
        string fullCommand = command + "\n";

        try
        {
            serialPort.Write(fullCommand);
            Debug.Log("����������: " + fullCommand.Trim());
        }
        catch (System.Exception e)
        {
            Debug.LogError("������ ��������: " + e.Message);
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
