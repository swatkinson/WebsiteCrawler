// Decompiled with JetBrains decompiler
// Type: SaAutomation.BackgroundJobs.SaStopSchedule
// Assembly: SaAutomation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 23B07740-69DA-4017-AF2F-4526F47AF9F0
// Assembly location: E:\Nitin\SaAutomation\Source Code\Compiled\bin\SaAutomation.dll

using Hangfire;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SaAutomation.BL;
using SaAutomation.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace SaAutomation.BackgroundJobs
{
  public class SaStopSchedule
  {
    private static SeleniumFacade selenium = new SeleniumFacade();
    private static string BaseUrl = "https://www.seekingarrangement.com";

    private static LoginConfigViewModel DeserializeFromString<LoginConfigViewModel>(string settings)
    {
      using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(settings)))
      {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        memoryStream.Seek(0L, SeekOrigin.Begin);
        return (LoginConfigViewModel) binaryFormatter.Deserialize((Stream) memoryStream);
      }
    }

    public static void FindAndStop()
    {
      ChromeOptions options = new ChromeOptions();
      options.AddArguments((IEnumerable<string>) new List<string>());
      ChromeDriver driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options);
      try
      {
        SaStopSchedule.selenium.Navigate(driver, SaStopSchedule.BaseUrl + "/messages/inbox");
        driver.FindElement(By.CssSelector("label[for=unread]")).Click();
        SaStopSchedule.selenium.SleepRandom();
        ReadOnlyCollection<IWebElement> elements = driver.FindElements(By.CssSelector("a.u-fauxBlockLink-overlay"));
        List<string> stringList = new List<string>();
        SaContext saContext = new SaContext();
        foreach (IWebElement webElement in elements)
        {
          string attribute = webElement.GetAttribute("href");
          SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(new SqlCommand("select Id from Hangfire.Job Where Arguments like '%" + attribute.Substring(attribute.LastIndexOf("/") + 1, attribute.Length - attribute.LastIndexOf("/") - 1) + "%'", (SqlConnection) saContext.Database.Connection));
          DataSet dataSet = new DataSet();
          sqlDataAdapter.Fill(dataSet);
          if (dataSet.Tables.Count > 0)
          {
            foreach (DataRow dataRow in dataSet.Tables[0].Rows.Cast<DataRow>())
              BackgroundJob.Delete(Convert.ToString(dataRow["Id"]));
          }
        }
      }
      catch (Exception ex)
      {
      }
      finally
      {
        driver.Quit();
      }
    }
  }
}
