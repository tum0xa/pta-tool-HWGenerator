using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Siemens.Engineering;
using Siemens.Engineering.Cax;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Extensions;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HW.Utilities;
using Siemens.Engineering.Connection;

namespace HwGen
{
  class HWDevice
  {
    public string group;
    public string field;
    public string name;
    public string parent;
    public string article_number;
    public string version;
    public string input_start_address;
    public string subnet;
    public string subnet_type;
    public string comment;

    public HWDevice(string device_group,
      string device_field,
     string device_name,
     string device_parent,
     string device_article_number,
     string device_version,
      string device_input_start_address,
      string device_subnet_type,
      string device_subnet,
      
     string device_comment)
    {
      group = device_group;
      field = device_field;
      version = device_version;
      name = device_name;
      parent = device_parent;
      article_number = device_article_number;
      input_start_address = device_input_start_address;
      subnet_type = device_subnet_type;
      subnet = device_subnet;
      comment = device_comment;
    }
  }

  class TiaProject
  {
    TiaPortal tiaPortal;
    public Project project;
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
    public bool CreateProject()
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
          return false;
        }
        return true;
      } else
      {
        return false;
      }


    }
    public bool CreateDevice(HWDevice device)
    {
      Device newDevice = null;
      string typeIndentifier;
      string device_name = device.group + device.field + device.name;
      int child_slot;
      try
      {
        if (device.version != "")
          device.version = "/" + device.version;
        typeIndentifier = "OrderNumber:" + device.article_number + device.version;
        Console.WriteLine(typeIndentifier + ". Parent: {0}", device.parent);

        if (device.parent == "-")
        {

          newDevice = project.Devices.CreateWithItem(typeIndentifier, device_name, device_name);
          if (newDevice != null)
          {
            Console.WriteLine("Device {0} created! ", device_name);
          }
        } else
        {
          HardwareObject parentDevice = project.Devices.Find(device.group + device.field + device.parent);
          DeviceItem parentRack = parentDevice.DeviceItems[0];

          if (parentRack != null)
          {
            Console.WriteLine(GetFreePlugPosition(parentRack));
            child_slot = GetFreePlugPosition(parentRack);
            if (parentRack.CanPlugNew(typeIndentifier, device_name, child_slot))
            {
              parentRack.PlugNew(typeIndentifier, device_name, child_slot);
              DeviceItem childDevice = parentRack.Items[child_slot - 1];
              if (childDevice != null)
              {

                Console.WriteLine("Device {0} created! ", childDevice.Name);
              }

            }

          }

        }

      } catch (Exception e)
      {
        Console.WriteLine("Could not create device {1}! Error: {0}", e.ToString(), device_name);
        Console.ReadKey();
        return false;
      }
      return true;
    }
    public bool CreateSubnet(string subnet_type, string subnet_name)
    {
      try
      {
        Subnet subnet = project.Subnets.Create(GetSubnetTypeIndetifier(subnet_type), subnet_name);
        return true;
      } catch (Exception e)
      {
        Console.WriteLine("Failed to create subnet {0}:{1}", subnet_name, e.ToString());
      }
      return false;
    }
    public bool ConnectToSubnet(Device device, Subnet subnet)
    {
      NetworkInterface connectionInterface;
      foreach (DeviceItem item in device.DeviceItems)
        
        if (item.Name == device.Name)
          foreach (DeviceItem netInterface in item.DeviceItems)
          {
            if (netInterface.Name.Contains(GetSubnetType(subnet)))
            {
              connectionInterface = netInterface.GetService<Siemens.Engineering.HW.Features.NetworkInterface>();
              
              Node node = connectionInterface.Nodes[0];
              node.ConnectToSubnet(subnet);
              
              //IoSystem ioSystem = subnet.IoSystems.;
              //IoController ioController = connectionInterface.IoControllers[0];
              
              return true;
            }
          }
      return false;
    }
    public bool SetProperty(DeviceItem childDevice, int inputStartAddress, int outputStartAddress)
    {
      
      AddressController addressController =((IEngineeringServiceProvider)childDevice).GetService<AddressController>();
      AddressComposition addresses = childDevice.Addresses;
      //Address InAddress = ((AddressComposition)addresses).
      return false;
    }
    public bool ImportFromCSV(string csv_file_name)
    {
      string[] raw_lines = { };
      string[] headers;

      var device_list = new List<HWDevice>();

      try
      {
        Console.WriteLine("Reading CSV...");
        raw_lines = File.ReadAllLines(csv_file_name);
        string[] device_lines = new string[raw_lines.Length - 1];
        headers = raw_lines[0].Split(',');
        Array.Copy(raw_lines, 1, device_lines, 0, raw_lines.Length - 1);
        foreach (string line in device_lines)
        {
          HWDevice hw_device;
          string[] properties = line.Split(',');
          string group = properties[0];
          string field = properties[1];
          string name = properties[2];
          string parent = properties[3];
          string article_number = properties[4];
          string version = properties[5];
          string input_start_address = properties[6];
          string subnet_type = properties[7];
          string subnet = properties[8];
          string comment = properties[9];
          hw_device = new HWDevice(group, field, name, parent, article_number, version, input_start_address, subnet_type, subnet, comment);
          device_list.Add(hw_device);
        }
      } catch (Exception e)
      {
        Console.WriteLine("{0}", e.ToString());
        Console.ReadKey();
        return false;
      }
      Console.WriteLine("Adding devices...");
      foreach (HWDevice device in device_list)
      {
        Subnet device_subnet = project.Subnets.Find(device.subnet);
        if (device_subnet == null)  
          project.Subnets.Create(GetSubnetTypeIndetifier(device.subnet_type), device.subnet);
        device_subnet = project.Subnets.Find(device.subnet);
        CreateDevice(device);
        if (device.parent == "-")
        {
          Device cur_device = project.Devices.Find(device.group + device.field + device.name);
          ConnectToSubnet(cur_device, device_subnet);
          }
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
    static int GetFreePlugPosition(HardwareObject device)
    {
      foreach (PlugLocation item in device.GetPlugLocations())
      {
        return item.PositionNumber;
      }
      return -1;
    }
    static string GetSubnetTypeIndetifier(string subnet_type)
    {
      subnet_type = subnet_type.ToLower();
      switch (subnet_type)
      {
        
        case "pn":
        case "profinet":
        case "pthernet":
          return "System:Subnet.Ethernet";
        case "pb":
        case "profibus":
          return "System:Subnet.Profibus";
        case "mpi":
          return "System:Subnet.Mpi";
        case "asi":
          return "System:Subnet.Asi";
        default:
          return "System:Subnet.Ethernet";
      }
      
    }
    static string GetSubnetType(Subnet subnet)
    {
      switch(subnet.NetType){
        case NetType.Ethernet:
          return "PROFINET";
        case NetType.Profibus:
          return "DB";

        default:
          return "PROFINET";
      }
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
          case "-f":
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
          case "-d": //Debug feature
            HWDevice test_device = new HWDevice("04", "+UH00", "-K100", "-", "6ES7 516-3FN01-0AB0", "V2.5", "-", "PN", "PN1", "Comment");

            tiaProject.CreateSubnet("PN", "PN1");
            tiaProject.CreateDevice(test_device);
            Device sel_device = tiaProject.project.Devices.Find("04+UH00-K100");
            Subnet subnet = tiaProject.project.Subnets.Find("PN1");
            tiaProject.ConnectToSubnet(sel_device, subnet);


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














