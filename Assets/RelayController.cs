using System.IO.Ports;
using UnityEngine;

public class RelayController : MonoBehaviour
{
    [SerializeField] private string portName = "COM3"; // измените на свой порт
    [SerializeField] private int baudRate = 9600;

    private SerialPort serialPort;
    private string incomingBuffer = ""; // накопление строки

    void Start()
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

    void Update()
    {
        // Чтение данных из порта
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                // Читаем все доступные байты
                while (serialPort.BytesToRead > 0)
                {
                    char c = (char)serialPort.ReadChar();
                    if (c == '\n')
                    {
                        // Завершённая строка
                        ProcessCommand(incomingBuffer.Trim());
                        incomingBuffer = "";
                    }
                    else
                    {
                        incomingBuffer += c;
                    }
                }
            }
            catch (System.TimeoutException)
            {
                // Игнорируем таймауты
            }
        }
    }

    // Обработка полученной команды
    private void ProcessCommand(string command)
    {
        if (string.IsNullOrEmpty(command)) return;

        // Если команда — это одиночный символ из нашего набора
        if (command.Length == 1)
        {
            char key = command[0];
            switch (key)
            {
                case 'q': OnQ(); break;
                case 'w': OnW(); break;
                case 'e': OnE(); break;
                case 'a': OnA(); break;
                case 'd': OnD(); break;
                case 'z': OnZ(); break;
                case 's': OnS(); break;
                case 'c': OnC(); break;
                default:
                    Debug.LogWarning("Неизвестный символ: " + command);
                    break;
            }
        }
        else
        {
            // Если это не одиночный символ – можно проигнорировать или добавить другую логику
            Debug.Log("Получено: " + command);
        }
    }

    // ----- Методы для каждой клавиши (пока пустые) -----
    private void OnQ() { Debug.Log("Нажата Q"); GameEvents.RaiseDancePadPressed(DancePadDirection.UpLeft); }
    private void OnW() { Debug.Log("Нажата W"); GameEvents.RaiseDancePadPressed(DancePadDirection.Up); }
    private void OnE() { Debug.Log("Нажата E"); GameEvents.RaiseDancePadPressed(DancePadDirection.UpRight); }
    private void OnA() { Debug.Log("Нажата A"); GameEvents.RaiseDancePadPressed(DancePadDirection.Left); }
    private void OnD() { Debug.Log("Нажата D"); GameEvents.RaiseDancePadPressed(DancePadDirection.Right); }
    private void OnZ() { Debug.Log("Нажата Z"); GameEvents.RaiseDancePadPressed(DancePadDirection.DownLeft); }
    private void OnS() { Debug.Log("Нажата S"); GameEvents.RaiseDancePadPressed(DancePadDirection.Down); }
    private void OnC() { Debug.Log("Нажата C"); GameEvents.RaiseDancePadPressed(DancePadDirection.DownRight); }

    // ----- Метод для отправки команд на реле (используется извне) -----
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

        string command = state ? "On" : "Of";
        string fullCommand = command + relayIndex + "\n";

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
}
/*
///*
/// Как это работает
Arduino отправляет по Serial строку вида "q\n" при нажатии кнопки, соответствующей букве 'q'.

Unity в Update() читает данные из порта, накапливает их до символа \n, затем обрабатывает команду.

Если команда – одиночная буква из списка, вызывается соответствующий метод (например, OnQ()).

Если команда начинается с "On" или "Of" (это мы отправляем из Unity через SetRelay), то Arduino обрабатывает её как команду управления реле.

Таким образом, канал используется в обе стороны: из Arduino в Unity идёт информация о нажатых кнопках, из Unity в Arduino – команды на реле.
///
//
//

/*/
