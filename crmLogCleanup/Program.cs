﻿using System;
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

        static void Main(string[] args)
        {
            string inputArgs = "crmLogCleanup";
            string root = "C:\\~LogFiles\\~CurrentLogs";
            List<string> openIncidentList = new List<string>(); //List for tar file selection
            List<string> dirList = new List<string>(); //List for tar file selection
            List<string> ticketFolderNumberList = new List<string>(); //list to store just the ticket number from the existing folders.
            List<string> sqliteTicket = new List<string>();
            List<string> sqliteFolderSelected = new List<string>();
            List<string> sqliteDeleted = new List<string>();
            List<string> sqliteDeleteFolder = new List<string>();

            // Open SQLite DB
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source=F:\\C#\\getLogsV15\\getLogsV15\\getLogsV15\\bin\\Debug\\sqlLiteDBFile.db;Version=3;");
            m_dbConnection.Open();
            // Open SQLite DB

            //Query SQLite DB
            string sql = "select Ticket,FolderSelected,Deleted from Incident where `Active` = 'YES'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                sqliteTicket.Add(Convert.ToString(reader["Ticket"]));
                sqliteDeleted.Add(Convert.ToString(reader["Deleted"]));
            }
            //Query SQLite DB

            //Import input arguments
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
            //Import input arguments

                                 
            foreach (string objTicket in sqliteTicket)
            {
                //Console.WriteLine(objTicket);
                if (!openIncidentList.Contains(objTicket))
                {
                    //Console.WriteLine(objTicket + ": objTicket");
                    sqliteDeleteFolder.Add(objTicket);
                }
            }

            
            Console.WriteLine("Tickets marked as not Active in SQLite Database");
            foreach (string objFolderDelete in sqliteDeleteFolder)
            {
                //Console.WriteLine(objFolderDelete + "Hello");
                string sqlQuery = "UPDATE Incident SET Active = 'NO' WHERE Ticket ='" + objFolderDelete + "'";
                Console.WriteLine(sqlQuery);
                SQLiteCommand command2 = new SQLiteCommand(sqlQuery, m_dbConnection);
                
                command2.ExecuteNonQuery();
                //Console.WriteLine(sqlQuery + "Hello WTF");
                
            }



            //Query SQLite DB
            string sql3 = "select FolderSelected from Incident where Active = 'NO' and Deleted = 'NO'";
            SQLiteCommand command3 = new SQLiteCommand(sql3, m_dbConnection);
            SQLiteDataReader reader2 = command3.ExecuteReader();
            while (reader2.Read())
            {
                //sqliteTicket.Add(Convert.ToString(reader["Ticket"]));
                sqliteFolderSelected.Add(Convert.ToString(reader2["FolderSelected"]));
                //sqliteDeleted.Add(Convert.ToString(reader["Deleted"]));
                //Console.WriteLine("Ticket#: " + reader["Ticket"] + "\tFolderSelected: " + reader["FolderSelected"] + "\tDeleted: " +reader["Deleted"]);
            }
            //Query SQLite DB

            Console.WriteLine("\nTicket folders to be cleaned up");
            foreach(string objFolder in sqliteFolderSelected)
            {
                Console.WriteLine(objFolder);
            }


            Console.WriteLine("Would you like to cleanup the folders above? [y/n] | Default = N");
            String deleteResponse = Console.ReadLine();
            Console.WriteLine("You have chosen: {0}", deleteResponse);

            if(deleteResponse == "y")
            {

                goto DELETE;
                
            }
            else if (deleteResponse =="n")
            {
                m_dbConnection.Close();
                return;
            }
            else
            {
                m_dbConnection.Close();
                return;
            }

            DELETE:
            foreach(string objFolder in sqliteFolderSelected)
            {
                Directory.Delete(objFolder, true);
                Console.WriteLine("Deleting: {0}", objFolder);

                string sql4 = "UPDATE Incident SET Deleted = 'YES' WHERE FolderSelected ='" + objFolder + "'";
                Console.WriteLine(sql4);
                SQLiteCommand command4 = new SQLiteCommand(sql4, m_dbConnection);
                command4.ExecuteNonQuery();

            }

            m_dbConnection.Close();


            //Folder cleanup
            Console.WriteLine("Cleaning up emtpy folders:");
            var subDirs = Directory.GetDirectories(root);

            foreach (string dir in subDirs)
            {
                //Console.WriteLine(dir);

                    //Console.WriteLine(dir);
                if (!Directory.EnumerateDirectories(dir).Any())
                {
                    Console.WriteLine(dir);
                    Directory.Delete(dir);
                }
                

            }
            

            //bool isEmpty = !Directory.EnumerateFiles(path).Any();



            Console.WriteLine("Done Exit ....");
            Console.Read();

        }
    }
}
