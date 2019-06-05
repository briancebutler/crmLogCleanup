using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.Diagnostics;
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
            string objDestFolder = "c:\\1\\"; //staging dir used to copy longer file to before cleaning them up. I could probably do away with this since we are now using robocopy to perform the deletes.


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

            inputArgs = inputArgs.Remove(0, 8);
            string[] cmdArgs = inputArgs.Split('/');

            //Get list of open tickets.
            foreach (string incident in cmdArgs)
            {
                if (incident.Contains("-"))
                {
                    openIncidentList.Add(incident);
                }
            }



            if (!Directory.Exists(objDestFolder))
            {
                Directory.CreateDirectory(objDestFolder);
            }

            // Open SQLite DB
            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            SQLiteConnection m_dbConnection;
            string sqlLiteDBFile = ".\\sqlLiteDBFile.db";
            string path2 = Directory.GetCurrentDirectory();
            m_dbConnection = new SQLiteConnection("Data Source=.\\sqlLiteDBFile.db;Version=3;");

            Console.WriteLine("DB File is located in " + path2 + sqlLiteDBFile);
            m_dbConnection.Open();
            // Open SQLite DB


            // Added the below section to set active in the case where the browser does not respond and marks all cases as 'Active' = 'NO'. If you re reun the tool again it will update the database table in the to YES for all tickets that are still open.
            foreach(string objTicket in openIncidentList)
            {
                string sql2 = "UPDATE Incident SET Active = 'YES' WHERE Ticket ='" + objTicket + "'"; //Problems with CRM may cause the tool to incorrectly mark incidents are deleted. This will force them to active if the tool is rereun in the case of that happening.
                //Console.WriteLine("SQL2: " + sql2);
                SQLiteCommand command2 = new SQLiteCommand(sql2, m_dbConnection);
                command2.ExecuteNonQuery();
            }

            string sql = "select Ticket,FolderSelected,Deleted from Incident where `Active` = 'YES'"; //create a list of active databasess 
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                sqliteTicket.Add(Convert.ToString(reader["Ticket"]));
                sqliteDeleted.Add(Convert.ToString(reader["Deleted"])); //No longer used could be cleaned up.
            }
            //Query SQLite DB




            //Console.WriteLine("Inactive Tickets");
            foreach (string objTicket in sqliteTicket)
            {
                //Console.WriteLine("Active Tickets: " + objTicket);
                if (!openIncidentList.Contains(objTicket))
                {
                    sqliteDeleteFolder.Add(objTicket); //create a list of tickets that need to be cleaned up.
                }
            }

            
            //Console.WriteLine("Tickets marked as not Active in SQLite Database");
            foreach (string objFolderDelete in sqliteDeleteFolder)
            {
                string sqlQuery = "UPDATE Incident SET Active = 'NO' WHERE Ticket ='" + objFolderDelete + "'"; //update sql to mark ticket in sqliteDeleteFolder list to active=no
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

            bool isEmpty = sqliteFolderSelected.Any();
            if(isEmpty)
            { 
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
            }

            else
            {
                processDirectory(@"C:\~LogFiles");
                Console.WriteLine("Nothing to delete. Press enter to exit.");
                Console.Read();
                return;
            }

            DELETE:
            foreach(string objFolder in sqliteFolderSelected)
            {

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
                        int numFolder = 1;
                        Console.WriteLine(objFolder);
                        Directory.Move(objFolder, objDestFolder + "\\" + numFolder++);
                        string sql4 = "UPDATE Incident SET Deleted = 'YES' WHERE FolderSelected ='" + objFolder + "'";
                        Console.WriteLine(sql4);
                        SQLiteCommand command4 = new SQLiteCommand(sql4, m_dbConnection);
                        command4.ExecuteNonQuery();
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Directory.Move(objFolder, objDestFolder);
                        string sql4 = "UPDATE Incident SET Deleted = 'YES' WHERE FolderSelected ='" + objFolder + "'";
                        Console.WriteLine(sql4);
                        SQLiteCommand command4 = new SQLiteCommand(sql4, m_dbConnection);
                        command4.ExecuteNonQuery();

                        if (Directory.Exists(objDestFolder))
                        {
                            Directory.Delete(objDestFolder, true);
                            processDirectory(@"C:\~LogFiles");
                        }
                        Console.ForegroundColor = defaultForeground;

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

                    string emptyFolder = "c:\\emptyfolder"; //empty folder needed for robocopy.exe

                    if (!Directory.Exists(emptyFolder))
                    {
                        Directory.CreateDirectory(emptyFolder);
                    }


                    //Added the following to use robocopy for cleanup of files that contain long file paths.
                    string cmdPath = "C:\\Windows\\System32\\robocopy.exe";
                    ProcessStartInfo pro = new ProcessStartInfo();
                    pro.WindowStyle = ProcessWindowStyle.Hidden;
                    pro.FileName = cmdPath; 


                    if (Directory.Exists(objDestFolder))
                    {
                        //Directory.Delete(objDestFolder, true); OLD CLEANUP METHOD

                        pro.Arguments = string.Format("{0} {1} /MIR", emptyFolder, objDestFolder);  //robocopy.exe c:\emptyfolder c:\1 /MIR
                        Process x = Process.Start(pro); 
                        x.WaitForExit();
                    }


                    if (Directory.Exists(emptyFolder))
                    {
                        Directory.Delete(emptyFolder);
                    }


                    Console.WriteLine("Deleting: {0}", objFolder);

                }

            }

            


            //Folder cleanup
            Console.WriteLine("Cleaning up emtpy folders:");
            var subDirs = Directory.GetDirectories(root);

            foreach (string dir in subDirs)
            {
                if (!Directory.EnumerateDirectories(dir).Any())
                {
                    Console.WriteLine(dir);
                    Directory.Delete(dir);
                    //goto DELETE;
                }
                
            }
            m_dbConnection.Close();
            //Folder cleanup
            processDirectory(@"C:\~LogFiles");
            Console.WriteLine("Done press enter to exit ....");
            Console.Read();

        }

        //Function added to cleanup empty folders 06/05/19
        private static void processDirectory(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                processDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Console.WriteLine("Cleaning up emtpy folder " + directory);
                    Directory.Delete(directory, false);
                }
            }
        }

    }
}



//pull from sql lite db location when its the same as the binary. 
// add chnages to close out gxtail, notepad, explorer windows if they are open. 