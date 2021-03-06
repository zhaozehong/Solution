﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Zehong.CShart.Solution.HelperLib.Properties;
using System.Text.RegularExpressions;
using System.Windows;
using System.Security.Principal;
using System.Globalization;
using System.Security.Cryptography;

namespace Zehong.CShart.Solution.HelperLib
{
  public static class Helper
  {
    public static String ForceCreateDirectory(String directory)
    {
      if (String.IsNullOrWhiteSpace(directory))
        return String.Empty;

      var pathRoot = Path.GetPathRoot(directory);
      if (String.IsNullOrEmpty(pathRoot))
        directory = String.Format("{0}\\{1}", AppDataDirectory, directory);

      var parentDirectory = Path.GetDirectoryName(directory);
      if (!Directory.Exists(parentDirectory))
        ForceCreateDirectory(parentDirectory);

      if (!Directory.Exists(directory))
        Directory.CreateDirectory(directory);

      return directory;
    }
    public static Int32 Launch(String shellexe, String args, Boolean isWaitForExit, Boolean isCreateNoWindow, String workingDirectory = null)
    {
      try
      {
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = shellexe;
        process.StartInfo.Arguments = args;
        process.StartInfo.CreateNoWindow = isCreateNoWindow;
        if (isCreateNoWindow)
          process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        if (!String.IsNullOrWhiteSpace(workingDirectory))
          process.StartInfo.WorkingDirectory = workingDirectory;
        process.EnableRaisingEvents = true;

        process.Start();
        if (isWaitForExit)
        {
          process.WaitForExit();
          return process.ExitCode;
        }
        return process.Id;
      }
      catch (Exception e)
      {
        if (!(e is InvalidOperationException))
          ExceptionHandler.ThrowException(String.Format("{0}({1} {2})", e.Message, shellexe, args));
        return -1;
      }
    }
    public static String GetLocalizedName(object value)
    {
      if (value == null)
        return String.Empty;

      var strValue = value.ToString().Replace(" ", "");
      if (String.IsNullOrWhiteSpace(strValue))
        return String.Empty;

      var typeValue = value.GetType();
      if (typeValue == typeof(Int16) || typeValue == typeof(Int32) || typeValue == typeof(Int64))
        return strValue;
      if (SpecialStrings.StringList.Contains(strValue))
        return strValue;
      var statisticVar = SpecialStrings.StringDictionary.FirstOrDefault(p => p.Value == value.ToString()).Key;
      if (!String.IsNullOrEmpty(statisticVar))
        strValue = statisticVar;

      var localizedName = String.Empty;
      if (typeValue.IsEnum)
        localizedName = GetAppResource(String.Format("strEnum_{0}_{1}", typeValue.Name, value));
      if (!String.IsNullOrWhiteSpace(localizedName))
        return localizedName;

      localizedName = GetAppResource("Common_" + strValue);
      if (!String.IsNullOrEmpty(localizedName))
        return localizedName;

      localizedName = GetAppResource("str" + strValue);
      if (!String.IsNullOrEmpty(localizedName))
        return localizedName;

#if DEBUG
      ExceptionHandler.ThrowException(String.Format("Cannot find resource (Key={0})", strValue));
#endif
      return ConvertStringByCapitalChar(strValue);
    }
    public static String GetAppResource(String key)
    {
      Resources.ResourceManager.IgnoreCase = true;
      var strValue = Resources.ResourceManager.GetString(key, Resources.Culture);
      if (String.IsNullOrWhiteSpace(strValue))
        return String.Empty;

      strValue = strValue.Replace("\\r", "\r");
      strValue = strValue.Replace("\\n", "\n");
      strValue = strValue.Replace("\\t", "\t");
      return strValue;
    }
    public static String ConvertStringByCapitalChar(String strSource)
    {
      var sb = new StringBuilder();
      var charArray = strSource.ToCharArray();
      var prevChartIsCaptital = true;
      foreach (Char c in charArray)
      {
        if (Regex.IsMatch(c.ToString(), "[A-Z]"))
        {
          if (!prevChartIsCaptital)
            sb.Append(' ');
          prevChartIsCaptital = true;
        }
        else
        {
          prevChartIsCaptital = false;
        }
        sb.Append(c);
      }
      return sb.ToString();
    }
    public static Stream GetResourceStream(String uriString)
    {
      try
      {
        var uri = new Uri(uriString, UriKind.Relative);
        var streamSourceInfo = Application.GetResourceStream(uri);
        if (streamSourceInfo == null)
          return null;
        return streamSourceInfo.Stream;
      }
      catch (Exception e)
      {
        ExceptionHandler.ThrowException(e);
        return null;
      }
    }
    public static void Pump(Stream input, Stream output)
    {
      var n = 0;
      var bytes = new byte[4096]; // 4KB at a time    
      while ((n = input.Read(bytes, 0, bytes.Length)) != 0)
      {
        output.Write(bytes, 0, n);
      }
    }
    public static Boolean IsAdmin()
    {
      var identity = WindowsIdentity.GetCurrent();
      var principal = new WindowsPrincipal(identity);
      return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    public static Boolean? ShowDialog(Window dialog, Boolean isDefaultPosition = true)
    {
      if (Application.Current != null && Application.Current.CheckAccess() == false)
      {
        Boolean? retValue = null;
        Application.Current.Dispatcher.Invoke(new Action(() => retValue = ShowDialog(dialog, isDefaultPosition)));
        return retValue;
      }

      Debug.Assert(dialog != null);
      if (dialog == null)
        return null;
      Boolean? isEnabledOld = null;
      var isTopmostOld = dialog.Topmost;
      if (Helper.IsBrowserApplication)
      {
        if (Application.Current != null && Application.Current.MainWindow != null)
        {
          isEnabledOld = OS.Win32.IsWindowEnabled(OS.Win32.GetActiveWindow());
          OS.Win32.EnableWindow(OS.Win32.GetActiveWindow(), false);

          if (isDefaultPosition)
            dialog.Loaded += new RoutedEventHandler(OnBrowserApplicationDialog_Loaded);
        }
        dialog.Topmost = true;
      }
      else
      {
        if (Application.Current != null && !dialog.Equals(Application.Current.MainWindow) && Application.Current.MainWindow.IsLoaded
          && Application.Current.MainWindow.Visibility == Visibility.Visible)
        {
          dialog.Owner = Application.Current.MainWindow;
          if (dialog.Icon == null)
            dialog.Icon = Application.Current.MainWindow.Icon;
        }
        if (isDefaultPosition)
          dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      }

      dialog.ShowInTaskbar = false;  // model dialog should not show icon in the task bar.
      using (new CursorSetter(null))
      {
        WindowsManager.Current.AddDialog(dialog);
        bool? result = dialog.ShowDialog();
        WindowsManager.Current.RemoveDialog(dialog);

        if (isEnabledOld != null)
          OS.Win32.EnableWindow(OS.Win32.GetActiveWindow(), isEnabledOld ?? true);

        dialog.Loaded -= new RoutedEventHandler(OnBrowserApplicationDialog_Loaded);
        dialog.Topmost = isTopmostOld;

        return result;
      }
    }
    private static void OnBrowserApplicationDialog_Loaded(object sender, RoutedEventArgs e)
    {
      var dialog = sender as Window;
      if (dialog == null || !Helper.IsBrowserApplication)
        return;

      if (Application.Current != null && Application.Current.MainWindow != null)
      {
        var orginalPoint = Application.Current.MainWindow.PointToScreen(new Point(0, 0));
        dialog.WindowStartupLocation = WindowStartupLocation.Manual;
        dialog.Left = orginalPoint.X + (Application.Current.MainWindow.Width - dialog.Width) / 2;
        dialog.Top = orginalPoint.Y + (Application.Current.MainWindow.Height - dialog.Height) / 2;
      }
    }

    // String<----->List
    public static List<Int32> StringToIntList(String strSource, String separator = ";")
    {
      var intList = new List<Int32>();
      var strList = StringToList(strSource, separator);
      foreach (var str in strList)
      {
        int valueInt = 0;
        if (Int32.TryParse(str, out valueInt))
          intList.Add(valueInt);
      }
      return intList;
    }
    public static List<String> StringToList(String strSource, String separator = ";", Boolean keepEmptyEntries = true)
    {
      var retList = new List<String>();
      if (String.IsNullOrWhiteSpace(strSource))
        return retList;

      var strArray = strSource.Split(new String[] { separator }, StringSplitOptions.None);
      foreach (var str in strArray)
      {
        if (keepEmptyEntries || !String.IsNullOrWhiteSpace(str))
          retList.Add(str);
      }
      return retList;
    }
    public static List<T> StringToList<T>(String strSource, String separator = ";")
    {
      var retList = new List<T>();
      var strList = StringToList(strSource, separator);
      foreach (var str in strList)
      {
        var value = str.SafeConvertInvariantStringTo<T>();
        retList.Add(value);
      }
      return retList;
    }
    public static String ListToString<T>(List<T> dataList, String separator = ";")
    {
      if (dataList == null || !dataList.Any())
        return String.Empty;

      var strReturn = String.Empty;
      for (int i = 0; i < dataList.Count; i++)
      {
        if (i == dataList.Count - 1)
          strReturn += (dataList[i] != null ? dataList[i].ToString() : String.Empty);
        else
          strReturn += (dataList[i] != null ? dataList[i].ToString() : String.Empty) + separator;
      }
      return strReturn;
    }

    // MD5
    public static byte[] GetMD5HashFromString(String strSource)
    {
      var md5 = new MD5CryptoServiceProvider();
      var sourceBytes = ASCIIEncoding.Default.GetBytes(strSource);
      return md5.ComputeHash(sourceBytes);
    }
    public static byte[] GetMD5HashFromFile(String filePath)
    {
      using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        var hash = GetMD5HashFromStream(file);
        file.Close();
        return hash;
      }
    }
    public static byte[] GetMD5HashFromStream(Stream inputStream)
    {
      Debug.Print("TotalMemory : {0}", GC.GetTotalMemory(false));
      inputStream.Position = 0;
      using (var md5 = new MD5CryptoServiceProvider())
      {
        var hash = md5.ComputeHash(inputStream);
        Debug.Print("TotalMemory : {0}", GC.GetTotalMemory(false));
        md5.Clear();
        Debug.Print("TotalMemory : {0}", GC.GetTotalMemory(false));
        return hash;
      }
    }


    public static ApplicationTypes ApplicationType { get; set; }
    public static String AppDataDirectory
    {
      get
      {
        var ret = String.Empty;
        if (IsWindows2000 || IsWindowsXP || IsWindows2003)
          ret = String.Format("{0}\\Documents\\Solution", Environment.GetEnvironmentVariable("ALLUSERSPROFILE"));
        else
          ret = String.Format("{0}\\Documents\\Solution", Environment.GetEnvironmentVariable("PUBLIC"));
        ForceCreateDirectory(ret);
        return ret;
      }
    }

    public static Boolean IsWindows2000 { get { return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 0); } }
    public static Boolean IsWindowsXP { get { return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 1); } }
    public static Boolean IsWindows2003 { get { return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 2); } }
    public static Boolean IsWindowsVista { get { return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor == 0); } }
    public static Boolean IsWindows7 { get { return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor == 1); } }
    public static Boolean IsWindows8 { get { return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor == 2); } }
    public static bool IsBrowserApplication { get { return System.Windows.Interop.BrowserInteropHelper.IsBrowserHosted; } }


    public static readonly Random RandomObj = new Random();

    private static CultureInfo _defaultCultureInfo = CultureInfo.CreateSpecificCulture("en");
    public static CultureInfo DefaultCultureInfo { get { return _defaultCultureInfo; } }
  }
  public static class SpecialStrings
  {
    private static List<String> _stringList = null;
    public static List<String> StringList
    {
      get
      {
        if (_stringList == null)
          _stringList = new List<String>() { "=", "!=", "<", ">", "<=", ">=" };
        return _stringList;
      }
    }

    private static Dictionary<String, String> _stringDictionary = null;
    public static Dictionary<String, String> StringDictionary
    {
      get
      {
        if (_stringDictionary == null)
        {
          _stringDictionary = new Dictionary<string, string>();
          _stringDictionary.Add("PpPercent", "PP%");
          _stringDictionary.Add("PcPercent", "PC%");
          _stringDictionary.Add("NSigma3", "-3 Sigma");
          _stringDictionary.Add("Sigma3", "3 Sigma");
          _stringDictionary.Add("Quart75", "75th Quartile");
          _stringDictionary.Add("Range", "Range");
          _stringDictionary.Add("dd_MM_yyyy", "dd-MM-yyyy");
          _stringDictionary.Add("hhmmss", "HH:mm:ss");
          _stringDictionary.Add("HH_mm_ss", "HH-mm-ss");
        }
        return _stringDictionary;
      }
    }
  }
}
