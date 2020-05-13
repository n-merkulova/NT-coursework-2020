using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;


namespace main_application
{
    public partial class Form1 : Form
    {
        public bool DEBUG_MODE = false;
        public bool RET_ALLOWED = false;

        // Использется для передачи байтов по COM порту
        public static Encoding WIN1251 = Encoding.GetEncoding("windows-1251");
        public static Encoding ASCII = Encoding.ASCII;

        public Dictionary<string, string> Auth_status = new Dictionary<string, string> {
            { "ACK1", "undef" },
            { "ACK2", "undef" },
            { "ACK_local", "undef" }
        };
        public Mutex Auth_status_mutex = new Mutex();

        // Перечисление состояний соединения
        public enum Connection_Status
        {
            CONNECTION_WAIT,
            CONNECTED,
            DISCONNECTION_WAIT,
            DISCONNECTED,
        };

        //Флаги наличия абонентов на линии
        public Connection_Status Phys_status1 = Connection_Status.DISCONNECTED;
        public Connection_Status Phys_status2 = Connection_Status.DISCONNECTED;
        public Mutex Phys_status1_mutex = new Mutex();
        public Mutex Phys_status2_mutex = new Mutex();

        // Здесь отмечаются только сообщения информационного кадра (пока так)
        One_Task LastFrameSenttoPort1 = new One_Task(null, null);
        One_Task LastFrameSenttoPort2 = new One_Task(null, null);

        public Mutex LastFrame_ToSend_mutex = new Mutex();

        public Int32 Ack1_awaited_Auth = 0;
        public Int32 Ack2_awaited_Auth = 0;
        public Mutex Ack1_mutex_Auth = new Mutex();
        public Mutex Ack2_mutex_Auth = new Mutex();

        //Парамаетры компортов
        string SelectedPort1Name;
        Mutex SelectedPort1Name_mutex = new Mutex();
        string SelectedPort2Name;
        Mutex SelectedPort2Name_mutex = new Mutex();
        string SelectedBaudrate;
        Mutex SelectedBaudrate_mutex = new Mutex();

        //Мьютекс для согласования: чтения, записи, и изменения в списке ReceivedFrames
        public Mutex ReceivedFrames_mutex1 = new Mutex();

        //Мьютекс для согласования: чтения, записи, и изменения в списке ReceivedFrames2
        public Mutex ReceivedFrames_mutex2 = new Mutex();

        //Списки принятых данных для обмена между потоками Serial1_receiving1() и FindFrame1()
        public List<byte> ReceivedFrames1 = new List<byte>();

        //Списки принятых данных для обмена между потоками Serial1_receiving2() и FindFrame2()
        public List<byte> ReceivedFrames2 = new List<byte>();

        // Структура для хранения задания (кадра)
        public struct One_Task
        {
            public string PortNum;
            public byte[] Frame;
            public One_Task(string name, byte[] frame)
            {
                PortNum = name;
                Frame = frame;
            }
        }

        // Список заданий используется четырьмя потоками - По два на каждый порт
        // Формат записи заданий One_Task("Номер порта", Байты[] пришедшие в порт)
        // Список заданий, новое задание помещается в конец списка, выполненное удаляется из начала списка
        // Список заданий содержит задания из первого и второго порта
        public List<One_Task> TasksReceived = new List<One_Task>();

        // Мьютекс для согласования: чтения, записи, и изменения заданий
        // Использется для первого и второго порта одновременно
        public Mutex TaskReceived_mutex = new Mutex();


        public List<One_Task> TasksToSend = new List<One_Task>();

        // Мьютекс для согласования: чтения, записи, и изменения заданий
        // Использется для первого и второго порта одновременно
        public Mutex TaskToSend_mutex = new Mutex();

        public Dictionary<string, string> AuthData = new Dictionary<string, string>
        {
            { "Port1", null },
            { "Port2", null },
            { "local", null }
        };
        public Mutex AuthData_mutex = new Mutex();

        public Boolean LetterReceived_flag = false;

        #region Описание Модели Данных

        class inbox_class
        {
            public string id { get; set; }
            public string sender { get; set; }
            public string recepient { get; set; }
            public string re { get; set; }
            public string msg { get; set; }
            public string status { get; set; }
            public string date_received { get; set; }
            public string foreign_id { get; set; }
            public inbox_class()
            { }
            public inbox_class(inbox letter)
            {
                this.foreign_id = letter.id.ToString();
                this.sender = letter.sender.ToString();
                this.recepient = letter.recepient.ToString();
                this.re = letter.re.ToString();
                this.msg = letter.msg.ToString();
                this.status = letter.status.ToString();
                this.date_received = letter.date_received.ToString();
                this.id = "";
            }
        };
        class outbox_class
        {
            public string id { get; set; }
            public string sender { get; set; }
            public string recepient { get; set; }
            public string re { get; set; }
            public string msg { get; set; }
            public string status { get; set; }
            public string date_sent { get; set; }
            public outbox_class()
            { }
            public outbox_class(outbox letter)
            {
                this.id = letter.id.ToString();
                this.sender = letter.sender.ToString();
                this.recepient = letter.recepient.ToString();
                this.re = letter.re.ToString();
                this.msg = letter.msg.ToString();
                this.status = letter.status.ToString();

                //this.date_sent = letter.date_sent.ToString();

            }
        };
        #endregion

        /**************************************************************
                   ФЛАГИ ДЛЯ ОБНОВЛЕНИЯ ПАПОК С ПИСЬМАМИ
        **************************************************************/

        public Mutex Inbox_update_mutex = new Mutex();
        public Mutex Outbox_update_mutex = new Mutex();

        public bool Inbox_update_needed = false;
        public bool Outbox_update_needed = false;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        #region КОДИРОВАНИЕ СООБЩЕНИЙ   
        /***********************************************************
                КОДИРОВАНИЕ СООБЩЕНИЙ И СБОРКА КАДРОВ                   
        ***********************************************************/

        class Codings
        {
            public static ushort SetDatalen(byte len1, byte len2)
            {
                return BitConverter.ToUInt16(new byte[] { len1, len2 }, 0);
            }

            // Таблица для определения синдрома ошибки
            public static Dictionary<byte, byte> synd_table = new Dictionary<byte, byte>
            {
                { 0x01, 0x01 },
                { 0x02, 0x02 },
                { 0x04, 0x04 },
                { 0x03, 0x08 },
                { 0x06, 0x10 },
                { 0x07, 0x20 },
                { 0x05, 0x40 }
            };

            //Получение битов контрольной суммы из исходных 4 бит //ОК
            public static byte GetChecksum(byte data)
            {
                byte temp = data;
                //Порождающий полином
                byte poly = 0x58;
                short shift_count = 0;

                byte mask = 0x40;
                short counter = 20;
                byte checksum;
                while (counter > 0)
                {
                    temp = (byte)(temp ^ poly);
                    if ((temp & mask) == mask)
                    {
                        continue;
                    }
                    else {
                        if (shift_count < 3)
                        {
                            mask = (byte)(mask >> 1);
                            poly = (byte)(poly >> 1);
                            shift_count++;
                        }
                        else { break; }
                    }
                    counter--;
                }
                checksum = temp;
                return checksum;
            }
            //OK
            public static byte EncodeDataBits(byte decoded_value)
            {
                byte checksum = GetChecksum((byte)(decoded_value << 3));
                byte encoded_value = (byte)((decoded_value << 3) + checksum);
                return encoded_value;
            }

            //Принимает байт с контрольной суммой, возвращает полубайт
            public static byte DecodeDataBits(byte encoded_value)
            {
                byte decoded_value;
                byte checksum = GetChecksum(encoded_value);
                if (checksum == 0)
                {
                    decoded_value = (byte)(encoded_value >> 3);
                }
                else
                {
                    decoded_value = (byte)((synd_table[checksum] ^ encoded_value) >> 3);
                }
                return decoded_value;
            }

            //Принимает массив полубайтов win1251 -> выдает строку на русском языке в utf-8
            public static string ByteMessageToString(byte[] message_data)
            {
                int Message_Len = (message_data.Length);
                byte[] bytestostring = new byte[Message_Len / 2];
                for (int i = 0; i < Message_Len / 2; i++)
                {
                    // Проверка crc в принятых байтах.
                    if (GetChecksum((byte)(message_data[i * 2] & 0x0F)) != (byte)(message_data[i * 2] >> 3))
                    {
                        return null;
                    }
                    if (GetChecksum((byte)(message_data[i * 2 + 1] & 0x0F)) != (byte)(message_data[i * 2 + 1] >> 3))
                    {
                        return null;
                    }
                }

                for (int i = 0; i < Message_Len / 2; i++)
                {
                    byte first = (byte)((message_data[i * 2] & 0x78) << 1);
                    byte second = (byte)((message_data[i * 2 + 1] & 0x78) >> 3);
                    byte code = (byte)(first + second);
                    bytestostring[i] = code;
                }

                byte[] utf8Bytes = Encoding.Convert(WIN1251, Encoding.UTF8, bytestostring);
                string Received_String = Encoding.UTF8.GetString(utf8Bytes);

                return Received_String;
            }
            ////Принимает строку на русском языке в utf-8 -> Выдает массив полубайтов win1251
            public static byte[] StringToByteMessage(string StringToSend)
            {
                byte[] BytesToSend = new byte[(StringToSend.Length) * 2];
                byte[] utf8bytes = Encoding.UTF8.GetBytes(StringToSend);
                byte[] win1251Bytes = Encoding.Convert(Encoding.UTF8, WIN1251, utf8bytes);
                string win1251string = WIN1251.GetString(win1251Bytes);

                for (int i = 0; i < win1251Bytes.Length; i++)
                {
                    byte nibble1 = EncodeDataBits((byte)(win1251Bytes[i] >> 4));
                    byte nibble2 = EncodeDataBits((byte)(win1251Bytes[i] & 0x0F));
                    BytesToSend[i * 2] = nibble1;
                    BytesToSend[(i * 2) + 1] = nibble2;
                }
                return BytesToSend;
            }
        };


        // Определение типов кадров
        public enum FrameType : byte
        {
            MEETING = 0x01,
            DISCONNECT = 0x02,
            LOGIN = 0x03,
            LOGOUT = 0x04,
            INFORMATION = 0x06,
            OPENLETTER = 0x07,
            ACK = 0x08,
            RET = 0x09
        };

        /******************************************************************
                         СОЗДАНИЕ И РАЗБОР КАДРОВ ЛЮБОГО ТИПА
        *******************************************************************/

        //сборка кадра для отправки в порт
        public byte[] CreateNewFrame(FrameType type, string senderstr, string datalenstr, string receiverstr, string payload, bool encoding = false)
        {
            List<byte> framebytes = new List<byte>();
            framebytes.Add((byte)0xFF);
            framebytes.Add((byte)type);
            byte sender;
            byte receiver;

            if (byte.TryParse(senderstr, out sender) && byte.TryParse(receiverstr, out receiver))
            {
                framebytes.Add((byte)sender);
                framebytes.Add((byte)receiver);
            }
            else {
                MessageBox.Show("FormNewFrame(): Не удалось преобразовать адрес машины в байт", "Error!");
                return null;
            }

            //Если тип кадра должен содержать данные                                 
            if (type == FrameType.INFORMATION || type == FrameType.LOGIN || type == FrameType.OPENLETTER || type == FrameType.MEETING)
            {
                UInt16 datalen;
                if (UInt16.TryParse(datalenstr, out datalen))
                {
                    byte highbyte = (BitConverter.GetBytes(datalen))[1];
                    byte lowbyte = (BitConverter.GetBytes(datalen))[0];
                    framebytes.Add((byte)highbyte);
                    framebytes.Add((byte)lowbyte);

                    //Если требуется кодирование данных
                    if (encoding == true)
                    {
                        byte[] double_encoded_payload = Codings.StringToByteMessage(payload);
                        framebytes.AddRange(double_encoded_payload);
                    }
                    else
                    {
                        byte[] utf8bytes = Encoding.UTF8.GetBytes(payload);
                        byte[] win1251Bytes = Encoding.Convert(Encoding.UTF8, WIN1251, utf8bytes);
                        framebytes.AddRange(win1251Bytes);

                    }
                }
                else {
                    MessageBox.Show("CreateNewFrame(): Не удалось преобразовать datalen  в Uint16 ", "Error!");
                    return null;
                }

            }
            //Добавление  стопового байта и возврат сформированного кадра в виде byte[]
            framebytes.Add((byte)0xFE);
            return framebytes.ToArray();
        }

        //разбор кадра, полученного из порта. Возвращает структуру,заполненную принятыми данными
        public DefaultFrame ParseReceivedFrame(byte[] frame, bool encoding = false)
        {
            DefaultFrame ParsedFrame = new DefaultFrame();
            if (frame == null) { ParsedFrame.ResultOfParsing = "Fail"; return ParsedFrame; }
            if (frame.Length < 5)
            {
                MessageBox.Show("ParseReceivedFrame(). Длина кадра меньше минимальной", "Error!");
                ParsedFrame.ResultOfParsing = "Failed";
                return ParsedFrame;
            }
            ParsedFrame.Startbyte = frame[0];
            ParsedFrame.Frametype = frame[1];

            ParsedFrame.OriginPort = ((UInt16)(frame[2])).ToString();
            ParsedFrame.DestinationPort = ((UInt16)(frame[3])).ToString();
            ParsedFrame.Stopbyte = frame[(frame.Length - 1)];

            byte type = ParsedFrame.Frametype;

            ParsedFrame.ResultOfParsing = "OK";
            // Кадры данных типов могут быть носителями payload'а
            if (type == (byte)FrameType.INFORMATION || type == (byte)FrameType.LOGIN || type == (byte)FrameType.OPENLETTER || type == (byte)FrameType.MEETING)
            {
                List<byte> data = new List<byte>();
                // Старший байт - 4, младший - 5.  В кадре передается в формате big-endian
                ParsedFrame.datalen = BitConverter.ToUInt16(new byte[] { frame[5], frame[4] }, 0);

                if (encoding == false)
                {
                    // проверка на совпадение размера данных с указанным в кадре значением
                    // 2 флаговых байта, 2 - адресация, 2 - колчество символов  win 1251 , 1 - тип кадра
                    if (frame.Length != (7 + ParsedFrame.datalen))
                    {
                        MessageBox.Show("ParseReceivedFrame(enc==false) Длина поля данных в кадре не совпадает с указанной", "Error!");
                        //Установка флага "Пришел битый кадр" 
                        ParsedFrame.ResultOfParsing = "Fail";
                        return ParsedFrame;
                    }
                    else {

                        for (int i = 6; i < frame.Length - 1; i++)
                        {
                            data.Add(frame[i]);
                        }

                    }

                    ParsedFrame.MessageData = WIN1251.GetString(data.ToArray());
                }
                else if (encoding == true)
                {
                    List<byte> doubled_frame_data = new List<byte>();

                    if (frame.Length != (7 + ParsedFrame.datalen * 2))
                    {
                        MessageBox.Show("ParseReceivedFrame(enc==true) Длина поля данных в кадре не совпадает с указанной ", "Error!");
                        //Установка флага "Пришел битый кадр" 
                        ParsedFrame.ResultOfParsing = "Fail";
                        return ParsedFrame;
                    }
                    for (int i = 6; i < frame.Length - 1; i++)
                    {
                        doubled_frame_data.Add(frame[i]);
                    }
                    //Возвращенная строка в UTF-8
                    string decoded_from_doubled = Codings.ByteMessageToString(doubled_frame_data.ToArray());
                    if (decoded_from_doubled == null)
                    {
                        MessageBox.Show("ParseReceivedFrame(encoding==true) Ошибка в crc кадра", "Error!");
                        //Установка флага "Пришел битый кадр" 
                        ParsedFrame.ResultOfParsing = "Fail";
                        return ParsedFrame;
                    }
                    else
                    { ParsedFrame.MessageData = decoded_from_doubled; }

                }

            }
            //Если кадр не должен содержать данные
            else
            {
                if (frame.Length > 5)
                {
                    ParsedFrame.ResultOfParsing = "Fail";
                    return ParsedFrame;
                }

            }
            return ParsedFrame;
        }
        //Структура для хранения результатов разбора любого кадра
        public struct DefaultFrame
        {
            public byte Startbyte;
            public byte Frametype;
            public string OriginPort;
            public string DestinationPort;
            public UInt16 datalen;
            public string MessageData;
            public byte Stopbyte;

            public string ResultOfParsing;
            public string PortName;
        }
        /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                         КОДИРОВАНИЕ СООБЩЕНИЙ    
        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/
        #endregion

        public Form1()
        {
            InitializeComponent();
            string[] portslist = SerialPort.GetPortNames();

            /*************************************************************
                              ЗАДАНИЕ ПАРАМЕТРОВ COM ПОРТА  
             *************************************************************/
            // Начальное задание параметров COM портов
            // Соединенные пары портов { COM3 <-> COM4, COM6 <-> COM7, COM8 <-> COM9 }
            serialPort1.Encoding = WIN1251;
            serialPort1.BaudRate = 9600;
            serialPort1.DataBits = 8;
            serialPort1.Parity = Parity.None;
            serialPort1.StopBits = StopBits.One;
            serialPort1.Handshake = Handshake.RequestToSend;
            serialPort1.PortName = "COM3";

            serialPort2.Encoding = WIN1251;
            serialPort2.BaudRate = 9600;
            serialPort2.DataBits = 8;
            serialPort2.Parity = Parity.None;
            serialPort2.StopBits = StopBits.One;
            serialPort2.Handshake = Handshake.RequestToSend;
            serialPort2.PortName = "COM6";

            toolStripComboBox3.SelectedItem = "9600";
            SelectedBaudrate = "9600";
            toolStripComboBox1.Items.AddRange(SerialPort.GetPortNames());
            toolStripComboBox2.Items.AddRange(SerialPort.GetPortNames());

            //Port1 
            toolStripComboBox1.SelectedItem = "COM3";
            SelectedPort1Name = "COM3";

            //Port2
            toolStripComboBox2.SelectedItem = "COM6";
            SelectedPort2Name = "COM6";

            this.button1.Enabled = true;
            this.button4.Enabled = false;
            this.AuthConnectButton.Enabled = true;
            this.AuthDisconnectButton.Enabled = false;

            /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                            ЗАДАНИЕ ПАРАМЕТРОВ КОМПОРТА    
             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        
        /****************************************************************
               ПОЛУЧЕНИЕ КАДРОВ ИЗ ПОРТОВ И ЗАПОЛНЕНИЕ СПИСКА ЗАДАНИЙ
         ****************************************************************/
        public enum ReceiveState { SOF_FOUND, EOF_FOUND, FREE }
        // Функции для считывания данных из COM порта в отдельном потоке
        public void Serial1_StartReceiving()
        {
            if (!serialPort1.IsOpen)
            {
                { MessageBox.Show("Порт Закрыт!", serialPort1.PortName); }
            }
            //Очистка буфера перед началом нового сеанса
            serialPort1.DiscardInBuffer();
            int bytestoread;
            while (serialPort1.IsOpen)
            {
                bytestoread = serialPort1.BytesToRead;
                //Буфер для чтения из порта
                byte[] ReceivedBytes = new byte[bytestoread];
                if (bytestoread > 0)
                {
                    try
                    {
                        // Чтение принятых данных из компорта в буферный массив принятых байтов
                        serialPort1.Read(ReceivedBytes, 0, bytestoread);
                        // Вход в критическую секцию
                        // разделяемый ресурс- Список байт, принятых из порта ReceivedFrames
                        ReceivedFrames_mutex1.WaitOne();
                        // Запись новых данных из компорта в глобальный Массив Принятых данных   
                        ReceivedFrames1.AddRange(ReceivedBytes);
                        ReceivedFrames_mutex1.ReleaseMutex();
                        // Выход из критической секции
                    }
                    catch (InvalidOperationException ex)
                    { MessageBox.Show(ex.Message, "Error!"); }
                }
                Thread.Sleep(20);
            }
        }

        // просматривает Список ReceivedFrames и разбирает найденные кадры
        // Читает поток входящих байт из порта 1 и заполняет список заданий в формате One_Task(Номер порта, Байты кадра)
        public void FindFrameInPort1()
        {
            int startbyte;
            int stopbyte;
            List<byte> Frame = new List<byte>();
            // Начальное состояние функции при запуске
            ReceiveState State = ReceiveState.FREE;
            while (true)
            {
                // Вход в критическую секцию
                // разделяемый ресурс- Список байт, принятых из порта
                ReceivedFrames_mutex1.WaitOne();
                // Выполнение условия, только если новый кадр ещё не поступил, а старый уже обработан
                if (State == ReceiveState.FREE)
                {
                    startbyte = ReceivedFrames1.IndexOf(0xFF);  // 11111111
                    stopbyte = ReceivedFrames1.IndexOf(0xFE);  // 11111110
                    //Если Начало кадра найдено, то отмечаем новое состояние 
                    if (startbyte != -1)
                    {
                        State = ReceiveState.SOF_FOUND;
                        //Удаляем всё, что было до начала кадра, т.е. теперь буфер кадров содержит только начало кадра и возможно конец
                        ReceivedFrames1.RemoveRange(0, startbyte);
                        //Далее, если конец кадра не обнаружен, то вырезаем всё, что накопилось в буфере кадров в локальный контейнер для кадра
                        if (stopbyte == -1)
                        {
                            Frame.AddRange(ReceivedFrames1.GetRange(0, ReceivedFrames1.Count));
                            ReceivedFrames1.RemoveRange(0, ReceivedFrames1.Count);
                        }
                        //Иначе вырезаем лишь часть и разбираем кадр(Передача контейнера кадра на разбор )
                        else
                        if (stopbyte != -1)
                        {
                            //Получение новых индексов для урезанного списка
                            startbyte = ReceivedFrames1.IndexOf(0xFF);
                            stopbyte = ReceivedFrames1.IndexOf(0xFE);
                            if (stopbyte == -1)
                            {
                                Frame.AddRange(ReceivedFrames1.GetRange(0, ReceivedFrames1.Count));
                                ReceivedFrames1.RemoveRange(0, ReceivedFrames1.Count);
                            }
                            else {
                                Frame.AddRange(ReceivedFrames1.GetRange(0, stopbyte - startbyte + 1));
                                ReceivedFrames1.RemoveRange(0, stopbyte - startbyte + 1);
                                State = ReceiveState.EOF_FOUND;
                                //Запуск разбора кадра Frame, после этого перевод в состояние FREE И очистка контейнера кадра

                                //Запись найденного кадра в список заданий
                                TaskReceived_mutex.WaitOne();
                                TasksReceived.Add(new One_Task("Port1", Frame.ToArray()));
                                TaskReceived_mutex.ReleaseMutex();
                                Frame.Clear();

                                State = ReceiveState.FREE;
                            }
                        }
                    }
                    //Если Начало кадра не найдено и при этом система готова к приему нового кадра,
                    // значит в порт пришел мусор, его удаляем из буфера кадров
                    else
                    { ReceivedFrames1.Clear();}

                }
                // Если уже был обнаружен стартовый байт, значит принимаем всё до конца буфера или до конечного байта 
                // Заход в эту область происходит, если размер кадра оказался больше, чем буфер приема
                else if (State == ReceiveState.SOF_FOUND)
                {
                    stopbyte = ReceivedFrames1.IndexOf(0xFE);
                    // Если не найден стоповый байт, достаём из буфера кадров всё, что там есть 
                    if (stopbyte == -1)
                    {
                        startbyte = ReceivedFrames1.IndexOf(0xFF);
                        if ((startbyte != -1) && State == ReceiveState.SOF_FOUND)
                        {
                            MessageBox.Show("Функция FindFrame1(), найден кадр без стопового байта", "Error!");
                            ReceivedFrames_mutex1.ReleaseMutex();
                            Frame.Clear();
                            State = ReceiveState.FREE;
                            continue;
                        }
                        Frame.AddRange(ReceivedFrames1.GetRange(0, ReceivedFrames1.Count));
                        ReceivedFrames1.RemoveRange(0, ReceivedFrames1.Count);

                    }
                    // Если стоповый байт найден, тогда вырезаем всё, что расположено до стопового байта, далее разбор кадра
                    else if (stopbyte != -1)
                    {
                        startbyte = ReceivedFrames1.IndexOf(0xFF);
                        if ((startbyte != -1) && State == ReceiveState.SOF_FOUND)
                        {
                            MessageBox.Show("Функция FindFrame1(), найден кадр без стопового байта", "Error!");
                            ReceivedFrames_mutex1.ReleaseMutex();
                            Frame.Clear();
                            State = ReceiveState.FREE;
                            continue;
                        }
                        stopbyte = ReceivedFrames1.IndexOf(0xFE);
                        // Добавляем в контейнер кадра до стопового байта, остальное переносим в начало буфера
                        Frame.AddRange(ReceivedFrames1.GetRange(0, stopbyte + 1));
                        ReceivedFrames1.RemoveRange(0, stopbyte + 1);
                        State = ReceiveState.EOF_FOUND;
                        // Запуск разбора кадра Frame, после этого перевод в состояние FREE и очистка контейнера кадра

                        // Запись найденного кадра в список заданий
                        TaskReceived_mutex.WaitOne();
                        TasksReceived.Add(new One_Task("Port1", Frame.ToArray()));
                        TaskReceived_mutex.ReleaseMutex();
                        Frame.Clear();
                        State = ReceiveState.FREE;
                    }
                }
                // Переход в эту область не должен происходить
                else if (State == ReceiveState.EOF_FOUND)
                {
                    MessageBox.Show(" Функция FindFrame1.\r\n Состояние осталось EOF_FOUND в начале прохода цикла", "Error!");
                    Frame.Clear();
                    State = ReceiveState.FREE;
                    continue;
                }
                ReceivedFrames_mutex1.ReleaseMutex();
                Thread.Sleep(10);
            }
        }
        
        //Всё тоже самое, что и для первого порта (Serial1_StartReceiving).
        public void Serial2_StartReceiving()
        {
            if (!serialPort2.IsOpen)
            { MessageBox.Show("Порт Закрыт!", serialPort2.PortName);}
            //Очистка буфера перед началом нового сеанса
            serialPort2.DiscardInBuffer();
            int bytestoread;
            while (serialPort2.IsOpen)
            {
                bytestoread = serialPort2.BytesToRead;
                //Буфер для чтения из порта
                byte[] ReceivedBytes = new byte[bytestoread];
                if (bytestoread > 0)
                {
                    try
                    {
                        //Чтение принятых данных из компорта в буферный массив принятых байтов
                        serialPort2.Read(ReceivedBytes, 0, bytestoread);
                        //Вход в критическую секцию
                        //разделяемый ресурс- Список байт, принятых из порта ReceivedFrames
                        ReceivedFrames_mutex2.WaitOne();
                        //Запись новых данных из компорта в глобальный Массив Принятых данных   
                        ReceivedFrames2.AddRange(ReceivedBytes);
                        ReceivedFrames_mutex2.ReleaseMutex();
                        //Выход из критической секции
                    }
                    catch (InvalidOperationException ex)
                    { MessageBox.Show(ex.Message, "Error!"); }
                }
                Thread.Sleep(20);
            }
        }

        // Всё тоже самое, что и для первого порта (FindFrameInPort1).
        public void FindFrameInPort2()
        {
            int startbyte;
            int stopbyte;
            List<byte> Frame = new List<byte>();
            //Наачльное состояние функции при запуске
            ReceiveState State = ReceiveState.FREE;
            while (true)
            {
                //Вход в критическую секцию
                //разделяемый ресурс- Список байт, принятых из порта
                ReceivedFrames_mutex2.WaitOne();
                //Выполнение условия, только если новый кадр ещё не поступил, а старый уже обработан
                if (State == ReceiveState.FREE)
                {
                    startbyte = ReceivedFrames2.IndexOf(0xFF);
                    stopbyte = ReceivedFrames2.IndexOf(0xFE);
                    //Если Начало кадра найдено, то отмечаем новое состояние 
                    if (startbyte != -1)
                    {
                        State = ReceiveState.SOF_FOUND;

                        //Удаляем всё, что было до начала кадра, т.е. теперь буфер кадров содержит только начало кадра и возможно конец
                        ReceivedFrames2.RemoveRange(0, startbyte);

                        //Далее, если конец кадра не обнаружен, то вырезаем всё, что накопилось в буфере кадров в локальный контейнер для кадра
                        if (stopbyte == -1)
                        {
                            Frame.AddRange(ReceivedFrames2.GetRange(0, ReceivedFrames2.Count));
                            ReceivedFrames2.RemoveRange(0, ReceivedFrames2.Count);
                        }
                        //Иначе вырезаем лишь часть и разбираем кадр(Передача контейнера кадра на разбор )
                        else
                        if (stopbyte != -1)
                        {
                            //Получение новых индексов для урезанного списка
                            startbyte = ReceivedFrames2.IndexOf(0xFF);
                            stopbyte = ReceivedFrames2.IndexOf(0xFE);
                            if (stopbyte == -1)
                            {
                                Frame.AddRange(ReceivedFrames2.GetRange(0, ReceivedFrames2.Count));
                                ReceivedFrames2.RemoveRange(0, ReceivedFrames2.Count);
                            }
                            else {
                                Frame.AddRange(ReceivedFrames2.GetRange(0, stopbyte - startbyte + 1));
                                ReceivedFrames2.RemoveRange(0, stopbyte - startbyte + 1);
                                State = ReceiveState.EOF_FOUND;
                                //Запуск разбора кадра Frame, после этого перевод в состояние FREE И очистка контейнера кадра

                                //Запись найденного кадра в список заданий
                                TaskReceived_mutex.WaitOne();
                                TasksReceived.Add(new One_Task("Port2", Frame.ToArray()));
                                TaskReceived_mutex.ReleaseMutex();
                                Frame.Clear();

                                State = ReceiveState.FREE;
                            }
                        }
                    }
                    //Если Начало кадра не найдено и при этом система готова к приему нового кадра,
                    // значит в порт пришел мусор, его удаляем из буфера кадров
                    else
                    {
                        ReceivedFrames2.Clear();
                    }

                }
                //Если уже был обнаружен стартовый байт, значит принимаем всё до конца буфера или до конечного байта 
                //Заход в эту область происходит, если разиер кадра оказался больше, чем буфер приема
                else if (State == ReceiveState.SOF_FOUND)
                {
                    stopbyte = ReceivedFrames2.IndexOf(0xFE);
                    //Если не найден стоповый байт, достаём из буфера кадров всё, что там есть 
                    if (stopbyte == -1)
                    {
                        startbyte = ReceivedFrames2.IndexOf(0xFF);
                        if ((startbyte != -1) && State == ReceiveState.SOF_FOUND)
                        {
                            MessageBox.Show("Функция FindFrame2(), найден кадр без стопового байта", "Error!");
                            ReceivedFrames_mutex2.ReleaseMutex();
                            Frame.Clear();
                            State = ReceiveState.FREE;
                            continue;
                        }
                        Frame.AddRange(ReceivedFrames2.GetRange(0, ReceivedFrames2.Count));
                        ReceivedFrames2.RemoveRange(0, ReceivedFrames2.Count);

                    }
                    //Если стоповый байт найден, тогда вырезаем всё, что расположено до стопового байта, далее разбор кадра
                    else if (stopbyte != -1)
                    {
                        startbyte = ReceivedFrames2.IndexOf(0xFF);
                        if ((startbyte != -1) && State == ReceiveState.SOF_FOUND)
                        {
                            MessageBox.Show("Функция FindFrame2(), найден кадр без стопового байта", "Error!");
                            ReceivedFrames_mutex2.ReleaseMutex();
                            Frame.Clear();
                            State = ReceiveState.FREE;
                            continue;
                        }
                        stopbyte = ReceivedFrames2.IndexOf(0xFE);
                        //Добавляем в контейнер кадра до стопового байта, остальное переносим в начало буфера
                        Frame.AddRange(ReceivedFrames2.GetRange(0, stopbyte + 1));
                        ReceivedFrames2.RemoveRange(0, stopbyte + 1);
                        State = ReceiveState.EOF_FOUND;
                        //Запуск разбора кадра Frame, после этого перевод в состояние FREE И очистка контейнера кадра

                        //Запись найденного кадра в список заданий
                        TaskReceived_mutex.WaitOne();
                        TasksReceived.Add(new One_Task("Port2", Frame.ToArray()));
                        TaskReceived_mutex.ReleaseMutex();
                        Frame.Clear();
                        State = ReceiveState.FREE;
                    }
                }
                // Переход в эту область не должен происходить
                else if (State == ReceiveState.EOF_FOUND)
                {
                    MessageBox.Show(" Функция FindFrame2.\r\n Состояние осталось EOF_FOUND в начале прохода цикла", "Error!");
                    Frame.Clear();
                    State = ReceiveState.FREE;
                    continue;
                }
                //Выход из критической секции 
                ReceivedFrames_mutex2.ReleaseMutex();
                Thread.Sleep(10);
            }
        }
        /*^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
             Получение кадров из ком портов и внесение их в список заданий
         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/

        /***************************************************************************
                             ВЫПОЛНЕНИЕ ЗАДАНИЙ НА ПРИЕМ 
         ***************************************************************************/

        // Новые задания появляются в списке TasksReceived из функции FindFrame1 и FindFrame2
        public void TaskHandler()
        {
            bool task_received = false;
            while (true)
            {
                task_received = false;
                One_Task task = new One_Task();
                //Получение доступа к списку заданий
                TaskReceived_mutex.WaitOne();
                if (TasksReceived.Count != 0)
                {
                    task = TasksReceived[0];
                    //Удаление кадра и списка заданий
                    TasksReceived.RemoveAt(0);
                    TaskReceived_mutex.ReleaseMutex();
                    task_received = true;
                }
                else
                { TaskReceived_mutex.ReleaseMutex(); }

                if (task_received == true)
                {
                    byte[] frame = task.Frame;
                    // не Number, а Name
                    // имена = {"Port1","Port2"} 
                    string PortNumber = task.PortNum;

                    DefaultFrame ReceivedFrameStruct = ParseReceivedFrame(frame);

                    if (ReceivedFrameStruct.ResultOfParsing == "OK")
                    {
                        ReceivedFrameStruct.PortName = PortNumber;
                        byte frametype = ReceivedFrameStruct.Frametype;

                        // ОПРЕДЕЛЕНИЕ НЕОБХОДИМОГО ОБРАБОТЧИКА
                        if (frametype == (byte)FrameType.ACK || frametype == (byte)FrameType.RET)
                        {
                            if (frametype == (byte)FrameType.ACK)
                                FrameReceivedAck(ReceivedFrameStruct);
                            else
                                FrameReceivedRet(ReceivedFrameStruct);
                        }
                        if (frametype == (byte)FrameType.MEETING || frametype == (byte)FrameType.DISCONNECT)
                        {
                            if (DEBUG_MODE) MessageBox.Show("Получен кадр типа meeting(не используется)", "Error!");

                        }
                        if (frametype == (byte)FrameType.LOGIN || frametype == (byte)FrameType.LOGOUT)
                        {
                            if (frametype == (byte)FrameType.LOGIN)
                                FrameReceivedLogin(ReceivedFrameStruct);
                            else
                                FrameReceivedLogout(ReceivedFrameStruct);
                        }
                        if (frametype == (byte)FrameType.OPENLETTER || frametype == (byte)FrameType.INFORMATION)
                        {
                            if (frametype == (byte)FrameType.OPENLETTER)
                                FrameReceivedOpenLetter(ReceivedFrameStruct);
                            else
                                FrameReceivedInformation(ReceivedFrameStruct);
                        }
                    }
                    //Если принятый кадр оказался битым, то отправка 
                    //отправка RET кадра в PortName или машине OriginPort
                    else {
                        if (RET_ALLOWED)
                        {
                            TaskToSend_mutex.WaitOne();
                            TasksToSend.Add(new One_Task(
                                PortNumber, new byte[] { 0xFF, (byte)FrameType.RET, 0x00, 0x00, 0xFE }));
                            TaskToSend_mutex.ReleaseMutex();
                        }
                    }
                }
                Thread.Sleep(20);
            }
        }

        /*******************************************************
                       ОБРАБОТЧИКИ СОБЫТИЙ
        ********************************************************/

        public void FrameReceivedLogin(DefaultFrame ReceivedFrame)
        {
            // Отправка ACK кадра в PortName
            TaskToSend_mutex.WaitOne();
            TasksToSend.Add(
                new One_Task(
                    ReceivedFrame.PortName, CreateNewFrame(FrameType.ACK, "0", null, "0", null, false)
                    ));

            TaskToSend_mutex.ReleaseMutex();
            if (ReceivedFrame.PortName == "Port1")
            {
                AuthData_mutex.WaitOne();
                AuthData["Port1"] = ReceivedFrame.MessageData;
                AuthData_mutex.ReleaseMutex();
            }

            if (ReceivedFrame.PortName == "Port2")
            {
                AuthData_mutex.WaitOne();
                AuthData["Port2"] = ReceivedFrame.MessageData;
                AuthData_mutex.ReleaseMutex();

            }

            string port1_auth_data, port2_auth_data;
            AuthData_mutex.WaitOne();

            port1_auth_data = AuthData["Port1"];
            port2_auth_data = AuthData["Port2"];
            AuthData_mutex.ReleaseMutex();

            //Если все login'ы были получены
            if (port1_auth_data != null && port2_auth_data != null)
            {
                Auth_status_mutex.WaitOne();
                Auth_status["ACK_local"] = "Received";
                Auth_status_mutex.ReleaseMutex();
                BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "Логины всех абонентов получены\r\n" });
            }

        }

        // Готово
        public void FrameReceivedAck(DefaultFrame ReceivedFrameStruct)
        {
            string status_1_auth;
            string status_2_auth;
            // Получение состояния авторизации
            Auth_status_mutex.WaitOne();
            status_1_auth = Auth_status["ACK1"];
            status_2_auth = Auth_status["ACK2"];
            Auth_status_mutex.ReleaseMutex();
            if (status_1_auth == "undef" || status_2_auth == "undef")
            {
                if (ReceivedFrameStruct.PortName == "Port1")
                {
                    Ack1_mutex_Auth.WaitOne();
                    Ack1_awaited_Auth = 0;
                    Ack1_mutex_Auth.ReleaseMutex();
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[]
                    { "Получен отчет о приеме Login из Port 1 \r\n" });

                    Auth_status_mutex.WaitOne();
                    Auth_status["ACK1"] = "Received";
                    Auth_status_mutex.ReleaseMutex();

                }
                if (ReceivedFrameStruct.PortName == "Port2")
                {
                    Ack2_mutex_Auth.WaitOne();
                    Ack2_awaited_Auth = 0;
                    Ack2_mutex_Auth.ReleaseMutex();
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[]
                    { "Получен отчет о приеме Login из Port 2 \r\n" });
                    Auth_status_mutex.WaitOne();
                    Auth_status["ACK2"] = "Received";
                    Auth_status_mutex.ReleaseMutex();
                }
            }
            bool auth_stat = false;
            Auth_status_mutex.WaitOne();

            if (Auth_status["ACK1"] != "undef" && Auth_status["ACK_local"] != "undef" && Auth_status["ACK2"] != "undef")
            {
                auth_stat = true;
            }

            Auth_status_mutex.ReleaseMutex();
            //Если авторизован, тогда приходящие ACK подтверждают прием сообщения infoframe
            if (auth_stat)
            {
                if (ReceivedFrameStruct.PortName == "Port1")
                {
                    LastFrame_ToSend_mutex.WaitOne();

                    LastFrameSenttoPort1.PortNum = null;
                    LastFrame_ToSend_mutex.ReleaseMutex();
                }
                if (ReceivedFrameStruct.PortName == "Port2")
                {
                    LastFrame_ToSend_mutex.WaitOne();

                    LastFrameSenttoPort2.PortNum = null;
                    LastFrame_ToSend_mutex.ReleaseMutex();
                }


            }


            // Отметка отправленного сообщения как доставленного 
            // Далее, открыть возможность оправлять новые сообщения

        }

        public void FrameReceivedRet(DefaultFrame frame)
        {
            // Здесь получаем значение last send frame о отправляем его вкомпорт
            LastFrame_ToSend_mutex.WaitOne();
            One_Task LastFrameSent_local1 = LastFrameSenttoPort1;
            One_Task LastFrameSent_local2 = LastFrameSenttoPort2;

            LastFrame_ToSend_mutex.ReleaseMutex();

            TaskToSend_mutex.WaitOne();

            if (frame.PortName == "Port1")
            {
                if (LastFrameSent_local1.PortNum != null && LastFrameSent_local1.Frame != null)
                    TasksToSend.Add(LastFrameSent_local1);
            }
            if (frame.PortName == "Port2")
            {
                if (LastFrameSent_local1.PortNum != null && LastFrameSent_local1.Frame != null)

                    TasksToSend.Add(LastFrameSent_local2);
            }
            TaskToSend_mutex.ReleaseMutex();


        }

        public void FrameReceivedLogout(DefaultFrame frame)
        {
            AuthData_mutex.WaitOne();
            AuthData["Port1"] = null;
            AuthData["Port2"] = null;
            AuthData["local"] = null;

            AuthData_mutex.ReleaseMutex();
            Auth_status_mutex.WaitOne();
            Auth_status["ACK_local"] = "undef";
            Auth_status["ACK1"] = "undef";
            Auth_status["ACK2"] = "undef";
            Auth_status_mutex.ReleaseMutex();

            BeginInvoke(new Set_ButtonState(Set_AuthConnectButton), new object[] { true });
            BeginInvoke(new Set_ButtonState(Set_AuthDisconnectButton), new object[] { false });

            BeginInvoke(new SetTextDeleg(addtotextbox1),
                new object[] { "Получен кадр о деавторизации, логины сброшены" });

        }
        // Обработчик открытия письма (готов)
        // В БД ищется письмо, затем отмечается как прочитанное
        public void FrameReceivedOpenLetter(DefaultFrame frame)
        {
            //Подключение к бд, поиск письма с тем id, который указан в принятом пакете
            //После этого обновление таблицы принятых сообщений на ui (установкой Outbox_update_needed= true)   
            long LetterId = long.Parse(frame.MessageData);
            using (CourseDB db = new CourseDB())
            {
                var result = db.outbox.SingleOrDefault(x => x.id == LetterId);
                if (result != null)
                {
                    if (result.status == "Прочитано")
                    {
                        BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { $" Сообщение : письмо с id = {LetterId} уже было прочитано\r\n" });
                    }
                    else {
                        BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { $" Сообщение : письмо с id = {LetterId} было прочитано\r\n" });
                        result.status = "Прочитано";
                        db.SaveChanges();
                        //Отметка для обновления формы отправленных сообщений
                        Outbox_update_mutex.WaitOne();
                        Outbox_update_needed = true;
                        Outbox_update_mutex.ReleaseMutex();
                    }
                }
            }
        }
        //Готов
        public void FrameReceivedInformation(DefaultFrame local_frame)
        {
            byte[] framedata = WIN1251.GetBytes(local_frame.MessageData);
            string framestr = WIN1251.GetString(framedata);
            inbox_class message_data = JsonConvert.DeserializeObject<inbox_class>(local_frame.MessageData);
            if (local_frame.ResultOfParsing == "OK")
            {


                long foreign_id = long.Parse(message_data.id);
                inbox received_letter = new inbox();
                received_letter.foreign_id = foreign_id;
                received_letter.re = message_data.re;
                received_letter.msg = message_data.msg;
                received_letter.recepient = message_data.recepient;
                received_letter.sender = message_data.sender;
                received_letter.status = "Принято";
                using (CourseDB db = new CourseDB())
                {
                    bool letter_already_exists = false;
                    try
                    {
                        inbox letter = db.inbox.FirstOrDefault(x => x.foreign_id == received_letter.foreign_id);
                        if (letter.foreign_id == received_letter.foreign_id)
                        { letter_already_exists = true; }
                    }
                    catch
                    { letter_already_exists = false; }
                    if (!letter_already_exists)
                    {
                        BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { $"Получено письмо от {message_data.sender}" + "\r\n" });
                        db.inbox.Add(received_letter);
                        db.SaveChanges();
                    }
                    db.Dispose();
                }

                Inbox_update_mutex.WaitOne();
                Inbox_update_needed = true;
                Inbox_update_mutex.ReleaseMutex();

                TaskToSend_mutex.WaitOne();
                TasksToSend.Add(new One_Task(local_frame.PortName,
                                CreateNewFrame(FrameType.ACK, "0", null, "0", null, false))
                                );
                TaskToSend_mutex.ReleaseMutex();
            }
            else
            {
                if (RET_ALLOWED)
                {
                    // Если сообщение кривое, то отправляем RET в порт из которого он был принят
                    TaskToSend_mutex.WaitOne();
                    TasksToSend.Add(new One_Task(local_frame.PortName,
                        CreateNewFrame(FrameType.RET, "0", null, "0", null, false)
                        ));
                    TaskToSend_mutex.ReleaseMutex();
                }
            }
        }

        /* 
        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
         функции для исполнения заданий, полученных из списка с заданиями
        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        
        **********************************************************
                          ОТПРАВКА НОВОГО ПИСЬМА 
        **********************************************************
        */

        //Источник особой уличной магии
        private volatile Type _dependency;
        public void MyClass()
        { _dependency = typeof(System.Data.Entity.SqlServer.SqlProviderServices); }

        public void Wait_for_info_ack1()
        {
            int counter = 0;
            while (true)
            {
                One_Task frame_acked1;
                LastFrame_ToSend_mutex.WaitOne();
                frame_acked1 = LastFrameSenttoPort1;
                LastFrame_ToSend_mutex.ReleaseMutex();

                bool frame_is_to_resend = true;
                if (frame_acked1.PortNum == null)
                { frame_is_to_resend = false; }

                if (!frame_is_to_resend)
                {
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[]
                                            { "Сообщение доставлено"});

                    using (CourseDB db = new CourseDB())
                    {
                        DefaultFrame a = ParseReceivedFrame(LastFrameSenttoPort1.Frame);
                        if (a.ResultOfParsing != "OK") { return; }
                        string id_string = JsonConvert.DeserializeObject<outbox_class>(a.MessageData).id;
                        long id_val = long.Parse(id_string);
                        var last_letter = db.outbox.FirstOrDefault<outbox>(x => x.id == id_val);
                        last_letter.status = "Доставлено";
                        db.SaveChanges();
                    }
                    Outbox_update_mutex.WaitOne();
                    Outbox_update_needed = true;
                    Outbox_update_mutex.ReleaseMutex();
                    return;
                }
                if (frame_is_to_resend)
                {
                    TaskToSend_mutex.WaitOne();
                    TasksToSend.Add(frame_acked1);
                    TaskToSend_mutex.ReleaseMutex();
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[]
                          {$"Отправка (попытка:{counter + 1}, порт: {frame_acked1.PortNum})"  + "\r\n"});
                    counter++;
                }

                if (counter > 10 && frame_is_to_resend)
                {
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[]
                                    { "Сообщение не было доставлено" + "\r\n"});
                    using (CourseDB db = new CourseDB())
                    {
                        DefaultFrame a = ParseReceivedFrame(LastFrameSenttoPort1.Frame);
                        string id_string = JsonConvert.DeserializeObject<outbox_class>(a.MessageData).id;
                        long id_val = long.Parse(id_string);
                        var last_letter = db.outbox.FirstOrDefault<outbox>(x => x.id == id_val);
                        last_letter.status = "Не lоставлено";
                        db.SaveChanges();
                    }
                    Outbox_update_mutex.WaitOne();
                    Outbox_update_needed = true;
                    Outbox_update_mutex.ReleaseMutex();
                    return;
                }
                Thread.Sleep(4000);
            }
        }

        public void Wait_for_info_ack2()
        {
            int counter = 0;
            while (true)
            {
                One_Task frame_acked2;
                LastFrame_ToSend_mutex.WaitOne();
                frame_acked2 = LastFrameSenttoPort2;
                LastFrame_ToSend_mutex.ReleaseMutex();

                bool frame_is_to_resend = true;

                if (frame_acked2.PortNum == null)
                { frame_is_to_resend = false; }

                if (!frame_is_to_resend)
                {
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[]
                                            { "Сообщение доставлено"});

                    using (CourseDB db = new CourseDB())
                    {
                        DefaultFrame a = ParseReceivedFrame(LastFrameSenttoPort2.Frame);
                        string id_string = JsonConvert.DeserializeObject<outbox_class>(a.MessageData).id;
                        long id_val = long.Parse(id_string);
                        var last_letter = db.outbox.FirstOrDefault<outbox>(x => x.id == id_val);
                        last_letter.status = "Доставлено";
                        db.SaveChanges();
                    }
                    Outbox_update_mutex.WaitOne();
                    Outbox_update_needed = true;
                    Outbox_update_mutex.ReleaseMutex();
                    return;
                }
                if (frame_is_to_resend)
                {
                    TaskToSend_mutex.WaitOne();
                    TasksToSend.Add(frame_acked2);
                    TaskToSend_mutex.ReleaseMutex();
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[]
                                           {$"сообщение(попытка:{counter}, порт:{frame_acked2.PortNum}"});
                    counter++;
                }

                if (counter > 10 && frame_is_to_resend)
                {
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[]
                                            { "Сообщение  не было доставлено"});
                    using (CourseDB db = new CourseDB())

                    {
                        DefaultFrame a = ParseReceivedFrame(LastFrameSenttoPort2.Frame);
                        string id_string = JsonConvert.DeserializeObject<outbox_class>(a.MessageData).id;
                        long id_val = long.Parse(id_string);
                        var last_letter = db.outbox.FirstOrDefault<outbox>(x => x.id == id_val);
                        last_letter.status = "Не Доставлено";
                        db.SaveChanges();
                    }
                    Outbox_update_mutex.WaitOne();
                    Outbox_update_needed = true;
                    Outbox_update_mutex.ReleaseMutex();
                    return;
                }
                Thread.Sleep(4000);
            }
        }

        public void SendNewLetterButton_Click(object sender, EventArgs e)
        {
            string Re_string = ReTextbox.Text;
            string Receiver_name = ReceiverComboBox.SelectedItem.ToString();
            string Letter_Message = LetterTextBox.Text;

            Phys_status1_mutex.WaitOne();
            Connection_Status p1 = Phys_status1;
            Phys_status1_mutex.ReleaseMutex();

            Phys_status2_mutex.WaitOne();
            Connection_Status p2 = Phys_status2;
            Phys_status2_mutex.ReleaseMutex();

            if (p1 != Connection_Status.CONNECTED || p2 != Connection_Status.CONNECTED)
            {
                BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "физическое соединение не установлено\r\n" });
                return;
            }

            if (AuthData["Port1"] != null && AuthData["Port2"] != null && AuthData["local"] == null)
            {
                BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "Логины не определены\r\n" });
                if (DEBUG_MODE)
                {
                    MessageBox.Show("SendNewLetterButton_Click(): логическое соединение не установлено", "Error!");
                }
                return;
            }
            using (CourseDB db = new CourseDB())
            {
                outbox letter = new outbox();
                letter.re = Re_string;
                letter.sender = AuthData["local"];
                letter.recepient = Receiver_name;
                letter.status = "Отправлено";
                letter.msg = Letter_Message;
                db.outbox.Add(letter);
                db.SaveChanges();
            }
            long max;
            outbox a;
            using (CourseDB db = new CourseDB())
            {
                max = db.outbox.Max(x => x.id);
                a = db.outbox.FirstOrDefault(x => x.id == max);
            }
            string letter_local_id = a.id.ToString();
            string receiver_port = AuthData.FirstOrDefault(x => x.Value == Receiver_name).Key;
            outbox_class letter_payload_obj = new outbox_class(a);
            string letter_payload_string = JsonConvert.SerializeObject(letter_payload_obj);
            string letter_len = letter_payload_string.Length.ToString();
            byte[] Letter_frame_to_send = CreateNewFrame(
                FrameType.INFORMATION, "0", letter_len, "0", letter_payload_string, false);

            if (receiver_port == "Port1")
            {
                LastFrame_ToSend_mutex.WaitOne();
                LastFrameSenttoPort1 = new One_Task(receiver_port, Letter_frame_to_send);
                LastFrame_ToSend_mutex.ReleaseMutex();
                Thread Wait_for_info_ack1thr = new Thread(Wait_for_info_ack1);
                Wait_for_info_ack1thr.IsBackground = true;
                Wait_for_info_ack1thr.Start();
            }

            if (receiver_port == "Port2")
            {
                LastFrame_ToSend_mutex.WaitOne();
                LastFrameSenttoPort2 = new One_Task(receiver_port, Letter_frame_to_send);
                LastFrame_ToSend_mutex.ReleaseMutex();
                Thread Wait_for_info_ack2thr = new Thread(Wait_for_info_ack2);
                Wait_for_info_ack2thr.IsBackground = true;
                Wait_for_info_ack2thr.Start();
            }

            TaskToSend_mutex.WaitOne();
            TasksToSend.Add(new One_Task(receiver_port, Letter_frame_to_send));
            TaskToSend_mutex.ReleaseMutex();

            Outbox_update_mutex.WaitOne();
            Outbox_update_needed = true;
            Outbox_update_mutex.ReleaseMutex();

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        // Делегат используется для записи в UI control из потока не-UI
        private delegate void SetTextDeleg(string text);
        private delegate void FillComboBoxDeleg();

        //Заполняет список авторизованных пользователей
        public void FillReceiverComboBox()
        {
            ReceiverComboBox.Items.Clear();
            AuthData_mutex.WaitOne();
            string[] values = new string[] { AuthData["Port1"], AuthData["Port2"] };
            // string[] values = new string[] { AuthData["Port1"] };
            ReceiverComboBox.Items.AddRange(values);
            AuthData_mutex.ReleaseMutex();
        }

        public void settextbox1(string text)
        { textBox1.Text = text; }

        public void addtotextbox1(string text)
        {
            textBox1.AppendText(text);
        }

        public void OpenSerial1()
        {
            //Установка параметров компорта1
            SelectedPort1Name_mutex.WaitOne();
            try
            { serialPort1.PortName = SelectedPort1Name; }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, serialPort1.PortName); }
            SelectedPort1Name_mutex.ReleaseMutex();

            SelectedBaudrate_mutex.WaitOne();
            serialPort1.BaudRate = int.Parse(SelectedBaudrate);
            SelectedBaudrate_mutex.ReleaseMutex();
            try
            {
                serialPort1.Open();
                serialPort1.DiscardOutBuffer();
                serialPort1.DiscardInBuffer();
                serialPort1.DtrEnable = true;
                BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "Порт1 был открыт\r\n" });
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, serialPort1.PortName);
                return;
            }
        }

        public void OpenSerial2()
        {
            //Установка параметров компорта2
            SelectedPort2Name_mutex.WaitOne();
            try
            { serialPort2.PortName = SelectedPort2Name; }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, serialPort1.PortName); };
            SelectedPort2Name_mutex.ReleaseMutex();
            // serialPort2.PortName = "COM6";
            SelectedBaudrate_mutex.WaitOne();
            serialPort2.BaudRate = int.Parse(SelectedBaudrate);
            SelectedBaudrate_mutex.ReleaseMutex();
            try
            {
                serialPort2.Open();
                serialPort2.DiscardOutBuffer();
                serialPort2.DiscardInBuffer();
                serialPort2.DtrEnable = true;
                BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "Порт2 был открыт\r\n" });
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, serialPort2.PortName);
                return;
            }
        }

        /************************************************************
                       ПРОВЕРКА СОСТОЯНИЯ ПОРТОВ
        ************************************************************/

        // Делегат используется для записи в UI control из потока не-UI
        private delegate void SetPortState(string text);
        public void setport1state(string text)
        { port1state_label.Text = text; }

        public void setport2state(string text)
        { port2state_label.Text = text; }

        // Мониторит физическое состояние портов
        public void serial1_monitor()
        {
            while (true)
            {
                if (serialPort1.IsOpen)
                {
                    try
                    {
                        if (serialPort1.CDHolding || serialPort1.DsrHolding)
                        {
                            BeginInvoke(new SetTextDeleg(setport1state), new object[] { "Подключен" });
                            Phys_status1_mutex.WaitOne();
                            Phys_status1 = Connection_Status.CONNECTED;
                            Phys_status1_mutex.ReleaseMutex();
                        }
                        else {
                            BeginInvoke(new SetTextDeleg(setport1state), new object[] { "Отключен" });
                            Phys_status1_mutex.WaitOne();
                            Phys_status1 = Connection_Status.CONNECTION_WAIT;
                            Phys_status1_mutex.ReleaseMutex();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Data.ToString(), "Error!");
                    }
                }
                else
                {
                    BeginInvoke(new SetTextDeleg(setport1state), new object[] { "Порт1 закрыт" });
                    Phys_status1_mutex.WaitOne();
                    Phys_status1 = Connection_Status.DISCONNECTED;
                    Phys_status1_mutex.ReleaseMutex();
                }
                Thread.Sleep(200);
            }
        }
        public void serial2_monitor()
        {
            while (true)
            {
                if (serialPort2.IsOpen)
                {
                    try
                    {
                        if (serialPort2.CDHolding || serialPort2.DsrHolding)
                        {
                            BeginInvoke(new SetTextDeleg(setport2state), new object[] { "Подключен" });
                            Phys_status2_mutex.WaitOne();
                            Phys_status2 = Connection_Status.CONNECTED;
                            Phys_status2_mutex.ReleaseMutex();
                        }
                        else {
                            BeginInvoke(new SetTextDeleg(setport2state), new object[] { "Отключен" });
                            Phys_status2_mutex.WaitOne();
                            Phys_status2 = Connection_Status.CONNECTION_WAIT;
                            Phys_status2_mutex.ReleaseMutex();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Data.ToString(), "Error!");
                    }
                }
                else
                {
                    BeginInvoke(new SetTextDeleg(setport2state), new object[] { "Порт2 закрыт" });
                    Phys_status2_mutex.WaitOne();
                    Phys_status2 = Connection_Status.DISCONNECTED;
                    Phys_status2_mutex.ReleaseMutex();
                }


                Thread.Sleep(200);
            }

        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        /***********************************************************
        ************************************************************
                         ЗДЕСЬ НАЧАЛО ВСЕГО
        ************************************************************/

        private void button1_Click(object sender, EventArgs e)
        {
            // Открытие портов
            Thread SerOpenthread1 = new Thread(OpenSerial1);
            Thread SerOpenthread2 = new Thread(OpenSerial2);
            SerOpenthread1.IsBackground = true;
            SerOpenthread1.Start();
            SerOpenthread2.IsBackground = true;
            SerOpenthread2.Start();
            Thread.Sleep(300);

            if (!serialPort1.IsOpen || !serialPort2.IsOpen)
            {
                if (DEBUG_MODE) MessageBox.Show("Не удалось открыть порты", "Warning");

                if (serialPort1.IsOpen) serialPort1.Close();
                if (serialPort2.IsOpen) serialPort2.Close();
                BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "Не удалось открыть порты\r\n" });
                return;
            }
            this.button1.Enabled = false;
            this.button4.Enabled = true;


            // После запуска потока, метод StartReceiving осуществляет прием из входного буфера в программный буфер принятых байтов
            Thread ReceiverThr1 = new Thread(Serial1_StartReceiving);
            ReceiverThr1.IsBackground = true;
            ReceiverThr1.Start();

            // FindFrame просматривает програмный буфер принятых байтов и составляет кадры, затем помещает  в список кадров (заданий)
            Thread ParseFrameThr1 = new Thread(FindFrameInPort1);
            ParseFrameThr1.IsBackground = true;
            ParseFrameThr1.Start();

            // После запуска потока, метод StartReceiving осуществляет прием из входного буфера в программный буфер принятых байтов
            Thread ReceiverThr2 = new Thread(Serial2_StartReceiving);
            ReceiverThr2.IsBackground = true;
            ReceiverThr2.Start();

            // FindFrame просматривает програмный буфер принятых байтов и составляет кадры, зате помещает  в список кадров (заданий)
            Thread ParseFrameThr2 = new Thread(FindFrameInPort2);
            ParseFrameThr2.IsBackground = true;
            ParseFrameThr2.Start();

            // Потоки нужны для определения состояния абонентов на физическом уровне
            // Состояние пишут в UI и в Phys_status 1|2
            Thread serial1_mon_thr = new Thread(serial1_monitor);
            serial1_mon_thr.IsBackground = true;
            serial1_mon_thr.Start();

            Thread serial2_mon_thr = new Thread(serial2_monitor);
            serial2_mon_thr.IsBackground = true;
            serial2_mon_thr.Start();

            // Обработка заданий, принятых из портов
            Thread TaskHandlerThr = new Thread(TaskHandler);
            TaskHandlerThr.IsBackground = true;
            TaskHandlerThr.Start();

            // Обработка заданий на отправку
            Thread TaskToSendHandlerThr = new Thread(TaskToSendHandler);
            TaskToSendHandlerThr.IsBackground = true;
            TaskToSendHandlerThr.Start();

        }
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        // При закрытии формы порты тоже закрываются
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            while (serialPort1.IsOpen || serialPort2.IsOpen)
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                }

                if (serialPort2.IsOpen)
                {
                    serialPort2.Close();
                }

            }
        }

        /***************************************************************************
                            ВЫПОЛНЕНИЕ ЗАДАНИЙ НА ОТПРАВКУ 
        ***************************************************************************/
        public void TaskToSendHandler()
        {
            while (true)
            {
                // Получение задания из очереди на отправку кадра в порт
                TaskToSend_mutex.WaitOne();
                if (TasksToSend.Count != 0)
                {
                    One_Task TaskToSend = TasksToSend[0];
                    TasksToSend.RemoveAt(0);
                    TaskToSend_mutex.ReleaseMutex();

                    string PortName = TaskToSend.PortNum;
                    byte[] frametosend = TaskToSend.Frame;

                    if (PortName == "Port1")
                    {
                        try
                        { serialPort1.Write(WIN1251.GetString(frametosend)); }
                        catch (Exception ex)
                        { MessageBox.Show(ex.ToString(), "Error!"); }
                    }
                    else if (PortName == "Port2")
                    {
                        try
                        { serialPort2.Write(WIN1251.GetString(frametosend)); }
                        catch (Exception ex)
                        { MessageBox.Show(ex.ToString(), "Error!"); }
                    }
                    else { MessageBox.Show("TaskToSendHandler() Нет такого порта", "Error!"); }
                }
                else {
                    TaskToSend_mutex.ReleaseMutex();
                }

                Thread.Sleep(20);
            }
        }
        /*
         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^*/
        // По кнопке начинает отсылать логины
        public void Establish_Logical()
        {
            // Получение локальногго логина
            AuthData_mutex.WaitOne();
            string local_auth_name = AuthData["local"];
            AuthData_mutex.ReleaseMutex();

            Ack1_mutex_Auth.WaitOne();
            Ack1_awaited_Auth = 0;
            Ack1_mutex_Auth.ReleaseMutex();

            Ack2_mutex_Auth.WaitOne();
            Ack2_awaited_Auth = 0;
            Ack2_mutex_Auth.ReleaseMutex();

            int ack1;
            int ack2;
            int counter = 0;
            while (true)
            {
                Auth_status_mutex.WaitOne();
                string s1, s2;
                s1 = Auth_status["ACK1"];
                s2 = Auth_status["ACK2"];
                Auth_status_mutex.ReleaseMutex();

                // Если ACK1 или ACK2 были получены то прерываем попытки отправлять логины
                if (s1 != "undef" && s2 != "undef")
                {
                    BeginInvoke(new SetTextDeleg(addtotextbox1),
                        new object[] { "Локальный логин был доставлен \r\n" });
                    Thread.Sleep(100);
                    return;
                }

                // Начальная попытка отправить LOGIN
                if (counter == 0)
                {
                    Ack1_mutex_Auth.WaitOne();
                    ack1 = Ack1_awaited_Auth;
                    Ack1_mutex_Auth.ReleaseMutex();

                    Ack2_mutex_Auth.WaitOne();
                    ack2 = Ack2_awaited_Auth;
                    Ack2_mutex_Auth.ReleaseMutex();

                    // Если это первая попытка отправки, то в порты отсылаются логины
                    if (ack1 == 0 && ack2 == 0)
                    {
                        TaskToSend_mutex.WaitOne();
                        byte[] frame1 = CreateNewFrame(FrameType.LOGIN, "0",
                            (local_auth_name.Length).ToString(), "0", local_auth_name);
                        TasksToSend.Add(new One_Task("Port1", frame1));

                        // Установка флага, что ожидается ack1
                        Ack1_mutex_Auth.WaitOne();
                        Ack1_awaited_Auth = 1;
                        Ack1_mutex_Auth.ReleaseMutex();

                        TaskToSend_mutex.ReleaseMutex();

                        counter++;
                        BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] {
                            " Попыток логин соединения (пк на порт1) : " + counter.ToString() + "\r\n" });


                        TaskToSend_mutex.WaitOne();
                        byte[] frame2 = CreateNewFrame(FrameType.LOGIN, "0",
                            (local_auth_name.Length).ToString(), "0", local_auth_name);
                        TasksToSend.Add(new One_Task("Port2", frame2));

                        //Установка флага, что ожидается ack2
                        Ack2_mutex_Auth.WaitOne();
                        Ack2_awaited_Auth = 1;
                        Ack2_mutex_Auth.ReleaseMutex();

                        TaskToSend_mutex.ReleaseMutex();
                        counter++;
                        BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { " Попыток логин соединения (пк на порт2) : " + counter.ToString() + "\r\n" });
                        Thread.Sleep(4000);
                        continue;
                    }
                }

                // Повторные попытки отправить логин
                if (counter < 10 && counter != 0)
                {
                    // Получение текущего статуса доставки
                    Ack1_mutex_Auth.WaitOne();
                    ack1 = Ack1_awaited_Auth;
                    Ack1_mutex_Auth.ReleaseMutex();

                    Ack2_mutex_Auth.WaitOne();
                    ack2 = Ack2_awaited_Auth;
                    Ack2_mutex_Auth.ReleaseMutex();

                    // Если ack 1&2 были получены
                    if (ack1 == 0 && ack2 == 0)
                    {
                        Auth_status_mutex.WaitOne();
                        Auth_status["ACK1"] = "Received";
                        Auth_status_mutex.ReleaseMutex();

                        Auth_status_mutex.WaitOne();
                        Auth_status["ACK2"] = "Received";
                        Auth_status_mutex.ReleaseMutex();

                        BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "Переданные логины доставлены  \r\n" });
                        return;
                    }

                    if (ack1 == 1)
                    {
                        TaskToSend_mutex.WaitOne();
                        byte[] frame1 = CreateNewFrame(FrameType.LOGIN, "0", (local_auth_name.Length).ToString(), "0", local_auth_name);
                        TasksToSend.Add(new One_Task("Port1", frame1));

                        Ack1_mutex_Auth.WaitOne();
                        Ack1_awaited_Auth = 1;
                        Ack1_mutex_Auth.ReleaseMutex();

                        TaskToSend_mutex.ReleaseMutex();
                        counter++;
                        BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { " Попыток передать логин (пк на порт1) : " + counter.ToString() + "\r\n" });

                    }

                    if (ack2 == 1)
                    {
                        TaskToSend_mutex.WaitOne();
                        byte[] frame2 = CreateNewFrame(FrameType.LOGIN, "0", (local_auth_name.Length).ToString(), "0", local_auth_name);
                        TasksToSend.Add(new One_Task("Port2", frame2));

                        Ack2_mutex_Auth.WaitOne();
                        Ack2_awaited_Auth = 1;
                        Ack2_mutex_Auth.ReleaseMutex();

                        TaskToSend_mutex.ReleaseMutex();
                        counter++;
                        BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "Попыток передать логин (пк на порт2) : " + counter.ToString() + "\r\n" });

                    }
                }
                else if (counter != 0)
                {
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "Передать логин не удалось. Попыток: " + counter.ToString() + "\r\n" });
                    return;
                }
                Thread.Sleep(4000);
            }
        }

        private delegate void Set_ButtonState(bool text);
        public void Set_AuthDisconnectButton(bool state)
        {
            this.AuthDisconnectButton.Enabled = state;
        }
        public void Set_AuthConnectButton(bool state)
        {
            this.AuthConnectButton.Enabled = state;
        }

        // Поток, следящий за Авторизацией
        // Если пользователи авторизованы в сети, тогда дается уведомление и заполняется список юзверей 
        public void Auth_Tracker()
        {
            bool stat = false;
            while (true)
            {
                Auth_status_mutex.WaitOne();
                if (Auth_status["ACK1"] != "undef" && Auth_status["ACK_local"] != "undef" && Auth_status["ACK2"] != "undef")
                { stat = true; }
                Auth_status_mutex.ReleaseMutex();
                if (stat == true)
                {
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[]{
                        "Авторизация всех пользователей прошла успешно\r\n" });
                    BeginInvoke(new FillComboBoxDeleg(FillReceiverComboBox), new object[] { });

                    BeginInvoke(new Set_ButtonState(Set_AuthConnectButton), new object[] { false });
                    BeginInvoke(new Set_ButtonState(Set_AuthDisconnectButton), new object[] { true });
                    return;
                }
                Thread.Sleep(200);
            }
        }

        // Кнопка попытки авторизации
        private void AuthConnectButton_Click(object sender, EventArgs e)
        {
            // Получение значений статуса физ подключения (берется из serial_monitor) 
            Phys_status1_mutex.WaitOne();
            Connection_Status local_status1 = Phys_status1;
            Phys_status1_mutex.ReleaseMutex();

            Phys_status2_mutex.WaitOne();
            Connection_Status local_status2 = Phys_status2;
            Phys_status2_mutex.ReleaseMutex();

            if (local_status1 != Connection_Status.CONNECTED || local_status2 != Connection_Status.CONNECTED)
            {
                BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "Проверьте физическое соединение \r\n" });
                return;
            }

            // Получение логина
            string local_login = LogintextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(local_login))
            {
                BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "Проверьте введенный логин \r\n" });
                return;
            }
            else
            {
                AuthData_mutex.WaitOne();
                AuthData["local"] = local_login;
                AuthData_mutex.ReleaseMutex();
            }
            // Запуск потока, следящего за ходом авторизации
            Thread Auth_Trackerthr = new Thread(Auth_Tracker);
            Auth_Trackerthr.IsBackground = true;
            Auth_Trackerthr.Start();

            // Попытка отправить свой логин 
            Thread connect_logicalthr = new Thread(Establish_Logical);
            connect_logicalthr.Start();

        }

        // Кнопка деавторизации, Disconnect
        private void AuthDisconnectButton_click(object sender, EventArgs e)
        {
            AuthData_mutex.WaitOne();
            AuthData["local"] = null;
            AuthData["Port1"] = null;
            AuthData["Port2"] = null;
            AuthData_mutex.ReleaseMutex();
            Auth_status_mutex.WaitOne();
            Auth_status["ACK1"] = "undef";
            Auth_status["ACK2"] = "undef";
            Auth_status["ACK_local"] = "undef";
            Auth_status_mutex.ReleaseMutex();

            TaskToSend_mutex.WaitOne();
            TasksToSend.Add(new One_Task("Port1", CreateNewFrame(FrameType.LOGOUT, "0", null, "0", null, false)));
            TasksToSend.Add(new One_Task("Port2", CreateNewFrame(FrameType.LOGOUT, "0", null, "0", null, false)));
            TaskToSend_mutex.ReleaseMutex();

            BeginInvoke(new SetTextDeleg(addtotextbox1), new object[] { "Вы были деавторизованы\r\n" });
            this.AuthDisconnectButton.Enabled = false;
            this.AuthConnectButton.Enabled = true;

        }
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedState = toolStripComboBox1.SelectedItem.ToString();
            SelectedPort1Name_mutex.WaitOne();
            SelectedPort1Name = selectedState;
            SelectedPort1Name_mutex.ReleaseMutex();
        }

        private void toolStripComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedState = toolStripComboBox3.SelectedItem.ToString();
            SelectedBaudrate_mutex.WaitOne();
            SelectedBaudrate = selectedState;
            SelectedBaudrate_mutex.ReleaseMutex();
        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedState = toolStripComboBox2.SelectedItem.ToString();
            SelectedPort2Name_mutex.WaitOne();
            SelectedPort2Name = selectedState;
            SelectedPort2Name_mutex.ReleaseMutex();
        }

        //Закрытие портов
        private void button4_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Handshake = Handshake.None;
                    Thread.Sleep(300);
                    serialPort1.Close();
                    if (DEBUG_MODE) MessageBox.Show("Порт 1 был Закрыт", "Error!");
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[]{
                        "Порт 1 был Закрыт\r\n" });
                    serialPort1.Handshake = Handshake.RequestToSend;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error!");
                }
            }
            if (serialPort2.IsOpen)
            {
                try
                {
                    serialPort2.Handshake = Handshake.None;
                    Thread.Sleep(300);
                    serialPort2.Close();
                    if (DEBUG_MODE) MessageBox.Show("Порт 2 был Закрыт", "Error!");
                    BeginInvoke(new SetTextDeleg(addtotextbox1), new object[]{
                        "Порт 2 был Закрыт\r\n" });
                    serialPort2.Handshake = Handshake.RequestToSend;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error!");
                }
            }
            if (!serialPort1.IsOpen && !serialPort1.IsOpen)
            {
                this.button1.Enabled = true;
                this.button4.Enabled = false;
            }
        }

        /*************************************************************************
                            ОТКРЫТИЕ ПАПОК С ПИСЬМАМИ
        **************************************************************************/
        //Открытие папки входящие
        private void входящиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 inbox_folder = new Form2(this);
            inbox_folder.Show();
        }
        //Инициация обновления формы inbox
        private void button2_Click(object sender, EventArgs e)
        {
            Inbox_update_mutex.WaitOne();
            Inbox_update_needed = true;
            Inbox_update_mutex.ReleaseMutex();
        }
        //Открытие папки исходящие
        private void исходящиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 outbox_folder = new Form3(this);
            outbox_folder.Show();
        }
        //Инициация обновления формы outbox
        private void button5_Click(object sender, EventArgs e)
        {
            Outbox_update_mutex.WaitOne();
            Outbox_update_needed = true;
            Outbox_update_mutex.ReleaseMutex();
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Работу выполнили:\r\n" +
                "    Зубков А.Д.\r\n" +
                "    Меркулова Н.А.\r\n" +
                "    Турусов В.И.", "Справка");
        }
    }
}


