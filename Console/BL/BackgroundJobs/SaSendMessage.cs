// Decompiled with JetBrains decompiler
// Type: SaAutomation.BackgroundJobs.SaSendMessage
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
using System.Linq;

namespace SaAutomation.BackgroundJobs
{
    public class SaSendMessage
    {
        private static SeleniumFacade selenium = new SeleniumFacade();
        private static string BaseUrl = "https://www.seekingarrangement.com";

        public static void Send(List<string> profileids, string batchId, string message, string username, string password)
        {
            ChromeOptions options = new ChromeOptions();
            SaContext saContext = new SaContext();
            options.AddArguments((IEnumerable<string>)new List<string>());
            ChromeDriver driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options);
            try
            {
                foreach (string profileid in profileids.Where(x=>x!= "proxy"))
                {
                    driver.FindElement(By.CssSelector("body")).SendKeys(Keys.Control + "t");
                    ReadOnlyCollection<string> windowHandles = driver.WindowHandles;
                    driver.SwitchTo().Window(windowHandles.ElementAt<string>(0));
                    SaSendMessage.selenium.Navigate(driver, SaSendMessage.BaseUrl + "/member/" + profileid);
                    SaSendMessage.selenium.SleepRandom();
                    driver.FindElementByClassName("btn-mobile-padding").Click();
                    SaSendMessage.selenium.SleepRandom();
                    //send message only if conversation is not turbulent
                    bool IsTurbulent = false;

                    var objLastSent = saContext.SaTimes.Where(x => x.ProfileId == profileid && x.batchId==batchId ).FirstOrDefault();
                    if (objLastSent != null)
                    {
                        var LastSent = objLastSent.LastSentTime;
                        var abbr = driver.FindElementsByClassName("timeago").Last().FindElement(By.TagName("time"));
                        var CurSent = abbr.GetAttribute("datetime");

                        if (CurSent != LastSent)
                        {
                            IsTurbulent = true;
                        }
                    }

                    if (IsTurbulent == false)
                    {
                        driver.FindElementById(nameof(message)).SendKeys(message);
                        SaSendMessage.selenium.SleepRandom();
                        driver.FindElementByClassName("MessageForm-send").Click();
                        SaSendMessage.selenium.SleepRandom();

                        driver.Navigate().Refresh();
                        SaSendMessage.selenium.SleepRandom();

                        var abbr = driver.FindElementsByClassName("timeago").Last().FindElement(By.TagName("time"));
                        var CurSent = abbr.GetAttribute("datetime");
                        if (objLastSent != null)
                        {
                            objLastSent.LastSentTime = CurSent;
                            saContext.Entry<SaTimes>(objLastSent).State = System.Data.Entity.EntityState.Modified;
                        }
                        else {
                            SaTimes time = new SaTimes();
                            time.ProfileId = profileid;
                            time.LastSentTime = CurSent;
                            time.batchId = batchId;
                            saContext.SaTimes.Add(time);
                        }
                        saContext.SaveChanges();
                    }
                    else {
                        //stop all future sending of messages(if any)
                        var con = (SqlConnection)saContext.Database.Connection;
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(new SqlCommand("select * from Hangfire.Job Where Arguments like '%" + profileid + "%'", con));
                        DataSet dataSet = new DataSet();
                        sqlDataAdapter.Fill(dataSet);
                        if (dataSet.Tables.Count > 0)
                        {
                            var JobIDs = (from p in dataSet.Tables[0].Rows.Cast<DataRow>()
                                         select new { Id = p["Id"].ToString(), Arguments = p["Arguments"].ToString() }).ToList();
                            //remove profileid from the JObs
                            foreach (var Job in JobIDs)
                            {
                                var id = Job.Id;
                                var arguments = Job.Arguments.Replace(profileid,"proxy");
                                SqlCommand cmd = new SqlCommand("UPDATE Hangfire.Job SET Arguments=@args where Id=@id");
                                cmd.Parameters.AddWithValue("@args", arguments);
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.Connection = con;
                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                            }
                        }
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

        public static void Send(string profileId, string message, string username, string password)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments((IEnumerable<string>)new List<string>());
            ChromeDriver driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options);
            try
            {
                SaSendMessage.selenium.Navigate(driver, SaSendMessage.BaseUrl + "/member/" + profileId);
                SaSendMessage.selenium.SleepRandom();
                driver.FindElementByClassName("btn-mobile-padding").Click();
                SaSendMessage.selenium.SleepRandom();
                driver.FindElementById(nameof(message)).SendKeys(message);
                driver.FindElementByClassName("MessageForm-send").Click();
                SaSendMessage.selenium.SleepRandom();
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
