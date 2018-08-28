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

        static void Main(string[] args)
        {
            ConsoleColor defaultForeground = Console.ForegroundColor;
            string inputArgs = "crmLogCleanup";
            string root = "C:\\~LogFiles";
            List<string> openIncidentList = new List<string>(); //List for tar file selection
            List<string> dirList = new List<string>(); //List for tar file selection
            List<string> ticketFolderNumberList = new List<string>(); //list to store just the ticket number from the existing folders.
            List<string> sqliteTicket = new List<string>();
            List<string> sqliteFolderSelected = new List<string>();
            List<string> sqliteDeleted = new List<string>();
            List<string> sqliteDeleteFolder = new List<string>();

            string objDestFolder = "c:\\1\\";

            
            if (!Directory.Exists(objDestFolder))
            {
                //Directory.Delete(objDestFolder, true);
                Directory.CreateDirectory(objDestFolder);
            }

            //

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
                    openIncidentList.Add(incident);
                }
            }
            //Import input arguments

                                 
            foreach (string objTicket in sqliteTicket)
            {

                if (!openIncidentList.Contains(objTicket))
                {
                    sqliteDeleteFolder.Add(objTicket);
                }
            }

            
            Console.WriteLine("Tickets marked as not Active in SQLite Database");
            foreach (string objFolderDelete in sqliteDeleteFolder)
            {
                string sqlQuery = "UPDATE Incident SET Active = 'NO' WHERE Ticket ='" + objFolderDelete + "'";
                Console.WriteLine(sqlQuery);
                SQLiteCommand command2 = new SQLiteCommand(sqlQuery, m_dbConnection);
                command2.ExecuteNonQuery();
               
            }



            //Query SQLite DB
            string sql3 = "select FolderSelected from Incident where Active = 'NO' and Deleted = 'NO'";
            SQLiteCommand command3 = new SQLiteCommand(sql3, m_dbConnection);
            SQLiteDataReader reader2 = command3.ExecuteReader();
            while (reader2.Read())
            {
                sqliteFolderSelected.Add(Convert.ToString(reader2["FolderSelected"]));
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
                //Console.WriteLine("\\?\" + objFolder);

                if (!Directory.Exists(objFolder))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Directory {0} no longer exists", objFolder);
                    Console.ForegroundColor = defaultForeground;
                    string sql4 = "UPDATE Incident SET Deleted = 'YES' WHERE FolderSelected ='" + objFolder + "'";
                    SQLiteCommand command4 = new SQLiteCommand(sql4, m_dbConnection);
                    command4.ExecuteNonQuery();
                }

                else
                {
                    try
                    {
                        //var tmpPath = @"\\?\" + objFolder;
                        Console.WriteLine(objFolder);
                        //Directory.Delete(@"\\?\" + "C:\\~LogFiles\\ChildrensHospitalColorado\\180822-534\\sendLogFiles_FE357_2018_08_22_11_21_14_1159319\\prdcvcs\\AllUsersProfile_1534958497\\LogFiles\\Instance001\\InstallLogs\\2017-08-23 08-27-48");
                        Directory.Move(objFolder, objDestFolder + "\\1");
                        Directory.Delete(objDestFolder, true);
                        string sql4 = "UPDATE Incident SET Deleted = 'YES' WHERE FolderSelected ='" + objFolder + "'";
                        Console.WriteLine(sql4);
                        SQLiteCommand command4 = new SQLiteCommand(sql4, m_dbConnection);
                        command4.ExecuteNonQuery();
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        //Directory.Delete(@"\\?\C:\\~LogFiles\\ChildrensHospitalColorado\\180822-534\\sendLogFiles_FE357_2018_08_22_11_21_14_1159319\\prdcvcs\\AllUsersProfile_1534958497\\LogFiles\\Instance001\\InstallLogs\\2017-08-23 08-27-48");
                        Console.WriteLine("Unable to completly delete " + objFolder);
                        Console.WriteLine("Moving folder {0} to shorter path {1} to attempt delete", objFolder, objDestFolder);
                        
                        Console.ForegroundColor = defaultForeground;
                        //Directory.Delete("\\\\?\\" + objFolder);
                    }
                    catch (System.IO.IOException)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Unable to access the folder {0} please ensure\nthat you do not have it open in explorer, gxtail, notepad, etc.\nOnce closed please re-reun the tool.",objFolder);
                        Console.ForegroundColor = defaultForeground;
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Unauthorized access to " + objFolder);
                        Console.ForegroundColor = defaultForeground;
                    }

                    if (Directory.Exists(objDestFolder))
                    {
                        Directory.Delete(objDestFolder, true);
                    }
                    
                    Console.WriteLine("Deleting: {0}", objFolder);

                }

            }

            m_dbConnection.Close();


            //Folder cleanup
            Console.WriteLine("Cleaning up emtpy folders:");
            var subDirs = Directory.GetDirectories(root);

            foreach (string dir in subDirs)
            {
                if (!Directory.EnumerateDirectories(dir).Any())
                {
                    Console.WriteLine(dir);
                    Directory.Delete(dir);
                }
                
            }
            //Folder cleanup

            Console.WriteLine("Done press enter to exit ....");
            Console.Read();

        }
    }
}



//pull from sql lite db location when its the same as the binary. 
//modify startup to look for inactive cases then prompt if there is any or close if there is not.