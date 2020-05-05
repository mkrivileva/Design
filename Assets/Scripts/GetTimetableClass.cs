using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Diagnostics;
using System.Security.Policy;
using FantomLib;

public class GetTimetableClass : MonoBehaviour
{
    public InputField from;
    public InputField to;
    public Text alert;
    public ScrollRect scrollView;
    public GameObject scrollContent;
    public GameObject scrollItemPrefab;

    public void ClickDateBtn(int state)
    {
        
        var date = System.DateTime.Now;
        string strdate;
        if (state == 0) // today option
            strdate = date.ToString("dd.MM.yyyy");
        else //tomorrow option
            strdate = date.AddDays(1).ToString("dd.MM.yyyy");
        SendRequest(strdate);
    }

    public void SendRequest(object date)
    {
        scrollContent.transform.DetachChildren();
        if (from.text.Length == 0)
        {
            alert.text = "Введите пункт отправления";
            return;
        }
        if (to.text.Length == 0)
        {
            alert.text = "Введите пункт назначения";
            return;
        }
        StartCoroutine(GetTimetable(date.ToString()));
    }

    private IEnumerator GetTimetable(string date)
    {
        alert.text = "загрузка...";
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        string routepathCode = "https://pass.rzd.ru/suggester?lang=ru&compactMode=y&stationNamePart=";

        //get code from
        string pathFrom = routepathCode + from.text.ToUpper();
        UnityWebRequest wwwCode = UnityWebRequest.Get(pathFrom);
        yield return wwwCode.SendWebRequest();
        if(IsError(wwwCode)) yield break;
        string code0 = GetCurrentCode(wwwCode.downloadHandler.text, from.text.ToUpper());
        if (code0 == "err")
        {
            alert.text = "Неправильно введён пункт отправления";
            yield break;
        }
        UnityEngine.Debug.Log(wwwCode.downloadHandler.text);
        UnityEngine.Debug.Log("code from = " + code0);

        //get code to
        string pathTo = routepathCode + to.text.ToUpper();
        wwwCode = UnityWebRequest.Get(pathTo);
        yield return wwwCode.SendWebRequest();
        if (IsError(wwwCode)) yield break;
        string code1 = GetCurrentCode(wwwCode.downloadHandler.text, to.text.ToUpper());
        if (code1 == "err")
        {
            alert.text = "Неправильно введён пункт назначения";
            yield break;
        }
        UnityEngine.Debug.Log("code to = " + code1);



        //get RID
        string pathRID = "https://pass.rzd.ru/timetable/public/ru?layer_id=5827&dir=0&tfl=3&checkSeats=1&code0=" + code0 + "&dt0=" + date + "&code1=" + code1;
        UnityWebRequest wwwRID = UnityWebRequest.Post(pathRID, formData);
        yield return wwwRID.SendWebRequest();
        yield return new WaitForSeconds(1);
        if (IsError(wwwRID)) yield break;
        UnityEngine.Debug.Log(wwwRID.downloadHandler.text);
        string rid = GetCurrentRID(wwwRID.downloadHandler.text);
        if (rid == "err")
            yield break;

        //get TimeTable
        string pathTimeTable = "https://pass.rzd.ru/timetable/public/ru?layer_id=5827&rid=" + rid;
        UnityWebRequest timeTable = UnityWebRequest.Post(pathTimeTable, formData);
        yield return timeTable.SendWebRequest();
        if (IsError(timeTable)) yield break;
        UnityEngine.Debug.Log(timeTable.downloadHandler.text);
        ParseTrainData(timeTable.downloadHandler.text, date);

    }
    string GetCurrentCode(string parseData, string target)
    {
        string[] options = parseData.Split(new[]{'{', '}'});
        
        foreach(string option in options)
        {
            if (option.IndexOf(target + "\"") > -1 || option.IndexOf(target + " (ВСЕ ВОКЗАЛЫ)") > -1)
            {
                var dict = option.Split(new[] {','})
                    .Select(part => part.Split(':'))
                    .ToDictionary(split => split[0], split => split[1]);
                return dict["\"c\""];
            }
        }
        return "err";
    }

    string GetCurrentRID(string parseData)
    {
        if (parseData.IndexOf("\"message\"") > -1)
        {
            parseData = parseData.Remove(0, parseData.IndexOf("\"message\"") + 11);
            parseData = parseData.Remove(parseData.IndexOf("\","));
            alert.text = parseData;
            return "err";

        }
        var dict = parseData.Split(new[] { ',' })
                .Select(part => part.Split(':'))
                .ToDictionary(split => split[0], split => split[1]);
        return dict["\"RID\""];
    }

    bool ParseTrainData(string parseData, string date)
    {
        List<string> options = new List<string>(parseData.Split(new[] { '{', '}' }));
        options.RemoveRange(0, 3);
        options.RemoveRange(options.Count() - 5, 4);
        int count = 0;
        Dictionary<string, string> dict, train = new Dictionary<string, string>();
        List<Dictionary<string, string>> cars = new List<Dictionary<string, string>>();

        foreach (string option in options)
        {
            if (option.Length < 3 || option[0] == ']') continue;
            UnityEngine.Debug.Log(option);
            dict = option.Split(new[] { ',' })
                .Select(part => part.Split(new string[] { "\":" }, System.StringSplitOptions.RemoveEmptyEntries))
                .ToDictionary(split => split[0], split => split[1]);
            if (dict.ContainsKey("\"number"))
            {
                if (count > 0)
                {
                    GenerateItem(train, cars);
                    cars.Clear();
                }
                train = dict;
            }
            else if (dict.ContainsKey("\"carDataType")) cars.Add(dict);
            else if (dict.ContainsKey("\"message"))
            {
                alert.text = dict["\"message"].Replace("\"", string.Empty) + "\nВыбранная дата: " + date;
                return true;
            }
            count++;
            
        }
        alert.text = "";
        GenerateItem(train, cars);
        return true;
    }

    void GenerateItem(Dictionary<string, string> train, List<Dictionary<string, string>> cars)
    {
        string name = "Поезд №" + train["\"number"].Replace("\"", string.Empty) + " " + train["\"trainName"].Replace("\"", string.Empty);

        string time0 = train["\"time0"].Replace("\"", string.Empty);
        string date0 = train["\"date0"].Replace("\"", string.Empty);
        string station0 = train["\"station0"].Replace("\"", string.Empty);

        string time1 = train["\"time1"].Replace("\"", string.Empty);
        string date1 = train["\"date1"].Replace("\"", string.Empty);
        string station1 = train["\"station1"].Replace("\"", string.Empty);

        string timeInWay = train["\"timeInWay"].Replace("\"", string.Empty).Replace(":", "ч. ") + "мин.";

        UnityEngine.Debug.Log("GenerateItem");
        GameObject scrollItemObj = Instantiate(scrollItemPrefab);

        scrollItemObj.transform.Find("TrainName").gameObject.GetComponent<Text>().text = name;

        scrollItemObj.transform.Find("InfoFrom").Find("TimeFrom").gameObject.GetComponent<Text>().text = time0;
        scrollItemObj.transform.Find("InfoFrom").Find("DateFrom").gameObject.GetComponent<Text>().text = date0;
        scrollItemObj.transform.Find("InfoFrom").Find("StationFrom").gameObject.GetComponent<Text>().text = station0;

        scrollItemObj.transform.Find("InfoTo").Find("TimeTo").gameObject.GetComponent<Text>().text = time1;
        scrollItemObj.transform.Find("InfoTo").Find("DateTo").gameObject.GetComponent<Text>().text = date1;
        scrollItemObj.transform.Find("InfoTo").Find("StationTo").gameObject.GetComponent<Text>().text = station1;

        scrollItemObj.transform.Find("Time").Find("TimeInWay").gameObject.GetComponent<Text>().text = timeInWay;

        scrollItemObj.transform.Find("MoreInfo").gameObject.SetActive(false);

        scrollItemObj.transform.SetParent(scrollContent.transform, false);
    }


    bool IsError (UnityWebRequest request)
    {
        if (request.isNetworkError)
        {
            alert.text = "Проверте ваше подключение к сети";
            return true;
        }
        else if (request.isHttpError)
        {
            alert.text = "Ошибка подключения к серверу. Пожалуйста, повторите попытку позже";
            return true;
        }
        return false;
    }
}
