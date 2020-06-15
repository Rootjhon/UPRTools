using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

using ThreadPriority = System.Threading.ThreadPriority;

namespace UPRProfiler
{
#if UNITY_2018_2_OR_NEWER
    using Unity.Collections;

    public sealed class UPRMessage
    {
        public int type;
        public NativeArray<byte> nativeRawBytes;
        public byte[] rawBytes;
        public int width;
        public int height;
    }
#else
    public sealed class UPRMessage
    {
        public int type;
        public byte[] rawBytes;
        public int width;
        public int height;
    }
#endif
    public static class NetworkServer
    {
        private static readonly byte[] MagicTag = { 26, 8, 110, 123, 187, 39, 7, 0 };
        private static Queue<UPRMessage> m_sampleQueue = new Queue<UPRMessage>(256);
        enum DataType { Screenshot, PSS, Device };
        private static NetworkStream ns;
        private static BinaryWriter bw;
        private static BinaryReader br;
        public static bool screenFlag = false;
        public static bool isConnected = false;
        public static bool enableScreenShot = false;
        public static bool sendDeviceInfo = false;
        private static TcpClient m_client = null;
        private static string host = "0.0.0.0";
        private static Thread m_sendThread;
        private static Thread m_receiveThread;
        
        private static int listenPort = 56000;
        private static byte[] dataType = new byte[2];
        private static JPGEncoder jpegEncoder;

        private static Thread listenThead;

        #region public
        public static void ConnectTcpPort(int port)
        {
            listenPort = GetAvailablePort(port, 500, "tcp");
            listenThead = new Thread(new ThreadStart(StartListening));
            listenThead.Start();
        }

        private static void StartListening()
        {
            if (m_client != null) return;

            IPAddress myIP = IPAddress.Parse(host);
            TcpListener tcpListener = new TcpListener(myIP, listenPort);
            tcpListener.Start();

            while (true)
            {
                try
                {
                    m_client = tcpListener.AcceptTcpClient();

                    if (m_client != null)
                    {
                        m_client.Client.SendTimeout = 30000;

                        ns = m_client.GetStream();
                        bw = new BinaryWriter(ns);
                        br = new BinaryReader(ns);

                        if (m_sendThread == null)
                        {
                            m_sendThread = new Thread(new ThreadStart(DoSendMessage));
                            m_sendThread.Priority = ThreadPriority.Highest;
                            m_sendThread.Start();
                        }

                        if (m_receiveThread == null)
                        {
                            m_receiveThread = new Thread(new ThreadStart(DoReceiveMessage));
                            m_receiveThread.Priority = ThreadPriority.Lowest;
                            m_receiveThread.Start();
                        }

                        // break;
                    }

                }
                catch (ThreadAbortException abortEx)
                {
                    Debug.LogWarning(abortEx.Message);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Close();
                }
                Thread.Sleep(1000);
            }
        }
        public static void Close()
        {
            try
            {
                isConnected = false;
                enableScreenShot = false;

                if (m_client != null)
                {
                    if (m_client.Connected)
                    {
                        m_client.Close();
                    }
                    m_client = null;
                }

                lock (m_sampleQueue)
                {
                    m_sampleQueue.Clear();
                }
                
                if (m_sendThread != null)
                {
                    var tmp = m_sendThread;
                    m_sendThread = null;
                    tmp.Abort();
                }

                if (m_receiveThread != null)
                {
                    var tmp = m_receiveThread;
                    m_receiveThread = null;
                    tmp.Abort();
                }

                if (listenThead != null)
                {
                    var tmp = listenThead;
                    listenThead = null;
                    tmp.Abort();
                }

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void SendMessage(UPRMessage sample)
        {
            if (m_client == null)
            {
                return;
            }
            lock (m_sampleQueue)
            {
                m_sampleQueue.Enqueue(sample);
            }
        }

        public static void SendMessage(byte[] rawBytes, int type, int width, int height)
        {
            UPRMessage sample = new UPRMessage
            {
                rawBytes = rawBytes,
                type = type,
                width = width,
                height = height
            };
            if (m_client == null)
            {
                return;
            }
            lock (m_sampleQueue)
            {
                m_sampleQueue.Enqueue(sample);
            }

        }
#if UNITY_2018_2_OR_NEWER
        public static void SendMessage(NativeArray<byte> nativeRawBytes, int type, int width, int height)
        {
            UPRMessage sample = new UPRMessage
            {
                nativeRawBytes = nativeRawBytes,
                type = type,
                width = width,
                height = height
            };
            if (m_client == null)
            {
                return;
            }
            lock (m_sampleQueue)
            {
                m_sampleQueue.Enqueue(sample);
            }

        }
#endif
        #endregion

        private static int GetAvailablePort(int beginPort, int maxIter, string type)
        {
            int availablePort = beginPort;
            for (int port = beginPort; port < beginPort + maxIter; ++port)
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //Port available
                    socket.Bind(ep);
                    socket.Close();
                    availablePort = port;
                    break;
                }
                catch (SocketException)
                {
                    Debug.LogWarningFormat("[NetworkServer] Port not available {0}", port);
                }
            }
            Debug.LogFormat("[NetworkServer] GetAvailablePort:{0}", availablePort);
            return availablePort;
        }

        private static void DoSendMessage()
        {
            while (true)
            {

                try
                {
                    if (m_sendThread == null)
                    {
                        Debug.Log("[NetworkServer] <color=#ff0000>Package m_sendThread null</color>");
                        return;
                    }
                    if (m_sampleQueue.Count > 0)
                    {
                        while (m_sampleQueue.Count > 0)
                        {
                            UPRMessage s = null;
                            lock (m_sampleQueue)
                            {
                                s = m_sampleQueue.Dequeue();
                            }
                            switch (s.type)
                            {
                                case (int)DataType.Screenshot:
#if UNITY_2018_2_OR_NEWER
                                    jpegEncoder.doNativeEncoding(s.nativeRawBytes, s.width, s.height);
#else
                                    jpegEncoder.doEncoding(s.rawBytes, s.width, s.height);
#endif
                                    byte[] image = jpegEncoder.GetBytes();
                                    screenFlag = false;
                                    PackAndSend(image, s.type);
                                    break;
                                case (int)DataType.PSS:
                                    PackAndSend(s.rawBytes, s.type);
                                    break;
                                case (int)DataType.Device:
                                    PackAndSend(s.rawBytes, s.type);
                                    break;
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Close();
                }
            }

        }

        private static void DoReceiveMessage()
        {
            string resultMess;
            while (true)
            {
                try
                {
                    if (m_receiveThread == null)
                    {
                        Debug.Log("[NetworkServer] <color=#ff0000>Package m_receiveThread null</color>");
                        return;
                    }
                    if (ns.CanRead && ns.DataAvailable)
                    {
                        resultMess = ParseMessage(br);
                        if (resultMess.Contains("Start Sending Message"))
                        {
                            InnerPackageS.screenCnt = 0;
                            InnerPackageS.memoryCnt = 0;
                            sendDeviceInfo = false;
                            isConnected = true;
                        }
                        else if (resultMess.Contains("Stop Sending Message") && isConnected)
                        {
                            isConnected = false;
                            Close();
                            break;
                        }
                        else if (resultMess.Contains("Screen"))
                        {
                            string[] sess = resultMess.Split(':');
                            if (sess.Length == 3)
                            {
                                enableScreenShot = Convert.ToBoolean(sess[1]);
                                var tempSnapSec = Convert.ToInt32(sess[2]);
                                InnerPackageS.Frequency = tempSnapSec;
                                jpegEncoder = new JPGEncoder(20);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                Thread.Sleep(1000);
            }
        }

        private static string ParseMessage(BinaryReader binaryReader)
        {
            int position = 0;
            string result = string.Empty;
            while (position < MagicTag.Length)
            {
                byte tmpByte = binaryReader.ReadByte();
                if (tmpByte == MagicTag[position])
                {
                    position++;
                }
                else if (tmpByte == MagicTag[0])
                {
                    position = 1;
                }
                else
                {
                    position = 0;
                }
            }
            try
            {
                int len = br.ReadInt32();
                byte[] data = br.ReadBytes(len);
                result = Encoding.UTF8.GetString(data, 0, len);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return result;
        }

        private static void PackAndSend(byte[] bytes, int type)
        {
            bw.Write(MagicTag);

            dataType[1] = (byte)((type >> 8) & 0xFF);
            dataType[0] = (byte)(type & 0xFF);
            bw.Write(dataType);

            byte[] dataLen = BitConverter.GetBytes(bytes.Length);
            bw.Write(dataLen);
            bw.Write(bytes);
        }
    }
}


