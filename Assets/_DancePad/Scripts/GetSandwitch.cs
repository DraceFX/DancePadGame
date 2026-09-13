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
#if UNITY_EDITOR
                Debug.Log("Порт " + portName + " открыт.");
#endif
                wasConnected = true;
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("Порт " + portName + " переподключён.");
#endif
            }
        }
        catch (System.Exception e)
        {
            if (wasConnected)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Порт " + portName + " недоступен: " + e.Message);
#endif
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
#if UNITY_EDITOR
            Debug.LogWarning("���������������� ���� �� ������.");
#endif
            return;
        }

        string command = "SANDWICH";
        string fullCommand = command + "\n";

        try
        {
            serialPort.Write(fullCommand);
#if UNITY_EDITOR
            Debug.Log("����������: " + fullCommand.Trim());
#endif
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            Debug.LogError("������ ��������: " + e.Message);
#endif
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
