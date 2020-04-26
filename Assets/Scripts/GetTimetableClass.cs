using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Diagnostics;

public class GetTimetableClass : MonoBehaviour
{
    public InputField from;
    public InputField to;
    public Text ftext;

    public void Start()
    {
        ftext.text = "Start";
        //Debug.Log("Start");
    }

    public void SetGet(string date)
    {
        //string date = "27.04.2020";
        ftext.text = date;
        //ftext.text = "You want to go from " + from.text + " to " + to.text;
        if (from.text.Length != 0 && to.text.Length != 0)
            StartCoroutine(GetTimetable(date));
    }

    public IEnumerator GetTimetable(string date)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        string routepath = "https://pass.rzd.ru/suggester?lang=ru&compactMode=y&stationNamePart=";

        //get code from
        string pathFrom = routepath + from.text.ToUpper();
        UnityEngine.Debug.Log(pathFrom);
        UnityWebRequest www = UnityWebRequest.Get(pathFrom);
        yield return www.SendWebRequest();
        CheckNetwork(www); 
        string code0 = GetCurrentCode(www.downloadHandler.text, from.text.ToUpper());
        UnityEngine.Debug.Log(code0);

        //get code to
        string pathTo = routepath + to.text.ToUpper();
        UnityEngine.Debug.Log(pathTo);
        www = UnityWebRequest.Get(pathTo);
        yield return www.SendWebRequest();
        CheckNetwork(www);
        string code1 = GetCurrentCode(www.downloadHandler.text, to.text.ToUpper());
        UnityEngine.Debug.Log(code1);



        //get RID
        string pathRID = "https://pass.rzd.ru/timetable/public/ru?layer_id=5827&dir=0&tfl=3&checkSeats=1&code0=" + code0 + "&dt0=" + date + "&code1=" + code1;
        UnityEngine.Debug.Log(pathRID);
        UnityWebRequest wwwRID = UnityWebRequest.Post(pathRID, formData);
        yield return wwwRID.SendWebRequest();
        //yield return new WaitForSeconds((float)0.5);
        CheckNetwork(wwwRID);
        string rid = GetCurrentRID(wwwRID.downloadHandler.text);
        UnityEngine.Debug.Log(wwwRID.downloadHandler.text);
        ftext.text = rid;

        //get TimeTable
        string pathTimeTable = "https://pass.rzd.ru/timetable/public/ru?layer_id=5827&rid=" + rid;
        UnityEngine.Debug.Log(pathTimeTable);
        UnityWebRequest timeTable = UnityWebRequest.Post(pathTimeTable, formData);
        yield return timeTable.SendWebRequest();
        //yield return new WaitForSeconds((float)0.5);
        CheckNetwork(timeTable);
        ftext.text = timeTable.downloadHandler.text;
        UnityEngine.Debug.Log(timeTable.downloadHandler.text);

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
