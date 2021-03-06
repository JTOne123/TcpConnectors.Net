﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TcpConnectors;
using TestRequestMultiResponseCommon;

namespace TestRequestMultiResponseClient
{
    class Program
    {
        internal static ClientConnector _clientConnector = null;
        static void Main(string[] args)
        {
            Console.WriteLine("TestRequestMultiResponseClient");

            var serverIp = "127.0.0.1";
            //var serverIp = "83.151.199.77";

            _clientConnector = new ClientConnector(new ClientConnectorSettings()
            {
                PacketsMap = new Dictionary<Tuple<int, int>, Type>() {
                    { new Tuple<int, int>(1, 1), typeof(GetListResponsePacket) },
                    { new Tuple<int, int>(1, 2), typeof(GetListResponsePacket) },
                },
                ServerAddressList = new List<Tuple<string, int>>() { new Tuple<string, int>(serverIp, 1112) }
            });

            _clientConnector.OnPacket += ClientConnector_OnPacket;
            _clientConnector.OnConnect += ClientConnector_OnConnect;
            _clientConnector.OnDisconnect += ClientConnector_OnDisconnect;
            _clientConnector.OnException += ClientConnector_OnException;
            _clientConnector.OnDebugLog += ClientConnector_OnDebugLog;

            _clientConnector.Connect();

            while (true)
            {
                try
                {
                    Console.WriteLine("Enter Input (r-request, m-request multi response, x - test RequestMultiResponsesHandler)");
                    var inputLine = Console.ReadLine();

                    if (inputLine == "r")
                    {
                        Console.WriteLine("Perform Request (large single response)");
                        var resPacket = _clientConnector.SendRequest(1, 1, new GetListRequestPacket()) as GetListResponsePacket;
                        //Console.WriteLine($"response packet:{JsonConvert.SerializeObject(resPacket)}");
                        Console.WriteLine($"response packet count:{resPacket.Records.Count}");
                    }
                    else if (inputLine == "m")
                    {
                        Console.WriteLine("Perform Request - multi responses");

                        for (int i = 0; i < 10; i++)
                        {

                            _clientConnector.SendRequestMultiResponses(
                                1,
                                2,
                                new GetListRequestPacket(),
                                MultiResponseCallback);
                        }

                        //var resPacket = _clientConnector.SendRequest(1, 1, new GetListRequestPacket()) as GetListResponsePacket;
                        //Console.WriteLine($"response packet:{JsonConvert.SerializeObject(resPacket)}");
                        //Console.WriteLine($"response packet count:{resPacket.List.Count}");
                    }
                    else if (inputLine == "x")
                    {
                        Console.WriteLine("TestRequestMultiResponsesHandler");
                        var test = new TestRequestMultiResponsesHandler();
                        test.Run();


                    }
                    else
                    {
                        Console.WriteLine("input incorrect.");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception:" + ex.ToString());
                }
            }

        }

        private static void MultiResponseCallback(object packet, bool isLast, int nReceived, int nTotal, Exception exp)
        {
            Console.WriteLine($"MultiResponseCallback isLast={isLast} nReceived={nReceived} nTotal={nTotal} ");

            if (exp != null)
            {
            Console.WriteLine($"*********** exp = {exp.Message} ***************");

            }

            Console.WriteLine($"packet = {JsonConvert.SerializeObject(packet, Formatting.Indented)}");

        }

        private static void ClientConnector_OnDisconnect()
        {
            Console.WriteLine($"ClientConnector_OnDisconnect");
        }

        private static void ClientConnector_OnConnect()
        {
            Console.WriteLine($"ClientConnector_OnConnect");
        }

        private static void ClientConnector_OnPacket(int module, int command, object packet)
        {
            Console.WriteLine($"ClientConnector_OnPacket module:{module} command:{command} packet:{JsonConvert.SerializeObject(packet)}");
        }

        private static void ClientConnector_OnDebugLog(DebugLogType logType, string info)
        {
            Console.WriteLine($"ClientConnector_OnDebugLog logType:{logType} info:{info}");
        }

        private static void ClientConnector_OnException(Exception exp)
        {
            Console.WriteLine($"ClientConnector_OnException {exp}");
        }
    }
}
