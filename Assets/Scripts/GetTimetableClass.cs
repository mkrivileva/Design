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
    public Text ftext;
    public ScrollRect scrollView;
    public GameObject scrollContent;
    public GameObject scrollItemPrefab;

    public void ClickDateBtn(int state)
    {
        scrollContent.transform.DetachChildren();
        var date = System.DateTime.Now;
        string strdate;
        if (state == 0) // today option
            strdate = date.ToString("dd.MM.yyyy");
        else if (state == 1) //tomorrow option
            strdate = date.AddDays(1).ToString("dd.MM.yyyy");
        else
        {
            //DatePickerController datePicker = new DatePickerController();
            //datePicker.resultDateFormat = "dd.MM.yyyy";
            //datePicker.Show();
            return;
        }
        SendRequest(strdate);
    }

    public void SendRequest(object date)
    {
        ftext.text = date.ToString();
        if (from.text.Length != 0 && to.text.Length != 0)
            StartCoroutine(GetTimetable(date.ToString()));
    }

    private IEnumerator GetTimetable(string date)
    {
        ftext.text = "GetTimetable";
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        string routepath = "https://pass.rzd.ru/suggester?lang=ru&compactMode=y&stationNamePart=";

        //get code from
        string pathFrom = routepath + from.text.ToUpper();
        UnityEngine.Debug.Log(pathFrom);
        UnityWebRequest www = UnityWebRequest.Get(pathFrom);
        yield return www.SendWebRequest();
        if(CheckNetwork(www)) yield break;
        string code0 = GetCurrentCode(www.downloadHandler.text, from.text.ToUpper());
        UnityEngine.Debug.Log(code0);
        ftext.text = code0;

        //get code to
        string pathTo = routepath + to.text.ToUpper();
        UnityEngine.Debug.Log(pathTo);
        www = UnityWebRequest.Get(pathTo);
        yield return www.SendWebRequest();
        if (CheckNetwork(www)) yield break;
        string code1 = GetCurrentCode(www.downloadHandler.text, to.text.ToUpper());
        UnityEngine.Debug.Log(code1);
        ftext.text = code1;



        //get RID
        string pathRID = "https://pass.rzd.ru/timetable/public/ru?layer_id=5827&dir=0&tfl=3&checkSeats=1&code0=" + code0 + "&dt0=" + date + "&code1=" + code1;
        UnityEngine.Debug.Log(pathRID);
        UnityWebRequest wwwRID = UnityWebRequest.Post(pathRID, formData);
        yield return wwwRID.SendWebRequest();
        yield return new WaitForSeconds((float)0.5);
        if (CheckNetwork(wwwRID)) yield break;
        string rid = GetCurrentRID(wwwRID.downloadHandler.text);
        UnityEngine.Debug.Log(wwwRID.downloadHandler.text);
        ftext.text = rid;

        //get TimeTable
        string pathTimeTable = "https://pass.rzd.ru/timetable/public/ru?layer_id=5827&rid=" + rid;
        UnityEngine.Debug.Log(pathTimeTable);
        UnityWebRequest timeTable = UnityWebRequest.Post(pathTimeTable, formData);
        yield return timeTable.SendWebRequest();
        yield return new WaitForSeconds((float)0.5);
        if (CheckNetwork(timeTable)) { ftext.text = "ERR"; yield break; }
        //ftext.text = timeTable.downloadHandler.text;
        UnityEngine.Debug.Log(timeTable.downloadHandler.text);
        ParseTrainData(timeTable.downloadHandler.text);

    }
    string GetCurrentCode(string parseData, string target)
    {
        string[] options = parseData.Split(new[]{'{', '}'});
        
        foreach(string option in options)
        {
            if (option.IndexOf(target) > -1)
            {
                var dict = option.Split(new[] {','})
                .Select(part => part.Split(':'))
                .ToDictionary(split => split[0], split => split[1]);
                return dict["\"c\""];
            }
        }
        return "NULL";
    }

    string GetCurrentRID(string parseData)
    {
        var dict = parseData.Split(new[] { ',' })
                .Select(part => part.Split(':'))
                .ToDictionary(split => split[0], split => split[1]);
        return dict["\"RID\""];
    }

    bool ParseTrainData(string parseData)
    {
        List<string> options = new List<string>(parseData.Split(new[] { '{', '}' }));
        options.RemoveRange(0, 3);
        options.RemoveRange(options.Count() - 5, 4);
        ftext.text = "";
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
                /*
                foreach (var item in dict)
                {
                    ftext.text += item.Key + " " + item.Value + "\n";
                }
                */
            }
            else if (dict.ContainsKey("\"carDataType\"")) cars.Add(dict);
            count++;
            
        }
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

        string timeInWay = train["\"timeInWay"].Replace("\"", string.Empty);

        UnityEngine.Debug.Log("GenerateItem");
        GameObject scrollItemObj = Instantiate(scrollItemPrefab);

        scrollItemObj.transform.Find("TrainName").gameObject.GetComponent<Text>().text = name;

        scrollItemObj.transform.Find("InfoFrom").Find("TimeFrom").gameObject.GetComponent<Text>().text = time0;
        scrollItemObj.transform.Find("InfoFrom").Find("StationFrom").gameObject.GetComponent<Text>().text = date0;
        scrollItemObj.transform.Find("InfoFrom").Find("DateFrom").gameObject.GetComponent<Text>().text = station0;

        scrollItemObj.transform.Find("InfoTo").Find("TimeTo").gameObject.GetComponent<Text>().text = time1;
        scrollItemObj.transform.Find("InfoTo").Find("StationTo").gameObject.GetComponent<Text>().text = date1;
        scrollItemObj.transform.Find("InfoTo").Find("DateTo").gameObject.GetComponent<Text>().text = station1;

        scrollItemObj.transform.Find("Time").Find("TimeInWay").gameObject.GetComponent<Text>().text = timeInWay;

        scrollItemObj.transform.SetParent(scrollContent.transform, false);
    }


    bool CheckNetwork (UnityWebRequest request)
    {
        if (request.isNetworkError)
        {
            ftext.text = "NETWORK ERROR";
            return true;
        }
        else if (request.isHttpError)
        {
            ftext.text = "HTTP ERROR";
            return true;
        }
        return false;
    }
}
