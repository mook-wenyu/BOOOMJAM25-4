using System;

[Serializable]
public class GameTime
{
    public int Day { get; set; } = 0;
    public int Hour { get; set; } = 0;
    public int Minute { get; set; } = 0;

    public event Action<GameTime> OnTimeChanged;

    public GameTime(int day = 0, int hour = 0, int minute = 0)
    {
        SetTime(day, hour, minute);
    }

    public GameTime(string timeString)
    {
        var parts = timeString.Split('/');
        if (parts.Length != 2)
            throw new ArgumentException("时间格式错误,应为: 天/小时:分钟");

        int day = int.Parse(parts[0]);

        var timeParts = parts[1].Split(':');
        if (timeParts.Length != 2)
            throw new ArgumentException("时间格式错误,应为: 天/小时:分钟");

        int hour = int.Parse(timeParts[0]);
        int minute = int.Parse(timeParts[1]);

        SetTime(day, hour, minute);
    }

    /// <summary>
    /// 获取时间字符串
    /// </summary>
    /// <returns>时间字符串</returns>
    public string GetTimeString()
    {
        return $"第{Day}天 {Hour:D2}:{Minute:D2}";
    }

    /// <summary>
    /// 设置时间
    /// </summary>
    /// <param name="day">天数</param>
    /// <param name="hour">小时数</param>
    /// <param name="minute">分钟数</param>
    public void SetTime(int day, int hour, int minute)
    {
        if (day < 0 || hour < 0 || minute < 0)
            throw new System.ArgumentException("时间值不能为负数");
        if (hour >= 24)
            throw new System.ArgumentException("小时必须在0-23之间");
        if (minute >= 60)
            throw new System.ArgumentException("分钟必须在0-59之间");

        this.Day = day;
        this.Hour = hour;
        this.Minute = minute;
    }

    /// <summary>
    /// 增加分钟
    /// </summary>
    /// <param name="minutes">分钟数</param>
    public void AddMinutes(int minutes)
    {
        AddTime(0, 0, minutes);
    }

    /// <summary>
    /// 增加小时
    /// </summary>
    /// <param name="hours">小时数</param>
    public void AddHours(int hours)
    {
        AddTime(0, hours, 0);
    }

    /// <summary>
    /// 增加天数
    /// </summary>
    /// <param name="days">天数</param>
    public void AddDays(int days)
    {
        AddTime(days, 0, 0);
    }

    /// <summary>
    /// 增加时间
    /// </summary>
    /// <param name="day">天数</param>
    /// <param name="hour">小时数</param>
    /// <param name="minute">分钟数</param>
    public void AddTime(int day, int hour, int minute)
    {
        if (day < 0 || hour < 0 || minute < 0)
            throw new System.ArgumentException("时间增量不能为负数");

        this.Minute += minute;
        if (this.Minute >= 60)
        {
            this.Hour += this.Minute / 60;
            this.Minute = this.Minute % 60;
        }

        this.Hour += hour;
        if (this.Hour >= 24)
        {
            this.Day += this.Hour / 24;
            this.Hour = this.Hour % 24;
        }

        this.Day += day;

        OnTimeChanged?.Invoke(this);
    }

    /// <summary>
    /// 获取总分钟数的时间戳
    /// </summary>
    /// <returns>从游戏开始到现在的总分钟数</returns>
    public int GetTimeStamp()
    {
        return Day * 24 * 60 + Hour * 60 + Minute;
    }

    /// <summary>
    /// 计算与另一个时间点的分钟差
    /// </summary>
    /// <param name="other">另一个时间点</param>
    /// <returns>当前时间与另一个时间点的分钟差（正数表示当前时间较晚，负数表示当前时间较早）</returns>
    public int GetTimeDifference(GameTime other)
    {
        return this.GetTimeStamp() - other.GetTimeStamp();
    }

    /// <summary>
    /// 判断当前时间是否为整点
    /// </summary>
    /// <returns>如果当前时间是整点则返回true，否则返回false</returns>
    public bool IsFullHour()
    {
        return Minute == 0;
    }

    /// <summary>
    /// 获取当前是否为指定的整点时间
    /// </summary>
    /// <param name="specificHour">指定的小时数（0-23）</param>
    /// <returns>如果当前时间是指定的整点则返回true，否则返回false</returns>
    public bool IsSpecificFullHour(int specificHour)
    {
        if (specificHour < 0 || specificHour >= 24)
            throw new System.ArgumentException("小时必须在0-23之间");

        return Hour == specificHour && Minute == 0;
    }
}