using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkVideoEncoder
{
    class Program
    {
        private static List<string> IP;
        private static string directory;
        private static string extension;
        private static int masterRender;

        private static string configFile = "VideoEncoder.conf";

        public static object IPAdress { get; private set; }

        static void Main(string[] args)
        {
            string answer;

            begin:

            Console.WriteLine("master or slave");
            answer = Console.ReadLine();

            if (answer == "master")
            {
                masterRoute();
            }
            else if (answer == "slave")
            {
                slave s = new slave(8002);
            }
            else
            {
                Console.WriteLine(answer);
                goto begin;
            }
        }

        private static void masterRoute()
        {
            if (File.Exists(configFile))
            {
                checkFileContents();
            }
            else
            {
                createBasicConf();
            }
        }

        private static void checkFileContents()
        {
            IP = new List<string>();
            masterRender = int.MaxValue;

            string read;
            StreamReader reader = new StreamReader(configFile);

            while (true)
            {
                read = reader.ReadLine();
                if (read != null)
                {
                    checkInfo(read);
                }
                else
                {
                    break;
                }
            }

            //start master
            master m = new master(IP, directory , extension , masterRender); 

        }

        private static void checkInfo(string read)
        {
            if (read != "")
            {
                bool ignore = false; // true when all variables are filled in meaning this read contain crap
                int type = int.MaxValue;
                char[] info = read.ToCharArray();
                string buffer = "";

                if (info[0] == ':')
                {
                    if (IP.Count == 0)
                    {
                        type = 1;
                    }
                    else if (directory == null)
                    {
                        type = 2;
                    }
                    else if (extension == null)
                    {
                        type = 3;
                    }
                    else if (masterRender == int.MaxValue)
                    {
                        type = 4;
                    }
                    else
                    {
                        ignore = true;
                    }

                    if (!ignore)
                    {
                        for (int i = 1; i < info.Length - 1; i++)
                        {
                            if (type == 1 && info[i] == ',')
                            {
                                testAdress(buffer);
                                buffer = "";
                                i++;
                            }

                            if (type == 4 && info[i] == '=')
                            {
                                if (buffer == "masterRender")
                                {
                                    string temp = ""; 
                                    temp += info[i + 1];
                                    masterRender = Convert.ToInt16(temp);
                                    break;
                                }
                            }

                            buffer += info[i];
                        }

                        if (type == 1)
                        {
                            testAdress(buffer);
                        }
                        if (type == 2)
                        {
                            directory = buffer;
                        }
                        if (type == 3)
                        {
                            extension = buffer;
                        }
                    }
                }
            }
        }

        private static void testAdress(string buffer)
        {
            try
            {
                IPAddress.Parse(buffer);
                IP.Add(buffer);
            }
            catch (FormatException e)
            {
                Console.WriteLine("IPAddres does not parse" + e.ToString());
            }
        }

        private static void createBasicConf()
        {
            StreamWriter writer = new StreamWriter(configFile, true);
            writer.WriteLine("#list of IP's#" + Environment.NewLine + "#like with no Hashtag :192.168.1.2,172.5.2.9: #" + Environment.NewLine);
            writer.WriteLine("#Directory full dir's like in linux  :/..../:#" + Environment.NewLine);
            writer.WriteLine("#extension like :.mkv:   or anything else possible#" + Environment.NewLine);
            writer.WriteLine("#master does render to#");
            writer.WriteLine(":masterRender=1:");
            writer.Close();
            Console.WriteLine("conf file created");
            Console.ReadLine();
        }
    }
}
