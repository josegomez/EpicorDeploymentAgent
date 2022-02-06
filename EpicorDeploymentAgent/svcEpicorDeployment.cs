﻿using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EpicorDeploymentAgent
{
    public partial class svcEpicorDeployment : ServiceBase
    {
        Thread theMain;
        bool killSvc = false;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public svcEpicorDeployment()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            StartWatch();
        }

        private void StartWatch()
        {
            killSvc = false;
            theMain = new Thread(RunSvc);
            theMain.Start();
        }

        public void Start()
        {
            StartWatch();
        }

      
        
        

        private void ProcessFile(string fileName)
        {
            
        }

        protected override void OnStop()
        {
            killSvc = true;
        }

        private void RunSvc()
        {
            while(!killSvc)
            {
                foreach(var x in Directory.GetFiles(Settings.Default.DropPath))
                {
                    StringBuilder sbGeneric = new StringBuilder();
                    if(Path.GetExtension(x).Equals(".cab",StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info($"Found File: {x} Processing...");
                        Parallel.ForEach(Settings.Default.EpicorProdSysConfigs.Cast<string>(), (config)=> {
                            StringBuilder sbOutput = new StringBuilder();
                            sbOutput.AppendLine("******************************************************************");
                            sbOutput.AppendLine($"Importing Cab {x}");
                            sbOutput.AppendLine($"Into Environment {config}");
                            Process proc = new Process()
                            {
                                StartInfo = new ProcessStartInfo()
                                {
                                    FileName = Settings.Default.EpicorSolutionPathProd,
                                    Arguments = $"install /cabPath=\"{x}\" /userID=\"{Settings.Default.EpicorUser}\" /password=\"{Settings.Default.EpicorPassword}\" /config=\"{config}\" /logLevel={Settings.Default.LogLevel} \"",
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    CreateNoWindow = true
                                }
                            };
                            proc.Start();
                            
                            
                            sbOutput.AppendLine(proc.StandardOutput.ReadToEnd());
                            sbOutput.AppendLine(proc.StandardError.ReadToEnd());
                            proc.WaitForExit();

                            lock(sbGeneric)
                            {
                                sbGeneric.AppendLine(sbOutput.ToString());
                                sbGeneric.AppendLine("******************************************************************");
                            }
                        });

                        Parallel.ForEach(Settings.Default.EpicorDevSysConfigs.Cast<string>(), (config) => {
                            StringBuilder sbOutput = new StringBuilder();
                            sbOutput.AppendLine("******************************************************************");
                            sbOutput.AppendLine($"Importing Cab {x}");
                            sbOutput.AppendLine($"Into Environment {config}");
                            Process proc = new Process()
                            {
                                StartInfo = new ProcessStartInfo()
                                {
                                    FileName = Settings.Default.EpicorSolutionPathDev,
                                    Arguments = $"install /cabPath=\"{x}\" /userID=\"{Settings.Default.EpicorUser}\" /password=\"{Settings.Default.EpicorPassword}\" /config=\"{config}\" /logLevel={Settings.Default.LogLevel} \"",
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    CreateNoWindow = true
                                }
                            };
                            proc.Start();


                            sbOutput.AppendLine(proc.StandardOutput.ReadToEnd());
                            sbOutput.AppendLine(proc.StandardError.ReadToEnd());
                            proc.WaitForExit();

                            lock (sbGeneric)
                            {
                                sbGeneric.AppendLine(sbOutput.ToString());
                                sbGeneric.AppendLine("******************************************************************");
                            }
                        });

                        logger.Info(sbGeneric.ToString());
                        
                    }
                    File.Delete(x);
                }
                LogManager.Flush();
                Thread.Sleep(60000);
            }

        }
    }
}
