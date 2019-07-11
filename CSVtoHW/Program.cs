using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Siemens.Engineering;
using Siemens.Engineering.Cax;
using Siemens.Engineering.HW;

namespace HwGen
{
  class HWDevice
  {
    public string group;
    public string field;
    public string name;
    public string article_number;
    public string addres_range;
    public string comment;
    public HWDevice(string device_group,
      string device_field,
     string device_name,
     string device_article_number,
      string device_address_range,
     string device_comment)
    {
      group = device_group;
      field = device_field;
      name = device_name;
      article_number = device_article_number;
      addres_range = device_address_range;
      comment = device_comment;
    }
  }

  class TiaProject
  {
    TiaPortal tiaPortal;
    Project project;
    string path;
    string name;

    public TiaProject(string project_name, string project_path, bool WithGUI = false)
    {
      name = project_name;
      path = project_path;
      Console.WriteLine("Connecting to TIA Portal...");
      if (WithGUI)
        try
        {

          tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);
        } catch (Exception e)
        {
          Console.WriteLine("{0}", e.ToString());
        } else
      {
        try
        {
          tiaPortal = new TiaPortal(TiaPortalMode.WithoutUserInterface);
        } catch (Exception e)
        {
          Console.WriteLine("{0}", e.ToString());
        }
      }
      if (tiaPortal != null)
      {
        Console.WriteLine("Connection is OK!");
        Console.WriteLine("Creating Project...");
        CreateProject();
        if (project != null)
        {
          Console.WriteLine("The project is created!");
        }
      }
    }
    ~TiaProject()
    {

    }
    public void CloseProject()
    {
      if (tiaPortal != null)
      {

        Console.WriteLine("Closing project...");
        project.Close();
        Console.WriteLine("Project closed!");
        Console.WriteLine("Disconnecting from TIA Portal...");
        tiaPortal.Dispose();
        Console.WriteLine("TIA Portal disconnected!");
      }
    }
    public void SaveProject()
    {
      if (tiaPortal != null)
      {
        Console.WriteLine("Saving project...");
        project.Save();
        Console.WriteLine("Save OK!");
      }
    }
    public void CreateProject()
    {
      if (tiaPortal != null)
      {
        DirectoryInfo projectDirectory = new DirectoryInfo(path);
        try
        {
          project = tiaPortal.Projects.Create(@projectDirectory, name);

        } catch (Exception e)
        {
          Console.WriteLine("Could not create project! Error: {0}", e.ToString());
          Console.ReadKey();

        }
      }


    }
    public Device CreateDevice(string device_name, string article_number)
    {
      try
      {
        Device device = project.Devices.Create(article_number, device_name);
        return device;
      } catch (Exception e)
      {
        Console.WriteLine("Could not create device! Error: {0}", e.ToString());
        Console.ReadKey();

      }
      return null;
    }
    public bool ImportFromCSV(string csv_file_name)
    {
      string[] raw_lines = { };
      string[] headers;

      var device_list = new List<HWDevice>();

      try
      {
        raw_lines = File.ReadAllLines(csv_file_name);
        string[] device_lines = new string[raw_lines.Length - 1];
        headers = raw_lines[0].Split(',');
        Array.Copy(raw_lines, 1, device_lines, 0, raw_lines.Length - 1);

        foreach (string line in device_lines)
        {
          Console.WriteLine(line);
          HWDevice hw_device;
          string[] properties = line.Split(',');
          string group = properties[0];
          string field = properties[1];
          string name = properties[2];
          string article_number = properties[3];
          string address_range = properties[4];
          string comment = properties[5];
          hw_device = new HWDevice(group, field, name, article_number, address_range, comment);
          device_list.Add(hw_device);

        }
      } catch (Exception e)
      {
        Console.WriteLine("{0}", e.ToString());
        Console.ReadKey();
        return false;

      }


      Console.WriteLine("The project is created!");
      Console.WriteLine("Adding devices...");
      foreach (HWDevice device in device_list)
      {
        try
        {
          CreateDevice(device.group + device.field + device.name, device.article_number);
        } catch (Exception e)
        {
          Console.WriteLine("Can't add device {0}:{1}", device.group + device.field + device.name, e.ToString());
          Console.ReadKey();
          return false;
        }
        Console.WriteLine("Device {0} added!", device.group + device.field + device.name);
      }
      Console.WriteLine("All devices added!");
      return true;

    }
    public bool ImportFromAML(string aml_file_name)
    {
      if (tiaPortal != null)
      {
        Console.WriteLine("Connecting to CAX Provider...");
        CaxProvider caxProvider = project.GetService<CaxProvider>();
        FileInfo amlFileInfo = new FileInfo(Path.GetFullPath(aml_file_name));
        FileInfo logFileInfo = new FileInfo(Path.GetTempPath() + Path.GetFileNameWithoutExtension(aml_file_name) + ".log");
        Console.WriteLine(logFileInfo.ToString());
        if (caxProvider != null)
        {
          Console.WriteLine("Connection to CAX Provider is OK!");
          Console.WriteLine("Importing AML to the project... It might take a while. Please wait!");
          try
          {
            caxProvider.Import(amlFileInfo, logFileInfo, CaxImportOptions.RetainTiaDevice);
          } catch (Exception e)
          {
            Console.WriteLine("Error until import: {0}", e.ToString());
            Console.ReadKey();
            return false;
          }

          Console.WriteLine("Import is done!");
          return true;
        } else

        {
          Console.WriteLine("Failed connect to CAX Provider!");
          Console.ReadKey();
          return false;
        }
      } else
        return false;
    }

  }



  class Program
  {


    static void Main(string[] args)
    {
      TiaProject tiaProject;
      //AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
      string filename = @"";
      string path_to_file = @"";
      DateTime dateTime = DateTime.Now;
      string date = dateTime.Year.ToString() + dateTime.Month.ToString().PadLeft(2, '0') + dateTime.Day.ToString().PadLeft(2, '0') + dateTime.Hour.ToString().PadLeft(2, '0') + dateTime.Minute.ToString().PadLeft(2, '0');
      string project_name = @"AutogeneratedProject_" + date;
      string project_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      bool with_gui=false;
      foreach (string key in args)
      {
        switch (key)
        {
          case "-p":
               project_name = GetArgumentValue(args, key);
            break;
          case "-d":
            project_path = GetArgumentValue(args, key);
            break;
          case "-g":
            with_gui = true;
            break;
        }
        
      }
      tiaProject = new TiaProject(project_name, project_path, with_gui);

      // Handling arguments
      foreach (string key in args)
      {
        switch (key)
        {
          //Input file is AML
          case "-a":
            path_to_file = GetArgumentValue(args, key);
            filename = Path.GetFileName(path_to_file);

            if (!CheckFile(path_to_file, ".aml", true))
            {
              Environment.Exit(1);
            } else
            {
              tiaProject.ImportFromAML(path_to_file);
            }
            break;
          //Input file is CSV
          case "-c":
            path_to_file = GetArgumentValue(args, key);
            if (!CheckFile(path_to_file, ".csv", true))
            {
              Environment.Exit(1);
            } else
            {
              tiaProject.ImportFromCSV(path_to_file);
            }
            break;
          //Input file is JSON
          case "-j":
            path_to_file = GetArgumentValue(args, key);
            if (!CheckFile(path_to_file, ".json", true))
            {
              Environment.Exit(1);
            }
            break;
          default:
            break;
        }

      }
      if (tiaProject != null)
      {
        tiaProject.SaveProject();
        tiaProject.CloseProject();
      }


      Console.WriteLine("Press any key to close application!");
      Console.ReadKey();
    }


    static string GetArgumentValue(string[] args, string arg)
    {
      string argument_value = "";
      IEnumerator args_enum = args.GetEnumerator();
      while (args_enum.MoveNext())
      {

        if (args_enum.Current.ToString() == arg)
        {
          args_enum.MoveNext();
          argument_value = args_enum.Current.ToString();
          break;
        }
      }
      return argument_value;

    }

    // checking a file
    static bool CheckFile(string path_to_file, string extension, bool checkExist)
    {
      string filename = Path.GetFileName(path_to_file);
      if (checkExist)
      {
        if (!new FileInfo(path_to_file).Exists)
        {
          Console.WriteLine("File does not exist!");
          Console.ReadKey();
          return false;
        }
      }
      if (Path.GetExtension(filename) != extension)
      {
        Console.WriteLine("Wrong file extension!");
        Console.ReadKey();
        return false;
      }
      return true;
    }
  }

}














