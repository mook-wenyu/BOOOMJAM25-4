using System;
using System.Linq;
using Cysharp.Threading.Tasks;

[Serializable]
public class GameTime
{
    public int day = 0;
    public int hour = 0;
    public int minute = 0;

    public event Func<GameTime, UniTask> OnTimeChanged;
    public event Func<GameTime, UniTask> OnHourChanged; // 整点事件

    public GameTime(int day = 0, int hour = 0, int minute = 0)
    {
        SetTime(day, hour, minute);
    }

    public GameTime(string timeString)
    {
        var parts = timeString.Split('/');
        if (parts.Length != 2)
            throw new ArgumentException("时间格式错误,应为: 天/小时:分钟");

        int days = int.Parse(parts[0]);

        var timeParts = parts[1].Split(':');
        if (timeParts.Length != 2)
            throw new ArgumentException("时间格式错误,应为: 天/小时:分钟");

        int hours = int.Parse(timeParts[0]);
        int minutes = int.Parse(timeParts[1]);

        SetTime(days, hours, minutes);
    }

    /// <summary>
    /// 获取时间字符串
    /// </summary>
    /// <returns>时间字符串</returns>
    public string GetTimeString()
    {
        return $"第{day}天 {hour:D2}:{minute:D2}";
    }

    /// <summary>
    /// 设置时间
    /// </summary>
    /// <param name="days">天数</param>
    /// <param name="hours">小时数</param>
    /// <param name="minutes">分钟数</param>
    public void SetTime(int days, int hours, int minutes)
    {
        if (days < 0 || hours < 0 || minutes < 0)
            throw new System.ArgumentException("时间值不能为负数");
        if (hours >= 24)
            throw new System.ArgumentException("小时必须在0-23之间");
        if (minutes >= 60)
            throw new System.ArgumentException("分钟必须在0-59之间");

        this.day = days;
        this.hour = hours;
        this.minute = minutes;
    }

    /// <summary>
    /// 增加1分钟
    /// </summary>
    public async UniTask AddMinutes()
    {
        await AddTime(1);
    }

    /// <summary>
    /// 增加小时，通过逐分钟增加实现
    /// </summary>
    /// <param name="hours">要增加的小时数</param>
    public async UniTask AddHours(int hours)
    {
        if (hours < 0)
            throw new System.ArgumentException("时间增量不能为负数");

        for (int i = 0; i < hours * 60; i++)
        {
            await AddMinutes();
        }
    }

    /// <summary>
    /// 增加天数，通过逐分钟增加实现
    /// </summary>
    /// <param name="days">要增加的天数</param>
    public async UniTask AddDays(int days)
    {
        if (days < 0)
            throw new System.ArgumentException("时间增量不能为负数");

        for (int i = 0; i < days * 24 * 60; i++)
        {
            await AddMinutes();
        }
    }

    /// <summary>
    /// 增加时间（只能增加分钟）
    /// </summary>
    /// <param name="minutes">要增加的分钟数</param>
    private async UniTask AddTime(int minutes = 1)
    {
        if (minutes < 0)
            throw new System.ArgumentException("时间增量不能为负数");

        int oldHour = this.hour; // 记录原小时值

        this.minute += minutes;
        if (this.minute >= 60)
        {
            this.hour += this.minute / 60;
            this.minute = this.minute % 60;
        }

        if (this.hour >= 24)
        {
            this.day += this.hour / 24;
            this.hour = this.hour % 24;
        }

        // 使用 UniTask.WhenAll 等待所有订阅者完成
        if (OnTimeChanged != null)
        {
            await UniTask.WhenAll(OnTimeChanged.GetInvocationList()
                .Cast<Func<GameTime, UniTask>>()
                .Select(handler => handler(this)));
        }

        // 如果小时发生变化且当前是整点，触发整点事件
        if (oldHour != this.hour && this.minute == 0 && OnHourChanged != null)
        {
            await UniTask.WhenAll(OnHourChanged.GetInvocationList()
                .Cast<Func<GameTime, UniTask>>()
                .Select(handler => handler(this)));
        }

        await UniTask.Yield();
    }

    /// <summary>
    /// 获取总分钟数的时间戳
    /// </summary>
    /// <returns>从游戏开始到现在的总分钟数</returns>
    public int GetTimeStamp()
    {
        return day * 24 * 60 + hour * 60 + minute;
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
        return minute == 0;
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

        return hour == specificHour && minute == 0;
    }
}
