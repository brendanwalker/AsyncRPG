using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Collections;
using System.Text;

/* Ported and extended from as3irclib
 * Copyright the original author or authors.
 * 
 * Licensed under the MOZILLA PUBLIC LICENSE, Version 1.1 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.mozilla.org/MPL/MPL-1.1.html
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
public class IRCSocket 
{
    private const char CARRIAGE_RETURN = '\r';
    private const char LINE_FEED = '\n';
    private const int MAX_LINE_READ_LENGTH = 1024;
    private const int READ_TIMEOUT_MILLISECONDS = 10;
    private const int ITERATION_MAX = 10;

    public delegate void OnSocketOpenDelegate();
    public delegate void OnSocketCloseDelegate();
    public delegate void OnSocketReadDelegate(string data);
    public delegate void OnSocketErrorDelegate(Exception error);

    private TcpClient m_socket;
    private NetworkStream m_stream;
    private StreamWriter m_writer;
    private byte[] m_byteReadBuffer = new byte[1024];
    private StringBuilder m_stringReadBuffer = new StringBuilder();

    public OnSocketOpenDelegate OnSocketOpen { get; set; }
    public OnSocketCloseDelegate OnSocketClose { get; set; }
    public OnSocketReadDelegate OnSocketRead { get; set; }
    public OnSocketErrorDelegate OnSocketError { get; set; }

    public bool SocketReady { get; private set; }

    public IRCSocket()
    {
        SocketReady = false;

        if (Debug.isDebugBuild)
        {
            DebugRegistry.SetToggle("irc.socket.log", false);
        }
    }

    public bool DebugEnabled 
    {
        get {
            return Debug.isDebugBuild ? DebugRegistry.TestToggle("irc.socket.log") : false;
        }
    }

    public bool Connect(
        string host, 
        int port)
    {
        try
        {
            m_socket = new TcpClient(host, port);           
            m_stream = m_socket.GetStream();
            m_stream.ReadTimeout = READ_TIMEOUT_MILLISECONDS; 
            m_writer = new StreamWriter(m_stream);

            SocketReady = true;

            if (OnSocketOpen != null)
            {
                OnSocketOpen();
            }
        }
        catch (Exception e)
        {
            SocketReady = false;
            OnSocketError(e);
        }

        return SocketReady;
    }

    public void Disconnect()
    {
        if (SocketReady)
        {
            try
            {
                m_writer.Close();
                m_socket.Close();
            }
            catch (Exception e)
            {
                OnSocketError(e);
            }

            SocketReady = false;

            if (OnSocketClose != null)
            {
                OnSocketClose();
            }
        }
    }

    public void Update()
    {
        if (SocketReady)
        {
            try
            {
                int iteration_count = 0;

                // Keep pumping the socket for more data while it says it has some
                while (m_stream.CanRead && m_stream.DataAvailable && iteration_count < ITERATION_MAX)
                {
                    // Read from the socket up to 1k at a time
                    int bytesRead = m_stream.Read(m_byteReadBuffer, 0, m_byteReadBuffer.Length);

                    if (bytesRead > 0)
                    {
                        int byteIndex = 0;

                        // Keep processing the byte buffer we read until we reached the end
                        while (byteIndex < bytesRead)
                        {
                            // Keep appending characters to the string builder until we hit a CR or LF
                            while (byteIndex < bytesRead)
                            {
                                char charRead = Convert.ToChar(m_byteReadBuffer[byteIndex]);

                                if (charRead != CARRIAGE_RETURN && 
                                    charRead != LINE_FEED && 
                                    m_stringReadBuffer.Length < MAX_LINE_READ_LENGTH)
                                {
                                    m_stringReadBuffer.Append(charRead);
                                    byteIndex++;
                                }
                                else
                                {
                                    // Once we hit a CR or LF, emit the completed line
                                    if (m_stringReadBuffer.Length > 0)
                                    {
                                        string readLine = m_stringReadBuffer.ToString();

                                        if (DebugEnabled)
                                        {
                                            Debug.Log("IRCSocketRead: " + readLine);
                                        }

                                        if (OnSocketRead != null)
                                        {
                                            OnSocketRead(readLine);
                                        }

                                        m_stringReadBuffer.Length = 0;
                                    }

                                    break;
                                }
                            }

                            // Eat all of the CRs and LFs
                            while (byteIndex < bytesRead)
                            {
                                char charRead = Convert.ToChar(m_byteReadBuffer[byteIndex]);

                                if (charRead == CARRIAGE_RETURN || charRead == LINE_FEED)
                                {
                                    byteIndex++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }

                    iteration_count++;
                }

                if (iteration_count >= ITERATION_MAX && DebugEnabled)
                {
                    Debug.LogError("IRCSocketRead: Iteration limit hit!");
                }
            }
            catch (Exception e)
            {
                OnSocketError(e);
                Disconnect();
            }
        }
    }

    public void SendString(string message)
    {
        if (SocketReady)
        {
            try
            {
                if (DebugEnabled)
                {
                    Debug.Log("IRCSocketWrite: " + message);
                }

                m_writer.WriteLine(message);
                m_writer.Flush();
            }
            catch (Exception e)
            {
                OnSocketError(e);
                Disconnect();
            }
        }
    }
}
