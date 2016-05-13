using System;
using System.IO;
using System.Threading;

namespace ScriptRunner.DBProcess
{
    public static class DBProcess
    {
       
        /// <summary>
        /// Execute the Sql script file and write the out to log file
        /// </summary>
        /// <param name="writr"></param>
        /// <param name="dname"></param>
        /// <param name="success"></param>
        /// <param name="file"></param>
        /// <returns></returns>

        public static bool executeProcess(StreamWriter writr, string dname, bool success, string file)
        {
            try
            {
                string output = "";
                string error = string.Empty;
                string cmd = string.Empty;

                cmd = "/C sqlcmd -S " + Environment.MachineName + "\\" + dname + " -i \"" + file + "\"";

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WorkingDirectory = @"C:\Windows\System32";
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = cmd;

                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                process.StartInfo = startInfo;
                writr.WriteLine("Started the " + file + " execution");
                process.Start();

                using (System.IO.StreamReader myOutput = process.StandardOutput)
                {
                    output = myOutput.ReadToEnd();
                    writr.WriteLine(output);
                }
                using (System.IO.StreamReader myError = process.StandardError)
                {
                    error = myError.ReadToEnd();
                    if (error != "")
                        writr.WriteLine(error);
                }
                process.WaitForExit();

                writr.WriteLine("The " + file + " execution Completed");
                writr.WriteLine("");
            }
            catch (Exception ex)
            { success = false; writr.WriteLine("Exception in File : " + file + "\n\r" + ex.ToString()); }
            Thread.Sleep(3000);
            return success;
        }



    }
}