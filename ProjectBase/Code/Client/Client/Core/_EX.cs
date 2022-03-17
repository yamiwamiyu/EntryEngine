using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>前端扩展方法</summary>
public static class _EX
{
    /// <summary>金币显示样式，后面加千和万</summary>
    /// <param name="coin">金币数</param>
    public static string 金币显示(this int coin)
    {
        if (coin < 10000)
            return coin.ToString();
        // 5.5万
        else if (coin < 100000)
            return (coin / 1000 * 1000 / 10000.0) + "万";
        // 10万 100万 1000万
        else if (coin < 100000000)
            return (coin / 10000) + "万";
        // 1亿 10亿
        else
            return (coin / 100000000) + "亿";
    }
    /// <summary>得分样式，+100 / -100</summary>
    public static string 得分显示(this int score)
    {
        if (score <= 0)
            return score.ToString();
        else
            return "+" + score;
    }
}
