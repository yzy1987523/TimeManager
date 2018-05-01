//Create in 20180430
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
///这是一个时间管理工具，用途是帮助我管理时间
/// </summary>
public class Main : MonoBehaviour {
    private List<TaskData> taskList = new List<TaskData>();//用来存当天的任务数据
    private Dictionary<SelfDateTime, TaskDataList> totalData;//用来存放即日起，每天的任务数据
    private SelfDateTime curDate;
    private int fpsCount;
    private int refreshRate = 3;
    private int curTaskIndex;

    #region Test
    public int Hour;
    public int Miunte;
    #endregion
    private void Start()
    {
        Init();
        SetNewDayTask();
        //读取当前日期，获取该天的任务列表
        ReadNowTaskData();
    }
    private void Update()
    {        
        TimeKeeper();   
    }
    private void Init()
    {
        totalData = new Dictionary<SelfDateTime, TaskDataList>();//应该是要从存档中读取的
        curDate = GetSelfDateByDateTime(DateTime.Now);
    }
    private void TimeKeeper()
    {
        fpsCount++;
        if (fpsCount >= refreshRate)
        {
            fpsCount = 0;
            CheckTask();
        }        
    }
    private void CheckTask()
    {
        if (curTaskIndex >= taskList.Count)
        {
            //所有任务都完成了
            return;
        }
        //判断下个任务是否到达开始时间
        if (IsExpire(curDate, taskList[curTaskIndex].StartTime))
        {
            //提醒
            Alarm(taskList[curTaskIndex].ImportanceLevel);
        }
    }
    //任务全部完成，或者到期后开始定制第二天的任务
    private void SetNewDayTask()
    {
        //开启新的一天的任务表，设置日期
        var _date = new SelfDateTime(2018,5,1);
        //读取惯例任务表，添加惯例任务，并显示列表
        //此时可以：
        //1.按住并往两侧划：移除
        //2.新建任务条，填写任务内容、时间（时、分）
        //3.按住任务条，上下划修改任务开始时间
        //设置完后就保存为该天的任务列表

        InsertTask(_date,"Eat",new SelfDayTime(Hour,Miunte));
        InsertTask(_date, "Get Up", new SelfDayTime(Hour, Miunte-1));
        var _taskList = new TaskDataList(taskList);
        totalData.Add(_date, _taskList);
    }
    private void SetNormalTask()
    {

    }
    private void LoadNormalTask()
    {

    }
    private void ReadNowTaskData()
    {
        var _key = GetSelfDateByDateTime(DateTime.Now);
        taskList.Clear();
        for (var i = 0; i < totalData[_key].TaskList.Count; i++)
        {
            taskList.Add(totalData[_key].TaskList[i]);
        }       
    }

    private void Alarm(ImportanceLevel _level)
    {
        //显示出新的任务：假如上个任务完成，则只显示开始、跳过；假如上个任务未完成，则显示完成、终止，然后再显示开始和跳过

        //震动提示
        switch (_level)
        {
            case ImportanceLevel.Soft:
                Debug.Log(taskList[curTaskIndex].TaskString);
                break;
        }
    }
    //将当前时间转换为DateTime
    private DateTime GetDateTimeBySelf(SelfDateTime _selfDate, SelfDayTime _selfDayTime)
    {
        return new DateTime(_selfDate.Year, _selfDate.Month, _selfDate.Day,_selfDayTime.Hour,_selfDayTime.Minute,_selfDayTime.Second);
    }
    //将当前DateTime转换为SelfDate
    private SelfDateTime GetSelfDateByDateTime(DateTime _dateTime)
    {
        return new SelfDateTime(_dateTime.Year, _dateTime.Month, _dateTime.Day);
    }
    private void InsertTask(SelfDateTime _selfDate ,string _taskString, SelfDayTime _startTime,  ImportanceLevel _importanceLevel=ImportanceLevel.Soft)
    {
        TaskData taskData = new TaskData(_taskString, _startTime, _importanceLevel);
        taskList.Insert(GetTaskIndex(_selfDate,_startTime),taskData);
    }
    //从开始对比所有任务的开始时间，如果插入的任务早于某个任务，则插入到该任务前
    private int GetTaskIndex(SelfDateTime _selfDate,SelfDayTime _startTime)
    {
        var _curTime = GetDateTimeBySelf(_selfDate,_startTime);        
        for (var i = 0; i < taskList.Count; i++)
        {
            if (_curTime.CompareTo(GetDateTimeBySelf(_selfDate,taskList[i].StartTime))==-1)
            {
                return i;
            }
        }
        return taskList.Count;
    }
    public void SetDay(int _day)
    {
        //curSettingTime.Day = _day;
    }
    //由于等于不好实现，所以用逾期代表到时
    public bool IsExpire(DateTime _target)
    {
        return DateTime.Now.CompareTo(_target)==1;
    }
    private bool IsExpire(SelfDateTime _selfDate,SelfDayTime _selfDay)
    {
        var _curTime = GetDateTimeBySelf(_selfDate, _selfDay);
        return IsExpire(_curTime);
    }
    public void FinishTask(TaskData _taskData)
    {
        //根据list序号判断是哪个任务完成
        //获取当前时间，存储该任务数据：完成时间、是否完成
        //显示选项：继续下个任务、休息
        //还要注意，完成时间是否超过当天
    }  
	
}
public enum ImportanceLevel
{
    None,//不震,属于无任务时的空闲时间
    Soft,//震一下
    Hard,//震动一段时间（3次）
    Severity,//声音和震动、不关不休、且标题醒目
}

[Serializable]
public struct SelfDayTime
{   
    public int Hour;
    public int Minute;
    public int Second;
    public SelfDayTime(int _hour,int _minute)
    {
        Hour = _hour;
        Minute=_minute;
        Second = 0;
    }
}
[Serializable]
public struct SelfDateTime
{
    public int Year;
    public int Month;
    public int Day;
    public SelfDateTime(int _year,int _month,int _day)
    {
        Year = _year;
        Month = _month;
        Day = _day;
    }
}
public struct TaskDataList
{
    public List<TaskData> TaskList;
    public TaskDataList(List<TaskData> _taskList)
    {
        TaskList = new List<TaskData>();
        for (var i = 0; i < _taskList.Count; i++)
        {
            TaskList.Add(_taskList[i]);
        }
    }
}
[Serializable]
public class TaskData
{    
    public string TaskString;
    public SelfDayTime StartTime;//不设结束时间，因为下个任务开始时间就是最后期限
    public SelfDayTime ReallyStartTime;
    public SelfDayTime ReallyEndTime;
    public bool IsFinish;
    public ImportanceLevel ImportanceLevel;
    public TaskData( string _taskString,SelfDayTime _startTime,ImportanceLevel _importanceLevel)
    {       
        TaskString = _taskString;
        StartTime = _startTime;
        ImportanceLevel = _importanceLevel;
    }
}


