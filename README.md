# GPSConverter

```C#
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                string parameter = string.Concat(args);
                
                switch (parameter)
                {
                    case "--install":
                        string pw = AesOperation.DecryptString(key3, key2);

                        ManagedInstallerClass.InstallHelper(new string[] { Encoding.UTF8.GetString(Convert.FromBase64String(key1)), 
                                                                           pw, 
                                                                           Assembly.GetExecutingAssembly().Location });
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;
                    default:
                        {
                            var p = new LocationService();
                            p.Init();
                            p.Run();
                            Thread.Sleep(Timeout.Infinite);
                            break;
                        }
                }                
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new LocationService()
                };
                Run(ServicesToRun);
            }
        }
```
