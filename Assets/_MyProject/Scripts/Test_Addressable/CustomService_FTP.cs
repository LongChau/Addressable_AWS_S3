using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Resources;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets.HostingServices;
using UnityEditor.AddressableAssets.Settings;

public class CustomService_FTP : IHostingService
{
    public List<string> HostingServiceContentRoots { get; }

    public Dictionary<string, string> ProfileVariables { get; }

    public bool IsHostingServiceRunning { get; }

    public ILogger Logger { get; set; }
    public string DescriptiveName { get; set; }
    public int InstanceId { get; set; }

    public string EvaluateProfileString(string key)
    {
        throw new NotImplementedException();
    }

    public void OnAfterDeserialize(KeyDataStore dataStore)
    {
        throw new NotImplementedException();
    }

    public void OnBeforeSerialize(KeyDataStore dataStore)
    {
        throw new NotImplementedException();
    }

    public void OnGUI()
    {
        throw new NotImplementedException();
    }

    public void StartHostingService()
    {
        throw new NotImplementedException();
    }

    public void StopHostingService()
    {
        throw new NotImplementedException();
    }
}
#endif
