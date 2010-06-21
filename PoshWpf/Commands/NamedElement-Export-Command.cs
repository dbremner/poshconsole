﻿using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace PoshWpf
{
   [Cmdlet(VerbsData.Export, "NamedElement", SupportsShouldProcess = false, ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = ByElement)]
   public class ExportNamedElementCommand : ScriptBlockBase
   {
      private const string ByTitle = "ByTitle";
      private const string ByIndex = "ByIndex";
      private const string ByElement = "ByElement";

      //[Parameter(Position = 0, Mandatory = true, ParameterSetName = ByElement, ValueFromPipeline = true)]
      //[ValidateNotNull]
      //[Alias("Window")]
      //public UIElement Element { get; set; }

      [Parameter(Position = 1, Mandatory = false)]
      [ValidateNotNullOrEmpty]
      public string Prefix { get; set; }

      private string _scope = "0";
      [Parameter(Mandatory = false)]
      [ValidateNotNullOrEmpty]
      [ValidatePattern("^(?:\\d+|Local|Script|Global|Private)$")]
      public string Scope
      {
         get { return _scope; }
         set { _scope = value; }
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
      protected override void ProcessRecord()
      {
         try
         {
            var active = Thread.CurrentThread;
            if (BootsWindowDictionary.Instance.Count > 0)
            {
               foreach (var window in BootsWindowDictionary.Instance.Values)
               {
                  if(window.Dispatcher.Thread == active)
                  {
                     ExportVisual(window, Scope);
                  }
               }
            }
         }
         catch (Exception ex)
         {
            WriteError(new ErrorRecord(ex, "TrappedException", ErrorCategory.NotSpecified, Thread.CurrentThread));
         }
         base.ProcessRecord();
      }

      // Enumerate all the descendants of the visual object.
      public static void ExportVisual(Visual myVisual, string scope)
      {
         for (int i = 0; i < VisualTreeHelper.GetChildrenCount(myVisual); i++)
         {
            // Retrieve child visual at specified index value.
            Visual childVisual = (Visual)VisualTreeHelper.GetChild(myVisual, i);
            if(childVisual is FrameworkElement)
            {
               // Do processing of the child visual object.
               string name = childVisual.GetValue(FrameworkElement.NameProperty) as string;
               if(!string.IsNullOrEmpty(name))
               {
                  Invoker.SetScriptVariableValue(name,childVisual, scope);
               }
            }
            // Enumerate children of the child visual object.
            ExportVisual(childVisual, scope);
         }
      }

   }
}
