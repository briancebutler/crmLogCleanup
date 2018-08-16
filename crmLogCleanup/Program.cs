using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data.SQLite;
namespace crmLogCleanup
{
    class Program
    {


        public static List<string> GetFilesAndFolders(string root, int depth)
        {
            var list = new List<string>();
            foreach (var directory in Directory.EnumerateDirectories(root))
            //foreach (var directory in Path.GetFileName(root))
                {
                list.Add(directory);
                if (depth > 0)
                {
                    list.AddRange(GetFilesAndFolders(directory, depth - 1));
                }
            }

            list.AddRange(Directory.EnumerateFiles(root));

            return list;
        }



        static void Main(string[] args)
        {
            string inputArgs = "crmLogCleanup";
            string root = "C:\\~LogFiles\\~CurrentLogs";
            List<string> openIncidentList = new List<string>(); //List for tar file selection
            List<string> dirList = new List<string>(); //List for tar file selection
            List<string> content = GetFilesAndFolders(root, 2);
            List<string> ticketFolderNumberList = new List<string>(); //list to store just the ticket number from the existing folders.
            //string sqliteDB = "F:\\C#\\getLogsV15\\getLogsV15\\getLogsV15\\bin\\Debug\\sqlLiteDBFile.db";

            // Open SQLite DB
            //SQLiteConnection.CreateFile(sqlLiteDBFile);
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source=F:\\C#\\getLogsV15\\getLogsV15\\getLogsV15\\bin\\Debug\\sqlLiteDBFile.db;Version=3;");
            m_dbConnection.Open();
            // Open SQLite DB

            //Query SQLite DB
            string sql = "select Ticket,FolderSelected,Deleted from Incident where `Active` = 'YES'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                Console.WriteLine("Ticket#: " + reader["Ticket"] + "\tFolderSelected: " + reader["FolderSelected"] + "\tDeleted: " +reader["Deleted"]);
            
            //Query SQLite DB



            if (args == null)
            {
                Console.WriteLine("No Customer info found");
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string argument = args[i];
                    inputArgs = args[0];
                }
            }

            inputArgs = inputArgs.Remove(0,8);
            string[] cmdArgs = inputArgs.Split('/');

            
            foreach(string incident in cmdArgs)
            {
                if(incident.Contains("-"))
                {
                    //Console.WriteLine(incident);
                    openIncidentList.Add(incident);
                }
                
                
            }

            Console.WriteLine("\nOpen Tickets - :openIncidentList:");

            foreach (string tr in openIncidentList)
            {
                Console.WriteLine(tr);
            }

            Console.WriteLine("\nLog directory's - :dirList:");

            foreach (string subDirList in content)
            {
                if (subDirList.Contains("18"))
                {


                    Console.WriteLine(subDirList);
                    dirList.Add(subDirList);//Full path to existing folders on the file system that are for a ticket.
                    ticketFolderNumberList.Add(Path.GetFileName(subDirList));
                }

            }


            Console.WriteLine("\nTicket Folders - :ticketFolderNumberList:");

            foreach(string ticketNumber in ticketFolderNumberList)
            {
                Console.WriteLine(ticketNumber);
            }







            var firstNotSecond = openIncidentList.Except(ticketFolderNumberList).ToList();
            Console.WriteLine("\nNo log files found for open tickets:");

            foreach(string diff in firstNotSecond)
            {
                Console.WriteLine(diff);
            }





            var SecondNotFirst = ticketFolderNumberList.Except(cmdArgs).ToList();

            Console.WriteLine("\nTicket Folders to be archived.");

            foreach(string diff in SecondNotFirst)
            {
                Console.WriteLine(diff);
            }


            foreach(string diff in cmdArgs)
            {
                if (dirList.Contains(diff) ==true)
                {
                    foreach(string dir in dirList)
                    { 
                    Console.WriteLine(dir);
                    }
                }
                    //Console.WriteLine(diff);
            }

            foreach(string folder in dirList)

                if(openIncidentList.Contains(folder)==true)
                {
                    Console.WriteLine(folder);
                }



            m_dbConnection.Close();


            Console.Read();

        }
    }
}
