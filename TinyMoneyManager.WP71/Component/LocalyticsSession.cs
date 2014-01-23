using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Resources;

public class LocalyticsSession
{
    private static string _version;
    private string appKey;
    private const string directoryName = "localytics";
    private bool isSessionClosed;
    private bool isSessionOpen;
    private static bool isUploading = false;
    private const string libraryVersion = "windowsphone_2.1";
    private static System.IO.IsolatedStorage.IsolatedStorageFile localStorage = null;
    private const int maxNameLength = 100;
    private const int maxStoredSessions = 10;
    private const string metaFileName = "m_meta";
    private const string serviceURLBase = "http://analytics.localytics.com/api/v2/applications/";
    private string sessionFilename;
    private const string sessionFilePrefix = "s_";
    private double sessionStartTime;
    private string sessionUuid;
    private const string uploadFilePrefix = "u_";

    public LocalyticsSession(string appKey)
    {
        this.appKey = appKey;
    }

    private static void appendTextToFile(string text, string filename)
    {
        System.IO.IsolatedStorage.IsolatedStorageFileStream stream = getStreamForFile(filename);
        System.IO.TextWriter writer = new System.IO.StreamWriter(stream);
        writer.Write(text);
        writer.Close();
        stream.Close();
    }

    private void BeginUpload()
    {
        LogMessage("Beginning upload.");
        try
        {
            this.renameOrAppendSessionFiles();
            HttpWebRequest state = (HttpWebRequest)WebRequest.Create("http://analytics.localytics.com/api/v2/applications/" + this.appKey + "/uploads");
            state.Method = "POST";
            state.ContentType = "application/json";
            state.BeginGetRequestStream(new System.AsyncCallback(LocalyticsSession.httpRequestCallback), state);
        }
        catch (System.Exception exception)
        {
            LogMessage("Swallowing exception: " + exception.Message);
        }
    }

    public void close()
    {
        if (!this.isSessionOpen || this.isSessionClosed)
        {
            LogMessage("Session not closed b/c it is either not open or already closed.");
        }
        else
        {
            try
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                builder.Append("{\"dt\":\"c\",");
                builder.Append("\"u\":\"" + System.Guid.NewGuid().ToString() + "\",");
                builder.Append("\"ss\":" + this.sessionStartTime.ToString() + ",");
                builder.Append("\"su\":\"" + this.sessionUuid + "\",");
                builder.Append("\"ct\":" + GetTimeInUnixTime().ToString());
                builder.Append("}");
                builder.Append(System.Environment.NewLine);
                appendTextToFile(builder.ToString(), this.sessionFilename);
                this.isSessionOpen = false;
                this.isSessionClosed = true;
                LogMessage("Session closed.");
            }
            catch (System.Exception exception)
            {
                LogMessage("Swallowing exception: " + exception.Message);
            }
        }
    }

    private static void DeleteUploadFiles()
    {
        System.IO.IsolatedStorage.IsolatedStorageFile file = getStore();
        if (file.DirectoryExists("localytics"))
        {
            foreach (string str in file.GetFileNames(@"localytics\u_*"))
            {
                if (str.StartsWith("u_"))
                {
                    file.DeleteFile(@"localytics\" + str);
                }
            }
        }
    }

    private static string EscapeString(string input)
    {
        string str = input.Replace(@"\", @"\\");
        return ("\"" + str.Replace("\"", "\\\"") + "\"");
    }

    public static string GetAppVersion()
    {
        if (!string.IsNullOrEmpty(_version))
        {
            return _version;
        }
        Uri uriResource = new Uri("WMAppManifest.xml", UriKind.Relative);
        StreamResourceInfo resourceStream = Application.GetResourceStream(uriResource);
        if (resourceStream != null)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader(resourceStream.Stream))
            {
                bool flag = false;
                while (!reader.EndOfStream)
                {
                    string str = reader.ReadLine();
                    if (!flag)
                    {
                        int num = str.IndexOf("AppPlatformVersion=\"", System.StringComparison.InvariantCulture);
                        if (num >= 0)
                        {
                            flag = true;
                            str = str.Substring(num + 20);
                        }
                    }
                    int index = str.IndexOf("Version=\"", System.StringComparison.InvariantCulture);
                    if (index >= 0)
                    {
                        int num3 = str.IndexOf("\"", index + 9, System.StringComparison.InvariantCulture);
                        if (num3 >= 0)
                        {
                            _version = str.Substring(index + 9, (num3 - index) - 9);
                            break;
                        }
                    }
                }
                goto Label_00D0;
            }
        }
        _version = "Unknown";
    Label_00D0:
        return _version;
    }

    private string GetBlobHeader()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Append("{\"dt\":\"h\",");
        builder.Append("\"pa\":" + GetPersistStoreCreateTime() + ",");
        string sequenceNumber = GetSequenceNumber();
        builder.Append("\"seq\":" + sequenceNumber + ",");
        SetNextSequenceNumber((int.Parse(sequenceNumber) + 1).ToString());
        builder.Append("\"u\":\"" + System.Guid.NewGuid().ToString() + "\",");
        builder.Append("\"attrs\":");
        builder.Append("{\"dt\":\"a\",");
        builder.Append("\"au\":\"" + this.appKey + "\",");
        builder.Append("\"du\":\"" + GetDeviceId() + "\",");
        builder.Append("\"lv\":\"windowsphone_2.1\",");
        builder.Append("\"av\":\"" + GetAppVersion() + "\",");
        builder.Append("\"dp\":\"Windows Phone\",");
        builder.Append("\"dll\":\"" + System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName + "\",");
        builder.Append("\"dma\":\"" + DeviceStatus.DeviceManufacturer + "\",");
        builder.Append("\"dmo\":\"" + DeviceStatus.DeviceName + "\",");
        builder.Append("\"dov\":\"" + System.Environment.OSVersion.Version.Build.ToString() + "\",");
        builder.Append("\"iu\":\"" + GetInstallId() + "\"");
        builder.Append("}}");
        builder.Append(System.Environment.NewLine);
        return builder.ToString();
    }

    private static string GetDatestring()
    {
        System.DateTime time = System.DateTime.Now.ToUniversalTime();
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Append(time.Year);
        builder.Append("-");
        builder.Append(time.Month.ToString("D2"));
        builder.Append("-");
        builder.Append(time.Day.ToString("D2"));
        builder.Append("T");
        builder.Append(time.Hour.ToString("D2"));
        builder.Append(":");
        builder.Append(time.Minute.ToString("D2"));
        builder.Append(":");
        builder.Append(time.Second.ToString("D2"));
        return builder.ToString();
    }

    private static string GetDeviceId()
    {
        byte[] inArray = (byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId");
        return System.Convert.ToBase64String(inArray);
    }

    private static string GetFileContents(string filename)
    {
        System.IO.IsolatedStorage.IsolatedStorageFileStream stream = getStore().OpenFile(@"localytics\" + filename, System.IO.FileMode.Open);
        System.IO.TextReader reader = new System.IO.StreamReader(stream);
        string str = reader.ReadToEnd();
        reader.Close();
        stream.Close();
        return str;
    }

    private static string GetInstallId()
    {
        System.IO.IsolatedStorage.IsolatedStorageFileStream stream = getStore().OpenFile(@"localytics\m_meta", System.IO.FileMode.Open);
        System.IO.TextReader reader = new System.IO.StreamReader(stream);
        string str = reader.ReadLine();
        reader.Close();
        stream.Close();
        return str;
    }

    private static int getNumberOfStoredSessions()
    {
        System.IO.IsolatedStorage.IsolatedStorageFile file = getStore();
        if (!file.DirectoryExists("localytics"))
        {
            return 0;
        }
        return file.GetFileNames(@"localytics\s_*").Length;
    }

    private static string GetPersistStoreCreateTime()
    {
        System.IO.IsolatedStorage.IsolatedStorageFile file = getStore();
        string path = @"localytics\m_meta";
        if (!file.FileExists(path))
        {
            SetNextSequenceNumber("1");
        }
        System.DateTime time = new System.DateTime(0x7b2, 1, 1);
        System.TimeSpan span = (System.TimeSpan)(file.GetCreationTime(path).DateTime - time.ToLocalTime());
        int num = (int)System.Math.Round(span.TotalSeconds);
        LogMessage("Seconds is: " + num.ToString());
        return num.ToString();
    }

    private static void GetResponseCallback(System.IAsyncResult asynchronousResult)
    {
        try
        {
            HttpWebRequest asyncState = (HttpWebRequest)asynchronousResult.AsyncState;
            HttpWebResponse response = (HttpWebResponse)asyncState.EndGetResponse(asynchronousResult);
            System.IO.Stream responseStream = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(responseStream);
            LogMessage("Upload complete. Response: " + reader.ReadToEnd());
            DeleteUploadFiles();
            responseStream.Close();
            reader.Close();
            response.Close();
        }
        catch (WebException)
        {
        }
        catch (System.Exception)
        {
        }
        finally
        {
            isUploading = false;
        }
    }

    private static string GetSequenceNumber()
    {
        System.IO.IsolatedStorage.IsolatedStorageFile file = getStore();
        string path = @"localytics\m_meta";
        if (!file.FileExists(path))
        {
            SetNextSequenceNumber("1");
            return "1";
        }
        System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(@"localytics\m_meta", System.IO.FileMode.Open);
        System.IO.TextReader reader = new System.IO.StreamReader(stream);
        reader.ReadLine();
        string str2 = reader.ReadLine();
        reader.Close();
        stream.Close();
        return str2;
    }

    private static System.IO.IsolatedStorage.IsolatedStorageFile getStore()
    {
        if (localStorage == null)
        {
            localStorage = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
        }
        return localStorage;
    }

    private static System.IO.IsolatedStorage.IsolatedStorageFileStream getStreamForFile(string filename)
    {
        System.IO.IsolatedStorage.IsolatedStorageFile isf = getStore();
        isf.CreateDirectory("localytics");
        return new System.IO.IsolatedStorage.IsolatedStorageFileStream(@"localytics\" + filename, System.IO.FileMode.Append, isf);
    }

    private static double GetTimeInUnixTime()
    {
        System.TimeSpan span = (System.TimeSpan)(System.DateTime.UtcNow - new System.DateTime(0x7b2, 1, 1, 0, 0, 0));
        return System.Math.Round(span.TotalSeconds, 0);
    }

    private static string GetUploadContents()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        System.IO.IsolatedStorage.IsolatedStorageFile file = getStore();
        if (file.DirectoryExists("localytics"))
        {
            foreach (string str in file.GetFileNames(@"localytics\u_*"))
            {
                if (str.StartsWith("u_"))
                {
                    builder.Append(GetFileContents(str));
                }
            }
        }
        return builder.ToString();
    }

    private static void httpRequestCallback(System.IAsyncResult asynchronousResult)
    {
        try
        {
            HttpWebRequest asyncState = (HttpWebRequest)asynchronousResult.AsyncState;
            System.IO.Stream stream = asyncState.EndGetRequestStream(asynchronousResult);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(GetUploadContents());
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            asyncState.BeginGetResponse(new System.AsyncCallback(LocalyticsSession.GetResponseCallback), asyncState);
        }
        catch (System.Exception exception)
        {
            LogMessage("Swallowing exception: " + exception.Message);
        }
    }

    private static void LogMessage(string msg)
    {
    }

    public void open()
    {
        if (this.isSessionOpen || this.isSessionClosed)
        {
            LogMessage("Session is already opened or closed.");
        }
        else
        {
            try
            {
                if (getNumberOfStoredSessions() > 10)
                {
                    LogMessage("Local stored session count exceeded.");
                }
                else
                {
                    this.sessionUuid = System.Guid.NewGuid().ToString();
                    this.sessionFilename = "s_" + this.sessionUuid;
                    this.sessionStartTime = GetTimeInUnixTime();
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    builder.Append("{\"dt\":\"s\",");
                    builder.Append("\"ct\":" + GetTimeInUnixTime().ToString() + ",");
                    builder.Append("\"u\":\"" + this.sessionUuid + "\"");
                    builder.Append("}");
                    builder.Append(System.Environment.NewLine);
                    appendTextToFile(builder.ToString(), this.sessionFilename);
                    this.isSessionOpen = true;
                    LogMessage("Session opened.");
                }
            }
            catch (System.Exception exception)
            {
                LogMessage("Swallowing exception: " + exception.Message);
            }
        }
    }

    private void renameOrAppendSessionFiles()
    {
        System.IO.IsolatedStorage.IsolatedStorageFile file = getStore();
        if (file.DirectoryExists("localytics"))
        {
            string[] fileNames = file.GetFileNames(@"localytics\s_*");
            bool flag = false;
            foreach (string str in fileNames)
            {
                if (str.StartsWith("s_"))
                {
                    string filename = "u_" + str;
                    if (!flag)
                    {
                        appendTextToFile(this.GetBlobHeader(), filename);
                        flag = true;
                    }
                    appendTextToFile(GetFileContents(str), filename);
                    file.DeleteFile(@"localytics\" + str);
                }
            }
        }
    }

    private static void SetNextSequenceNumber(string number)
    {
        System.IO.IsolatedStorage.IsolatedStorageFile file = getStore();
        string path = @"localytics\m_meta";
        if (!file.FileExists(path))
        {
            appendTextToFile(System.Guid.NewGuid().ToString() + System.Environment.NewLine + number, "m_meta");
        }
        else
        {
            System.IO.IsolatedStorage.IsolatedStorageFileStream stream = file.OpenFile(path, System.IO.FileMode.Open);
            System.IO.TextReader reader = new System.IO.StreamReader(stream);
            string str2 = reader.ReadLine();
            reader.Close();
            stream.Close();
            System.IO.IsolatedStorage.IsolatedStorageFileStream stream2 = file.OpenFile(path, System.IO.FileMode.Truncate);
            System.IO.TextWriter writer = new System.IO.StreamWriter(stream2);
            writer.WriteLine(str2);
            writer.Write(number);
            writer.Close();
            stream2.Close();
        }
    }

    public void tagEvent(string eventName, System.Collections.Generic.Dictionary<String, String> attributes = null)
    {
        if (!this.isSessionOpen)
        {
            LogMessage("Event not tagged because session is not open.");
        }
        else
        {
            try
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                builder.Append("{\"dt\":\"e\",");
                builder.Append("\"ct\":" + GetTimeInUnixTime().ToString() + ",");
                builder.Append("\"u\":\"" + System.Guid.NewGuid().ToString() + "\",");
                builder.Append("\"su\":\"" + this.sessionUuid + "\",");
                builder.Append("\"n\":" + EscapeString(eventName));
                if (attributes != null)
                {
                    builder.Append(",\"attrs\": {");
                    bool flag = true;
                    foreach (string str in attributes.Keys)
                    {
                        if (!flag)
                        {
                            builder.Append(",");
                        }
                        builder.Append(EscapeString(str) + ":" + EscapeString(attributes[str]));
                        flag = false;
                    }
                    builder.Append("}");
                }
                builder.Append("}");
                builder.Append(System.Environment.NewLine);
                appendTextToFile(builder.ToString(), this.sessionFilename);
                LogMessage("Tagged event: " + EscapeString(eventName));
            }
            catch (System.Exception exception)
            {
                LogMessage("Swallowing exception: " + exception.Message);
            }
        }
    }

    public void upload()
    {
        if (!isUploading)
        {
            isUploading = true;
            try
            {
                System.Threading.ThreadStart start = new System.Threading.ThreadStart(this.BeginUpload);
                new System.Threading.Thread(start).Start();
            }
            catch (System.Exception exception)
            {
                LogMessage("Swallowing exception: " + exception.Message);
            }
        }
    }
}

